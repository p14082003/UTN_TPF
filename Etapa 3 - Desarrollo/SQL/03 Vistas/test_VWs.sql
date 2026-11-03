SELECT * FROM vw_Responsables_Con_Alumnos ORDER BY idResponsable, idAlumno;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
SELECT * FROM vw_Valoraciones_Por_Institucion 
-- WHERE idInstitucion = 
-- ORDER BY idResponsable, idAlumno
ORDER BY fecha DESC;
-- podria sacarse el apellido, si no queremos exponer privacidad. O hacer como temu q pone asteriscos haciendo así:
SELECT
    idResenia,
    idInstitucion,
    institucion,
    idResponsable,
    nombreResponsable,
    CONCAT(
        LEFT(apellidoResponsable, 3),
        REPEAT('*', GREATEST(0, CHAR_LENGTH(apellidoResponsable) - 3))
    ) AS apellidoResponsable,
    calificacion,
    comentario,
    respuestaInstitucion,
    fecha
FROM vw_Valoraciones_Por_Institucion
ORDER BY fecha DESC;
-- creo q conviene hacerlo por front ya que en varias situaciones vamos a querer tener los apellidos completos.

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
SELECT * FROM vw_Postulaciones_Admin_Institucion;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
SELECT * FROM vw_Postulaciones_Alumnos ORDER BY fecha;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
SELECT * FROM vw_Postulaciones_Por_Responsable ORDER BY fecha;

-- ==============================================================================
-- ==============================================================================
-- ==============================================================================
SELECT * FROM vw_Instituciones;