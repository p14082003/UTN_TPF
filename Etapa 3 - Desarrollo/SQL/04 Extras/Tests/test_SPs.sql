-- PRUEBAS MANUALES DE STORED PROCEDURES
-- HISTORICO: estos bloques usan IDs de insert_tables_data_viejo.sql.
-- Para la carga nueva, ejecutar verificar_seed.sql y seguir Seed/LEEME_seed.txt.
-- No ejecutar este archivo completo sobre el seed nuevo.
-- Requiere tablas, datos de prueba, procedimientos y vistas creados.
-- Ejecutar por bloques: incluye casos que deben fallar e IDs de los datos de prueba.
-- Los casos de autogestor se encuentran en test_alumno_autogestor.sql, en esta misma carpeta.
USE InstiRed_DEV;

-- =====================================================================
-- REGISTRO DE RESPONSABLES Y ALUMNOS
-- =====================================================================

CALL sp_Registrar_Responsable('test1234', 'mlopez@mail.test', 'Mariana', 'Lopez', '40111222', 'Calle Falsa 123, Tigre', '1992-03-15', '+54 9 11 5555-2000');
Select * from Usuarios where id = 25;
Select * from Personas where id = 34;
select * from Telefonos where idResponsable = 11;
select * from Responsables where idPersona = 34;

CALL sp_Registrar_Alumno(11, 2, 'Sofia', 'Lopez', '55111222', 'Calle Falsa 123, Tigre', '2018-05-10');
Select * from Alumnos where id = 14;
Select * from Personas where id = 35;
Select * from ResponsablesPorAlumno where idAlumno = 14;
--
-- Casos que deberian fallar:
-- CALL sp_Registrar_Responsable('x', 'mlopez@mail.test', 'Otra', 'Persona', '40999888', 'Calle 1', '1990-01-01', NULL); -- email repetido
-- CALL sp_Registrar_Responsable('x', 'menor@mail.test', 'Joven', 'Menor', '45000111', 'Calle 1', '2015-01-01', NULL);       -- menor de edad
-- CALL sp_Registrar_Responsable('x', 'sofia2@mail.test', 'Sofia', 'Benitez', '57412086', 'Calle 1', '1990-01-01', NULL);   -- DNI de una menor ya cargada: usa su fecha guardada, no la recibida
-- CALL sp_Registrar_Alumno(999, 2, 'Test', 'Test', '55000111', 'Calle 1', '2018-01-01');                        -- responsable inexistente
-- CALL sp_Registrar_Alumno(11, 9, 'Test', 'Test', '55000111', 'Calle 1', '2018-01-01');                         -- rol invalido
-- CALL sp_Registrar_Alumno(11, 2, 'Sofia', 'Lopez', '55111222', 'Calle 1', '2018-05-10');                       -- DNI ya registrado (el alumno ya tiene su unico responsable)
-- CALL sp_Registrar_Responsable('x', 'cero@mail.test', 'Luis', 'R', '01234567', 'Calle 1', '1990-01-01', NULL);             -- el documento no puede empezar con 0
-- CALL sp_Registrar_Responsable('x', 'largo@mail.test', 'Luis', 'R', '123456789', 'Calle 1', '1990-01-01', NULL);          -- el documento no puede tener mas de 8 caracteres
-- CALL sp_Registrar_Responsable('x', 'vacio@mail.test', '   ', 'R', '40555666', 'Calle 1', '1990-01-01', NULL);            -- nombre obligatorio (persona nueva)
--
--
-- Autogestor (la misma Persona que el responsable): crea el Alumno sin fila en ResponsablesPorAlumno
-- CALL sp_Registrar_Alumno(11, NULL, 'Mariana', 'Lopez', '40111222', 'Calle Falsa 123, Tigre', '1992-03-15');
-- Si ese DNI ya es alumno, falla con 'Ya existe un alumno registrado con ese documento.'
--
-- SELECT * FROM Responsables;
-- SELECT * FROM Alumnos;
-- SELECT * FROM ResponsablesPorAlumno;

-- Posible vw? nosotros tenemos anotado para hacer alumnos por responsable, entonces en create_vws hice eso. Acá, esta seria responsables por alumno, es decir, al reves.
SELECT a.id                         AS idAlumno,
       pa.nombre                    AS nombreAlumno,
       pa.apellido                  AS apellidoAlumno,
       pa.documento                 AS docAlumno,
       pa.fechaNacimiento,
       CASE WHEN rpa.id IS NULL THEN 'Autogestor' ELSE ro.descripcion END AS rol,
       r.id                         AS idResponsable,
       pr.nombre                    AS nombreResponsable,
       pr.apellido                  AS apellidoResponsable,
       pr.documento                 AS docResponsable,
       u.email                      AS emailResponsable,
       c.id                         AS idCurso,
       i.nombre                     AS institucion
FROM Alumnos a
JOIN Personas pa                  ON pa.id = a.idPersona
LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idAlumno = a.id
LEFT JOIN Roles ro                ON ro.id = rpa.idRol
LEFT JOIN Responsables r          ON r.id = rpa.idResponsable
LEFT JOIN Personas pr             ON pr.id = r.idPersona
LEFT JOIN Usuarios u              ON u.id = r.idUsuario
LEFT JOIN PostulacionesAlumnos po ON po.idAlumno = a.id
                                 AND po.idEstado = (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Confirmado')
LEFT JOIN Cursos c                ON c.id = po.idCurso
LEFT JOIN Instituciones i         ON i.id = c.idInstitucion
ORDER BY a.id;

-- =====================================================================
-- ADMINISTRADORES, INSTITUCIONES Y DATOS DE CONTACTO
-- =====================================================================

-- TEST sp_AdministradorInstitucion_Registrar
-- Orden de parametros: nombre, apellido, documento, domicilio, fechaNacimiento,
-- clave, email, telefono, nombreInstitucion, cuit, constanciaFiscal, habilitacion,
-- constanciaLaboral, codigoSeguimiento, OUT idPostulacion.

-- Caso OK:
CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'clave123', 'TESTadmin@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0001', @idPost);

