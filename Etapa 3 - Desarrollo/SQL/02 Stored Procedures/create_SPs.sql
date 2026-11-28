-- CREACION DE TODOS LOS STORED PROCEDURES DE INSTIRED
-- Requiere haber creado las tablas de 01 Tablas DB.
USE InstiRed_DEV;

-- =====================================================================
-- REGISTRO DE RESPONSABLES Y ALUMNOS
-- =====================================================================

DELIMITER $$
-- ------------------------------------------------------------
-- 1) sp_Registrar_Responsable
-- Crea Usuario (tipo 1 = Responsable) + Persona (o reutiliza la
-- existente si el DNI ya esta cargado) + Responsable + Telefono.
-- El responsable tiene que ser mayor de edad. Si la Persona ya existe
-- se usa la fecha de nacimiento guardada, no la que llega por parametro.
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Registrar_Responsable$$
CREATE PROCEDURE sp_Registrar_Responsable(
    IN p_clave           VARCHAR(255),
    IN p_email           VARCHAR(100),
    IN p_nombre          VARCHAR(50),
    IN p_apellido        VARCHAR(50),
    IN p_documento       VARCHAR(30),	  -- se lo deja mas grande para poder captarlo incluso si el usuario lo manda mal. Si no, da un error no catcheable.
    IN p_domicilio       VARCHAR(100),
    IN p_fechaNacimiento DATE,
    IN p_telefono        VARCHAR(30)      -- puede ser NULL.
)
BEGIN
    DECLARE v_idPersona       INT DEFAULT NULL;
    DECLARE v_fechaNacimiento DATE DEFAULT NULL;
    DECLARE v_idUsuario       INT;
    DECLARE v_idResponsable   INT;

    -- Si algo falla, hace un rollback y devuelve el error (resignal)
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    -- Limpieza
    SET p_clave     = TRIM(p_clave);
    SET p_email     = TRIM(p_email);
    SET p_nombre    = TRIM(p_nombre);
    SET p_apellido  = TRIM(p_apellido);
    SET p_documento = TRIM(p_documento);
    SET p_domicilio = TRIM(p_domicilio);
    SET p_telefono  = TRIM(p_telefono);

    -- Validaciones
    IF p_clave IS NULL OR p_clave = '' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La clave es obligatoria.';
    END IF;

    IF p_email IS NULL OR p_email = '' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email es obligatorio.';
    END IF;

    IF p_documento IS NULL OR p_documento = '' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento es obligatorio.';
    END IF;

    IF CHAR_LENGTH(p_documento) > 8 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento no puede tener mas de 8 caracteres.';
    END IF;

    IF LEFT(p_documento, 1) = '0' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento no puede empezar con 0.';
    END IF;

    IF EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email ya esta registrado.';
    END IF;

    -- Si el DNI ya existe se reutiliza esa Persona (con su fecha de nacimiento guardada).
    SELECT id, fechaNacimiento INTO v_idPersona, v_fechaNacimiento FROM Personas WHERE documento = p_documento;

    IF v_idPersona IS NULL THEN
        SET v_fechaNacimiento = p_fechaNacimiento;

        IF p_nombre IS NULL OR p_nombre = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nombre es obligatorio.';
        END IF;

        IF p_apellido IS NULL OR p_apellido = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El apellido es obligatorio.';
        END IF;

        IF p_domicilio IS NULL OR p_domicilio = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El domicilio es obligatorio.';
        END IF;
    END IF;

    IF v_fechaNacimiento IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de nacimiento es obligatoria.';
    END IF;

    IF v_fechaNacimiento > CURDATE() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de nacimiento no puede ser futura.';
    END IF;

    IF TIMESTAMPDIFF(YEAR, v_fechaNacimiento, CURDATE()) < 18 THEN
        -- Si el DNI ya estaba cargado, no se da mas detalle.
        IF v_idPersona IS NOT NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ese documento ya fue registrado.';
        END IF;

        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable debe ser mayor de edad.';
    END IF;

    IF v_idPersona IS NOT NULL
       AND EXISTS (SELECT 1 FROM Responsables WHERE idPersona = v_idPersona) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe un responsable registrado con ese documento.';
    END IF;

    START TRANSACTION;

    IF v_idPersona IS NULL THEN -- Si el DNI no fue encontrado antes, se inserta esa Persona. Si no, se reutiliza mas adelante.
        INSERT INTO Personas (nombre, apellido, documento, domicilio, fechaNacimiento)
        VALUES (p_nombre, p_apellido, p_documento, p_domicilio, p_fechaNacimiento);
        SET v_idPersona = LAST_INSERT_ID();
    END IF;

    INSERT INTO Usuarios (clave, email, activo, idTipoUsuario)
    VALUES (p_clave, p_email, TRUE, 1);
    SET v_idUsuario = LAST_INSERT_ID();

    INSERT INTO Responsables (idPersona, idUsuario)
    VALUES (v_idPersona, v_idUsuario);
    SET v_idResponsable = LAST_INSERT_ID();

    IF p_telefono IS NOT NULL AND p_telefono <> '' THEN
        INSERT INTO Telefonos (idResponsable, telefono)
        VALUES (v_idResponsable, p_telefono);
    END IF;

    COMMIT;

    SELECT v_idResponsable AS idResponsable,
           v_idUsuario     AS idUsuario,
           v_idPersona     AS idPersona;
END$$


-- ------------------------------------------------------------
-- 2) sp_Registrar_Alumno
-- Crea Persona (o reutiliza por DNI) + Alumno (sin cursos: se vinculan
-- por sus postulaciones confirmadas) + vinculo en ResponsablesPorAlumno
-- con su rol.
-- Solo se reutiliza una Persona existente si es el propio responsable
-- (autogestor: un adulto que se carga a si mismo). En ese caso NO se crea el
-- vinculo en ResponsablesPorAlumno, p_idRol se ignora y se exige ser mayor de
-- edad. Cualquier otro DNI ya registrado se rechaza. Hay un solo responsable
-- por alumno.
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Registrar_Alumno$$
CREATE PROCEDURE sp_Registrar_Alumno(
    IN p_idResponsable   INT,
    IN p_idRol           TINYINT,         -- 1 Padre, 2 Madre, 3 Tutor legal, 4 Familiar a cargo. Se ignora si es autogestor (puede ser NULL)
    IN p_nombre          VARCHAR(50),
    IN p_apellido        VARCHAR(50),
	IN p_documento       VARCHAR(30),	  -- se lo deja mas grande para poder captarlo incluso si el usuario lo manda mal. Si no, da un error no catcheable.
    IN p_domicilio       VARCHAR(100),
    IN p_fechaNacimiento DATE
)
BEGIN
    DECLARE v_idPersona       INT DEFAULT NULL;
    DECLARE v_idPersonaResp   INT DEFAULT NULL;
    DECLARE v_fechaNacimiento DATE DEFAULT NULL;
    DECLARE v_esAutogestor    BOOLEAN DEFAULT FALSE;
    DECLARE v_idAlumno        INT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    -- Limpieza
    SET p_nombre    = TRIM(p_nombre);
    SET p_apellido  = TRIM(p_apellido);
    SET p_documento = TRIM(p_documento);
    SET p_domicilio = TRIM(p_domicilio);

    -- Validaciones
    SELECT idPersona INTO v_idPersonaResp FROM Responsables WHERE id = p_idResponsable;

    IF v_idPersonaResp IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable no existe.';
    END IF;

    IF p_documento IS NULL OR p_documento = '' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento es obligatorio.';
    END IF;

    IF CHAR_LENGTH(p_documento) > 8 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento no puede tener mas de 8 caracteres.';
    END IF;

    IF LEFT(p_documento, 1) = '0' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento no puede empezar con 0.';
    END IF;

    -- Se busca por DNI (con su fecha de nacimiento guardada, por si es el propio responsable).
    SELECT id, fechaNacimiento INTO v_idPersona, v_fechaNacimiento FROM Personas WHERE documento = p_documento;

    IF v_idPersona IS NULL THEN
        SET v_fechaNacimiento = p_fechaNacimiento;

        IF p_nombre IS NULL OR p_nombre = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nombre es obligatorio.';
        END IF;

        IF p_apellido IS NULL OR p_apellido = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El apellido es obligatorio.';
        END IF;

        IF p_domicilio IS NULL OR p_domicilio = '' THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El domicilio es obligatorio.';
        END IF;
    END IF;

    IF v_fechaNacimiento IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de nacimiento es obligatoria.';
    END IF;

    IF v_fechaNacimiento > CURDATE() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La fecha de nacimiento no puede ser futura.';
    END IF;

    -- Autogestor: el alumno es la misma Persona que el responsable.
    SET v_esAutogestor = (v_idPersona IS NOT NULL AND v_idPersona = v_idPersonaResp);

    IF v_esAutogestor THEN
        IF TIMESTAMPDIFF(YEAR, v_fechaNacimiento, CURDATE()) < 18 THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Para cargarse a si mismo como alumno hay que ser mayor de edad.';
        END IF;

        IF EXISTS (SELECT 1 FROM Alumnos WHERE idPersona = v_idPersona) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe un alumno registrado con ese documento.';
        END IF;
    ELSE
        IF NOT EXISTS (SELECT 1 FROM Roles WHERE id = p_idRol) THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El rol del responsable no es valido.';
        END IF;

        -- Mismo mensaje si el DNI es de un alumno o de cualquier otra persona, para no revelar de quien es.
        IF v_idPersona IS NOT NULL THEN
            SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ese documento ya esta registrado.';
        END IF;
    END IF;

    START TRANSACTION;

    IF v_idPersona IS NULL THEN
        INSERT INTO Personas (nombre, apellido, documento, domicilio, fechaNacimiento)
        VALUES (p_nombre, p_apellido, p_documento, p_domicilio, p_fechaNacimiento);
        SET v_idPersona = LAST_INSERT_ID();
    END IF;

    INSERT INTO Alumnos (idPersona)
    VALUES (v_idPersona);
    SET v_idAlumno = LAST_INSERT_ID();

    IF NOT v_esAutogestor THEN
        INSERT INTO ResponsablesPorAlumno (idResponsable, idAlumno, idRol)
        VALUES (p_idResponsable, v_idAlumno, p_idRol);
    END IF;

    COMMIT;

    SELECT v_idAlumno  AS idAlumno,
           v_idPersona AS idPersona;
END$$

DELIMITER ;

-- =====================================================================
-- ADMINISTRADORES, INSTITUCIONES Y DATOS DE CONTACTO
-- =====================================================================

-- ---------------------------------------------------------------------
-- sp_AdministradorInstitucion_Registrar
-- Un administrador envia una postulacion para registrar su institucion.
-- Segun el documento, el SP sigue uno de estos caminos:
--   * ALTA INICIAL: no tiene cuenta de administrador. Crea persona (si no existe),
--     usuario inactivo, administrador, telefono y una postulacion Pendiente.
--   * REINTENTO: ya tiene cuenta y su ultima postulacion fue Rechazada. Reutiliza
--     la cuenta, actualiza sus datos personales y telefono, y crea otra postulacion Pendiente.
--   * ERROR: ultima postulacion Pendiente, o cuenta ya aprobada / con institucion.
-- En el reintento el email tiene que ser el mismo y p_clave se ignora (la API ya
-- verifico la contraseña con sp_AdministradorInstitucion_ObtenerClaveReintento).
-- Devuelve el id de la postulacion nueva en p_idPostulacion.
-- ---------------------------------------------------------------------

DROP PROCEDURE IF EXISTS sp_AdministradorInstitucion_Registrar;

DELIMITER $$

