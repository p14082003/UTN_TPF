-- -----Test sp validar usuario Login--------
-- Select * from Usuarios; 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', 'test1234');     
CALL sp_Validar_Usuario_Login('LBenitez@mail.test ', 'test1234'); --  funciona porque se pasa a minuscula con lower y se recortan espacios con trim
CALL sp_Validar_Usuario_Login('', 'test1234'); -- falla por email 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', ''); -- falla por clave 
CALL sp_Validar_Usuario_Login('clarisa@mail.test', '1234'); -- falla por usuario no registrado (no válido)
CALL sp_Validar_Usuario_Login('rsilva@mail.test', 'test1234'); -- falla por usuario inactivo

-- ------Test sp registrar resenia -------- 
CALL sp_Registrar_Resenia('', 4, 2, 2); -- falla por comentario vacío
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 7, 2, 2); -- falla por valoracion mas alta que 5
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', -3, 2, 2); -- falla por valoracion mas baja que 1
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', NULL, 2, 2); -- falla por valoracion NULL
CALL sp_Registrar_Resenia( REPEAT('A', 256), 2, 2, 2); -- falla por longitud
CALL sp_Registrar_Resenia('        ', 3, 2, 2); -- falla por comentario con espacios
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 2, 1000); -- falla por responsable inexistente
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 10, 8); -- deberia fallar por responsable inactivo
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 4, 1); -- deberia fallar por no coincide responsable con postulacion 
-- UPDATE Instituciones SET estado = FALSE WHERE id = 2;
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 4, 3); -- deberia fallar por institucion inactiva pero necesitamos hacer un insert o cambiar el estado de una institucion. (para probar este setee momentaneamente el estado de la institucion 3 en false)
-- UPDATE Instituciones SET estado = TRUE WHERE id = 2;
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 2, 2); -- este intenta hacer nuevamente resenia a misma institucion, resenia duplicada
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Excelente clima', 5, 8, 6); -- este es correcto
-- select * from resenias;

-- ----Test sp responder resenia --------
CALL sp_Responder_Resenia( 1, 1,''); -- valido que la respuesta no quede vacia 
CALL sp_Responder_Resenia( 1, 1, NULL); -- valido que la respuesta no quede null 
CALL sp_Responder_Resenia( 1, 1, REPEAT('A', 256)); -- valido que la respuesta no mas larga
CALL sp_Responder_Resenia( 1, 1200,'Gracias por su reseña, hacemos todo lo que este en nuestras manos para que nuestros alumnos alcancen la excelencia.'); -- valido que exista
CALL sp_Responder_Resenia( 1, 3,'Gracias por su reseña, hacemos todo lo que este en nuestras manos para que nuestros alumnos alcancen la excelencia.'); -- valido que pertenezca a la institucion logeado
-- UPDATE Instituciones SET estado = FALSE WHERE id = 2;
CALL sp_Responder_Resenia( 2, 3,'Gracias por su reseña, hacemos todo lo que este en nuestras manos para que nuestros alumnos alcancen la excelencia.'); -- valido que la institucion este activa (momentaneamente pongo en estado false a una)
-- UPDATE Instituciones SET estado = TRUE WHERE id = 2;
CALL sp_Responder_Resenia( 1, 2,'Gracias por su reseña, hacemos todo lo que este en nuestras manos para que nuestros alumnos alcancen la excelencia.'); -- valido que no exista ya una respuesta 
-- SELECT * FROM Resenias WHERE id = 1;
CALL sp_Responder_Resenia( 1, 1,'Gracias por su reseña, hacemos todo lo que este en nuestras manos para que nuestros alumnos alcancen la excelencia. (Caso de éxito.)'); -- esta si deberia funcionar
-- UPDATE Resenias SET respuestaInstitucion = NULL WHERE id = 1;
-- SELECT * FROM Resenias WHERE id = 1;

