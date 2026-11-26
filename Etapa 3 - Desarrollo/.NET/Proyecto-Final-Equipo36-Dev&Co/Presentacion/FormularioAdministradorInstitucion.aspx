<%@ Page Title="Preregistro de administrador" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="FormularioAdministradorInstitucion.aspx.cs" Inherits="Presentacion.FormularioAdministradorInstitucion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .registro-administrador {
            max-width: 1100px;
        }

        .registro-administrador header img {
            max-width: 160px;
        }

        .registro-administrador-columnas {
            display: grid;
            grid-template-columns: minmax(0, 1fr);
            gap: 2rem;
        }

        .registro-administrador-columnas > div {
            min-width: 0;
        }

        @media (min-width: 992px) {
            .registro-administrador-columnas {
                grid-template-columns: repeat(2, minmax(0, 1fr));
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid bg-fondo min-vh-100 py-3">
        <div class="row justify-content-center">
            <div class="col-12 registro-administrador">
                <header class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-25 img-fluid mb-1" />
                    <h2 class="h3 fw-bold text-indigo mb-2">Registrate como Administrador de la Institución</h2>
                    <p class="text-secondary mb-0">Completá tus datos y los de la institución. Dev&amp;Co revisará tu solicitud para habilitar tu cuenta.</p>
                </header>

                <div class="card mb-4">
                    <div class="card-body p-3 p-md-4">
                        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert mb-3" role="status">
                            <asp:Literal ID="litMensaje" runat="server" Mode="Encode" />
                        </asp:Panel>

                        <div class="registro-administrador-columnas">
                            <div>
                                <h4 class="text-indigo mb-3">DATOS PERSONALES</h4>
                                <div class="mb-3">
                                    <label for="<%= txtNombre.ClientID %>" class="form-label text-indigo fw-semibold">Nombre</label>
                                    <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre" CssClass="form-control" MaxLength="50" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtApellido.ClientID %>" class="form-label text-indigo fw-semibold">Apellido</label>
                                    <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido" CssClass="form-control" MaxLength="50" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtDocumento.ClientID %>" class="form-label text-indigo fw-semibold">Documento</label>
                                    <asp:TextBox ID="txtDocumento" runat="server" placeholder="12345678" CssClass="form-control" MaxLength="8" inputmode="numeric" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtDomicilio.ClientID %>" class="form-label text-indigo fw-semibold">Domicilio</label>
                                    <asp:TextBox ID="txtDomicilio" runat="server" placeholder="Av. Rivadavia 1234" CssClass="form-control" MaxLength="100" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtFechaNacimiento.ClientID %>" class="form-label text-indigo fw-semibold">Fecha de nacimiento</label>
                                    <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="form-control" TextMode="Date" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtEmail.ClientID %>" class="form-label text-indigo fw-semibold">Correo electrónico</label>
                                    <asp:TextBox ID="txtEmail" runat="server" placeholder="email@extension.com" CssClass="form-control" TextMode="Email" MaxLength="100" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtTelefono.ClientID %>" class="form-label text-indigo fw-semibold">Teléfono</label>
                                    <asp:TextBox ID="txtTelefono" runat="server" placeholder="+54 11 1234-5678" CssClass="form-control" TextMode="Phone" MaxLength="30" />
                                </div>

                                <div class="row g-2 mb-3">
                                    <div class="col-12 md:col-6">
                                        <label for="<%= txtClave.ClientID %>" class="form-label text-indigo fw-semibold">Contraseña</label>
                                        <asp:TextBox ID="txtClave" runat="server" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="20" CssClass="form-control" autocomplete="new-password" aria-describedby="requisitosClave" />
                                    </div>
                                    <div class="col-12 md:col-6">
                                        <label for="<%= txtClaveRepetida.ClientID %>" class="form-label text-indigo fw-semibold">Confirmar contraseña</label>
                                        <asp:TextBox ID="txtClaveRepetida" runat="server" TextMode="Password" ValidateRequestMode="Disabled" MaxLength="20" CssClass="form-control" autocomplete="new-password" />
                                    </div>
                                    <div id="requisitosClave" class="col-12 form-text mt-1">Usá entre 8 y 20 caracteres e incluí una mayúscula, una minúscula, un número y un símbolo.</div>
                                </div>

                            </div>
                            <div>
                                <h4 class="text-indigo mb-3">DATOS DE LA INSTITUCIÓN</h4>
                                <div class="mb-3">
                                    <label for="<%= txtNombreInstitucion.ClientID %>" class="form-label text-indigo fw-semibold">Nombre de la institución</label>
                                    <asp:TextBox ID="txtNombreInstitucion" runat="server" placeholder="Institución" CssClass="form-control" MaxLength="100" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= txtCuit.ClientID %>" class="form-label text-indigo fw-semibold">CUIT</label>
                                    <asp:TextBox ID="txtCuit" runat="server" placeholder="30710000022" CssClass="form-control" MaxLength="11" inputmode="numeric" />
                                    <div class="form-text">Ingresá los 11 dígitos, sin guiones.</div>
                                </div>

                                <h4 class="text-indigo mb-3">DOCUMENTACIÓN</h4>
                                <p class="form-text">Adjuntá los tres documentos en PDF, JPG o PNG, de hasta 5 MB cada uno.</p>
                                <div class="mb-3">
                                    <label for="<%= fuConstanciaFiscal.ClientID %>" class="form-label text-indigo fw-semibold">Constancia fiscal</label>
                                    <asp:FileUpload ID="fuConstanciaFiscal" runat="server" CssClass="form-control" accept=".pdf,.jpg,.jpeg,.png" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= fuHabilitacion.ClientID %>" class="form-label text-indigo fw-semibold">Habilitación</label>
                                    <asp:FileUpload ID="fuHabilitacion" runat="server" CssClass="form-control" accept=".pdf,.jpg,.jpeg,.png" />
                                </div>
                                <div class="mb-3">
                                    <label for="<%= fuConstanciaLaboral.ClientID %>" class="form-label text-indigo fw-semibold">Constancia laboral</label>
                                    <asp:FileUpload ID="fuConstanciaLaboral" runat="server" CssClass="form-control" accept=".pdf,.jpg,.jpeg,.png" />
                                </div>

                                <asp:Button ID="btnGuardar" runat="server" Text="Preregistro" CssClass="btn btn-lilaOscuro w-100 py-2 mb-2" OnClick="btnGuardar_Click" />
                                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" CssClass="btn btn-lilaClaro w-100 py-2" OnClick="btnCancelar_Click" CausesValidation="false" />
                            </div>
                        </div>
                    </div>
                </div>

                <p class="text-center small text-secondary mb-0">
                    ¿Ya tenés una cuenta?
                    <asp:HyperLink ID="hlIniciarSesion" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro fw-semibold">Iniciá sesión</asp:HyperLink>
                </p>
            </div>
        </div>
    </div>
</asp:Content>
