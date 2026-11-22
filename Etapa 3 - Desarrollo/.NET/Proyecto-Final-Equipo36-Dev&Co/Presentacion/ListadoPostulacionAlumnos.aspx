<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="ListadoPostulacionAlumnos.aspx.cs" Inherits="Presentacion.ListadoPostulacionAlumnos" %>
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

        .idioma-y-botones {
            display: grid;
            grid-template-columns:
                minmax(0, 1fr)
                min(2rem, 12%)
                min(7.5rem, 30%);
            gap: min(0.875rem, 4%);
        }

        .btn-limpiar-filtros {
            padding: 0;
            font-size: 1.35rem;
            line-height: 1;
        }
        .table th {
        color: #373A72 !important;
        background-color: #F7F5FC;
        border-bottom: 1px solid #E4E0F0;
        --bs-table-bg: #F7F5FC;
        --bs-table-accent-bg: transparent;
        }
  
       * {
            scrollbar-color: #373A72 transparent;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div class="container-fluid p-4">

        <!-- Titulo -->
        <div class="row mb-4 text-center">
            <div class="col-12 text-center">
                <h1 class="text-indigo fw-bold">
                <i class="bi bi-file-earmark-text"></i>Postulaciones de alumnos
                </h1>
                 <h5>
                    Consultá el estado de las postulaciones y gestioná cada una de ellas.
                </h5>
            </div>
        </div>
        <!-- Mensaje error-->
        <div class="row mb-3">
            <div class="col-12">
                <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-2">
                </asp:Label>
            </div>
        </div>

          <!-- Filtro unificado -->
       <div class="card flex-shrink-0 p-4">
          <div class="row">
              <div class="col-6">
                  <asp:DropDownList runat="server" ID="ddlAlumnos" CssClass="form-control h-100 w-100">
                      <asp:ListItem Value="0">Todos los Alumnos</asp:ListItem>
                  </asp:DropDownList>
              </div>
              <div class="col-6 idioma-y-botones">
                  <asp:DropDownList runat="server" ID="ddlEstadosPostulacion" CssClass="form-control h-100 w-100" AppendDataBoundItems="true">
                      <asp:ListItem Value="0">Todos los Estados</asp:ListItem>
                  </asp:DropDownList>
                  <a href="ListadoPostulacionAlumnos.aspx" class="btn-lilaOscuro-invertido btn-limpiar-filtros d-flex align-items-center justify-content-center text-decoration-none" title="Limpiar filtros" aria-label="Limpiar filtros">X</a>
                  <asp:Button runat="server" ID="btnFiltrar" CssClass="btn-lilaOscuro h-100 w-100 px-0" Text="Filtrar" OnClick="btnFiltrar_Click" />
              </div>
          </div>
       </div>
        <!-- Listado de postulaciones -->
        <div class="card p-3 text-center ">
            <div class="table-responsive">
                <asp:GridView ID="dgvPostulaciones" runat="server" CssClass="table table-hover align-middle border-neutro mb-0" AutoGenerateColumns="False" GridLines="None" EmptyDataText="No hay postulaciones para mostrar." AllowPaging ="true" PageSize="10" OnPageIndexChanging="dgvPostulaciones_PageIndexChanging">
                    <HeaderStyle  CssClass="text-indigo border-bottom border-neutro" />
                    <Columns>
                        <asp:BoundField DataField="Alumno" HeaderText="Alumno" />
                        <asp:BoundField DataField="Documento" HeaderText="Docum" />
                        <asp:BoundField DataField="Institucion" HeaderText="Institución" />
                        <asp:BoundField  DataField="CicloLectivo" HeaderText="Ciclo lectivo" />
                        <asp:BoundField DataField="Estado" HeaderText="Estado" />
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnVerPostulacion" runat="server"  CommandArgument='<%# Eval("IdPostulacion") %>' CssClass="btn btn-sm btn-outline-lavanda rounded-pill px-3 fw-bold" OnClick="btnVerPostulacion_Click1">
                                    Gestionar postulación
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>