CREATE PROCEDURE sp_AdministradorInstitucion_Registrar(
  -- Datos personales
  IN  p_nombre               VARCHAR(50),
  IN  p_apellido             VARCHAR(50),
  IN  p_documento            VARCHAR(8),
  IN  p_domicilio            VARCHAR(100),
  IN  p_fechaNacimiento      DATE,
  -- Cuenta
  IN  p_clave                VARCHAR(255),   -- ya llega hasheada desde la API (el SP la guarda tal cual)
  IN  p_email                VARCHAR(100),
  IN  p_telefono             VARCHAR(30),
  -- Institución que declara (lo que verifica Dev&Co)
  IN  p_nombreInstitucion    VARCHAR(100),
  IN  p_cuitInstitucion      VARCHAR(11),
  IN  p_rutaConstanciaFiscal VARCHAR(255),
  IN  p_rutaHabilitacion     VARCHAR(255),
  IN  p_rutaConstanciaLaboral VARCHAR(255),
  -- Seguimiento (lo genera la API, el SP solo lo guarda)
  IN  p_codigoSeguimiento    VARCHAR(30),
  -- Salida
  OUT p_idPostulacion        INT
)
BEGIN
  DECLARE v_idTipoAdministrador TINYINT;
  DECLARE v_idEstadoPendiente   INT;
  DECLARE v_idEstadoRechazado   INT;
  DECLARE v_idPersona           INT;
  DECLARE v_idUsuario           INT;
  DECLARE v_idAdministrador     INT;
  DECLARE v_fechaNac            DATE;
  DECLARE v_fecha               DATE;
  -- Variables para reintento de postulaciones rechazadas
  DECLARE v_idUltimoEstado      INT;
  DECLARE v_idInstitucionAdmin  INT;
  DECLARE v_emailActual         VARCHAR(100);
  DECLARE v_usuarioActivo       BOOL;
  DECLARE v_idTelefono          INT;

  -- Casos que se colen entre la validación y el INSERT
  DECLARE EXIT HANDLER FOR 1062 -- error 1062 se da cuando se viola una clave UNIQUE, cuando ocurra ese error se ejecuta el bloque y termina.
  BEGIN
    ROLLBACK;
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Ya existe un registro con esos datos (documento, email o código de seguimiento).';
  END;

  -- Cualquier otro error (incluidos los SIGNAL de validación): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  -- Normalizar (la clave no se toca, puede contener espacios)
  SET p_nombre            = TRIM(p_nombre);
  SET p_apellido          = TRIM(p_apellido);
  SET p_documento         = TRIM(p_documento);
  SET p_domicilio         = TRIM(p_domicilio);
  SET p_email             = LOWER(TRIM(p_email));
  SET p_telefono          = TRIM(p_telefono);
  SET p_nombreInstitucion = TRIM(p_nombreInstitucion);
  SET p_cuitInstitucion   = TRIM(p_cuitInstitucion);

  -- Validaciones que no necesitan la base
  IF COALESCE(p_nombre,'') = '' OR COALESCE(p_apellido,'') = '' OR COALESCE(p_documento,'') = ''
     OR COALESCE(p_domicilio,'') = '' OR p_fechaNacimiento IS NULL
     OR COALESCE(p_clave,'') = '' OR COALESCE(p_email,'') = ''
     OR COALESCE(p_telefono,'') = '' OR COALESCE(p_nombreInstitucion,'') = ''
     OR COALESCE(p_cuitInstitucion,'') = ''
     OR COALESCE(p_rutaConstanciaFiscal,'') = '' OR COALESCE(p_rutaHabilitacion,'') = ''
     OR COALESCE(p_rutaConstanciaLaboral,'') = ''
     OR COALESCE(p_codigoSeguimiento,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_documento NOT REGEXP '^[0-9]{7,8}$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento debe tener 7 u 8 dígitos.';
  END IF;

  IF p_cuitInstitucion NOT REGEXP '^[0-9]{11}$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El CUIT debe tener 11 dígitos, sin guiones.';
  END IF;

  IF p_email NOT REGEXP '^[^@ ]+@[^@ ]+\\.[^@ ]+$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email no tiene un formato válido.';
  END IF;


  START TRANSACTION;

	-- Catálogos: se buscan por descripción (no por id), igual que en el archivo de INSERTs
  SET v_idTipoAdministrador = (SELECT id FROM TipoUsuario        WHERE descripcion = 'Administrador');
  SET v_idEstadoPendiente   = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente');
  SET v_idEstadoRechazado   = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Rechazado');

	-- Validacion DB Cargada con tipos de administrador y estados.
  IF v_idTipoAdministrador IS NULL OR v_idEstadoPendiente IS NULL OR v_idEstadoRechazado IS NULL THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Catálogos incompletos: falta Administrador, Pendiente o Rechazado.';
  END IF;

  -- Validaciones contra la DB

  -- La persona puede existir por otro rol (por ejemplo, ya es responsable): se reutiliza.
  -- En ese caso se conservan los datos personales ya guardados (no se actualizan).
  -- Solo se actualizan en el reintento de una postulación rechazada.
  -- Para tener cuenta de administrador necesita su propia cuenta en Usuarios, con un email distinto al del otro rol.
  SET v_idPersona = (SELECT id FROM Personas WHERE documento = p_documento);

  IF v_idPersona IS NOT NULL THEN
    SET v_idAdministrador = (SELECT id FROM AdministradoresInstitucion WHERE idPersona = v_idPersona);
  END IF;

	-- El SP sigue dos caminos según el DNI:
	-- ALTA INICIAL: esa persona todavía no tiene cuenta de administrador.
	-- REINTENTO:    ya la tiene, y solo se permite si su última postulación fue rechazada.
  IF v_idAdministrador IS NULL THEN
    -- ALTA INICIAL: el email no puede estar en uso
    IF EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ese email ya está registrado.';
    END IF;

	-- Si la persona ya existe (es responsable o alumno autogestor y ahora se carga como administrador)
    -- se ignoran los datos personales nuevos (nombre, apellido, domicilio, fecha) y se usan los ya guardados.
    -- Solo se asigna v_fechaNac porque hace falta para validar la mayoría de edad:
    -- si la persona existe se usa la fecha guardada, si no, la recibida por parámetro.
    IF v_idPersona IS NOT NULL THEN
      SET v_fechaNac = (SELECT fechaNacimiento FROM Personas WHERE id = v_idPersona);
    ELSE
      SET v_fechaNac = p_fechaNacimiento;
    END IF;
  ELSE -- Si idAdmin NO ES NULL (reintento) Seteamos las variables que necesitamos:
    -- YA ES ADMINISTRADOR: solo se permite reintentar si su ultima postulacion fue rechazada.
    -- (La contraseña la verifica la API antes de llamar a este SP)
    -- Se bloquea la fila del administrador hasta el COMMIT: si llega otro reintento al mismo tiempo (doble click), espera acá.
    SELECT id INTO v_idAdministrador FROM AdministradoresInstitucion
    WHERE id = v_idAdministrador FOR UPDATE;

    -- La última postulación se lee con FOR UPDATE para ver lo que el otro reintento ya confirmó.
    SELECT idEstado INTO v_idUltimoEstado FROM PostulacionesAdminInstitucion
    WHERE idAdministradorInstitucion = v_idAdministrador
    ORDER BY id DESC LIMIT 1 FOR UPDATE;
    SET v_idUsuario = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = v_idAdministrador);
    SET v_idInstitucionAdmin = (SELECT idInstitucion FROM AdministradoresInstitucion WHERE id = v_idAdministrador);
    SET v_emailActual = (SELECT email FROM Usuarios WHERE id = v_idUsuario);
    SET v_usuarioActivo = (SELECT activo FROM Usuarios WHERE id = v_idUsuario);

    IF v_idUltimoEstado = v_idEstadoPendiente THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Ya tenés una postulación pendiente.';
    END IF;

    -- Solo se puede reintentar si la última fue Rechazada, la cuenta sigue inactiva y no tiene institución.
    IF v_idUltimoEstado IS NULL OR v_idUltimoEstado <> v_idEstadoRechazado
       OR v_idInstitucionAdmin IS NOT NULL OR v_usuarioActivo = TRUE THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Ya existe una cuenta de administrador con ese documento.';
    END IF;

    IF v_emailActual <> p_email THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Usá el email con el que te postulaste anteriormente.';
    END IF;

    -- En el reintento los datos personales se actualizan con lo nuevo
    SET v_fechaNac = p_fechaNacimiento;
  END IF;
  -- FIN DEL IF DE REINTENTO (idAdmin con valor)

  IF v_fechaNac > CURDATE() OR TIMESTAMPDIFF(YEAR, v_fechaNac, CURDATE()) < 18 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador debe ser mayor de 18 años.';
  END IF;

  IF EXISTS (SELECT 1 FROM Instituciones WHERE cuit = p_cuitInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una institución registrada con ese CUIT.';
  END IF;

  -- Si hay otra postulacion para esa institucion (cuit) con estado aprobado o pendiente lanzamos un error (solo se permite
  -- volver a enviar una postulacion en caso de tener una rechazada).
  IF EXISTS (SELECT 1 FROM PostulacionesAdminInstitucion
             WHERE cuitInstitucion = p_cuitInstitucion
               AND idEstado <> v_idEstadoRechazado) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una solicitud en curso o aprobada con ese CUIT.';
  END IF;

  -- Escrituras
  SET v_fecha = CURDATE();

  IF v_idAdministrador IS NULL THEN
    -- Alta inicial
    IF v_idPersona IS NULL THEN
      INSERT INTO Personas (nombre, apellido, documento, domicilio, fechaNacimiento)
      VALUES (p_nombre, p_apellido, p_documento, p_domicilio, p_fechaNacimiento);
      SET v_idPersona = LAST_INSERT_ID();
    END IF;

    INSERT INTO Usuarios (clave, email, activo, idTipoUsuario)
    VALUES (p_clave, p_email, FALSE, v_idTipoAdministrador); -- se crea con estado dado de inactivo
    SET v_idUsuario = LAST_INSERT_ID();

    INSERT INTO AdministradoresInstitucion (idPersona, idUsuario, idInstitucion)
    VALUES (v_idPersona, v_idUsuario, NULL); -- se crea sin institucion ya que todavía no está creada.
    SET v_idAdministrador = LAST_INSERT_ID();

    INSERT INTO Telefonos (idAdministrador, telefono)
    VALUES (v_idAdministrador, p_telefono);
  ELSE
    -- Reintento: se reutilizan persona, usuario y administrador (la clave no se toca);
    -- solo se actualizan los datos personales y el telefono, y mas abajo se crea la nueva postulacion.
    UPDATE Personas
    SET nombre = p_nombre, apellido = p_apellido, domicilio = p_domicilio, fechaNacimiento = p_fechaNacimiento
    WHERE id = v_idPersona;

	-- Toma el primer teléfono del administrador para actualizarlo (si no tiene ninguno, se crea).
    -- Solo cuenta un teléfono exclusivo del administrador: si los que tiene son compartidos con una institución
    -- o un responsable, no se les cambia el número (sería cambiárselo a los otros dueños): se le saca el
    -- administrador a esa fila (queda para los otros dueños) y se crea un teléfono nuevo, exclusivo del administrador.
    SET v_idTelefono = (SELECT id FROM Telefonos
                        WHERE idAdministrador = v_idAdministrador AND idInstitucion IS NULL AND idResponsable IS NULL
                        ORDER BY id LIMIT 1);

    IF v_idTelefono IS NULL THEN
      -- Si no tenía ningún teléfono, este UPDATE no modifica nada. Si solo tenía compartidos, se le sacan.
      UPDATE Telefonos SET idAdministrador = NULL WHERE idAdministrador = v_idAdministrador;

      INSERT INTO Telefonos (idAdministrador, telefono)
      VALUES (v_idAdministrador, p_telefono);
    ELSE
      UPDATE Telefonos SET telefono = p_telefono WHERE id = v_idTelefono;
    END IF;
  END IF;

	-- En los dos caminos se crea una postulación nueva Pendiente (el historial de rechazos se conserva).
  INSERT INTO PostulacionesAdminInstitucion
    (fecha, codigoSeguimiento, idAdministradorInstitucion, idEstado,
     nombreInstitucion, cuitInstitucion, rutaConstanciaFiscal, rutaHabilitacion, rutaConstanciaLaboral, motivoRechazo)
  VALUES
    (v_fecha, p_codigoSeguimiento, v_idAdministrador, v_idEstadoPendiente,
     p_nombreInstitucion, p_cuitInstitucion, p_rutaConstanciaFiscal, p_rutaHabilitacion, p_rutaConstanciaLaboral, NULL);
  SET p_idPostulacion = LAST_INSERT_ID();

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_PostulacionAdmin_Aprobar
-- Dev&Co aprueba la postulacion de un administrador de institucion:
--   * la postulacion pasa de Pendiente a Confirmado
--   * el usuario del administrador pasa a activo (ya puede iniciar sesion)
--  mail de aviso via C# (por fuera del SP)
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_PostulacionAdmin_Aprobar;

DELIMITER $$

CREATE PROCEDURE sp_PostulacionAdmin_Aprobar(
  IN p_idPostulacion  INT,
  IN p_idUsuarioDevCo INT     -- usuario Dev&Co que resuelve (lo toma C# de la sesion)
)
BEGIN
  DECLARE v_idTipoDevCo         TINYINT;
  DECLARE v_idEstadoPendiente   INT;
  DECLARE v_idEstadoConfirmado  INT;
  DECLARE v_idAdministrador     INT;

  -- Cualquier error (incluidos los SIGNAL de validacion): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  IF p_idPostulacion IS NULL OR p_idUsuarioDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  START TRANSACTION;

  -- Catalogos: se buscan por descripcion, igual que en el archivo de inserts
  SET v_idTipoDevCo        = (SELECT id FROM TipoUsuario        WHERE descripcion = 'DevCo');
  SET v_idEstadoPendiente  = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente');
  SET v_idEstadoConfirmado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Confirmado');

  IF v_idTipoDevCo IS NULL OR v_idEstadoPendiente IS NULL OR v_idEstadoConfirmado IS NULL THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Solo un usuario Dev&Co activo puede resolver postulaciones
  IF NOT EXISTS (SELECT 1 FROM Usuarios
                 WHERE id = p_idUsuarioDevCo
                   AND idTipoUsuario = v_idTipoDevCo
                   AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para resolver postulaciones.';
  END IF;

  SET v_idAdministrador = (SELECT idAdministradorInstitucion
                           FROM PostulacionesAdminInstitucion
                           WHERE id = p_idPostulacion);

  IF v_idAdministrador IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no existe.';
  END IF;

  -- de Pendiente a Confirmado en un solo UPDATE. Si la postulacion ya fue resuelta
  -- (aunque sea un instante antes por otra persona) no se modifica ninguna fila.
  UPDATE PostulacionesAdminInstitucion
  SET idEstado = v_idEstadoConfirmado
  WHERE id = p_idPostulacion
    AND idEstado = v_idEstadoPendiente;

  IF ROW_COUNT() = 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación ya fue resuelta.';
  END IF;

  -- Se activa el usuario del administrador
  UPDATE Usuarios
  SET activo = TRUE
  WHERE id = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = v_idAdministrador);

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_PostulacionAdmin_Rechazar
-- Dev&Co rechaza la postulacion de un administrador de institucion:
--   * la postulacion pasa de Pendiente a Rechazado y guarda el motivo
--   * el usuario sigue inactivo. Para volver a intentar, el administrador
--     envia una postulacion nueva (el historial se conserva).
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_PostulacionAdmin_Rechazar;

DELIMITER $$

CREATE PROCEDURE sp_PostulacionAdmin_Rechazar(
  IN p_idPostulacion  INT,
  IN p_idUsuarioDevCo INT,
  IN p_motivoRechazo  VARCHAR(255)
)
BEGIN
  DECLARE v_idTipoDevCo         TINYINT;
  DECLARE v_idEstadoPendiente   INT;
  DECLARE v_idEstadoRechazado   INT;
  DECLARE v_idAdministrador     INT;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  SET p_motivoRechazo = TRIM(p_motivoRechazo);

  IF p_idPostulacion IS NULL OR p_idUsuarioDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF COALESCE(p_motivoRechazo, '') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El motivo de rechazo es obligatorio.';
  END IF;

  START TRANSACTION;

  SET v_idTipoDevCo       = (SELECT id FROM TipoUsuario        WHERE descripcion = 'DevCo');
  SET v_idEstadoPendiente = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente');
  SET v_idEstadoRechazado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Rechazado');

  IF v_idTipoDevCo IS NULL OR v_idEstadoPendiente IS NULL OR v_idEstadoRechazado IS NULL THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Usuarios
                 WHERE id = p_idUsuarioDevCo
                   AND idTipoUsuario = v_idTipoDevCo
                   AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para resolver postulaciones.';
  END IF;

  SET v_idAdministrador = (SELECT idAdministradorInstitucion
                           FROM PostulacionesAdminInstitucion
                           WHERE id = p_idPostulacion);

  IF v_idAdministrador IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no existe.';
  END IF;

  UPDATE PostulacionesAdminInstitucion
  SET idEstado = v_idEstadoRechazado,
      motivoRechazo = p_motivoRechazo
  WHERE id = p_idPostulacion
    AND idEstado = v_idEstadoPendiente;

  IF ROW_COUNT() = 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación ya fue resuelta.';
  END IF;

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Institucion_Crear
-- El administrador (ya aprobado por Dev&Co) da de alta su institucion.
--   * nombre y CUIT NO se reciben se toman de su postulacion aprobada,
--   * crea la institucion (nace activa), su fila de Discapacidades y
--     vincula al administrador con la institucion nueva
--   * telefonos, idiomas e imagenes se cargan despues con los SP _Agregar
-- Devuelve el id de la institucion nueva en p_idInstitucion.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Institucion_Crear;

DELIMITER $$

CREATE PROCEDURE sp_Institucion_Crear(
  IN  p_idAdministrador   INT,            -- administrador logueado
  IN  p_direccion         VARCHAR(100),
  IN  p_email             VARCHAR(100),   -- email de contacto de la institucion
  IN  p_latitud           DOUBLE,
  IN  p_longitud          DOUBLE,
  IN  p_descripcion       VARCHAR(3000),
  IN  p_idTipoInstitucion TINYINT,
  -- Discapacidades
  IN  p_movilidad         BOOL,
  IN  p_visual            BOOL,
  IN  p_auditiva          BOOL,
  IN  p_verbal            BOOL,
  IN  p_intelectual       BOOL,
  IN  p_sindromeDown      BOOL,
  IN  p_autismo           BOOL,
  -- Salida
  OUT p_idInstitucion     INT
)
BEGIN
  DECLARE v_idEstadoConfirmado INT;
  DECLARE v_idUsuario          INT;
  DECLARE v_idInstitucionActual INT;
  DECLARE v_nombre             VARCHAR(100);
  DECLARE v_cuit               VARCHAR(11);
  DECLARE v_idInstitucion      INT;

  -- Si otra persona inserta el mismo CUIT justo entre la validación y nuestro INSERT, MySQL tira error 1062: lo convertimos en un mensaje claro.
  DECLARE EXIT HANDLER FOR 1062
  BEGIN
    ROLLBACK;
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Ya existe una institución registrada con ese CUIT.';
  END;

  -- Cualquier otro error (incluidos los SIGNAL de validacion): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  -- Normalizar
  SET p_direccion   = TRIM(p_direccion);
  SET p_email       = LOWER(TRIM(p_email));
  SET p_descripcion = TRIM(p_descripcion);

  -- Validaciones que no necesitan la base
  IF p_idAdministrador IS NULL OR COALESCE(p_direccion,'') = '' OR COALESCE(p_email,'') = ''
     OR p_latitud IS NULL OR p_longitud IS NULL OR COALESCE(p_descripcion,'') = ''
     OR p_idTipoInstitucion IS NULL
     OR p_movilidad IS NULL OR p_visual IS NULL OR p_auditiva IS NULL OR p_verbal IS NULL
     OR p_intelectual IS NULL OR p_sindromeDown IS NULL OR p_autismo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_email NOT REGEXP '^[^@ ]+@[^@ ]+\\.[^@ ]+$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email no tiene un formato válido.';
  END IF;

	-- Solo coordenadas validas (-90 a 90 del Ecuador  y -180 a 180 del Meridiano)
  IF p_latitud NOT BETWEEN -90 AND 90 OR p_longitud NOT BETWEEN -180 AND 180 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Las coordenadas no son válidas.';
  END IF;

  START TRANSACTION;

  -- Catalogo: se busca por descripcion, igual que en el resto de los SP
  SET v_idEstadoConfirmado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Confirmado');

  IF v_idEstadoConfirmado IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos: falta estado de postulacion Confirmado.';
  END IF;

  -- Validaciones contra la DB
  IF NOT EXISTS (SELECT 1 FROM TipoInstitucion WHERE id = p_idTipoInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El tipo de institución no existe.';
  END IF;

  SET v_idUsuario = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = p_idAdministrador);

  IF v_idUsuario IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE id = v_idUsuario AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta del administrador no está habilitada.';
  END IF;

  SET v_idInstitucionActual = (SELECT idInstitucion FROM AdministradoresInstitucion WHERE id = p_idAdministrador);

  IF v_idInstitucionActual IS NOT NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Este administrador ya tiene una institución registrada.';
  END IF;

  -- Nombre y CUIT salen de su postulacion aprobada (la mas reciente, por si tuvo rechazos antes)
  SET v_nombre = (SELECT nombreInstitucion FROM PostulacionesAdminInstitucion
                  WHERE idAdministradorInstitucion = p_idAdministrador AND idEstado = v_idEstadoConfirmado
                  ORDER BY id DESC LIMIT 1);
  SET v_cuit   = (SELECT cuitInstitucion FROM PostulacionesAdminInstitucion
                  WHERE idAdministradorInstitucion = p_idAdministrador AND idEstado = v_idEstadoConfirmado
                  ORDER BY id DESC LIMIT 1);

  IF v_cuit IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No hay una postulación aprobada para registrar una institución.';
  END IF;

  IF EXISTS (SELECT 1 FROM Instituciones WHERE cuit = v_cuit) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una institución registrada con ese CUIT.';
  END IF;

  --  Escrituras
  INSERT INTO Instituciones
    (nombre, cuit, direccion, email, latitud, longitud, descripcion, idTipoInstitucion, estado)
  VALUES
    (v_nombre, v_cuit, p_direccion, p_email, p_latitud, p_longitud, p_descripcion, p_idTipoInstitucion, TRUE);

  -- Será el parámetro que devolveremos como OUT para luego insertar los parametros opcionales que van en otras tablas (y otros SP)
  SET v_idInstitucion = LAST_INSERT_ID();


  INSERT INTO Discapacidades
    (idInstitucion, movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo)
  VALUES
    (v_idInstitucion, p_movilidad, p_visual, p_auditiva, p_verbal, p_intelectual, p_sindromeDown, p_autismo);

  UPDATE AdministradoresInstitucion
  SET idInstitucion = v_idInstitucion
  WHERE id = p_idAdministrador;

  COMMIT;

  SET p_idInstitucion = v_idInstitucion; -- Asignamos el id de la institucion creada en el SP al parametro OUT
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Telefono_Agregar
-- Agrega un telefono a una institucion, a un administrador o a un responsable.
--   p_tipoDueno: 'Institucion' | 'Administrador' | 'Responsable'
--   p_idDueno:   id de esa institucion / administrador / responsable.
-- Permisos (Dev&Co no edita telefonos):
--   Institucion   -> administrador activo de esa institucion.
--   Administrador -> el propio administrador (su usuario tiene que estar activo).
--   Responsable   -> el propio responsable (su usuario tiene que estar activo).
-- Maximo 2 telefonos por dueño y sin repetir el numero.
-- Un solo INSERT: no necesita transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Telefono_Agregar;

DELIMITER $$

CREATE PROCEDURE sp_Telefono_Agregar(
  IN p_idUsuarioActor INT,
  IN p_tipoDueno      VARCHAR(15),
  IN p_idDueno        INT,
  IN p_telefono       VARCHAR(30)
)
BEGIN
  DECLARE v_cantidad INT DEFAULT 0;
  DECLARE v_repetido INT DEFAULT 0;

  SET p_telefono = TRIM(p_telefono);

  IF p_idUsuarioActor IS NULL OR p_idDueno IS NULL OR COALESCE(p_telefono,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_tipoDueno IS NULL OR p_tipoDueno NOT IN ('Institucion', 'Administrador', 'Responsable') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El tipo de dueño no es válido.';
  END IF;

  -- Permiso y cantidad de telefonos actuales, segun el tipo de dueño
  IF p_tipoDueno = 'Institucion' THEN
    IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idDueno) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
    END IF;

    IF NOT EXISTS (SELECT 1
                   FROM AdministradoresInstitucion a
                   JOIN Usuarios u ON u.id = a.idUsuario
                   WHERE a.idUsuario = p_idUsuarioActor
                     AND a.idInstitucion = p_idDueno
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
    END IF;

    SELECT COUNT(*), COALESCE(SUM(telefono = p_telefono), 0)
    INTO v_cantidad, v_repetido
    FROM Telefonos WHERE idInstitucion = p_idDueno;

  ELSEIF p_tipoDueno = 'Administrador' THEN
    IF NOT EXISTS (SELECT 1
                   FROM AdministradoresInstitucion a
                   JOIN Usuarios u ON u.id = a.idUsuario
                   WHERE a.id = p_idDueno
                     AND a.idUsuario = p_idUsuarioActor
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar estos teléfonos.';
    END IF;

    SELECT COUNT(*), COALESCE(SUM(telefono = p_telefono), 0)
    INTO v_cantidad, v_repetido
    FROM Telefonos WHERE idAdministrador = p_idDueno;

  ELSE
    IF NOT EXISTS (SELECT 1
                   FROM Responsables r
                   JOIN Usuarios u ON u.id = r.idUsuario
                   WHERE r.id = p_idDueno
                     AND r.idUsuario = p_idUsuarioActor
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar estos teléfonos.';
    END IF;

    SELECT COUNT(*), COALESCE(SUM(telefono = p_telefono), 0)
    INTO v_cantidad, v_repetido
    FROM Telefonos WHERE idResponsable = p_idDueno;
  END IF;

  IF v_repetido > 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya tiene ese teléfono.';
  END IF;

  IF v_cantidad >= 2 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya tiene el máximo de 2 teléfonos.';
  END IF;

  IF p_tipoDueno = 'Institucion' THEN
    INSERT INTO Telefonos (idInstitucion, telefono) VALUES (p_idDueno, p_telefono);
  ELSEIF p_tipoDueno = 'Administrador' THEN
    INSERT INTO Telefonos (idAdministrador, telefono) VALUES (p_idDueno, p_telefono);
  ELSE
    INSERT INTO Telefonos (idResponsable, telefono) VALUES (p_idDueno, p_telefono);
  END IF;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Telefono_Eliminar
-- Quita un telefono de una institucion, de un administrador o de un responsable
-- Administrador y Responsable: no se puede quitar el unico telefono (minimo 1).
-- (se identifica por el id de la fila de Telefonos). Mismos tipos y permisos que
-- sp_Telefono_Agregar. Sin transaccion ni handler.
-- Si el telefono tambien es de otro dueño no se borra: solo se le saca este dueño.
-- Si no tenia otro dueño, se borra.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Telefono_Eliminar;

DELIMITER $$

CREATE PROCEDURE sp_Telefono_Eliminar(
  IN p_idUsuarioActor INT,
  IN p_tipoDueno      VARCHAR(15),
  IN p_idDueno        INT,
  IN p_idTelefono     INT
)
BEGIN
  IF p_idUsuarioActor IS NULL OR p_idDueno IS NULL OR p_idTelefono IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_tipoDueno IS NULL OR p_tipoDueno NOT IN ('Institucion', 'Administrador', 'Responsable') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El tipo de dueño no es válido.';
  END IF;

  IF p_tipoDueno = 'Institucion' THEN
    IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idDueno) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
    END IF;

    IF NOT EXISTS (SELECT 1
                   FROM AdministradoresInstitucion a
                   JOIN Usuarios u ON u.id = a.idUsuario
                   WHERE a.idUsuario = p_idUsuarioActor
                     AND a.idInstitucion = p_idDueno
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
    END IF;

    -- El telefono tiene que ser de ese dueño (evita borrar uno ajeno mandando otro id)
    IF NOT EXISTS (SELECT 1 FROM Telefonos WHERE id = p_idTelefono AND idInstitucion = p_idDueno) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No tiene ese teléfono.';
    END IF;

    IF EXISTS (SELECT 1 FROM Telefonos
               WHERE id = p_idTelefono AND (idAdministrador IS NOT NULL OR idResponsable IS NOT NULL)) THEN
      UPDATE Telefonos SET idInstitucion = NULL
      WHERE id = p_idTelefono AND idInstitucion = p_idDueno;
    ELSE
      DELETE FROM Telefonos WHERE id = p_idTelefono AND idInstitucion = p_idDueno;
    END IF;

  ELSEIF p_tipoDueno = 'Administrador' THEN
    IF NOT EXISTS (SELECT 1
                   FROM AdministradoresInstitucion a
                   JOIN Usuarios u ON u.id = a.idUsuario
                   WHERE a.id = p_idDueno
                     AND a.idUsuario = p_idUsuarioActor
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar estos teléfonos.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Telefonos WHERE id = p_idTelefono AND idAdministrador = p_idDueno) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No tiene ese teléfono.';
    END IF;

        -- Minimo 1 telefono: no se puede quitar el unico que tiene
    IF (SELECT COUNT(*) FROM Telefonos WHERE idAdministrador = p_idDueno) <= 1 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tiene que dejar al menos un teléfono. Agregue otro antes de quitar este.';
    END IF;

    IF EXISTS (SELECT 1 FROM Telefonos
               WHERE id = p_idTelefono AND (idInstitucion IS NOT NULL OR idResponsable IS NOT NULL)) THEN
      UPDATE Telefonos SET idAdministrador = NULL
      WHERE id = p_idTelefono AND idAdministrador = p_idDueno;
    ELSE
      DELETE FROM Telefonos WHERE id = p_idTelefono AND idAdministrador = p_idDueno;
    END IF;

  ELSE
    IF NOT EXISTS (SELECT 1
                   FROM Responsables r
                   JOIN Usuarios u ON u.id = r.idUsuario
                   WHERE r.id = p_idDueno
                     AND r.idUsuario = p_idUsuarioActor
                     AND u.activo = TRUE) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar estos teléfonos.';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Telefonos WHERE id = p_idTelefono AND idResponsable = p_idDueno) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No tiene ese teléfono.';
    END IF;

        -- Minimo 1 telefono: no se puede quitar el unico que tiene
    IF (SELECT COUNT(*) FROM Telefonos WHERE idResponsable = p_idDueno) <= 1 THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Tiene que dejar al menos un teléfono. Agregue otro antes de quitar este.';
    END IF;

    IF EXISTS (SELECT 1 FROM Telefonos
               WHERE id = p_idTelefono AND (idInstitucion IS NOT NULL OR idAdministrador IS NOT NULL)) THEN
      UPDATE Telefonos SET idResponsable = NULL
      WHERE id = p_idTelefono AND idResponsable = p_idDueno;
    ELSE
      DELETE FROM Telefonos WHERE id = p_idTelefono AND idResponsable = p_idDueno;
    END IF;
  END IF;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Idioma_Institucion_Agregar
-- Agrega un idioma a la institucion del administrador.
-- La clave primaria (idInstitucion, idIdioma) impide repetirlo: el handler
-- convierte ese error en un mensaje claro.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Idioma_Institucion_Agregar;

DELIMITER $$

CREATE PROCEDURE sp_Idioma_Institucion_Agregar(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_idIdioma        TINYINT
)
BEGIN
  DECLARE EXIT HANDLER FOR 1062
  BEGIN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución ya tiene ese idioma.';
  END;

  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL OR p_idIdioma IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Idiomas WHERE id = p_idIdioma) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El idioma no existe.';
  END IF;

  INSERT INTO IdiomaInstitucion (idInstitucion, idIdioma)
  VALUES (p_idInstitucion, p_idIdioma);
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Imagen_Institucion_Agregar
-- Agrega una imagen a la institucion del administrador. Solo guarda la
-- ruta; el archivo lo guarda el programa (C#). La descripcion es obligatoria.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Imagen_Institucion_Agregar;

DELIMITER $$

CREATE PROCEDURE sp_Imagen_Institucion_Agregar(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_urlImagen       VARCHAR(255),
  IN p_descripcion     VARCHAR(255)
)
BEGIN
  SET p_urlImagen   = TRIM(p_urlImagen);
  SET p_descripcion = TRIM(p_descripcion);

  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL
     OR COALESCE(p_urlImagen,'') = '' OR COALESCE(p_descripcion,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  INSERT INTO Imagenes (idInstitucion, urlImagen, descripcion)
  VALUES (p_idInstitucion, p_urlImagen, p_descripcion);
END$$

DELIMITER ;



-- ---------------------------------------------------------------------
-- sp_Institucion_Modificar
-- Modifica los datos de la ficha de la institucion del administrador.
--   * NO se pueden cambiar nombre ni CUIT (vienen de la postulacion aprobada)
--   * las 7 discapacidades se mandan siempre tal como quedaron en el formulario
-- Dos UPDATE (Instituciones y Discapacidades): van en una transaccion para
-- que se guarden los dos o ninguno ya que son tablas separadas.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Institucion_Modificar;

DELIMITER $$

CREATE PROCEDURE sp_Institucion_Modificar(
  IN p_idAdministrador   INT,
  IN p_idInstitucion     INT,
  IN p_direccion         VARCHAR(100),
  IN p_email             VARCHAR(100),
  IN p_latitud           DOUBLE,
  IN p_longitud          DOUBLE,
  IN p_descripcion       VARCHAR(3000),
  IN p_idTipoInstitucion TINYINT,
  -- Discapacidades
  IN p_movilidad         BOOL,
  IN p_visual            BOOL,
  IN p_auditiva          BOOL,
  IN p_verbal            BOOL,
  IN p_intelectual       BOOL,
  IN p_sindromeDown      BOOL,
  IN p_autismo           BOOL
)
BEGIN
  -- Cualquier error (incluidos los SIGNAL de validacion): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  -- Normalizar
  SET p_direccion   = TRIM(p_direccion);
  SET p_email       = LOWER(TRIM(p_email));
  SET p_descripcion = TRIM(p_descripcion);

  -- Validaciones que no necesitan la base
  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL
     OR COALESCE(p_direccion,'') = '' OR COALESCE(p_email,'') = ''
     OR p_latitud IS NULL OR p_longitud IS NULL OR COALESCE(p_descripcion,'') = ''
     OR p_idTipoInstitucion IS NULL
     OR p_movilidad IS NULL OR p_visual IS NULL OR p_auditiva IS NULL OR p_verbal IS NULL
     OR p_intelectual IS NULL OR p_sindromeDown IS NULL OR p_autismo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_email NOT REGEXP '^[^@ ]+@[^@ ]+\\.[^@ ]+$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email no tiene un formato válido.';
  END IF;

  -- Solo coordenadas validas (-90 a 90 del Ecuador  y -180 a 180 del Meridiano)
  IF p_latitud NOT BETWEEN -90 AND 90 OR p_longitud NOT BETWEEN -180 AND 180 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Las coordenadas no son válidas.';
  END IF;

  START TRANSACTION;

  -- Validaciones contra la DB
  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  -- El administrador tiene que estar activo y ser de esa institucion
  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM TipoInstitucion WHERE id = p_idTipoInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El tipo de institución no existe.';
  END IF;

  --  Escrituras
  UPDATE Instituciones
  SET direccion         = p_direccion,
      email             = p_email,
      latitud           = p_latitud,
      longitud          = p_longitud,
      descripcion       = p_descripcion,
      idTipoInstitucion = p_idTipoInstitucion
  WHERE id = p_idInstitucion;

  UPDATE Discapacidades
  SET movilidad    = p_movilidad,
      visual       = p_visual,
      auditiva     = p_auditiva,
      verbal       = p_verbal,
      intelectual  = p_intelectual,
      sindromeDown = p_sindromeDown,
      autismo      = p_autismo
  WHERE idInstitucion = p_idInstitucion;

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Idioma_Institucion_Eliminar
-- Quita un idioma de la institucion del administrador. Un solo DELETE.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Idioma_Institucion_Eliminar;

DELIMITER $$

CREATE PROCEDURE sp_Idioma_Institucion_Eliminar(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_idIdioma        TINYINT
)
BEGIN
  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL OR p_idIdioma IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM IdiomaInstitucion WHERE idInstitucion = p_idInstitucion AND idIdioma = p_idIdioma) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no tiene ese idioma.';
  END IF;

  DELETE FROM IdiomaInstitucion
  WHERE idInstitucion = p_idInstitucion AND idIdioma = p_idIdioma;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Imagen_Institucion_Eliminar
-- Quita una imagen de la institucion del administrador (se identifica por
-- el id de la fila de Imagenes). Solo borra el registro; el archivo lo
-- borra el programa (C#). Un solo DELETE.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Imagen_Institucion_Eliminar;

DELIMITER $$

CREATE PROCEDURE sp_Imagen_Institucion_Eliminar(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_idImagen        INT
)
BEGIN
  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL OR p_idImagen IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Imagenes WHERE id = p_idImagen AND idInstitucion = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no tiene esa imagen.';
  END IF;

  DELETE FROM Imagenes
  WHERE id = p_idImagen AND idInstitucion = p_idInstitucion;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_AdministradorInstitucion_ObtenerClaveReintento
-- Devuelve la clave (hash) de una cuenta de administrador que puede reintentar su postulacion,
-- para que la API verifique la contraseña antes de llamar a sp_AdministradorInstitucion_Registrar.
-- Es reintentable solo si: usuario inactivo, administrador sin institucion y ultima
-- postulacion Rechazada (mismas condiciones que usa el Registrar para permitir el reintento).
-- Si no cumple, devuelve 0 filas (no se informa el motivo).
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_AdministradorInstitucion_ObtenerClaveReintento;

DELIMITER $$

CREATE PROCEDURE sp_AdministradorInstitucion_ObtenerClaveReintento(
  IN p_email VARCHAR(100)
)
BEGIN
  DECLARE v_idEstadoRechazado INT;

  SET p_email = LOWER(TRIM(p_email));

  IF COALESCE(p_email,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  SET v_idEstadoRechazado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Rechazado');

  IF v_idEstadoRechazado IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Devuelve 1 fila con la clave si se cumplen todas las condiciones, sino devuelve 0 filas.
  SELECT u.clave AS Clave
  FROM Usuarios u
  JOIN AdministradoresInstitucion a ON a.idUsuario = u.id
  WHERE u.email = p_email
    AND u.activo = FALSE
    AND a.idInstitucion IS NULL
    AND (SELECT p.idEstado
         FROM PostulacionesAdminInstitucion p
         WHERE p.idAdministradorInstitucion = a.id
         ORDER BY p.id DESC LIMIT 1) = v_idEstadoRechazado;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Postulacion_ConsultarPorCodigo
-- Consulta el estado de una postulacion con su codigo de seguimiento + el email
-- de quien la hizo. Sirve para postulaciones de administradores y de alumnos:
--   * Administrador: el email es el del propio administrador.
--   * Alumno: el email es el del responsable (o el del propio alumno si es autogestor).
-- Si el codigo no existe o el email no coincide se da el MISMO mensaje, para no
-- revelar si el codigo existe.
-- Devuelve una fila: Tipo, Fecha, Estado, NombreInstitucion, Cuit, MotivoRechazo.
-- Usa las vistas vw_Postulaciones_Admin_Institucion y vw_Postulaciones_Alumnos.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Postulacion_ConsultarPorCodigo;

DELIMITER $$

CREATE PROCEDURE sp_Postulacion_ConsultarPorCodigo(
  IN p_codigo VARCHAR(30),
  IN p_email  VARCHAR(100)
)
BEGIN
  SET p_codigo = TRIM(p_codigo);
  SET p_email  = LOWER(TRIM(p_email));

  IF COALESCE(p_codigo,'') = '' OR COALESCE(p_email,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  -- Primero se busca entre las postulaciones de administradores...
  IF EXISTS (SELECT 1 FROM vw_Postulaciones_Admin_Institucion
             WHERE codigoSeguimiento = p_codigo AND emailAdministrador = p_email) THEN

    SELECT 'Administrador'    AS Tipo,
           fecha              AS Fecha,
           estado             AS Estado,
           nombreInstitucion  AS NombreInstitucion,
           cuitInstitucion    AS Cuit,
           motivoRechazo      AS MotivoRechazo
    FROM vw_Postulaciones_Admin_Institucion
    WHERE codigoSeguimiento = p_codigo AND emailAdministrador = p_email;

  -- ...y si no esta ahi, entre las de alumnos (el CUIT sale de la institucion del curso)
  ELSEIF EXISTS (SELECT 1 FROM vw_Postulaciones_Alumnos
                 WHERE codigoSeguimiento = p_codigo AND emailResponsable = p_email) THEN

    SELECT 'Alumno'           AS Tipo,
           v.fecha            AS Fecha,
           v.estado           AS Estado,
           v.institucion      AS NombreInstitucion,
           i.cuit             AS Cuit,
           v.motivoRechazo    AS MotivoRechazo
    FROM vw_Postulaciones_Alumnos v
    JOIN Instituciones i ON i.id = v.idInstitucion
    WHERE v.codigoSeguimiento = p_codigo AND v.emailResponsable = p_email;

  ELSE
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se encontró una postulación con esos datos.';
  END IF;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Institucion_DarDeBaja
-- Dev&Co da de baja una institucion. Es una baja logica: estado = FALSE (no se borra nada).
--   * las postulaciones de alumnos Pendientes y Aceptadas a los cursos de la institucion
--     se rechazan solas con el motivo 'Institución dada de baja.'
--   * las Confirmadas quedan como historial
--   * los administradores y sus cuentas no se tocan
-- Se revierte con sp_Institucion_Reactivar (las postulaciones rechazadas NO se restauran).
-- Va en una transaccion: se da de baja y se rechazan las pendientes, o no se hace nada.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Institucion_DarDeBaja;

DELIMITER $$

CREATE PROCEDURE sp_Institucion_DarDeBaja(
  IN p_idInstitucion  INT,
  IN p_idUsuarioDevCo INT     -- usuario Dev&Co que resuelve (lo toma C# de la sesion)
)
BEGIN
  DECLARE v_idTipoDevCo       TINYINT;
  DECLARE v_idEstadoPendiente INT;
  DECLARE v_idEstadoAceptado  INT;
  DECLARE v_idEstadoRechazado INT;

  -- Cualquier error (incluidos los SIGNAL de validacion): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  IF p_idInstitucion IS NULL OR p_idUsuarioDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  START TRANSACTION;

  -- Catalogos: se buscan por descripcion, igual que en el resto de los SP
  SET v_idTipoDevCo       = (SELECT id FROM TipoUsuario        WHERE descripcion = 'DevCo');
  SET v_idEstadoPendiente = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente');
  SET v_idEstadoAceptado  = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Aceptado');
  SET v_idEstadoRechazado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Rechazado');

  IF v_idTipoDevCo IS NULL OR v_idEstadoPendiente IS NULL
     OR v_idEstadoAceptado IS NULL OR v_idEstadoRechazado IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Solo un usuario Dev&Co activo puede dar de baja una institucion
  IF NOT EXISTS (SELECT 1 FROM Usuarios
                 WHERE id = p_idUsuarioDevCo
                   AND idTipoUsuario = v_idTipoDevCo
                   AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para dar de baja instituciones.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  -- Baja logica en un solo UPDATE. Si ya estaba dada de baja no se modifica ninguna fila.
  UPDATE Instituciones
  SET estado = FALSE
  WHERE id = p_idInstitucion
    AND estado = TRUE;

  IF ROW_COUNT() = 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución ya está dada de baja.';
  END IF;

  -- Las postulaciones Pendientes y Aceptadas (todavia sin confirmar) a cursos de esta institucion se rechazan solas
  UPDATE PostulacionesAlumnos
  SET idEstado      = v_idEstadoRechazado,
      motivoRechazo = 'Institución dada de baja.'
  WHERE idCurso IN (SELECT id FROM Cursos WHERE idInstitucion = p_idInstitucion)
    AND idEstado IN (v_idEstadoPendiente, v_idEstadoAceptado);

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Institucion_Reactivar
-- Dev&Co vuelve a activar una institucion dada de baja (estado = TRUE).
-- Las postulaciones que se rechazaron con la baja NO se restauran:
-- las familias tienen que postularse de nuevo.
-- Un solo UPDATE: sin transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Institucion_Reactivar;

DELIMITER $$

CREATE PROCEDURE sp_Institucion_Reactivar(
  IN p_idInstitucion  INT,
  IN p_idUsuarioDevCo INT
)
BEGIN
  DECLARE v_idTipoDevCo TINYINT;

  IF p_idInstitucion IS NULL OR p_idUsuarioDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  SET v_idTipoDevCo = (SELECT id FROM TipoUsuario WHERE descripcion = 'DevCo');

  IF v_idTipoDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Solo un usuario Dev&Co activo puede reactivar una institucion
  IF NOT EXISTS (SELECT 1 FROM Usuarios
                 WHERE id = p_idUsuarioDevCo
                   AND idTipoUsuario = v_idTipoDevCo
                   AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para reactivar instituciones.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  UPDATE Instituciones
  SET estado = TRUE
  WHERE id = p_idInstitucion
    AND estado = FALSE;

  IF ROW_COUNT() = 0 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución ya está activa.';
  END IF;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_AdministradorInstitucion_CrearAdicional
-- Un administrador activo de una institucion da de alta a otro administrador de la misma institucion.
-- La cuenta se crea activa y ya vinculada a la institucion del que la crea (no pasa por Dev&Co).
--   * La clave llega ya hasheada desde la API (el SP la guarda tal cual).
--   * Si la persona ya existe por otro rol (responsable, alumno autogestor) se reutiliza y se
--     conservan sus datos personales; necesita igual su propio email, distinto al del otro rol.
--   * Si la persona ya tiene cuenta de administrador, no se puede volver a dar de alta.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_AdministradorInstitucion_CrearAdicional;

DELIMITER $$

CREATE PROCEDURE sp_AdministradorInstitucion_CrearAdicional(
  IN  p_idUsuarioActor  INT,            -- administrador que da el alta (lo toma C# de la sesion)
  -- Datos personales
  IN  p_nombre          VARCHAR(50),
  IN  p_apellido        VARCHAR(50),
  IN  p_documento       VARCHAR(8),
  IN  p_domicilio       VARCHAR(100),
  IN  p_fechaNacimiento DATE,
  -- Cuenta
  IN  p_clave           VARCHAR(255),   -- ya llega hasheada desde la API
  IN  p_email           VARCHAR(100),
  IN  p_telefono        VARCHAR(30),
  -- Salida
  OUT p_idAdministrador INT
)
BEGIN
  DECLARE v_idTipoAdministrador TINYINT;
  DECLARE v_idInstitucion       INT;
  DECLARE v_idPersona           INT;
  DECLARE v_idUsuario           INT;
  DECLARE v_fechaNac            DATE;

  -- Casos que puedan pasar entre la validacion y el INSERT
  DECLARE EXIT HANDLER FOR 1062
  BEGIN
    ROLLBACK;
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Ya existe un registro con esos datos (documento o email).';
  END;

  -- Cualquier otro error (incluidos los SIGNAL de validacion): revierte y relanza
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  -- Normalizar (la clave no se toca, puede contener espacios)
  SET p_nombre    = TRIM(p_nombre);
  SET p_apellido  = TRIM(p_apellido);
  SET p_documento = TRIM(p_documento);
  SET p_domicilio = TRIM(p_domicilio);
  SET p_email     = LOWER(TRIM(p_email));
  SET p_telefono  = TRIM(p_telefono);

  -- Validaciones que no necesitan la base
  IF p_idUsuarioActor IS NULL
     OR COALESCE(p_nombre,'') = '' OR COALESCE(p_apellido,'') = '' OR COALESCE(p_documento,'') = ''
     OR COALESCE(p_domicilio,'') = '' OR p_fechaNacimiento IS NULL
     OR COALESCE(p_clave,'') = '' OR COALESCE(p_email,'') = ''
     OR COALESCE(p_telefono,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF p_documento NOT REGEXP '^[0-9]{7,8}$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El documento debe tener 7 u 8 dígitos.';
  END IF;

  IF p_email NOT REGEXP '^[^@ ]+@[^@ ]+\\.[^@ ]+$' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email no tiene un formato válido.';
  END IF;

  START TRANSACTION;

  SET v_idTipoAdministrador = (SELECT id FROM TipoUsuario WHERE descripcion = 'Administrador');

  IF v_idTipoAdministrador IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Quien da el alta tiene que ser un administrador ACTIVO con institucion (si no, queda en NULL)
  SET v_idInstitucion = (SELECT a.idInstitucion
                         FROM AdministradoresInstitucion a
                         JOIN Usuarios u ON u.id = a.idUsuario
                         WHERE a.idUsuario = p_idUsuarioActor AND u.activo = TRUE);

  IF v_idInstitucion IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para dar de alta administradores.';
  END IF;

  IF EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ese email ya está registrado.';
  END IF;

  -- La persona puede existir por otro rol: se reutiliza y se conservan sus datos guardados.
  SET v_idPersona = (SELECT id FROM Personas WHERE documento = p_documento);

  IF v_idPersona IS NOT NULL THEN
    IF EXISTS (SELECT 1 FROM AdministradoresInstitucion WHERE idPersona = v_idPersona) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una cuenta de administrador con ese documento.';
    END IF;
    SET v_fechaNac = (SELECT fechaNacimiento FROM Personas WHERE id = v_idPersona);
  ELSE
    SET v_fechaNac = p_fechaNacimiento;
  END IF;

  IF v_fechaNac > CURDATE() OR TIMESTAMPDIFF(YEAR, v_fechaNac, CURDATE()) < 18 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador debe ser mayor de 18 años.';
  END IF;

  -- Escrituras
  IF v_idPersona IS NULL THEN
    INSERT INTO Personas (nombre, apellido, documento, domicilio, fechaNacimiento)
    VALUES (p_nombre, p_apellido, p_documento, p_domicilio, p_fechaNacimiento);
    SET v_idPersona = LAST_INSERT_ID();
  END IF;

  INSERT INTO Usuarios (clave, email, activo, idTipoUsuario)
  VALUES (p_clave, p_email, TRUE, v_idTipoAdministrador);   -- activa desde el alta
  SET v_idUsuario = LAST_INSERT_ID();

  INSERT INTO AdministradoresInstitucion (idPersona, idUsuario, idInstitucion)
  VALUES (v_idPersona, v_idUsuario, v_idInstitucion);       -- ya vinculada a la institucion de quien la crea
  SET p_idAdministrador = LAST_INSERT_ID();

  INSERT INTO Telefonos (idAdministrador, telefono)
  VALUES (p_idAdministrador, p_telefono);

  COMMIT;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_AdministradorInstitucion_DarDeBaja
-- Da de baja a un administrador de institucion (baja logica: su usuario pasa a inactivo,
-- no se borra nada y sigue vinculado a su institucion para conservar el historial).
-- Quien puede hacerlo (p_idUsuarioActor, siempre un usuario activo):
--   * Dev&Co, sobre cualquier administrador
--   * otro administrador de la misma institucion
-- Reglas:
--   * no se puede dar de baja a uno mismo
--   * no se puede dar de baja al ultimo administrador activo de una institucion
--   * no se pide motivo
-- Un solo UPDATE: sin transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_AdministradorInstitucion_DarDeBaja;

DELIMITER $$

CREATE PROCEDURE sp_AdministradorInstitucion_DarDeBaja(
  IN p_idUsuarioActor  INT,   -- usuario que hace la baja (lo toma C# de la sesion)
  IN p_idAdministrador INT    -- administrador al que se le da de baja
)
BEGIN
  DECLARE v_idTipoDevCo        TINYINT;
  DECLARE v_idTipoActor        TINYINT;
  DECLARE v_idUsuarioAdmin     INT;
  DECLARE v_idInstitucionAdmin INT;
  DECLARE v_adminActivo        BOOL;

  IF p_idUsuarioActor IS NULL OR p_idAdministrador IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  -- Catalogo: se busca por descripcion, igual que en el resto de los SP
  SET v_idTipoDevCo = (SELECT id FROM TipoUsuario WHERE descripcion = 'DevCo');

  IF v_idTipoDevCo IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Quien hace la baja tiene que existir y estar activo (si no, v_idTipoActor queda en NULL)
  SET v_idTipoActor = (SELECT idTipoUsuario FROM Usuarios WHERE id = p_idUsuarioActor AND activo = TRUE);

  IF v_idTipoActor IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para dar de baja administradores.';
  END IF;

  -- Datos del administrador al que se le da de baja
  SET v_idUsuarioAdmin     = (SELECT idUsuario     FROM AdministradoresInstitucion WHERE id = p_idAdministrador);
  SET v_idInstitucionAdmin = (SELECT idInstitucion FROM AdministradoresInstitucion WHERE id = p_idAdministrador);

  IF v_idUsuarioAdmin IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador no existe.';
  END IF;

  -- Sin auto-baja
  IF v_idUsuarioAdmin = p_idUsuarioActor THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No podés darte de baja a vos mismo.';
  END IF;

  -- Permiso: Dev&Co puede con cualquiera. Un administrador solo con otro de SU institucion
  -- (si el otro todavia no tiene institucion, solo puede Dev&Co).
  IF v_idTipoActor <> v_idTipoDevCo THEN
    IF v_idInstitucionAdmin IS NULL
       OR NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion
                      WHERE idUsuario = p_idUsuarioActor
                        AND idInstitucion = v_idInstitucionAdmin) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para dar de baja administradores.';
    END IF;
  END IF;

  SET v_adminActivo = (SELECT activo FROM Usuarios WHERE id = v_idUsuarioAdmin);

  IF v_adminActivo = FALSE THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador ya está inactivo.';
  END IF;

  -- La institucion no puede quedarse sin ningun administrador activo
  IF v_idInstitucionAdmin IS NOT NULL
     AND NOT EXISTS (SELECT 1
                     FROM AdministradoresInstitucion a
                     JOIN Usuarios u ON u.id = a.idUsuario
                     WHERE a.idInstitucion = v_idInstitucionAdmin
                       AND a.id <> p_idAdministrador
                       AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede dar de baja al último administrador activo de la institución.';
  END IF;

  UPDATE Usuarios
  SET activo = FALSE
  WHERE id = v_idUsuarioAdmin;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_AdministradorInstitucion_Reactivar
-- Vuelve a activar a un administrador dado de baja. Pueden hacerlo los mismos
-- actores que la baja: Dev&Co, u otro administrador activo de la misma institucion.
-- Solo se reactivan cuentas que alguna vez fueron aprobadas (tienen institucion o una
-- postulacion Confirmada). Una cuenta pendiente o rechazada NO se reactiva por aca:
-- se resuelve con la aprobacion de Dev&Co (sp_PostulacionAdmin_Aprobar).
-- Un solo UPDATE: sin transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_AdministradorInstitucion_Reactivar;

DELIMITER $$

CREATE PROCEDURE sp_AdministradorInstitucion_Reactivar(
  IN p_idUsuarioActor  INT,   -- usuario que reactiva (lo toma C# de la sesion)
  IN p_idAdministrador INT    -- administrador que se reactiva
)
BEGIN
  DECLARE v_idTipoDevCo         TINYINT;
  DECLARE v_idEstadoConfirmado  INT;
  DECLARE v_idTipoActor         TINYINT;
  DECLARE v_idUsuarioAdmin      INT;
  DECLARE v_idInstitucionAdmin  INT;
  DECLARE v_adminActivo         BOOL;

  IF p_idUsuarioActor IS NULL OR p_idAdministrador IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  -- Catalogos: se buscan por descripcion, igual que en el resto de los SP
  SET v_idTipoDevCo        = (SELECT id FROM TipoUsuario        WHERE descripcion = 'DevCo');
  SET v_idEstadoConfirmado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Confirmado');

  IF v_idTipoDevCo IS NULL OR v_idEstadoConfirmado IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Catálogos incompletos.';
  END IF;

  -- Quien reactiva tiene que existir y estar activo (si no, v_idTipoActor queda en NULL)
  SET v_idTipoActor = (SELECT idTipoUsuario FROM Usuarios WHERE id = p_idUsuarioActor AND activo = TRUE);

  IF v_idTipoActor IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para reactivar administradores.';
  END IF;

  -- Datos del administrador que se reactiva
  SET v_idUsuarioAdmin     = (SELECT idUsuario     FROM AdministradoresInstitucion WHERE id = p_idAdministrador);
  SET v_idInstitucionAdmin = (SELECT idInstitucion FROM AdministradoresInstitucion WHERE id = p_idAdministrador);

  IF v_idUsuarioAdmin IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador no existe.';
  END IF;

  -- Permiso: Dev&Co puede con cualquiera. Un administrador solo con otro de SU institucion.
  IF v_idTipoActor <> v_idTipoDevCo THEN
    IF v_idInstitucionAdmin IS NULL
       OR NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion
                      WHERE idUsuario = p_idUsuarioActor
                        AND idInstitucion = v_idInstitucionAdmin) THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para reactivar administradores.';
    END IF;
  END IF;

  SET v_adminActivo = (SELECT activo FROM Usuarios WHERE id = v_idUsuarioAdmin);

  IF v_adminActivo = TRUE THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador ya está activo.';
  END IF;

  -- Solo cuentas que alguna vez fueron aprobadas: tienen institucion o una postulacion Confirmada
  IF v_idInstitucionAdmin IS NULL
     AND NOT EXISTS (SELECT 1 FROM PostulacionesAdminInstitucion
                     WHERE idAdministradorInstitucion = p_idAdministrador
                       AND idEstado = v_idEstadoConfirmado) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta nunca fue aprobada, no puede reactivarse.';
  END IF;

  UPDATE Usuarios
  SET activo = TRUE
  WHERE id = v_idUsuarioAdmin;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Usuario_ObtenerClave
-- Devuelve la clave (hash) del usuario logueado, para que la API verifique la contraseña
-- ACTUAL antes de llamar a sp_Usuario_CambiarClave (la comparacion del hash la hace la API).
-- Si el usuario no existe o esta inactivo devuelve 0 filas.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Usuario_ObtenerClave;

DELIMITER $$

CREATE PROCEDURE sp_Usuario_ObtenerClave(
  IN p_idUsuarioActor INT
)
BEGIN
  IF p_idUsuarioActor IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  SELECT clave AS Clave
  FROM Usuarios
  WHERE id = p_idUsuarioActor
    AND activo = TRUE;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Usuario_CambiarClave
-- Cambia la contraseña del usuario logueado (cualquier rol, incluido Dev&Co).
-- La API ya verifico la contraseña actual (con sp_Usuario_ObtenerClave) y ya genero el hash
-- de la nueva: el SP guarda p_clave
-- El usuario tiene que estar activo. Un solo UPDATE: sin transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Usuario_CambiarClave;

DELIMITER $$

CREATE PROCEDURE sp_Usuario_CambiarClave(
  IN p_idUsuarioActor INT,           -- usuario logueado (lo toma C# de la sesion)
  IN p_clave          VARCHAR(255)   -- ya llega hasheada desde la API
)
BEGIN
  IF p_idUsuarioActor IS NULL OR COALESCE(p_clave,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE id = p_idUsuarioActor AND activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para cambiar la contraseña.';
  END IF;

  UPDATE Usuarios SET clave = p_clave WHERE id = p_idUsuarioActor;
END$$

DELIMITER ;


-- ---------------------------------------------------------------------
-- sp_Alumno_Eliminar
-- El responsable elimina un alumno que tiene a su cargo.
-- Solo se puede si el alumno NUNCA fue postulado: si tiene alguna postulacion
-- (de cualquier estado) no se borra, porque la institucion ya la vio.
-- Borra el vinculo en ResponsablesPorAlumno, el alumno y, si nadie mas la usa,
-- la persona (asi el DNI queda libre para volver a registrarlo).
-- Si el alumno es el propio responsable (autogestor), la persona se conserva.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Alumno_Eliminar;

DELIMITER $$

CREATE PROCEDURE sp_Alumno_Eliminar(
  IN p_idResponsable INT,   -- lo toma C# de la sesion
  IN p_idAlumno      INT
)
BEGIN
  DECLARE v_idPersona     INT DEFAULT NULL;
  DECLARE v_idPersonaResp INT DEFAULT NULL;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  IF p_idResponsable IS NULL OR p_idAlumno IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  SELECT r.idPersona INTO v_idPersonaResp
  FROM Responsables r
  JOIN Usuarios u ON u.id = r.idUsuario
  WHERE r.id = p_idResponsable AND u.activo = TRUE;

  IF v_idPersonaResp IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para eliminar alumnos.';
  END IF;

  SELECT idPersona INTO v_idPersona FROM Alumnos WHERE id = p_idAlumno;

  IF v_idPersona IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no existe.';
  END IF;

  -- Tiene que ser un alumno a su cargo, o el propio responsable (autogestor)
  IF v_idPersona <> v_idPersonaResp
     AND NOT EXISTS (SELECT 1 FROM ResponsablesPorAlumno
                     WHERE idResponsable = p_idResponsable AND idAlumno = p_idAlumno) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no está a su cargo.';
  END IF;

  IF EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE idAlumno = p_idAlumno) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede eliminar un alumno que tiene postulaciones.';
  END IF;

  START TRANSACTION;

  DELETE FROM ResponsablesPorAlumno WHERE idAlumno = p_idAlumno;
  DELETE FROM Alumnos WHERE id = p_idAlumno;

  -- La persona se borra solo si no es tambien responsable ni administrador
  IF NOT EXISTS (SELECT 1 FROM Responsables WHERE idPersona = v_idPersona)
     AND NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion WHERE idPersona = v_idPersona) THEN
    DELETE FROM Personas WHERE id = v_idPersona;
  END IF;

  COMMIT;
END$$

DELIMITER ;

-- =====================================================================
-- LOGIN, RESENIAS, POSTULACIONES, CURSOS Y RECUPERACION DE CLAVE
-- =====================================================================

-- ------------------------------------------------------------------------------
-- sp_Validar_Usuario_Login
-- Ingresan parámetros email y clave.
-- Devuelve una fila con los datos para iniciar sesión: ids del usuario, de la persona
-- y del rol (responsable / administrador de institución / alumno autogestor). Los que no aplican vienen en NULL.

DROP PROCEDURE IF EXISTS sp_Validar_Usuario_Login;

DELIMITER $$

CREATE PROCEDURE sp_Validar_Usuario_Login(
IN p_email       VARCHAR(100),
IN p_clave       VARCHAR(255)
)
BEGIN
	DECLARE v_idUsuario INT;
	DECLARE v_activo BOOL;
	DECLARE v_clave VARCHAR (255);
    DECLARE v_idTipoUsuario TINYINT;

    -- limpieza
    SET p_email = LOWER(TRIM(p_email));

    -- validaciones
    IF p_email IS NULL OR p_email = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email es un campo obligatorio.';
	END IF;

    IF p_clave IS NULL OR p_clave = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La clave es obligatoria.';
	END IF;

		-- validamos que el usuario exista
	IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario o contraseña no válidos.';
	END IF;

	-- Si el usuario existe, recuperamos datos para poder iniciar sesión
   SELECT id, activo, clave, idTipoUsuario INTO v_idUsuario, v_activo, v_clave, v_idTipoUsuario FROM Usuarios WHERE email = p_email;

	-- validamos que la cuenta este activa
    IF v_activo = FALSE THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario inactivo.';
	END IF;

    -- Retornamos valores. La persona sale del rol que tenga el usuario:
    -- un usuario es responsable o administrador (los DevCo no tienen ninguno de los dos: vienen NULL).
    SELECT
    v_idUsuario AS IdUsuario,
    p_email AS Email,
    v_clave AS Clave,
    v_idTipoUsuario AS IdTipoUsuario,
    p.id AS IdPersona,
    p.nombre AS Nombre,
    p.apellido AS Apellido,
    r.id AS IdResponsable,
    al.id AS IdAlumno,
    ad.id AS IdAdministradorInstitucion,
    ad.idInstitucion AS IdInstitucion
    FROM Usuarios u
    LEFT JOIN Responsables r              ON r.idUsuario = u.id
    LEFT JOIN AdministradoresInstitucion ad ON ad.idUsuario = u.id
    LEFT JOIN Personas p                  ON p.id = COALESCE(r.idPersona, ad.idPersona)
    LEFT JOIN Alumnos al                  ON al.idPersona = r.idPersona -- solo si es autogestor
    WHERE u.id = v_idUsuario;
END$$
DELIMITER ;

-- -----------------------------------------------------------------------------
-- sp_Registrar_Resenia
-- solo postulaciones de alumnos en estado confirmado dejan resenia
-- en la creación de tabla solo se puede dejar una resenia por institucion. Permite solo una
-- Combinacion entre institucion y responsable. Ej:  2 hijos solo 1 resenia en misma institucion.
-- ------------------------------------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Registrar_Resenia$$

CREATE PROCEDURE sp_Registrar_Resenia(
IN p_comentario     TEXT,
IN p_calificacion   TINYINT,
IN p_idPostulacion  INT,
IN p_idResponsable  INT
)
BEGIN

	DECLARE v_idAlumno      INT;
	DECLARE v_idInstitucion INT;

    DECLARE EXIT HANDLER FOR 1062
		BEGIN
		ROLLBACK;
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable ya realizó reseña para la institución.';
	END;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;

	-- validaciones

	IF p_calificacion IS NULL OR p_calificacion < 1 OR p_calificacion > 5 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La calificacion debe ser entre 1 y 5.';
	END IF;

	IF p_comentario IS NULL OR TRIM(p_comentario) = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El comentario es un campo obligatorio.'; -- campo obligatorio porque en la tabla se creo como not null
	END IF;
    IF CHAR_LENGTH(TRIM(p_comentario)) > 255 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El comentario no puede superar los 255 caracteres.';
    END IF;

    -- comenzamos con la transacción a la BD
	START TRANSACTION;

		-- compruebo que exista la postulacion
		IF NOT EXISTS( SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no existe.';
		END IF;

		-- recupero valor de variables con consulta
		SELECT pos.idAlumno, cu.idInstitucion INTO v_idAlumno, v_idInstitucion FROM PostulacionesAlumnos pos INNER JOIN Cursos cu ON pos.idCurso = cu.id WHERE pos.id = p_idPostulacion FOR UPDATE;

		 -- Valido que el reponsable exista
		IF NOT EXISTS (SELECT 1 FROM Responsables WHERE id = p_idResponsable) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable indicado no existe.';
		END IF;

		-- Valido que el usuario responsable este activo
		IF NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN Usuarios u ON u.id = r.idUsuario
													WHERE r.id = p_idResponsable AND u.activo = TRUE) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario no tiene la cuenta activa.';
		END IF;

		-- valido que el estado sea confirmado
		IF NOT EXISTS( SELECT 1 FROM PostulacionesAlumnos po INNER JOIN EstadosPostulacion e ON po.idEstado = e.id
													  WHERE LOWER(e.descripcion) = 'confirmado' AND po.id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no esta confirmada.';
		END IF;

		-- Valido que el alumno postulado sea coincidente con un alumno registrado en su cuenta responsable o que el responsable/alumno sea autogestor de su postulación
		IF NOT EXISTS (SELECT 1 FROM Responsables r LEFT JOIN ResponsablesPorAlumno ra ON ra.idResponsable = r.id
													LEFT JOIN Alumnos al ON (al.id = ra.idAlumno OR al.idPersona = r.idPersona)
													WHERE r.id = p_idResponsable AND al.id = v_idAlumno) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no se encuentra registrado en la cuenta del responsable.';
		END IF;

		-- Valido que la institución se encuentre activa
		IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE estado = TRUE AND id = v_idInstitucion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El instituto no se encuentra activo.';
		END IF;

		-- valido que no haya una reseña previa para la institución por ese responsable
		IF EXISTS (SELECT 1 FROM Resenias r WHERE r.idResponsable = p_idResponsable AND r.idInstitucion = v_idInstitucion)
		THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable ya realizó reseña para la institución.';
		END IF;

		INSERT INTO Resenias (calificacion, comentario, fecha, idInstitucion, idResponsable) VALUES (p_calificacion, TRIM(p_comentario), CURDATE(), v_idInstitucion, p_idResponsable);

	COMMIT;

END $$
DELIMITER ;

-- ---------------------------------
-- sp_Responder_Resenia
-- el admin de institución solo puede responder una resenia cuando la respuesta sea NULL
-- Recibo el id de la resenia, p_respuestaInstitucion, p_idAdministradorInstitucion
-- --------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Responder_Resenia $$

CREATE PROCEDURE sp_Responder_Resenia(
IN p_idAdministradorInstitucion    INT,
IN p_idResenia                     INT,
IN p_respuestaInstitucion          TEXT -- (Lo declaro como text para que alcance a mostrar el error personalizado de la validación de longitud, con VARCHAR(255) no lleha a mostrarmelo)
)

BEGIN

    DECLARE v_idInstitucion    INT;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;

	-- Validaciones

	-- valido que la respuesta no sea null, ni vacía
	IF p_respuestaInstitucion IS NULL OR TRIM(p_respuestaInstitucion) = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe ingresar una respuesta en la reseña.';
	END IF;

	-- valido que la longitud no sea mayor a 255 caracteres
	IF CHAR_LENGTH(TRIM(p_respuestaInstitucion)) > 255 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La respuesta supera los 255 caracteres permitidos';
	END IF;

	-- comienzo la transacción
	START TRANSACTION;

		SELECT a.idInstitucion INTO v_idInstitucion FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON u.id = a.idUsuario WHERE a.id = p_idAdministradorInstitucion AND u.activo = TRUE;

        IF v_idInstitucion IS NULL THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Administrador inexistente, tiene cuenta inactiva o no tiene institución asignada.';
        END IF;

		-- valido que la resenia exista
		IF NOT EXISTS(SELECT 1 FROM Resenias WHERE id = p_idResenia AND idInstitucion = v_idInstitucion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La reseña no fue encontrada o no pertenece a su institución.';
		END IF;

		-- valido que la isntitución este activa
		IF NOT EXISTS(SELECT 1 FROM Instituciones WHERE id = v_idInstitucion AND estado = TRUE) THEN
			SIGNAL SQLSTATE  '45000' SET MESSAGE_TEXT = 'La institución no se encuentra activa';
		END IF;

		-- valido que la respuesta sea null
		IF NOT EXISTS (SELECT 1 FROM Resenias WHERE respuestaInstitucion IS NULL AND id = p_idResenia) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La reseña ya fue contestada.';
		END IF;

		-- actualizo el campo respuesta institucion
			UPDATE Resenias SET respuestaInstitucion = TRIM(p_respuestaInstitucion) WHERE id = p_idResenia AND idInstitucion = v_idInstitucion AND respuestaInstitucion IS NULL;

        IF ROW_COUNT() <> 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se pudo responder la reseña.';
		END IF;

    COMMIT;
END$$
DELIMITER ;

-- ----------------------------
-- sp_Registrar_Postulacion_Alumno
-- Un responsable registra una postulación de alumnno (puede ser autogestor - mismo idPersona)
-- El registro de postulación incluye la documentación asociada al idPostulacion
-- Parametros que debo recibir: p_idResponsable, p_idAlumno, p_idCurso, p_listaDocumentos (JSON)
-- ----------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Registrar_Postulacion_Alumno $$

CREATE PROCEDURE sp_Registrar_Postulacion_Alumno(
IN p_idResponsable      INT,
IN p_idAlumno           INT,
IN p_idCurso            INT,
IN p_listaDocumentos    JSON
)
BEGIN
	DECLARE v_idPostulacion     INT;
	DECLARE v_codigoSeguimiento VARCHAR(30);
	DECLARE v_capacidad         INT DEFAULT NULL;
	DECLARE v_inscriptos        INT DEFAULT NULL;
    DECLARE v_cicloLectivo      INT DEFAULT NULL;
    DECLARE v_idNivel           INT DEFAULT NULL;
    DECLARE v_anio              INT DEFAULT NULL;
	DECLARE v_nivelPersonalizado VARCHAR(255) DEFAULT NULL;
	DECLARE v_esNormalizado     TINYINT DEFAULT NULL;
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;

	-- Validaciones que no consultan la BD
	-- Validamos que la lista de documentos no este vacía, siempre necesitaremos al menos un documento para registrar la postulación
	IF p_listaDocumentos IS NULL THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe cargar documentación para registrar la postulación.';
	END IF;

	-- Validamos que el parámetro sea un Array Json
	IF JSON_TYPE(p_listaDocumentos) <> 'ARRAY' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La lista de documentos no tiene un formato válido.';
	END IF;

	-- Validamos que tenga al menos un documento
	IF JSON_LENGTH(p_listaDocumentos) = 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe adjuntar la documentación solicitada por la institución.';
	END IF;

	 -- Validamos que la logitud ingresada de ruta y descripcion sea correcta
	 IF EXISTS (SELECT 1 FROM JSON_TABLE (p_listaDocumentos, '$[*]' COLUMNS (numero FOR ORDINALITY)) AS d WHERE CHAR_LENGTH(JSON_UNQUOTE(JSON_EXTRACT(p_listaDocumentos, CONCAT('$[',d.numero - 1 ,'].descripcion')))) > 100
																											        OR CHAR_LENGTH( JSON_UNQUOTE(JSON_EXTRACT(p_listaDocumentos, CONCAT('$[', d.numero -1,'].ruta')))) > 255) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La descripción o la ruta superan la longitud permitida.';
	END IF;

	-- validamos que no vengan campos vacíos o null dentro del Json o extension no permitida
	IF EXISTS (SELECT 1 FROM JSON_TABLE (p_listaDocumentos, '$[*]' COLUMNS (descripcion VARCHAR(100) PATH '$.descripcion', ruta VARCHAR(255) PATH '$.ruta'))AS d WHERE d.descripcion IS NULL
																																								OR TRIM(d.descripcion) = ''
																																								OR d.ruta IS NULL
																																								OR TRIM(d.ruta)=''
																																								OR LOCATE('.', d.ruta) = 0
																																								OR LOWER(SUBSTRING_INDEX(d.ruta, '.', -1)) NOT IN ('jpg','jpeg','png','pdf')) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Los documentos tienen datos incompletos o extensiones no permitidas.';
	 END IF;

	-- inicio la transacción
	START TRANSACTION;

	SELECT capacidad, inscriptos, cicloLectivo, idNivel, anio,NULLIF(TRIM(nivelPersonalizado), '') INTO v_capacidad, v_inscriptos, v_cicloLectivo, v_idNivel, v_anio, v_nivelPersonalizado FROM Cursos WHERE id = p_idCurso FOR UPDATE; -- bloqueamos fila para que al momento de postular, paralelamente otro usuario este confirmando el último cupo y cuando esta postulación se acepte lo haga sin vacante disponible.

	-- Validaciones                                                                                          -- el bloqueo se mantiene hasta terminar transaccion (commit o rollback)
	-- Curso existente
	IF v_capacidad IS NULL THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Curso inexistente';
	END IF;

     -- Validación del ciclo lectivo -- primero detectamos tipo de curso
	IF v_idNivel IS NOT NULL AND v_anio IS NOT NULL AND v_nivelPersonalizado IS NULL THEN
		SET v_esNormalizado = 1;
	ELSEIF v_idNivel IS NULL AND v_anio IS NULL AND v_nivelPersonalizado IS NOT NULL THEN
		SET v_esNormalizado = 0;
	ELSE
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El curso tiene una configuración inválida (ni normalizado ni personalizado).';
	END IF;

	-- No permitir ciclos anteriores al año actual (ambos cursos)
	IF v_cicloLectivo < YEAR(CURDATE()) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se permiten postulaciones a cursos de ciclos lectivos anteriores.';
	-- No permitir año actual + 2 o posterior (ambos cursos)
	ELSEIF v_cicloLectivo > YEAR(CURDATE()) + 1 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se permiten postulaciones a cursos posteriores al próximo ciclo lectivo.';
	-- Única diferencia: los normalizados del año actual tienen plazo hasta el 15/02 inclusive
	ELSEIF v_esNormalizado = 1 AND v_cicloLectivo = YEAR(CURDATE()) AND CURDATE() > STR_TO_DATE(CONCAT(YEAR(CURDATE()), '-02-15'), '%Y-%m-%d') THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo de inscripción para los cursos normalizados del ciclo lectivo actual.';
	END IF;

	-- Verificar cupo
	IF (v_capacidad - v_inscriptos) <= 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El curso no tiene cupos disponibles';
	END IF;

	-- Usuario activo o existente
	IF NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN Usuarios u ON u.id = r.idUsuario
												WHERE r.id = p_idResponsable AND u.Activo = TRUE) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Responsable inexistente o usuario inactivo.';
	END IF;
	-- Alumno existente
	IF NOT EXISTS (SELECT 1 FROM Alumnos WHERE id = p_idAlumno) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Alumno inexistente.';
	END IF;
	-- Alumno a cargo o autogestor
	IF NOT EXISTS(SELECT 1 FROM Responsables r INNER JOIN Alumnos a ON a.idPersona = r.idPersona WHERE r.id = p_idResponsable AND r.idPersona = a.idPersona AND a.id = p_idAlumno)
											 AND NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN ResponsablesPorAlumno ra ON r.id = ra.idResponsable WHERE ra.idAlumno = p_idAlumno AND r.id = p_idResponsable) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no se encuentra asociado a la cuenta responsable.';
	END IF;

	-- Institucion activa
	IF NOT EXISTS (SELECT 1 FROM Cursos c INNER JOIN Instituciones i ON c.idInstitucion = i.id
										  WHERE i.estado = TRUE AND c.id = p_idCurso) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institucion se encuentra inactiva';
	END IF;

	-- Postulacion duplicada
	IF EXISTS(SELECT 1 FROM Alumnos a INNER JOIN PostulacionesAlumnos p ON a.id = p.idAlumno WHERE a.id = p_idAlumno AND p.idCurso = p_idCurso AND p.idEstado IN(1,2,3)) THEN -- no permite duplicados cuando fue confirmado el estado de postulacion pero si fue rechazado si se puede volver a postular
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno ya generó una postulación al curso.';
	END IF;

	-- Insertamos datos en tabla de postulacionesAlumnos
	INSERT INTO PostulacionesAlumnos (fecha, codigoSeguimiento, idCurso, idAlumno, idEstado ) VALUES (CURDATE(),'temporal', p_idCurso, p_idAlumno, 1); -- insertamos un valor temporal en el codigo de seguimiento porque no acepta null
	SET v_idPostulacion = LAST_INSERT_ID();
	SET v_codigoSeguimiento = CONCAT('PST-', LPAD(v_idPostulacion, 6, '0')); -- concatenando pst-idPosutlacion No deberia repetirse nunca
	UPDATE PostulacionesAlumnos SET codigoSeguimiento = v_codigoSeguimiento WHERE id = v_idPostulacion;

	-- Insertamos datos en tabla DocumentosPostulacion
	INSERT INTO DocumentosPostulacion (idPostulacionAlumnos, descripcion, ruta) SELECT v_idPostulacion, d.descripcion, d.ruta FROM JSON_TABLE(p_listaDocumentos,'$[*]' COLUMNS (descripcion VARCHAR (100) PATH '$.descripcion', ruta VARCHAR (255) PATH '$.ruta')) AS d;
	COMMIT;

	-- retornamos los valores de los campos del sp.
	SELECT
	p.id AS IdPostulacion,
	p.fecha AS FechaPostulacion,
	p.codigoSeguimiento AS CodigoSeguimiento,
	p.idCurso AS IdCurso,
	p.idAlumno AS IdAlumno,
	p.idEstado AS IdEstado,
	i.id AS IdInstitucion,
	pe.nombre AS Nombre,
	pe.apellido AS Apellido,
	pe.documento AS Documento,
	e.descripcion AS Descripcion
	FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON a.id = p.idAlumno
								INNER JOIN Personas pe ON pe.id = a.idPersona
								INNER JOIN Cursos c ON c.id = p.idCurso
								INNER JOIN Instituciones i ON c.idInstitucion = i.id
								INNER JOIN EstadosPostulacion e ON e.id = p.idEstado
	WHERE p.id = v_idPostulacion ;

END$$
DELIMITER ;

-- sp_Aceptar_Postulacion
-- Esto lo hace la institución!! Se recibe una postulación de alumnos.
-- Si se acepta, solo cambia el estado a aceptado. NO modifica cupos.
-- Se hace update a estado de la postulacion
-- Necesito parametros p_idAdministradorInstitucion y p_idPostulacion, al resto lo puedo obtener de la postulacion
-- ---------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Aceptar_Postulacion_Alumno$$

CREATE PROCEDURE sp_Aceptar_Postulacion_Alumno(
IN p_idAdministradorInstitucion     INT,
IN p_idPostulacion                  INT
)

BEGIN

	DECLARE v_inscriptos    INT DEFAULT NULL;
	DECLARE v_capacidad     INT DEFAULT NULL;
	DECLARE v_idPostulacion INT DEFAULT NULL;
    DECLARE v_cicloLectivo INT DEFAULT NULL;
    DECLARE v_idNivel INT DEFAULT NULL;
	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;
	-- iniciamos transacción
	START TRANSACTION;
	SELECT c.capacidad, c.inscriptos, p.id, c.cicloLectivo, c.idNivel INTO v_capacidad, v_inscriptos, v_idPostulacion, v_cicloLectivo, v_idNivel FROM Cursos c INNER JOIN PostulacionesAlumnos p ON p.idCurso = c.id WHERE p.id = p_idPostulacion FOR UPDATE;

	-- validamos que la postulacion exista
	IF v_idPostulacion IS NULL THEN
		SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'Postulación inexistente';
	END IF;

	-- Validar existencia de usuario/administrador
    IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON a.idUsuario = u.id
															  WHERE u.Id = a.idUsuario  AND a.id = p_idAdministradorInstitucion) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario o administrador inexistente.';
	END IF;
	-- validar que el admin se encuentre activo
	IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON a.idUsuario = u.id
															  WHERE u.activo = TRUE AND a.id = p_idAdministradorInstitucion) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario no se encuentra activo.';
	END IF;

	-- validar que la institución se encuentre activa
	IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Instituciones i ON a.idInstitucion = i.id
															 WHERE i.estado = TRUE and a.id = p_idAdministradorInstitucion ) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no se encuentra activa.';
	END IF;

	 -- validar que la postulación pertenezca a la institucion y a un curso de la institucion
	IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Cursos c ON a.idInstitucion = c.idInstitucion
															  INNER JOIN PostulacionesAlumnos p ON p.idCurso = c.id
															  WHERE a.id = p_idAdministradorInstitucion AND p.id = p_idPostulacion) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no pertenece a la institución.';
	END IF;

	-- validar que el alumno exista
	IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON p.idAlumno = a.id
													   WHERE  p.id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Alumno no existente. ';
	END IF;

     -- validar que el usuario responsable este activo -- es autogestor a.idPersona = r.idPersona -- es responsablePorAlumno res res.idAlumno = a.id
    IF NOT (EXISTS(SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON a.id = p.idAlumno
													    INNER JOIN ResponsablesPorAlumno r ON r.idAlumno = a.id
                                                        INNER JOIN Responsables re ON re.id = r.idResponsable
                                                        INNER JOIN Usuarios u ON re.idUsuario = u.id WHERE p.id = p_idPostulacion AND u.activo = TRUE) OR
			EXISTS(SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON p.idAlumno = a.id
													    INNER JOIN Responsables re ON re.idPersona = a.idPersona
                                                        INNER JOIN Usuarios u ON u.id = re.idUsuario WHERE p.id = p_idPostulacion AND u.activo = TRUE)) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno pertenece a una cuenta inactiva ';
	END IF;

    -- validamos que el ciclo lectivo no sea antiguo
    IF v_cicloLectivo < YEAR(CURDATE()) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede aceptar una postulación a un curso de un ciclo lectivo anterior.';
	END IF;

    -- plazo para cursos normalizados del ciclo actual: hasta el 20/02 inclusive
	IF v_idNivel IS NOT NULL AND v_cicloLectivo = YEAR(CURDATE())
	   AND CURDATE() > STR_TO_DATE(CONCAT(YEAR(CURDATE()), '-02-20'), '%Y-%m-%d') THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo para aceptar postulaciones de cursos normalizados del ciclo lectivo actual.';
	END IF;

	-- validar que el estado actual sea pendiente
	IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion AND idEstado = 1) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no se encuentra pendiente.';
	END IF;

	-- validamos que haya cupo
	IF (v_capacidad-v_inscriptos)<=0 THEN
		SIGNAL SQLSTATE '45000'  SET MESSAGE_TEXT = 'No hay cupos disponibles para aceptar la postulación.';
	END IF;

	-- hacemos el update de estado de postulacion
	UPDATE PostulacionesAlumnos SET idEstado = 2 WHERE id = p_idPostulacion AND idEstado = 1;

    IF ROW_COUNT() <> 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se pudo aceptar la postulación.';
	END IF;

	COMMIT;
	-- Retornamos valores que puedan sernos utiles
	SELECT
	p.id AS IdPostulacion,
	p.fecha AS FechaPostulacion,
	p.codigoSeguimiento AS CodigoSeguimiento,
	p.idCurso AS IdCurso,
	p.idAlumno AS IdAlumno,
	p.idEstado AS IdEstado,
	i.id AS IdInstitucion,
	pe.nombre AS Nombre,
	pe.apellido AS Apellido,
	pe.documento AS Documento,
	e.descripcion AS Descripcion
	FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON a.id = p.idAlumno
								INNER JOIN Personas pe ON pe.id = a.idPersona
								INNER JOIN Cursos c ON c.id = p.idCurso
								INNER JOIN Instituciones i ON c.idInstitucion = i.id
								INNER JOIN EstadosPostulacion e ON e.id = p.idEstado
	WHERE p.id = v_idPostulacion ;

END$$
DELIMITER ;

-- sp_Rechazar_Postulacion_Alumno
-- solo se rechazan las postulaciones pendietes, no necesitamos validar si queda o no cupo
-- necesitamos recibir parámetros p_idPostulacion y p_idAdministradorInstitucion
-- la trasacción es un update al estado de la postulación + motivo
-- ---------------------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Rechazar_Postulacion_Alumno$$

CREATE PROCEDURE sp_Rechazar_Postulacion_Alumno(
IN p_idPostulacion                INT,
IN p_idAdministradorInstitucion   INT,
IN p_motivoRechazo                TEXT
)
BEGIN

	DECLARE v_motivoRechazo VARCHAR(255);
    DECLARE v_cicloLectivo INT DEFAULT NULL;
	DECLARE v_idNivel      INT DEFAULT NULL;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
		ROLLBACK;
        RESIGNAL;
	END;

      -- iniciamos transacción
    START TRANSACTION;

		-- validamos que no sea null o vacío el motivo (hay que avisar porque fue el rechazo para orientar por si hay una nueva postulación a futuro)
		IF(p_motivoRechazo IS NULL OR TRIM(p_motivoRechazo) = '') THEN
			SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'Ingrese un motivo de rechazo válido.';
		END IF;

		-- validamos que el motivo de rechazo no tenga longitud mas larga de la posible
		IF  (CHAR_LENGTH(TRIM(p_motivoRechazo)) > 255) THEN
			SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'El motivo de rechazo supera los 255 caracteres disponibles.';
		END IF;

		-- si no se pasa de cantidad de caracteres lo guardo para llevarlo limpio a la bd
		SET v_motivoRechazo = TRIM(p_motivoRechazo);

		-- validamos que postulación exista
		IF NOT EXISTS(SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'Postulación inexistente';
		END IF;

        SELECT c.cicloLectivo, c.idNivel INTO v_cicloLectivo, v_idNivel FROM PostulacionesAlumnos p INNER JOIN Cursos c ON c.id = p.idCurso WHERE p.id = p_idPostulacion;

		-- plazo para cursos normalizados del ciclo actual: hasta el 20/02 inclusive
		IF v_idNivel IS NOT NULL AND v_cicloLectivo = YEAR(CURDATE())
		   AND CURDATE() > STR_TO_DATE(CONCAT(YEAR(CURDATE()), '-02-20'), '%Y-%m-%d') THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo para rechazar postulaciones de cursos normalizados del ciclo lectivo actual.';
		END IF;

		-- validamos usuario o administrador existente
		IF NOT EXISTS(SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON u.id = a.idUsuario WHERE a.id = p_idAdministradorInstitucion) THEN
			SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'Usuario o administrador inexistente.';
		END IF;
		-- validamos usuario activo
		IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON u.id = a.idUsuario WHERE u.activo = TRUE AND a.id = p_idAdministradorInstitucion) THEN
			SIGNAL SQLSTATE '45000' SET  MESSAGE_TEXT = 'El usuario no se encuentra activo.';
		END IF;
		-- validamos institucion activa
		IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Instituciones i ON a.idInstitucion = i.id
																 WHERE i.estado = TRUE and a.id = p_idAdministradorInstitucion ) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no se encuentra activa.';
		END IF;

		   -- validar que el usuario responsable este activo -- es autogestor a.idPersona = r.idPersona -- es responsablePorAlumno res res.idAlumno = a.id
		IF NOT (EXISTS(SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON a.id = p.idAlumno
															INNER JOIN ResponsablesPorAlumno r ON r.idAlumno = a.id
															INNER JOIN Responsables re ON re.id = r.idResponsable
															INNER JOIN Usuarios u ON re.idUsuario = u.id WHERE p.id = p_idPostulacion AND u.activo = TRUE) OR
				EXISTS(SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON p.idAlumno = a.id
															INNER JOIN Responsables re ON re.idPersona = a.idPersona
															INNER JOIN Usuarios u ON u.id = re.idUsuario WHERE p.id = p_idPostulacion AND u.activo = TRUE)) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno pertenece a una cuenta inactiva ';
		END IF;

		-- validar que la postulación pertenezca a la institucion y a un curso de la institucion
		IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Cursos c ON a.idInstitucion = c.idInstitucion
																  INNER JOIN PostulacionesAlumnos p ON p.idCurso = c.id
																  WHERE a.id = p_idAdministradorInstitucion AND p.id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no pertenece a la institución.';
		END IF;

		-- validar que el estado actual sea pendiente y motivo NULL
		IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion AND idEstado = 1 AND motivoRechazo IS NULL) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no se encuentra pendiente o ya tiene un motivo de rechazo.';
		END IF;

		UPDATE PostulacionesAlumnos SET idEstado = 4, motivoRechazo = v_motivoRechazo WHERE id = p_idPostulacion AND idEstado = 1 AND motivoRechazo IS NULL;

		IF ROW_COUNT() <> 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede rechazar la postulación.';
		END IF;

    COMMIT;

END$$
DELIMITER ;

-- sp_Confirmar_Postulacion_Alumno
-- confirma responsable o autogestor. OJO aca si quitamos cupo en institucion. incrementamos en 1 el valor de inscriptos!
-- parametros necesarios p_idPostulacion, id_responsable
-- ---------------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Confirmar_Postulacion_Alumno$$

CREATE PROCEDURE sp_Confirmar_Postulacion_Alumno(
IN p_idPostulacion    INT,
IN p_idResponsable    INT
)
BEGIN
	DECLARE v_capacidad                   INT;
    DECLARE v_inscriptos                  INT;
    DECLARE v_idCurso                     INT;
    DECLARE v_idNivelCurso                INT;
    DECLARE v_cicloLectivoCurso           INT;
    DECLARE v_anioCurso                   TINYINT;
    DECLARE v_nivelPersonalizadoCurso 	  VARCHAR(30);
    DECLARE v_idAlumno                    INT;
    DECLARE v_idAlumnoBloqueado           INT;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;

   SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
    START TRANSACTION;

		-- validaciones

		-- que exista la postulacion
		IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no existe.';
		END IF;

		SELECT idCurso INTO v_idCurso FROM PostulacionesAlumnos WHERE id = p_idPostulacion;

		-- validamos que el curso exista
		IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Cursos c ON p.idCurso = c.id
															WHERE p.id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El curso no existe.';
		END IF;
		SELECT capacidad, inscriptos INTO v_capacidad, v_inscriptos FROM Cursos WHERE id = v_idCurso FOR UPDATE; -- bloqueamos fila para que al momento de confirmar, paralelamente otro usuario este confirmando el último cupo y cuando esta postulación se acepte lo haga sin vacante disponible.

		-- que estado de postulación sea aceptado
		IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE idEstado = 2 AND id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no se encuentra aceptada por la institución.';
		END IF;

		-- Valido que el reponsable exista
		IF NOT EXISTS (SELECT 1 FROM Responsables WHERE id = p_idResponsable) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable indicado no existe.';
		END IF;

		-- Valido que el usuario responsable este activo
		IF NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN Usuarios u ON u.id = r.idUsuario
												WHERE r.id = p_idResponsable AND u.activo = TRUE) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario no tiene la cuenta activa.';
		END IF;

		-- Valido que el alumno postulado sea coincidente con un alumno registrado en su cuenta responsable o que el responsable/alumno sea autogestor de su postulacion
		IF NOT (EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos al ON al.id = p.idAlumno
															INNER JOIN ResponsablesPorAlumno ra ON ra.idAlumno = al.id
															WHERE p.id = p_idPostulacion AND ra.idResponsable = p_idResponsable) OR
			   EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos al ON al.id = p.idAlumno
															INNER JOIN Responsables r ON r.idPersona = al.idPersona
															WHERE p.id = p_idPostulacion AND r.id = p_idResponsable)) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no se encuentra registrado en la cuenta del responsable.';
		END IF;

		-- Institucion activa
		IF NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Cursos c ON p.idCurso = c.id
															INNER JOIN Instituciones i ON c.idInstitucion = i.id
															WHERE i.estado = TRUE AND p.id = p_idPostulacion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institucion se encuentra inactiva';
		END IF;

        SELECT c.idNivel, c.cicloLectivo, c.anio, c.nivelPersonalizado, p.idAlumno INTO v_idNivelCurso, v_cicloLectivoCurso, v_anioCurso, v_nivelPersonalizadoCurso, v_idAlumno FROM PostulacionesAlumnos p INNER JOIN Cursos c ON p.idCurso = c.id WHERE p.id = p_idPostulacion;
        SELECT id into v_idAlumnoBloqueado FROM Alumnos WHERE id = v_idAlumno FOR UPDATE;
       -- valido que el alumno no tenga en estado confirmado otra postulación con un curso idéntico y año lectivo. (Si permite otro año lectivo por si ha repetido)
        IF EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Cursos c ON p.idCurso = c.id WHERE p.idAlumno = v_idAlumno AND
                                                                                                      p.idEstado = 3 AND
                                                                                                      p.id <> p_idPostulacion AND
                                                                                                      c.cicloLectivo = v_cicloLectivoCurso  AND
                                                                                                      ((v_idNivelCurso IS NOT NULL AND v_anioCurso IS NOT NULL AND c.idNivel IS NOT NULL AND c.anio IS NOT NULL AND c.anio = v_anioCurso AND c.idNivel = v_idNivelCurso) OR
                                                                                                      (v_anioCurso IS NULL AND v_idNivelCurso IS NULL AND c.idNivel IS NULL AND c.anio IS NULL AND v_nivelPersonalizadoCurso IS NOT NULL AND c.nivelPersonalizado IS NOT NULL AND TRIM(c.nivelPersonalizado) <>''AND TRIM(v_nivelPersonalizadoCurso) <> '' AND LOWER(TRIM(c.nivelPersonalizado)) = LOWER(TRIM(v_nivelPersonalizadoCurso))))) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno ya tiene confirmada una postulación para un curso de las mismas características.';
        END IF;

		-- validamos que el ciclo lectivo no sea antiguo
		IF v_cicloLectivoCurso < YEAR(CURDATE()) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede confirmar una postulación a un curso de un ciclo lectivo anterior.';
		END IF;

        -- plazo para cursos normalizados del ciclo actual: hasta el 20/02 inclusive
		IF v_idNivelCurso IS NOT NULL AND v_cicloLectivoCurso = YEAR(CURDATE())
		   AND CURDATE() > STR_TO_DATE(CONCAT(YEAR(CURDATE()), '-02-20'), '%Y-%m-%d') THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo para confirmar postulaciones de cursos normalizados del ciclo lectivo actual.';
		END IF;

		-- que haya cupo
		IF (v_capacidad - v_inscriptos) <= 0 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Lo lamentamos. No quedan cupos disponibles para poder confirmar su postulación.';
		END IF;

		UPDATE PostulacionesAlumnos SET idEstado = 3 WHERE id = p_idPostulacion AND idEstado = 2;

		IF ROW_COUNT() <> 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación ya fue confirmada o no se encuentra aceptada por la institución.';
		END IF;

		UPDATE Cursos SET inscriptos = inscriptos + 1 WHERE id = v_idCurso;

    COMMIT;

END$$

DELIMITER ;

-- sp_Registrar_Curso-----------------------------
-- recibe por parametro:
-- p_idAministradorInstitucion, p_idTurno, p_cicloLectivo, p_idNivel, p_anio, p_nivelPersonalizado, p_division, p_capacidad, p_inscriptos
-- -----------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Registrar_Curso$$

CREATE PROCEDURE sp_Registrar_Curso(
IN p_idAdministradorInstitucion    INT,
IN P_idTurno                       INT,
IN p_cicloLectivo                  INT,
IN p_idNivel	                   INT,
IN p_anio                          TINYINT,
IN p_nivelPersonalizado            TEXT,
IN p_division                      TEXT,
IN p_capacidad                     SMALLINT,
IN p_inscriptos                    SMALLINT
)
BEGIN
	DECLARE v_idInstitucion     INT;
    DECLARE v_idTipoInstitucion TINYINT;
    DECLARE v_idCurso           INT;
    DECLARE v_estado            BOOLEAN;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
		ROLLBACK;
        RESIGNAL;
	END;

    -- validaciones

    -- valido administrador
    IF p_idAdministradorInstitucion IS NULL OR p_idAdministradorInstitucion <= 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Administrador no válido.';
    END IF;

    -- valido que el ingreso de turno
	IF p_idTurno IS NULL OR p_idTurno <= 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe ingresar un turno válido.';
	END IF;

      -- Valido que el ciclo lectivo sea el actual o el siguiente
		IF p_cicloLectivo IS NULL OR p_cicloLectivo NOT BETWEEN YEAR(CURDATE()) AND YEAR(CURDATE()) + 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Solo se pueden registrar cursos del ciclo lectivo actual o siguiente.';
		END IF;

    -- Comprobamos que nivel personalizado no venga vacío, si viene, lo seteamos null. Sino se normaliza con la limpieza
    SET p_nivelPersonalizado = NULLIF(TRIM(p_nivelPersonalizado), '');

    -- Nivel normalizado solo permite hasta el 1ro de febrero antes de que arranque el ciclo lectivo de ese año
		IF p_nivelPersonalizado IS NULL THEN
			-- Solo se permite el ciclo actual hasta el 1 de febrero
			IF p_cicloLectivo = YEAR(CURDATE()) AND CURDATE() > DATE(CONCAT(YEAR(CURDATE()), '-02-01')) THEN
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo para registrar cursos del ciclo lectivo actual.';
			END IF;
		END IF;

    -- validamos el ingreso de nivel personalizado o nivel normalizado
    -- si recibimos nivel personalizado, anio y idNivel van NULL
    IF p_nivelPersonalizado IS NOT NULL THEN
		IF p_idNivel IS NOT NULL OR p_anio IS NOT NULL THEN
			SIGNAL SQLSTATE '45000'	SET MESSAGE_TEXT = 'Si ingresa un nivel personalizado, el nivel y el año deben quedar vacíos.';
		END IF;
	ELSE
		-- Si es nivel normalizado, idNivel y anio son obligatorios y personalizado NULL
		IF p_idNivel IS NULL OR p_idNivel <= 0 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe ingresar un nivel escolar válido.';
		END IF;
		IF p_anio IS NULL OR p_anio <= 0 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe ingresar un año válido.';
		END IF;
        IF p_idNivel = 1 AND p_anio NOT BETWEEN 3 AND 5 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El año ingresado no es valido para Preescolar.';
        END IF;
        IF p_idNivel = 2 AND p_anio NOT BETWEEN 1 AND 7 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El año ingresado no es válido para Primaria.';
        END IF;
        IF p_idNivel = 3 AND p_anio NOT BETWEEN 1 AND 6 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El año ingresado no es válido para Secundaria.';
        END IF;

	END IF;

    -- valido la longitud del nivel personalizado
    IF p_nivelPersonalizado IS NOT NULL THEN
		IF CHAR_LENGTH(p_nivelPersonalizado) > 30 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nivel personalizado no puede superar los 30 caracteres.';
		END IF;
	END IF;

    -- Valido el ingreso de la división
    IF p_division IS NULL OR TRIM(p_division) = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Debe ingresar una división válida.';
    END IF;

    SET p_division = TRIM(p_division);

    -- valido la longitud de la división
    IF CHAR_LENGTH(TRIM(p_division)) > 50 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La división no puede superar los 50 caracteres.';
    END IF;

     -- valido capacidad que no sea menor a 1 ni
     IF p_capacidad IS NULL OR p_capacidad < 1 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Capacidad de alumnos inválida.';
     END IF;

     SET p_inscriptos = COALESCE(p_inscriptos, 0);

    -- valido el ingreso de inscriptos- no debe superar capacidad
    IF p_inscriptos < 0 OR p_inscriptos > p_capacidad THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Cantidad de alumnos inscriptos inválidos.';
    END IF;

    SET TRANSACTION ISOLATION LEVEL READ COMMITTED; -- para que la  segunda ejecucion no tenga una versión desactualizada de datos. Luego del bloqueo FOR UPDATE puede ver los datos que insertó la primer ejecución del sp.

    START TRANSACTION;

		-- valido que exista idAdministradorInstitucion y que tenga cuenta activa
		IF NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a INNER JOIN Usuarios u ON a.idUsuario = u.id
																  WHERE a.id = p_idAdministradorInstitucion AND u.activo = TRUE) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Administrador de institución inexistente o usuario inactivo.';
		END IF;

		-- Obtengo el idInstitucion y tipo institucion
		SELECT idInstitucion
		INTO v_idInstitucion
		FROM AdministradoresInstitucion
		WHERE id = p_idAdministradorInstitucion;

        -- bloqueamos fila para evitar dos registros iguales paralelos (duplicidad)
        SELECT idTipoInstitucion, estado
		INTO v_idTipoInstitucion, v_estado
		FROM Instituciones
		WHERE id = v_idInstitucion
		FOR UPDATE;

		-- valido que la institucion exista - la tenga asignada el administrador
		IF v_idInstitucion IS NULL THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador no tiene una institución asignada.';
		END IF;

        -- valido la variable obtenida v_idTipoInstitucion
        IF v_idTipoInstitucion IS NULL OR v_idTipoInstitucion NOT BETWEEN 1 AND 8 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El tipo de institucion no es válido.';
        END IF;

		-- valido que la institución este activa
		IF v_estado IS NULL OR v_estado = FALSE THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Institución inexistente o inactiva.';
		END IF;
        -- validamos que solo los institutos de idiomas pueden utilizar niveles personalizados
		IF p_nivelPersonalizado IS NOT NULL AND v_idTipoInstitucion <> 4 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Solo los institutos de idiomas pueden registrar cursos con nivel personalizado.';
		END IF;

        -- valido que el turno exista
		IF NOT EXISTS (SELECT 1 FROM Turnos WHERE id = p_idTurno) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El turno seleccionado no existe.';
		END IF;

        -- valido que nivel normalizado exista
        IF p_nivelPersonalizado IS NULL THEN
			IF NOT EXISTS (SELECT 1 FROM NivelesEscolares WHERE id = p_idNivel) THEN
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nivel escolar seleccionado no existe.';
			END IF;
        END IF;

        -- valido que no haya un curso con las mismas características en la institución
        IF p_nivelPersonalizado IS NULL AND p_anio IS NOT NULL AND p_idNivel IS NOT NULL THEN
			IF EXISTS (SELECT 1 FROM Cursos WHERE anio = p_anio AND idTurno = p_idTurno AND cicloLectivo = p_cicloLectivo AND idNivel = p_idNivel AND TRIM(division) COLLATE utf8mb4_0900_ai_ci = TRIM(p_division) COLLATE utf8mb4_0900_ai_ci AND idInstitucion =v_idInstitucion) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El curso ya se encuentra registrado.';
			END IF;
        END IF;
        -- valido que no haya un curso con las mismas características en la institución
        IF p_nivelPersonalizado IS NOT NULL AND p_anio IS NULL AND p_idNivel IS NULL THEN
			IF EXISTS (SELECT 1 FROM Cursos WHERE idTurno = p_idTurno AND cicloLectivo = p_cicloLectivo AND TRIM(division) COLLATE utf8mb4_0900_ai_ci = TRIM(p_division) COLLATE utf8mb4_0900_ai_ci AND TRIM(nivelPersonalizado) COLLATE utf8mb4_0900_ai_ci = TRIM(p_nivelPersonalizado) COLLATE utf8mb4_0900_ai_ci AND idInstitucion =v_idInstitucion) THEN
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El curso personalizado ya se encuentra registrado.';
			END IF;
        END IF;

        -- valido que el tipo de institucion permite el nivel personalizado
        IF v_idTipoInstitucion = 4 AND p_idNivel IS NOT NULL THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Los institutos de idiomas deben registrar cursos con nivel personalizado.';
		END IF;


        -- valido que el tipo de institucion corresponda al nivel seleccionado
		IF p_idNivel IS NOT NULL THEN
			IF v_idTipoInstitucion = 1 THEN
				IF p_idNivel <> 1 THEN SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece nivel Preescolar.';
				END IF;

			ELSEIF v_idTipoInstitucion = 2 THEN
				IF p_idNivel <> 2 THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece nivel Primaria.';
				END IF;

            ELSEIF v_idTipoInstitucion = 3 THEN
				IF p_idNivel <> 3 THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece nivel Secundaria.';
				END IF;

            ELSEIF v_idTipoInstitucion = 5 THEN
				IF p_idNivel NOT IN (1, 2) THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece niveles Preescolar y Primaria.';
				END IF;

			ELSEIF v_idTipoInstitucion = 6 THEN
				IF p_idNivel NOT IN (2, 3) THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece niveles Primaria y Secundaria.';
				END IF;

			ELSEIF v_idTipoInstitucion = 7 THEN
				IF p_idNivel NOT IN (1, 2, 3) THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El nivel seleccionado no corresponde a la institución.';
				END IF;

			ELSEIF v_idTipoInstitucion = 8 THEN
				IF p_idNivel NOT IN (1, 3) THEN
					SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución solo ofrece niveles Preescolar y Secundaria.';
				END IF;
			END IF;
		END IF;

        INSERT INTO Cursos (idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos)
        VALUES (v_idInstitucion, p_idTurno, p_cicloLectivo, p_idNivel, p_anio, p_nivelPersonalizado, p_division, p_capacidad, p_inscriptos);

        SET v_idCurso = LAST_INSERT_ID();

        COMMIT;

        -- retornamos valores
        SELECT
        id AS Id,
        idInstitucion AS IdInstitucion,
        idTurno AS IdTurno,
        cicloLectivo AS CicloLectivo,
        idNivel AS IdNivel,
        anio AS Anio,
        nivelPersonalizado AS NivelPersonalizado,
        division AS Division,
        capacidad AS Capacidad,
        inscriptos AS Inscriptos
        FROM Cursos
        WHERE id = v_idCurso;

END$$

DELIMITER ;

-- sp_Cancelar_Postulacion_Alumno
-- Lo hace el responsable (o autogestor) desde su cuenta.
-- Se puede cancelar en cualquier estado salvo Rechazada y Cancelada.
-- Solo se permiten postulaciones del ciclo lectivo actual o siguiente.
-- Solo si estaba CONFIRMADA devuelve la vacante: inscriptos - 1.
-- Parametros necesarios: p_idPostulacion, p_idResponsable
-- ---------------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Cancelar_Postulacion_Alumno$$

CREATE PROCEDURE sp_Cancelar_Postulacion_Alumno(
IN p_idPostulacion    INT,
IN p_idResponsable    INT
)
BEGIN
	DECLARE v_idCurso       INT DEFAULT NULL;
	DECLARE v_idEstado      INT DEFAULT NULL;
	DECLARE v_cicloLectivo  INT DEFAULT NULL;
    DECLARE v_idNivel INT DEFAULT NULL;

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;

    SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
	START TRANSACTION;

		-- valido que exista la postulacion
		SELECT idCurso INTO v_idCurso FROM PostulacionesAlumnos WHERE id = p_idPostulacion;

		IF v_idCurso IS NULL THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no existe.';
		END IF;

		SELECT id, cicloLectivo, idNivel INTO v_idCurso, v_cicloLectivo, v_idNivel FROM Cursos WHERE id = v_idCurso FOR UPDATE;-- bloqueamos la fila del curso para que la devolución de la vacante no pise a otra confirmación concurrente

		-- leemos el estado luego del bloqueo, para tener el valor vigente
		SELECT idEstado INTO v_idEstado FROM PostulacionesAlumnos WHERE id = p_idPostulacion;

		-- valido que el responsable exista
		IF NOT EXISTS (SELECT 1 FROM Responsables WHERE id = p_idResponsable) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable indicado no existe.';
		END IF;

		-- valido que el usuario responsable este activo
		IF NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN Usuarios u ON u.id = r.idUsuario
							   WHERE r.id = p_idResponsable AND u.activo = TRUE) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario no tiene cuenta activa.';
		END IF;

		-- valido que el alumno sea del responsable o que sea autogestor
		IF NOT (EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN ResponsablesPorAlumno ra ON ra.idAlumno = p.idAlumno
								WHERE p.id = p_idPostulacion AND ra.idResponsable = p_idResponsable) OR
				EXISTS (SELECT 1 FROM PostulacionesAlumnos p INNER JOIN Alumnos al ON al.id = p.idAlumno
														  INNER JOIN Responsables r ON r.idPersona = al.idPersona
								WHERE p.id = p_idPostulacion AND r.id = p_idResponsable)) THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no pertenece a un alumno de la cuenta del responsable.';
		END IF;

		-- valido estados que no se pueden cancelar
		IF v_idEstado = 4 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se puede cancelar una postulación rechazada.';
		END IF;

		IF v_idEstado = 5 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación ya se encuentra cancelada.';
		END IF;

		-- valido que el ciclo lectivo sea el actual o el siguiente
		IF v_cicloLectivo < YEAR(CURDATE()) OR v_cicloLectivo > YEAR(CURDATE()) + 1 THEN
			SIGNAL SQLSTATE '45000'
				SET MESSAGE_TEXT = 'Solo se pueden cancelar postulaciones del ciclo lectivo actual o del siguiente.';
		END IF;

        -- plazo para cursos normalizados del ciclo actual: hasta el 20/02 inclusive
		IF v_idNivel IS NOT NULL AND v_cicloLectivo = YEAR(CURDATE())
		   AND CURDATE() > STR_TO_DATE(CONCAT(YEAR(CURDATE()), '-02-20'), '%Y-%m-%d') THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Finalizó el plazo para cancelar postulaciones de cursos normalizados del ciclo lectivo actual.';
		END IF;

		UPDATE PostulacionesAlumnos SET idEstado = 5 WHERE id = p_idPostulacion AND idEstado IN (1,2,3); -- cancelamos (1 Pendiente, 2 Aceptado, 3 Confirmado)

		IF ROW_COUNT() <> 1 THEN
			SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se pudo cancelar la postulación.';
		END IF;

		-- solo si estaba confirmada ocupaba vacante: la devolvemos
		IF v_idEstado = 3 AND v_cicloLectivo >= YEAR(CURDATE()) THEN
			UPDATE Cursos SET inscriptos = inscriptos - 1 WHERE id = v_idCurso AND inscriptos > 0;

			IF ROW_COUNT() <> 1 THEN
				SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No se pudo devolver la vacante del curso.';
			END IF;
		END IF;

	COMMIT;

	-- Retornamos valores que puedan sernos utiles
	SELECT
	p.id AS IdPostulacion,
	p.fecha AS FechaPostulacion,
	p.codigoSeguimiento AS CodigoSeguimiento,
	p.idCurso AS IdCurso,
	p.idAlumno AS IdAlumno,
	p.idEstado AS IdEstado,
	i.id AS IdInstitucion,
	pe.nombre AS Nombre,
	pe.apellido AS Apellido,
	pe.documento AS Documento,
	e.descripcion AS Descripcion
	FROM PostulacionesAlumnos p INNER JOIN Alumnos a ON a.id = p.idAlumno
								INNER JOIN Personas pe ON pe.id = a.idPersona
								INNER JOIN Cursos c ON c.id = p.idCurso
								INNER JOIN Instituciones i ON c.idInstitucion = i.id
								INNER JOIN EstadosPostulacion e ON e.id = p.idEstado
	WHERE p.id = p_idPostulacion;

