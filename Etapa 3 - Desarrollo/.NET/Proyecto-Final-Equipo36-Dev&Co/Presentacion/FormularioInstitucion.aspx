<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioInstitucion.aspx.cs" Inherits="Presentacion.FormularioInstitucion" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container-fluid bg-fondo">

        <!-- TITULO -->
        <div class="row m-5">
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

        <!-- PANEL SUPERIOR -->
        <h2 class="text-indigo text-center mb-4">
            ¿Qué podés hacer con esta cuenta?
        </h2>

        <div class="row m-1">

            <!-- BENEFICIOS -->
            <div class="col-4">
                <div class="card">
                    <div class="card-body">
                        <h3 class="text-indigo">
                            Mayor visibilidad
                        </h3>
                        <p>
                            Aparecé en el mapa cuando las familias buscan vacantes por zona y nivel educativo.
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="card">
                    <div class="card-body">
                        <h3 class="text-indigo">
                            Seguimiento en tiempo real
                        </h3>
                        <p>
                            Visualizá postulaciones y gestioná las admisiones directamente desde la aplicación.
                        </p>
                    </div>
                </div>
            </div>
            <div class="col-4">
                <div class="card">
                    <div class="card-body">
                        <h3 class="text-indigo">
                            Gestión centralizada
                        </h3>
                        <p>
                            Organizá la información de tu institución y facilitá el contacto con las familias.
                        </p>
                    </div>
                </div>
            </div>

        </div>

        <!-- FORMULARIO PRINCIPAL -->
        <div class="card m-5">

            <div class="card-body">

                <h3 class="text-indigo">
                    Bloque A: Datos legales e identificación
                </h3>

                <!-- NOMBRE Y CUIT -->
                <div class="row">

                    <div class="col-8">
                        <!-- NOMBRE -->
                        <div class="mb-3">
                            <asp:Label ID="lblNombre" runat="server" Text="Nombre de la institución" CssClass="form-label fw-semibold"></asp:Label>
                            <asp:TextBox ID="txtNombre" runat="server" CssClass="form-control border-neutro mt-3" placeholder="Ej. Colegio de la Nación"></asp:TextBox>
                        </div>
                    </div>

                    <div class="col-4">
                        <!-- CUIT -->
                        <div class="mb-3">
                            <asp:Label ID="lblCuit" runat="server" Text="CUIT de la institución" CssClass="form-label fw-semibold"> </asp:Label>
                            <asp:TextBox ID="txtCuit" runat="server" CssClass="form-control border-neutro mt-3" placeholder="XX-XXXXXXXX-X"></asp:TextBox>
                        </div>
                    </div>

                </div>

                <!-- EMAIL -->
                <div class="mb-3">
                    <asp:Label ID="lblEmail" runat="server" Text="Correo electrónico" CssClass="form-label fw-semibold"></asp:Label>
                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control border-neutro mt-3" TextMode="Email" placeholder="tuColegio@mail.com.ar"></asp:TextBox>
                </div>

                <!-- TELEFONOS -->
                <div class="row">
                    <div class="col-6">
                        <asp:Label ID="lblTelefono" runat="server" Text="Teléfono principal" CssClass="form-label fw-semibold"></asp:Label>
                        <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control border-neutro mt-3" TextMode="Phone" placeholder="+54 11 0000-0000"></asp:TextBox>
                    </div>

                    <div class="col-6">
                        <asp:Label ID="lblTelefonoAlternativo" runat="server" Text="Télefono alternativo" CssClass="form-label fw-semibold"></asp:Label>
                        <asp:TextBox ID="txtTelefonoAlternativo" runat="server" CssClass="form-control border-neutro mt-3" TextMode="Phone" placeholder="+54 9 11 0000-0000"></asp:TextBox>
                    </div>

                </div>

                <!-- UBICACION -->
                <div class="mb-3">
                    <asp:Label ID="lblUbicacion" runat="server" Text="Dirección de la institución" CssClass="form-label fw-semibold"></asp:Label>
                    <asp:TextBox ID="txtUbicacion" runat="server" CssClass="form-control border-neutro mt-3" placeholder="Calle, número y localidad"></asp:TextBox>
                </div>

            </div>

        </div>

        <!-- BLOQUE B -->
        <div class="card m-5">

            <div class="card-body">

                <h3 class="text-indigo">
                    Bloque B: Información complementaria
                </h3>
                
                <!-- IDIOMAS -->
                <div class="mb-3">

                    <asp:Label ID="lblIdiomas" runat="server" Text="Idiomas" CssClass="form-label fw-semibold"></asp:Label>

                    <!-- IDIOMAS DE EJEMPLO (REPEATER DE ASP) -->
                    <div class="d-flex gap-4 mt-3">
                        <div class="btn btn-lilaClaro">
                            Español
                            <button type="button" class="text-indigo border-0 bg-transparent"> ×</button>
                        </div>

                        <div class="btn btn-lilaClaro">
                            Ingles
                            <button type="button" class="text-indigo border-0 bg-transparent">×</button>
                        </div>

                        <asp:Button ID="btnAgregarIdioma" runat="server" Text="+ Agregar idioma" CssClass="btn-lilaOscuro"/>
                    </div>

                </div>

                <!-- DISCAPACIDADES -->
                <div class="mb-3">

                    <asp:Label ID="lblDiscapacidades" runat="server" Text="Inclusión y accesibilidad" CssClass="form-label fw-semibold"></asp:Label>

                    <!-- DISCAPACIDADES DE EJEMPLO (REPEATER DE ASP) -->
                    <div class="d-flex gap-4 mt-3">
                        <div class="btn btn-lilaClaro">
                            Visual
                            <button type="button" class="text-indigo border-0 bg-transparent">×</button>
                        </div>

                        <div class="btn btn-lilaClaro">
                            Movilidad reducida
                            <button type="button" class="text-indigo border-0 bg-transparent">×</button>
                        </div>

                        <asp:Button ID="btnAgregarAccesibilidad" runat="server" Text="+ Agregar accesibilidad" CssClass="btn-lilaOscuro" />
                    </div>

                </div>

                <!-- IMAGENES -->
                <div class="mb-3">

                    <asp:Label ID="lblImagenes" runat="server" Text="Imagenes de las instalaciones" CssClass="form-label fw-semibold mb-5"></asp:Label>

                    <asp:FileUpload ID="FileUpload1" runat="server" CssClass="form-control mb-5 mt-3"/>

                    <!-- CARROUSEL DE IMAGENES (REPEATER DE ASP) -->
                    <div class="row">
                        <div id="carouselMultiple" class="carousel slide" data-bs-ends="stop" style="--bs-carousel-items: 3; --bs-carousel-items-gap: 1rem;">
                            <div class="d-flex justify-content-between align-items-center">
                                <div>
                                    <button class="btn-icon btn-sm" type="button" data-bs-target="#carouselMultiple" data-bs-slide="prev" style="--bs-carousel-control-icon-width: 1rem;">
                                        <span class="carousel-icon-prev" aria-hidden="true"></span>
                                        <span class="visually-hidden">Previous</span>
                                    </button>
                                    <button class="btn-icon btn-sm" type="button" data-bs-target="#carouselMultiple" data-bs-slide="next" style="--bs-carousel-control-icon-width: 1rem;">
                                        <span class="carousel-icon-next" aria-hidden="true"></span>
                                        <span class="visually-hidden">Next</span>
                                    </button>
                                </div>
                            </div>
                            <div class="carousel-inner">
                                <div class="carousel-item active">
                                    <div class="card">
                                        <asp:Image ID="Image1" runat="server" ImageUrl="https://placehold.co/400x250" CssClass="card-img-top" />
                                        <div class="card-body">
                                            <div class="d-flex justify-content-between">
                                                <p>Ejemplo 1</p>
                                                <asp:Button ID="Button1" runat="server" Text="x" CssClass="btn-solid theme-danger" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="carousel-item">
                                    <div class="card">
                                        <asp:Image ID="Image2" runat="server" ImageUrl="https://placehold.co/400x250" CssClass="card-img-top" />
                                        <div class="card-body">
                                            <div class="d-flex justify-content-between">
                                                <p>Ejemplo 2</p>
                                                <asp:Button ID="Button2" runat="server" Text="x" CssClass="btn-solid theme-danger" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="carousel-item">
                                    <div class="card">
                                        <asp:Image ID="Image3" runat="server" ImageUrl="https://placehold.co/400x250" CssClass="card-img-top" />
                                        <div class="card-body">
                                            <div class="d-flex justify-content-between">
                                                <p>Ejemplo 3</p>
                                                <asp:Button ID="Button3" runat="server" Text="x" CssClass="btn-solid theme-danger" />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="d-grid m-5">
            <asp:Button ID="btnRegistrarInstitucion" runat="server" Text="Registrar institución" CssClass="btn btn-lilaOscuro mt-4" />
        </div>

        <p class="text-center text-secondary small m-5">
            ¿Ya registraste tu colegio?
            <a href="#" class="link-lilaClaro">Iniciá sesión como institución</a>
        </p>
    </div>

</asp:Content>
