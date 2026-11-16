<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="ListadoInstituciones.aspx.cs" Inherits="Presentacion.ListadoInstituciones" %>
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
                <div class="col-3">
                    <asp:DropDownList runat="server" ID="ddlIdiomas" CssClass="form-control h-100 w-100" AppendDataBoundItems="true">
                        <asp:ListItem Value="0">Todos los idiomas</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-2">
                    <asp:Button runat="server" ID="btnFiltrar" CssClass="btn-lilaOscuro h-100 w-100" Text="Filtrar" OnClick="btnFiltrar_Click" />
                </div>
            </div>
        </div>
        
        <!-- LISTADO + MAPA -->
        <div class="card flex-grow-1 min-h-0 p-4 mt-4">
            <div class="row flex-grow-1 min-h-0">
                <!-- LISTADO -->
                <div class="col-6 h-100 overflow-auto">
                    <asp:Repeater ID="rptInstituciones" runat="server">
                        <ItemTemplate>
                            <div class="card mb-2 overflow-hidden" data-id='<%# Eval("Id") %>' style="cursor: pointer;">
                                <div class="row">
                                    <!-- IMAGEN -->
                                    <div class="col-4">
                                        <asp:Image ID="imgInstitucion" runat="server" ImageUrl='<%# Eval("Imagenes") != null && ((List<Dominio.Imagen>)Eval("Imagenes")).Count > 0 ? Eval("Imagenes[0].Url") : "https://placehold.co/400x250" %>' CssClass="w-100 h-100 d-block" Style="object-fit: cover; aspect-ratio: 4 / 3;" />
                                    </div>
                                    <!-- TEXTO -->
                                    <div class="col-8">
                                        <div class="card-body">
                                            <h5 class="card-title text-indigo"><%# Eval("Nombre") %></h5>
                                            <p class="card-text mb-1"><%# Eval("Direccion") %></p>
                                            <p class="card-text text-lavanda"><%# Eval("Descripcion") %></p>
                                            <asp:Button ID="btnDetalle" runat="server" CssClass="btn-lilaOscuro-invertido btn-sm w-100" Text="Más infromación" OnClick="btnDetalle_Click" CommandArgument='<%# Eval("Id") %>'/>
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

        instituciones.forEach(function (i) {
            marcadores[i.Id] = L.marker([i.Latitud, i.Longitud])
                .addTo(mapa)
                .bindPopup('<b>' + i.Nombre + '</b><br>' + i.Direccion);
        });

        // Seleccionar institución
        document.addEventListener('click', function (e) {
            var card = e.target.closest('[data-id]');
            console.log('click', card);
            if (!card) return;

            var marcador = marcadores[card.getAttribute('data-id')];
            console.log('marcador', marcador);
            if (!marcador) return;

            mapa.flyTo(marcador.getLatLng(), 16);
            marcador.openPopup();
        });
    </script>

</asp:Content>
