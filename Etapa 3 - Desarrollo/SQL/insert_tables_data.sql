-- =====================================================================
-- InstiRed - Datos de prueba (SOLO DEV, no cargar en PRD)
--
-- Correr UNA vez, sobre la base DEV vacia, despues de create_tables.sql.
--
-- REQUISITO: en create_tables.sql, Cursos.cicloLectivo debe ser SMALLINT
-- (con TINYINT no entra 2026). Si la base ya existe:
--   ALTER TABLE Cursos MODIFY cicloLectivo SMALLINT NOT NULL;
--
-- Todos los usuarios tienen clave 'test1234' (texto plano, temporal).
-- Los ids van explicitos para que las referencias se puedan leer a ojo.
-- Los telefonos, emails y URLs son ficticios.
-- =====================================================================

-- Los datos llevan acentos y enie: esto hace que se guarden bien aunque el
-- cliente que corra el script no use utf8mb4 por defecto.
SET NAMES utf8mb4;

-- ---------------------------------------------------------------------
-- 1. Catalogos
-- ---------------------------------------------------------------------

INSERT INTO TipoUsuario (id, descripcion) VALUES
  (1, 'Responsable'),
  (2, 'Administrador'),
  (3, 'DevCo');

INSERT INTO Roles (id, descripcion) VALUES
  (1, 'Padre'),
  (2, 'Madre'),
  (3, 'Tutor legal'),
  (4, 'Familiar a cargo');

INSERT INTO TipoInstitucion (id, tipoInstitucion) VALUES
  (1, 'Jardín'),
  (2, 'Primaria'),
  (3, 'Secundaria'),
  (4, 'Instituto de idiomas');

INSERT INTO Idiomas (id, idioma) VALUES
  (1, 'Español'),
  (2, 'Inglés'),
  (3, 'Portugués'),
  (4, 'Francés'),
  (5, 'Italiano');

INSERT INTO Turnos (id, descripcion) VALUES
  (1, 'Mañana'),
  (2, 'Tarde'),
  (3, 'Noche'),
  (4, 'Jornada completa');

INSERT INTO EstadosPostulacion (id, descripcion) VALUES
  (1, 'Pendiente'),
  (2, 'En revisión'),
  (3, 'Aceptada'),
  (4, 'Rechazada'),
  (5, 'Lista de espera');

-- ---------------------------------------------------------------------
-- 2. Personas (31)
--    1-10  responsables (el 6 tambien es alumno; el 7 tambien administra)
--    11-19 administradores
--    20-31 alumnos menores
-- ---------------------------------------------------------------------