END$$
DELIMITER ;

 -- ------------------------------------------------------
 -- SP para completar el GrifView en Form de ListadoPostulacionAlumnos
 -- ----------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_postulaciones_Por_Responsable$$

CREATE PROCEDURE sp_postulaciones_Por_Responsable(
IN p_idResponsable    INT
)
BEGIN
	-- validaciones
	IF p_idResponsable IS NULL OR p_idResponsable < 1 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Responsable no válido';
	END IF;
	-- que el responsable exista
	IF NOT EXISTS (SELECT 1 FROM Responsables WHERE id = p_idResponsable) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Responsable no existente';
	END IF;

	SELECT p.id AS IdPostulacion,
	CONCAT(pe.nombre,' ',pe.apellido) AS Alumno,
	pe.documento AS Documento,
	i.nombre AS Institucion,
	c.cicloLectivo AS CicloLectivo,
	e.descripcion AS Estado,
    p.idEstado AS IdEstado,
    p.idAlumno AS IdAlumno
	FROM PostulacionesAlumnos p
	INNER JOIN Alumnos a ON p.idAlumno = a.id
	INNER JOIN Personas pe ON a.idPersona = pe.id
	INNER JOIN EstadosPostulacion e ON e.id = p.idEstado
	INNER JOIN Cursos c ON c.id = p.idCurso
	INNER JOIN Instituciones i ON i.id = c.idInstitucion
	WHERE EXISTS(SELECT 1 FROM Responsables r WHERE r.id = p_idResponsable AND (a.idPersona = r.idPersona
	OR EXISTS (SELECT 1 FROM ResponsablesPorAlumno re WHERE re.idResponsable = p_idResponsable AND re.idAlumno = a.id)))
	ORDER BY p.fecha DESC;

