(function () {
    var iconoOjo = '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true" focusable="false">' +
        '<path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />' +
        '<circle cx="12" cy="12" r="3" />';

    document.querySelectorAll('input[type="password"]').forEach(function (campo) {
        if (campo.closest('.campo-contrasenia')) return;

        var contenedor = document.createElement('div');
        contenedor.className = 'campo-contrasenia';
        campo.parentNode.insertBefore(contenedor, campo);
        contenedor.appendChild(campo);

        var boton = document.createElement('button');
        boton.type = 'button';
        boton.className = 'boton-ver-contrasenia';
        boton.setAttribute('aria-controls', campo.id);
        boton.disabled = campo.disabled;

        function mostrarContrasenia(mostrar) {
            campo.type = mostrar ? 'text' : 'password';
            var texto = mostrar ? 'Ocultar contraseña' : 'Mostrar contraseña';
            boton.setAttribute('aria-label', texto);
            boton.setAttribute('aria-pressed', mostrar ? 'true' : 'false');
            boton.title = texto;
            boton.innerHTML = iconoOjo + (mostrar ? '<path d="m3 3 18 18" />' : '') + '</svg>';
        }

        boton.addEventListener('click', function () {
            mostrarContrasenia(campo.type === 'password');
        });

        mostrarContrasenia(false);
        contenedor.appendChild(boton);
        window.addEventListener('pagehide', function () { mostrarContrasenia(false); });
    });
})();
