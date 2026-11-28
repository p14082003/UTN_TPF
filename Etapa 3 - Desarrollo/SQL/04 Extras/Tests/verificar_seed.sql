-- Verificación inicial del NUEVO seed. Solo lectura. MySQL 8.0.
-- Ejecutar antes de modificar los casos desde la web; luego variarán cantidades.
USE InstiRed_DEV;
SET NAMES utf8mb4;

SELECT tabla,esperadas,actuales,IF(esperadas=actuales,'OK','REVISAR') AS resultado
FROM (
SELECT 'TipoUsuario' AS tabla, 3 AS esperadas, COUNT(*) AS actuales FROM TipoUsuario
UNION ALL
SELECT 'Roles' AS tabla, 4 AS esperadas, COUNT(*) AS actuales FROM Roles
UNION ALL
SELECT 'TipoInstitucion' AS tabla, 8 AS esperadas, COUNT(*) AS actuales FROM TipoInstitucion
UNION ALL
SELECT 'Idiomas' AS tabla, 5 AS esperadas, COUNT(*) AS actuales FROM Idiomas
UNION ALL
SELECT 'Turnos' AS tabla, 4 AS esperadas, COUNT(*) AS actuales FROM Turnos
UNION ALL
SELECT 'EstadosPostulacion' AS tabla, 5 AS esperadas, COUNT(*) AS actuales FROM EstadosPostulacion
UNION ALL
SELECT 'NivelesEscolares' AS tabla, 3 AS esperadas, COUNT(*) AS actuales FROM NivelesEscolares
UNION ALL
SELECT 'Personas' AS tabla, 94 AS esperadas, COUNT(*) AS actuales FROM Personas
UNION ALL
SELECT 'Usuarios' AS tabla, 48 AS esperadas, COUNT(*) AS actuales FROM Usuarios
UNION ALL
SELECT 'Instituciones' AS tabla, 8 AS esperadas, COUNT(*) AS actuales FROM Instituciones
UNION ALL
SELECT 'Discapacidades' AS tabla, 8 AS esperadas, COUNT(*) AS actuales FROM Discapacidades
UNION ALL
SELECT 'IdiomaInstitucion' AS tabla, 17 AS esperadas, COUNT(*) AS actuales FROM IdiomaInstitucion
UNION ALL
SELECT 'Cursos' AS tabla, 43 AS esperadas, COUNT(*) AS actuales FROM Cursos
UNION ALL
SELECT 'Imagenes' AS tabla, 16 AS esperadas, COUNT(*) AS actuales FROM Imagenes
UNION ALL
SELECT 'Alumnos' AS tabla, 48 AS esperadas, COUNT(*) AS actuales FROM Alumnos
UNION ALL
SELECT 'Responsables' AS tabla, 34 AS esperadas, COUNT(*) AS actuales FROM Responsables
UNION ALL
SELECT 'AdministradoresInstitucion' AS tabla, 13 AS esperadas, COUNT(*) AS actuales FROM AdministradoresInstitucion
UNION ALL
SELECT 'ResponsablesPorAlumno' AS tabla, 47 AS esperadas, COUNT(*) AS actuales FROM ResponsablesPorAlumno
UNION ALL
SELECT 'PostulacionesAlumnos' AS tabla, 56 AS esperadas, COUNT(*) AS actuales FROM PostulacionesAlumnos
UNION ALL
SELECT 'DocumentosPostulacion' AS tabla, 171 AS esperadas, COUNT(*) AS actuales FROM DocumentosPostulacion
UNION ALL
SELECT 'PostulacionesAdminInstitucion' AS tabla, 11 AS esperadas, COUNT(*) AS actuales FROM PostulacionesAdminInstitucion
UNION ALL
SELECT 'Resenias' AS tabla, 30 AS esperadas, COUNT(*) AS actuales FROM Resenias
UNION ALL
SELECT 'Telefonos' AS tabla, 57 AS esperadas, COUNT(*) AS actuales FROM Telefonos
) conteos;

