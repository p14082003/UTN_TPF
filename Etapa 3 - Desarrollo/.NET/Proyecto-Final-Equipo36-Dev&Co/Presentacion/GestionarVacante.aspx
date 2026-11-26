<%@ Page Title="InstiRed - Cursos y vacantes" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="GestionarVacante.aspx.cs" Inherits="Presentacion.GestionarVacante" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .encabezado-curso {
            display: grid;
            grid-template-columns: minmax(4.5rem, 1fr) minmax(0, max-content) minmax(4.5rem, 1fr);
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 1.5rem;
        }

        .encabezado-curso h5 {
            grid-column: 2;
            text-align: center;
        }

        .encabezado-curso .btn {
            grid-column: 3;
            justify-self: end;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2 d-flex align-items-center justify-content-between flex-wrap gap-2">
            <div>
                <h1 tabindex="0" class="text-indigo fw-bold">Gestión de vacantes</h1>
                <p class="text-muted mb-0">Actualizá las vacantes disponibles de cada curso. Los padres, madres y tutores ven esta disponibilidad al buscar instituciones.</p>
            </div>
            <asp:Button ID="btnNuevoCurso" runat="server" Text="Nuevo curso" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnNuevoCurso_Click" />
        </header>

        <!-- MENSAJE DE RESULTADO -->
        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert mb-4">
            <asp:Literal ID="litMensaje" runat="server" />
        </asp:Panel>

        <!-- VISTA 1: RESUMEN -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="resumen-gestion row mb-4">
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-journal-bookmark text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Cursos</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblTotalCursos" runat="server" Text="0" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-door-open text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Con vacantes</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblConVacantes" runat="server" Text="0" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-door-closed text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Completos</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblSinVacantes" runat="server" Text="0" /></h2>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 2: LISTADO DE CURSOS -->
        <asp:Panel ID="pnlListado" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Cursos de la institución</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- Filtros -->
                <div class="row g-3 mb-3">
                    <div class="col-12 col-sm-6 col-md-4">
                        <label for="<%= ddlCicloLectivo.ClientID %>" class="form-label text-indigo fw-semibold">Año calendario</label>
                        <asp:DropDownList ID="ddlCicloLectivo" runat="server" CssClass="form-select border-neutro" AutoPostBack="true"
                            OnSelectedIndexChanged="Filtro_SelectedIndexChanged" />
                    </div>
                    <div class="col-12 col-sm-6 col-md-4">
                        <label for="<%= ddlNivel.ClientID %>" class="form-label text-indigo fw-semibold">Nivel</label>
                        <asp:DropDownList ID="ddlNivel" runat="server" CssClass="form-select border-neutro" AutoPostBack="true"
                            OnSelectedIndexChanged="Filtro_SelectedIndexChanged" />
                    </div>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="dgvCursos" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" DataKeyNames="Id"
                        AllowPaging="true" PageSize="25" OnPageIndexChanging="dgvCursos_PageIndexChanging"
                        EmptyDataText="No hay cursos para mostrar con este filtro.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <EmptyDataRowStyle CssClass="text-muted text-center py-4" />
                        <PagerStyle CssClass="text-indigo" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="NombreCurso" HeaderText="Curso" />
                            <asp:BoundField DataField="CicloLectivo" HeaderText="Año calendario" />
                            <asp:BoundField DataField="Turno" HeaderText="Turno" />
                            <asp:BoundField DataField="Capacidad" HeaderText="Capacidad" />
                            <asp:BoundField DataField="Inscriptos" HeaderText="Inscriptos" />
                            <asp:TemplateField HeaderText="Vacantes">
                                <ItemTemplate>
                                    <span class="badge rounded-pill px-3 py-2 <%# Convert.ToInt32(Eval("Vacantes")) == 0 ? "bg-secondary" : "bg-lavanda" %>">
                                        <%# Convert.ToInt32(Eval("Vacantes")) == 0 ? "Completo" : Eval("Vacantes").ToString() %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnEditar" runat="server" CssClass="btn btn-sm btn-outline-lavanda rounded-pill px-3 fw-bold"
                                        CommandArgument='<%# Eval("Id") %>' OnClick="btnEditar_Click">
                                        Actualizar vacantes
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 3: ACTUALIZAR VACANTES DE UN CURSO -->
        <asp:Panel ID="pnlEdicion" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white encabezado-curso">
                <h5 class="mb-0">Actualizar vacantes</h5>
                <asp:Button ID="btnCerrarEdicion" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3" OnClick="btnCerrarEdicion_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- Datos del curso -->
                <div class="p-4 bg-fondo rounded-4 border border-neutro mb-4">
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Curso</span>
                            <asp:Label ID="lblCursoNombre" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Año calendario</span>
                            <asp:Label ID="lblCicloLectivo" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Turno</span>
                            <asp:Label ID="lblTurno" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Capacidad</span>
                            <asp:Label ID="lblCapacidad" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Inscriptos actuales</span>
                            <asp:Label ID="lblInscriptos" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                    </div>
                </div>

                <!-- Nueva cantidad de vacantes -->
                <div class="mb-2" style="max-width: 280px;">
                    <label for="<%= txtVacantes.ClientID %>" class="form-label text-indigo fw-semibold">Vacantes disponibles</label>
                    <asp:TextBox ID="txtVacantes" runat="server" TextMode="Number" CssClass="form-control border-neutro rounded-3" />
                    <div class="form-text">Ingresá cuántas vacantes querés ofrecer. Los inscriptos no cambian: se ajusta la capacidad del curso. Con 0 el curso figura como completo.</div>
                </div>

                <div class="d-flex justify-content-end gap-3 mt-4">
                    <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-lilaClaro fw-bold rounded-pill px-4" OnClick="btnCerrarEdicion_Click" />
                    <asp:Button ID="btnGuardar" runat="server" Text="Guardar vacantes" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnGuardar_Click" />
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 4: ALTA DE CURSO -->
        <asp:Panel ID="pnlAlta" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white text-center">
                <h5 class="mb-0">Alta de curso</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <div class="row g-3 mb-3">
                    <div class="col-12 col-md-6">
                        <label for="<%= ddlAltaCiclo.ClientID %>" class="form-label text-indigo fw-semibold">Año calendario</label>
                        <asp:DropDownList ID="ddlAltaCiclo" runat="server" CssClass="form-select border-neutro" />
                    </div>
                    <div class="col-12 col-md-6">
                        <label for="<%= ddlAltaTurno.ClientID %>" class="form-label text-indigo fw-semibold">Turno</label>
                        <asp:DropDownList ID="ddlAltaTurno" runat="server" CssClass="form-select border-neutro" />
                    </div>
                </div>

                <!-- Instituciones comunes: nivel + año -->
                <asp:Panel ID="pnlAltaEstandar" runat="server" CssClass="p-3 bg-fondo rounded-4 border border-neutro mb-3">
                    <div class="row g-3">
                        <div class="col-12 col-md-6">
                            <label for="<%= ddlAltaNivel.ClientID %>" class="form-label text-indigo fw-semibold">Nivel</label>
                            <asp:DropDownList ID="ddlAltaNivel" runat="server" CssClass="form-select border-neutro" />
                        </div>
                        <div class="col-12 col-md-6">
                            <label for="<%= txtAltaAnio.ClientID %>" class="form-label text-indigo fw-semibold">Año</label>
                            <asp:TextBox ID="txtAltaAnio" runat="server" TextMode="Number" CssClass="form-control border-neutro" />
                            <div class="form-text">Preescolar: 3 a 5 · Primaria: 1 a 7 · Secundaria: 1 a 6.</div>
                        </div>
                    </div>
                </asp:Panel>

                <!-- Institutos de idiomas: nombre personalizado -->
                <asp:Panel ID="pnlAltaPersonalizado" runat="server" Visible="false" CssClass="p-3 bg-fondo rounded-4 border border-neutro mb-3">
                    <label for="<%= txtAltaPersonalizado.ClientID %>" class="form-label text-indigo fw-semibold">Nombre del curso</label>
                    <asp:TextBox ID="txtAltaPersonalizado" runat="server" CssClass="form-control border-neutro" MaxLength="30" />
                    <div class="form-text">Ej: 2nd Children. Los institutos de idiomas registran sus cursos con nombre propio.</div>
                </asp:Panel>

                <div class="row g-3 mb-2">
                    <div class="col-12 col-md-6">
                        <label for="<%= txtAltaDivision.ClientID %>" class="form-label text-indigo fw-semibold">División</label>
                        <asp:TextBox ID="txtAltaDivision" runat="server" CssClass="form-control border-neutro" MaxLength="50" />
                        <div class="form-text">Si hay una sola, completá con 1. Ej: A, Salita azul, Lunes y miércoles.</div>
                    </div>
                    <div class="col-6 col-md-3">
                        <label for="<%= txtAltaCapacidad.ClientID %>" class="form-label text-indigo fw-semibold">Capacidad</label>
                        <asp:TextBox ID="txtAltaCapacidad" runat="server" TextMode="Number" CssClass="form-control border-neutro" />
                    </div>
                    <div class="col-6 col-md-3">
                        <label for="<%= txtAltaInscriptos.ClientID %>" class="form-label text-indigo fw-semibold">Inscriptos</label>
                        <asp:TextBox ID="txtAltaInscriptos" runat="server" TextMode="Number" CssClass="form-control border-neutro" Text="0" />
                    </div>
                </div>

                <div class="d-flex justify-content-end gap-3 mt-4">
                    <asp:Button ID="btnCancelarAlta" runat="server" Text="Cancelar" CssClass="btn btn-lilaClaro fw-bold rounded-pill px-4" OnClick="btnCancelarAlta_Click" />
                    <asp:Button ID="btnGuardarCurso" runat="server" Text="Crear curso" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnGuardarCurso_Click" />
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
