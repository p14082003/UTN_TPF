<%@ Page Title="Buscar instituciones" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="ListadoInstituciones.aspx.cs" Inherits="Presentacion.ListadoInstituciones" %>
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

        .filtros-principales {
            display: grid;
            grid-template-columns: minmax(0, 1.6fr) minmax(0, 1fr) auto;
            align-items: end;
            gap: 1rem;
        }

        .filtros-secundarios {
            display: grid;
            grid-template-columns: repeat(3, minmax(0, 1fr));
            align-items: end;
            gap: 1rem;
            margin-top: 1rem;
        }

        .filtros-instituciones {
            max-height: 55vh;
            overflow-y: auto;
        }

        .filtros-instituciones label, .orden-instituciones label {
            display: block;
            margin-bottom: 0.35rem;
        }

        .acciones-busqueda .btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            min-height: 2.75rem;
            padding: 0.625rem 1rem;
        }

        .estado-busqueda { display: block; margin-top: 0.5rem; }
        .estado-busqueda:empty { display: none; }

        .acciones-busqueda, .barra-resultados, .orden-instituciones {
            display: flex;
            align-items: center;
            flex-wrap: wrap;
            gap: 0.75rem;
        }

        .filtros-instituciones summary {
            color: #373A72;
            font-weight: 600;
            cursor: pointer;
            width: fit-content;
        }

        .filtro-vacantes {
            display: flex;
            align-items: center;
            min-height: 2.75rem;
        }

        .filtro-vacantes input { margin-right: 0.5rem; accent-color: #373A72; }
        .filtro-vacantes label { display: inline; margin: 0; }
        .barra-resultados { justify-content: space-between; margin-bottom: 1rem; }
        .orden-instituciones label { margin: 0; white-space: nowrap; }
        .orden-instituciones select { width: auto; }

        .resultados-instituciones {
            display: grid;
            grid-template-columns: repeat(2, minmax(0, 1fr));
            gap: 1rem;
            flex: 1;
            min-height: 0;
        }

        .lista-instituciones { overflow-y: auto; min-height: 0; }

        @media (max-width: 991px) {
            .filtros-principales { grid-template-columns: repeat(2, minmax(0, 1fr)); }
            .acciones-busqueda { grid-column: 1 / -1; }
            .filtros-secundarios { grid-template-columns: repeat(2, minmax(0, 1fr)); }
        }

        @media (max-width: 600px) {
            .filtros-principales, .filtros-secundarios { grid-template-columns: minmax(0, 1fr); }
            .resultados-instituciones { grid-template-columns: minmax(0, 1fr); grid-template-rows: minmax(12rem, 1fr) minmax(10rem, 0.7fr); }
        }

       * {
            scrollbar-color: #373A72 transparent;
        }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div id="busquedaInstituciones" class="container-fluid d-flex flex-column h-100 p-4"
        data-listado-url="<%= ResolveUrl("~/ListadoInstituciones.aspx") %>" data-avanzados-id="<%= hfFiltrosAvanzados.ClientID %>">

        <!-- FILTROS -->
        <div class="card flex-shrink-0 p-4 filtros-instituciones">
            <div class="filtros-principales">
                <div>
                    <label for="<%= txtFiltro.ClientID %>" class="text-indigo">Buscar institución</label>
                    <asp:TextBox runat="server" ID="txtFiltro" MaxLength="150" placeholder="Nombre o dirección" CssClass="form-control campo-instired w-100" data-parametro="texto" />
                </div>
                <div>
                    <label for="<%= ddlOfertaEducativa.ClientID %>" class="text-indigo">Oferta educativa</label>
                    <asp:DropDownList ID="ddlOfertaEducativa" runat="server" CssClass="form-select w-100" data-parametro="oferta" OnSelectedIndexChanged="OfertaEducativa_SelectedIndexChanged">
                        <asp:ListItem Value="0">Todas las ofertas</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="acciones-busqueda">
                    <asp:Button runat="server" ID="btnFiltrar" CssClass="btn btn-lilaOscuro px-4" Text="Filtrar" data-filtrar="true" />
                    <a href="ListadoInstituciones.aspx" class="btn btn-lilaClaro text-decoration-none" data-limpiar="true">Limpiar filtros</a>
                </div>
            </div>
            <span id="estadoBusqueda" class="estado-busqueda small text-indigo" role="status" aria-live="polite"></span>
            <asp:HiddenField ID="hfFiltrosAvanzados" runat="server" Value="false" />
            <details id="filtrosAvanzados" class="mt-3" <%= hfFiltrosAvanzados.Value == "true" ? "open" : "" %>>
                <summary>Más filtros</summary>
                <div class="filtros-secundarios">
                    <div>
                        <label for="<%= ddlAnio.ClientID %>" class="text-indigo">Sala / grado / año</label>
                        <asp:DropDownList ID="ddlAnio" runat="server" CssClass="form-select w-100" Enabled="false" data-parametro="anio">
                            <asp:ListItem Value="0">Elegí un nivel escolar</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label for="<%= ddlCicloLectivo.ClientID %>" class="text-indigo">Ciclo lectivo</label>
                        <asp:DropDownList ID="ddlCicloLectivo" runat="server" CssClass="form-select w-100" data-parametro="cicloLectivo">
                            <asp:ListItem Value="0">Actual o próximo</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label for="<%= ddlTurno.ClientID %>" class="text-indigo">Turno</label>
                        <asp:DropDownList ID="ddlTurno" runat="server" CssClass="form-select w-100" data-parametro="idTurno">
                            <asp:ListItem Value="0">Todos los turnos</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label for="<%= ddlIdiomas.ClientID %>" class="text-indigo">Idioma</label>
                        <asp:DropDownList runat="server" ID="ddlIdiomas" CssClass="form-select w-100" AppendDataBoundItems="true" data-parametro="idIdioma">
                            <asp:ListItem Value="0">Todos los idiomas</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label for="<%= ddlDiscapacidades.ClientID %>" class="text-indigo">Inclusión y accesibilidad</label>
                        <asp:DropDownList runat="server" ID="ddlDiscapacidades" CssClass="form-select w-100" data-parametro="accesibilidad">
                            <asp:ListItem Value="0">Sin filtro de accesibilidad</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="filtro-vacantes" data-vacantes="true">
                        <asp:CheckBox ID="chkSoloConVacantes" runat="server" Text="Solo con vacantes disponibles" />
                    </div>
                </div>
                <p class="small text-muted mt-3 mb-0">El grado, ciclo, turno y las vacantes se buscan juntos en un mismo curso.</p>
            </details>
        </div>
        
        <!-- LISTADO + MAPA -->
        <div id="resultadosBusqueda" class="card flex-grow-1 min-h-0 p-4 mt-4" aria-busy="false">
            <div class="barra-resultados">
                <asp:Label ID="lblCantidad" runat="server" CssClass="text-indigo fw-semibold" role="status" data-contador="true" />
                <div class="orden-instituciones">
                    <label for="<%= ddlOrdenar.ClientID %>" class="text-indigo">Ordenar por</label>
                    <asp:DropDownList ID="ddlOrdenar" runat="server" CssClass="form-select" data-parametro="orden">
                        <asp:ListItem Value="nombre">Nombre</asp:ListItem>
                        <asp:ListItem Value="valoracion">Mejor valoración</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="resultados-instituciones">
                <!-- LISTADO -->
                <div id="listaInstituciones" class="lista-instituciones">
                    <asp:Label ID="lblListado" runat="server" CssClass="d-block mb-3" Visible="false" role="status" />
                    <asp:Repeater ID="rptInstituciones" runat="server">
                        <ItemTemplate>
                            <div class="tarjeta-institucion card mb-2 overflow-hidden" data-id='<%# Eval("IdInstitucion") %>' style="cursor: pointer;">
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
                <div class="mapa-instituciones h-100">
                    <div id="mapa"></div>
                </div>
            </div>
        </div>
    </div>

    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script type="application/json" id="datosInstituciones"><%= InstitucionesJson.Replace("<", "\\u003c") %></script>
    <script src="<%= ResolveUrl("~/Scripts/listado-instituciones.js") %>"></script>

</asp:Content>
