CREATE OR REPLACE VIEW vw_ResponsablesConAlumnos AS
-- 1) Responsables con los alumnos a su cargo (o sin ninguno)
SELECT r.id                  AS idResponsable,
       pr.nombre             AS nombreResponsable,
       pr.apellido           AS apellidoResponsable,
       pr.documento          AS docResponsable,
       u.email               AS emailResponsable,
       u.activo              AS usuarioActivo,
       a.id                  AS idAlumno,
       pa.nombre             AS nombreAlumno,
       pa.apellido           AS apellidoAlumno,
       pa.documento          AS docAlumno,
       ro.descripcion        AS rol,
       c.id                  AS idCurso,
       i.nombre              AS institucion
FROM Responsables r
JOIN Personas pr                    ON pr.id = r.idPersona
JOIN Usuarios u                     ON u.id = r.idUsuario
LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idResponsable = r.id
LEFT JOIN Alumnos a                 ON a.id = rpa.idAlumno
LEFT JOIN Personas pa               ON pa.id = a.idPersona
LEFT JOIN Roles ro                  ON ro.id = rpa.idRol
LEFT JOIN Cursos c                  ON c.id = a.idCurso
LEFT JOIN Instituciones i           ON i.id = c.idInstitucion
WHERE rpa.id IS NOT NULL
   OR NOT EXISTS (SELECT 1 FROM Alumnos x WHERE x.idPersona = r.idPersona)

UNION ALL

-- 2) Responsables que ademas son alumnos de si mismos (autogestor)
SELECT r.id, pr.nombre, pr.apellido, pr.documento, u.email, u.activo,
       a.id, pr.nombre, pr.apellido, pr.documento,
       'Autogestor',
       c.id, i.nombre
FROM Responsables r
JOIN Personas pr          ON pr.id = r.idPersona
JOIN Usuarios u           ON u.id = r.idUsuario
JOIN Alumnos a            ON a.idPersona = r.idPersona
LEFT JOIN Cursos c        ON c.id = a.idCurso
LEFT JOIN Instituciones i ON i.id = c.idInstitucion;