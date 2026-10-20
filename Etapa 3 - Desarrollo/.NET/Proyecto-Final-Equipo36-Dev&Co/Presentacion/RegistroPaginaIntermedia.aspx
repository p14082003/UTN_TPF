<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="RegistroPaginaIntermedia.aspx.cs" Inherits="Presentacion.RegistroPaginaIntermedia" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
  <div class="container-fluid bg-fondo min-vh-100 py-3">

        <div class="row justify-content-center">
            <div class="col-12 col-md-9 col-lg-6">

                <!-- Titulo -->
                <header class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-50 img-fluid mb-1" />
                    <h2 class="h3 fw-bold mb-2">
                        Creá tu cuenta 
                    </h2>
                </header>

                <!-- Registro para responsable -->
                <div class="card mb-3">
                    <div class="card-body p-3 p-md-4">
                        <div class="d-flex align-items-center gap-3 mb-3">
                            <div class="rounded-3 ">
                                 <img src="Imagenes/PerfilFamiliar.png"alt="Perfil familiar" width="80" class="img-fluid" />
                            </div>
                            <div>
                                 <span class="text-secondary small fw-semibold">
                                     RESPONSABLES
                                 </span>                          
                                <h2 class="h5 text-indigo fw-bold mb-0">
                                    Soy padre, madre o tutor
                                </h2>
                            </div>
                        </div>
                        <p class=" text-secondary mb-3">
                            Encontrá instituciones educativas ideales, compará programas académicos y gestioná las postulaciones en un solo lugar.
                        </p>
                        <ul class="list-unstyled small text-secondary mb-4">
                            <li class="mb-2">
                                <i class="bi bi-check-circle-fill text-lavanda me-2"></i>
                                Búsqueda geolocalizada y filtros por nivel educativo
                            </li>

                            <li class="mb-2">
                                <i class="bi bi-check-circle-fill text-lavanda me-2"></i>
                                Gestión y seguimiento de vacantes en tiempo real
                            </li>

                            <li>
                                <i class="bi bi-check-circle-fill text-lavanda me-2"></i>
                                Comunicación directa y segura con los colegios
                            </li>
                        </ul>
                       <asp:Button ID="btnRegistroResponsable" runat="server" Text="Registrarme como responsable" CssClass="btn btn-lilaOscuro w-100 py-2" PostBackUrl="~/RegistroResponsable.aspx" />
                    </div>
                </div>

                <!-- Registro para instituciones o administradores -->
                <div class="card mb-4">
                    <div class="card-body p-3 p-md-4">
                        <div class="d-flex align-items-center gap-3 mb-3">
                          <div class="rounded-3 p-2">
                                <img src="Imagenes/Institucion.png" alt="Perfil institución" width="75" class="img-fluid" />
                          </div>
                          <div>
                               <span class="text-secondary small fw-semibold">
                                    EDUCADORES
                                </span>
                                <h2 class="h5 text-indigo fw-bold mb-0">
                                    Represento una institución
                                </h2>
                           </div>
                        </div>
                            <p class="text-secondary mb-4">
                                Registrá tu institución, publicá vacantes y gestioná las solicitudes de admisión de nuevos estudiantes de manera centralizada.
                            </p>
                            <asp:Button ID="btnRegistroInstitucion" runat="server" Text="Registrar institución" CssClass="btn btn-lilaClaro w-100 py-2" PostBackUrl="~/RegistroInstitucion.aspx" />
                        </div>
                 </div>

                <!-- Volver al login -->
                <p class="text-center small text-secondary mb-0">
                    ¿Ya tenés una cuenta?
                    <asp:HyperLink ID="hlIniciarSesion" runat="server" NavigateUrl="~/Login.aspx" CssClass="link-lilaClaro fw-semibold">
                         Iniciá sesión
                    </asp:HyperLink>
                </p>
            </div>
        </div>
    </div>
</asp:Content>


