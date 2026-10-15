CREATE TABLE TipoUsuario (
  id TINYINT NOT NULL AUTO_INCREMENT, 
  descripcion VARCHAR(50) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE Roles (
  id TINYINT NOT NULL AUTO_INCREMENT, 
  descripcion VARCHAR(50) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE TipoInstitucion (
  id TINYINT NOT NULL AUTO_INCREMENT, 
  tipoInstitucion VARCHAR(20) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE Idiomas (
  id TINYINT NOT NULL AUTO_INCREMENT, 
  idioma VARCHAR(20) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE Turnos (
  id INT NOT NULL AUTO_INCREMENT, 
  descripcion VARCHAR(20) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE EstadosPostulacion (
  id INT NOT NULL AUTO_INCREMENT, 
  descripcion VARCHAR(30) NOT NULL, 
  PRIMARY KEY (id)
);

CREATE TABLE Personas (
  id INT NOT NULL AUTO_INCREMENT, 
  nombre VARCHAR(50) NOT NULL, 
  apellido VARCHAR(50) NOT NULL, 
  documento VARCHAR(8) NOT NULL, 
  domicilio VARCHAR(100) NOT NULL, 
  fechaNacimiento DATE NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_personas_documento (documento)
);

CREATE TABLE Usuarios (
  id INT NOT NULL AUTO_INCREMENT, 
  usuario VARCHAR(50) NOT NULL, 
  clave VARCHAR(255) NOT NULL, 
  activo BOOLEAN NOT NULL DEFAULT TRUE, 
  idTipoUsuario TINYINT NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_usuarios_usuario (usuario), 
  CONSTRAINT fk_usuarios_tipousuario FOREIGN KEY (idTipoUsuario) REFERENCES TipoUsuario (id)
);

CREATE TABLE Instituciones (
  id INT NOT NULL AUTO_INCREMENT, 
  nombre VARCHAR(100) NOT NULL, 
  cuit VARCHAR(11) NOT NULL, 
  direccion VARCHAR(50) NOT NULL, 
  email VARCHAR(50) NOT NULL, 
  latitud DOUBLE NOT NULL, 
  longitud DOUBLE NOT NULL, 
  idTipoInstitucion TINYINT NOT NULL, 
  PRIMARY KEY (id), 
  CONSTRAINT fk_instituciones_tipoinstitucion FOREIGN KEY (idTipoInstitucion) REFERENCES TipoInstitucion (id)
);

CREATE TABLE Discapacidades ( #Hecho aparte de la tabla "Instituciones" por ordenar. En teoria, los campos de esta tabla podrian ir dentro de Instituciones.
  idInstitucion INT NOT NULL AUTO_INCREMENT,
  movilidad BOOL NOT NULL,
  visual BOOL NOT NULL,
  auditiva BOOL NOT NULL,
  intelectual BOOL NOT NULL,
  sindromeDown BOOL NOT NULL,
  autismo BOOL NOT NULL,
  PRIMARY KEY (idInstitucion)
);

CREATE TABLE IdiomaInstitucion (
  idInstitucion INT NOT NULL, 
  idIdioma TINYINT NOT NULL, 
  PRIMARY KEY (idInstitucion, idIdioma), 
  CONSTRAINT fk_idiomainstitucion_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id), 
  CONSTRAINT fk_idiomainstitucion_idioma FOREIGN KEY (idIdioma) REFERENCES Idiomas (id)
);

CREATE TABLE NivelesEscolares (
  id INT NOT NULL AUTO_INCREMENT, 
  descripcion VARCHAR(30) NOT NULL,
  PRIMARY KEY (id)
);

CREATE TABLE Cursos (
  id INT NOT NULL AUTO_INCREMENT, 
  idInstitucion INT NOT NULL, 
  idTurno INT NOT NULL, 
  cicloLectivo INT NOT NULL, 					#ANIO CALENDARIO: 2026, 2007, 1998
  idNivel INT NULL,								#EJ: 1 si es preescolar, 2 si es primaria, 3 si es secundaria
  anio TINYINT NULL,							#ANIO ESTANDAR ESCOLAR, PARA PRE, PRIMARIA Y SEC: 1er grado, 2do grado, 1er año, 3er año. preescolar.
  nivelPersonalizado VARCHAR(30) NULL, 			#TAG CUSTOM EN CASO DE NO SER ESTANDAR: 2nd Children, 1° grado, 4to año.
  division VARCHAR(50) NOT NULL, 				#SI SOLO HAY 1, SE COMPLETA CON 1: 4ta division, salita azul, A B C, Lunes y Miercoles, martes y jueves
  capacidad TINYINT NOT NULL, 
  inscriptos TINYINT NOT NULL DEFAULT 0, 
  PRIMARY KEY (id), 
  CONSTRAINT fk_cursos_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id), 
  CONSTRAINT fk_cursos_turno FOREIGN KEY (idTurno) REFERENCES Turnos (id), 
  CONSTRAINT fk_niveles_escolares FOREIGN KEY (idNivel) REFERENCES NivelesEscolares (id), 
  CONSTRAINT ck_cursos_inscriptos CHECK (inscriptos <= capacidad)
);

CREATE TABLE Imagenes (
  id INT NOT NULL AUTO_INCREMENT, 
  idInstitucion INT NOT NULL, 
  urlImagen VARCHAR(255) NOT NULL, 
  descripcion VARCHAR(255) NOT NULL, 
  PRIMARY KEY (id), 
  CONSTRAINT fk_imagenes_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id)
);

CREATE TABLE Alumnos (
  id INT NOT NULL AUTO_INCREMENT, 
  idPersona INT NOT NULL, 
  idInstitucion INT NULL, 
  idCurso INT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_alumnos_persona (idPersona), 
  CONSTRAINT fk_alumnos_persona FOREIGN KEY (idPersona) REFERENCES Personas (id), 
  CONSTRAINT fk_alumnos_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id), 
  CONSTRAINT fk_alumnos_curso FOREIGN KEY (idCurso) REFERENCES Cursos (id)
);

CREATE TABLE Responsables (
  id INT NOT NULL AUTO_INCREMENT, 
  idPersona INT NOT NULL, 
  idUsuario INT NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_responsables_persona (idPersona), 
  UNIQUE KEY uq_responsables_usuario (idUsuario), 
  CONSTRAINT fk_responsables_persona FOREIGN KEY (idPersona) REFERENCES Personas (id), 
  CONSTRAINT fk_responsables_usuario FOREIGN KEY (idUsuario) REFERENCES Usuarios (id)
);

CREATE TABLE AdministradoresInstitucion (
  id INT NOT NULL AUTO_INCREMENT, 
  idPersona INT NOT NULL, 
  idUsuario INT NOT NULL, 
  idInstitucion INT NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_administradores_persona (idPersona), 
  UNIQUE KEY uq_administradores_usuario (idUsuario), 
  CONSTRAINT fk_administradores_persona FOREIGN KEY (idPersona) REFERENCES Personas (id), 
  CONSTRAINT fk_administradores_usuario FOREIGN KEY (idUsuario) REFERENCES Usuarios (id), 
  CONSTRAINT fk_administradores_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id)
);