END$$
DELIMITER ;

-- -----------------------------------------------------
-- sp_Alumnos_Por_Responsable
-- ---------------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_alumnos_Por_Responsable$$

CREATE PROCEDURE sp_alumnos_Por_Responsable(
    IN p_idResponsable INT
)
BEGIN
    IF p_idResponsable IS NULL OR p_idResponsable < 1 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Responsable no válido';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM Responsables WHERE id = p_idResponsable) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Responsable no existente';
    END IF;

    SELECT a.id AS IdAlumno,
	CONCAT(pe.nombre, ' ', pe.apellido) AS NombreCompleto,
	pe.documento AS Documento,
	pe.fechaNacimiento AS FechaNacimiento,
	pe.domicilio AS Domicilio,
	COALESCE(ro.descripcion, 'Autogestor') AS Rol
    FROM Alumnos a
    INNER JOIN Personas pe ON pe.id = a.idPersona
    LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idAlumno = a.id AND rpa.idResponsable = p_idResponsable
    LEFT JOIN Roles ro ON ro.id = rpa.idRol
    WHERE EXISTS (SELECT 1 FROM Responsables r WHERE r.id = p_idResponsable AND (a.idPersona = r.idPersona    -- autogestor
               OR EXISTS (SELECT 1 FROM ResponsablesPorAlumno rpa WHERE rpa.idResponsable = p_idResponsable AND rpa.idAlumno = a.id)) -- alumno a cargo
    )
    ORDER BY pe.apellido, pe.nombre;
