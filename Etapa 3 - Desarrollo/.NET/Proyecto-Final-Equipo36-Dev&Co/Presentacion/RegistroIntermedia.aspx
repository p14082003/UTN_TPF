<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="RegistroIntermedia.aspx.cs" Inherits="Presentacion.prueba" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    
<div class="container py-2">

    <div class="text-center mb-2">

        <h1>Encontrá el lugar ideal para aprender</h1>

        <p>
            Creá tu cuenta para descubrir instituciones educativas y gestionar
            las postulaciones de tus hijos de forma ágil y segura.
        </p>

        <span class="badge rounded-pill text-white" style="background-color: #373A78;">
            ★ RECOMENDADO PARA FAMILIAS Y TUTORES
        </span>

    </div>



    <div class="card border-0 shadow-sm rounded-4 mb-2">

        <div class="card-body p-4 p-lg-5">

            <div class="row align-items-center g-4">

                <div class="col-md-5">

                    <div class="d-flex flex-column gap-2">

                        <div class="mb-2">
                            <img src="Imagenes/PerfilFamiliar.png" alt="Perfil familiar" width="80" height="80" class="img-fluid" />
                        </div>

                        <small class="text-uppercase">
                            Perfil familiar
                        </small>

                        <h2 class="h4 mb-0">
                            Soy padre, madre o tutor
                        </h2>

                        <p class="mb-2">
                            Buscá instituciones educativas, conocé sus proyectos
                            pedagógicos y administrá todas las admisiones de tus
                            alumnos a cargo en un único lugar.
                        </p>

                        <div class="d-flex align-items-center gap-2">

                            <div>
                                <div class="mb-2">
                                    <img src="Imagenes/Usuarios.png" alt="Usuarios" width="80" height="80" class="img-fluid" />
                                </div>
                            </div>

                            <div>
                                <small class="d-block fw-bold">
                                    +18.000 familias
                                </small>

                                <small class="text-muted">
                                    Ya eligieron escuela
                                </small>
                            </div>

                        </div>

                    </div>

                </div>

                <div class="col-md-7">

                    <div class="d-flex flex-column gap-3 p-3 p-lg-4 rounded-4">

                        <h3 class="h6 text-uppercase mb-1">
                            ¿Qué podés hacer con esta cuenta?
                        </h3>

                        <div class="d-flex align-items-start gap-3">

                            <div class="flex-shrink-0">
                                <span style="color: #8D91D5; font-size: 1.3rem;">★</span>
                            </div>
                            <div>
                                <h4 class="h6 mb-1">
                                    Búsqueda geolocalizada y comparador pedagógico
                                </h4>

                                <p class="small mb-0">
                                    Filtrá por barrio, orientación, jornada y
                                    servicios extracurriculares.
                                </p>
                            </div>

                        </div>

                        <div class="d-flex align-items-start gap-3">

                            <div class="flex-shrink-0">
                                <span style="color: #8D91D5; font-size: 1.3rem;">★</span>
                            </div>

                            <div>
                                <h4 class="h6 mb-1">
                                    Seguimiento en tiempo real de vacantes y entrevistas
                                </h4>

                                <p class="small mb-0">
                                    Notificaciones directas sobre disponibilidad
                                    de bancos y fechas de reunión.
                                </p>
                            </div>

                        </div>

                        <div class="d-flex align-items-start gap-3">

                            <div class="flex-shrink-0">
                                <span style="color: #8D91D5; font-size: 1.3rem;">★</span>
                            </div>

                            <div>
                                <h4 class="h6 mb-1">
                                    Trámites de postulación 100 % digitalizados
                                </h4>

                                <p class="small mb-0">
                                    Cargá la documentación una única vez y aplicá
                                    a múltiples colegios sin papeles.
                                </p>
                            </div>

                        </div>     
                        <div class="mt-2">
                            <asp:Button
                                ID="btnRegistrarResponsable" runat="server" OnClick="btnRegistrarResponsable_Click" Text="Registrar responsable →" CssClass="btn btn-primary w-100 py-3" Style="background-color: #373A78; border-color: #373A78;"/>

                            <small class="d-block text-center text-muted mt-2">
                                Sin costo de registro· Toma menos de 2 minutos
                            </small>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>


    <div class="card border-0 shadow-sm rounded-4">

        <div class="card-body p-3 p-lg-4">

            <div class="row align-items-center g-3">
 
                <div class="col-md-8">

                    <div class="d-flex align-items-start gap-3">

                        <div class="flex-shrink-0">
                           <div class="mb-2">
                                <img src="Imagenes/Institucion.png" alt="Usuarios" width="80" height="80" class="img-fluid" />
                            </div>
                        </div>

                        <div>
                            <h3 class="h6 mb-1">
                                ¿Querés registrar una institución educativa?
                            </h3>

                            <p class="small mb-0">
                                Para directivos, colegios e institutos que deseen
                                publicar vacantes y administrar admisiones.
                            </p>
                        </div>
                    </div>
                </div>

                <div class="col-md-4">
                    <asp:Button
                        ID="btnAccesoInstituciones" runat="server" Text="Acceso para instituciones →"  CssClass="btn btn-outline-primary w-100" />
                </div>
            </div>
        </div>
    </div>

</div>
</asp:Content>