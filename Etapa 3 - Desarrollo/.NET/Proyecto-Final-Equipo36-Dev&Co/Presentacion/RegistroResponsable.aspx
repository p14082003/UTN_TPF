<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="RegistroResponsable.aspx.cs" Inherits="Presentacion.RegistroResponsable" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">
        <!--Titulo-->
        <div class ="row justify-content-center">
            <div class="col-12 col-md-9 col-lg-6">
                <div class ="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-25 img-fluid mb-1" />
                    <h2 class ="h3 fw-bold text-indigo mb-2">Creá tu cuenta de Responsable</h2>
                    <p>Completá tus datos para comenzar a gestionar postulaciones educativas</p>
                </div>
           
            <div class ="card mb-4">
                <div class="card-body p-3 p-md-4">
                    <!--Datos personales-->
                    <h4 class="text-indigo mb-3">DATOS PERSONALES</h4>
                     <asp:Label ID="lblParentesco" runat="server" Text="Rol/Parentesco:" CssClass="form-label fw-bold"></asp:Label>
                    <div class="row g-2 mb-3">
                        <!--Rol/parentesco-->
                       
                            <div class="col-12 md:col-4">
                                <div class =" border border-neutro rounded-3 p-2">
                                    <div class=" form-check mb-0">
                                        <asp:RadioButton ID="rbPadre" runat="server" GroupName="Parentesco" CssClass="form-check-input"/>
                                        <asp:Label ID="lblPadre" runat="server" AssociatedControlID="rbPadre" Text="Padre" CssClass="form-check-label ms-2"></asp:Label>
                                    </div>
                                </div>
                            </div>

                            <div class="col-12 md:col-4">
                                <div class="border border-neutro rounded-3 p-2">
                                    <div class="form-check mb-0"> 
                                        <asp:RadioButton ID="rbMadre" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                        <asp:Label ID="lblMadre" runat="server" AssociatedControlID="rbMadre" Text="Madre" CssClass="form-check-label ms-2"></asp:Label>
                                    </div>
                                </div>
                            </div>

                            <div class= "col-12 md:col-4">
                                <div class ="border border-neutro rounded-3 p-2">
                                    <div class="form-check mb-0">
                                        <asp:RadioButton ID="rbTutor" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                        <asp:Label ID="lbTutor" runat="server" Text="Tutor Legal" AssociatedControlID="rbTutor" CssClass="form-check-label ms-2"></asp:Label>
                                    </div>
                                </div>
                            </div>
                        
                    </div>
                     <!--Datos Personales-->
                    <div class="row g-2 mb-3">
                        <div class="col-12">
                            <asp:Label ID="lblDatosPersonales" runat="server" Text="Datos Personales:" CssClass="form-label fw-bold"></asp:Label>
                        </div>
                            
                        <div class="col-12 md:col-6">
                            <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtNombre" runat="server" placeholder="Nombre" MaxLength="50" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="col-12 md:col-6">
                            <asp:Label ID="lblApellido" runat="server" Text="Apellido" AssociatedControlID="txtApellido" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtApellido" runat="server" placeholder="Apellido" MaxLength="50" CssClass="form-control"></asp:TextBox>
                        </div>
                         <div class="col-12 md:col-6">
                            <asp:Label ID="lblDocumento" runat="server" Text="Documento" AssociatedControlID="txtDocumento" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtDocumento" runat="server" placeholder="35887625" MaxLength="8" CssClass="form-control"></asp:TextBox>
                        </div>
                         <div class="col-12 md:col-6">
                            <asp:Label ID="lblFechaNacimiento" runat="server" Text="Fecha de Nacimiento" AssociatedControlID="txtFechaNacimiento" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtFechaNacimiento" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                        </div>
                   </div>
                   <!--Datos de contacto-->
                   <div class="row g-2 mb-3">
                        <div class="col-12">
                            <asp:Label ID="lblDatosContacto" runat="server" Text="Datos de Contacto:" CssClass="form-label fw-bold"></asp:Label>
                        </div>
                        <div class="col-12 md:col-6">
                            <asp:Label ID="lblTelefono" runat="server" Text="Telefono" AssociatedControlID="txtTelefono" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtTelefono" runat="server" placeholder="2613333333" MaxLength="30" CssClass="form-control"></asp:TextBox>
                         </div>
                         <div class="col-12 md:col-6">
                            <asp:Label ID="lblEmail" runat="server" Text="Correo Electrónico" AssociatedControlID="txtEmail" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtEmail" runat="server" TextMode="Email" placeholder="tunombre@ejemplo.com" MaxLength="50" CssClass="form-control"></asp:TextBox>
                         </div>
                         <div class="col-12">
                             <asp:Label ID="lblDomicilio" runat="server" Text="Domicilio" AssociatedControlID="txtDomicilio" CssClass="form-label text-indigo"></asp:Label>
                             <asp:TextBox ID ="txtDomicilio" runat="server" placeholder="Av.Rivadavia 123,Mza" MaxLength="100" CssClass="form-control"></asp:TextBox>
                         </div>
                    </div>
                    <!--Datos de seguridad-->
                    <div class ="row g-2 mb-3">
                        <div class="col-12">
                            <asp:Label ID ="Seguridad" runat="server" Text="Seguridad:" CssClass ="form-label fw-bold"></asp:Label>
                        </div>
                        <div class="col-12 md:col-6">
                            <asp:Label ID="lblClave" runat="server" Text="Contraseña" AssociatedControlID="txtClave" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtClave" runat="server" TextMode="Password" placeholder="********" MaxLength="255" CssClass="form-control"></asp:TextBox>
                        </div>
                        <div class="col-12 md:col-6">
                            <asp:Label ID="lblClaveRepetida" runat="server" Text="Confirmar contraseña" AssociatedControlID="txtClaveRepetida" CssClass="form-label text-indigo"></asp:Label>
                            <asp:TextBox ID ="txtClaveRepetida" runat="server" TextMode="Password" placeholder="********" MaxLength="255" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>
                    <!--Botones-->
                    <div class ="row g-2 mb-3">
                        <div class="col-12">
                            <asp:Button Id="btnRegistrar" runat="server" Text="Registrar Responsable" OnClick="btnRegistrar_Click" CssClass="btn btn-lilaOscuro w-100 py-2 mb-2"/>
                            <asp:Button Id="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CssClass="btn btn-lilaClaro w-100 py-2 mb-2"/>
                        </div>
                    </div>

                    <!-- Volver al login -->
                     <p class="text-center small text-secondary mb-0">
                        ¿Ya tenés una cuenta?
                        <asp:HyperLink ID="hlIniciarSesion" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro fw-semibold">Iniciá sesión
                        </asp:HyperLink>
                    </p>
                    
                    </div>
                </div>
            </div>
        </div>
    </div>
  
</asp:Content>
