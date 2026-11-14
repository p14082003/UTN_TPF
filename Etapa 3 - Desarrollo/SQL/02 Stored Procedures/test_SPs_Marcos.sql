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
  'PAI-TEST-0002', @idPost); */ -- ERROR DOCUMENTO EN USO (ya es administrador)

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

-- Deberian fallar (se corre de a uno):
/* CALL sp_PostulacionAdmin_Aprobar(@idPost2, 23); */                                     -- ERROR YA FUE RESUELTA
/* CALL sp_PostulacionAdmin_Aprobar(999999, 23); */                                       -- ERROR NO EXISTE
/* CALL sp_PostulacionAdmin_Aprobar(@idPost2, 11); */                                     -- ERROR SIN PERMISO
/* CALL sp_PostulacionAdmin_Aprobar(NULL, 23); */                                         -- ERROR FALTAN DATOS

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

-- AGREGAR TELEFONO. Orden: admin, institucion, telefono
CALL sp_Telefono_Institucion_Agregar(@idAdmin2, @idInst, '+54 11 4000-0000');
SELECT * FROM Telefonos WHERE idInstitucion = @idInst;              -- 1 fila
/* CALL sp_Telefono_Institucion_Agregar(@idAdmin2, @idInst, '+54 11 4000-0000'); */   -- ERROR YA TIENE ESE TELEFONO
/* CALL sp_Telefono_Institucion_Agregar(@idAdmin2, @idInst, '  '); */                 -- ERROR FALTAN DATOS
/* CALL sp_Telefono_Institucion_Agregar(@idAdmin1, @idInst, '1111'); */               -- ERROR SIN PERMISO (otro administrador no aceptado)
/* CALL sp_Telefono_Institucion_Agregar(@idAdmin2, 999999, '1111'); */                -- ERROR INSTITUCION NO EXISTE

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

-- Limpieza (el modo seguro de Workbench se apaga solo para esta sesion).
-- El orden importa: primero lo que depende de la institucion, despues el resto.
/* SET SQL_SAFE_UPDATES = 0;
DELETE FROM Imagenes WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM IdiomaInstitucion WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Telefonos WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Discapacidades WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
UPDATE AdministradoresInstitucion SET idInstitucion = NULL WHERE idInstitucion IN (SELECT id FROM Instituciones WHERE cuit = '30999999994');
DELETE FROM Instituciones WHERE cuit = '30999999994';
DELETE FROM PostulacionesAdminInstitucion WHERE codigoSeguimiento LIKE 'PAI-TEST-%';
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com')));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com'));
DELETE FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com');
DELETE FROM Personas WHERE documento IN ('12121212', '12121214');
SET SQL_SAFE_UPDATES = 1; */
