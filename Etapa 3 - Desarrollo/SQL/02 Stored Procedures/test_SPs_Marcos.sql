-- TEST sp_AdministradorInstitucion_Registrar
-- Orden de parametros: nombre, apellido, documento, domicilio, fechaNacimiento,
-- clave, email, telefono, nombreInstitucion, cuit, constanciaFiscal, habilitacion,
-- constanciaLaboral, codigoSeguimiento, OUT idPostulacion.

-- Caso feliz:
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

-- Limpieza (el modo seguro de Workbench se apaga solo para esta sesion):
/* SET SQL_SAFE_UPDATES = 0;
DELETE FROM PostulacionesAdminInstitucion WHERE codigoSeguimiento LIKE 'PAI-TEST-%';
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com')));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com'));
DELETE FROM Usuarios WHERE email IN ('testadmin@mail.com', 'testadmin2@mail.com');
DELETE FROM Personas WHERE documento IN ('12121212', '12121214');
SET SQL_SAFE_UPDATES = 1; */