SELECT @idPost;
SELECT * FROM PostulacionesAdminInstitucion WHERE id = @idPost;                -- Pendiente
SELECT * FROM Usuarios WHERE email = 'testadmin@mail.com';                     -- activo = 0, email en minúscula
SELECT * FROM AdministradoresInstitucion
WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'); -- idInstitucion NULL

-- Deberian fallar (se corre de a uno; cada uno repite UN solo dato del caso feliz):

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121213', 'Calle 123', '1990-05-10',
  'clave123', 'testadmin@mail.com', '121212',
  'Instituto fest', '30999999993', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0002', @idPost); */ -- ERROR EMAIL EN USO

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999993', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0002', @idPost); */ -- ERROR DOCUMENTO EN USO (ya es administrador) -> responde 'Ya tenés una postulación pendiente'

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121213', 'Calle 123', '1990-05-10',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0002', @idPost); */ -- ERROR CUIT EN USO

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121213', 'Calle 123', '2015-01-01',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999993', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0002', @idPost); */ -- ERROR MAYOR DE EDAD

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121213', 'Calle 123', '1990-05-10',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999993', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0001', @idPost); */ -- ERROR CODIGO EN USO (salta el handler 1062: no debe quedar la persona ni el usuario nuevos)

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121213', 'Calle 123', '1990-05-10',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999993', 'fest.pdf', 'habilitacion_fest.pdf', '',
  'PAI-TEST-0002', @idPost); */ -- ERROR FALTAN DATOS (constancia laboral vacia)

-- =====================================================================
-- TEST sp_PostulacionAdmin_Aprobar / sp_PostulacionAdmin_Rechazar
-- Usan el usuario DevCo 23 (devco.admin@devco.test, activo) del archivo de INSERTs.
-- Parametros: (idPostulacion, idUsuarioDevCo [, motivoRechazo])
-- =====================================================================

-- RECHAZAR sobre la postulacion creada arriba (@idPost, queda Pendiente):
CALL sp_PostulacionAdmin_Rechazar(@idPost, 23, 'La constancia laboral no es legible.');
SELECT id, idEstado, motivoRechazo FROM PostulacionesAdminInstitucion WHERE id = @idPost;   -- estado Rechazado + motivo
SELECT activo FROM Usuarios WHERE email = 'testadmin@mail.com';                           -- sigue en 0

-- Deberian fallar (se corre de a uno):
/* CALL sp_PostulacionAdmin_Rechazar(@idPost, 23, 'Otro motivo'); */                      -- ERROR YA FUE RESUELTA
/* CALL sp_PostulacionAdmin_Rechazar(@idPost, 23, '   '); */                              -- ERROR MOTIVO OBLIGATORIO
/* CALL sp_PostulacionAdmin_Rechazar(999999, 23, 'Motivo'); */                            -- ERROR NO EXISTE
/* CALL sp_PostulacionAdmin_Rechazar(@idPost, 11, 'Motivo'); */                           -- ERROR SIN PERMISO (usuario 11 es administrador de institucion)

-- REINTENTO tras un rechazo: mismo documento y mismo email, datos nuevos, codigo nuevo.
-- La clave que llega se ignora (la API ya la verifico) y se conserva la guardada.
CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'AdminNuevo', '12121212', 'Calle Nueva 999', '1990-05-11',
  'otraClave', 'testadmin@mail.com', '555555',
  'Instituto fest', '30999999992', 'fest_b.pdf', 'habilitacion_fest_b.pdf', 'laboral_fest_b.pdf',
  'PAI-TEST-0004', @idPost3);

SELECT @idPost3;
SELECT id, codigoSeguimiento, idEstado, motivoRechazo FROM PostulacionesAdminInstitucion
WHERE idAdministradorInstitucion = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'));   -- 2 filas: la rechazada y la nueva Pendiente
SELECT apellido, domicilio, fechaNacimiento FROM Personas WHERE documento = '12121212';      -- AdminNuevo, Calle Nueva 999, 1990-05-11
SELECT telefono FROM Telefonos WHERE idAdministrador = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'));   -- 1 fila: 555555
SELECT id, email, clave, activo FROM Usuarios WHERE email = 'testadmin@mail.com';  -- 1 usuario, clave123, activo 0
-- Deberia fallar: ahora tiene una postulacion Pendiente
/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'clave123', 'testadmin@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0005', @idPost); */ -- ERROR YA TIENE UNA POSTULACION PENDIENTE

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'clave123', 'otro@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'habilitacion_fest.pdf', 'laboral_fest.pdf',
  'PAI-TEST-0005', @idPost); */ -- ERROR 'Usá el email con el que te postulaste anteriormente' (reintento con otro email)

-- Se vuelve a rechazar para dejarlo en estado "rechazado" (los tests de Crear lo usan inactivo)
CALL sp_PostulacionAdmin_Rechazar(@idPost3, 23, 'Documentacion incompleta.');

-- REINTENTO con telefono compartido: el unico telefono del administrador pasa a ser tambien del responsable 1.
-- El reintento no debe cambiarle el numero al responsable: le saca el administrador a esa fila y crea un
-- telefono nuevo, exclusivo del administrador.
SET @idTelAdm = (SELECT id FROM Telefonos WHERE idAdministrador = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com')) LIMIT 1);
UPDATE Telefonos SET idResponsable = 1 WHERE id = @idTelAdm;
CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'AdminNuevo', '12121212', 'Calle Nueva 999', '1990-05-11',
  'otraClave', 'testadmin@mail.com', '777777',
  'Instituto fest', '30999999992', 'fest_c.pdf', 'habilitacion_fest_c.pdf', 'laboral_fest_c.pdf',
  'PAI-TEST-0007', @idPost4);