END$$

DELIMITER ;

-- ---------------------------------------------
-- sp_Olvide_Clave
-- Valida la cuenta y devuelve los datos para el correo.
-- La API genera la nueva contraseña con un generador criptográfico.
-- ---------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Olvide_Clave$$

CREATE PROCEDURE sp_Olvide_Clave(
IN p_email          VARCHAR (100)
)
BEGIN

    DECLARE v_idUsuario             INT;

    SET p_email = LOWER(TRIM(p_email));

    -- validamos que el email no sea null o vacío
    IF p_email IS NULL OR TRIM(p_email) = '' THEN
		SIGNAL SQLSTATE '45000'  SET MESSAGE_TEXT = 'El email es un campo obligatorio.';
    END IF;

    -- validamos que el email ingresado tenga formato válido
    IF p_email NOT LIKE '%_@_%._%' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email no tiene un formato válido.';
    END IF;

    -- validamos que el mail exista
    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El email ingresado no se encuentra registrado.';
    END IF;
    -- validamos que el usuario este activo
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE activo = TRUE AND email = p_email) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta se encuentra inactiva. Para activarla y recibir credenciales de acceso debe contacatrse con DEV&CO';
    END IF;

    SELECT id INTO v_idUsuario FROM Usuarios WHERE email = p_email;
        -- La clave solo se genera y hashea en la API; no se guarda en texto plano.
		SELECT u.email AS Email, COALESCE(p_resp.nombre, p_admin.nombre) AS Nombre, COALESCE(p_resp.apellido, p_admin.apellido) AS Apellido, u.id AS IdUsuario
		FROM Usuarios u LEFT JOIN Responsables r ON r.idUsuario = u.id
						LEFT JOIN Personas p_resp ON p_resp.id = r.idPersona
                        LEFT JOIN AdministradoresInstitucion ai ON ai.idUsuario = u.id
                        LEFT JOIN Personas p_admin ON p_admin.id = ai.idPersona WHERE u.id = v_idUsuario;

