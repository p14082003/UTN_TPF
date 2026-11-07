USE InstiRed_DEV;

DELIMITER $$
-- ------------------------------------------------------------------------------
-- sp_Validar_Usuario_Login
-- Ingresan paramretros usuario y contraseña 
-- Necesito variables: idUsuario, usuario, activo, clave, tipoUduario
-- ------------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Validar_Usuario_Login$$

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


DELIMITER $$
-- -----------------------------------------------------------------------------
-- sp_Registrar_Resenia
-- solo postulaciones de alumnos en estado aprobado dejan resenia
-- por decisión de UNIQUE KEY uq_resenias_institucion_responsable (idInstitucion, idResponsable) 
   -- en la creación de tabla solo se puede dejar una resenia por institucion. Permite solo una
   -- Combinacion entre institucion y responsable. Ej:  2 hijos solo 1 resenia en misma institucion.
-- ------------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS sp_Registrar_Resenia$$

CREATE PROCEDURE sp_Registrar_Resenia(
IN p_comentario     VARCHAR(255),
IN p_calificacion   TINYINT,
IN p_idPostulacion  INT,
IN p_idResponsable  INT
)
BEGIN

DECLARE v_idAlumno INT;
DECLARE v_idInstitucion INT;

DECLARE EXIT HANDLER FOR SQLEXCEPTION
	BEGIN 
		ROLLBACK;
		RESIGNAL;
	END;

-- validaciones

IF p_calificacion IS NULL OR p_calificacion < 1 OR p_calificacion > 5 THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La calificacion debe ser entre 1 y 5'; 
END IF;

IF p_comentario IS NULL OR TRIM(p_comentario) = '' THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El comentario es un campo obligatorio'; -- campo obligatorio porque en la tabla se creo como not null
END IF;

-- compruebo que exista la postulacion
IF NOT EXISTS( SELECT 1 FROM Postulaciones WHERE id = p_idPostulacion) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no fue confirmada.';
END IF; 

-- recupero valor de variables con consulta
SELECT pos.idAlumno, cu.idInstitucion INTO v_idAlumno, v_idInstitucion FROM Postulaciones pos INNER JOIN Cursos cu ON pos.idCurso = cu.id WHERE pos.id = p_idPostulacion;

-- Valido que el usuario responsable este activo 
IF NOT EXISTS (SELECT 1 FROM Responsables r INNER JOIN Usuarios u ON u.id = r.idUsuario    
                                            WHERE r.id = p_idResponsable AND u.activo = TRUE) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El usuario no tiene la cuenta activa.';
END IF;

-- Valido que el alumno postulado sea coincidente con un alumno registrado en su cuenta responsable
IF NOT EXISTS (SELECT 1 FROM  ResponsablesPorAlumno  
                                            WHERE idResponsable = p_idResponsable AND idAlumno = v_idAlumno) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no se encuentra registrado en la cuenta del responsable.';
END IF;

-- Valido que la institucion se encuentre activa
IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE estado = TRUE AND id = v_idInstitucion) THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El instituto no se encuentra activo';
END IF; 

-- valido que no haya una reseña previa para la institucion por ese responsable 
IF EXISTS (SELECT 1 FROM Resenias r WHERE r.idResponsable = p_idResponsable AND r.idInstitucion = v_idInstitucion) 
THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable ya realizó reseña para la institución.';
END IF;

-- valido que el estado sea aprobado
IF NOT EXISTS( SELECT 1 FROM Postulaciones po INNER JOIN EstadosPostulacion e ON po.idEstado = e.id
                                              WHERE e.descripcion = 'Aprobado' AND po.id = p_idPostulacion) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no esta aprobada.';
END IF;

-- comenzamos con la transaccion a la BD
START TRANSACTION;

INSERT INTO Resenias (calificacion, comentario, fecha, idInstitucion, idResponsable) VALUES (p_calificacion, p_comentario, CURDATE(), v_idInstitucion, p_idResponsable);

COMMIT;

END $$
DELIMITER ;

