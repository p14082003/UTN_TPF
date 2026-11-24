(function () {
    'use strict';

    var raiz = document.getElementById('busquedaInstituciones');
    if (!raiz) return;

    var lista = document.getElementById('listaInstituciones');
    var contador = raiz.querySelector('[data-contador]');
    var resultados = document.getElementById('resultadosBusqueda');
    var estado = document.getElementById('estadoBusqueda');
    var avanzados = document.getElementById('filtrosAvanzados');
    var campoAvanzados = document.getElementById(raiz.dataset.avanzadosId);
    var texto = raiz.querySelector('[data-parametro="texto"]');
    var anio = raiz.querySelector('[data-parametro="anio"]');
    var vacantes = raiz.querySelector('[data-vacantes] input');
    var mapa = null;
    var capaMarcadores = null;
    var marcadores = {};

    if (typeof L !== 'undefined') {
        mapa = L.map('mapa').setView([-34.6037, -58.3816], 12);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(mapa);
        capaMarcadores = L.layerGroup().addTo(mapa);
        actualizarMapa(JSON.parse(document.getElementById('datosInstituciones').textContent));
        if (typeof ResizeObserver !== 'undefined') {
            new ResizeObserver(function () { mapa.invalidateSize(); }).observe(document.getElementById('mapa'));
        }
    }

    function actualizarMapa(instituciones) {
        if (!mapa) return;
        capaMarcadores.clearLayers();
        marcadores = {};
        var limites = L.latLngBounds();
        instituciones.forEach(function (i) {
            var popup = document.createElement('div');
            var nombre = document.createElement('b');
            nombre.textContent = i.Nombre;
            popup.appendChild(nombre);
            popup.appendChild(document.createElement('br'));
            popup.appendChild(document.createTextNode(i.Direccion || ''));
            var marcador = L.marker([i.Latitud, i.Longitud]).addTo(capaMarcadores).bindPopup(popup);
            marcadores[i.Id] = marcador;
            limites.extend(marcador.getLatLng());
        });
        if (limites.isValid()) mapa.fitBounds(limites, { padding: [20, 20], maxZoom: 15 });
    }

    avanzados.addEventListener('toggle', function () {
        campoAvanzados.value = avanzados.open ? 'true' : 'false';
        if (mapa) mapa.invalidateSize();
    });

    lista.addEventListener('click', function (event) {
        if (!mapa || event.target.closest('a')) return;
        var card = event.target.closest('[data-id]');
        var marcador = card && marcadores[card.dataset.id];
        if (marcador) {
            mapa.flyTo(marcador.getLatLng(), 16);
            marcador.openPopup();
        }
    });

    if (!window.fetch || !window.AbortController) return;

    var temporizador;
    var solicitud;
    var version = 0;

    function crearUrl() {
        var url = new URL(raiz.dataset.listadoUrl, window.location.href);
        raiz.querySelectorAll('[data-parametro]').forEach(function (campo) {
            var valor = campo.value.trim();
            if (valor && (campo.dataset.parametro === 'texto' || valor !== '0') &&
                !(campo.dataset.parametro === 'orden' && valor === 'nombre')) {
                url.searchParams.set(campo.dataset.parametro, valor);
            }
        });
        if (vacantes.checked) url.searchParams.set('soloConVacantes', 'true');
        if (avanzados.open) url.searchParams.set('avanzados', 'true');
        return url;
    }

    // Conserva los tokens de WebForms para que otros botones, como cerrar sesión,
    // sigan funcionando después de cambiar las opciones del grado en el cliente.
    function actualizarEstadoFormulario(doc) {
        var selector = 'input[type="hidden"][name^="__VIEWSTATE"], input[type="hidden"][name="__EVENTVALIDATION"], ' +
            'input[type="hidden"][name="__EVENTTARGET"], input[type="hidden"][name="__EVENTARGUMENT"]';
        var form = raiz.closest('form');
        form.querySelectorAll(selector).forEach(function (campo) { campo.remove(); });
        doc.querySelectorAll(selector).forEach(function (campo) { form.appendChild(campo.cloneNode(true)); });
    }

    function actualizarSelects(doc) {
        raiz.querySelectorAll('select[data-parametro]').forEach(function (campo) {
            var nuevo = doc.getElementById(campo.id);
            if (!nuevo) return;
            var opciones = function (select) {
                return JSON.stringify(Array.from(select.options, function (opcion) { return [opcion.value, opcion.text]; }));
            };
            if (opciones(campo) !== opciones(nuevo)) campo.innerHTML = nuevo.innerHTML;
            campo.value = nuevo.value;
            campo.disabled = nuevo.disabled;
        });
    }

    async function buscar(actual) {
        solicitud = new AbortController();
        resultados.setAttribute('aria-busy', 'true');
        estado.textContent = '';
        try {
            var respuesta = await fetch(crearUrl().href, {
                signal: solicitud.signal,
                credentials: 'same-origin',
                cache: 'no-store',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!respuesta.ok) throw new Error('No se pudo consultar el listado.');
            var doc = new DOMParser().parseFromString(await respuesta.text(), 'text/html');
            if (actual !== version) return;
            var nuevaLista = doc.getElementById('listaInstituciones');
            var nuevosDatos = doc.getElementById('datosInstituciones');
            var nuevoContador = doc.querySelector('[data-contador]');
            if (!nuevaLista || !nuevosDatos || !nuevoContador) throw new Error('Respuesta inválida.');
            var instituciones = JSON.parse(nuevosDatos.textContent);

            actualizarEstadoFormulario(doc);
            actualizarSelects(doc);
            lista.innerHTML = nuevaLista.innerHTML;
            lista.scrollTop = 0;
            contador.textContent = nuevoContador.textContent;
            document.getElementById('datosInstituciones').textContent = nuevosDatos.textContent;
            actualizarMapa(instituciones);
            var urlActual = crearUrl();
            history.replaceState(null, '', urlActual.pathname + urlActual.search);
            estado.textContent = '';
        } catch (error) {
            if (actual !== version || error.name === 'AbortError') return;
            lista.textContent = '';
            contador.textContent = 'No se pudo actualizar la búsqueda';
            actualizarMapa([]);
            estado.textContent = 'No pudimos cargar las instituciones. Probá nuevamente con Filtrar.';
        } finally {
            if (actual === version) resultados.setAttribute('aria-busy', 'false');
        }
    }

    function programarBusqueda(espera) {
        clearTimeout(temporizador);
        if (solicitud) solicitud.abort();
        var actual = ++version;
        temporizador = setTimeout(function () { buscar(actual); }, espera);
    }

    raiz.addEventListener('change', function (event) {
        if (event.target.tagName !== 'SELECT' && event.target !== vacantes) return;
        if (event.target.dataset.parametro === 'oferta') {
            anio.value = '0';
            anio.disabled = true;
        }
        programarBusqueda(0);
    });
    texto.addEventListener('input', function (event) {
        if (!event.isComposing) programarBusqueda(400);
    });
    texto.addEventListener('compositionend', function () { programarBusqueda(400); });
    texto.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && !event.isComposing) {
            event.preventDefault();
            programarBusqueda(0);
        }
    });
    raiz.querySelector('[data-filtrar]').addEventListener('click', function (event) {
        event.preventDefault();
        programarBusqueda(0);
    });
    raiz.querySelector('[data-limpiar]').addEventListener('click', function (event) {
        event.preventDefault();
        raiz.querySelectorAll('[data-parametro]').forEach(function (campo) {
            campo.value = campo.dataset.parametro === 'texto' ? '' : campo.dataset.parametro === 'orden' ? 'nombre' : '0';
        });
        vacantes.checked = false;
        avanzados.open = false;
        campoAvanzados.value = 'false';
        anio.disabled = true;
        programarBusqueda(0);
    });
})();
