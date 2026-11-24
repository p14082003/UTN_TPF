USE InstiRed_DEV;

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
	DECLARE EXIT HANDLER FOR SQLEXCEPTION 
	BEGIN
		ROLLBACK;
		RESIGNAL;
	END;
	-- iniciamos transacción 
	START TRANSACTION;
	SELECT c.capacidad, c.inscriptos,p.id, c.cicloLectivo INTO v_capacidad, v_inscriptos,v_idPostulacion, v_cicloLectivo FROM Cursos c INNER JOIN PostulacionesAlumnos p ON p.idCurso = c.id WHERE p.id = p_idPostulacion FOR UPDATE;

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

		SELECT id, cicloLectivo INTO v_idCurso, v_cicloLectivo FROM Cursos WHERE id = v_idCurso FOR UPDATE;-- bloqueamos la fila del curso para que la devolución de la vacante no pise a otra confirmación concurrente

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
	CONCAT(pe.nombre, ' ', pe.apellido) AS NombreCompleto
    FROM Alumnos a
    INNER JOIN Personas pe ON pe.id = a.idPersona
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