-- Test sp registrar postulacion
	-- SETEAMOS LISTAS (JSON)de documentos para probar  
    SET @documentos_validos = JSON_ARRAY(
    JSON_OBJECT(
        'descripcion', 'DNI',
        'ruta', '/documentos/dni.jpg'
    ),
    JSON_OBJECT(
        'descripcion', 'Partida de nacimiento',
        'ruta', '/documentos/partida.pdf'
    ),
    JSON_OBJECT(
        'descripcion', 'Certificado médico',
        'ruta', '/documentos/certificado.png'
    )
);
	-- seteamos una lista invalida de longitud
        SET @documentos_invalidos = JSON_ARRAY(
    JSON_OBJECT(
        'descripcion', 'DNI',
        'ruta', '/documentos/dni.jpg'
    ),
    JSON_OBJECT(
        'descripcion', 'Partida de nacimiento',
        'ruta', CONCAT(REPEAT('A', 252), '.pdf')
    ),
    JSON_OBJECT(
        'descripcion', 'holalala',
        'ruta', '/documentos/certificado.png'
    )
);

	-- seteamos una lista invalida null o cadena vacia 
        SET @documentos_invalidos = JSON_ARRAY(
    JSON_OBJECT(
        'descripcion', 'DNI',
        'ruta', '/documentos/dni.jpg'
    ),
    JSON_OBJECT(
        'descripcion', 'Partida de nacimiento',
        'ruta', ''
    ),
    JSON_OBJECT(
        'descripcion', 'holalala',
        'ruta', '/documentos/certificado.png'
    )
);

CALL sp_Registrar_Postulacion_Alumno(1, 1, 1002, @documentos_validos); -- curso existente 
CALL sp_Registrar_Postulacion_Alumno(1, 1, 15, @documentos_validos); -- cupo disponible 
CALL sp_Registrar_Postulacion_Alumno(1002, 1, 12, @documentos_validos);-- usuario inexistente 
CALL sp_Registrar_Postulacion_Alumno(8, 11, 12, @documentos_validos);-- usuario inactivo
CALL sp_Registrar_Postulacion_Alumno(1, 1002, 12, @documentos_validos);-- alumno inexistente 
CALL sp_Registrar_Postulacion_Alumno(1, 2, 12, @documentos_validos); -- reponsable a cargo del alumno o autogestor 
-- UPDATE Instituciones SET estado = FALSE WHERE id = 9;
CALL sp_Registrar_Postulacion_Alumno(1, 1, 13, @documentos_validos); -- institucion activa
-- UPDATE Instituciones SET estado = TRUE WHERE id = 9;
CALL sp_Registrar_Postulacion_Alumno(9, 12, 12, @documentos_validos); -- postulacion duplicada 
CALL sp_Registrar_Postulacion_Alumno(1, 1, 2, @documentos_invalidos); -- documentos invalidos (probe null, '' y sin extension)
CALL sp_Registrar_Postulacion_Alumno(2, 2, 3, @documentos_invalidos); -- descripcion documento con longitud larga o datos incompletos o no permitidos (depende que set usemos arriba)
CALL sp_Registrar_Postulacion_Alumno(1, 1, 2, @documentos_validos); -- caso exitoso
CALL sp_Registrar_Postulacion_Alumno(1, 1, 12, @documentos_validos); -- caso exitoso
-- CASO AUTOGESTOR 
-- preparo documentos validos 
SET @documentos_validos = JSON_ARRAY(
    JSON_OBJECT(
        'descripcion', 'DNI de Marcos Acosta',
        'ruta', 'marcos_dni.pdf'
    ),
    JSON_OBJECT(
        'descripcion', 'Certificado de estudios',
        'ruta', 'marcos_certificado.pdf'
    )
);

CALL sp_Registrar_Postulacion_Alumno(6, 8, 8, @documentos_validos);
-- probamos nuevas validaciones con postulacion con estado cancelado que si deberia dejar postular 
UPDATE Cursos SET cicloLectivo = 2027 WHERE id = 18;
CALL sp_Registrar_Postulacion_Alumno(1, 1, 18, @documentos_validos); -- normalizado 2025 no dejo 
CALL sp_Registrar_Postulacion_Alumno(1, 1, 18, @documentos_validos); -- normalizado 2028 no dejo 
CALL sp_Registrar_Postulacion_Alumno(1, 1, 18, @documentos_validos); -- normalizado 2026 actual con ciclo lectivo comenzado no deja
CALL sp_Registrar_Postulacion_Alumno(1, 1, 18, @documentos_validos); -- normalizado 2027 actual si deja 
 UPDATE Cursos SET cicloLectivo = 2027 WHERE id = 18;
