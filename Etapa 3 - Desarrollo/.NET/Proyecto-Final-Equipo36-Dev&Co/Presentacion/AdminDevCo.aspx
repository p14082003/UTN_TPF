<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="AdminDevCo.aspx.cs" Inherits="Presentacion.AdminDevCo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2">
            <h1 tabindex="0" class="text-indigo fw-bold">Panel de Control - Dev&Co </h1>
            <p class="text-muted">Gestión de solicitudes de nuevas instituciones.</p>
        </header>

        <!-- VISTA 1: DASHBOARD -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="row mb-5">
            <div class="col-md-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-hourglass-split text-lavanda fs-4 me-2"></i>Pendientes</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">12</h2>
                    </div>
                </div>
            </div>
            <div class="col-md-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-building-check text-lavanda fs-4 me-2"></i>Activas</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">45</h2>
                    </div>
                </div>
            </div>
            <div class="col-md-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-x-octagon text-lavanda fs-4 me-2"></i>Rechazadas</h5>
                        <!--Cantidad hardcodeada -->
                        <h2 class="display-4 fw-bold text-indigo mt-3">3</h2>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 2: LISTADO DE POSTULACIONES -->
        <asp:Panel ID="pnlListado" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Instituciones Postuladas</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">
                <div class="table-responsive">
                    <asp:GridView ID="dgvInstituciones" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <Columns>
                            <asp:BoundField DataField="Cuit" HeaderText="CUIT" />
                            <asp:BoundField DataField="Nombre" HeaderText="Nombre Institución" />
                            <asp:BoundField DataField="Email" HeaderText="Correo Electrónico" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerDetalle" runat="server" CssClass="btn btn-sm btn-outline-lavanda rounded-pill px-3 fw-bold" OnClick="btnVerDetalle_Click">
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
                <h5 class="mb-0">Revisión de Solicitud</h5>
                <asp:Button ID="btnCerrarDetalle" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3" OnClick="btnCerrarDetalle_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-4">
                <div class="p-4 bg-fondo rounded-4 border border-neutro mb-4">
                    <div class="row">
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">Institución</span>
                            <span class="text-indigo fw-bold fs-5">Colegio San Martín</span>
                        </div>
                        <div class="col-md-6 mb-2">
                            <span class="text-muted d-block small">CUIT</span>
                            <span class="text-indigo fw-bold fs-5">30-12345678-9</span>
                        </div>
                    </div>
                </div>

                <div class="d-flex justify-content-end gap-3 mt-4">
                    <asp:Button ID="btnDesplegarRechazo" runat="server" Text="Rechazar" CssClass="btn btn-outline-danger fw-bold rounded-pill px-4" OnClick="btnDesplegarRechazo_Click" />
                    <asp:Button ID="btnAprobar" runat="server" Text="Aprobar Institución" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnAprobar_Click" />
                </div>

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