INSERT INTO Personas (id, nombre, apellido, documento, domicilio, fechaNacimiento) VALUES
  (1,  'Laura',    'Benítez',    '33456872', 'Calle Italia 450, General Pacheco',      '1987-02-11'),
  (2,  'Carlos',   'Benítez',    '31892345', 'Calle Italia 450, General Pacheco',      '1985-09-23'),
  (3,  'Andrea',   'Ruiz',       '32654108', 'Av. Sarmiento 1820, Tigre',              '1986-04-05'),
  (4,  'Pablo',    'Fernández',  '28123907', 'Calle Los Ceibos 735, Don Torcuato',     '1980-12-17'),
  (5,  'Marcela',  'Domínguez',  '35478216', 'Calle Mitre 288, Garín',                 '1990-07-30'),
  (6,  'Marcos',   'Acosta',     '36215874', 'Calle Lavalle 1105, General Pacheco',    '1991-08-12'),
  (7,  'Gabriela', 'Torres',     '34087532', 'Av. Libertador 2740, Boulogne',          '1988-03-19'),
  (8,  'Rosa',     'Silva',      '13245790', 'Calle Alvear 96, Benavídez',             '1957-10-08'),
  (9,  'Julián',   'Gómez',      '34912063', 'Calle Rivadavia 640, Escobar',           '1989-01-26'),
  (10, 'Valeria',  'Castro',     '35330481', 'Calle Rivadavia 640, Escobar',           '1990-05-14'),

  (11, 'Silvia',   'Maidana',    '22876540', 'Calle San Martín 310, General Pacheco',  '1972-06-02'),
  (12, 'Roberto',  'Salinas',    '21934107', 'Calle Colón 785, Tigre',                 '1970-11-21'),
  (13, 'Mónica',   'Peralta',    '24561893', 'Calle Urquiza 1450, Don Torcuato',       '1975-02-28'),
  (14, 'Natalia',  'Ocampo',     '30441275', 'Calle Pellegrini 220, San Fernando',     '1983-09-09'),
  (15, 'Daniel',   'Quiroga',    '26705319', 'Calle Paraná 905, Boulogne',             '1978-08-15'),
  (16, 'Verónica', 'Luna',       '28998604', 'Calle Dorrego 512, Benavídez',           '1981-04-04'),
  (17, 'Héctor',   'Giménez',    '20187462', 'Calle Moreno 77, Escobar',               '1968-12-12'),
  (18, 'Lorena',   'Sosa',       '30972158', 'Calle Las Heras 1330, Del Viso',         '1984-03-03'),
  (19, 'Alberto',  'Rojas',      '23640897', 'Calle Güemes 468, General Pacheco',      '1974-07-27'),

  (20, 'Sofía',    'Benítez',    '57412086', 'Calle Italia 450, General Pacheco',      '2022-03-14'),
  (21, 'Mateo',    'Benítez',    '56730915', 'Calle Italia 450, General Pacheco',      '2021-07-02'),
  (22, 'Valentina','Ruiz',       '56084327', 'Av. Sarmiento 1820, Tigre',              '2020-05-21'),
  (23, 'Joaquín',  'Ruiz',       '54218603', 'Av. Sarmiento 1820, Tigre',              '2017-09-09'),
  (24, 'Camila',   'Fernández',  '52561478', 'Calle Los Ceibos 735, Don Torcuato',     '2014-02-27'),
  (25, 'Tomás',    'Fernández',  '50934762', 'Calle Los Ceibos 735, Don Torcuato',     '2012-11-30'),
  (26, 'Lucas',    'Domínguez',  '54967120', 'Calle Mitre 288, Garín',                 '2018-04-18'),
  (27, 'Emma',     'Acosta',     '59103846', 'Calle Lavalle 1105, General Pacheco',    '2024-01-25'),
  (28, 'Benjamín', 'Torres',     '55402981', 'Av. Libertador 2740, Boulogne',          '2019-06-06'),
  (29, 'Martina',  'Silva',      '51027384', 'Calle Alvear 96, Benavídez',             '2012-10-03'),
  (30, 'Santino',  'Gómez',      '56812074', 'Calle Rivadavia 640, Escobar',           '2021-04-15'),
  (31, 'Delfina',  'Gómez',      '59460213', 'Calle Rivadavia 640, Escobar',           '2024-09-17');

-- ---------------------------------------------------------------------
-- 3. Usuarios (20): 1-10 responsables, 11-20 administradores
--    Gabriela (persona 7) tiene dos cuentas: una como responsable (7)
--    y otra como administradora (14).
-- ---------------------------------------------------------------------

INSERT INTO Usuarios (id, usuario, clave, activo, idTipoUsuario) VALUES
  (1,  'lbenitez',    'test1234', TRUE,  1),
  (2,  'cbenitez',    'test1234', TRUE,  1),
  (3,  'aruiz',       'test1234', TRUE,  1),
  (4,  'pfernandez',  'test1234', TRUE,  1),
  (5,  'mdominguez',  'test1234', TRUE,  1),
  (6,  'macosta',     'test1234', TRUE,  1),
  (7,  'gtorres',     'test1234', TRUE,  2),
  (8,  'rsilva',      'test1234', FALSE, 2),
  (9,  'jgomez',      'test1234', TRUE,  2),
  (10, 'vcastro',     'test1234', TRUE,  2),
  (11, 'smaidana',    'test1234', TRUE,  2),
  (12, 'rsalinas',    'test1234', TRUE,  2),
  (13, 'mperalta',    'test1234', TRUE,  3),
  (14, 'gtorres.adm', 'test1234', TRUE,  3),
  (15, 'nocampo',     'test1234', TRUE,  3),
  (16, 'dquiroga',    'test1234', TRUE,  3),
  (17, 'vluna',       'test1234', TRUE,  3),
  (18, 'hgimenez',    'test1234', FALSE, 3),
  (19, 'lsosa',       'test1234', TRUE,  1),
  (20, 'arojas',      'test1234', TRUE,  2);