select * from PostulacionesAlumnos;
select * from DocumentosPostulacion;

-- sp Aceptar postulación 
CALL sp_Aceptar_Postulacion_Alumno(9, 1002); -- Postulación inexistente
CALL sp_Aceptar_Postulacion_Alumno(1002, 13); -- Administrador inexistente
CALL sp_Aceptar_Postulacion_Alumno(8, 12); -- Administrador con usuario inactivo
-- UPDATE Instituciones SET estado = FALSE WHERE id = 9;
CALL sp_Aceptar_Postulacion_Alumno(9, 13); -- Institución inactiva
-- UPDATE Instituciones SET estado = TRUE WHERE id = 9;
CALL sp_Aceptar_Postulacion_Alumno(1, 13); -- La postulación pertenece a otra institución
CALL sp_Aceptar_Postulacion_Alumno(9, 15); -- La postulación no se encuentra pendiente
-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id = 18;  -- la ponemos pendiente para que podamos aceptar y falle por falta de cupos
CALL sp_Aceptar_Postulacion_Alumno(9, 18); -- Curso sin cupos disponibles
-- UPDATE PostulacionesAlumnos SET idEstado = 4 WHERE id = 18; -- la dejamos rechazada como estaba originalmente
CALL sp_Aceptar_Postulacion_Alumno(9, 13); -- caso exitoso
-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id = 13 -- Para dejar el caso exitoso otra vez pendiente como estaba originalmente

-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id =8; -- admin 10, postulacion 8, alumno 8 , usuario autogestor 6. MODIFICAMOS postulacion PENDIENTE porque esta CONFIRMADA 
-- UPDATE Usuarios SET activo = FALSE WHERE id = 6; -- inactivamos a marcos autogestor
CALL sp_Aceptar_Postulacion_Alumno(10,8); -- autogestor inactivo
-- UPDATE PostulacionesAlumnos SET idEstado = 3 WHERE id =8; -- vuelvo a estado CONFIRMADO originalmente
-- UPDATE Usuarios SET activo = TRUE WHERE id = 6; -- activamos a marcos autogestor

-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id = 15; -- pasamos la postulacion aceptada a pendiente temporalmente
-- UPDATE Usuarios SET activo = FALSE WHERE id = 6;
CALL sp_Aceptar_Postulacion_Alumno(9,15); -- administrador 9 ,postulación 15, alumno 9, Responsable 6 Usuario 6
-- UPDATE PostulacionesAlumnos SET idEstado = 2 WHERE id = 15;
-- UPDATE Usuarios SET activo = TRUE WHERE id =6; -- dejo activo usuario 6 como estaba originalmente

-- sp Rechazar postulacion alumno
CALL sp_Rechazar_Postulacion_Alumno(13,9,NULL); -- valido motivo que no sea null 
CALL sp_Rechazar_Postulacion_Alumno(13,9,''); -- valido motivo que no esté vacío
CALL sp_Rechazar_Postulacion_Alumno(13,9,'      '); -- valido motivo con espacios
CALL sp_Rechazar_Postulacion_Alumno(13,9, REPEAT('A', 256)); -- valido que el motivo no supere los 255 caracteres disponibles.
CALL sp_Rechazar_Postulacion_Alumno(1002,9,'No adjunta foto de documento de identidad de ambas caras.'); -- valido que la postulacion exista
CALL sp_Rechazar_Postulacion_Alumno(13,1002,'No adjunta foto de documento de identidad de ambas caras.'); -- valido que administrador exista
CALL sp_Rechazar_Postulacion_Alumno(12,8,'No adjunta foto de documento de identidad de ambas caras.');-- valido que usuario este activo
-- UPDATE Instituciones SET estado = FALSE WHERE id = 9;
CALL sp_Rechazar_Postulacion_Alumno(13,9,'No adjunta foto de documento de identidad de ambas caras.'); -- Institución inactiva
-- UPDATE Instituciones SET estado = TRUE WHERE id = 9;
CALL sp_Rechazar_Postulacion_Alumno(13,1,'No adjunta foto de documento de identidad de ambas caras.'); -- Postulación no corresponde a la institución

