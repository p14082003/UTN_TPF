<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="ListadoInstituciones.aspx.cs" Inherits="Presentacion.ListadoInstituciones" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />

    <style>
        #mapa {
            width: 100%;
            height: 100%;
            z-index: 1;
        }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid d-flex flex-column bg-fondo h-100 p-2">

        <!-- FILTROS -->
        <div class="row flex-shrink-0 m-2">
            <div class="col-4 d-flex">
                <asp:TextBox runat="server" ID="txtFiltro" placeholder="Busqueda avanzada" CssClass="form-control"></asp:TextBox>
            </div>
            <div class="col-3 d-flex">
                <asp:DropDownList runat="server" ID="ddlDiscapacidades" CssClass="form-control w-100">
                    <asp:ListItem Value="0">Todas las discapacidades</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-3 d-flex">
                <asp:DropDownList runat="server" ID="ddlIdiomas" CssClass="form-control w-100">
                    <asp:ListItem Value="0">Todos los idiomas</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="col-2 d-flex">
                <asp:Button runat="server" ID="btnLimpiarFiltros" CssClass="btn-lilaOscuro w-100" Text="Limpiar Filtros" />
            </div>
        </div>

        <!-- LISTADO + MAPA -->
        <div class="row flex-shrink-1 m-2" style="min-height: 0;">

            <div class="col-6 h-100 p-3" style="overflow-y: auto; overflow-x: hidden; scrollbar-color: #373A72 transparent;">
                <asp:Repeater ID="rptInstituciones" runat="server">
                    <ItemTemplate>
                        <div class="card mb-3 overflow-hidden" data-id='<%# Eval("Id") %>' style="cursor: pointer;">
                            <div class="row">

                                <!-- IMAGEN -->
                                <div class="col-4">
                                    <img src="https://placehold.co/400x250" class="w-100 h-100 d-block" style="object-fit: cover; aspect-ratio: 4 / 3;" />
                                </div>

                                <!-- TEXTO -->
                                <div class="col-8">
                                    <div class="card-body">
                                        <h5 class="card-title"><%# Eval("Nombre") %></h5>
                                        <p class="card-text mb-1"><%# Eval("Direccion") %></p>
                                        <p class="card-text"><%# Eval("Descripcion") %></p>
                                    </div>
                                </div>

                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="col-6 h-100 p-3">
                <div id="mapa" class="h-100"></div>
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
