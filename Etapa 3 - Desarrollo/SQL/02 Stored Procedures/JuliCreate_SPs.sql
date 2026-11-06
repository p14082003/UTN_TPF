USE InstiRed_DEV;
-- sp_Validar_Usuario_Login
-- Ingresan paramretros usuario y contraseña 
-- Necesito variables: idUsuario, usuario, activo, clave, tipoUduario

DELIMITER $$
DROP PROCEDURE IF EXISTS sp_Validar_Usuario_Login;

CREATE PROCEDURE sp_Validar_Usuario_Login(
IN p_usuario     VARCHAR(50), 
IN p_clave       VARCHAR(255)
)
BEGIN
	DECLARE v_idUsuario INT;
	DECLARE v_usuario VARCHAR(50);
	DECLARE v_activo BOOL;
	DECLARE v_clave VARCHAR (255);
    DECLARE v_idTipoUsuario TINYINT;
    
    -- limpieza
    SET p_usuario= TRIM(p_usuario);
    
    -- validaciones
    IF p_usuario IS NULL OR p_usuario = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario es un campo obligatorio.';
	END IF;
    
    IF p_clave IS NULL OR p_clave = '' THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La clave es obligatoria.';
	END IF;
    
		-- validamos que el usuario exista
	IF NOT EXISTS (SELECT 1 FROM Usuarios WHERE usuario = p_usuario) THEN 
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario o contraseña no válidos.';
	END IF;
   
	-- Si el usuario existe, recuperamos datos para poder iniciar sesión
   SELECT id, usuario, activo, clave, idTipoUsuario INTO v_idUsuario, v_usuario, v_activo, v_clave, v_idTipoUsuario FROM Usuarios WHERE usuario = p_usuario;
   
	-- validamos que la cuenta este activa
    IF v_activo = FALSE THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Usuario inactivo.';
	END IF;
    
    -- Retornamos valores 
    SELECT
    v_idUsuario AS IdUsuario,
    v_usuario AS Usuario,
    v_clave AS Clave,
    v_idTipoUsuario AS IdTipoUsuario;
END$$
DELIMITER ;