SELECT id, telefono, idResponsable FROM Telefonos WHERE idAdministrador = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'));   -- 1 fila: '777777', sin responsable
SELECT id, telefono, idAdministrador, idResponsable FROM Telefonos WHERE id = @idTelAdm;   -- la fila compartida sigue existiendo, sin cambios en el numero: '555555', idAdministrador NULL, idResponsable 1
CALL sp_PostulacionAdmin_Rechazar(@idPost4, 23, 'Documentacion incompleta.');   -- queda rechazado de nuevo para los tests que siguen

-- OBTENER CLAVE PARA REINTENTO
CALL sp_AdministradorInstitucion_ObtenerClaveReintento('TESTadmin@mail.com');   -- 1 fila: la clave guardada (rechazado, inactivo, sin institucion)
/* CASOS DONDE NO DEVUELVE EL HASH DE LA CLAVE (0 FILAS) */
CALL sp_AdministradorInstitucion_ObtenerClaveReintento('devco.admin@devco.test'); -- 0 filas (no es administrador de institucion)
CALL sp_AdministradorInstitucion_ObtenerClaveReintento('noexiste@mail.com');      -- 0 filas
/* CALL sp_AdministradorInstitucion_ObtenerClaveReintento('  '); */                -- ERROR FALTAN DATOS

-- CONSULTAR POSTULACION POR CODIGO. Orden: codigo, email
-- Administrador (usa las postulaciones de prueba rechazadas):
CALL sp_Postulacion_ConsultarPorCodigo('PAI-TEST-0004', 'TESTadmin@mail.com');   -- 1 fila: Administrador, Rechazado, motivo 'Documentacion incompleta.'
CALL sp_Postulacion_ConsultarPorCodigo('PAI-TEST-0001', 'testadmin@mail.com');   -- 1 fila: la primera postulacion, Rechazado, motivo 'La constancia laboral no es legible.'
/* CALL sp_Postulacion_ConsultarPorCodigo('PAI-TEST-0004', 'otro@mail.com'); */          -- ERROR NO SE ENCONTRO (email que no corresponde)
/* CALL sp_Postulacion_ConsultarPorCodigo('PAI-NOEXISTE', 'testadmin@mail.com'); */      -- ERROR NO SE ENCONTRO (codigo inexistente, MISMO mensaje)
/* CALL sp_Postulacion_ConsultarPorCodigo('PAI-TEST-0004', '  '); */                     -- ERROR FALTAN DATOS

-- Alumno (usa una postulacion del archivo de INSERTs; el email es el del responsable):
SET @emailResp = (SELECT emailResponsable FROM vw_Postulaciones_Alumnos WHERE codigoSeguimiento = 'PST-20251112-0001');
CALL sp_Postulacion_ConsultarPorCodigo('PST-20251112-0001', @emailResp);          -- 1 fila: Alumno, Confirmado, institucion y CUIT
/* CALL sp_Postulacion_ConsultarPorCodigo('PST-20251112-0001', 'testadmin@mail.com'); */ -- ERROR NO SE ENCONTRO (email de otra persona)


-- APROBAR: se registra otro administrador de prueba para tener una postulacion Pendiente.
CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin2', '12121214', 'Calle 456', '1990-05-10',
  'clave123', 'testadmin2@mail.com', '121214',
  'Instituto fest 2', '30999999994', 'fest2.pdf', 'habilitacion_fest2.pdf', 'laboral_fest2.pdf',
  'PAI-TEST-0003', @idPost2);

SELECT activo FROM Usuarios WHERE email = 'testadmin2@mail.com';                          -- 0 antes de aprobar
CALL sp_PostulacionAdmin_Aprobar(@idPost2, 23);
SELECT id, idEstado, motivoRechazo FROM PostulacionesAdminInstitucion WHERE id = @idPost2;  -- estado Confirmado, motivo NULL
SELECT activo FROM Usuarios WHERE email = 'testadmin2@mail.com';                          -- 1 despues de aprobar

CALL sp_AdministradorInstitucion_ObtenerClaveReintento('testadmin2@mail.com');     -- 0 filas (cuenta activa y con postulacion Confirmada)

-- Deberian fallar (se corre de a uno):
/* CALL sp_PostulacionAdmin_Aprobar(@idPost2, 23); */                                     -- ERROR YA FUE RESUELTA
/* CALL sp_PostulacionAdmin_Aprobar(999999, 23); */                                       -- ERROR NO EXISTE
/* CALL sp_PostulacionAdmin_Aprobar(@idPost2, 11); */                                     -- ERROR SIN PERMISO
/* CALL sp_PostulacionAdmin_Aprobar(NULL, 23); */                                         -- ERROR FALTAN DATOS

-- Reintento de una cuenta ya aprobada (ultima postulacion Confirmado):
/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin2', '12121214', 'Calle 456', '1990-05-10',
  'clave123', 'testadmin2@mail.com', '121214',
  'Instituto fest 2', '30999999994', 'fest2.pdf', 'habilitacion_fest2.pdf', 'laboral_fest2.pdf',
  'PAI-TEST-0006', @idPost); */ -- ERROR YA EXISTE UNA CUENTA DE ADMINISTRADOR CON ESE DOCUMENTO

-- =====================================================================
-- TEST sp_Institucion_Crear y los SP _Agregar
-- Usa el administrador de prueba testadmin2 (ya aprobado y activo arriba)
-- y el administrador testadmin (rechazado, usuario inactivo).
-- =====================================================================
SET @idAdmin1 = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'));
SET @idAdmin2 = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE email = 'testadmin2@mail.com'));

-- CREAR (caso OK). Orden: admin, direccion, email, lat, lon, descripcion, tipo,
-- movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo, OUT idInstitucion
CALL sp_Institucion_Crear(@idAdmin2, 'Av. Test 123, Tigre', 'Contacto@Fest.com', -34.4264, -58.5796,
  'Institucion de prueba.', 1, 1, 0, 0, 0, 0, 0, 0, @idInst);

SELECT @idInst;
SELECT * FROM Instituciones WHERE id = @idInst;                     -- nombre y CUIT de la postulacion, email en minuscula, estado = 1
SELECT * FROM Discapacidades WHERE idInstitucion = @idInst;         -- solo movilidad = 1
SELECT id, idInstitucion FROM AdministradoresInstitucion WHERE id = @idAdmin2;   -- idInstitucion = @idInst

