USE InstiRed_DEV;

DELIMITER $$
-- ------------------------------------------------------------------------------
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
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La calificacion debe ser entre 1 y 5.'; 
END IF;

IF p_comentario IS NULL OR TRIM(p_comentario) = '' THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El comentario es un campo obligatorio.'; -- campo obligatorio porque en la tabla se creo como not null
END IF;

-- compruebo que exista la postulacion
IF NOT EXISTS( SELECT 1 FROM PostulacionesAlumnos WHERE id = p_idPostulacion) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no fue confirmada.';
END IF; 

-- recupero valor de variables con consulta
SELECT pos.idAlumno, cu.idInstitucion INTO v_idAlumno, v_idInstitucion FROM PostulacionesAlumnos pos INNER JOIN Cursos cu ON pos.idCurso = cu.id WHERE pos.id = p_idPostulacion;

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
IF NOT EXISTS (SELECT 1 FROM Responsables r LEFT JOIN ResponsablesPorAlumno ra ON ra.idResponsable = r.id
                                            LEFT JOIN Alumnos al ON (al.id = ra.idAlumno OR al.idPersona = r.idPersona)
                                            WHERE r.id = p_idResponsable AND al.id = v_idAlumno) THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El alumno no se encuentra registrado en la cuenta del responsable.';
END IF;

-- Valido que la institucion se encuentre activa
IF NOT EXISTS (SELECT 1 FROM Instituciones WHERE estado = TRUE AND id = v_idInstitucion) THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El instituto no se encuentra activo.';
END IF; 

-- valido que no haya una reseña previa para la institucion por ese responsable 
IF EXISTS (SELECT 1 FROM Resenias r WHERE r.idResponsable = p_idResponsable AND r.idInstitucion = v_idInstitucion) 
THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'El responsable ya realizó reseña para la institución.';
END IF;

-- valido que el estado sea confirmado
IF NOT EXISTS( SELECT 1 FROM PostulacionesAlumnos po INNER JOIN EstadosPostulacion e ON po.idEstado = e.id
                                              WHERE e.descripcion = 'Confirmado' AND po.id = p_idPostulacion) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La postulación no esta aprobada.';
END IF;

-- comenzamos con la transaccion a la BD
START TRANSACTION;

INSERT INTO Resenias (calificacion, comentario, fecha, idInstitucion, idResponsable) VALUES (p_calificacion, p_comentario, CURDATE(), v_idInstitucion, p_idResponsable);

COMMIT;

END $$
DELIMITER ;

DELIMITER $$
-- sp_Responder_Resenia
-- Recibo el id de la resenia, p_respuestaInstitucion, p_idIsntitucion 
DROP PROCEDURE IF EXISTS sp_Responder_Resenia $$

CREATE PROCEDURE sp_Responder_Resenia(
IN p_idInstitucion                 INT,
IN p_idResenia                     INT,
IN p_respuestaInstitucion          TEXT -- (Lo declaro como text para que alcance a mostrar el error personalizado de la validación de longitud, con VARCHAR(255) no lleha a mostrarmelo)
)

BEGIN 

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
IF CHAR_LENGTH(p_respuestaInstitucion) > 255 THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La respuesta supera los 255 caracteres peritidos';
END IF;
-- valido que la resenia exista 
IF NOT EXISTS(SELECT 1 FROM Resenias WHERE id = p_idResenia) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La reseña no fue encontrada.';
END IF;
-- valido que la isntitucion este activa
IF NOT EXISTS(SELECT 1 FROM Instituciones WHERE id = p_idInstitucion AND estado = TRUE) THEN 
	SIGNAL SQLSTATE  '45000' SET MESSAGE_TEXT = 'La institución no se encuentra activa';
END IF;
-- valido que la resenia sea de la institucion correspondiente 
IF NOT EXISTS(SELECT 1 FROM Resenias WHERE idInstitucion = p_idInstitucion AND id = p_idResenia) THEN
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La reseña no pertenece a su institución.';
END IF;
-- valido que la respuesta sea null
IF NOT EXISTS (SELECT 1 FROM Resenias WHERE respuestaInstitucion IS NULL AND id = p_idResenia) THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'La reseña ya fue contestada.';
END IF;
-- comienzo la transacción
START TRANSACTION;
-- actualizo el campo respuesta institucion 
	UPDATE Resenias SET respuestaInstitucion = p_respuestaInstitucion WHERE id = p_idResenia AND idInstitucion = p_idInstitucion AND respuestaInstitucion IS NULL;
COMMIT;
END$$
DELIMITER ;

DELIMITER $$ 
-- ----------------------------
-- sp_Registrar_Postulacion_Alumno
-- Parametros: p_idResponsable, p_idAlumno, p_idCurso
-- ----------------------------
DROP PROCEDURE IF EXISTS sp_Registrar_Postulacion_Alumno $$

CREATE PROCEDURE sp_Registrar_Postulacion_Alumno(
IN p_idResponsable      INT,
IN p_idAlumno           INT,
IN p_idCurso            INT
)
BEGIN 
DECLARE v_idPostulacion     INT;
DECLARE v_codigoSeguimiento VARCHAR(30);
DECLARE v_capacidad         INT;
DECLARE v_inscriptos        INT;
DECLARE EXIT HANDLER FOR SQLEXCEPTION
BEGIN
	ROLLBACK;
    RESIGNAL;
END;

-- inicio la transaccion 
START TRANSACTION; 

SELECT capacidad, inscriptos INTO v_capacidad, v_inscriptos FROM Cursos WHERE id = p_idCurso FOR UPDATE; -- bloqueamos fila para que al momento de postular, paralelamente otro usuario este confirmando el ultimo cupo y cuando esta postulacion se acepte lo haga sin vacante disponible. 

-- Validaciones                                                                                          -- el bloqueo se mantiene hasta terminar transaccion (commit o rollback)
-- Curso existente 
IF v_capacidad IS NULL THEN 
	SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Curso inexistente';
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
-- Alumno a cargo o autogesto
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

INSERT INTO PostulacionesAlumnos (fecha, codigoSeguimiento, idCurso, idAlumno, idEstado ) VALUES (CURDATE(),'temporal', p_idCurso, p_idAlumno, 1); -- insertamos un valor temporal en el codigo de seguimiento porque no acepta null
SET v_idPostulacion = LAST_INSERT_ID();
SET v_codigoSeguimiento = CONCAT('PST-', LPAD(v_idPostulacion, 6, '0')); -- concatenando pst-idPosutlacion No deberia repetirse nunca 
UPDATE PostulacionesAlumnos SET codigoSeguimiento = v_codigoSeguimiento WHERE id = v_idPostulacion; 
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