SELECT comprobacion,revisar AS cantidad_a_revisar,IF(revisar=0,'OK','REVISAR') AS resultado
FROM (
SELECT 'Catálogos con IDs esperados' AS comprobacion, (SELECT (SELECT COUNT(*) FROM TipoUsuario WHERE (id=1 AND descripcion='Responsable') OR (id=2 AND descripcion='Administrador') OR (id=3 AND descripcion='DevCo')) <> 3 OR (SELECT COUNT(*) FROM EstadosPostulacion WHERE (id=1 AND descripcion='Pendiente') OR (id=2 AND descripcion='Aceptado') OR (id=3 AND descripcion='Confirmado') OR (id=4 AND descripcion='Rechazado') OR (id=5 AND descripcion='Cancelado')) <> 5) AS revisar
UNION ALL
SELECT 'Capacidad e inscriptos válidos' AS comprobacion, (SELECT COUNT(*) FROM Cursos WHERE capacidad < 1 OR inscriptos < 0 OR inscriptos > capacidad) AS revisar
UNION ALL
SELECT 'Normalizados y personalizados excluyentes' AS comprobacion, (SELECT COUNT(*) FROM Cursos WHERE NOT ((idNivel IS NOT NULL AND anio IS NOT NULL AND nivelPersonalizado IS NULL AND ((idNivel=1 AND anio BETWEEN 3 AND 5) OR (idNivel=2 AND anio BETWEEN 1 AND 7) OR (idNivel=3 AND anio BETWEEN 1 AND 6))) OR (idNivel IS NULL AND anio IS NULL AND nivelPersonalizado IS NOT NULL AND TRIM(nivelPersonalizado) <> ''))) AS revisar
UNION ALL
SELECT 'Nivel compatible con tipo de institución' AS comprobacion, (SELECT COUNT(*) FROM Cursos c JOIN Instituciones i ON i.id=c.idInstitucion WHERE NOT ((i.idTipoInstitucion=4 AND c.idNivel IS NULL) OR (i.idTipoInstitucion=1 AND c.idNivel=1) OR (i.idTipoInstitucion=2 AND c.idNivel=2) OR (i.idTipoInstitucion=3 AND c.idNivel=3) OR (i.idTipoInstitucion=5 AND c.idNivel IN (1,2)) OR (i.idTipoInstitucion=6 AND c.idNivel IN (2,3)) OR (i.idTipoInstitucion=7 AND c.idNivel IN (1,2,3)) OR (i.idTipoInstitucion=8 AND c.idNivel IN (1,3)))) AS revisar
UNION ALL
SELECT 'Sin cursos duplicados' AS comprobacion, (SELECT COUNT(*) FROM (SELECT idInstitucion,idTurno,cicloLectivo,idNivel,anio,nivelPersonalizado,division FROM Cursos GROUP BY idInstitucion,idTurno,cicloLectivo,idNivel,anio,nivelPersonalizado,division HAVING COUNT(*)>1) q) AS revisar
UNION ALL
SELECT 'Sin postulaciones activas repetidas' AS comprobacion, (SELECT COUNT(*) FROM (SELECT idAlumno,idCurso FROM PostulacionesAlumnos WHERE idEstado IN (1,2,3) GROUP BY idAlumno,idCurso HAVING COUNT(*)>1) q) AS revisar
UNION ALL
SELECT 'Sin doble confirmación del mismo nivel/año/ciclo' AS comprobacion, (SELECT COUNT(*) FROM (SELECT p.idAlumno,c.cicloLectivo,c.idNivel,c.anio,c.nivelPersonalizado FROM PostulacionesAlumnos p JOIN Cursos c ON c.id=p.idCurso WHERE p.idEstado=3 GROUP BY p.idAlumno,c.cicloLectivo,c.idNivel,c.anio,c.nivelPersonalizado HAVING COUNT(*)>1) q) AS revisar
UNION ALL
SELECT 'Un responsable por alumno, incluido autogestor' AS comprobacion, (SELECT COUNT(*) FROM Alumnos a WHERE ((SELECT COUNT(*) FROM ResponsablesPorAlumno x WHERE x.idAlumno=a.id)+(SELECT COUNT(*) FROM Responsables r WHERE r.idPersona=a.idPersona)) <> 1) AS revisar
UNION ALL
SELECT 'Responsables y administradores mayores de edad' AS comprobacion, (SELECT COUNT(*) FROM Personas p WHERE p.fechaNacimiento > DATE_SUB(CURDATE(), INTERVAL 18 YEAR) AND (EXISTS (SELECT 1 FROM Responsables r WHERE r.idPersona=p.id) OR EXISTS (SELECT 1 FROM AdministradoresInstitucion a WHERE a.idPersona=p.id))) AS revisar
UNION ALL
SELECT 'Documento frente/dorso y partida salvo autogestores' AS comprobacion, (SELECT COUNT(*) FROM PostulacionesAlumnos p JOIN Alumnos a ON a.id=p.idAlumno WHERE NOT EXISTS (SELECT 1 FROM DocumentosPostulacion d WHERE d.idPostulacionAlumnos=p.id AND d.descripcion='DNI del alumno - Frente') OR NOT EXISTS (SELECT 1 FROM DocumentosPostulacion d WHERE d.idPostulacionAlumnos=p.id AND d.descripcion='DNI del alumno - Dorso') OR (NOT EXISTS (SELECT 1 FROM Responsables r WHERE r.idPersona=a.idPersona) AND NOT EXISTS (SELECT 1 FROM DocumentosPostulacion d WHERE d.idPostulacionAlumnos=p.id AND d.descripcion='Partida de nacimiento'))) AS revisar
UNION ALL
SELECT 'Sin repetición de documentos de tipo único' AS comprobacion, (SELECT COUNT(*) FROM (SELECT idPostulacionAlumnos,descripcion FROM DocumentosPostulacion WHERE descripcion IN ('DNI del alumno - Frente','DNI del alumno - Dorso','Partida de nacimiento','Certificado de estudios') GROUP BY idPostulacionAlumnos,descripcion HAVING COUNT(*)>1) q) AS revisar
UNION ALL
SELECT 'Sin fechas de postulación futuras' AS comprobacion, (SELECT COUNT(*) FROM PostulacionesAlumnos WHERE fecha>CURDATE()) AS revisar
UNION ALL
SELECT 'Reseñas con confirmación del responsable' AS comprobacion, (SELECT COUNT(*) FROM Resenias re WHERE NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos p JOIN Cursos c ON c.id=p.idCurso JOIN Alumnos a ON a.id=p.idAlumno WHERE p.idEstado=3 AND c.idInstitucion=re.idInstitucion AND (EXISTS (SELECT 1 FROM ResponsablesPorAlumno ra WHERE ra.idAlumno=a.id AND ra.idResponsable=re.idResponsable) OR EXISTS (SELECT 1 FROM Responsables r WHERE r.id=re.idResponsable AND r.idPersona=a.idPersona)))) AS revisar
UNION ALL
SELECT 'Institución inactiva sin pendientes ni aceptadas' AS comprobacion, (SELECT COUNT(*) FROM PostulacionesAlumnos p JOIN Cursos c ON c.id=p.idCurso JOIN Instituciones i ON i.id=c.idInstitucion WHERE i.estado=0 AND p.idEstado IN (1,2)) AS revisar
UNION ALL
SELECT 'Motivos de rechazo completos' AS comprobacion, (SELECT (SELECT COUNT(*) FROM PostulacionesAlumnos WHERE (idEstado=4 AND COALESCE(TRIM(motivoRechazo),'')='') OR (idEstado<>4 AND motivoRechazo IS NOT NULL))+(SELECT COUNT(*) FROM PostulacionesAdminInstitucion WHERE (idEstado=4 AND COALESCE(TRIM(motivoRechazo),'')='') OR (idEstado<>4 AND motivoRechazo IS NOT NULL))) AS revisar
UNION ALL
SELECT 'Tipos de usuario coinciden con responsables/admins' AS comprobacion, (SELECT (SELECT COUNT(*) FROM Responsables r JOIN Usuarios u ON u.id=r.idUsuario WHERE u.idTipoUsuario<>1)+(SELECT COUNT(*) FROM AdministradoresInstitucion a JOIN Usuarios u ON u.id=a.idUsuario WHERE u.idTipoUsuario<>2)) AS revisar
UNION ALL
SELECT 'Admin pendiente o rechazado no activado' AS comprobacion, (SELECT COUNT(*) FROM PostulacionesAdminInstitucion p JOIN AdministradoresInstitucion a ON a.id=p.idAdministradorInstitucion JOIN Usuarios u ON u.id=a.idUsuario WHERE p.idEstado IN (1,4) AND u.activo=1) AS revisar
UNION ALL
SELECT 'Instituciones creadas con aprobación administrativa' AS comprobacion, (SELECT COUNT(*) FROM Instituciones i WHERE NOT EXISTS (SELECT 1 FROM PostulacionesAdminInstitucion p JOIN AdministradoresInstitucion a ON a.id=p.idAdministradorInstitucion WHERE a.idInstitucion=i.id AND p.idEstado=3 AND p.cuitInstitucion=i.cuit AND p.nombreInstitucion=i.nombre)) AS revisar
UNION ALL
SELECT 'Teléfonos con un solo dueño y formato esperado' AS comprobacion, (SELECT COUNT(*) FROM Telefonos WHERE ((idInstitucion IS NOT NULL)+(idAdministrador IS NOT NULL)+(idResponsable IS NOT NULL))<>1 OR telefono NOT REGEXP '^[+]54 9 11 [0-9]{4}-[0-9]{4}$') AS revisar
UNION ALL
SELECT 'Mínimo un teléfono propio y máximo dos' AS comprobacion, (SELECT (SELECT COUNT(*) FROM Responsables r WHERE (SELECT COUNT(*) FROM Telefonos t WHERE t.idResponsable=r.id) NOT BETWEEN 1 AND 2)+(SELECT COUNT(*) FROM AdministradoresInstitucion a WHERE (SELECT COUNT(*) FROM Telefonos t WHERE t.idAdministrador=a.id) NOT BETWEEN 1 AND 2)+(SELECT COUNT(*) FROM Instituciones i WHERE (SELECT COUNT(*) FROM Telefonos t WHERE t.idInstitucion=i.id)>2)) AS revisar
UNION ALL
SELECT 'Rutas de imágenes con formato de la API' AS comprobacion, (SELECT COUNT(*) FROM Imagenes WHERE urlImagen NOT REGEXP '^imagenes-instituciones/[a-f0-9]{32}/imagen[.]png$') AS revisar
UNION ALL
SELECT 'Rutas de documentos con formato de la API' AS comprobacion, (SELECT COUNT(*) FROM DocumentosPostulacion WHERE ruta NOT REGEXP '^postulaciones-alumnos/[a-f0-9]{32}/documento-[0-9]+[.]pdf$') AS revisar
UNION ALL
SELECT 'Rutas administrativas con formato de la API' AS comprobacion, (SELECT COUNT(*) FROM PostulacionesAdminInstitucion WHERE rutaConstanciaFiscal NOT REGEXP '^postulaciones-admin/[a-f0-9]{32}/constancia-fiscal[.]pdf$' OR rutaHabilitacion NOT REGEXP '^postulaciones-admin/[a-f0-9]{32}/habilitacion[.]pdf$' OR rutaConstanciaLaboral NOT REGEXP '^postulaciones-admin/[a-f0-9]{32}/constancia-laboral[.]pdf$') AS revisar
UNION ALL
SELECT 'Horizonte tiene más de 25 cursos del ciclo siguiente' AS comprobacion, (SELECT (SELECT COUNT(*) FROM Cursos WHERE idInstitucion=1 AND cicloLectivo=YEAR(CURDATE())+1)<=25) AS revisar
UNION ALL
SELECT 'Horizonte tiene más de 25 reseñas' AS comprobacion, (SELECT (SELECT COUNT(*) FROM Resenias WHERE idInstitucion=1)<=25) AS revisar
UNION ALL
SELECT 'Laura tiene más de 10 alumnos' AS comprobacion, (SELECT (SELECT COUNT(*) FROM ResponsablesPorAlumno WHERE idResponsable=1)<=10) AS revisar
UNION ALL
SELECT 'Horizonte tiene más de 10 pendientes' AS comprobacion, (SELECT (SELECT COUNT(*) FROM PostulacionesAlumnos p JOIN Cursos c ON c.id=p.idCurso WHERE c.idInstitucion=1 AND p.idEstado=1)<=10) AS revisar
UNION ALL
SELECT 'Tres aceptaciones para la última vacante del curso 3' AS comprobacion, (SELECT NOT ((SELECT capacidad-inscriptos FROM Cursos WHERE id=3)=1 AND (SELECT COUNT(*) FROM PostulacionesAlumnos WHERE idCurso=3 AND idEstado=2)=3)) AS revisar
UNION ALL
SELECT 'Carla puede publicar su primera reseña en Horizonte' AS comprobacion, (SELECT EXISTS (SELECT 1 FROM Resenias WHERE idResponsable=6 AND idInstitucion=1) OR NOT EXISTS (SELECT 1 FROM PostulacionesAlumnos WHERE id=12 AND idEstado=3)) AS revisar
UNION ALL
SELECT 'Autogestora precargada sin partida' AS comprobacion, (SELECT (SELECT COUNT(*) FROM DocumentosPostulacion WHERE idPostulacionAlumnos IN (6,7) AND descripcion='Partida de nacimiento')<>0 OR (SELECT COUNT(*) FROM DocumentosPostulacion WHERE idPostulacionAlumnos IN (6,7))<>4) AS revisar
UNION ALL
SELECT 'Aprobado sin institución y usuario activo' AS comprobacion, (SELECT NOT EXISTS (SELECT 1 FROM AdministradoresInstitucion a JOIN Usuarios u ON u.id=a.idUsuario JOIN PostulacionesAdminInstitucion p ON p.idAdministradorInstitucion=a.id WHERE a.id=11 AND a.idInstitucion IS NULL AND u.activo=1 AND p.idEstado=3)) AS revisar
) controles;

