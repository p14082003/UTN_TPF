<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="GestionarPostulacionAlumno.aspx.cs" Inherits="Presentacion.GestionarPostulacionAlumno" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2">
            <h1 tabindex="0" class="text-indigo fw-bold">Postulaciones recibidas</h1>
            <p class="text-muted">Gestión de las postulaciones de alumnos a tu institución. El tutor recibe una notificación con cada decisión.</p>
        </header>

        <!-- MENSAJE DE RESULTADO DE LA ÚLTIMA ACCIÓN -->
        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert mb-4">
            <asp:Literal ID="litMensaje" runat="server" />
        </asp:Panel>

        <!-- VISTA 1: DASHBOARD -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="row mb-5">
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-hourglass-split text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Pendientes</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">8</h2>
                    </div>
                </div>
            </div>
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-check-circle text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Aceptadas</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">21</h2>
                    </div>
                </div>
            </div>
            <div class="col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-x-octagon text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Rechazadas</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">5</h2>
                    </div>
                </div>
            </div>
        </asp:Panel>


        <!-- VISTA 2: LISTADO DE POSTULACIONES -->
        <asp:Panel ID="pnlListado" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Postulaciones pendientes</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">
                <div class="table-responsive">
                    <asp:GridView ID="dgvPostulaciones" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" DataKeyNames="IdPostulacion"
                        EmptyDataText="No hay postulaciones pendientes.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <EmptyDataRowStyle CssClass="text-muted text-center py-4" />
                        <Columns>
                            <asp:BoundField DataField="NombreAlumno" HeaderText="Alumno" />
                            <asp:BoundField DataField="Curso" HeaderText="Curso" />
                            <asp:BoundField DataField="FechaPostulacion" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="NombreTutor" HeaderText="Tutor" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerDetalle" runat="server" CssClass="btn btn-sm btn-outline-lavanda rounded-pill px-3 fw-bold"
                                        CommandArgument='<%# Eval("IdPostulacion") %>' OnClick="btnVerDetalle_Click">
                                        Ver Detalle
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 3: DETALLE DE LA POSTULACIÓN -->
        <asp:Panel ID="pnlDetalle" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white py-3 d-flex justify-content-between align-items-center">
                <h5 class="mb-0">Revisión de Postulación</h5>
                <asp:Button ID="btnCerrarDetalle" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3" OnClick="btnCerrarDetalle_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- Datos del alumno y la postulación (valores de ejemplo) -->
                <div class="p-4 bg-fondo rounded-4 border border-neutro mb-4">
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Alumno</span>
                            <span class="text-indigo fw-bold fs-5">Martina Gómez</span>
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Curso</span>
                            <span class="text-indigo fw-bold fs-5">1° año - Turno mañana</span>
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Tutor</span>
                            <span class="text-indigo fw-bold fs-5">Laura Gómez</span>
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Correo del tutor</span>
                            <span class="text-indigo fw-bold fs-5">laura.gomez@mail.com</span>
                        </div>
                    </div>
                </div>

                <!-- Documentación (valores de ejemplo) -->
                <div class="mb-4">
                    <span class="text-muted d-block small mb-2">Documentación presentada</span>
                    <ul class="list-unstyled mb-0">
                        <li class="p-3 bg-fondo rounded-4 border border-neutro mb-2">
                            <i class="bi bi-file-earmark-text text-lavanda me-2"></i>
                            <asp:HyperLink ID="hlDocumento1" runat="server" CssClass="link-lilaClaro" Target="_blank" NavigateUrl="#">DNI del alumno.pdf</asp:HyperLink>
                        </li>
                        <li class="p-3 bg-fondo rounded-4 border border-neutro mb-2">
                            <i class="bi bi-file-earmark-text text-lavanda me-2"></i>
                            <asp:HyperLink ID="hlDocumento2" runat="server" CssClass="link-lilaClaro" Target="_blank" NavigateUrl="#">Certificado de estudios.pdf</asp:HyperLink>
                        </li>
                    </ul>
                </div>

                <!-- Vacantes: el code-behind muestra uno de los dos paneles -->
                <asp:Panel ID="pnlConVacantes" runat="server" CssClass="p-3 bg-fondo rounded-4 border border-neutro">
                    <i class="bi bi-check-circle-fill text-lavanda me-2"></i>
                    <span class="text-indigo">Vacantes disponibles en este curso:
                        <asp:Label ID="lblVacantes" runat="server" CssClass="fw-bold" Text="3" />.
                        Al aceptar la postulación se descuenta una vacante.
                    </span>
                </asp:Panel>

                <asp:Panel ID="pnlSinVacantes" runat="server" Visible="false" CssClass="p-3 rounded-4 border border-danger bg-fondo">
                    <i class="bi bi-exclamation-triangle-fill text-danger me-2"></i>
                    <span class="text-danger fw-bold">No hay vacantes disponibles en este curso.</span>
                    <span class="text-indigo">Solo podés rechazar la postulación.</span>
                </asp:Panel>

                <!-- Acciones -->
                <div class="d-flex justify-content-end gap-3 mt-4">
                    <asp:Button ID="btnDesplegarRechazo" runat="server" Text="Rechazar" CssClass="btn btn-outline-danger fw-bold rounded-pill px-4" OnClick="btnDesplegarRechazo_Click" />
                    <asp:Button ID="btnAceptar" runat="server" Text="Aceptar Postulación" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnAceptar_Click" />
                </div>

                <!-- Rechazo con motivo -->
                <asp:Panel ID="panelRechazo" runat="server" CssClass="mt-4" Visible="false">
                    <div class="card card-body bg-fondo border-danger rounded-4">
                        <label class="form-label text-danger fw-bold">Motivo del rechazo (Obligatorio)</label>
                        <asp:TextBox ID="txtMotivoRechazo" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control mb-3 border-neutro rounded-3"></asp:TextBox>
                        <div class="text-end">
                            <asp:Button ID="btnConfirmarRechazo" runat="server" Text="Confirmar Rechazo" CssClass="btn btn-danger fw-bold rounded-pill px-4" OnClick="btnConfirmarRechazo_Click" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
