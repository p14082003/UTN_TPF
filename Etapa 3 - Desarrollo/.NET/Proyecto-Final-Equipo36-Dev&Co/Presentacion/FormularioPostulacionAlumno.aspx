<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="FormularioPostulacionAlumno.aspx.cs" Inherits="Presentacion.FormularioPostulacionAlumno" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <style>
        .btn-lilaOscuro-invertido {
            background-color: #FFFFFF !important;
            color: #373A72 !important;
            border: 1px solid #373A72;
            border-radius: 15px;
            padding: 14px;
            font-weight: 600;
            transition: background-color 0.2s ease, color 0.2s ease;
        }

        .btn-lilaOscuro-invertido:hover {
            background-color: #373A72 !important;
            color: #FFFFFF !important;
        }

        * {
            scrollbar-color: #373A72 transparent;
        }

    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid">

        <!-- ENCABEZADO -->
        <div class="m-4">
            <h1 class="text-indigo fw-bold text-center">
                Inscribí a tu alumno
            </h1>
            <h5 class="text-center">
                Elegí el curso de la institución y el alumno a cargo que querés inscribir, adjuntá la documentación
                y enviá la postulación.
            </h5>
        </div>

        <!-- CURSO -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <h4 class="text-indigo mb-4">
                        <i class="bi bi-mortarboard mx-2"></i>
                        Curso
                    </h4>

                    <div class="row">
                        <div class="col-8">
                            <!-- INSTITUCION -->
                            <div class="mb-4">
                                <asp:Label ID="lblInstitucion" runat="server" Text="Institución" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:TextBox ID="txtInstitucion" runat="server" CssClass="form-control" ReadOnly="true" Text="Jardín de Infantes N° 915 Mundo Mágico"></asp:TextBox>
                            </div>
                        </div>

                        <div class="col-4">
                            <!-- CICLO LECTIVO -->
                            <div class="mb-4">
                                <asp:Label ID="lblCicloLectivo" runat="server" Text="Ciclo lectivo" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:DropDownList ID="ddlCiclo" runat="server" CssClass="form-control" required="required">
                                    <asp:ListItem Text="Seleccionar..." Value=""></asp:ListItem>
                                    <asp:ListItem Text="2027" Value="2027"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-4">
                            <!-- NIVEL -->
                            <div class="mb-4">
                                <asp:Label ID="lblNivel" runat="server" Text="Nivel" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:DropDownList ID="ddlNivel" runat="server" CssClass="form-control" required="required">
                                    <asp:ListItem Text="Seleccionar..." Value=""></asp:ListItem>
                                    <asp:ListItem Text="Sala de 3" Value="Sala de 3"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-2">
                            <!-- AÑO -->
                            <div class="mb-4">
                                <asp:Label ID="lblAnio" runat="server" Text="Año" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:DropDownList ID="ddlAnio" runat="server" CssClass="form-control" required="required">
                                    <asp:ListItem Text="No aplica" Value="0"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-2">
                            <div class="mb-4">
                                <asp:Label ID="lblDivision" runat="server" Text="División" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:DropDownList ID="ddlDivision" runat="server" CssClass="form-control" required="required">
                                    <asp:ListItem Text="Seleccionar..." Value=""></asp:ListItem>
                                    <asp:ListItem Text="Naranja" Value="Naranja"></asp:ListItem>
                                    <asp:ListItem Text="Verde" Value="Verde"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="col-4">
                            <div class="mb-4">
                                <asp:Label ID="lblTurno" runat="server" Text="Turno" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:DropDownList ID="ddlTurno" runat="server" CssClass="form-control" required="required">
                                    <asp:ListItem Text="Seleccionar..." Value=""></asp:ListItem>
                                    <asp:ListItem Text="Mañana" Value="1"></asp:ListItem>
                                    <asp:ListItem Text="Tarde" Value="2"></asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-4">
                            <!-- VACANTES -->
                            <div class="mb-4">
                                <asp:Label ID="lblVacantes" runat="server" Text="Vacantes disponibles" CssClass="form-label fw-semibold"></asp:Label>
                                <asp:TextBox ID="txtVacantes" runat="server" CssClass="form-control" ReadOnly="true" placeholder="Se completa al elegir el curso"></asp:TextBox>
                            </div>
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <!-- ALUMNO -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <h4 class="text-indigo mb-4">
                        <i class="bi bi-person mx-2"></i>
                        Alumno a inscribir
                    </h4>

                    <!-- ALUMNOS A CARGO DEL RESPONSABLE -->
                    <div class="mb-4">
                        <asp:Label ID="lblAlumno" runat="server" Text="Alumno a cargo" CssClass="form-label fw-semibold"></asp:Label>
                        <asp:DropDownList ID="ddlAlumnos" runat="server" CssClass="form-control" required="required">
                            <asp:ListItem Text="Seleccionar..." Value=""></asp:ListItem>
                            <asp:ListItem Text="Gómez, Delfina" Value="13"></asp:ListItem>
                        </asp:DropDownList>
                    </div>

                </div>
            </div>
        </div>

        <!-- DOCUMENTACION -->
        <div class="row m-4">
            <div class="col">
                <div class="card p-5">

                    <h4 class="text-indigo mb-4">
                        <i class="bi bi-folder2-open mx-2"></i>
                        Documentación
                    </h4>

                    <!-- CARGA DE DOCUMENTOS -->
                    <asp:Label ID="lblDocumentos" runat="server" Text="Documentos de la postulación (PDF, JPG o PNG, hasta 6 MB cada uno)" CssClass="form-label fw-semibold mb-5"></asp:Label>
                    <div class="d-flex align-items-start gap-4 mb-4">
                        <asp:FileUpload ID="fuDocumentos" runat="server" CssClass="form-control" AllowMultiple="true" accept=".pdf,.jpg,.jpeg,.png" />
                        <asp:Button ID="btnSubirDocumentos" runat="server" CssClass="btn-lilaOscuro flex-shrink-0" Text="Subir documentos" CausesValidation="false"></asp:Button>
                    </div>

                    <!-- DOCUMENTOS CARGADOS -->
                    <div class="d-flex flex-wrap gap-4">
                        <div class="btn btn-lilaClaro">
                            <i class="bi bi-file-earmark-text me-1"></i>
                            DNI del alumno
                            <asp:LinkButton ID="lnkEliminarDocumento1" runat="server" CausesValidation="false" CssClass="text-indigo text-decoration-none ms-1">×</asp:LinkButton>
                        </div>
                        <div class="btn btn-lilaClaro">
                            <i class="bi bi-file-earmark-text me-1"></i>
                            Partida de nacimiento
                            <asp:LinkButton ID="lnkEliminarDocumento2" runat="server" CausesValidation="false" CssClass="text-indigo text-decoration-none ms-1">×</asp:LinkButton>
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <!-- BOTON PARA GUARDAR DATOS -->
        <div class="row m-4">
            <div class="col">
                <asp:Button ID="btnGuardar" runat="server" Text="Enviar postulación" CssClass="btn btn-lilaOscuro w-100 mb-4" />
                <p class="text-center text-secondary small">
                    ¿Querés elegir otra institución?
                    <a href="#" class="link-lilaClaro">Volver a la búsqueda</a>
                </p>
            </div>
        </div>
    </div>

</asp:Content>