CREATE TABLE ResponsablesPorAlumno (
  id INT NOT NULL AUTO_INCREMENT, 
  idResponsable INT NOT NULL, 
  idAlumno INT NOT NULL, 
  idRol TINYINT NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_responsablesporalumno_par (idResponsable, idAlumno), 
  CONSTRAINT fk_rpa_responsable FOREIGN KEY (idResponsable) REFERENCES Responsables (id), 
  CONSTRAINT fk_rpa_alumno FOREIGN KEY (idAlumno) REFERENCES Alumnos (id), 
  CONSTRAINT fk_rpa_rol FOREIGN KEY (idRol) REFERENCES Roles (id)
);

CREATE TABLE Postulaciones (
  id INT NOT NULL AUTO_INCREMENT, 
  fecha DATE NOT NULL, 
  codigoSeguimiento VARCHAR(30) NOT NULL, 
  idCurso INT NOT NULL, 
  idAlumno INT NOT NULL, 
  idEstado INT NOT NULL, 
  PRIMARY KEY (id), 
  UNIQUE KEY uq_postulaciones_codigo (codigoSeguimiento), 
  CONSTRAINT fk_postulaciones_curso FOREIGN KEY (idCurso) REFERENCES Cursos (id), 
  CONSTRAINT fk_postulaciones_alumno FOREIGN KEY (idAlumno) REFERENCES Alumnos (id), 
  CONSTRAINT fk_postulaciones_estado FOREIGN KEY (idEstado) REFERENCES EstadosPostulacion (id)
);

CREATE TABLE Resenias (
  id INT NOT NULL AUTO_INCREMENT, 
  calificacion TINYINT NOT NULL, 
  comentario VARCHAR(255) NOT NULL, 
  fecha DATE NOT NULL, 
  idInstitucion INT NOT NULL, 
  idResponsable INT NOT NULL, 
  PRIMARY KEY (id), 
  CONSTRAINT fk_resenias_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id), 
  CONSTRAINT fk_resenias_responsable FOREIGN KEY (idResponsable) REFERENCES Responsables (id), 
  CONSTRAINT ck_resenias_calificacion CHECK (calificacion BETWEEN 1 AND 5)
);


CREATE TABLE Telefonos (
  id INT NOT NULL AUTO_INCREMENT, 
  idInstitucion INT NULL, 
  idAdministrador INT NULL, 
  idResponsable INT NULL, 
  telefono VARCHAR(30) NOT NULL, 
  PRIMARY KEY (id), 
  CONSTRAINT fk_telefonos_institucion FOREIGN KEY (idInstitucion) REFERENCES Instituciones (id), 
  CONSTRAINT fk_telefonos_administrador FOREIGN KEY (idAdministrador) REFERENCES AdministradoresInstitucion (id), 
  CONSTRAINT fk_telefonos_responsable FOREIGN KEY (idResponsable) REFERENCES Responsables (id), 
  CONSTRAINT ck_telefonos_dueno CHECK (idInstitucion IS NOT NULL OR idAdministrador IS NOT NULL OR idResponsable IS NOT NULL)
);

#A FUTURO: 
#historial de cursos