-- Deberian fallar (se corre de a uno):
/* CALL sp_Institucion_Crear(@idAdmin2, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */   -- ERROR YA TIENE INSTITUCION
/* CALL sp_Institucion_Crear(@idAdmin1, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */   -- ERROR CUENTA NO HABILITADA (administrador rechazado)
/* CALL sp_Institucion_Crear(999999, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */       -- ERROR ADMINISTRADOR NO EXISTE
/* CALL sp_Institucion_Crear(@idAdmin2, '', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */         -- ERROR FALTAN DATOS
/* CALL sp_Institucion_Crear(@idAdmin2, 'Otra 1', 'sin-arroba', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */ -- ERROR EMAIL
/* CALL sp_Institucion_Crear(@idAdmin2, 'Otra 1', 'a@b.com', 100, -58.5, 'x', 1, 0,0,0,0,0,0,0, @idX); */     -- ERROR COORDENADAS
/* CALL sp_Institucion_Crear(@idAdmin2, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 99, 0,0,0,0,0,0,0, @idX); */  -- ERROR TIPO NO EXISTE

-- =====================================================================
-- TELEFONOS (general): sp_Telefono_Agregar y sp_Telefono_Eliminar, juntos
-- Usa @idInst y @idAdmin1/@idAdmin2 de arriba
-- =====================================================================
-- AGREGAR TELEFONO (general). Orden: usuario que actua (idUsuario), tipo de dueño, id del dueño, telefono
-- Tipos: 'Institucion' (id de la institucion), 'Administrador' (id del administrador), 'Responsable' (id del responsable)
SET @idUsuarioAdmin1 = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = @idAdmin1);   -- testadmin (rechazado, inactivo)
SET @idUsuarioAdmin2 = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = @idAdmin2);   -- testadmin2 (activo, de @idInst)
SET @idUsuarioResp2  = (SELECT idUsuario FROM Responsables WHERE id = 2);                         -- responsable 2 del archivo de INSERTs (1 telefono)

-- Institucion (puede el administrador activo de esa institucion)
CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', @idInst, '+54 11 4000-0000');
CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', @idInst, '+54 11 4000-0001');
SELECT * FROM Telefonos WHERE idInstitucion = @idInst;              -- 2 filas
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', @idInst, '+54 11 4000-0000'); */   -- ERROR YA TIENE ESE TELEFONO
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', @idInst, '+54 11 4000-0002'); */   -- ERROR YA TIENE EL MAXIMO DE 2 TELEFONOS
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', @idInst, '  '); */                 -- ERROR FALTAN DATOS
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin1, 'Institucion', @idInst, '1111'); */               -- ERROR SIN PERMISO (otro administrador no aceptado)
/* CALL sp_Telefono_Agregar(23, 'Institucion', @idInst, '1111'); */                             -- ERROR SIN PERMISO (Dev&Co no edita telefonos)
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Institucion', 999999, '1111'); */                -- ERROR INSTITUCION NO EXISTE
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Colegio', @idInst, '1111'); */                   -- ERROR TIPO DE DUEÑO NO VALIDO

-- Administrador (solo el propio administrador). testadmin2 ya tiene el telefono de su registro: queda con 2
CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Administrador', @idAdmin2, '1166660001');
SELECT * FROM Telefonos WHERE idAdministrador = @idAdmin2;          -- 2 filas ('121214' y '1166660001')
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Administrador', @idAdmin2, '1166660009'); */     -- ERROR YA TIENE EL MAXIMO DE 2 TELEFONOS
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Administrador', @idAdmin1, '1166660009'); */     -- ERROR SIN PERMISO (telefonos de otro administrador)
/* CALL sp_Telefono_Agregar(23, 'Administrador', @idAdmin2, '1166660009'); */                   -- ERROR SIN PERMISO (Dev&Co)

-- Responsable (solo el propio responsable). El responsable 2 ya tiene 1 telefono: queda con 2
CALL sp_Telefono_Agregar(@idUsuarioResp2, 'Responsable', 2, '1166660002');
SELECT * FROM Telefonos WHERE idResponsable = 2;                    -- 2 filas
/* CALL sp_Telefono_Agregar(@idUsuarioResp2, 'Responsable', 2, '1166660009'); */                -- ERROR YA TIENE EL MAXIMO DE 2 TELEFONOS
/* CALL sp_Telefono_Agregar(@idUsuarioResp2, 'Responsable', 1, '1166660009'); */                -- ERROR SIN PERMISO (telefonos de otro responsable)
/* CALL sp_Telefono_Agregar(@idUsuarioAdmin2, 'Responsable', 2, '1166660009'); */               -- ERROR SIN PERMISO (un administrador no edita telefonos de un responsable)

-- ELIMINAR TELEFONO (general). Orden: usuario que actua (idUsuario), tipo de dueño, id del dueño, id del telefono
SET @idTel = (SELECT id FROM Telefonos WHERE idInstitucion = @idInst AND telefono = '+54 11 4000-0000');
SET @idTelAdm2 = (SELECT id FROM Telefonos WHERE idAdministrador = @idAdmin2 AND telefono = '1166660001');
SET @idTelResp2 = (SELECT id FROM Telefonos WHERE idResponsable = 2 AND telefono = '1166660002');
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin1, 'Institucion', @idInst, @idTel); */          -- ERROR SIN PERMISO
/* CALL sp_Telefono_Eliminar(23, 'Institucion', @idInst, @idTel); */                         -- ERROR SIN PERMISO (Dev&Co)
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, 999999); */           -- ERROR NO TIENE ESE TELEFONO
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, @idTelAdm2); */       -- ERROR NO TIENE ESE TELEFONO (es de un administrador, no de la institucion)
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Colegio', @idInst, @idTel); */               -- ERROR TIPO DE DUEÑO NO VALIDO
CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, @idTel);
SELECT * FROM Telefonos WHERE idInstitucion = @idInst;              -- 1 fila ('+54 11 4000-0001')
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, @idTel); */           -- ERROR NO TIENE ESE TELEFONO (ya se quito)

