USE InstiRed_DEV;
-- sp_Validar_Usuario_Login
-- Ingresan parametros email y clave.
-- Devuelve una fila con los datos para iniciar sesion: ids del usuario, de la persona
-- y del rol (responsable / administrador de institucion / alumno autogestor). Los que no aplican vienen en NULL.

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
