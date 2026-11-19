<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioAdministradorAdicional.aspx.cs" Inherits="Presentacion.FormularioAdministradorAdicional" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">
        <div class="row justify-content-center">
            <div class="col-12 md:col-9 lg:col-6">

                <!--Titulo-->
                <div class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-25 img-fluid mb-1" />
                    <h2 class="h3 fw-bold text-indigo mb-2">Agregar nuevo administrador</h2>
                </div>

                <div class="card mb-4">
                    <div class="card-body p-3 p-md-4">

                        <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-3"></asp:Label>

                        <!--Datos personales-->
                        <div class="row g-2 mb-3">
                            <div class="col-12">
                                <asp:Label ID="lblDatosPersonales" runat="server" Text="Datos personales:" CssClass="form-label fw-bold"></asp:Label>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre" MaxLength="50" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblApellido" runat="server" Text="Apellido" AssociatedControlID="txtApellido" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido" MaxLength="50" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblDocumento" runat="server" Text="Documento" AssociatedControlID="txtDocumento" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtDocumento" runat="server" placeholder="35887625" MaxLength="8" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblFechaNacimiento" runat="server" Text="Fecha de nacimiento" AssociatedControlID="txtFechaNacimiento" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <!--Datos de contacto-->
                        <div class="row g-2 mb-3">
                            <div class="col-12">
                                <asp:Label ID="lblDatosContacto" runat="server" Text="Datos de contacto:" CssClass="form-label fw-bold"></asp:Label>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblTelefono" runat="server" Text="Teléfono" AssociatedControlID="txtTelefono" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtTelefono" runat="server" placeholder="2613333333" MaxLength="30" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblEmail" runat="server" Text="Correo electrónico" AssociatedControlID="txtEmail" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="tunombre@ejemplo.com" MaxLength="100" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12">
                                <asp:Label ID="lblDomicilio" runat="server" Text="Domicilio" AssociatedControlID="txtDomicilio" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtDomicilio" runat="server" placeholder="Av. Rivadavia 123, Mza" MaxLength="100" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <!--Seguridad-->
                        <div class="row g-2 mb-3">
                            <div class="col-12">
                                <asp:Label ID="lblSeguridad" runat="server" Text="Seguridad:" CssClass="form-label fw-bold"></asp:Label>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblClave" runat="server" Text="Contraseña" AssociatedControlID="txtClave" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtClave" runat="server" TextMode="Password" ValidateRequestMode="Disabled" placeholder="********" MaxLength="20" CssClass="form-control" aria-describedby="requisitosClave"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblClaveRepetida" runat="server" Text="Confirmar contraseña" AssociatedControlID="txtClaveRepetida" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtClaveRepetida" runat="server" TextMode="Password" ValidateRequestMode="Disabled" placeholder="********" MaxLength="20" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div id="requisitosClave" class="col-12 form-text mt-1">Usá entre 8 y 20 caracteres e incluí una mayúscula, una minúscula, un número y un símbolo.</div>
                        </div>

                        <!--Botones-->
                        <div class="row g-2">
                            <div class="col-12">
                                <asp:Button ID="btnAgregar" runat="server" Text="Agregar administrador" OnClick="btnAgregar_Click" CssClass="btn btn-lilaOscuro w-100 py-2 mb-2" />
                                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" CssClass="btn btn-lilaClaro w-100 py-2 mb-2" />
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
