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

-- Limpieza (el modo seguro de Workbench se apaga solo para esta sesion):
/* SET SQL_SAFE_UPDATES = 0;
DELETE FROM PostulacionesAdminInstitucion WHERE codigoSeguimiento LIKE 'PAI-TEST-%';
DELETE FROM Telefonos WHERE idAdministrador IN (SELECT id FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com'));
DELETE FROM AdministradoresInstitucion WHERE idUsuario IN (SELECT id FROM Usuarios WHERE email = 'testadmin@mail.com');
DELETE FROM Usuarios WHERE email = 'testadmin@mail.com';
DELETE FROM Personas WHERE documento = '12121212';
SET SQL_SAFE_UPDATES = 1; */