-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id =8; -- admin 10, postulacion 8, alumno 8 , usuario autogestor 6. MODIFICAMOS postulacion PENDIENTE porque esta CONFIRMADA 
-- UPDATE Usuarios SET activo = FALSE WHERE id = 6; -- inactivamos a marcos autogestor
CALL sp_Rechazar_Postulacion_Alumno(8,10,'No adjunta foto de documento de identidad de ambas caras.'); -- autogestor inactivo
-- UPDATE PostulacionesAlumnos SET idEstado = 3 WHERE id =8; -- vuelvo a estado CONFIRMADO originalmente
-- UPDATE Usuarios SET activo = TRUE WHERE id = 6; -- activamos a marcos autogestor

-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id = 11; -- postulacion 11, alumno 11 (Martina), Responsable 8 (rosa), Usuario responsable 8 Ya esta inactivo, Admin institucion 7 PASAMOS POSTULACION a Pendiente  temporalmente 
CALL sp_Rechazar_Postulacion_Alumno(11,7,'No adjunta foto de documento de identidad de ambas caras.'); -- responsable no activo
-- UPDATE PostulacionesAlumnos SET idEstado = 3 WHERE id = 11; -- lo dejamos como estaba

CALL sp_Rechazar_Postulacion_Alumno(15,9,'No adjunta foto de documento de identidad de ambas caras.'); -- postulacion no pendiente
CALL sp_Rechazar_Postulacion_Alumno(13,9,'No adjunta foto de documento de identidad de ambas caras.'); -- caso exitoso 
-- UPDATE PostulacionesAlumnos SET idEstado = 1, motivoRechazo = NULL WHERE id =13; -- vuelvo a pendiente sin motivo rechazo 
-- SELECT id, idEstado, motivoRechazo FROM PostulacionesAlumnos WHERE id = 13; -- chequeo
-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id =13; 

-- sp_Confirmar_Postulacion_Alumno
CALL sp_Confirmar_Postulacion_Alumno(1002,6);-- postulacion no existe 
CALL sp_Confirmar_Postulacion_Alumno(15,1002);-- responsable inexistente
CALL sp_Confirmar_Postulacion_Alumno(15,8);-- responsable inactivo
CALL sp_Confirmar_Postulacion_Alumno(15,1);-- responsable que no corresponde al alumno
CALL sp_Confirmar_Postulacion_Alumno(12,9);-- postulacion pendiente - no esta aceptada

-- UPDATE Instituciones SET estado = FALSE WHERE id = 9;
CALL sp_Confirmar_Postulacion_Alumno(15,6);-- institución no activa
-- UPDATE Instituciones SET estado = TRUE WHERE id = 9;

-- UPDATE PostulacionesAlumnos SET idEstado = 2 WHERE id = 18; -- cambiamos estado a 2 (Aceptado) 
CALL sp_Confirmar_Postulacion_Alumno(18,6);-- curso sin cupo 
-- UPDATE PostulacionesAlumnos SET idEstado = 4 WHERE id = 18; -- volvemos a rechazado
 
CALL sp_Confirmar_Postulacion_Alumno(15,6);-- caso exitoso 
-- SELECT id, idAlumno, idCurso, idEstado FROM PostulacionesAlumnos WHERE id = 15;
-- SELECT id, capacidad, inscriptos FROM Cursos WHERE id = 13;
-- UPDATE PostulacionAlumnos SET idEstado = 2 WHERE id = 13;
CALL sp_Confirmar_Postulacion_Alumno(13,10);-- caso exitoso
-- SELECT * FROM PostulacionesAlumnos ;
-- UPDATE PostulacionesAlumnos SET idEstado = 1 WHERE id =13;
-- UPDATE Cursos SET inscriptos = 1 WHERE id = 13; 

  -- preparo mas insert para testear el sp_Registrar_Cursos
  
