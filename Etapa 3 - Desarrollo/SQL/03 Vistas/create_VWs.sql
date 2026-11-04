USE InstiRed_DEV;

CREATE OR REPLACE VIEW vw_Responsables_Con_Alumnos AS
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
   OR NOT EXISTS (SELECT 1
                    FROM Alumnos x
                   WHERE x.idPersona = r.idPersona
                     AND NOT EXISTS (SELECT 1 FROM ResponsablesPorAlumno y WHERE y.idAlumno = x.id))

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
LEFT JOIN Instituciones i ON i.id = c.idInstitucion
WHERE NOT EXISTS (SELECT 1 FROM ResponsablesPorAlumno y WHERE y.idAlumno = a.id);

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
CREATE OR REPLACE VIEW vw_Valoraciones_Por_Institucion AS
SELECT rs.id                   AS idResenia,
       rs.idInstitucion,
       i.nombre                AS institucion,
       rs.idResponsable,
       p.nombre                AS nombreResponsable,
       p.apellido              AS apellidoResponsable,
       rs.calificacion,
       rs.comentario,
       rs.respuestaInstitucion,
       rs.fecha
FROM Resenias rs
JOIN Instituciones i ON i.id = rs.idInstitucion
JOIN Responsables r  ON r.id = rs.idResponsable
JOIN Personas p      ON p.id = r.idPersona;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
CREATE OR REPLACE VIEW vw_Postulaciones_Admin_Institucion AS
SELECT pa.id                 AS idPostulacion,
       pa.fecha,
       pa.codigoSeguimiento,
       pa.idEstado,
       ep.descripcion        AS estado,
       pa.idAdministradorInstitucion,
       p.nombre              AS nombreAdministrador,
       p.apellido            AS apellidoAdministrador,
       p.documento           AS docAdministrador,
       u.email               AS emailAdministrador,
       u.activo              AS usuarioActivo,
       pa.nombreInstitucion,
       pa.cuitInstitucion,
       pa.rutaConstanciaFiscal,
       pa.rutaHabilitacion,
       pa.motivoRechazo,
       a.idInstitucion       -- NULL hasta que el administrador cree la institucion
FROM PostulacionesAdminInstitucion pa
JOIN EstadosPostulacion ep        ON ep.id = pa.idEstado
JOIN AdministradoresInstitucion a ON a.id = pa.idAdministradorInstitucion
JOIN Personas p                   ON p.id = a.idPersona
JOIN Usuarios u                   ON u.id = a.idUsuario;
-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
CREATE OR REPLACE VIEW vw_Postulaciones_Alumnos AS
SELECT po.id                 AS idPostulacion,
       po.fecha,
       po.codigoSeguimiento,
       po.idEstado,
       ep.descripcion        AS estado,
       po.motivoRechazo,
       po.idAlumno,
       p.nombre              AS nombreAlumno,
       p.apellido            AS apellidoAlumno,
       p.documento           AS docAlumno,
       p.fechaNacimiento     AS fechaNacimientoAlumno,
       r.id                  AS idResponsable,
       pr.nombre             AS nombreResponsable,
       pr.apellido           AS apellidoResponsable,
       u.email               AS emailResponsable,
       (SELECT GROUP_CONCAT(te.telefono ORDER BY te.id SEPARATOR ', ')
          FROM Telefonos te
         WHERE te.idResponsable = r.id) AS telefonosResponsable,
       COALESCE(ro.descripcion, 'Autogestor') AS rolResponsable,
       po.idCurso,
       c.cicloLectivo,
       COALESCE(ne.descripcion, c.nivelPersonalizado) AS nivel,
       c.anio,
       c.division,
       tu.descripcion        AS turno,
       c.idInstitucion,
       i.nombre              AS institucion
FROM PostulacionesAlumnos po
JOIN EstadosPostulacion ep          ON ep.id = po.idEstado
JOIN Alumnos a                      ON a.id = po.idAlumno
JOIN Personas p                     ON p.id = a.idPersona
JOIN Cursos c                       ON c.id = po.idCurso
JOIN Turnos tu                      ON tu.id = c.idTurno
JOIN Instituciones i                ON i.id = c.idInstitucion
LEFT JOIN NivelesEscolares ne       ON ne.id = c.idNivel
LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idAlumno = a.id
LEFT JOIN Roles ro                  ON ro.id = rpa.idRol
LEFT JOIN Responsables rs           ON rs.idPersona = a.idPersona      -- autogestor
LEFT JOIN Responsables r            ON r.id = COALESCE(rpa.idResponsable, rs.id)
LEFT JOIN Personas pr               ON pr.id = r.idPersona
LEFT JOIN Usuarios u                ON u.id = r.idUsuario;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
CREATE OR REPLACE VIEW vw_Postulaciones_Por_Responsable AS
SELECT r.id                  AS idResponsable,
       po.id                 AS idPostulacion,
       po.fecha,
       po.codigoSeguimiento,
       po.idEstado,
       ep.descripcion        AS estado,
       po.motivoRechazo,                  
       po.idAlumno,
       p.nombre              AS nombreAlumno,
       p.apellido            AS apellidoAlumno,
       po.idCurso,
       c.cicloLectivo,
       COALESCE(ne.descripcion, c.nivelPersonalizado) AS nivel,
       c.anio,
       c.division,
       tu.descripcion        AS turno,
       c.idInstitucion,
       i.nombre              AS institucion,
       i.direccion           AS direccionInstitucion
