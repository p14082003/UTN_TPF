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
  IN  p_clave                VARCHAR(255),   -- se guarda tal cual llega, ver tema hash
  IN  p_email                VARCHAR(100),
  IN  p_telefono             VARCHAR(30),
  -- Institución que declara (lo que verifica Dev&Co)
  IN  p_nombreInstitucion    VARCHAR(100),
  IN  p_cuitInstitucion      VARCHAR(11),
  IN  p_rutaConstanciaFiscal VARCHAR(255),
  IN  p_rutaHabilitacion     VARCHAR(255),
  IN  p_rutaConstanciaLaboral VARCHAR(255),
  -- Seguimiento (lo genera C#, el SP solo lo guarda)
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

  -- 1. Normalizar (la clave no se toca, puede contener espacios)
  SET p_nombre            = TRIM(p_nombre);
  SET p_apellido          = TRIM(p_apellido);
  SET p_documento         = TRIM(p_documento);
  SET p_domicilio         = TRIM(p_domicilio);
  SET p_email             = LOWER(TRIM(p_email));
  SET p_telefono          = TRIM(p_telefono);
  SET p_nombreInstitucion = TRIM(p_nombreInstitucion);
  SET p_cuitInstitucion   = TRIM(p_cuitInstitucion);

  -- 2. Validaciones que no necesitan la base
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

  -- 3. Catálogos: se buscan por descripción (no por id), igual que en el archivo de INSERTs
  SET v_idTipoAdministrador = (SELECT id FROM TipoUsuario        WHERE descripcion = 'Administrador');
  SET v_idEstadoPendiente   = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente');
  SET v_idEstadoRechazado   = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Rechazado');

-- Validacion DB Cargada con tipos de administrador y estados.
  IF v_idTipoAdministrador IS NULL OR v_idEstadoPendiente IS NULL OR v_idEstadoRechazado IS NULL THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Catálogos incompletos: falta Administrador, Pendiente o Rechazado.';
  END IF;

  -- 4. Validaciones contra la DB
  IF EXISTS (SELECT 1 FROM Usuarios WHERE email = p_email) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ese email ya está registrado.';
  END IF;

  -- La persona puede existir por otro rol (por ejemplo, ya es responsable): se reutiliza.
  -- En ese caso se conservan los datos personales ya guardados (no se actualizan).
  -- Para tener cuenta de administrador necesita su propio usuario y email.
  SET v_idPersona = (SELECT id FROM Personas WHERE documento = p_documento);

  IF v_idPersona IS NOT NULL THEN
    IF EXISTS (SELECT 1 FROM AdministradoresInstitucion WHERE idPersona = v_idPersona) THEN
      SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Ya existe una cuenta de administrador con ese documento. Consultá el estado de tu solicitud con el código de seguimiento que te enviamos por mail.';
    END IF;
    SET v_fechaNac = (SELECT fechaNacimiento FROM Personas WHERE id = v_idPersona);
  ELSE
    SET v_fechaNac = p_fechaNacimiento;
  END IF;

  IF v_fechaNac > CURDATE() OR TIMESTAMPDIFF(YEAR, v_fechaNac, CURDATE()) < 18 THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El administrador debe ser mayor de 18 años.';
  END IF;

  IF EXISTS (SELECT 1 FROM Instituciones WHERE cuit = p_cuitInstitucion) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una institución registrada con ese CUIT.';
  END IF;

  -- Si hay otra postulacion para esa institucion (cuit) con estado aprobado o pendiente lanzamos un error (solo se permite
  -- volver a enviar una postulacion en caso de tener una rechazada.
  IF EXISTS (SELECT 1 FROM PostulacionesAdminInstitucion
             WHERE cuitInstitucion = p_cuitInstitucion
               AND idEstado <> v_idEstadoRechazado) THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Ya existe una solicitud en curso o aprobada con ese CUIT.';
  END IF;

  -- 5. Escrituras
  SET v_fecha = CURDATE();

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
