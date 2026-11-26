<%@ Page Title="InstiRed - Detalle de la institución" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="DetalleInstitucion.aspx.cs" Inherits="Presentacion.DetalleInstitucion" %>
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

        .galeria-fotos {
            --separacion-fotos: 1rem;
            height: clamp(18rem, 40dvh, 27rem);
            padding: 1rem;
        }

        .foto-unica, .foto-galeria {
            display: block;
            width: 100%;
            height: 100%;
            padding: 0.5rem;
            object-fit: contain;
            object-position: center;
            background: #F7F5FC;
            border-radius: 12px;
        }

        .galeria-controles { flex: 0 0 auto; flex-wrap: nowrap !important; gap: 0.75rem; }
        .galeria-controles .carousel-indicators { min-width: 0; flex-wrap: wrap; }
        .galeria-botones { display: flex; flex-shrink: 0; gap: 0.25rem; }

        @media (max-width: 767px) {
            #contenido .galeria-fotos { height: clamp(16rem, 70vw, 23rem); padding: 0.75rem !important; }
            .galeria-fotos .carousel { --bs-carousel-items: 1 !important; --bs-carousel-items-gap: 0.5rem !important; }
            .galeria-controles .carousel-indicators { max-width: 100%; }
        }

        .dialogo-resenia {
            width: min(32rem, calc(100% - 2rem));
            max-height: calc(100dvh - 2rem);
            overflow-y: auto;
            border: 1px solid #E4E0F0;
            border-radius: 15px;
            padding: 1.5rem;
            color: #373A72;
        }

        .dialogo-resenia::backdrop {
            background: rgb(0 0 0 / 40%);
        }

        .resenia-propia {
            background: #F7F5FC;
            border: 1px solid #E4E0F0;
            border-left: 3px solid #8D91D5;
            border-radius: 12px;
            padding: 1rem;
            margin: 1rem 0;
        }

        * {
            scrollbar-color: #373A72 transparent;
        }

    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

