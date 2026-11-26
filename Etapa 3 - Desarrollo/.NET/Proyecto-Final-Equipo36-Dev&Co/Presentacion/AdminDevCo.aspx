<%@ Page Title="Panel de control" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="AdminDevCo.aspx.cs" Inherits="Presentacion.AdminDevCo" %>

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
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="p-3 bg-fondo rounded-9">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2">
            <h1 tabindex="0" class="text-indigo fw-bold">Panel de Control - Dev&Co </h1>
            <p class="fg-secondary">Gestión de solicitudes de administradores y de instituciones.</p>
        </header>

        <!-- MENSAJE (éxito o error de la última acción) -->
        <asp:Panel ID="pnlMensaje" runat="server" CssClass="card card-body border mb-4" Visible="false">
            <asp:Label ID="lblMensaje" runat="server"></asp:Label>
        </asp:Panel>

        <!-- TARJETAS -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="resumen-gestion row mb-4">
            <div class="md:col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-hourglass-split text-lavanda fs-4 me-2"></i>Pendientes</h5>
                        <h2 class="fs-5xl fw-bold text-indigo mt-3"><asp:Literal ID="litPendientes" runat="server" Text="—" /></h2>
                    </div>
                </div>
            </div>
            <div class="md:col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-building-check text-lavanda fs-4 me-2"></i>Activas</h5>
                        <h2 class="fs-5xl fw-bold text-indigo mt-3"><asp:Literal ID="litActivas" runat="server" Text="—" /></h2>
                    </div>
                </div>
            </div>
            <div class="md:col-4 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-x-octagon text-lavanda fs-4 me-2"></i>Dadas de baja</h5>
                        <h2 class="fs-5xl fw-bold text-indigo mt-3"><asp:Literal ID="litDadasDeBaja" runat="server" Text="—" /></h2>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- MENÚ -->
        <div class="d-flex gap-5 mb-4">
            <asp:Button ID="btnMenuSolicitudes" runat="server" Text="Solicitudes" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnMenuSolicitudes_Click" />
            <asp:Button ID="btnMenuInstituciones" runat="server" Text="Instituciones" CssClass="btn btn-outline-lavanda fw-bold rounded-pill px-4" OnClick="btnMenuInstituciones_Click" />
        </div>

        <!-- ================= SOLICITUDES ================= -->

        <!-- LISTADO DE SOLICITUDES -->
        <asp:Panel ID="pnlSolicitudes" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Solicitudes de administradores</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-9">
                <div class="mb-3" style="max-width: 260px;">
                    <label for="<%= ddlFiltroEstado.ClientID %>" class="form-label text-indigo fw-bold">Mostrar</label>
                    <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-control border-neutro" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroEstado_SelectedIndexChanged">
                        <asp:ListItem Text="Pendientes" Value="1" Selected="True"></asp:ListItem>
                        <asp:ListItem Text="Aprobadas" Value="3"></asp:ListItem>
                        <asp:ListItem Text="Rechazadas" Value="4"></asp:ListItem>
                        <asp:ListItem Text="Canceladas" Value="5"></asp:ListItem>
                        <asp:ListItem Text="Todas" Value=""></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="table-responsive">
                    <asp:GridView ID="dgvSolicitudes" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" EmptyDataText="No hay solicitudes para mostrar.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <Columns>
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="NombreInstitucion" HeaderText="Institución" />
                            <asp:BoundField DataField="CuitInstitucion" HeaderText="CUIT" />
                            <asp:BoundField DataField="Administrador" HeaderText="Administrador" />
                            <asp:BoundField DataField="EmailAdministrador" HeaderText="Correo electrónico" />
                            <asp:BoundField DataField="EstadoVisible" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerSolicitud" runat="server" CommandArgument='<%# Eval("IdPostulacion") %>' CssClass="btn btn-outline-secondary rounded-pill px-3 fw-bold" OnClick="btnVerSolicitud_Click">
                                       <i class="bi bi-eye fg-info fs-lg"></i> Ver
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- DETALLE DE LA SOLICITUD -->
        <asp:Panel ID="pnlDetalleSolicitud" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white encabezado-postulacion">
                <h2>Revisión de solicitud</h2>
                <asp:Button ID="btnVolverSolicitud" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3 volver-postulacion" OnClick="btnVolverSolicitud_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-9">


                <!-- Institución seleccionada -->
                <div class="p-4 bg-fondo rounded-9 border border-neutro mb-4">
                    <h6 class="text-indigo fw-bold mb-3">Institución</h6>
                    <div class="row">
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Nombre</span>
                            <span class="text-indigo fw-bold fs-lg"><asp:Literal ID="litSolInstitucion" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">CUIT</span>
                            <span class="text-indigo fw-bold fs-lg"><asp:Literal ID="litSolCuit" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Fecha de la solicitud</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litSolFecha" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Código de seguimiento</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litSolCodigo" runat="server" Mode="Encode" /></span>
                        </div>
                    </div>
                </div>

                <!-- Administrador que envió la postulación -->
                <div class="p-4 bg-fondo rounded-9 border border-neutro mb-4">
                    <h6 class="text-indigo fw-bold mb-3">Administrador</h6>
                    <div class="row">
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Nombre y apellido</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litSolAdministrador" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Documento</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litSolDocumento" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Correo electrónico</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litSolEmail" runat="server" Mode="Encode" /></span>
                        </div>
                    </div>
                </div>

                <!-- Documentación (se abre en otra pestaña) -->
                <div class="p-4 bg-fondo rounded-9 border border-neutro mb-4">
                    <h6 class="text-indigo fw-bold mb-3">Documentación</h6>
                    <ul class="list-unstyled mb-0">
                        <li class="mb-1"><i class="bi bi-file-earmark-text text-lavanda me-2"></i><asp:HyperLink ID="lnkConstanciaFiscal" runat="server" Target="_blank" CssClass="link-lilaClaro" Text="Constancia fiscal" /></li>
                        <li class="mb-1"><i class="bi bi-file-earmark-text text-lavanda me-2"></i><asp:HyperLink ID="lnkHabilitacion" runat="server" Target="_blank" CssClass="link-lilaClaro" Text="Habilitación" /></li>
                        <li class="mb-1"><i class="bi bi-file-earmark-text text-lavanda me-2"></i><asp:HyperLink ID="lnkConstanciaLaboral" runat="server" Target="_blank" CssClass="link-lilaClaro" Text="Constancia laboral" /></li>
                    </ul>
                </div>

                <!-- Acciones: solo si la solicitud está Pendiente. Si ya fue Aprobada o Rechazada, ocultar este panel (pnlAcciones.Visible = false)
                     y mostrar el estado y el motivo del rechazo. -->
                <asp:Panel ID="pnlAcciones" runat="server">
                    <div class="d-flex justify-content-end gap-5 mt-4">
                        <asp:Button ID="btnDesplegarRechazo" runat="server" Text="Rechazar" CssClass="btn-outline theme-danger fw-bold rounded-pill px-4" OnClick="btnDesplegarRechazo_Click" />
                        <asp:Button ID="btnAprobar" runat="server" Text="Aprobar solicitud" CssClass="btn btn-indigo fw-bold rounded-pill px-4" OnClick="btnAprobar_Click" />
                    </div>

                    <asp:Panel ID="panelRechazo" runat="server" CssClass="mt-4" Visible="false">
                        <div class="card card-body bg-fondo border-danger rounded-9">
                            <label for="<%= txtMotivoRechazo.ClientID %>" class="form-label fg-danger fw-bold">Motivo del rechazo (obligatorio)</label>
                            <asp:TextBox ID="txtMotivoRechazo" runat="server" TextMode="MultiLine" Rows="3" MaxLength="255" CssClass="form-control mb-3 border-neutro rounded-5"></asp:TextBox>
                            <div class="d-flex justify-content-end gap-5">
                                <asp:Button ID="btnCancelarRechazo" runat="server" Text="Cancelar" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3" OnClick="btnCancelarRechazo_Click" />
                                <asp:Button ID="btnConfirmarRechazo" runat="server" Text="Confirmar rechazo" CssClass="btn-solid theme-danger fw-bold rounded-pill px-4" OnClick="btnConfirmarRechazo_Click" />
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>
                <asp:Panel ID="pnlResuelta" runat="server" CssClass="p-4 bg-fondo rounded-9 border border-neutro mb-4" Visible="false">
                    <span class="fg-secondary d-block small">Estado</span>
                    <asp:Label ID="lblSolEstado" runat="server" CssClass="fw-bold"></asp:Label>
                    <asp:Panel ID="pnlMotivoRechazo" runat="server" CssClass="mt-3" Visible="false">
                        <span class="fg-secondary d-block small">Motivo del rechazo</span>
                        <asp:Label ID="lblMotivoRechazo" runat="server" CssClass="text-indigo"></asp:Label>
                    </asp:Panel>
                </asp:Panel>
            </div>
        </asp:Panel>

        <!-- ================= INSTITUCIONES ================= -->

        <!-- LISTADO DE INSTITUCIONES -->
        <asp:Panel ID="pnlInstituciones" runat="server" CssClass="card shadow-sm mb-5 border-0" Visible="false">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-building me-2"></i>Instituciones</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-9">
                <div class="table-responsive">
                    <asp:GridView ID="dgvInstituciones" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" EmptyDataText="No hay instituciones para mostrar.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <Columns>
                            <asp:BoundField DataField="Nombre" HeaderText="Institución" />
                            <asp:BoundField DataField="Cuit" HeaderText="CUIT" />
                            <asp:BoundField DataField="TipoInstitucion" HeaderText="Tipo" />
                            <asp:BoundField DataField="Email" HeaderText="Correo electrónico" />
                            <asp:BoundField DataField="EstadoVisible" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerInstitucion" runat="server" CommandArgument='<%# Eval("IdInstitucion") %>' CssClass="btn btn-outline-secondary rounded-pill px-3 fw-bold" OnClick="btnVerInstitucion_Click">
                                        <i class="bi bi-eye fg-info fs-lg"></i> Ver
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- DETALLE DE LA INSTITUCIÓN -->
        <asp:Panel ID="pnlDetalleInstitucion" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white encabezado-postulacion">
                <h2>Detalle de la institución</h2>
                <asp:Button ID="btnVolverInstitucion" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3 volver-postulacion" OnClick="btnVolverInstitucion_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-9">

                <div class="p-4 bg-fondo rounded-9 border border-neutro mb-4">
                    <div class="row">
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Institución</span>
                            <span class="text-indigo fw-bold fs-lg"><asp:Literal ID="litInstNombre" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">CUIT</span>
                            <span class="text-indigo fw-bold fs-lg"><asp:Literal ID="litInstCuit" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Tipo</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litInstTipo" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Correo electrónico</span>
                            <span class="text-indigo fw-bold"><asp:Literal ID="litInstEmail" runat="server" Mode="Encode" /></span>
                        </div>
                        <div class="md:col-6 mb-2">
                            <span class="fg-secondary d-block small">Estado</span>
                            <asp:Label ID="lblInstEstado" runat="server" />
                        </div>
                    </div>
                </div>

                <!-- Baja / reactivación de la institución.
                     Si la institución está activa se muestra pnlBajaInstitucion; si está dada de baja, pnlReactivarInstitucion. -->
                <div class="mb-4">
                    <asp:Panel ID="pnlBajaInstitucion" runat="server">
                        <p class="fg-secondary small mb-2">
                            Al dar de baja la institución se rechazan automáticamente las postulaciones de alumnos pendientes y aceptadas.
                            Las confirmadas se conservan. Si después se reactiva, las rechazadas no se restauran.
                        </p>
                       
                        <asp:Button ID="btnBajaInstitucion" runat="server" Text="Dar de baja la institución" CssClass="btn-outline theme-danger fw-bold rounded-pill px-4"
                            OnClientClick="return confirm('¿Dar de baja esta institución? Se rechazarán las postulaciones pendientes y aceptadas.');" OnClick="btnBajaInstitucion_Click" />
                    </asp:Panel>
                    <asp:Panel ID="pnlReactivarInstitucion" runat="server" Visible="false">
                        <asp:Button ID="btnReactivarInstitucion" runat="server" Text="Reactivar institución" CssClass="btn btn-indigo fw-bold rounded-pill px-4"
                            OnClientClick="return confirm('¿Reactivar esta institución?');" OnClick="btnReactivarInstitucion_Click" />
                    </asp:Panel>
                </div>

                <!-- Administradores de la institución seleccionada -->
                <h6 class="text-indigo fw-bold mb-3">Administradores</h6>
                <div class="table-responsive">
                    <asp:GridView ID="dgvAdministradores" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" EmptyDataText="La institución no tiene administradores.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <Columns>
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Administrador" />
                            <asp:BoundField DataField="DocAdministrador" HeaderText="Documento" />
                            <asp:BoundField DataField="EmailAdministrador" HeaderText="Correo electrónico" />
                            <asp:BoundField DataField="EstadoVisible" HeaderText="Estado" />
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnBajaAdministrador" runat="server" Visible='<%# (bool)Eval("UsuarioActivo") %>' CommandArgument='<%# Eval("IdAdministrador") %>'
                                        CssClass="btn btn-light btn-sm rounded-4 p-3"
                                        OnClientClick="return confirm('¿Dar de baja a este administrador?');" OnClick="btnBajaAdministrador_Click">
                                        <i class="bi bi-trash fg-danger fs-lg"></i>
                                        Baja
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnReactivarAdministrador" runat="server" Visible='<%# !(bool)Eval("UsuarioActivo") %>' CommandArgument='<%# Eval("IdAdministrador") %>'
                                        CssClass="btn btn-light btn-sm rounded-4 p-3"
                                        OnClientClick="return confirm('¿Reactivar a este administrador?');" OnClick="btnReactivarAdministrador_Click">
                                        <i class="bi bi-arrow-repeat fg-success fs-lg"></i> 
                                        Reactivar
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

    </div>
</asp:Content>