-- INSERTS: INSTITUCIONES DE TIPOS 5, 6, 7 Y 8
-- use instired;
-- Personas
INSERT INTO Personas
(id, nombre, apellido, documento, domicilio, fechaNacimiento)
VALUES
(34, 'Test', 'TipoCinco', '40000001', 'Calle Test 100', '1985-01-10'),
(35, 'Test', 'TipoSeis',  '40000002', 'Calle Test 200', '1985-02-10'),
(36, 'Test', 'TipoSiete', '40000003', 'Calle Test 300', '1985-03-10'),
(37, 'Test', 'TipoOcho',  '40000004', 'Calle Test 400', '1985-04-10');


-- Usuarios activos
INSERT INTO Usuarios
(id, clave, email, activo, idTipoUsuario)
VALUES
(25, 'test1234', 'test.tipo5@mail.test', TRUE, 2),
(26, 'test1234', 'test.tipo6@mail.test', TRUE, 2),
(27, 'test1234', 'test.tipo7@mail.test', TRUE, 2),
(28, 'test1234', 'test.tipo8@mail.test', TRUE, 2);


-- Instituciones activas
INSERT INTO Instituciones
(id, nombre, cuit, direccion, email, latitud, longitud,
 descripcion, idTipoInstitucion, estado)
VALUES
(11, 'Institución de prueba Tipo 5', '30710000129',
 'Calle Test 100', 'tipo5@instired.test', -34.60, -58.40,
 'Institución de prueba Jardín y Primaria.',
 5, TRUE),

(12, 'Institución de prueba Tipo 6', '30710000137',
 'Calle Test 200', 'tipo6@instired.test', -34.61, -58.41,
 'Institución de prueba Primaria y Secundaria.',
 6, TRUE),

(13, 'Institución de prueba Tipo 7', '30710000145',
 'Calle Test 300', 'tipo7@instired.test', -34.62, -58.42,
 'Institución de prueba Jardín, Primaria y Secundaria.',
 7, TRUE),

(14, 'Institución de prueba Tipo 8', '30710000153',
 'Calle Test 400', 'tipo8@instired.test', -34.63, -58.43,
 'Institución de prueba Jardín y Secundaria.',
 8, TRUE);


-- Administradores vinculados a las instituciones
INSERT INTO AdministradoresInstitucion
(id, idPersona, idUsuario, idInstitucion)
VALUES
(13, 34, 25, 11),
(14, 35, 26, 12),
(15, 36, 27, 13),
(16, 37, 28, 14);

-- ------ OTROS INSERTS NECESARIOS

-- Personas
INSERT INTO Personas
(id, nombre, apellido, documento, domicilio, fechaNacimiento)
VALUES
(38, 'Test', 'SinInstitucion', '40000005', 'Calle Test 500', '1985-05-10'),
(39, 'Test', 'InstitucionInactiva', '40000006', 'Calle Test 600', '1985-06-10'),
(40, 'Test', 'TipoInvalido', '40000007', 'Calle Test 700', '1985-07-10');


-- Usuarios activos
INSERT INTO Usuarios
(id, clave, email, activo, idTipoUsuario)
VALUES
(29, 'test1234', 'test.sininstitucion@mail.test', TRUE, 2),
(30, 'test1234', 'test.inactiva@mail.test', TRUE, 2),
(31, 'test1234', 'test.tipoinvalido@mail.test', TRUE, 2);


-- Agregamos temporalmente un tipo 9 para probar la validación del SP
INSERT INTO TipoInstitucion (id, tipoInstitucion)
VALUES (9, 'Tipo inválido de prueba');


-- Instituciones
INSERT INTO Instituciones
(id, nombre, cuit, direccion, email, latitud, longitud,
 descripcion, idTipoInstitucion, estado)
VALUES
(15, 'Institución inactiva de prueba', '30710000161',
 'Calle Test 600', 'inactiva@instired.test', -34.64, -58.44,
 'Institución creada para probar estado inactivo.',
 2, FALSE),

(16, 'Institución tipo inválido de prueba', '30710000170',
 'Calle Test 700', 'tipoinvalido@instired.test', -34.65, -58.45,
 'Institución creada para probar tipo inválido.',
 9, TRUE);


