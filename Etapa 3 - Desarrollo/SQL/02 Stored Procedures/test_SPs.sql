CALL sp_Registrar_Responsable('mlopez', 'test1234', 'mlopez@mail.test', 'Mariana', 'Lopez', '40111222', 'Calle Falsa 123, Tigre', '1992-03-15', '+54 9 11 5555-2000');
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
-- CALL sp_Registrar_Responsable('mlopez', 'x', 'otro@mail.test', 'Otra', 'Persona', '40999888', 'Calle 1', '1990-01-01', NULL);     -- usuario repetido
-- CALL sp_Registrar_Responsable('otrolopez', 'x', 'mlopez@mail.test', 'Otra', 'Persona', '40999888', 'Calle 1', '1990-01-01', NULL); -- email repetido
-- CALL sp_Registrar_Responsable('menor', 'x', 'menor@mail.test', 'Joven', 'Menor', '45000111', 'Calle 1', '2015-01-01', NULL);       -- menor de edad
-- CALL sp_Registrar_Responsable('sofia2', 'x', 'sofia2@mail.test', 'Sofia', 'Benitez', '57412086', 'Calle 1', '1990-01-01', NULL);   -- DNI de una menor ya cargada: usa su fecha guardada, no la recibida
-- CALL sp_Registrar_Alumno(999, 2, 'Test', 'Test', '55000111', 'Calle 1', '2018-01-01');                        -- responsable inexistente
-- CALL sp_Registrar_Alumno(11, 9, 'Test', 'Test', '55000111', 'Calle 1', '2018-01-01');                         -- rol invalido
-- CALL sp_Registrar_Alumno(11, 2, 'Sofia', 'Lopez', '55111222', 'Calle 1', '2018-05-10');                       -- DNI ya registrado (el alumno ya tiene su unico responsable)
-- CALL sp_Registrar_Responsable('cero', 'x', 'cero@mail.test', 'Luis', 'R', '01234567', 'Calle 1', '1990-01-01', NULL);             -- el documento no puede empezar con 0
-- CALL sp_Registrar_Responsable('largo', 'x', 'largo@mail.test', 'Luis', 'R', '123456789', 'Calle 1', '1990-01-01', NULL);          -- el documento no puede tener mas de 8 caracteres
-- CALL sp_Registrar_Responsable('vacio', 'x', 'vacio@mail.test', '   ', 'R', '40555666', 'Calle 1', '1990-01-01', NULL);            -- nombre obligatorio (persona nueva)
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
LEFT JOIN Cursos c                ON c.id = a.idCurso
LEFT JOIN Instituciones i         ON i.id = c.idInstitucion
ORDER BY a.id;