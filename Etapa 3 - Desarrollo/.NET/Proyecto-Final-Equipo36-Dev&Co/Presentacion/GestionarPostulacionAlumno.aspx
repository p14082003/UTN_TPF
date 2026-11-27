<%@ Page Title="InstiRed - Postulaciones recibidas" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="GestionarPostulacionAlumno.aspx.cs" Inherits="Presentacion.GestionarPostulacionAlumno" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .encabezado-postulacion {
            display: grid;
            grid-template-columns: minmax(4.5rem, 1fr) minmax(0, max-content) minmax(4.5rem, 1fr);
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 1.5rem;
        }

        .encabezado-postulacion h2 {
            grid-column: 2;
            margin: 0;
            font-size: clamp(1.125rem, 2vw, 1.5rem);
            text-align: center;
        }

        .volver-postulacion {
            grid-column: 3;
            justify-self: end;
        }

        .datos-postulacion {
            display: grid;
            grid-template-columns: minmax(0, 2fr) minmax(0, 1fr);
            gap: 1rem;
        }

        @media (max-width: 767px) {
            .datos-postulacion {
                grid-template-columns: minmax(0, 1fr);
            }
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2">
            <h1 tabindex="0" class="text-indigo fw-bold">Postulaciones recibidas</h1>
            <p class="text-muted">Gestión de las postulaciones de alumnos a tu institución.</p>
        </header>

        <!-- MENSAJE DE RESULTADO DE LA ÚLTIMA ACCIÓN -->
        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert mb-4">
            <asp:Literal ID="litMensaje" runat="server" />
        </asp:Panel>

        <!-- VISTA 1: DASHBOARD -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="resumen-gestion row mb-5">
            <div class="col-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-hourglass-split text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Pendientes</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblPendientes" runat="server" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-check-circle text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Aceptadas</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblAceptadas" runat="server" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-check-circle-fill text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Confirmadas</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblConfirmadas" runat="server" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-x-octagon text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Rechazadas</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblRechazadas" runat="server" /></h2>
                    </div>
                </div>
            </div>
        </asp:Panel>


        <!-- VISTA 2: LISTADO DE POSTULACIONES -->
        <asp:Panel ID="pnlListado" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Postulaciones de alumnos</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">
                <div class="mb-3">
                    <asp:Label runat="server" AssociatedControlID="ddlEstado" Text="Estado" CssClass="form-label text-indigo fw-semibold" />
                    <asp:DropDownList ID="ddlEstado" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlEstado_SelectedIndexChanged">
                        <asp:ListItem Value="1" Selected="True">Pendientes</asp:ListItem>
                        <asp:ListItem Value="2">Aceptadas</asp:ListItem>
                        <asp:ListItem Value="3">Confirmadas</asp:ListItem>
                        <asp:ListItem Value="4">Rechazadas</asp:ListItem>
                        <asp:ListItem Value="5">Canceladas</asp:ListItem>
                        <asp:ListItem Value="0">Todas</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="dgvPostulaciones" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" DataKeyNames="IdPostulacion"
                        EmptyDataText="No hay postulaciones para mostrar con este filtro." AllowPaging="true" PageSize="10" OnPageIndexChanging="dgvPostulaciones_PageIndexChanging">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <EmptyDataRowStyle CssClass="text-muted text-center py-4" />
                        <PagerStyle CssClass="paginador-grilla" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="Alumno" HeaderText="Alumno" />
                            <asp:BoundField DataField="Curso" HeaderText="Curso" />
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="CodigoSeguimiento" HeaderText="Código de seguimiento" ItemStyle-CssClass="text-nowrap" />
                            <asp:BoundField DataField="NombreTutor" HeaderText="Tutor" />
                            <asp:BoundField DataField="Estado" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerDetalle" runat="server" CssClass="btn btn-outline-secondary rounded-pill px-3 fw-bold"
                                        CommandArgument='<%# Eval("IdPostulacion") %>' OnClick="btnVerDetalle_Click">
                                         <i class="bi bi-eye fg-info fs-lg"></i> Ver
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 3: DETALLE DE LA POSTULACIÓN -->
        <asp:Panel ID="pnlDetalle" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white encabezado-postulacion">
                <h2>Revisión de Postulación</h2>
                <asp:LinkButton ID="btnCerrarDetalle" runat="server" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3 volver-postulacion" OnClick="btnCerrarDetalle_Click"> 
                    <i class="bi bi-arrow-left fs-lg"></i> 
                    Volver
                </asp:LinkButton>
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- Datos del alumno y la postulación -->
                <div class="p-4 bg-fondo rounded-4 border border-neutro mb-4">
                    <div class="datos-postulacion">
                        <dl class="mb-0">
                            <dt class="text-muted small fw-normal">Código de seguimiento</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litCodigoSeguimiento" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Estado</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litEstadoPostulacion" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Alumno</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litAlumno" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Curso</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litCurso" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">DNI del alumno</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litDniAlumno" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Fecha de nacimiento</dt>
                            <dd class="text-indigo fw-bold fs-5 mb-0"><asp:Literal ID="litFechaNacimientoAlumno" runat="server" Mode="Encode" /></dd>
                        </dl>

                        <dl class="mb-0">
                            <dt class="text-muted small fw-normal">Tutor</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litTutor" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Rol del responsable</dt>
                            <dd class="text-indigo fw-bold fs-5"><asp:Literal ID="litRolResponsable" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Correo del tutor</dt>
                            <dd class="text-indigo fw-bold fs-5 text-break"><asp:Literal ID="litCorreoTutor" runat="server" Mode="Encode" /></dd>

                            <dt class="text-muted small fw-normal">Teléfonos del responsable</dt>
                            <dd class="text-indigo fw-bold fs-5 mb-0"><asp:Literal ID="litTelefonosResponsable" runat="server" Mode="Encode" /></dd>
                        </dl>
                    </div>
                </div>

                <!-- Documentación -->
                <div class="mb-4">
                    <span class="text-muted d-block small mb-2">Documentación presentada</span>
                    <ul class="list-unstyled mb-0">
                        <asp:Repeater ID="rptDocumentos" runat="server">
                            <ItemTemplate>
                                <li class="p-3 bg-fondo rounded-4 border border-neutro mb-2">
                                    <i class="bi bi-file-earmark-text text-lavanda me-2"></i>
                                    <a href="<%#: ResolveUrl(Eval("Id", "~/DescargarDocumentoAlumno.ashx?id={0}")) %>" class="link-lilaClaro"><%#: Eval("Descripcion") %></a>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                    <asp:Panel ID="pnlSinDocumentos" runat="server" Visible="false" CssClass="text-muted">
                        No hay documentos adjuntos.
                    </asp:Panel>
                </div>

                <asp:Label ID="lblAvisoEstado" runat="server" Visible="false" CssClass="alert alert-info d-block mb-3" />
                <asp:Panel ID="pnlMotivoRechazo" runat="server" Visible="false" CssClass="p-3 bg-fondo rounded-4 border border-neutro mb-3">
                    <span class="text-indigo fw-bold d-block">Motivo de rechazo</span>
                    <asp:Literal ID="litMotivoRechazo" runat="server" Mode="Encode" />
                </asp:Panel>

                <!-- Vacantes: el code-behind muestra uno de los dos paneles -->
                <asp:Panel ID="pnlConVacantes" runat="server" CssClass="p-3 bg-fondo rounded-4 border border-neutro">
                    <i class="bi bi-check-circle-fill text-lavanda me-2"></i>
                    <span class="text-indigo">Vacantes disponibles en este curso:
                        <asp:Label ID="lblVacantes" runat="server" CssClass="fw-bold" />.
                        La vacante se descuenta cuando el responsable confirma la postulación.
                    </span>
                </asp:Panel>

                <asp:Panel ID="pnlSinVacantes" runat="server" Visible="false" CssClass="p-3 rounded-4 border border-danger bg-fondo">
                    <i class="bi bi-exclamation-triangle-fill text-danger me-2"></i>
                    <span class="text-danger fw-bold">No hay vacantes disponibles en este curso.</span>
                    <span class="text-indigo">Solo podés rechazar la postulación.</span>
                </asp:Panel>

                <!-- Acciones -->
                <div class="d-flex justify-content-end gap-3 mt-4">
                   
                    <asp:LinkButton ID="btnDesplegarRechazo" runat="server" CssClass="btn btn-outline-danger fw-bold rounded-pill px-4" OnClick="btnDesplegarRechazo_Click"> 
                        <i class="bi bi-x-lg fg-danger fs-lg me-2"></i> 
                        Rechazar
                    </asp:LinkButton>

                    <asp:LinkButton ID="btnAceptar" runat="server" CssClass="btn btn-light fw-bold rounded-pill px-4" OnClientClick="return confirmarAccion(this, '¿Desea aceptar esta postulación?', 'Aceptar');" OnClick="btnAceptar_Click"> 
                        <i class="bi bi-check-lg fg-success fs-lg me-2"></i> 
                        Aceptar Postulación
                    </asp:LinkButton>
                </div>

                <!-- Rechazo con motivo -->
                <asp:Panel ID="panelRechazo" runat="server" CssClass="mt-4" Visible="false">
                    <div class="card card-body bg-fondo border-danger rounded-4">
                        <label class="form-label text-danger fw-bold">Motivo del rechazo (Obligatorio)</label>
                        <asp:TextBox ID="txtMotivoRechazo" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control mb-3 border-neutro rounded-3"></asp:TextBox>
                        <div class="text-end">
                            <asp:LinkButton ID="btnConfirmarRechazo" runat="server" CssClass="btn btn-danger fw-bold rounded-pill px-4" OnClick="btnConfirmarRechazo_Click"> 
                                <i class="bi bi-x-lg fg-danger fs-lg me-2"></i>
                                Confirmar Rechazo
                            </asp:LinkButton>
                
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
