Select * from Usuarios; 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', 'test1234');     
CALL sp_Validar_Usuario_Login('LBenitez@mail.test ', 'test1234'); --  funciona porque se pasa a minuscula con lower y se recortan espacios con trim
CALL sp_Validar_Usuario_Login('', 'test1234'); -- falla por email 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', ''); -- falla por clave 
CALL sp_Validar_Usuario_Login('clarisa@mail.test', '1234'); -- falla por usuario no registrado (no válido)
CALL sp_Validar_Usuario_Login('rsilva@mail.test', 'test1234'); -- falla por usuario inactivo
