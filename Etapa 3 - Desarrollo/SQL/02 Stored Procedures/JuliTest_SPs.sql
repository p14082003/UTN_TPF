
-- Test sp validar usuario Login

-- Select * from Usuarios; 
CALL sp_Validar_Usuario_Login('lbenitez', 'test1234');
CALL sp_Validar_Usuario_Login('', 'test1234'); -- falla por usuario 
CALL sp_Validar_Usuario_Login('lbenitez', ''); -- falla por clave 
CALL sp_Validar_Usuario_Login('clarisa', '1234'); -- falla por usuario no registrad (no válido)
CALL sp_Validar_Usuario_Login('rsilva', 'test1234'); -- falla por usuario inactivo

-- Test sp registrar resenia 

CALL sp_Registrar_Resenia('', 4, 2, 2); -- falla por comentario vacío
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', 7, 2, 2); -- falla por valoracion mas alta que 5
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', -3, 2, 2); -- falla por valoracion mas baja que 1
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', 5, 10, 8); -- deberia fallar por responsable inactivo
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', 5, 4, 1); -- deberia fallar por no coincide responsable con postulacion
-- CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', 5, 4, 1); -- deberia fallar por institucion inactiva pero necesitamos hacer un insert o cambiar el estado de una institucion
CALL sp_Registrar_Resenia('muy buenos maestros y compañeros. Escelente clima', 5, 2, 2); -- este es correcto 
Select * from Usuarios; 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', 'test1234');     
CALL sp_Validar_Usuario_Login('LBenitez@mail.test ', 'test1234'); --  funciona porque se pasa a minuscula con lower y se recortan espacios con trim
CALL sp_Validar_Usuario_Login('', 'test1234'); -- falla por email 
CALL sp_Validar_Usuario_Login('lbenitez@mail.test', ''); -- falla por clave 
CALL sp_Validar_Usuario_Login('clarisa@mail.test', '1234'); -- falla por usuario no registrado (no válido)
CALL sp_Validar_Usuario_Login('rsilva@mail.test', 'test1234'); -- falla por usuario inactivo