-- ELIMINAR TELEFONO COMPARTIDO: un telefono de la institucion que tambien es del administrador no se borra,
-- solo se le saca la institucion (el administrador lo conserva).
INSERT INTO Telefonos (idInstitucion, idAdministrador, telefono) VALUES (@idInst, @idAdmin2, '+54 11 4111-1111');
SET @idTelComp = LAST_INSERT_ID();
CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, @idTelComp);
SELECT id, idInstitucion, idAdministrador FROM Telefonos WHERE id = @idTelComp;   -- 1 fila: idInstitucion NULL, idAdministrador = @idAdmin2 (sigue existiendo)
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Institucion', @idInst, @idTelComp); */      -- ERROR NO TIENE ESE TELEFONO (ya no es de la institucion)

-- Ahora el administrador lo quita: como ya no tiene otro dueño, se borra
CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Administrador', @idAdmin2, @idTelComp);
SELECT * FROM Telefonos WHERE id = @idTelComp;                      -- 0 filas

-- Administrador y responsable quitan el telefono que agregaron
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Administrador', @idAdmin1, @idTelAdm2); */  -- ERROR SIN PERMISO (telefonos de otro administrador)
CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Administrador', @idAdmin2, @idTelAdm2);
SELECT * FROM Telefonos WHERE idAdministrador = @idAdmin2;          -- 1 fila ('121214')

/* CALL sp_Telefono_Eliminar(@idUsuarioResp2, 'Responsable', 2, 11); */                      -- ERROR NO TIENE ESE TELEFONO (el 11 es del responsable 1)
/* CALL sp_Telefono_Eliminar(@idUsuarioAdmin2, 'Responsable', 2, @idTelResp2); */            -- ERROR SIN PERMISO (un administrador no edita telefonos de un responsable)
CALL sp_Telefono_Eliminar(@idUsuarioResp2, 'Responsable', 2, @idTelResp2);
SELECT * FROM Telefonos WHERE idResponsable = 2;                    -- 1 fila (la original del archivo de INSERTs)

-- AGREGAR IDIOMA. Orden: admin, institucion, idioma
CALL sp_Idioma_Institucion_Agregar(@idAdmin2, @idInst, 2);
SELECT * FROM IdiomaInstitucion WHERE idInstitucion = @idInst;      -- 1 fila (idioma 2)
/* CALL sp_Idioma_Institucion_Agregar(@idAdmin2, @idInst, 2); */      -- ERROR YA TIENE ESE IDIOMA
/* CALL sp_Idioma_Institucion_Agregar(@idAdmin2, @idInst, 99); */     -- ERROR IDIOMA NO EXISTE
/* CALL sp_Idioma_Institucion_Agregar(@idAdmin1, @idInst, 3); */      -- ERROR SIN PERMISO (admin 1 no aceptado)

-- AGREGAR IMAGEN. Orden: admin, institucion, ruta, descripcion
CALL sp_Imagen_Institucion_Agregar(@idAdmin2, @idInst, 'img/fest/frente.jpg', 'Frente de la institución');
SELECT * FROM Imagenes WHERE idInstitucion = @idInst;               -- 1 fila
/* CALL sp_Imagen_Institucion_Agregar(@idAdmin2, @idInst, 'img/fest/patio.jpg', ''); */   -- ERROR FALTAN DATOS (descripcion obligatoria)
/* CALL sp_Imagen_Institucion_Agregar(@idAdmin1, @idInst, 'img/x.jpg', 'x'); */           -- ERROR SIN PERMISO

-- MODIFICAR. Orden: admin, institucion, direccion, email, lat, lon, descripcion, tipo,
-- movilidad, visual, auditiva, verbal, intelectual, sindromeDown, autismo
CALL sp_Institucion_Modificar(@idAdmin2, @idInst, 'Av. Nueva 456, Tigre', 'Nuevo@Fest.com', -34.43, -58.58,
  'Descripcion modificada.', 2, 0, 1, 1, 0, 0, 0, 0);
SELECT * FROM Instituciones WHERE id = @idInst;                     -- direccion, email (minuscula), coordenadas, descripcion y tipo 2 nuevos; nombre y CUIT iguales
SELECT * FROM Discapacidades WHERE idInstitucion = @idInst;         -- ahora visual = 1 y auditiva = 1, movilidad = 0

-- Deberian fallar (se corre de a uno):
/* CALL sp_Institucion_Modificar(@idAdmin2, @idInst, '', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0); */          -- ERROR FALTAN DATOS
/* CALL sp_Institucion_Modificar(@idAdmin2, @idInst, 'Otra 1', 'sin-arroba', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0); */ -- ERROR EMAIL
/* CALL sp_Institucion_Modificar(@idAdmin2, @idInst, 'Otra 1', 'a@b.com', 100, -58.5, 'x', 1, 0,0,0,0,0,0,0); */     -- ERROR COORDENADAS
/* CALL sp_Institucion_Modificar(@idAdmin2, @idInst, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 99, 0,0,0,0,0,0,0); */  -- ERROR TIPO NO EXISTE
/* CALL sp_Institucion_Modificar(@idAdmin1, @idInst, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0); */   -- ERROR SIN PERMISO
/* CALL sp_Institucion_Modificar(@idAdmin2, 999999, 'Otra 1', 'a@b.com', -34.4, -58.5, 'x', 1, 0,0,0,0,0,0,0); */    -- ERROR INSTITUCION NO EXISTE

-- ELIMINAR IDIOMA. Orden: admin, institucion, idioma
/* CALL sp_Idioma_Institucion_Eliminar(@idAdmin1, @idInst, 2); */          -- ERROR SIN PERMISO
/* CALL sp_Idioma_Institucion_Eliminar(@idAdmin2, @idInst, 3); */          -- ERROR NO TIENE ESE IDIOMA
CALL sp_Idioma_Institucion_Eliminar(@idAdmin2, @idInst, 2);
SELECT * FROM IdiomaInstitucion WHERE idInstitucion = @idInst;      -- 0 filas

