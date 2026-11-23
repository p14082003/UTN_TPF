<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="ListadoInstituciones.aspx.cs" Inherits="Presentacion.ListadoInstituciones" %>
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

        .idioma-y-botones {
            display: grid;
            grid-template-columns:
                minmax(0, 1fr)
                min(2rem, 12%)
                min(7.5rem, 30%);
            gap: min(0.875rem, 4%);
        }

        .btn-limpiar-filtros {
            padding: 0;
            font-size: 1.35rem;
            line-height: 1;
        }

       * {
            scrollbar-color: #373A72 transparent;
        }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid d-flex flex-column h-100 p-4">

        <!-- FILTROS -->
        <div class="card flex-shrink-0 p-4">
            <div class="row">
                <div class="col-4">
                    <asp:TextBox runat="server" ID="txtFiltro" placeholder="Busqueda avanzada" CssClass="form-control h-100 w-100"></asp:TextBox>
                </div>
                <div class="col-3">
                    <asp:DropDownList runat="server" ID="ddlDiscapacidades" CssClass="form-control h-100 w-100">
                        <asp:ListItem Value="0">Todas las discapacidades</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-5 idioma-y-botones">
                    <asp:DropDownList runat="server" ID="ddlIdiomas" CssClass="form-control h-100 w-100" AppendDataBoundItems="true">
                        <asp:ListItem Value="0">Todos los idiomas</asp:ListItem>
                    </asp:DropDownList>
                    <a href="ListadoInstituciones.aspx" class="btn-lilaOscuro-invertido btn-limpiar-filtros d-flex align-items-center justify-content-center text-decoration-none" title="Limpiar filtros" aria-label="Limpiar filtros">X</a>
                    <asp:Button runat="server" ID="btnFiltrar" CssClass="btn-lilaOscuro h-100 w-100 px-0" Text="Filtrar" />
                </div>
            </div>
        </div>
        
        <!-- LISTADO + MAPA -->
        <div class="card flex-grow-1 min-h-0 p-4 mt-4">
            <div class="row flex-grow-1 min-h-0">
                <!-- LISTADO -->
                <div class="col-6 h-100 overflow-auto">
                    <asp:Label ID="lblListado" runat="server" CssClass="d-block mb-3" Visible="false" role="status" />
                    <asp:Repeater ID="rptInstituciones" runat="server">
                        <ItemTemplate>
                            <div class="card mb-2 overflow-hidden" data-id='<%# Eval("IdInstitucion") %>' style="cursor: pointer;">
                                <div class="row">
                                    <!-- IMAGEN -->
                                    <div class="col-4">
                                        <img src="<%#: Eval("UrlFoto") %>" alt="" loading="lazy" class="w-100 h-100 d-block"
                                             data-imagen-generica="<%#: ImagenGenerica %>" onerror="this.onerror = null; this.src = this.dataset.imagenGenerica;"
                                             style="object-fit: cover; aspect-ratio: 4 / 3;" />
                                    </div>
                                    <!-- TEXTO -->
                                    <div class="col-8">
                                        <div class="card-body">
                                            <h5 class="card-title text-indigo"><%#: Eval("Nombre") %></h5>
                                            <p class="card-text text-indigo mb-1"><%#: Eval("Valoracion") %></p>
                                            <p class="card-text mb-1"><%#: Eval("Direccion") %></p>
                                            <p class="card-text text-lavanda"><%#: Eval("Descripcion") %></p>
                                            <asp:HyperLink ID="lnkDetalle" runat="server" CssClass="btn btn-lilaOscuro-invertido btn-sm w-100" Text="Más información" NavigateUrl='<%# Eval("IdInstitucion", "~/DetalleInstitucion.aspx?id={0}") %>' />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                <!-- MAPA -->
                <div class="col-6 h-100">
                    <div id="mapa"></div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script>
        // Generar mapa
        var mapa = L.map('mapa').setView([-34.6037, -58.3816], 12);
        L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(mapa);

        // Generar marcadores
        var instituciones = <%= InstitucionesJson %>;
        var marcadores = {};
        var limites = L.latLngBounds();

        instituciones.forEach(function (i) {
            var popup = document.createElement('div');
            var nombre = document.createElement('b');
            nombre.textContent = i.Nombre;
            popup.appendChild(nombre);
            popup.appendChild(document.createElement('br'));
            popup.appendChild(document.createTextNode(i.Direccion || ''));

            marcadores[i.Id] = L.marker([i.Latitud, i.Longitud])
                .addTo(mapa)
                .bindPopup(popup);
            limites.extend(marcadores[i.Id].getLatLng());
        });

        if (limites.isValid()) {
            mapa.fitBounds(limites, { padding: [20, 20], maxZoom: 15 });
        }

        // Seleccionar institución
        document.addEventListener('click', function (e) {
            var card = e.target.closest('[data-id]');
            if (!card) return;

            var marcador = marcadores[card.getAttribute('data-id')];
            if (!marcador) return;

            mapa.flyTo(marcador.getLatLng(), 16);
            marcador.openPopup();
        });
    </script>

</asp:Content>