-- ---------------------------------------------------------------------
-- 4. Instituciones (10) - zona norte del GBA
-- ---------------------------------------------------------------------

INSERT INTO Instituciones (id, nombre, cuit, direccion, email, latitud, longitud, idTipoInstitucion) VALUES
  (1,  'Jardín de Infantes N° 902 Arco Iris',      '30710000014', 'Av. Hipólito Yrigoyen 1250, Gral. Pacheco', 'jardin902@instired.test',                  -34.4586, -58.6431, 1),
  (2,  'Escuela Primaria N° 14 Domingo F. Sarmiento','30710000022','Calle Cazón 456, Tigre',                      'primaria14@instired.test',                -34.4264, -58.5791, 2),
  (3,  'Escuela Secundaria N° 6 Manuel Belgrano',  '30710000030', 'Av. Constituyentes 2300, Don Torcuato',    'secundaria6@instired.test',               -34.4999, -58.6169, 3),
  (4,  'Instituto de Idiomas Lingua Norte',        '30710000049', 'Av. San Martín 800, Garín',                'info@linguanorte.instired.test',          -34.4256, -58.7447, 4),
  (5,  'Jardín Maternal Pequeños Pasos',           '30710000057', 'Calle Moreno 345, San Fernando',           'maternal@pequenospasos.instired.test',    -34.4421, -58.5594, 1),
  (6,  'Escuela Primaria Bilingüe St. Patrick',    '30710000065', 'Av. del Libertador 3100, Boulogne',        'admisiones@stpatrick.instired.test',      -34.4950, -58.5550, 2),
  (7,  'Instituto Técnico San Martín',             '30710000073', 'Colectora Panamericana 4200, Benavídez',   'secretaria@itsanmartin.instired.test',    -34.4097, -58.6857, 3),
  (8,  'Escuela Primaria N° 31 Juana Manso',       '30710000081', 'Calle Belgrano 120, Escobar',              'primaria31@instired.test',                -34.3496, -58.7918, 2),
  (9,  'Jardín de Infantes N° 915 Mundo Mágico',   '30710000103', 'Av. Eva Perón 980, Del Viso',              'jardin915@instired.test',                 -34.4559, -58.7992, 1),
  (10, 'Escuela Secundaria para Adultos N° 703',   '30710000111', 'Av. Hipólito Yrigoyen 1800, Gral. Pacheco','adultos703@instired.test',                -34.4540, -58.6380, 3);

