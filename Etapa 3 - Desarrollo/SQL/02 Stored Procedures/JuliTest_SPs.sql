Select * from Usuarios; 
CALL sp_Validar_Usuario_Login('lbenitez', 'test1234');
CALL sp_Validar_Usuario_Login('', 'test1234'); -- falla por usuario 
CALL sp_Validar_Usuario_Login('lbenitez', ''); -- falla por clave 
CALL sp_Validar_Usuario_Login('clarisa', '1234'); -- falla por usuario no registrad (no válido)
CALL sp_Validar_Usuario_Login('rsilva', 'test1234'); -- falla por usuario inactivo
