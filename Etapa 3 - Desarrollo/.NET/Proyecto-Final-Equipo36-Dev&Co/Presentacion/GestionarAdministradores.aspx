<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="GestionarAdministradores.aspx.cs" Inherits="Presentacion.GestionarAdministradores" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2 d-flex align-items-center justify-content-between flex-wrap gap-2">
            <div>
                <h1 tabindex="0" class="text-indigo fw-bold">Administradores</h1>
                <p class="text-muted mb-0">Gestión de los administradores de tu institución.</p>
            </div>
            <asp:Button ID="btnAgregarAdministrador" runat="server" Text="Agregar administrador" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnAgregarAdministrador_Click" />
        </header>

        <!-- MENSAJE DE RESULTADO DE LA ÚLTIMA ACCIÓN -->
        <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-3"></asp:Label>

        <!-- LISTADO -->
        <div class="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white">
                <h5 class="mb-0"><i class="bi bi-people me-2"></i>Administradores de la institución</h5>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <asp:GridView ID="dgvAdministradores" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" EmptyDataText="La institución no tiene administradores.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <Columns>
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Administrador" />
                            <asp:BoundField DataField="Documento" HeaderText="Documento" />
                            <asp:BoundField DataField="Email" HeaderText="Correo electrónico" />
                            <asp:BoundField DataField="Estado" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <%-- No se puede dar de baja a uno mismo (lo rechaza el SP), por eso no se muestra el botón --%>
                                    <asp:LinkButton ID="btnBajaAdministrador" runat="server" Visible='<%# (bool)Eval("Activo") && !(bool)Eval("EsPropio") %>' CommandArgument='<%# Eval("IdAdministrador") %>'
                                        CssClass="btn-outline theme-danger btn-sm rounded-pill px-3 fw-bold"
                                        OnClientClick="return confirm('¿Dar de baja a este administrador?');" OnClick="btnBajaAdministrador_Click">
                                        Dar de baja
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnReactivarAdministrador" runat="server" Visible='<%# !(bool)Eval("Activo") %>' CommandArgument='<%# Eval("IdAdministrador") %>'
                                        CssClass="btn btn-sm btn-outline-lavanda rounded-pill px-3 fw-bold"
                                        OnClientClick="return confirm('¿Reactivar a este administrador?');" OnClick="btnReactivarAdministrador_Click">
                                        Reactivar
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
