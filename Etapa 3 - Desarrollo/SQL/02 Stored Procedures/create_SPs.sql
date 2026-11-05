USE InstiRed_DEV;

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