-- ELIMINAR IMAGEN. Orden: admin, institucion, id de la imagen
SET @idImg = (SELECT id FROM Imagenes WHERE idInstitucion = @idInst LIMIT 1);
/* CALL sp_Imagen_Institucion_Eliminar(@idAdmin1, @idInst, @idImg); */     -- ERROR SIN PERMISO
/* CALL sp_Imagen_Institucion_Eliminar(@idAdmin2, @idInst, 999999); */     -- ERROR NO TIENE ESA IMAGEN
CALL sp_Imagen_Institucion_Eliminar(@idAdmin2, @idInst, @idImg);
SELECT * FROM Imagenes WHERE idInstitucion = @idInst;               -- 0 filas

-- BAJA / REACTIVAR INSTITUCION. Orden: institucion, usuario Dev&Co (23)
-- Como todavia no hay SP de cursos, se arma un curso con tres postulaciones por INSERT directo:
-- una Pendiente, una Confirmada y una Aceptada.
INSERT INTO Cursos (idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos)
VALUES (@idInst, 1, 2026, 1, NULL, 'Sala test', 'T', 10, 0);
SET @idCursoTest = LAST_INSERT_ID();
INSERT INTO PostulacionesAlumnos (fecha, codigoSeguimiento, idCurso, idAlumno, idEstado, motivoRechazo)
VALUES (CURDATE(), 'PST-TEST-0001', @idCursoTest, 1, (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Pendiente'), NULL);
INSERT INTO PostulacionesAlumnos (fecha, codigoSeguimiento, idCurso, idAlumno, idEstado, motivoRechazo)
VALUES (CURDATE(), 'PST-TEST-0002', @idCursoTest, 2, (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Confirmado'), NULL);
INSERT INTO PostulacionesAlumnos (fecha, codigoSeguimiento, idCurso, idAlumno, idEstado, motivoRechazo)
VALUES (CURDATE(), 'PST-TEST-0003', @idCursoTest, 3, (SELECT id FROM EstadosPostulacion WHERE descripcion = 'Aceptado'), NULL);

-- Deberian fallar (se corre de a uno):
/* CALL sp_Institucion_DarDeBaja(@idInst, 11); */          -- ERROR SIN PERMISO (usuario 11 es administrador de institucion)
/* CALL sp_Institucion_DarDeBaja(999999, 23); */           -- ERROR NO EXISTE
/* CALL sp_Institucion_DarDeBaja(NULL, 23); */             -- ERROR FALTAN DATOS

CALL sp_Institucion_DarDeBaja(@idInst, 23);
SELECT id, estado FROM Instituciones WHERE id = @idInst;                      -- estado = 0
SELECT codigoSeguimiento, idEstado, motivoRechazo FROM PostulacionesAlumnos
WHERE codigoSeguimiento LIKE 'PST-TEST-%';                                    -- 0001 y 0003 Rechazado + motivo 'Institución dada de baja.'; 0002 sigue Confirmado, sin motivo
/* CALL sp_Institucion_DarDeBaja(@idInst, 23); */          -- ERROR YA ESTA DADA DE BAJA

/* CALL sp_Institucion_Reactivar(@idInst, 11); */          -- ERROR SIN PERMISO
CALL sp_Institucion_Reactivar(@idInst, 23);
SELECT id, estado FROM Instituciones WHERE id = @idInst;                      -- estado = 1
SELECT codigoSeguimiento, idEstado FROM PostulacionesAlumnos
WHERE codigoSeguimiento LIKE 'PST-TEST-%';                                    -- 0001 y 0003 siguen Rechazado (no se restauran)
/* CALL sp_Institucion_Reactivar(@idInst, 23); */          -- ERROR YA ESTA ACTIVA
/* CALL sp_Institucion_Reactivar(999999, 23); */           -- ERROR NO EXISTE


-- BAJAS Y REACTIVACIONES
SET @idUsuario1 = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = @idAdmin1);   -- testadmin (rechazado, inactivo, sin institucion)
SET @idUsuario2 = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = @idAdmin2);   -- testadmin2 (activo, de @idInst)
SET @idUsuarioOtraInst = (SELECT idUsuario FROM AdministradoresInstitucion
                          WHERE idInstitucion IS NOT NULL AND idInstitucion <> @idInst LIMIT 1);   -- administrador de otra institucion (del archivo de INSERTs)

-- ALTA DE ADMINISTRADOR ADICIONAL. Orden: actor (idUsuario), nombre, apellido, documento, domicilio, fechaNac, clave, email, telefono, OUT idAdministrador
-- Deberian fallar (se corre de a uno):
/* CALL sp_AdministradorInstitucion_CrearAdicional(23, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3); */   -- ERROR SIN PERMISO (Dev&Co no es administrador de una institucion)
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario1, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3); */   -- ERROR SIN PERMISO (actor inactivo y sin institucion)
/* CALL sp_AdministradorInstitucion_CrearAdicional(NULL, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3); */   -- ERROR FALTAN DATOS
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '1234', 'Calle 789', '1990-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3); */   -- ERROR DOCUMENTO
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'sin-arroba', '1155550003', @idAdmin3); */   -- ERROR EMAIL FORMATO
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '12121215', 'Calle 789', '2015-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3); */   -- ERROR MENOR DE 18
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'testadmin2@mail.com', '1155550003', @idAdmin3); */   -- ERROR EMAIL YA REGISTRADO (el de testadmin2)

-- Alta correcta, hecha por testadmin2 (administrador activo de @idInst)
CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'testadmin3@mail.com', '1155550003', @idAdmin3);
SELECT @idAdmin3;
SELECT a.id, a.idInstitucion, u.email, u.activo, t.telefono       -- activo = 1, idInstitucion = @idInst, con su telefono
FROM AdministradoresInstitucion a
JOIN Usuarios u ON u.id = a.idUsuario
JOIN Telefonos t ON t.idAdministrador = a.id
WHERE a.id = @idAdmin3;
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Test', 'Admin3', '12121215', 'Calle 789', '1990-05-10', 'clave123', 'otro@mail.com', '1155550003', @idAdmin4); */   -- ERROR YA EXISTE ADMINISTRADOR CON ESE DOCUMENTO

