-- TEST ALUMNO AUTOGESTOR
-- Un alumno que pasa a ser mayor de edad no debe reconocerse como autogestionado automaticamente.
-- Una versión vieja de la vw_ contenía "WHERE rpa.id IS NOT NULL OR NOT EXISTS (SELECT 1 FROM Alumnos x WHERE x.idPersona = r.idPersona) UNION ALL"
-- La nueva contiene 					"WHERE rpa.id IS NOT NULL OR NOT EXISTS (SELECT 1 FROM Alumnos x WHERE x.idPersona = r.idPersona AND NOT EXISTS (SELECT 1 FROM ResponsablesPorAlumno y WHERE y.idAlumno = x.id)) UNION ALL"
-- Esto fuerza a que deba borrarse el enlace con un responsable antes de que una persona que paso a ser adulta se considere "autogestionada".
USE InstiRed_DEV;

-- 1) Se carga un Madre (responsable). 
CALL sp_Registrar_Responsable('test_madre', 'clave123', 'test_madre@mail.test',
                              'Test', 'Madre', '90000001', 'Calle Falsa 123', '1975-03-20', NULL);

SET @idMadre = (SELECT r.id FROM Responsables r
                JOIN Personas p ON p.id = r.idPersona
                WHERE p.documento = '90000001');

-- 2) Hijo cargado como alumno, a cargo de la madre (rol 2 = Madre)
CALL sp_Registrar_Alumno(@idMadre, 2, 'Test', 'Hijo', '90000002', 'Calle Falsa 123', '2007-05-10');

-- 3) El hijo, ya adulto, se registra como responsable con el mismo DNI (reutiliza la Persona), pero no tiene alumnos a cargo aún.
CALL sp_Registrar_Responsable('test_hijo', 'clave123', 'test_hijo@mail.test',
                              'Test', 'Hijo', '90000002', 'Calle Falsa 123', '2007-05-10', NULL);

-- Verificación: La madre figura con "hijo" a cargo, e hijo figura sin nadie a cargo.
SELECT * FROM vw_Responsables_Con_Alumnos WHERE docResponsable IN ('90000001', '90000002');

-- Control: hijo no debe aparecer como autogestor.
SELECT * FROM vw_Responsables_Con_Alumnos WHERE rol = 'Autogestor';

-- 4) Si se corta el lazo con la madre (Responsables por alumno), ahora si aparecerá como auto gestor.
DELETE FROM ResponsablesPorAlumno
WHERE idAlumno = (SELECT a.id FROM Alumnos a
                  JOIN Personas p ON p.id = a.idPersona
                  WHERE p.documento = '90000002');
                  
-- Verificación
SELECT * FROM vw_Responsables_Con_Alumnos WHERE docResponsable IN ('90000001', '90000002');

-- Control: Ahora si aparecera como autogestor.
SELECT * FROM vw_Responsables_Con_Alumnos WHERE rol = 'Autogestor';
                  
                  
-- Limpieza:
DELETE FROM ResponsablesPorAlumno WHERE idAlumno IN (
  SELECT a.id FROM Alumnos a JOIN Personas p ON p.id = a.idPersona WHERE p.documento = '90000002');
DELETE FROM Alumnos      WHERE idPersona IN (SELECT id FROM Personas WHERE documento IN ('90000001', '90000002'));
DELETE FROM Responsables WHERE idPersona IN (SELECT id FROM Personas WHERE documento IN ('90000001', '90000002'));
DELETE FROM Usuarios     WHERE usuario IN ('test_madre', 'test_hijo');
DELETE FROM Personas     WHERE documento IN ('90000001', '90000002');