-- Solo las confirmaciones se suman a la matrícula externa inicial.
SELECT c.id AS idCurso,c.inscriptos,b.matricula_previa,
       (SELECT COUNT(*) FROM PostulacionesAlumnos p WHERE p.idCurso=c.id AND p.idEstado=3) AS confirmadas,
       IF(c.inscriptos=b.matricula_previa+(SELECT COUNT(*) FROM PostulacionesAlumnos p WHERE p.idCurso=c.id AND p.idEstado=3),'OK','REVISAR') AS resultado
FROM Cursos c JOIN (
SELECT 1 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 2 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 3 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 4 AS idCurso, 5 AS matricula_previa
UNION ALL
SELECT 5 AS idCurso, 18 AS matricula_previa
UNION ALL
SELECT 6 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 7 AS idCurso, 3 AS matricula_previa
UNION ALL
SELECT 8 AS idCurso, 29 AS matricula_previa
UNION ALL
SELECT 9 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 10 AS idCurso, 3 AS matricula_previa
UNION ALL
SELECT 11 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 12 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 13 AS idCurso, 16 AS matricula_previa
UNION ALL
SELECT 14 AS idCurso, 4 AS matricula_previa
UNION ALL
SELECT 15 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 16 AS idCurso, 4 AS matricula_previa
UNION ALL
SELECT 17 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 30 AS idCurso, 20 AS matricula_previa
UNION ALL
SELECT 31 AS idCurso, 1 AS matricula_previa
UNION ALL
SELECT 32 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 33 AS idCurso, 3 AS matricula_previa
UNION ALL
SELECT 34 AS idCurso, 4 AS matricula_previa
UNION ALL
SELECT 35 AS idCurso, 5 AS matricula_previa
UNION ALL
SELECT 36 AS idCurso, 6 AS matricula_previa
UNION ALL
SELECT 37 AS idCurso, 7 AS matricula_previa
UNION ALL
SELECT 38 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 39 AS idCurso, 1 AS matricula_previa
UNION ALL
SELECT 40 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 41 AS idCurso, 3 AS matricula_previa
UNION ALL
SELECT 42 AS idCurso, 4 AS matricula_previa
UNION ALL
SELECT 43 AS idCurso, 25 AS matricula_previa
UNION ALL
SELECT 44 AS idCurso, 6 AS matricula_previa
UNION ALL
SELECT 45 AS idCurso, 7 AS matricula_previa
UNION ALL
SELECT 46 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 47 AS idCurso, 1 AS matricula_previa
UNION ALL
SELECT 48 AS idCurso, 2 AS matricula_previa
UNION ALL
SELECT 49 AS idCurso, 3 AS matricula_previa
UNION ALL
SELECT 50 AS idCurso, 4 AS matricula_previa
UNION ALL
SELECT 51 AS idCurso, 5 AS matricula_previa
UNION ALL
SELECT 52 AS idCurso, 6 AS matricula_previa
UNION ALL
SELECT 53 AS idCurso, 7 AS matricula_previa
UNION ALL
SELECT 54 AS idCurso, 0 AS matricula_previa
UNION ALL
SELECT 55 AS idCurso, 1 AS matricula_previa
) b ON b.idCurso=c.id ORDER BY c.id;