SET @idUsuario3 = (SELECT idUsuario FROM AdministradoresInstitucion WHERE id = @idAdmin3);

-- BAJA / REACTIVAR ADMINISTRADOR. Orden: usuario que actua (idUsuario), administrador objetivo (idAdministrador)
-- Deberian fallar (se corre de a uno):
/* CALL sp_AdministradorInstitucion_DarDeBaja(@idUsuario2, @idAdmin2); */          -- ERROR NO PODES DARTE DE BAJA A VOS MISMO
/* CALL sp_AdministradorInstitucion_DarDeBaja(@idUsuario1, @idAdmin3); */          -- ERROR SIN PERMISO (el actor esta inactivo)
/* CALL sp_AdministradorInstitucion_DarDeBaja(@idUsuarioOtraInst, @idAdmin3); */   -- ERROR SIN PERMISO (administrador de otra institucion)
/* CALL sp_AdministradorInstitucion_DarDeBaja(23, 999999); */                      -- ERROR EL ADMINISTRADOR NO EXISTE
/* CALL sp_AdministradorInstitucion_DarDeBaja(NULL, @idAdmin3); */                 -- ERROR FALTAN DATOS

-- Baja hecha por otro administrador de la misma institucion
CALL sp_AdministradorInstitucion_DarDeBaja(@idUsuario2, @idAdmin3);
SELECT activo FROM Usuarios WHERE id = @idUsuario3;                                -- 0
SELECT idInstitucion FROM AdministradoresInstitucion WHERE id = @idAdmin3;         -- sigue siendo @idInst (no se desvincula)
/* CALL sp_AdministradorInstitucion_DarDeBaja(23, @idAdmin3); */                   -- ERROR YA ESTA INACTIVO
/* CALL sp_AdministradorInstitucion_DarDeBaja(23, @idAdmin2); */                   -- ERROR ULTIMO ADMINISTRADOR ACTIVO (admin3 ya esta de baja)

-- Reactivacion hecha por Dev&Co
/* CALL sp_AdministradorInstitucion_Reactivar(@idUsuarioOtraInst, @idAdmin3); */   -- ERROR SIN PERMISO (administrador de otra institucion)
CALL sp_AdministradorInstitucion_Reactivar(23, @idAdmin3);
SELECT activo FROM Usuarios WHERE id = @idUsuario3;                                -- 1
/* CALL sp_AdministradorInstitucion_Reactivar(23, @idAdmin3); */                   -- ERROR YA ESTA ACTIVO

-- Baja hecha por Dev&Co y reactivacion hecha por otro administrador de la misma institucion
CALL sp_AdministradorInstitucion_DarDeBaja(23, @idAdmin3);
SELECT activo FROM Usuarios WHERE id = @idUsuario3;                                -- 0
CALL sp_AdministradorInstitucion_Reactivar(@idUsuario2, @idAdmin3);
SELECT activo FROM Usuarios WHERE id = @idUsuario3;                                -- 1

-- Una cuenta que nunca fue aprobada no se puede reactivar (testadmin: solo tiene postulaciones rechazadas)
/* CALL sp_AdministradorInstitucion_Reactivar(23, @idAdmin1); */                   -- ERROR LA CUENTA NUNCA FUE APROBADA
/* CALL sp_AdministradorInstitucion_Reactivar(23, 999999); */                      -- ERROR EL ADMINISTRADOR NO EXISTE
/* CALL sp_AdministradorInstitucion_Reactivar(NULL, @idAdmin3); */                 -- ERROR FALTAN DATOS

-- CREAR ADICIONAL con una persona que ya existe por otro rol: Laura Benitez (documento 33456872) es responsable
-- en el archivo de INSERTs. Se reutiliza su persona y se conservan sus datos guardados (los nuevos se ignoran),
-- pero necesita su propia cuenta de administrador con un email distinto al de responsable.
CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Otro', 'Nombre', '33456872', 'Otra calle 1', '2000-01-01', 'clave123', 'laura.admin@mail.com', '1155550004', @idAdmin4);
SELECT @idAdmin4;
SELECT nombre, apellido, domicilio, fechaNacimiento FROM Personas WHERE documento = '33456872';   -- 1 sola fila, con los datos originales: Laura, Benitez, Calle Italia 450, 1987-02-11
SELECT a.idPersona, a.idInstitucion, u.email, u.activo
FROM AdministradoresInstitucion a
JOIN Usuarios u ON u.id = a.idUsuario
WHERE a.id = @idAdmin4;                                                                              -- idPersona = 1, idInstitucion = @idInst, email laura.admin@mail.com, activo = 1
SELECT idUsuario FROM Responsables WHERE idPersona = 1;                                              -- sigue su usuario de responsable (1): ahora la persona tiene dos cuentas
/* CALL sp_AdministradorInstitucion_CrearAdicional(@idUsuario2, 'Laura', 'Benitez', '33456872', 'Calle Italia 450', '1987-02-11', 'clave123', 'lbenitez@mail.test', '1155550004', @idAdmin5); */   -- ERROR EMAIL YA REGISTRADO (usa el email de su cuenta de responsable)

-- Una baja de administrador no afecta a los de otra institucion.
SET @idAdminOtraInst = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = @idUsuarioOtraInst);
/* CALL sp_AdministradorInstitucion_DarDeBaja(@idUsuario2, @idAdminOtraInst); */     -- ERROR SIN PERMISO (testadmin2 no es de esa institucion)
/* CALL sp_AdministradorInstitucion_Reactivar(@idUsuario2, @idAdminOtraInst); */     -- ERROR SIN PERMISO (idem)
SELECT activo FROM Usuarios WHERE id = @idUsuarioOtraInst;                           -- 1: el administrador de la otra institucion sigue activo


