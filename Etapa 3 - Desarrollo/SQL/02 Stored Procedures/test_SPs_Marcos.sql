CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'testadminas', 'clave123', 'TESTasas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost);

SELECT @idPost;
SELECT * FROM PostulacionesAdminInstitucion WHERE id = @idPost;  -- Pendiente
SELECT * FROM Usuarios WHERE usuario = 'testadmin';              -- activo = 0, email en minúscula
SELECT * FROM AdministradoresInstitucion WHERE idUsuario = (SELECT id FROM Usuarios WHERE usuario = 'testadmin'); -- idInstitucion NULL

-- Deberia fallar:
/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'testadminas', 'clave123', 'TESTasas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost); */ -- ERROR USUARIO EN USO 
  
/* CALL sp_AdministradorInstitucion_Registrar(
  'Test1', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'testadminas', 'clave123', 'TESTasas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost); */ -- ERROR MAIL EN USO

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test1', 'Admin', '12121212', 'Calle 123', '1990-05-10',
  'testadminas', 'clave123', 'TEST1asas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost); */ -- ERROR DOCUMENTO EN USO
  
  /* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121219', 'Calle 123', '1990-05-10',
  'testadminas', 'clave123', 'TESTasas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost); */ -- ERROR CUIT EN USO

/* CALL sp_AdministradorInstitucion_Registrar(
  'Test', 'Admin', '12121212', 'Calle 123', '2015-01-01',
  'testadminas', 'clave123', 'TESTasas@mail.com', '121212',
  'Instituto fest', '30999999992', 'fest.pdf', 'hafasd.pdf',
  'PAI-TEST-0001', @idPost);*/  -- ERROR MAYOR DE EDAD
