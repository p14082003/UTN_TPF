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
    SET v_idUltimoEstado = (SELECT idEstado FROM PostulacionesAdminInstitucion
                            WHERE idAdministradorInstitucion = v_idAdministrador
                            ORDER BY id DESC LIMIT 1);
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
    SET v_idTelefono = (SELECT id FROM Telefonos WHERE idAdministrador = v_idAdministrador ORDER BY id LIMIT 1);

    IF v_idTelefono IS NULL THEN
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
-- sp_Telefono_Institucion_Agregar
-- Agrega un telefono a la institucion del administrador.
-- Un solo INSERT: no necesita transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Telefono_Institucion_Agregar;

DELIMITER $$

CREATE PROCEDURE sp_Telefono_Institucion_Agregar(
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
-- sp_Telefono_Institucion_Eliminar
-- Quita un telefono de la institucion del administrador (se identifica por
-- el id de la fila de Telefonos). Un solo DELETE: sin transaccion ni handler.
-- ---------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Telefono_Institucion_Eliminar;

DELIMITER $$

CREATE PROCEDURE sp_Telefono_Institucion_Eliminar(
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