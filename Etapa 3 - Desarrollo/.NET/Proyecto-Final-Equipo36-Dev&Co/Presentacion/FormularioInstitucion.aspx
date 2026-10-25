<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioInstitucion.aspx.cs" Inherits="Presentacion.FormularioInstitucion" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid bg-fondo min-vh-100 py-5">

        <!-- TITULO -->
        <div class="container mb-4">
            <div class="text-center mx-auto encabezado-formulario">
                <h1 class="display-6 fw-bold text-dark">
                    Registrá tu institución educativa
                </h1>

                <p class="text-secondary">
                    Creá el perfil oficial de tu colegio o instituto para que miles
                    de familias encuentren tu propuesta pedagógica y gestionen
                    admisiones de forma ágil y segura.
                </p>
            </div>
        </div>

        <div class="container">
            <div class="row g-4 align-items-start">

                <!-- PANEL LATERAL -->
                <div class="col-12 text-center">

                    <h5 class="card-title text-indigo mb-3">¿Qué podés hacer con esta cuenta?</h5>

                    <div class="card mb-3">
                        <div class="card-body p-4">

                            <div class="row g-4">

                                <div class="col-12 col-md-4">
                                    <h6 class="fw-semibold">Mayor visibilidad</h6>
                                    <p class="text-secondary small mb-0">Aparecé en el mapa cuando las familias buscan vacantes por zona y nivel educativo.</p>
                                </div>

                                <div class="col-12 col-md-4">
                                    <h6 class="fw-semibold">Seguimiento en tiempo real</h6>
                                    <p class="text-secondary small mb-0">Visualizá postulaciones y gestioná las admisiones directamente desde la aplicación.</p>
                                </div>

                                <div class="col-12 col-md-4">
                                    <h6 class="fw-semibold">Gestión centralizada</h6>
                                    <p class="text-secondary small mb-0">Organizá la información de tu institución y facilitá el contacto con las familias.</p>
                                </div>

                            </div>

                        </div>
                    </div>

                </div>

                <!-- FORMULARIO PRINCIPAL -->
                <div class="col-12">

                    <div class="card bg-white">
                        <div class="card-body p-4">

                            <!-- BLOQUES A Y B -->
                            <div class="row g-4">

                                <!-- BLOQUE A -->
                                <div class="col-12 col-lg">

                                        <h5 class="card-title text-indigo mb-1">Bloque A: Datos legales e identificación</h5>
                                        <p class="text-secondary small mb-4"> Información principal para identificar la institución.</p>

                                        <!-- NOMBRE -->
                                        <div class="mb-3">
                                            <asp:Label ID="lblNombre" runat="server" Text="Nombre de la institución" CssClass="form-label fw-semibold"></asp:Label>
                                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control" placeholder="Ej. Colegio San Martín"></asp:TextBox>
                                        </div>

                                        <!-- CUIT -->
                                        <div class="mb-3">
                                            <asp:Label ID="lblCuit" runat="server" Text="CUIT institucional" CssClass="form-label fw-semibold"> </asp:Label>
                                            <asp:TextBox ID="txtCuit" runat="server" CssClass="form-control" placeholder="XX-XXXXXXXX-X"></asp:TextBox>
                                        </div>

                                        <!-- UBICACION -->
                                        <div class="mb-3">
                                            <asp:Label ID="lblDireccion" runat="server" Text="Dirección de la sede" CssClass="form-label fw-semibold"></asp:Label>
                                            <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" placeholder="Calle, número y localidad"></asp:TextBox>
                                        </div>

                                        <!-- TELEFONOS -->
                                        <div class="row g-3 mb-3">
                                            <div class="col-12 col-md-6">
                                                <asp:Label ID="lblTelefonoPrincipal" runat="server" Text="Teléfono principal" CssClass="form-label fw-semibold"></asp:Label>
                                                <asp:TextBox ID="txtTelefonoPrincipal" runat="server" CssClass="form-control" TextMode="Phone" placeholder="+54 11 0000-0000"></asp:TextBox>
                                            </div>

                                            <div class="col-12 col-md-6">
                                                <asp:Label ID="lblTelefonoAlternativo" runat="server" Text="Télefono alternativo" CssClass="form-label fw-semibold"></asp:Label>
                                                <asp:TextBox ID="txtTelefonoAlternativo" runat="server" CssClass="form-control" TextMode="Phone" placeholder="+54 9 11 0000-0000"></asp:TextBox>
                                            </div>
                                        </div>

                                        <!-- EMAIL -->
                                        <div class="mb-3">
                                            <asp:Label ID="lblEmail" runat="server" Text="Correo electrónico" CssClass="form-label fw-semibold"></asp:Label>
                                            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="tuColegio@mail.com.ar"></asp:TextBox>
                                        </div>
                                   
                                </div>

                                <!-- SEPARADOR ENTRE BLOQUES -->
                                <div class="col-auto d-none d-lg-block">
                                    <div class="vr h-100"></div>
                                </div>
                                <div class="col-12 d-lg-none">
                                    <hr class="border-neutro my-0" />
                                </div>

                                <!-- BLOQUE B -->
                                <div class="col-12 col-lg">
                                    <h5 class="card-title text-indigo mb-1">Bloque B: Propuesta académica y turnos</h5>
                                    <p class="text-secondary small mb-4">Definí los turnos y las características de tu propuesta educativa.</p>

                                    <!-- TIPO INSTITUCION -->
                                    <asp:Label ID="lblTipoInstitucion" runat="server" Text="Tipo de institucion" CssClass="form-label fw-semibold"></asp:Label>
                                    <div class="mb-4">
                                        <asp:CheckBoxList ID="cblTipoInstitucion" runat="server" CssClass="opciones-lista">
                                            <asp:ListItem Value="1">Jardín</asp:ListItem>
                                            <asp:ListItem Value="2">Primaria</asp:ListItem>
                                            <asp:ListItem Value="3">Secundaria</asp:ListItem>
                                            <asp:ListItem Value="4">Intituto de idiomas</asp:ListItem>
                                        </asp:CheckBoxList>
                                    </div>

                                    <!-- TURNOS -->
                                    <asp:Label ID="lblTurnos" runat="server" Text="Turnos" CssClass="form-label fw-semibold d-block mb-2"></asp:Label>
                                    <div class="mb-4">                                  
                                        <asp:RadioButtonList ID="rblTurnos" runat="server" CssClass="opciones-lista">
                                            <asp:ListItem Value="1">Mañana</asp:ListItem>
                                            <asp:ListItem Value="2">Tarde</asp:ListItem>
                                            <asp:ListItem Value="3">Noche</asp:ListItem>
                                            <asp:ListItem Value="4">Jornada completa</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </div>

                                    <!-- ALUMNOS POR CURSO -->
                                    <asp:Label ID="lblAlumnosoPorCurso" runat="server" Text="Promedio de alumnos por curso" CssClass="form-label fw-semibold"></asp:Label>
                                    <div class="mb-3">
                                        <asp:DropDownList ID="ddlAlumnosPorCurso" runat="server" CssClass="form-select">
                                            <asp:ListItem Text="Seleccionar cantidad" Value="" />
                                            <asp:ListItem Text="1 a 10 alumnos" Value="1" />
                                            <asp:ListItem Text="11 a 20 alumnos" Value="2" />
                                            <asp:ListItem Text="21 a 34 alumnos" Value="3" />
                                            <asp:ListItem Text="35 o más alumnos" Value="4" />
                                        </asp:DropDownList>
                                    </div>

                                    <!-- IDIOMAS -->
                                    <asp:Label ID="lblIdiomas" runat="server" Text="Idiomas" CssClass="form-label fw-semibold"></asp:Label>

                                    <!-- IDIOMAS DE EJEMPLO -->
                                    <div class="d-flex flex-wrap gap-2 mb-3">
                                        <div class="d-inline-flex align-items-center gap-2 rounded-3 px-3 py-2 border border-neutro bg-fondo">Español
                                            <button type="button" class="btn p-0 border-0 text-indigo fw-bold">×</button>
                                        </div>

                                        <div class="d-inline-flex align-items-center gap-2 rounded-3 px-3 py-2 border border-neutro bg-fondo">Inglés
                                            <button type="button" class="btn p-0 border-0 text-indigo fw-bold">×</button>
                                        </div>
                                    </div>

                                    <!-- AGREGAR IDIOMA -->
                                    <div class="row g-2">
                                        <div class="col">
                                            <asp:TextBox ID="txtOtroIdioma" runat="server" CssClass="form-control" placeholder="Ej. Italiano"></asp:TextBox>
                                        </div>
                                        <div class="col-auto">
                                            <asp:Button ID="btnAgregarIdioma" runat="server" Text="+ Agregar idioma" CssClass="btn btn-lilaClaro"/>
                                        </div>
                                    </div>
                                </div>

                            </div>

                            <!-- REGISTRO -->
                            <div class="d-grid mt-3">
                                <asp:Button ID="btnRegistrarInstitucion" runat="server" Text="Registrar institución →" CssClass="btn btn-lilaOscuro mt-4"/>
                            </div>
                            <p class="text-center text-secondary small mt-3 mb-0">
                                ¿Ya registraste tu colegio?
                                <a href="#" class="link-lilaClaro">Iniciá sesión como institución</a>
                            </p>
                        </div>
                    </div>
                </div>
            </div>

        </div>

    </div>

</asp:Content>