<%@ Page Title="Registrar institución" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioAdministradorInstitucion.aspx.cs" Inherits="Presentacion.FormularioAdministradorInstitucion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">

        <div class="row justify-content-center">
            <div class="col-12 col-md-9 col-lg-6">

                <header class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-50 img-fluid mb-1" />
                    <h2 class="h3 fw-bold text-indigo mb-2">Registrá tu institución</h2>
                    <p class="text-secondary mb-0">
                        Completá tus datos y los de la institución. Te enviaremos la contraseña por correo.
                    </p>
                </header>

                <!-- Formulario -->
                <div class="card mb-4">
                    <div class="card-body p-3 p-md-4">

                        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert">
                            <asp:Literal ID="litMensaje" runat="server" />
                        </asp:Panel>

                        <div class="mb-3">
                            <label for="<%= txtNombreUsuario.ClientID %>" class="form-label text-indigo fw-semibold">Nombre</label>
                            <asp:TextBox ID="txtNombreUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                        </div>

                        <div class="mb-3">
                            <label for="<%= txtApellidoUsuario.ClientID %>" class="form-label text-indigo fw-semibold">Apellido</label>
                            <asp:TextBox ID="txtApellidoUsuario" runat="server" CssClass="form-control" MaxLength="50" />
                        </div>

                        <div class="mb-3">
                            <label for="<%= txtEmail.ClientID %>" class="form-label text-indigo fw-semibold">Correo electrónico</label>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" MaxLength="50" />
                        </div>

                        <div class="mb-3">
                            <label for="<%= txtTelefono.ClientID %>" class="form-label text-indigo fw-semibold">Teléfono</label>
                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" TextMode="Phone" MaxLength="30" />
                        </div>

                        <div class="mb-3">
                            <label for="<%= txtNombreInstitucion.ClientID %>" class="form-label text-indigo fw-semibold">Nombre de la institución</label>
                            <asp:TextBox ID="txtNombreInstitucion" runat="server" CssClass="form-control" MaxLength="100" />
                        </div>

                        <div class="mb-3">
                            <label for="<%= ddlTipoInstitucion.ClientID %>" class="form-label text-indigo fw-semibold">Tipo de institución</label>
                            <asp:DropDownList ID="ddlTipoInstitucion" runat="server" CssClass="form-select" />
                        </div>

                        <div class="mb-4">
                            <label for="<%= fuDocumentos.ClientID %>" class="form-label text-indigo fw-semibold">Documentos</label>
                            <asp:FileUpload ID="fuDocumentos" runat="server" CssClass="form-control" AllowMultiple="true" accept=".pdf,.jpg,.jpeg,.png" />
                            <div class="form-text">PDF, JPG o PNG. Máximo 5 MB por archivo. Podés agregar varios.</div>
                            <div id="errorArchivos" class="text-danger small mt-1" style="display:none;"></div>
                            <ul id="listaArchivos" class="list-unstyled mt-2 mb-0"></ul>
                        </div>

                        <asp:Button ID="btnGuardar" runat="server" Text="Registrar institución" CssClass="btn btn-lilaOscuro w-100 py-2 mb-2" OnClick="btnGuardar_Click" />
                        <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-lilaClaro w-100 py-2" OnClick="btnCancelar_Click" />
                    </div>
                </div>

                <!-- Volver al login -->
                <p class="text-center small text-secondary mb-0">
                    ¿Ya tenés una cuenta?
                    <asp:HyperLink ID="hlIniciarSesion" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro fw-semibold">
                        Iniciá sesión
                    </asp:HyperLink>
                </p>
            </div>
        </div>
    </div>

    <script>
        (function () {
            var input = document.getElementById('<%= fuDocumentos.ClientID %>');
            var lista = document.getElementById('listaArchivos');
            var error = document.getElementById('errorArchivos');
            var extensiones = ['pdf', 'jpg', 'jpeg', 'png'];
            var maxBytes = 5 * 1024 * 1024;
            var seleccion = new DataTransfer();

            function formatoTamanio(bytes) {
                return bytes >= 1048576
                    ? (bytes / 1048576).toFixed(1) + ' MB'
                    : Math.max(1, Math.round(bytes / 1024)) + ' KB';
            }

            function dibujar() {
                lista.innerHTML = '';
                Array.prototype.forEach.call(seleccion.files, function (archivo, i) {
                    var li = document.createElement('li');
                    li.className = 'd-flex align-items-center justify-content-between border border-neutro rounded-3 bg-white px-3 py-2 mb-2';

                    var nombre = document.createElement('span');
                    nombre.className = 'small text-indigo text-truncate me-2';
                    nombre.textContent = archivo.name + ' (' + formatoTamanio(archivo.size) + ')';

                    var boton = document.createElement('button');
                    boton.type = 'button';
                    boton.className = 'btn btn-lilaClaro btn-sm py-1 px-2';
                    boton.setAttribute('aria-label', 'Quitar ' + archivo.name);
                    boton.textContent = 'Quitar';
                    boton.addEventListener('click', function () { quitar(i); });

                    li.appendChild(nombre);
                    li.appendChild(boton);
                    lista.appendChild(li);
                });
                input.files = seleccion.files;
            }

            function quitar(indice) {
                var nueva = new DataTransfer();
                Array.prototype.forEach.call(seleccion.files, function (archivo, i) {
                    if (i !== indice) nueva.items.add(archivo);
                });
                seleccion = nueva;
                error.style.display = 'none';
                dibujar();
            }

            input.addEventListener('change', function () {
                var rechazados = [];

                Array.prototype.forEach.call(input.files, function (archivo) {
                    var ext = archivo.name.split('.').pop().toLowerCase();
                    var repetido = Array.prototype.some.call(seleccion.files, function (f) {
                        return f.name === archivo.name && f.size === archivo.size;
                    });

                    if (extensiones.indexOf(ext) < 0 || archivo.size === 0 || archivo.size > maxBytes) {
                        rechazados.push(archivo.name);
                    } else if (!repetido) {
                        seleccion.items.add(archivo);
                    }
                });

                if (rechazados.length > 0) {
                    error.textContent = 'No se agregaron (solo PDF, JPG o PNG de hasta 5 MB): ' + rechazados.join(', ');
                    error.style.display = 'block';
                } else {
                    error.style.display = 'none';
                }

                dibujar();
            });
        })();
    </script>
</asp:Content>