END$$

DELIMITER ;

-- ---------------------------------------------
-- sp_Actualizar_Clave
-- recibe la contraseña hasheada y la updatea en la bd
-- ---------------------------------------------
DELIMITER $$

DROP PROCEDURE IF EXISTS sp_Actualizar_Clave$$

CREATE PROCEDURE sp_Actualizar_Clave(
IN p_clave          TEXT,
IN p_idUsuario      INT
)
BEGIN

	DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;


    -- validamos que el clave no sea null o vacío
    IF p_clave IS NULL OR TRIM(p_clave) = '' THEN
		SIGNAL SQLSTATE '45000'  SET MESSAGE_TEXT = 'La clave es un campo obligatorio.';
    END IF;

    -- validamos la longitud de la clave hasheada
    IF CHAR_LENGTH(p_clave) > 255 THEN
		SIGNAL SQLSTATE '45000'  SET MESSAGE_TEXT = 'La longitud de la clave no es válido.';
    END IF;

    IF p_idUsuario IS NULL OR p_idUsuario <= 0 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El identificador del usuario no es válido.';
	END IF;

    -- validamos que el usuario exista
    IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE id = p_idUsuario) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario inexistente';
    END IF;

    -- validamos que el usuario este activo
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE activo = TRUE AND id = p_idUsuario) THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta se encuentra inactiva. Para activarla y recibir credenciales de acceso debe contacatrse con DEV&CO';
    END IF;

    START TRANSACTION;

    UPDATE Usuarios SET clave = p_clave WHERE id = p_idUsuario AND activo = TRUE;
    IF ROW_COUNT() = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La cuenta ya no está activa para recuperar la contraseña.';
    END IF;

    COMMIT;