-- Administradores
INSERT INTO AdministradoresInstitucion
(id, idPersona, idUsuario, idInstitucion)
VALUES
(17, 38, 29, NULL),
(18, 39, 30, 15),
(19, 40, 31, 16);

-- Verificar que quedaron correctamente relacionados
SELECT
    ai.id AS idAdministrador,
    i.id AS idInstitucion,
    i.nombre,
    ti.tipoInstitucion,
    u.activo
FROM AdministradoresInstitucion ai
INNER JOIN Instituciones i ON i.id = ai.idInstitucion
INNER JOIN TipoInstitucion ti ON ti.id = i.idTipoInstitucion
INNER JOIN Usuarios u ON u.id = ai.idUsuario
WHERE ai.id BETWEEN 13 AND 16;

-- Validaciones sp_Registrar_Cursos 
	-- admin
CALL sp_Registrar_Curso(NULL, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_NULL', 20, 0); -- valido admin null
CALL sp_Registrar_Curso(0, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_CERO', 20, 0);-- administrador id 0
CALL sp_Registrar_Curso(-1, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_NEGATIVO', 20, 0);-- administrador id negativo
CALL sp_Registrar_Curso(999, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_INEXISTENTE', 20, 0); -- admin inexistente
CALL sp_Registrar_Curso(8, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_INACTIVO', 20, 0); -- usuario inactivo

-- instituciones
CALL sp_Registrar_Curso(17, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_ADMIN_SIN_INSTITUCION', 20, 0); -- admnin sin institucion asignada 
CALL sp_Registrar_Curso(19, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_TIPO_INSTITUCION_INVALIDO', 20, 0); -- Administrador 19: institución con tipo 9
CALL sp_Registrar_Curso(18, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_INSTITUCION_INACTIVA', 20, 0); -- Administrador 18: usuario activo, institución inactiva
CALL sp_Registrar_Curso(13, 1, YEAR(CURDATE())+1, NULL, NULL, 'Inglés Inicial', 'TEST_CUSTOM_TIPO5', 20, 0);-- Institución tipo 5 intenta utilizar nivel personalizado


    -- Turno 
CALL sp_Registrar_Curso(2, NULL, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_TURNO_NULL', 20, 0); -- turno null
CALL sp_Registrar_Curso(2, 0, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_TURNO_CERO', 20, 0); -- turno igual a 0
CALL sp_Registrar_Curso(2, -1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_TURNO_NEGATIVO', 20, 0); -- Turno negativo
CALL sp_Registrar_Curso(2, 999, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_TURNO_INEXISTENTE', 20, 0); -- Turno inexistente

	-- ciclo lectivo

CALL sp_Registrar_Curso(2, 1, NULL, 2, 1, NULL, 'TEST_CICLO_NULL', 20, 0); -- Ciclo NULL
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())-1, 2, 1, NULL, 'TEST_CICLO_ANTERIOR', 20, 0); -- Ciclo anterior al actual
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+2, 2, 1, NULL, 'TEST_CICLO_FUTURO', 20, 0); -- Ciclo dos años posterior al actual
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE()), 2, 1, NULL, 'TEST_PLAZO_CICLO_ACTUAL', 20, 0); -- para este año creando un curso de tipo normalizado para este momento no se puede. ciclo lectivo en curso.

	-- nivel personalizado y normaizado 
    
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_IDIOMAS_NIVEL_NORMALIZADO', 20, 0); -- Instituto de idiomas intenta registrar Primaria
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1,2, 1, NULL,'TEST_IDIOMAS_NIVEL_NORMALIZADO',20, 0); -- admin de instituto de idioma intentando registrar un normalizado
CALL sp_Registrar_Curso(1, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_T1_PRIMARIA', 20, 0); -- jardin intenta crear curso primaria
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_T2_PREESCOLAR', 20, 0); -- primaria intenta registrar un curso preescolar 
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1,3, 1, NULL, 'TEST_T2_SECUNDARIA', 20, 0); -- primaria intenta crear curso de secundaria 
CALL sp_Registrar_Curso(3, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_T3_PREESCOLAR', 20, 0); -- secundaria intenta crear curso de preescolar 
CALL sp_Registrar_Curso(3, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_T3_PRIMARIA', 20, 0);-- secundaria Intenta registrar Primaria
CALL sp_Registrar_Curso(13, 1, YEAR(CURDATE())+1,3, 1, NULL, 'TEST_T5_SECUNDARIA', 20, 0); -- jardin y primaria intenta registrar un curso de secundaria
CALL sp_Registrar_Curso(14, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_T6_PREESCOLAR', 20, 0); -- primaria y secundaria intenta registrar un preescolar 
CALL sp_Registrar_Curso(16, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_T8_PRIMARIA', 20, 0);-- preescolar y secundaria intenta crear curso primaria 



CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, 2, NULL, 'Inglés Inicial', 'TEST_CUSTOM_IDNIVEL', 20, 0); -- Nivel personalizado informado junto con idNivel
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, NULL, 1, 'Inglés Inicial', 'TEST_CUSTOM_ANIO', 20, 0); -- Nivel personalizado informado junto con año
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, NULL, NULL, '     ', 'TEST_CUSTOM_VACIO', 20, 0); -- Nivel personalizado compuesto solamente por espacios
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, NULL, 1, NULL, 'TEST_IDNIVEL_NULL', 20, 0); -- idNivel NULL
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 0, 1, NULL, 'TEST_IDNIVEL_CERO', 20, 0); -- idNivel igual a cero
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, -1, 1, NULL, 'TEST_IDNIVEL_NEGATIVO', 20, 0); -- idNivel negativo
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, NULL, NULL, 'TEST_ANIO_NULL', 20, 0); -- Año NULL
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 0, NULL, 'TEST_ANIO_CERO', 20, 0); -- Año igual a cero
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, -1, NULL, 'TEST_ANIO_NEGATIVO', 20, 0); -- Año negativo

 -- Rango de años por nivel ej. preescolar solo permite 3 4 y 5

CALL sp_Registrar_Curso(1, 1, YEAR(CURDATE())+1, 1, 2, NULL, 'TEST_PREESCOLAR_MENOR', 20, 0);-- Preescolar: año menor al mínimo (3)
CALL sp_Registrar_Curso(1, 1, YEAR(CURDATE())+1, 1, 6, NULL, 'TEST_PREESCOLAR_MAYOR', 20, 0);  -- Preescolar: año mayor al máximo (5)
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 8, NULL, 'TEST_PRIMARIA_MAYOR', 20, 0); -- Primaria: año mayor al máximo (7)
CALL sp_Registrar_Curso(3, 1, YEAR(CURDATE())+1, 3, 7, NULL, 'TEST_SECUNDARIA_MAYOR', 20, 0); -- Secundaria: año mayor al máximo (6)

  -- Longitudes de nivel y division 
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, NULL, NULL, 'Ingles para adultos nivel avanzado', 'TEST_CUSTOM_LARGO', 20, 0);   -- Nivel personalizado de más de 30 caracteres
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'Esta division tiene mas de cincuenta caracteres para probar', 20, 0); -- División de más de 50 caracteres

