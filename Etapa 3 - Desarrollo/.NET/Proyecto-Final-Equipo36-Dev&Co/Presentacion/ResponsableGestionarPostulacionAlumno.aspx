<%@ Page Title="Gestión de postulación" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="ResponsableGestionarPostulacionAlumno.aspx.cs" Inherits="Presentacion.ResponsableGestionarPostulacionAlumno" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid">

        <!-- Titulo -->
        <div class="m-4">
            <h1 class="text-indigo fw-bold text-center">Gestión de postulación</h1>
            <h5 class="text-center">Consultá el detalle de la postulación y realizá las acciones disponibles según su estado.</h5>
        </div>
        <!-- Tarjeta -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-4"></asp:Label>

                    <asp:Panel ID="pnlDatos" runat="server" Visible="false">
                    <!-- Datos del alumno -->
                    <div class="d-flex align-items-center gap-4 mb-4">
                        <h2 class="text-indigo mb-0">
                            <i class="bi bi-mortarboard"></i>
                        </h2>
                        <div class="flex-grow-1">
                            <h3 class="text-indigo fw-bold mb-0">
                                <asp:Label ID="lblNombreCompleto" runat="server"></asp:Label>
                            </h3>
                            <asp:Label ID="lblDocumento" runat="server" CssClass="d-block fw-semibold text-secondary"></asp:Label>
                        </div>
                        <h3 class="mb-0">
                            <asp:Label ID="lblEstado" runat="server" CssClass="badge bg-lavanda rounded-pill px-4 py-2"></asp:Label>
                        </h3>
                    </div>
                    <hr class="border-neutro mb-4" />

                    <!-- Datos de la postulación -->
                    <h4 class="text-indigo mb-4">Datos de la postulación</h4>
                    <p>Código de seguimiento: <strong><asp:Literal ID="litCodigoSeguimiento" runat="server" Mode="Encode"></asp:Literal></strong></p>

                    <div class="row mb-4">

                        <!-- Anio -->
                        <asp:Panel ID="pnlAnio" runat="server" Visible="false" CssClass="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-mortarboard fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblAnioTitulo" runat="server" Text="Año" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblAnio" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>

                        <!-- Nivel -->
                        <asp:Panel ID="pnlNivel" runat="server" Visible="false" CssClass="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-mortarboard fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblNivelTitulo" runat="server" Text="Nivel" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblNivel" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>

                        <!-- Nivel personalizado -->
                        <asp:Panel ID="pnlNivelPersonalizado" runat="server" Visible="true" CssClass="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-mortarboard fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblNivelPersonalizadoTitulo" runat="server" Text="Nivel Personalizado" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblNivelPersonalizado" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>

                        <!-- División -->
                        <asp:Panel ID="pnlDivision" runat="server" Visible="true" CssClass="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-mortarboard fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblDivisionTitulo" runat="server" Text="División" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblDivision" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>

                        <!-- Fecha de postulación -->
                        <div class="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-calendar-event fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblFechaPostulacionTitulo" runat="server" Text="Fecha de postulación" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblFechaPostulacion" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </div>

                        <!-- Ciclo lectivo -->
                        <div class="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-calendar-event fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblCicloLectivoTitulo" runat="server" Text="Ciclo Lectivo" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblCicloLectivo" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </div>

                        <!-- Institución -->
                        <div class="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-building fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblInstitucionTitulo" runat="server" Text="Institución" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblInstitucion" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </div>

                        <!-- Modalidad -->
                        <div class="col-6 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-people fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblTurnoTitulo" runat="server" Text="Turno" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblTurno" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </div>

                        <!-- Motivo de rechazo -->
                        <asp:Panel ID="pnlMotivoRechazo" runat="server" Visible="true" CssClass="col-12 mb-3">
                            <div class="bg-fondo border border-neutro rounded-3 d-flex align-items-center gap-3 p-3">
                                <div class="bg-lavanda rounded-3 px-3 py-2">
                                    <i class="bi bi-people fs-4"></i>
                                </div>
                                <div>
                                    <asp:Label ID="lblMotivoRechazoTitulo" runat="server" Text="Motivo de rechazo" CssClass="d-block text-secondary"></asp:Label>
                                    <asp:Label ID="lblMotivoRechazo" runat="server" CssClass="d-block fw-bold text-indigo"></asp:Label>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                    <hr class="border-neutro mb-4" />
                    <!-- Botones según estado -->
                    <asp:Label ID="lblVacantesAviso" runat="server" Visible="false" CssClass="alert alert-info d-block mb-3"></asp:Label>
                    <asp:Button ID="btnConfirmar" runat="server" Visible="false" Text="Confirmar postulación" OnClick="btnConfirmar_Click" CssClass="btn btn-lilaOscuro w-100 mb-3" />
                    <asp:Button ID="btnCancelarPostulacion" runat="server" Visible="false" Text="Cancelar postulación" OnClientClick="return confirmarAccion(this, 'Si se cancela esta postulación perderá el cupo reservado en el curso ¿Desea continuar?', 'Aceptar');"  OnClick="btnCancelarPostulacion_Click" CausesValidation="false" CssClass="btn btn-solid theme-danger w-100 mb-3" style="--bs-btn-padding-y: 14px; --bs-btn-border-radius: 15px;" />
                    </asp:Panel>
                    <asp:Button ID="btnVolver" runat="server" Text="Volver al listado" OnClick="btnVolver_Click" CausesValidation="false" CssClass="btn btn-lilaClaro w-100" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
