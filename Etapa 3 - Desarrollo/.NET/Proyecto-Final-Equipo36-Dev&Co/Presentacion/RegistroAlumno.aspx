<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.Master" AutoEventWireup="true" CodeBehind="RegistroAlumno.aspx.cs" Inherits="Presentacion.RegistroAlumno" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="container-fluid bg-fondo min-vh-100 py-3">
        <div class="row justify-content-center">
            <div class="col-12 md:col-9 lg:col-6">

                <!--Titulo-->
                <div class="text-center mb-4">
                    <img src="Imagenes/Logo.png" alt="InstiRed" class="w-25 img-fluid mb-1" />
                    <h2 class="h3 fw-bold text-indigo mb-2">Registrar alumno</h2>
                    <p>Completá los datos del alumno que querés postular a una institución</p>
                </div>

                <div class="card mb-4">
                    <div class="card-body p-3 p-md-4">

                        <!--Mensaje de resultado-->
                        <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-3"></asp:Label>

                        <!--Autogestor-->
                        <div class="border border-neutro rounded-3 p-2 mb-3">
                            <div class="form-check mb-0">
                                <asp:CheckBox ID="chkAutogestor" runat="server" AutoPostBack="true" OnCheckedChanged="chkAutogestor_CheckedChanged" CssClass="form-check-input" />
                                <asp:Label ID="lblAutogestor" runat="server" AssociatedControlID="chkAutogestor" Text="El alumno soy yo (mayor de edad)" CssClass="form-check-label ms-2"></asp:Label>
                            </div>
                        </div>

                        <!--Rol/parentesco (solo si el alumno es otra persona)-->
                        <asp:Panel ID="pnlRol" runat="server">
                            <asp:Label ID="lblParentesco" runat="server" Text="Tu vínculo con el alumno:" CssClass="form-label fw-bold"></asp:Label>
                            <div class="row g-2 mb-3">
                                <div class="col-12 md:col-6">
                                    <div class="border border-neutro rounded-3 p-2">
                                        <div class="form-check mb-0">
                                            <asp:RadioButton ID="rbPadre" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                            <asp:Label ID="lblPadre" runat="server" AssociatedControlID="rbPadre" Text="Padre" CssClass="form-check-label ms-2"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-12 md:col-6">
                                    <div class="border border-neutro rounded-3 p-2">
                                        <div class="form-check mb-0">
                                            <asp:RadioButton ID="rbMadre" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                            <asp:Label ID="lblMadre" runat="server" AssociatedControlID="rbMadre" Text="Madre" CssClass="form-check-label ms-2"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-12 md:col-6">
                                    <div class="border border-neutro rounded-3 p-2">
                                        <div class="form-check mb-0">
                                            <asp:RadioButton ID="rbTutor" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                            <asp:Label ID="lblTutor" runat="server" AssociatedControlID="rbTutor" Text="Tutor legal" CssClass="form-check-label ms-2"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                                <div class="col-12 md:col-6">
                                    <div class="border border-neutro rounded-3 p-2">
                                        <div class="form-check mb-0">
                                            <asp:RadioButton ID="rbFamiliar" runat="server" GroupName="Parentesco" CssClass="form-check-input" />
                                            <asp:Label ID="lblFamiliar" runat="server" AssociatedControlID="rbFamiliar" Text="Familiar a cargo" CssClass="form-check-label ms-2"></asp:Label>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>

                        <!--Datos del alumno-->
                        <div class="row g-2 mb-3">
                            <div class="col-12">
                                <asp:Label ID="lblDatosAlumno" runat="server" Text="Datos del alumno:" CssClass="form-label fw-bold"></asp:Label>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblNombre" runat="server" Text="Nombre" AssociatedControlID="txtNombre" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtNombre" runat="server" placeholder="Nombre" MaxLength="50" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblApellido" runat="server" Text="Apellido" AssociatedControlID="txtApellido" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtApellido" runat="server" placeholder="Apellido" MaxLength="50" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblDocumento" runat="server" Text="Documento" AssociatedControlID="txtDocumento" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtDocumento" runat="server" placeholder="35887625" MaxLength="8" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12 md:col-6">
                                <asp:Label ID="lblFechaNacimiento" runat="server" Text="Fecha de nacimiento" AssociatedControlID="txtFechaNacimiento" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtFechaNacimiento" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>
                            </div>
                            <div class="col-12">
                                <asp:Label ID="lblDomicilio" runat="server" Text="Domicilio" AssociatedControlID="txtDomicilio" CssClass="form-label text-indigo"></asp:Label>
                                <asp:TextBox ID="txtDomicilio" runat="server" placeholder="Av. Rivadavia 123, Mza" MaxLength="100" CssClass="form-control"></asp:TextBox>
                            </div>
                        </div>

                        <!--Botones-->
                        <div class="row g-2">
                            <div class="col-12">
                                <asp:Button ID="btnRegistrar" runat="server" Text="Registrar alumno" OnClick="btnRegistrar_Click" CssClass="btn btn-lilaOscuro w-100 py-2 mb-2" />
                                <asp:Button ID="btnCancelar" runat="server" Text="Cancelar" OnClick="btnCancelar_Click" CausesValidation="false" CssClass="btn btn-lilaClaro w-100 py-2 mb-2" />
                            </div>
                        </div>

                    </div>
                </div>
            </div>
        </div>
    </div>

</asp:Content>
