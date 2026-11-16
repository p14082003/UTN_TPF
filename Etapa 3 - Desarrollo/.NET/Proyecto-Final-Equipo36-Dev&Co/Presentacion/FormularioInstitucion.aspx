<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioInstitucion.aspx.cs" Inherits="Presentacion.FormularioInstitucion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />

    <style>
        #mapa {
            width: 100%;
            height: 100%;
            z-index: 1;
            border-radius: 15px;
        }
        .btn-lilaOscuro-invertido {
            background-color: #FFFFFF !important;
            color: #373A72 !important;
            border: 1px solid #373A72;
            border-radius: 15px;
            padding: 14px;
            font-weight: 600;
            transition: background-color 0.2s ease, color 0.2s ease;
        }

        .btn-lilaOscuro-invertido:hover {
            background-color: #373A72 !important;
            color: #FFFFFF !important;
        }

        * {
            scrollbar-color: #373A72 transparent;
        }

    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:ScriptManager ID="scriptManager" runat="server"></asp:ScriptManager>
    
    <div class="container-fluid">

        <!-- ENCABEZADO -->
        <div class="m-4">
            <h1 class="text-indigo fw-bold text-center">
                Registra tu institución educativa
            </h1>
            <h5 class="text-center">
                Creá el perfil oficial de tu colegio o instituto para que miles
                de familias encuentren tu propuesta pedagógica y gestionen
                admisiones de forma ágil y segura.
            </h5>
        </div>

        <!-- BENEFICIOS -->
        <div class="row m-4">

            <h3 class="text-indigo text-center my-4">
                ¿Qué podés hacer con esta cuenta?
            </h3>

            <div class="col-4">
                <div class="card h-100">
                    <div class="card-header text-center">
                        <h4 class="text-indigo">
                            <i class="bi bi-geo-alt mx-2"></i>
                            Mayor visibilidad
                        </h4>
                    </div>
                    <div class="card-body">
                        Aparecé en el mapa cuando las familias buscan vacantes por zona y nivel educativo.
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="card h-100">
                    <div class="card-header text-center">
                        <h4 class="text-indigo">
                            <i class="bi bi-clock-history mx-2"></i>
                            Seguimiento en tiempo real
                        </h4>
                    </div>
                    <div class="card-body">
                        Visualizá postulaciones y gestioná las admisiones directamente desde la aplicación.
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="card h-100">
                    <div class="card-header text-center">
                        <h4 class="text-indigo">
                            <i class="bi bi-folder2-open mx-2"></i>
                            Gestión centralizada
                        </h4>
                    </div>
                    <div class="card-body">
                        Organizá la información de tu institución y facilitá el contacto con las familias.
                    </div>
                </div>
            </div>

        </div>

        <!-- DATOS LEGALES E IDENTIFICACIÓN -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <div class="row">
                        <div class="col-8">
                            <!-- NOMBRE -->
                            <div class="mb-4">
                                <asp:Label ID="lblNombre" runat="server" Text="Nombre de la institución" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" placeholder="Ej. Colegio de la Nación"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-4">
                            <!-- CUIT -->
                            <div class="mb-4">
                                <asp:Label ID="lblCuit" runat="server" Text="CUIT de la institución" CssClass="form-label fw-semibold"> </asp:Label>
                                <asp:TextBox ID="txtCuit" runat="server" CssClass="form-control" placeholder="XX-XXXXXXXX-X"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                    <!-- EMAIL -->
                    <div class="mb-4">
                        <asp:Label ID="lblEmail" runat="server" Text="Correo electrónico" CssClass="form-label fw-semibold"></asp:Label>
                        <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="tuColegio@mail.com.ar"></asp:TextBox>
                    </div>

                    <!-- TELEFONOS -->
                    <div class="row">
                        <div class="col-6 mb-4">
                            <asp:Label ID="lblTelefono" runat="server" Text="Teléfono principal" CssClass="form-label fw-semibold"></asp:Label>
                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" TextMode="Phone" placeholder="+54 11 0000-0000"></asp:TextBox>
                        </div>
                            
                        <div class="col-6 mb-4">
                            <asp:Label ID="lblTelefonoAlternativo" runat="server" Text="Télefono alternativo" CssClass="form-label fw-semibold"></asp:Label>
                            <asp:TextBox ID="txtTelefonoAlternativo" runat="server" CssClass="form-control" TextMode="Phone" placeholder="+54 9 11 0000-0000"></asp:TextBox>
                        </div>

                    </div>

                    <!-- DESCRIPCION -->
                    <div class="mb-4">
                        <asp:Label ID="lblDescripcion" runat="server" Text="Descripción de la institución" CssClass="form-label fw-semibold"></asp:Label>
                        <asp:TextBox ID="txtDescripcion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" style="resize: none;" placeholder="Contá brevemente qué ofrece tu institución"></asp:TextBox>
                    </div>

                </div>
            </div>
        </div>

        <!-- UBICACION -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">
                    <!-- UBICACION -->
                    <div class="mb-4">
                        <asp:Label ID="lblDireccion" runat="server" Text="Dirección de la institución" CssClass="form-label fw-semibold"></asp:Label>
                        <div class="d-flex gap-4">
                            <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" placeholder="Calle, número y localidad"></asp:TextBox>
                            <button type="button" id="btnVerificarDireccion" class="btn-lilaOscuro">
                                <i class="bi bi-search"></i>
                            </button>
                        </div>
                    </div>

                    <!-- MENSAJE DE ESTADO-->
                    <div id="mensajeDireccion" class="form-text">Tocá la lupa o presioná Enter para ubicar la dirección en el mapa.</div>

                    <div style="height: 25dvh; min-height: 250px;">
                        <div id="mapa"></div>
                    </div>

                    <!-- GUARDAR COORDENADAS -->
                    <asp:HiddenField ID="hfLatitud" runat="server" />
                    <asp:HiddenField ID="hfLongitud" runat="server" />
                </div>
            </div>
        </div>

        <!-- IDIOMAS + ACCESIBILIDADES -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <!-- IDIOMAS -->
                    <div class="mb-4">
                        <asp:Label ID="lblIdiomas" runat="server" Text="Idiomas" CssClass="form-label fw-semibold"></asp:Label>

                        <asp:UpdatePanel ID="updIdiomas" runat="server" UpdateMode="Conditional">

                            <ContentTemplate>
                                <div class="d-flex flex-wrap gap-4">
                                    <asp:Repeater ID="rptIdiomas" runat="server" OnItemCommand="rptIdiomas_ItemCommand">
                                        <ItemTemplate>
                                            <div class="btn btn-lilaClaro">
                                                <%# Eval("NombreIdioma") %>
                                                <asp:LinkButton runat="server" CommandName="EliminarIdioma" CommandArgument='<%# Eval("Id") %>' CausesValidation="false" CssClass="text-indigo text-decoration-none ms-1">×</asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <button type="button" class="btn-lilaOscuro" onclick="document.getElementById('dlgIdiomas').showModal()"> + Agregar idioma</button>
                                </div>

                                <!-- Dialog -->
                                <dialog class="dialog" id="dlgIdiomas">
                                    <div class="dialog-header">
                                        <h5 class="dialog-title text-indigo">Agregar idiomas</h5>
                                    </div>
                                    <div class="dialog-body">
                                        <asp:DropDownList ID="ddlIdiomas" runat="server" CssClass="form-control"></asp:DropDownList>
                                    </div>
                                    <div class="dialog-footer">
                                        <button type="button" class="btn btn-lilaClaro" onclick="document.getElementById('dlgIdiomas').close()">Cancelar</button>
                                        <asp:Button ID="btnAgregarIdioma" runat="server" Text="Agregar" CssClass="btn-lilaOscuro" CausesValidation="false" OnClick="btn_AgregarIdioma_Click" />

                                    </div>
                                </dialog>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>

                    <!-- ACCESIBILIDADES -->
                    <div class="mb-4">
                        <asp:Label ID="lblAccesibilidades" runat="server" Text="Accesibilidades" CssClass="form-label fw-semibold"></asp:Label>

                        <asp:UpdatePanel ID="udpAccesibilidades" runat="server" UpdateMode="Conditional">

                            <ContentTemplate>
                                <div class="d-flex flex-wrap gap-4">
                                    <asp:Repeater ID="rptAccesibilidades" runat="server" OnItemCommand="rptAccesibilidades_ItemCommand">
                                        <ItemTemplate>
                                            <div class="btn btn-lilaClaro">
                                                <%# Container.DataItem %>
                                                <asp:LinkButton runat="server" CommandName="EliminarAccesibilidad" CommandArgument='<%# Container.DataItem %>' CausesValidation="false" CssClass="text-indigo text-decoration-none ms-1">×</asp:LinkButton>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>

                                    <button type="button" class="btn-lilaOscuro" onclick="document.getElementById('dlgAccesibilidades').showModal()"> + Agregar accesibilidad</button>
                                </div>

                                <!-- Dialog -->
                                <dialog class="dialog" id="dlgAccesibilidades">
                                    <div class="dialog-header">
                                        <h5 class="dialog-title text-indigo">Agregar accesibilidad</h5>
                                    </div>
                                    <div class="dialog-body">
                                        <asp:DropDownList ID="ddlAccesibilidades" runat="server" CssClass="form-control"></asp:DropDownList>
                                    </div>
                                    <div class="dialog-footer">
                                        <button type="button" class="btn btn-lilaClaro" onclick="document.getElementById('dlgAccesibilidades').close()">Cancelar</button>
                                        <asp:Button ID="btnAgregarAccesibilidad" runat="server" Text="Agregar" CssClass="btn-lilaOscuro" CausesValidation="false" OnClick="btn_AgregarAccesibilidad_Click" />
                                    </div>
                                </dialog>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </div>

                </div>
            </div>
        </div>

        <!-- IMAGENES -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">
                
                    <!-- CARGA DE IMAGENES -->
                    <asp:Label ID="lblImagenes" runat="server" Text="Imagenes de las instalaciones" CssClass="form-label fw-semibold mb-5"></asp:Label>
                    <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-control mb-5 mt-3"/>

                    <!-- CARROUSEL DE IMAGENES (REPEATER DE ASP) -->
                    <div class="row">
                        <div id="carouselMultiple" class="carousel slide" data-bs-ends="wrap" style="--bs-carousel-items: 3; --bs-carousel-items-gap: 1rem;">

                            <!-- CONTROLES -->
                            <div class="d-flex justify-content-end align-items-center">
                                <button class="btn-icon btn-sm" type="button" data-bs-target="#carouselMultiple" data-bs-slide="prev" style="--bs-carousel-control-icon-width: 1rem;">
                                    <span class="carousel-icon-prev" aria-hidden="true"></span>
                                    <span class="visually-hidden">Previous</span>
                                </button>
                                <button class="btn-icon btn-sm" type="button" data-bs-target="#carouselMultiple" data-bs-slide="next" style="--bs-carousel-control-icon-width: 1rem;">
                                    <span class="carousel-icon-next" aria-hidden="true"></span>
                                    <span class="visually-hidden">Next</span>
                                </button>
                            </div>

                            <!-- IMAGENES -->
                            <div class="carousel-inner">
                                <asp:Repeater ID="rptImagenes" runat="server" OnItemCommand="rptImagenes_ItemCommand">
                                    <ItemTemplate>
                                        <div class='carousel-item <%# Container.ItemIndex == 0 ? "active" : "" %>'>
                                            <div class="card">
                                                <asp:Image runat="server" ImageUrl='<%# Eval("Url") %>' AlternateText="Imagen de las instalaciones" CssClass="d-block w-100 h-100 rounded-5" />
                                                <div class="card-body">
                                                    <div class="d-flex justify-content-between">
                                                        <p>Imagen <%# Eval("Descripcion") %></p>
                                                        <asp:Button runat="server" Text="x" CssClass="btn-solid theme-danger" CommandName="EliminarImagen" CommandArgument='<%# Container.ItemIndex %>' CausesValidation="false" />
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <!-- BOTON PARA GUARDAR DATOS -->
        <div class="row m-4">
            <div class="col">
                <div class="p-5">
                    <asp:Button ID="btnGuardar" runat="server" Text="Registrar institución" CssClass="btn btn-lilaOscuro w-100 mb-4"/>
                    <p class="text-center text-secondary small">
                        ¿Ya registraste tu colegio?
                        <a href="#" class="link-lilaClaro">Iniciá sesión como institución</a>
                    </p>
                </div>
            </div>
        </div>
    </div>

    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script>

        // CAROUSEL: Al intentar avanzar en la ultima imagen, vuelve al inicio
        (function () {
            var carrusel = document.getElementById('carouselMultiple');
            var inner = carrusel.querySelector('.carousel-inner');
            var visibles = 3; // el mismo valor que --bs-carousel-items
            var ultimo = carrusel.querySelectorAll('.carousel-item').length - visibles;

            carrusel.addEventListener('slide.bs.carousel', function (e) {
                // Solo al avanzar un paso más allá del final (no al retroceder desde la primera)
                if (e.to > ultimo && e.to === e.from + 1) {
                    e.preventDefault();
                    inner.scrollTo({ left: 0, behavior: 'smooth' });
                }
            });
        })();

        // MAPA
        var institucion = <%= institucionJson %>;

        // Variables necesrias
        var txtDireccion = document.getElementById('<%= txtDireccion.ClientID %>');
        var hfLat = document.getElementById('<%= hfLatitud.ClientID %>');
        var hfLng = document.getElementById('<%= hfLongitud.ClientID %>');
        var btnVerificar = document.getElementById('btnVerificarDireccion');
        var mensaje = document.getElementById('mensajeDireccion');

        // Valor por defecto (Buenos Aires)
        var posicionPorDefecto = [-34.6037, -58.3816];
        var marcador;

        // Cargar ubicacion en caso de entrar al formulario para modificar una institución
        function coordenadasIniciales() {

            var latitud = parseFloat(hfLat.value);
            var longitud = parseFloat(hfLng.value);

            if (!isNaN(latitud) && !isNaN(longitud)) {
                return [latitud, longitud];
            }

            if (institucion.Latitud !== 0 || institucion.Longitud !== 0) {
                return [institucion.Latitud, institucion.Longitud];
            }

            return null;
        }

        function guardarCoordenadas(lat, lng) {

            hfLat.value = lat.toFixed(6); // toFixed siempre usa punto decimal
            hfLng.value = lng.toFixed(6);
        }

        // Crear el marcador arrastrable
        // Cuando el usuario lo suelta (dragend) o cuando se verifica la dirección
        function crearMarcador(posicion) {

            marcador = L.marker(posicion, { draggable: true }).addTo(mapa);

            // Cuando el usuario suelta el marcador (dragend), se verifica y guarda la dirección
            marcador.on('dragend', function () {
                var p = marcador.getLatLng();
                guardarCoordenadas(p.lat, p.lng);
            });
        }

        // Mensaje de estado
        function mostrarMensaje(texto, esError) {
            mensaje.textContent = texto;
            mensaje.className = 'form-text ' + (esError ? 'text-danger' : '');
        }

        var inicial = coordenadasIniciales();

        // Mostrar mapa
        var mapa = L.map('mapa').setView(inicial || posicionPorDefecto, inicial ? 16 : 12);

        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(mapa);

        if (inicial) {
            crearMarcador(inicial);
        } else if (navigator.geolocation) {
            // Registro nuevo: ubicación del usuario como punto de partida (no se guarda)
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    var posicion = [pos.coords.latitude, pos.coords.longitude];
                    mapa.setView(posicion, 16);
                    crearMarcador(posicion);
                },
                function () {
                    // Si el usuario no habilta la ubicación, queda el centro por defecto, sin marcador
                },
                { timeout: 8000, maximumAge: 600000 }
            );
        }

        // NOMINATIM: Buscar la dirección y ubicarla en el mapa
        async function verificarDireccion() {
            var direccion = txtDireccion.value.trim();

            if (!direccion) {
                mostrarMensaje('Ingresá una dirección para ubicarla en el mapa.', true);
                return;
            }

            btnVerificar.disabled = true;
            mostrarMensaje('Buscando...', false);

            try {
                // API Endpoint + Parametros
                var url = 'https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&countrycodes=ar&accept-language=es&q=' + encodeURIComponent(direccion);
                var respuesta = await fetch(url);
                
                if (!respuesta.ok) {
                    throw new Error('HTTP ' + respuesta.status);
                }

                var resultados = await respuesta.json();

                if (resultados.length === 0) {
                    mostrarMensaje('No encontramos esa dirección. Probá agregando la localidad, o ubicá el marcador a mano.', true);
                    return;
                }

                // Parsear latitud y longitud del JSON a FLOAT
                var latitud = parseFloat(resultados[0].lat);
                var longitud = parseFloat(resultados[0].lon);

                if (marcador) {
                    marcador.setLatLng([latitud, longitud]);
                } else {
                    crearMarcador([latitud, longitud]);
                }

                mapa.setView([latitud, longitud], 17);
                guardarCoordenadas(latitud, longitud);

                mostrarMensaje('Dirección ubicada. Si el marcador no está en el lugar correcto, arrastralo.', false);
            }
            catch (error) {
                mostrarMensaje('No se pudo consultar el mapa. Intentá de nuevo en unos segundos.', true);
            }
            finally {
                btnVerificar.disabled = false;
            }
        }

        btnVerificar.addEventListener('click', verificarDireccion);
    </script>

</asp:Content>
