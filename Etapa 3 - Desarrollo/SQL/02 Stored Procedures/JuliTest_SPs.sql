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
