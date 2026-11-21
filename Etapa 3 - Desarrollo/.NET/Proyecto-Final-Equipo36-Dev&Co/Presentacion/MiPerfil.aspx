<%@ Page Title="Mi perfil" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="MiPerfil.aspx.cs" Inherits="Presentacion.MiPerfil" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">
        <div class="row justify-content-center">
            <div class="col-12 md:col-9 lg:col-6">

                <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-3"></asp:Label>

                <!--Encabezado-->
                <div class="card mb-4">
                    <div class="card-body p-4">
                        <h1 class="h3 fw-bold text-indigo mb-1"><asp:Literal ID="litNombreCompleto" runat="server" Mode="Encode"></asp:Literal></h1>
                        <p class="mb-2"><span class="badge rounded-pill bg-fondo text-indigo border border-neutro"><asp:Literal ID="litRol" runat="server" Mode="Encode"></asp:Literal></span></p>
                        <p class="text-muted mb-0"><i class="bi bi-envelope me-2"></i><asp:Literal ID="litEmail" runat="server" Mode="Encode"></asp:Literal></p>
                        <asp:Panel ID="pnlInstitucion" runat="server" CssClass="text-muted mt-1">
                            <i class="bi bi-building me-2"></i><asp:Literal ID="litInstitucion" runat="server" Mode="Encode"></asp:Literal>
                        </asp:Panel>
                    </div>
                </div>

                <!--Datos personales (responsable y administrador)-->
                <asp:Panel ID="pnlDatosPersonales" runat="server" CssClass="card mb-4">
                    <div class="card-body p-4">
                        <h2 class="h5 text-indigo mb-3"><i class="bi bi-person-vcard me-2"></i>Datos personales</h2>

                        <div class="row g-3">
                            <div class="col-12 md:col-6">
                                <div class="small text-muted">Nombre</div>
                                <div class="fw-bold"><asp:Literal ID="litNombre" runat="server" Mode="Encode"></asp:Literal></div>
                            </div>
                            <div class="col-12 md:col-6">
                                <div class="small text-muted">Apellido</div>
                                <div class="fw-bold"><asp:Literal ID="litApellido" runat="server" Mode="Encode"></asp:Literal></div>
                            </div>
                            <div class="col-12 md:col-6">
                                <div class="small text-muted">Documento</div>
                                <div class="fw-bold"><asp:Literal ID="litDocumento" runat="server" Mode="Encode"></asp:Literal></div>
                            </div>
                            <div class="col-12 md:col-6">
                                <div class="small text-muted">Fecha de nacimiento</div>
                                <div class="fw-bold"><asp:Literal ID="litFechaNacimiento" runat="server" Mode="Encode"></asp:Literal></div>
                            </div>
                            <div class="col-12">
                                <div class="small text-muted">Domicilio</div>
                                <div class="fw-bold"><asp:Literal ID="litDomicilio" runat="server" Mode="Encode"></asp:Literal></div>
                            </div>
                        </div>
                        <div class="form-text mt-3">Si hay un error en tus datos personales, comunicate con Dev&amp;Co.</div>
                    </div>
                </asp:Panel>

                <!--Teléfonos (responsable y administrador)-->
                <asp:Panel ID="pnlTelefonos" runat="server" CssClass="card mb-4">
                    <div class="card-body p-4">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h2 class="h5 text-indigo mb-0"><i class="bi bi-telephone me-2"></i>Teléfonos de contacto</h2>
                            <asp:LinkButton ID="btnEditarTelefonos" runat="server" Enabled="false" OnClick="btnEditarTelefonos_Click" CausesValidation="false" CssClass="btn btn-sm rounded-pill border border-neutro text-indigo px-3" ToolTip="Editar teléfonos"><i class="bi bi-pencil"></i></asp:LinkButton>
                        </div>

                        <asp:Repeater ID="rptTelefonos" runat="server" OnItemCommand="rptTelefonos_ItemCommand">
                            <HeaderTemplate><ul class="list-unstyled d-flex flex-wrap gap-2 mb-3"></HeaderTemplate>
                            <ItemTemplate>
                                <li class="badge rounded-pill bg-fondo text-indigo border border-neutro fs-6 fw-semibold d-inline-flex align-items-center gap-2">
                                    <span><%# Server.HtmlEncode((string)Eval("Numero")) %></span>
                                    <asp:LinkButton ID="btnQuitarTelefono" runat="server" Visible="false" CommandName="Quitar" CommandArgument='<%# Eval("Id") %>'
                                        CausesValidation="false" CssClass="btn btn-sm border-0 p-0 text-indigo" ToolTip="Quitar este teléfono"><i class="bi bi-x-lg"></i></asp:LinkButton>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate></ul></FooterTemplate>
                        </asp:Repeater>

                        <asp:Panel ID="pnlTelefonosEditar" runat="server" Visible="false">
                            <div class="form-text mb-2">Podés tener hasta 2 teléfonos y como mínimo 1.</div>
                            <asp:Panel ID="pnlAgregarTelefono" runat="server">
                                <asp:Label ID="lblTelefono" runat="server" Text="Agregar teléfono" CssClass="form-label text-indigo d-block"></asp:Label>
                                <asp:TextBox ID="txtTelefono" runat="server" TextMode="Phone" placeholder="2617777777" MaxLength="30" CssClass="form-control"></asp:TextBox>
                            </asp:Panel>
                            <div class="d-flex flex-wrap gap-2 mt-3">
                                <asp:Button ID="btnAgregarTelefono" runat="server" Text="Agregar" OnClick="btnAgregarTelefono_Click" CssClass="btn btn-lilaOscuro px-4 py-2" />
                                <asp:Button ID="btnListoTelefonos" runat="server" Text="Listo" OnClick="btnListoTelefonos_Click" CausesValidation="false" CssClass="btn btn-lilaClaro px-4 py-2" />
                            </div>
                        </asp:Panel>
                    </div>
                </asp:Panel>

                <!--Contraseña (todos los roles)-->
                <div class="card mb-4">
                    <div class="card-body p-4">
                        <div class="d-flex justify-content-between align-items-center mb-3">
                            <h2 class="h5 text-indigo mb-0"><i class="bi bi-shield-lock me-2"></i>Contraseña</h2>
                            <asp:LinkButton ID="btnEditarClave" runat="server" Enabled="false" OnClick="btnEditarClave_Click" CausesValidation="false" CssClass="btn btn-sm rounded-pill border border-neutro text-indigo px-3" ToolTip="Cambiar contraseña"><i class="bi bi-pencil"></i></asp:LinkButton>
                        </div>

                        <asp:Panel ID="pnlClaveVer" runat="server" CssClass="fw-bold">
                            ••••••••
                        </asp:Panel>

                        <asp:Panel ID="pnlClaveEditar" runat="server" Visible="false">
                            <div class="row g-3">
                                <div class="col-12">
                                    <asp:Label ID="lblClaveActual" runat="server" Text="Contraseña actual" CssClass="form-label text-indigo d-block"></asp:Label>
                                    <asp:TextBox ID="txtClaveActual" runat="server" TextMode="Password" EnableViewState="false" ValidateRequestMode="Disabled" autocomplete="current-password" placeholder="********" MaxLength="255" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-12 md:col-6">
                                    <asp:Label ID="lblClaveNueva" runat="server" Text="Contraseña nueva" CssClass="form-label text-indigo d-block"></asp:Label>
                                    <asp:TextBox ID="txtClaveNueva" runat="server" TextMode="Password" EnableViewState="false" ValidateRequestMode="Disabled" autocomplete="new-password" placeholder="********" MaxLength="20" CssClass="form-control"></asp:TextBox>
                                </div>
                                <div class="col-12 md:col-6">
                                    <asp:Label ID="lblClaveRepetida" runat="server" Text="Repetir contraseña nueva" CssClass="form-label text-indigo d-block"></asp:Label>
                                    <asp:TextBox ID="txtClaveRepetida" runat="server" TextMode="Password" EnableViewState="false" ValidateRequestMode="Disabled" autocomplete="new-password" placeholder="********" MaxLength="20" CssClass="form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-text mt-2">Usá entre 8 y 20 caracteres e incluí una mayúscula, una minúscula, un número y un símbolo.</div>
                            <div class="d-flex flex-wrap gap-2 mt-3">
                                <asp:Button ID="btnGuardarClave" runat="server" Text="Guardar" OnClick="btnGuardarClave_Click" CssClass="btn btn-lilaOscuro px-4 py-2" />
                                <asp:Button ID="btnCancelarClave" runat="server" Text="Cancelar" OnClick="btnCancelarClave_Click" CausesValidation="false" CssClass="btn btn-lilaClaro px-4 py-2" />
                            </div>
                        </asp:Panel>
                    </div>
                </div>

            </div>
        </div>
    </div>

</asp:Content>
