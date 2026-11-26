<%@ Page Title="Consultar postulación" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="ConsultarPostulacionAdministrador.aspx.cs" Inherits="Presentacion.ConsultarPostulacionAdministrador" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">

        <div class="row justify-content-center">
            <div class="col-12 md:col-9 lg:col-6">

                <!-- Titulo -->
                <header class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-25 img-fluid mb-1" />
                    <h2 class="h3 fw-bold text-indigo mb-2">Consultá el estado de tu postulación</h2>
                    <p class="text-secondary mb-0">
                        Ingresá el código de seguimiento que recibiste al postularte como administrador y el correo con el que te registraste.
                    </p>
                </header>

                <!-- Mensaje de error -->
                <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="col-12 alert mb-3" role="alert">
                    <asp:Literal ID="litMensaje" runat="server" Mode="Encode"  />
                </asp:Panel>

                <!-- FORMULARIO DE CONSULTA -->
                <asp:Panel ID="pnlConsulta" runat="server" CssClass="card mb-4">
                    <div class="card-body p-3 p-md-4">

                        <div class="mb-3">
                            <label for="<%= txtCodigo.ClientID %>" class="form-label text-indigo fw-semibold">Código de seguimiento</label>
                            <asp:TextBox ID="txtCodigo" runat="server" placeholder="Código de seguimiento" CssClass="form-control" MaxLength="30" />
                        </div>

                        <div class="mb-4">
                            <label for="<%= txtEmail.ClientID %>" class="form-label text-indigo fw-semibold">Correo electrónico</label>
                            <asp:TextBox ID="txtEmail" runat="server" placeholder="email@extension.com" CssClass="form-control" TextMode="Email" MaxLength="100" />
                            <div class="form-text">Es el correo del administrador que realizó la postulación.</div>
                        </div>

                        <asp:Button ID="btnConsultar" runat="server" Text="Consultar" CssClass="btn btn-lilaOscuro w-100 py-2" OnClick="btnConsultar_Click" />
                    </div>
                </asp:Panel>

                <!-- RESULTADO -->
                <asp:Panel ID="pnlResultado" runat="server" Visible="false" CssClass="card mb-4">
                    <div class="card-body p-3 p-md-4">

                        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
                            <h3 class="h5 text-indigo fw-bold mb-0">Estado de tu postulación</h3>
                            <asp:Label ID="lblEstado" runat="server" CssClass="badge rounded-pill bg-lavanda px-3 py-2" />
                        </div>

                        <div class="p-4 bg-fondo rounded-4 border border-neutro mb-3">
                            <div class="row">
                                <div class="col-md-6 mb-3">
                                    <span class="text-muted d-block small">Institución</span>
                                    <asp:Label ID="lblInstitucion" runat="server" CssClass="text-indigo fw-bold fs-5" />
                                </div>
                                <div class="col-md-6 mb-3">
                                    <span class="text-muted d-block small">CUIT</span>
                                    <asp:Label ID="lblCuit" runat="server" CssClass="text-indigo fw-bold fs-5" />
                                </div>
                                <div class="col-md-6">
                                    <span class="text-muted d-block small">Fecha de postulación</span>
                                    <asp:Label ID="lblFecha" runat="server" CssClass="text-indigo fw-bold fs-5" />
                                </div>
                            </div>
                        </div>

                        <!-- Solo se muestra si la postulación fue rechazada -->
                        <asp:Panel ID="pnlMotivoRechazo" runat="server" Visible="false" CssClass="p-3 bg-fondo rounded-4 border border-danger mb-3">
                            <span class="text-danger fw-bold d-block mb-1">
                                <i class="bi bi-exclamation-triangle-fill me-1"></i>Motivo del rechazo
                            </span>
                            <asp:Label ID="lblMotivoRechazo" runat="server" CssClass="text-indigo d-block" />
                        </asp:Panel>

                        <asp:Button ID="btnNuevaConsulta" runat="server" Text="Hacer otra consulta" CssClass="btn btn-lilaClaro w-100 py-2" OnClick="btnNuevaConsulta_Click" />
                    </div>
                </asp:Panel>

                <!-- Volver al login -->
                <p class="text-center small text-secondary mb-0">
                    <asp:HyperLink ID="hlVolver" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro fw-semibold">
                        Volver al inicio de sesión
                    </asp:HyperLink>
                </p>
            </div>
        </div>
    </div>
</asp:Content>