SELECT e.nombre, IF(r.ROUTINE_NAME IS NULL,'FALTA','OK') AS resultado
FROM (
SELECT 'sp_Registrar_Responsable' AS nombre
UNION ALL
SELECT 'sp_Registrar_Alumno' AS nombre
UNION ALL
SELECT 'sp_AdministradorInstitucion_Registrar' AS nombre
UNION ALL
SELECT 'sp_PostulacionAdmin_Aprobar' AS nombre
UNION ALL
SELECT 'sp_PostulacionAdmin_Rechazar' AS nombre
UNION ALL
SELECT 'sp_Institucion_Crear' AS nombre
UNION ALL
SELECT 'sp_Telefono_Agregar' AS nombre
UNION ALL
SELECT 'sp_Telefono_Eliminar' AS nombre
UNION ALL
SELECT 'sp_Idioma_Institucion_Agregar' AS nombre
UNION ALL
SELECT 'sp_Imagen_Institucion_Agregar' AS nombre
UNION ALL
SELECT 'sp_Institucion_Modificar' AS nombre
UNION ALL
SELECT 'sp_Idioma_Institucion_Eliminar' AS nombre
UNION ALL
SELECT 'sp_Imagen_Institucion_Eliminar' AS nombre
UNION ALL
SELECT 'sp_AdministradorInstitucion_ObtenerClaveReintento' AS nombre
UNION ALL
SELECT 'sp_Postulacion_ConsultarPorCodigo' AS nombre
UNION ALL
SELECT 'sp_Institucion_DarDeBaja' AS nombre
UNION ALL
SELECT 'sp_Institucion_Reactivar' AS nombre
UNION ALL
SELECT 'sp_AdministradorInstitucion_CrearAdicional' AS nombre
UNION ALL
SELECT 'sp_AdministradorInstitucion_DarDeBaja' AS nombre
UNION ALL
SELECT 'sp_AdministradorInstitucion_Reactivar' AS nombre
UNION ALL
SELECT 'sp_Usuario_ObtenerClave' AS nombre
UNION ALL
SELECT 'sp_Usuario_CambiarClave' AS nombre
UNION ALL
SELECT 'sp_Alumno_Eliminar' AS nombre
UNION ALL
SELECT 'sp_Validar_Usuario_Login' AS nombre
UNION ALL
SELECT 'sp_Registrar_Resenia' AS nombre
UNION ALL
SELECT 'sp_Responder_Resenia' AS nombre
UNION ALL
SELECT 'sp_Registrar_Postulacion_Alumno' AS nombre
UNION ALL
SELECT 'sp_Aceptar_Postulacion_Alumno' AS nombre
UNION ALL
SELECT 'sp_Rechazar_Postulacion_Alumno' AS nombre
UNION ALL
SELECT 'sp_Confirmar_Postulacion_Alumno' AS nombre
UNION ALL
SELECT 'sp_Registrar_Curso' AS nombre
UNION ALL
SELECT 'sp_Cancelar_Postulacion_Alumno' AS nombre
UNION ALL
SELECT 'sp_postulaciones_Por_Responsable' AS nombre
UNION ALL
SELECT 'sp_alumnos_Por_Responsable' AS nombre
UNION ALL
SELECT 'sp_Olvide_Clave' AS nombre
UNION ALL
SELECT 'sp_Actualizar_Clave' AS nombre
UNION ALL
SELECT 'sp_Telefono_Institucion_Agregar' AS nombre
UNION ALL
SELECT 'sp_Telefono_Institucion_Eliminar' AS nombre
) e LEFT JOIN information_schema.ROUTINES r ON r.ROUTINE_SCHEMA=DATABASE() AND r.ROUTINE_TYPE='PROCEDURE' AND r.ROUTINE_NAME=e.nombre;