-- Discapacidades: 1 fila por institucion (TRUE = la institucion cuenta con
-- acceso o apoyo para esa condicion). El id coincide con Instituciones.id.
INSERT INTO Discapacidades (idInstitucion, movilidad, visual, auditiva, intelectual, sindromeDown, autismo) VALUES
  (1,  TRUE,  FALSE, FALSE, FALSE, FALSE, FALSE),
  (2,  TRUE,  TRUE,  TRUE,  TRUE,  TRUE,  TRUE),
  (3,  TRUE,  FALSE, TRUE,  FALSE, FALSE, FALSE),
  (4,  FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
  (5,  TRUE,  FALSE, FALSE, FALSE, FALSE, FALSE),
  (6,  TRUE,  FALSE, FALSE, TRUE,  FALSE, TRUE),
  (7,  TRUE,  FALSE, FALSE, FALSE, FALSE, FALSE),
  (8,  FALSE, FALSE, FALSE, FALSE, FALSE, FALSE),
  (9,  TRUE,  FALSE, FALSE, TRUE,  TRUE,  TRUE),
  (10, TRUE,  TRUE,  TRUE,  FALSE, FALSE, FALSE);

INSERT INTO IdiomaInstitucion (idInstitucion, idIdioma) VALUES
  (1, 1), (2, 1), (3, 1), (3, 2),
  (4, 2), (4, 3), (4, 4), (4, 5),
  (5, 1), (6, 1), (6, 2), (7, 1), (7, 2),
  (8, 1), (9, 1), (10, 1);

-- ---------------------------------------------------------------------
-- 5. Cursos (14). inscriptos arranca en 0 y se recalcula al final.
--    Los cursos 12 y 13 son del ciclo 2027 (postulaciones en curso).
-- ---------------------------------------------------------------------

INSERT INTO NivelesEscolares (id, descripcion) VALUES
  (1, 'Preescolar'),
  (2, 'Primaria'),
  (3, 'Secundaria');

INSERT INTO Cursos (id, idInstitucion, idTurno, cicloLectivo, idNivel, anio, nivelPersonalizado, division, capacidad, inscriptos) VALUES
  (1,  1,  1, 2026, 1, NULL, 'Sala de 4',        'Azul',              20, 0),
  (2,  1,  2, 2026, 1, NULL, 'Sala de 5',        'Roja',              22, 0),
  (3,  2,  1, 2026, 1, 1,    NULL,               'A',                 28, 0),
  (4,  2,  2, 2026, 1, 4,    NULL,               'B',                 28, 0),
  (5,  3,  1, 2026, 2, 1,    NULL,               'A',                 35, 0),
  (6,  3,  2, 2026, 2, 3,    NULL,               'C',                 35, 0),
  (7,  4,  2, 2026, 2, NULL, 'Kids 2',           '1',                 15, 0),
  (8,  4,  3, 2026, 2, NULL, 'Adults Beginner',  'Lunes y Miércoles', 12, 0),
  (9,  5,  4, 2026, 3, NULL, 'Sala de 2',        'Amarilla',          12, 0),
  (10, 6,  4, 2026, 3, 2,    '2nd Grade',        'A',                 24, 0),
  (11, 7,  1, 2026, 3, 3,    NULL,               'B',                 30, 0),
  (12, 8,  1, 2027, 3, 1,    NULL,               'A',                 26, 0),
  (13, 9,  2, 2027, 1, NULL, 'Sala de 3',        'Verde',             18, 0),
  (14, 10, 3, 2026, 2, 1,    NULL,               '1',                 25, 0);

INSERT INTO Imagenes (id, idInstitucion, urlImagen, descripcion) VALUES
  (1,  1,  'https://instired.test/img/inst1/fachada.jpg',      'Fachada del jardín'),
  (2,  1,  'https://instired.test/img/inst1/patio.jpg',        'Patio de juegos'),
  (3,  2,  'https://instired.test/img/inst2/fachada.jpg',      'Frente de la escuela'),
  (4,  2,  'https://instired.test/img/inst2/aula.jpg',         'Aula de primer grado'),
  (5,  3,  'https://instired.test/img/inst3/fachada.jpg',      'Entrada principal'),
  (6,  3,  'https://instired.test/img/inst3/laboratorio.jpg',  'Laboratorio de ciencias'),
  (7,  4,  'https://instired.test/img/inst4/fachada.jpg',      'Sede del instituto'),
  (8,  5,  'https://instired.test/img/inst5/sala.jpg',         'Sala de 2'),
  (9,  6,  'https://instired.test/img/inst6/fachada.jpg',      'Fachada de la escuela'),
  (10, 6,  'https://instired.test/img/inst6/biblioteca.jpg',   'Biblioteca bilingüe'),
  (11, 7,  'https://instired.test/img/inst7/taller.jpg',       'Taller de electrónica'),
  (12, 8,  'https://instired.test/img/inst8/fachada.jpg',      'Frente de la escuela'),
  (13, 9,  'https://instired.test/img/inst9/patio.jpg',        'Patio del jardín'),
  (14, 10, 'https://instired.test/img/inst10/fachada.jpg',     'Ingreso al turno noche');

-- ---------------------------------------------------------------------
-- 6. Alumnos (13), Responsables (10), Administradores (10)
--    Alumno 8 es Marcos (adulto, secundaria nocturna) y a la vez
--    responsable de Emma. Alumnos 12 y 13 todavia no tienen institucion.
-- ---------------------------------------------------------------------

INSERT INTO Alumnos (id, idPersona, idInstitucion, idCurso) VALUES
  (1,  20, 1,    1),
  (2,  21, 1,    2),
  (3,  22, 2,    3),
  (4,  23, 2,    4),
  (5,  24, 3,    5),
  (6,  25, 3,    6),
  (7,  26, 4,    7),
  (8,  6,  10,   14),
  (9,  27, 5,    9),
  (10, 28, 6,    10),
  (11, 29, 7,    11),
  (12, 30, NULL, NULL),
  (13, 31, NULL, NULL);

INSERT INTO Responsables (id, idPersona, idUsuario) VALUES
  (1, 1, 1), (2, 2, 2), (3, 3, 3), (4, 4, 4), (5, 5, 5),
  (6, 6, 6), (7, 7, 7), (8, 8, 8), (9, 9, 9), (10, 10, 10);

INSERT INTO AdministradoresInstitucion (id, idPersona, idUsuario, idInstitucion) VALUES
  (1,  11, 11, 1),
  (2,  12, 12, 2),
  (3,  13, 13, 3),
  (4,  7,  14, 4),
  (5,  14, 15, 5),
  (6,  15, 16, 6),
  (7,  16, 17, 7),
  (8,  17, 18, 8),
  (9,  18, 19, 9),
  (10, 19, 20, 10);

-- Roles: 1 Padre, 2 Madre, 3 Tutor legal, 4 Familiar a cargo.
-- Andrea (responsable 3) es madre de los alumnos 3 y 4 y tutora legal del 11.
INSERT INTO ResponsablesPorAlumno (id, idResponsable, idAlumno, idRol) VALUES
  (1,  1,  1,  2),
  (2,  1,  2,  2),
  (3,  2,  1,  1),
  (4,  2,  2,  1),
  (5,  3,  3,  2),
  (6,  3,  4,  2),
  (7,  4,  5,  1),
  (8,  4,  6,  1),
  (9,  5,  7,  2),
  (10, 6,  9,  1),
  (11, 7,  10, 2),
  (12, 8,  11, 4),
  (13, 3,  11, 3),
  (14, 9,  12, 1),
  (15, 9,  13, 1),
  (16, 10, 12, 2),
  (17, 10, 13, 2);

-- ---------------------------------------------------------------------
-- 7. Postulaciones (15)
--    Estados: 1 Pendiente, 2 En revision, 3 Aceptada, 4 Rechazada,
--    5 Lista de espera. Cada alumno con curso asignado tiene su
--    postulacion aceptada a ese mismo curso.
-- ---------------------------------------------------------------------

INSERT INTO Postulaciones (id, fecha, codigoSeguimiento, idCurso, idAlumno, idEstado) VALUES
  (1,  '2025-11-12', 'PST-20251112-0001', 1,  1,  3),
  (2,  '2025-11-12', 'PST-20251112-0002', 2,  2,  3),
  (3,  '2025-11-20', 'PST-20251120-0003', 3,  3,  3),
  (4,  '2025-11-20', 'PST-20251120-0004', 4,  4,  3),
  (5,  '2025-12-03', 'PST-20251203-0005', 5,  5,  3),
  (6,  '2025-12-03', 'PST-20251203-0006', 6,  6,  3),
  (7,  '2026-02-10', 'PST-20260210-0007', 7,  7,  3),
  (8,  '2026-02-16', 'PST-20260216-0008', 14, 8,  3),
  (9,  '2025-12-15', 'PST-20251215-0009', 9,  9,  3),
  (10, '2025-11-28', 'PST-20251128-0010', 10, 10, 3),
  (11, '2025-12-09', 'PST-20251209-0011', 11, 11, 3),
  (12, '2026-10-02', 'PST-20261002-0012', 12, 12, 2),
  (13, '2026-10-05', 'PST-20261005-0013', 13, 13, 1),
  (14, '2026-09-18', 'PST-20260918-0014', 11, 6,  4),
  (15, '2026-10-07', 'PST-20261007-0015', 13, 9,  5);

-- ---------------------------------------------------------------------
-- 8. Resenias (9): cada una la escribe un responsable con un alumno
--    en esa institucion. Calificacion de 1 a 5.
-- ---------------------------------------------------------------------

INSERT INTO Resenias (id, calificacion, comentario, fecha, idInstitucion, idResponsable) VALUES
  (1, 5, 'Las seños son un amor y mis hijos van felices todos los días. Muy buena comunicación con las familias.', '2026-04-10', 1, 1),
  (2, 4, 'Excelente propuesta pedagógica. Solo mejoraría el horario de ingreso, que es un poco temprano.',          '2026-05-02', 1, 2),
  (3, 5, 'Muy contenedores con los chicos. Valentina se adaptó enseguida y la directora siempre está disponible.', '2026-04-22', 2, 3),
  (4, 4, 'Buen nivel académico y docentes comprometidos. Instalaciones algo antiguas pero bien cuidadas.',         '2026-06-15', 3, 4),
  (5, 3, 'Los profesores son buenos, pero los grupos son grandes y a veces se cancelan clases sin aviso.',         '2026-07-01', 4, 5),
  (6, 5, 'El mejor lugar para dejar a mi hija. Personal atento, sala luminosa y mucha comunicación diaria.',      '2026-03-12', 5, 6),
  (7, 5, 'Muy buena propuesta bilingüe. Benjamín disfruta las clases en inglés y la infraestructura es excelente.','2026-05-20', 6, 7),
  (8, 2, 'Falta comunicación con las familias y los reclamos tardan mucho en resolverse.',                        '2026-08-05', 7, 8),
  (9, 4, 'Buenos talleres y orientación técnica. Martina está contenta, aunque llegar en transporte es complicado.','2026-08-19', 7, 3);

-- ---------------------------------------------------------------------
-- 9. Telefonos (16). Un telefono puede tener mas de un dueno:
--    el 4 es de la institucion 4 y del administrador 4; el 16 es el
--    celular de Gabriela como administradora 4 y como responsable 7.
-- ---------------------------------------------------------------------

INSERT INTO Telefonos (id, idInstitucion, idAdministrador, idResponsable, telefono) VALUES
  (1,  1,    NULL, NULL, '+54 11 5555-0101'),
  (2,  2,    NULL, NULL, '+54 11 5555-0102'),
  (3,  3,    NULL, NULL, '+54 11 5555-0103'),
  (4,  4,    4,    NULL, '+54 11 5555-0104'),
  (5,  5,    NULL, NULL, '+54 11 5555-0105'),
  (6,  6,    NULL, NULL, '+54 11 5555-0106'),
  (7,  7,    NULL, NULL, '+54 11 5555-0107'),
  (8,  8,    NULL, NULL, '+54 11 5555-0108'),
  (9,  9,    NULL, NULL, '+54 11 5555-0109'),
  (10, 10,   NULL, NULL, '+54 11 5555-0110'),
  (11, NULL, NULL, 1,    '+54 9 11 5555-1001'),
  (12, NULL, NULL, 2,    '+54 9 11 5555-1002'),
  (13, NULL, NULL, 3,    '+54 9 11 5555-1003'),
  (14, NULL, NULL, 9,    '+54 9 11 5555-1009'),
  (15, NULL, 1,    NULL, '+54 9 11 5555-2001'),
  (16, NULL, 4,    7,    '+54 9 11 5555-1007');

-- ---------------------------------------------------------------------
-- 10. inscriptos = alumnos realmente asignados a cada curso
-- ---------------------------------------------------------------------
SET SQL_SAFE_UPDATES = 0;
UPDATE Cursos c
SET c.inscriptos = (SELECT COUNT(*) FROM Alumnos a WHERE a.idCurso = c.id);
SET SQL_SAFE_UPDATES = 1;