END$$

DELIMITER ;

-- ---------------------------------------------------------------------
-- Procedimientos de telefonos de institucion conservados del respaldo.
-- La API actual utiliza sp_Telefono_Agregar y sp_Telefono_Eliminar.
-- Se mantienen tambien estas definiciones para reproducir todos los SP de la BD.
-- ---------------------------------------------------------------------

DROP PROCEDURE IF EXISTS sp_Telefono_Institucion_Agregar;

DELIMITER ;;

CREATE PROCEDURE `sp_Telefono_Institucion_Agregar`(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_telefono        VARCHAR(30)
)
BEGIN
  SET p_telefono = TRIM(p_telefono);

  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL OR COALESCE(p_telefono,'') = '' THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  -- El administrador tiene que estar activo y ser de esa institucion
  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  IF EXISTS (SELECT 1 FROM Telefonos WHERE idInstitucion = p_idInstitucion AND telefono = p_telefono) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución ya tiene ese teléfono.';
  END IF;

  INSERT INTO Telefonos (idInstitucion, telefono)
  VALUES (p_idInstitucion, p_telefono);
END ;;
DELIMITER ;

DROP PROCEDURE IF EXISTS sp_Telefono_Institucion_Eliminar;

DELIMITER ;;

CREATE PROCEDURE `sp_Telefono_Institucion_Eliminar`(
  IN p_idAdministrador INT,
  IN p_idInstitucion   INT,
  IN p_idTelefono      INT
)
BEGIN
  IF p_idAdministrador IS NULL OR p_idInstitucion IS NULL OR p_idTelefono IS NULL THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan datos obligatorios.';
  END IF;

  IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE id = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no existe.';
  END IF;

  IF NOT EXISTS (SELECT 1
                 FROM AdministradoresInstitucion a
                 JOIN Usuarios u ON u.id = a.idUsuario
                 WHERE a.id = p_idAdministrador
                   AND a.idInstitucion = p_idInstitucion
                   AND u.activo = TRUE) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'No cuenta con permiso para modificar esta institución.';
  END IF;

  -- El telefono tiene que ser de esa institucion (evita borrar uno ajeno mandando otro id)
  IF NOT EXISTS (SELECT 1 FROM Telefonos WHERE id = p_idTelefono AND idInstitucion = p_idInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La institución no tiene ese teléfono.';
  END IF;

  DELETE FROM Telefonos
  WHERE id = p_idTelefono AND idInstitucion = p_idInstitucion;
END ;;
DELIMITER ;
