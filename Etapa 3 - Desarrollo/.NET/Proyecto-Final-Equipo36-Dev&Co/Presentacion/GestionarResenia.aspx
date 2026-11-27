<%@ Page Title="InstiRed - Reseñas" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" Async="true" CodeBehind="GestionarResenia.aspx.cs" Inherits="Presentacion.GestionarResenia" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .encabezado-resenia {
            display: grid;
            grid-template-columns: minmax(4.5rem, 1fr) minmax(0, max-content) minmax(4.5rem, 1fr);
            align-items: center;
            gap: 0.5rem;
            padding: 1rem 1.5rem;
        }

        .encabezado-resenia h2 {
            grid-column: 2;
            margin: 0;
            font-size: clamp(1.125rem, 2vw, 1.5rem);
            text-align: center;
        }

        .volver-resenia {
            grid-column: 3;
            justify-self: end;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="p-3 bg-fondo rounded-4">

        <!-- TÍTULO -->
        <header class="mb-4 ps-2">
            <h1 tabindex="0" class="text-indigo fw-bold">Reseñas de mi institución</h1>
            <p class="text-muted">Leé lo que opinan los padres, madres y tutores y respondé cada reseña. Podés eliminar tu respuesta y volver a responder.</p>
        </header>

        <!-- MENSAJE DE RESULTADO -->
        <asp:Panel ID="pnlMensaje" runat="server" Visible="false" CssClass="alert mb-4">
            <asp:Literal ID="litMensaje" runat="server" />
        </asp:Panel>

        <!-- VISTA 1: RESUMEN -->
        <asp:Panel ID="pnlDashboard" runat="server" CssClass="resumen-gestion row mb-4">
            <div class="col-6 col-md-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-chat-left-text text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Reseñas</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblTotal" runat="server" Text="-" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-6 col-md-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-hourglass-split text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Sin responder</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblSinResponder" runat="server" Text="-" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-6 col-md-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-check-circle text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Respondidas</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblRespondidas" runat="server" Text="-" /></h2>
                    </div>
                </div>
            </div>
            <div class="col-6 col-md-3 mb-3">
                <div class="card shadow-sm h-100 bg-white">
                    <div class="card-body text-center px-2 py-4">
                        <h5 class="card-title text-indigo"><i class="bi bi-star-fill text-lavanda fs-4 d-block d-sm-inline me-sm-2"></i>Promedio</h5>
                        <h2 class="display-4 fw-bold text-indigo mt-3"><asp:Label ID="lblPromedio" runat="server" Text="-" /></h2>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 2: LISTADO DE RESEÑAS -->
        <asp:Panel ID="pnlListado" runat="server" CssClass="card shadow-sm mb-5 border-0">
            <div class="card-header bg-indigo text-white py-3">
                <h5 class="mb-0"><i class="bi bi-list-ul me-2"></i>Reseñas recibidas</h5>
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- Filtro -->
                <div class="row g-3 mb-3">
                    <div class="col-12 col-sm-6 col-md-4">
                        <label for="<%= ddlFiltroEstado.ClientID %>" class="form-label text-indigo fw-semibold">Mostrar</label>
                        <asp:DropDownList ID="ddlFiltroEstado" runat="server" CssClass="form-select border-neutro" AutoPostBack="true"
                            OnSelectedIndexChanged="Filtro_SelectedIndexChanged">
                            <asp:ListItem Text="Todas" Value="Todas" Selected="True" />
                            <asp:ListItem Text="Sin responder" Value="SinResponder" />
                            <asp:ListItem Text="Respondidas" Value="Respondidas" />
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="table-responsive">
                    <asp:GridView ID="dgvResenias" runat="server" CssClass="table table-hover align-middle border-neutro"
                        AutoGenerateColumns="False" GridLines="None" DataKeyNames="IdResenia"
                        AllowPaging="true" PageSize="25" OnPageIndexChanging="dgvResenias_PageIndexChanging"
                        EmptyDataText="No hay reseñas para mostrar con este filtro.">
                        <HeaderStyle CssClass="text-indigo border-bottom border-neutro" />
                        <EmptyDataRowStyle CssClass="text-muted text-center py-4" />
                        <PagerStyle CssClass="text-indigo" HorizontalAlign="Center" />
                        <Columns>
                            <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                            <asp:BoundField DataField="NombreCompleto" HeaderText="Responsable" />
                            <asp:TemplateField HeaderText="Calificación">
                                <ItemTemplate>
                                    <span class="text-lavanda text-nowrap" role="img" aria-label="<%# Eval("Calificacion") %> de 5 estrellas">
                                        <%# new string('★', Convert.ToInt32(Eval("Calificacion"))) %><span class="text-muted opacity-50"><%# new string('★', 5 - Convert.ToInt32(Eval("Calificacion"))) %></span>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Comentario" HeaderText="Comentario" />
                            <asp:TemplateField HeaderText="Estado">
                                <ItemTemplate>
                                    <span class="badge rounded-pill px-3 py-2 <%# Eval("RespuestaInstitucion") == null ? "bg-lavanda" : "bg-indigo" %>">
                                        <%# Eval("RespuestaInstitucion") == null ? "Sin responder" : "Respondida" %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Acciones">
                                <ItemTemplate>
                                    <asp:LinkButton ID="btnVerDetalle" runat="server" CssClass="btn btn-sm btn-outline-lavanda rounded-circle d-inline-flex align-items-center justify-content-center m-3"
                                        CommandArgument='<%# Eval("IdResenia") %>' OnClick="btnVerDetalle_Click">
                                         <i class='<%# Eval("RespuestaInstitucion") == null ? "bi bi-pencil-square fs-lg" : "bi bi-eye fs-lg" %>'></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <!-- VISTA 3: DETALLE DE LA RESEÑA Y RESPUESTA -->
        <asp:Panel ID="pnlDetalle" runat="server" CssClass="card shadow border-neutro mb-5" Visible="false">
            <div class="card-header bg-indigo text-white encabezado-resenia">
                <h2>Reseña</h2>
                <asp:Button ID="btnCerrarDetalle" runat="server" Text="Volver" CssClass="btn btn-sm bg-fondo text-indigo fw-bold rounded-pill px-3 volver-resenia" OnClick="btnCerrarDetalle_Click" />
            </div>
            <div class="card-body bg-white rounded-bottom-4">

                <!-- La reseña -->
                <div class="p-4 bg-fondo rounded-4 border border-neutro mb-4">
                    <div class="row mb-3">
                        <div class="col-md-5 mb-2">
                            <span class="text-muted d-block small">Responsable</span>
                            <asp:Label ID="lblDetalleResponsable" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-3 mb-2">
                            <span class="text-muted d-block small">Fecha</span>
                            <asp:Label ID="lblDetalleFecha" runat="server" CssClass="text-indigo fw-bold fs-5" />
                        </div>
                        <div class="col-md-4 mb-2">
                            <span class="text-muted d-block small">Calificación</span>
                            <asp:Label ID="lblDetalleEstrellas" runat="server" CssClass="text-lavanda fs-5 text-nowrap" />
                        </div>
                    </div>
                    <span class="text-muted d-block small">Comentario</span>
                    <asp:Label ID="lblDetalleComentario" runat="server" CssClass="text-indigo d-block" />
                </div>

                <!-- Reseña sin responder: formulario de respuesta -->
                <asp:Panel ID="pnlResponder" runat="server">
                    <label for="<%= txtRespuesta.ClientID %>" class="form-label text-indigo fw-bold">Tu respuesta</label>
                    <asp:TextBox ID="txtRespuesta" runat="server" TextMode="MultiLine" Rows="4" maxlength="255"
                        CssClass="form-control border-neutro rounded-3" />
                    <div class="d-flex justify-content-between align-items-start mt-1">
                        <div class="form-text">
                            <i class="bi bi-info-circle text-lavanda me-1"></i>
                            Cada reseña admite una respuesta. Podés eliminarla si necesitás escribir otra.
                        </div>
                        <span id="contadorRespuesta" class="form-text text-nowrap ms-3">0 / 255</span>
                    </div>

                    <div class="d-flex justify-content-end gap-3 mt-4">
                        <asp:Button ID="btnCancelarRespuesta" runat="server" Text="Cancelar" CssClass="btn btn-lilaClaro fw-bold rounded-pill px-4" OnClick="btnCerrarDetalle_Click" />
                        <asp:Button ID="btnEnviarRespuesta" runat="server" Text="Enviar respuesta" CssClass="btn btn-indigo fw-bold rounded-pill px-4"
                            OnClick="btnEnviarRespuesta_Click"
                            OnClientClick="return confirmarAccion(this, '¿Desea enviar esta respuesta?', 'Aceptar');" />
                    </div>
                </asp:Panel>

                <!-- Respuesta publicada y opción para eliminarla -->
                <asp:Panel ID="pnlRespuestaExistente" runat="server" Visible="false">
                    <span class="text-indigo fw-bold d-block mb-2">
                        <i class="bi bi-check-circle-fill text-lavanda me-1"></i>Respuesta de la institución
                    </span>
                    <div class="p-4 bg-fondo rounded-4 border border-neutro">
                        <asp:Label ID="lblRespuestaExistente" runat="server" CssClass="text-indigo d-block" />
                    </div>
                    <div class="d-flex justify-content-end mt-3">
                        <asp:Button ID="btnEliminarRespuesta" runat="server" Text="Eliminar respuesta" CssClass="btn btn-outline-danger rounded-pill px-4"
                            OnClick="btnEliminarRespuesta_Click" CausesValidation="false"
                            OnClientClick="return confirmarAccion(this, '¿Querés eliminar la respuesta de tu institución? La reseña y la puntuación del responsable se conservarán.' , 'Aceptar');" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </div>


    <script>
        (function () {
            var campo = document.getElementById('<%= txtRespuesta.ClientID %>');
            var contador = document.getElementById('contadorRespuesta');
            if (!campo || !contador) return;

            function actualizar() { contador.textContent = campo.value.length + ' / 255'; }
            campo.addEventListener('input', actualizar);
            actualizar();
        })();
    </script>
</asp:Content>