-- vaidaciones division 
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, NULL, 20, 0); 	-- División NULL
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, '', 20, 0); -- División vacía
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, '     ', 20, 0); -- División con espacios

-- validaciones de capacidad 
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_CAPACIDAD_NULL', NULL, 0); -- Capacidad null
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_CAPACIDAD_CERO', 0, 0); -- Capacidad igual a cero
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_CAPACIDAD_NEGATIVA', -5, 0); -- Capacidad negativa

-- validacion de inscriptos 
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_INSCRIPTOS_NEGATIVOS', 20, -1); -- Inscriptos negativos
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_INSCRIPTOS_SUPERIOR', 20, 21); -- Inscriptos superiores a capacidad

-- duplicidad de cursos, inserto un curso y tambien intento cargar el mismo por sp. NORMALIZADO
 -- Registro para probar duplicidad
INSERT INTO Cursos
(idInstitucion, idTurno, cicloLectivo, idNivel, anio,
 nivelPersonalizado, division, capacidad, inscriptos)
VALUES
(2, 1, YEAR(CURDATE())+1, 2, 1,
 NULL, 'TEST_DUP_NORMALIZADO', 20, 0);
 
CALL sp_Registrar_Curso(2, 1, YEAR(CURDATE())+1, 2, 1, NULL, 'TEST_DUP_NORMALIZADO', 20, 0); -- Intentamos registrar el mismo curso mediante el SP
 
 -- duplicidad de cursos, inserto un curso y tambien intento cargar el mismo por sp. PERSONALIZADO
 -- Registro para probar duplicidad