-- Todas las vistas deben poder consultarse sin errores.
SELECT 'vw_Responsables_Con_Alumnos' AS vista, COUNT(*) AS filas FROM vw_Responsables_Con_Alumnos
UNION ALL
SELECT 'vw_Valoraciones_Por_Institucion' AS vista, COUNT(*) AS filas FROM vw_Valoraciones_Por_Institucion
UNION ALL
SELECT 'vw_Postulaciones_Admin_Institucion' AS vista, COUNT(*) AS filas FROM vw_Postulaciones_Admin_Institucion
UNION ALL
SELECT 'vw_Postulaciones_Alumnos' AS vista, COUNT(*) AS filas FROM vw_Postulaciones_Alumnos
UNION ALL
SELECT 'vw_Postulaciones_Por_Responsable' AS vista, COUNT(*) AS filas FROM vw_Postulaciones_Por_Responsable
UNION ALL
SELECT 'vw_Instituciones' AS vista, COUNT(*) AS filas FROM vw_Instituciones
UNION ALL
SELECT 'vw_Administradores_Por_Institucion' AS vista, COUNT(*) AS filas FROM vw_Administradores_Por_Institucion
UNION ALL
SELECT 'vw_Perfil_Usuario' AS vista, COUNT(*) AS filas FROM vw_Perfil_Usuario
UNION ALL
SELECT 'vw_Telefonos_Por_Usuario' AS vista, COUNT(*) AS filas FROM vw_Telefonos_Por_Usuario
UNION ALL
SELECT 'vw_Cursos' AS vista, COUNT(*) AS filas FROM vw_Cursos;
