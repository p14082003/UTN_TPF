<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="DetalleInstitucion.aspx.cs" Inherits="Presentacion.DetalleInstitucion" %>
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

<div class="container-fluid">

        <!-- Carousel de imagenes -->
        <asp:Panel ID="pnlImagenes" runat="server">
            <div class="row m-4">
                <div class="col-12">
                    <div class="card p-5" style="height: 40dvh;">

                        <div id="carouselMultiple" class="carousel slide h-100 d-flex flex-column" data-bs-autoplay="true" data-bs-ends="wrap" style="--bs-carousel-items: 3; --bs-carousel-items-gap: 1rem;">

                            <!-- Controles -->
                            <div class="d-flex justify-content-between align-items-center mb-2">
                                <div class="carousel-indicators">
                                    <asp:Repeater ID="rptIndicadores" runat="server">
                                        <ItemTemplate>
                                            <button type="button" data-bs-target="#carouselMultiple"
                                                data-bs-slide-to="<%# Container.ItemIndex %>"
                                                class="<%# Container.ItemIndex == 0 ? "active" : "" %>"
                                                <%# Container.ItemIndex == 0 ? "aria-current=\"true\"" : "" %>
                                                aria-label="Slide <%# Container.ItemIndex + 1 %>"></button>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>

                                <div>
                                    <button class="btn-icon btn-sm carousel-control-play-pause" type="button" data-bs-target="#carouselMultiple" aria-label="Pause" data-bs-pause-label="Pause" data-bs-play-label="Play">
                                        <span class="carousel-icon-pause" aria-hidden="true"></span>
                                        <span class="carousel-icon-play" aria-hidden="true"></span>
                                    </button>

                                    <button class="btn-icon btn-sm" type="button"
                                        data-bs-target="#carouselMultiple" data-bs-slide="prev">
                                        <span class="carousel-icon-prev" aria-hidden="true"></span>
                                        <span class="visually-hidden">Previous</span>
                                    </button>

                                    <button class="btn-icon btn-sm" type="button"
                                        data-bs-target="#carouselMultiple" data-bs-slide="next">
                                        <span class="carousel-icon-next" aria-hidden="true"></span>
                                        <span class="visually-hidden">Next</span>
                                    </button>
                                </div>
                            </div>

                            <!-- Imágenes -->
                            <div class="carousel-inner flex-grow-1" style="min-height: 0;">

                                <asp:Repeater ID="rptImagenes" runat="server">
                                    <ItemTemplate>
                                        <div class="carousel-item h-100 <%# Container.ItemIndex == 0 ? "active" : "" %>">
                                            <img src="<%# Eval("Url") %>"
                                                class="d-block w-100 h-100 rounded-5"
                                                style="object-fit: cover;" alt="<%#: Eval("Descripcion") %>">
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>


                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- Datos generales -->
        <div class="row m-4">

            <!-- Información de la institución -->
            <div class="col-8">
                <div class="card h-100 p-5">
                    <asp:Label ID="lblNombreInstitucion" runat="server" CssClass="h2 text-indigo"></asp:Label>
                    <asp:Label ID="lblDescripcion" runat="server" CssClass="p"></asp:Label>

                    <hr />

                    <label class="text-indigo fw-bold mb-2 mt-2">Idiomas</label>
                    <div class="d-flex flex-wrap gap-2 mb-2">
                        <asp:Repeater ID="rptIdiomas" runat="server">
                            <ItemTemplate>
                                <div class="btn-lilaClaro">
                                    <%#: Eval("NombreIdioma") %>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <label class="text-indigo fw-bold mb-2 mt-2">Inclusión, Accesibilidad y Discapacidades</label>
                    <div class="d-flex flex-wrap gap-2 mb-2">
                        <asp:Repeater ID="rptAccesibilidades" runat="server">
                            <ItemTemplate>
                                <div class="btn-lilaClaro">
                                    <%#: Container.DataItem %>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                </div>
            </div>

            <!-- Información de contacto -->
            <div class="col-4">
                <div class="card h-100 p-5">

                    <div class="mb-4">
                        <label class="text-indigo fw-bold">Tipo de institución</label>
                        <asp:Label ID="lblTipoInstitucion" runat="server" CssClass="d-block"></asp:Label>
                    </div>

                    <div class="mb-4">
                        <label class="text-indigo fw-bold">Correo electrónico</label>
                        <asp:Label ID="lblEmail" runat="server" CssClass="d-block link-lilaClaro fw-bold"></asp:Label>
                    </div>

                    <div class="mb-4">
                        <label class="text-indigo fw-bold">Teléfonos</label>
                        <asp:Label ID="lblTelefonoPrincipal" runat="server" CssClass="d-block"></asp:Label>
                        <asp:Label ID="lblTelefonoAlternativo" runat="server" CssClass="d-block"></asp:Label>
                    </div>

                    <div class="mb-4">
                        <label class="text-indigo fw-bold">Dirección</label>
                        <asp:Label ID="lblDireccion" runat="server" CssClass="d-block"></asp:Label>
                    </div>

                    <asp:Button ID="btnRegistrarAlumno" runat="server" CssClass="btn-lilaOscuro w-100" Text="Matricular"></asp:Button>

                </div>
            </div>
        </div>

        <!-- Reseñas -->
        <div class="row m-4">
            <div class="col-12">
                <div class="card p-5">
                    <div class="d-flex align-items-center">
                        <h4 class="text-indigo fw-bold flex-grow-1">Reseñas de las familias y comunidad</h4>
                        <asp:Button ID="btnReseña" runat="server" CssClass="btn-lilaOscuro-invertido just" Text="Nueva reseña"></asp:Button>
                    </div>

                    <asp:Repeater ID="rptResenias" runat="server">
                        <ItemTemplate>
                            <div class="border-bottom border-neutro py-4">
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <span class="fw-bold text-indigo"><%#: Eval("Responsable.Nombre") %></span>
                                    <small class="text-muted"><%# Eval("Fecha", "{0:dd/MM/yyyy}") %></small>
                                </div>

                                <div class="text-lavanda mb-2" aria-label="Calificación: <%# Eval("Calificacion") %> de 5">
                                    <%# new string('★', (int)Eval("Calificacion")) %><%# new string('☆', 5 - (int)Eval("Calificacion")) %>
                                </div>

                                <p class="mb-2"><%#: Eval("Comentario") %></p>

                                <asp:Panel runat="server" CssClass="bg-fondo ms-4 px-4 py-2" Style="border-left: 3px solid #373A72;" Visible='<%# !string.IsNullOrEmpty((string)Eval("RespuestaInstitucion")) %>'>
                                    <small class="fw-bold text-indigo">Respuesta de la institución</small>
                                    <p><%#: Eval("RespuestaInstitucion") %></p>
                                </asp:Panel>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="pnlSinResenias" runat="server" Visible="false">
                        <p class="text-muted mb-0">Todavía no hay reseñas. ¡Sé el primero en opinar!</p>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <!-- Mapa -->
        <div class="row m-4">
            <div class="p-5" style="height: 25dvh; min-height: 250px;">
                <div id="mapa"></div>
            </div>
        </div>

    </div>

    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script>
        // Traer datos de la institucion
        var institucion = <%= institucionJson %>;

        // Generar mapa
        var mapa = L.map('mapa').setView([institucion.Latitud, institucion.Longitud], 12);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(mapa);

        // Generar marcadores
        var marcador = L.marker([institucion.Latitud, institucion.Longitud])
            .addTo(mapa)
            .bindPopup('<b>' + institucion.Nombre + '</b><br>' + institucion.Direccion);

        // Reset: al intentar avanzar más allá del final, vuelve al inicio
        (function () {
            var carrusel = document.getElementById('carouselMultiple');
            var inner = carrusel.querySelector('.carousel-inner');
            var visibles = 3; // el mismo valor que --bs-carousel-items
            var ultimo = carrusel.querySelectorAll('.carousel-item').length - visibles;

            carrusel.addEventListener('slide.bs.carousel', function (e) {
                if (e.to > ultimo) {
                    e.preventDefault();
                    inner.scrollTo({ left: 0, behavior: 'smooth' });
                }
            });
        })();
    </script>

</asp:Content>
