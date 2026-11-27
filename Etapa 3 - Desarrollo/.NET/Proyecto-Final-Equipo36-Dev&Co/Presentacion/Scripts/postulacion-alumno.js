(function () {
    'use strict';

    var datos = document.getElementById('cursosPostulacion');
    var ciclo = document.querySelector('[data-curso="ciclo"]');
    if (!datos || !ciclo) return;

    var niveles = JSON.parse(datos.textContent);
    var nivel = document.querySelector('[data-curso="nivel"]');
    var anio = document.querySelector('[data-curso="anio"]');
    var division = document.querySelector('[data-curso="division"]');
    var turno = document.querySelector('[data-curso="turno"]');
    var vacantes = document.querySelector('input[id$="txtVacantes"]');
    var tipoDocumento = document.querySelector('select[id$="ddlTipoDocumento"]');
    var archivo = document.querySelector('input[type="file"][id$="fuDocumentos"]');

    function actualizarCargaDocumento() {
        var opcion = tipoDocumento.selectedOptions[0];
        archivo.multiple = !opcion || opcion.dataset.unico !== 'true';
    }
    tipoDocumento.addEventListener('change', actualizarCargaDocumento);
    actualizarCargaDocumento();

    var alumnos = document.querySelector('select[id$="ddlAlumnos"]');
    var avisoDocumentos = document.querySelector('[data-documentos-postulacion]');
    var documentosCargados = JSON.parse(avisoDocumentos.dataset.documentosPostulacion);

    function actualizarDocumentacion() {
        var alumno = alumnos.selectedOptions[0];
        var alumnoSeleccionado = alumno && alumno.value !== '';
        var esAutogestor = alumno && alumno.dataset.autogestor === 'true';
        var faltantes = [];
        Array.from(tipoDocumento.options).forEach(function (opcion) {
            var obligatorio = opcion.dataset.obligatorio === 'true' &&
                !(esAutogestor && opcion.dataset.exentoAutogestor === 'true');
            if (opcion.dataset.exentoAutogestor === 'true') {
                opcion.textContent = opcion.value + (!alumnoSeleccionado ? ' (obligatoria salvo para autogestores)' :
                    obligatorio ? ' (obligatoria)' : ' (opcional)');
            }
            if (obligatorio && !documentosCargados.includes(opcion.value)) faltantes.push(opcion.value);
        });
        avisoDocumentos.textContent = !alumnoSeleccionado ? 'Seleccioná el alumno para ver los documentos obligatorios.' :
            faltantes.length ? 'Falta adjuntar: ' + faltantes.join(', ') + '.' :
            'Ya adjuntaste los documentos obligatorios.';
        var completos = !!alumnoSeleccionado && faltantes.length === 0;
        avisoDocumentos.classList.toggle('fg-success', completos);
        avisoDocumentos.classList.toggle('text-indigo', !completos);
    }

    alumnos.addEventListener('change', actualizarDocumentacion);
    actualizarDocumentacion();
    var campos = [ciclo, nivel, anio, division, turno];
    var opciones = new Map(campos.map(function (campo) {
        return [campo, Array.from(campo.options).map(function (opcion) { return opcion.cloneNode(true); })];
    }));

    function mostrarOpciones(campo, valores) {
        var seleccion = campo.value;
        var permitidos = new Set(valores.map(String));
        var disponibles = opciones.get(campo).filter(function (opcion) {
            return opcion.value === '' || permitidos.has(opcion.value);
        });
        campo.replaceChildren.apply(campo, disponibles.map(function (opcion) { return opcion.cloneNode(true); }));
        campo.value = permitidos.has(seleccion) ? seleccion : '';
        campo.disabled = disponibles.length <= 1;
    }

    function valorAnio(curso) {
        return curso.Anio === null ? 'null' : String(curso.Anio);
    }

    function actualizar() {
        var nivelesCiclo = niveles.filter(function (grupo) {
            return grupo.Cursos.some(function (curso) { return String(curso.CicloLectivo) === ciclo.value; });
        });
        mostrarOpciones(nivel, nivelesCiclo.map(function (grupo) { return grupo.Valor; }));

        var grupo = nivelesCiclo.find(function (item) { return String(item.Valor) === nivel.value; });
        var cursos = grupo ? grupo.Cursos.filter(function (curso) { return String(curso.CicloLectivo) === ciclo.value; }) : [];
        mostrarOpciones(anio, cursos.map(valorAnio));
        cursos = cursos.filter(function (curso) { return valorAnio(curso) === anio.value; });
        mostrarOpciones(division, cursos.map(function (curso) { return curso.Division; }));
        cursos = cursos.filter(function (curso) { return curso.Division === division.value; });
        mostrarOpciones(turno, cursos.map(function (curso) { return curso.IdTurno; }));

        var curso = cursos.find(function (item) { return String(item.IdTurno) === turno.value; });
        vacantes.value = curso ? curso.VacantesDisponibles : '';
    }

    campos.forEach(function (campo, indice) {
        campo.addEventListener('change', function () {
            campos.slice(indice + 1).forEach(function (siguiente) { siguiente.value = ''; });
            actualizar();
        });
    });
    actualizar();
})();