<div class="container-fluid">

        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="card m-4 p-4">
            <asp:Label ID="lblError" runat="server" role="status" />
            <a href="ListadoInstituciones.aspx" class="link-lilaClaro mt-2">Volver al listado</a>
        </asp:Panel>

        <asp:Panel ID="pnlDetalle" runat="server" Visible="false">
        <!-- Carousel de imagenes -->
        <asp:Panel ID="pnlImagenes" runat="server" Visible="false">
            <div class="row m-4">
                <div class="col-12">
                    <% if (ImagenesVisibles == 1) { %>
                        <div class="card galeria-fotos">
                            <img src="<%: UrlFotoUnica %>" alt="<%: DescripcionFotoUnica %>" class="foto-unica">
                        </div>
                    <% } else { %>
                    <div class="card galeria-fotos">

                        <div id="carouselMultiple" class="carousel slide h-100 d-flex flex-column" data-bs-autoplay="true" data-bs-ends="wrap" style="--bs-carousel-items: <%= ImagenesVisibles %>; --bs-carousel-items-gap: var(--separacion-fotos);">

                            <!-- Controles -->
                            <div class="galeria-controles d-flex justify-content-between align-items-center mb-2">
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

                                <div class="galeria-botones">
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
                                            <img src="<%#: Eval("UrlPublica") %>"
                                                class="foto-galeria" alt="<%#: Eval("Descripcion") %>">
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>


                            </div>
                        </div>
                    </div>
                    <% } %>
                </div>
            </div>
        </asp:Panel>

        <!-- Datos generales -->
        <div class="row m-4">

            <!-- Información de la institución -->
            <div class="col-8">
                <div class="card h-100 p-5">
                    <asp:Label ID="lblNombreInstitucion" runat="server" CssClass="h2 text-indigo"></asp:Label>
                    <asp:Label ID="lblValoracion" runat="server" CssClass="d-block text-indigo my-2"></asp:Label>
                    <asp:Label ID="lblDescripcion" runat="server" CssClass="p"></asp:Label>

                    <hr />

                    <label class="text-indigo fw-bold mb-2 mt-2">Idiomas</label>
                    <div class="d-flex flex-wrap gap-2 mb-2">
                        <asp:Repeater ID="rptIdiomas" runat="server">
                            <ItemTemplate>
                                <div class="btn-lilaClaro">
                                    <%#: Container.DataItem %>
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
                        <asp:Repeater ID="rptTelefonos" runat="server">
                            <ItemTemplate>
                                <span class="d-block"><%#: Container.DataItem %></span>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>

                    <div class="mb-4">
                        <label class="text-indigo fw-bold">Dirección</label>
                        <asp:Label ID="lblDireccion" runat="server" CssClass="d-block"></asp:Label>
                    </div>

                    <asp:HyperLink ID="btnRegistrarAlumno" runat="server" CssClass="btn btn-lilaOscuro w-100" Text="Matricular"></asp:HyperLink>

                </div>
            </div>
        </div>

        <!-- Reseñas -->
        <div class="row m-4">
            <div class="col-12">
                <div class="card p-5">
                    <div class="d-flex align-items-center">
                        <h4 class="text-indigo fw-bold flex-grow-1">Reseñas de las familias y comunidad</h4>
                        <asp:Button ID="btnReseña" runat="server" CssClass="btn-lilaOscuro-invertido just" Text="Nueva reseña" OnClick="btnReseña_Click" CausesValidation="false"></asp:Button>
                    </div>

                    <asp:HyperLink ID="lnkIniciarSesionResenia" runat="server" Visible="false" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro d-inline-block my-3" Text="Iniciá sesión como responsable para dejar una reseña."></asp:HyperLink>
                    <asp:Label ID="lblMensajeResenia" runat="server" Visible="false" role="status"></asp:Label>

                    <asp:Panel ID="pnlFormularioResenia" runat="server" Visible="false">
                        <dialog id="dialogResenia" class="dialogo-resenia" aria-labelledby="tituloResenia">
                            <h3 id="tituloResenia" class="fw-bold mb-3">Nueva reseña</h3>
                            <p>Contanos tu experiencia con esta institución.</p>
                            <asp:Label ID="lblErrorResenia" runat="server" Visible="false" CssClass="alert alert-danger d-block mb-3" role="alert"></asp:Label>
                            <div class="mb-3">
                                <asp:Label runat="server" AssociatedControlID="ddlCalificacion" Text="Calificación" CssClass="form-label"></asp:Label>
                                <asp:DropDownList ID="ddlCalificacion" runat="server" CssClass="form-control">
                                    <asp:ListItem Value="">Seleccioná una puntuación</asp:ListItem>
                                    <asp:ListItem Value="1">1 estrella</asp:ListItem>
                                    <asp:ListItem Value="2">2 estrellas</asp:ListItem>
                                    <asp:ListItem Value="3">3 estrellas</asp:ListItem>
                                    <asp:ListItem Value="4">4 estrellas</asp:ListItem>
                                    <asp:ListItem Value="5">5 estrellas</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="mb-4">
                                <asp:Label runat="server" AssociatedControlID="txtComentarioResenia" Text="Comentario" CssClass="form-label"></asp:Label>
                                <asp:TextBox ID="txtComentarioResenia" runat="server" TextMode="MultiLine" Rows="4" MaxLength="255" ValidateRequestMode="Disabled" CssClass="form-control" aria-describedby="ayudaComentarioResenia"></asp:TextBox>
                                <small id="ayudaComentarioResenia" class="text-secondary">Hasta 255 caracteres.</small>
                            </div>
                            <div class="d-flex justify-content-end gap-2">
                                <button type="button" class="btn btn-lilaClaro" onclick="document.getElementById('dialogResenia').close()">Cancelar</button>
                                <asp:Button ID="btnPublicarResenia" runat="server" Text="Publicar reseña" OnClick="btnPublicarResenia_Click" CausesValidation="false" CssClass="btn btn-lilaOscuro"></asp:Button>
                            </div>
                        </dialog>
                        <script>
                            document.getElementById('dialogResenia').showModal();
                        </script>
                    </asp:Panel>

                    <asp:Repeater ID="rptResenias" runat="server">
                        <ItemTemplate>
                            <div class='<%# PuedeEliminarResenia(Convert.ToInt32(Eval("IdResponsable"))) ? "resenia-propia" : "border-bottom border-neutro py-4" %>'>
                                <asp:Panel runat="server" Visible='<%# PuedeEliminarResenia(Convert.ToInt32(Eval("IdResponsable"))) %>' CssClass="text-indigo fw-bold mb-3">
                                    Tu reseña
                                </asp:Panel>
                                <div class="d-flex justify-content-between align-items-center mb-2">
                                    <span class="fw-bold text-indigo"><%#: Eval("NombreResponsable") %> <%#: Eval("ApellidoResponsable") %></span>
                                    <div class="d-flex align-items-center gap-2">
                                        <small class="text-muted"><%# Eval("Fecha", "{0:dd/MM/yyyy}") %></small>
                                        <asp:LinkButton ID="btnEliminarResenia" runat="server" Text="Eliminar" CssClass="btn btn-sm btn-outline-danger rounded-pill"
                                            Visible='<%# PuedeEliminarResenia(Convert.ToInt32(Eval("IdResponsable"))) %>'
                                            CommandArgument='<%# Eval("IdResenia") %>' OnClick="btnEliminarResenia_Click" CausesValidation="false"
                                            OnClientClick="return confirmarAccion(this, '¿Querés eliminar tu reseña? También se eliminará la respuesta de la institución, si la tiene. Esta acción es definitiva.', 'Aceptar');"></asp:LinkButton>
                                    </div>
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

                    <asp:Panel ID="pnlPaginacionResenias" runat="server" Visible="false" CssClass="d-flex justify-content-center align-items-center flex-wrap gap-3 mt-3" role="navigation" aria-label="Páginas de reseñas">
                        <asp:LinkButton ID="btnReseniasAnterior" runat="server" Text="Anterior" CausesValidation="false"
                            CssClass="btn btn-sm btn-lilaClaro" OnClick="btnReseniasAnterior_Click" />
                        <asp:Label ID="lblPaginaResenias" runat="server" CssClass="text-indigo small" aria-live="polite" />
                        <asp:LinkButton ID="btnReseniasSiguiente" runat="server" Text="Siguiente" CausesValidation="false"
                            CssClass="btn btn-sm btn-lilaClaro" OnClick="btnReseniasSiguiente_Click" />
                    </asp:Panel>

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
        </asp:Panel>

    </div>

    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script>
        var institucion = <%= InstitucionJson %>;
        if (institucion && typeof L !== 'undefined') {
            var ubicacion = [institucion.Latitud, institucion.Longitud];
            var mapa = L.map('mapa').setView(ubicacion, 12);
            L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
                maxZoom: 19,
                attribution: '&copy; OpenStreetMap contributors'
            }).addTo(mapa);

            var popup = document.createElement('div');
            var nombre = document.createElement('b');
            nombre.textContent = institucion.Nombre;
            popup.appendChild(nombre);
            popup.appendChild(document.createElement('br'));
            popup.appendChild(document.createTextNode(institucion.Direccion || ''));
            L.marker(ubicacion).addTo(mapa).bindPopup(popup);
        }

        // Reset: al intentar avanzar más allá del final, vuelve al inicio
        (function () {
            var carrusel = document.getElementById('carouselMultiple');
            if (!carrusel) return;
            var inner = carrusel.querySelector('.carousel-inner');
            var visibles = <%= ImagenesVisibles %>;
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