-- ---------------------------------------------------------------------
-- VISTAS
-- ---------------------------------------------------------------------
SELECT * FROM vw_Perfil_Usuario WHERE idUsuario IN (1, 11, 23);
-- usuario 1: responsable (idResponsable = 1, sin institucion)
-- usuario 11: administrador (idAdministrador = 1, con nombreInstitucion)
-- usuario 23: Dev&Co (nombre, apellido, documento, domicilio en NULL)

SELECT * FROM vw_Telefonos_Por_Usuario WHERE idUsuario IN (1, 7, 11);
-- usuario 1: 1 telefono | usuario 7: 2 telefonos | usuario 11: 1 telefono

-- ---------------------------------------------------------------------
-- sp_Usuario_ObtenerClave / sp_Usuario_CambiarClave
-- ---------------------------------------------------------------------
CALL sp_Usuario_ObtenerClave(1);                                          -- 1 fila: 'test1234'
CALL sp_Usuario_ObtenerClave(8);                                          -- 0 filas (inactivo)
CALL sp_Usuario_ObtenerClave(999999);                                     -- 0 filas (no existe)
/* CALL sp_Usuario_ObtenerClave(NULL); */                                 -- ERROR FALTAN DATOS

CALL sp_Usuario_CambiarClave(1, 'hash-de-prueba-nuevo');
SELECT clave FROM Usuarios WHERE id = 1;                                  -- 'hash-de-prueba-nuevo'

CALL sp_Usuario_CambiarClave(23, 'hash-de-prueba-devco');                 -- Dev&Co tambien puede cambiar su clave
SELECT clave FROM Usuarios WHERE id = 23;

/* CALL sp_Usuario_CambiarClave(1, ''); */                              -- ERROR FALTAN DATOS
/* CALL sp_Usuario_CambiarClave(NULL, 'x'); */                          -- ERROR FALTAN DATOS
/* CALL sp_Usuario_CambiarClave(8, 'x'); */                             -- ERROR SIN PERMISO (inactivo)
/* CALL sp_Usuario_CambiarClave(999999, 'x'); */                        -- ERROR SIN PERMISO (no existe)

-- Dejar las claves como estaban:
CALL sp_Usuario_CambiarClave(1,  'test1234');
CALL sp_Usuario_CambiarClave(23, 'test1234');

-- ---------------------------------------------------------------------
-- REGLA "MINIMO 1 TELEFONO" en sp_Telefono_Eliminar (solo Administrador y Responsable)
-- ---------------------------------------------------------------------
SET @idResp1 = (SELECT id FROM Responsables WHERE idUsuario = 1);         -- responsable con 1 solo telefono
SET @idTel1  = (SELECT id FROM Telefonos WHERE idResponsable = @idResp1 LIMIT 1);
/* CALL sp_Telefono_Eliminar(1, 'Responsable', @idResp1, @idTel1); */    -- ERROR tiene que dejar al menos un telefono

SET @idAdm1    = (SELECT id FROM AdministradoresInstitucion WHERE idUsuario = 11);
SET @idTelAdm1 = (SELECT id FROM Telefonos WHERE idAdministrador = @idAdm1 LIMIT 1);
/* CALL sp_Telefono_Eliminar(11, 'Administrador', @idAdm1, @idTelAdm1); */  -- ERROR tiene que dejar al menos un telefono

-- Con 2 telefonos si se puede quitar uno (queda 1). Se vuelve a agregar para dejar los datos como estaban.
SET @idResp7 = (SELECT id FROM Responsables WHERE idUsuario = 7);
SET @idTel7  = (SELECT id FROM Telefonos WHERE idResponsable = @idResp7 AND telefono = '+54 9 11 5555-1008');
CALL sp_Telefono_Eliminar(7, 'Responsable', @idResp7, @idTel7);
SELECT * FROM vw_Telefonos_Por_Usuario WHERE idUsuario = 7;               -- queda 1 telefono
SET @idTel7b = (SELECT id FROM Telefonos WHERE idResponsable = @idResp7 LIMIT 1);
/* CALL sp_Telefono_Eliminar(7, 'Responsable', @idResp7, @idTel7b); */   -- ERROR tiene que dejar al menos un telefono (ahora le queda 1)
CALL sp_Telefono_Agregar(7, 'Responsable', @idResp7, '+54 9 11 5555-1008');  -- se restaura
SELECT * FROM vw_Telefonos_Por_Usuario WHERE idUsuario = 7;               -- vuelve a tener 2


-- Limpieza (el modo seguro de Workbench se apaga solo para esta sesion).
-- El orden importa: primero lo que depende de la institucion, despues el resto.
/* SET SQL_SAFE_UPDATES = 0;
DELETE FROM Imagenes WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM IdiomaInstitucion WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Telefonos WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM PostulacionesAlumnos WHERE codigoSeguimiento LIKE 'PST-TEST-%';
DELETE FROM Cursos WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'testadmin3@mail.com'));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'testadmin3@mail.com');
DELETE FROM Usuarios WHERE email = 'testadmin3@mail.com';
DELETE FROM Personas WHERE documento = '12121215';
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'laura.admin@mail.com'));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'laura.admin@mail.com');
DELETE FROM Usuarios WHERE email = 'laura.admin@mail.com';
DELETE FROM Discapacidades WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
UPDATE AdministradoresInstitucion SET idInstitucion = NULL WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Instituciones WHERE cuit = '30999999994';
DELETE FROM PostulacionesAdminInstitucion WHERE codigoSeguimiento LIKE 'PAI-TEST-%';
DELETE FROM Telefonos WHERE telefono = '555555' AND idAdministrador IS NULL AND idResponsable = 1;
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com')));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com'));
DELETE FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com');
DELETE FROM Personas WHERE documento IN ('12121212', '12121214');
SET SQL_SAFE_UPDATES = 1; */

-- =====================================================================
-- LOGIN, RESENIAS, POSTULACIONES Y CURSOS
-- =====================================================================

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