FROM PostulacionesAlumnos po
JOIN EstadosPostulacion ep          ON ep.id = po.idEstado
JOIN Alumnos a                      ON a.id = po.idAlumno
JOIN Personas p                     ON p.id = a.idPersona
JOIN Cursos c                       ON c.id = po.idCurso
JOIN Turnos tu                      ON tu.id = c.idTurno
JOIN Instituciones i                ON i.id = c.idInstitucion
LEFT JOIN NivelesEscolares ne       ON ne.id = c.idNivel
LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idAlumno = a.id
LEFT JOIN Responsables rs           ON rs.idPersona = a.idPersona      -- autogestor
JOIN Responsables r                 ON r.id = COALESCE(rpa.idResponsable, rs.id);


-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
CREATE OR REPLACE VIEW vw_Instituciones AS
SELECT i.id                  AS idInstitucion,
       i.nombre,
       i.direccion,
       i.email,
       i.latitud,
       i.longitud,
       i.descripcion,
       i.estado,
       i.idTipoInstitucion,
       ti.tipoInstitucion,
       d.movilidad,
       d.visual,
       d.auditiva,
       d.verbal,
       d.intelectual,
       d.sindromeDown,
       d.autismo,
       (SELECT GROUP_CONCAT(idi.idioma ORDER BY idi.idioma SEPARATOR ', ')
          FROM IdiomaInstitucion ii
          JOIN Idiomas idi ON idi.id = ii.idIdioma
         WHERE ii.idInstitucion = i.id)                      AS idiomas,
       (SELECT GROUP_CONCAT(t.telefono ORDER BY t.id SEPARATOR ', ')
          FROM Telefonos t
         WHERE t.idInstitucion = i.id)                       AS telefonos,
       (SELECT ROUND(AVG(rs.calificacion), 1)
          FROM Resenias rs
         WHERE rs.idInstitucion = i.id)                      AS promedioCalificacion,
       (SELECT COUNT(*)
          FROM Resenias rs
         WHERE rs.idInstitucion = i.id)                      AS cantidadResenias
FROM Instituciones i
JOIN TipoInstitucion ti    ON ti.id = i.idTipoInstitucion
LEFT JOIN Discapacidades d ON d.idInstitucion = i.id;
































-- VERSIONES VIEJAS:
-- CREATE OR REPLACE VIEW vw_Responsables_Con_Alumnos AS
-- -- 1) Responsables con los alumnos a su cargo (o sin ninguno)
-- SELECT r.id                  AS idResponsable,
--        pr.nombre             AS nombreResponsable,
--        pr.apellido           AS apellidoResponsable,
--        pr.documento          AS docResponsable,
--        u.email               AS emailResponsable,
--        u.activo              AS usuarioActivo,
--        a.id                  AS idAlumno,
--        pa.nombre             AS nombreAlumno,
--        pa.apellido           AS apellidoAlumno,
--        pa.documento          AS docAlumno,
--        ro.descripcion        AS rol,
--        c.id                  AS idCurso,
--        i.nombre              AS institucion
-- FROM Responsables r
-- JOIN Personas pr                    ON pr.id = r.idPersona
-- JOIN Usuarios u                     ON u.id = r.idUsuario
-- LEFT JOIN ResponsablesPorAlumno rpa ON rpa.idResponsable = r.id
-- LEFT JOIN Alumnos a                 ON a.id = rpa.idAlumno
-- LEFT JOIN Personas pa               ON pa.id = a.idPersona
-- LEFT JOIN Roles ro                  ON ro.id = rpa.idRol
-- LEFT JOIN Cursos c                  ON c.id = a.idCurso
-- LEFT JOIN Instituciones i           ON i.id = c.idInstitucion
-- WHERE rpa.id IS NOT NULL
--    OR NOT EXISTS (SELECT 1 FROM Alumnos x WHERE x.idPersona = r.idPersona)

-- UNION ALL

-- -- 2) Responsables que ademas son alumnos de si mismos (autogestor)
-- SELECT r.id, pr.nombre, pr.apellido, pr.documento, u.email, u.activo,
--        a.id, pr.nombre, pr.apellido, pr.documento,
--        'Autogestor',
--        c.id, i.nombre
-- FROM Responsables r
-- JOIN Personas pr          ON pr.id = r.idPersona
-- JOIN Usuarios u           ON u.id = r.idUsuario
-- JOIN Alumnos a            ON a.idPersona = r.idPersona
-- LEFT JOIN Cursos c        ON c.id = a.idCurso
-- LEFT JOIN Instituciones i ON i.id = c.idInstitucion;