INSERT INTO Cursos
(idInstitucion, idTurno, cicloLectivo, idNivel, anio,
 nivelPersonalizado, division, capacidad, inscriptos)
VALUES
(4, 1, YEAR(CURDATE())+1, NULL, NULL,
 'Inglés Inicial', 'TEST_DUP_PERSONALIZADO', 20, 0);
CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1, NULL, NULL, 'ingles inicial', '  test_dup_personalizado  ', 20, 0); -- Mismo curso, con diferencias de mayúsculas, acentos y espacios

-- CASOS EXITO 
CALL sp_Registrar_Curso(13, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_OK_T5_PREESCOLAR', 20, 0);-- Tipo 5: Jardín-Primaria
CALL sp_Registrar_Curso(13, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_OK_T5_PRIMARIA', 20, 0);

CALL sp_Registrar_Curso(14, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_OK_T6_PRIMARIA', 20, 0);-- Tipo 6: Primaria-Secundaria
CALL sp_Registrar_Curso(14, 1, YEAR(CURDATE())+1, 3, 1, NULL, 'TEST_OK_T6_SECUNDARIA', 20, 0);

CALL sp_Registrar_Curso(15, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_OK_T7_PREESCOLAR', 20, 0);-- Tipo 7: Jardín-Primaria-Secundaria
CALL sp_Registrar_Curso(15, 1, YEAR(CURDATE())+1,2, 1, NULL, 'TEST_OK_T7_PRIMARIA', 20, 0);
CALL sp_Registrar_Curso(15, 1, YEAR(CURDATE())+1,3, 1, NULL, 'TEST_OK_T7_SECUNDARIA', 20, 0);
-- Tipo 8: Jardín-Secundaria
CALL sp_Registrar_Curso(16, 1, YEAR(CURDATE())+1,1, 3, NULL, 'TEST_OK_T8_PREESCOLAR', 20, 0);-- Tipo 8: Jardín-Secundaria
CALL sp_Registrar_Curso(16, 1, YEAR(CURDATE())+1,3, 1, NULL, 'TEST_OK_T8_SECUNDARIA', 20, 0);

CALL sp_Registrar_Curso(4, 1, YEAR(CURDATE())+1,NULL, NULL, 'Inglés Inicial','TEST_OK_IDIOMAS', 20, 0); -- ok curso en institutto de idiomas

-- cancelar postulacion 
 -- sp cancelar postulacion
 update PostulacionesAlumnos set idEstado = 4 where id = 27;
CALL sp_Cancelar_Postulacion_Alumno(27, 6); -- tiene estado rechazado falla
update PostulacionesAlumnos set idEstado = 5 where id = 27;
CALL sp_Cancelar_Postulacion_Alumno(27, 6); -- tiene estado cancelado falla
 update PostulacionesAlumnos set idEstado = 1 where id = 27;
select * from PostulacionesAlumnos where id = 27;
select * from Cursos Where id = 20;

CALL sp_Aceptar_Postulacion_Alumno(14, 27);
CALL sp_Confirmar_Postulacion_Alumno(27, 6);


UPDATE Cursos
SET cicloLectivo = 2025
WHERE id = 18;
UPDATE PostulacionesAlumnos
SET idEstado = 1
WHERE id = 26;
CALL sp_Aceptar_Postulacion_Alumno(13, 26);
CALL sp_Confirmar_Postulacion_Alumno(26, 1);
CALL sp_Cancelar_Postulacion_Alumno(26, 1);

UPDATE PostulacionesAlumnos
SET idEstado = 2
WHERE id = 26;
