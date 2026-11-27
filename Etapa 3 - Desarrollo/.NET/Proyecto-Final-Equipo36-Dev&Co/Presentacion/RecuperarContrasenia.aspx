<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="RecuperarContrasenia.aspx.cs" Inherits="Presentacion.RecuperarContrasenia" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>InstiRed - Recuperar contraseña</title>
    <link rel="icon" href="Imagenes/InstiRed.ico" />
    <link href="~/EstilosInstiRed.css" rel="stylesheet" type="text/css" runat="server" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>

<body id="bg-indigo" class="acceso-cuenta">
    <form id="RecuperarContrasenia" runat="server">
        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <div class="w-100 p-4" style="max-width: 1100px;">
                <div class="row border rounded-5 p-3 bg-white shadow">

                    <%-- Imagen institucional --%>
                    <div class="col-md-6 rounded-4 d-flex justify-content-center align-items-center flex-column">

                        <div class="featured-image mb-3">
                            <picture>
                                <source media="(max-width: 767px)" srcset="Imagenes/Logo.png" />
                                <img src="Imagenes/Login.png" class="img-fluid" style="width: 350px;" alt="InstiRed" />
                            </picture>
                        </div>

                    </div>

                    <%-- Formulario de recuperación --%>
                    <div class="col-md-6" style="padding: 20px;">

                        <div class="row align-items-center">

                            <%-- Encabezado --%>
                            <div class="header-text mb-4">
                                <h3>Recuperar contraseña</h3>
                                <p>
                                    Ingresá tu correo electrónico y te enviaremos
                                    las credenciales para recuperar tu cuenta.
                                </p>
                                <p class="small text-muted">Cada solicitud reemplaza tu contraseña. Si solicitás varias, solo sirve la clave de la última solicitud.</p>
                            </div>

                            <%-- Correo electrónico --%>
                            <div class="input-group mb-3">
                                <asp:TextBox runat="server" ID="txtEmail" TextMode="Email" MaxLength="100" CssClass="form-control form-control-lg bg-light fs-6" placeholder="nombre@ejemplo.com"  aria-label="Correo electrónico" autocomplete="username" />
                            </div>

                            <%-- Mensaje de respuesta --%>
                            <div class="col-12">
                                <asp:Label ID="lblMensaje" runat="server" Visible="false" CssClass="d-block mb-4"></asp:Label>
                            </div>

                            <%-- Enviar email nueva clave --%>
                            <div class="input-group mb-3">
                                <asp:Button CssClass="btn-lilaOscuro w-100 fs-6" ID="btnEnviarCredenciales" runat="server" OnClick="btnEnviarCredenciales_Click" Text="Enviar credenciales"/>
                            </div>

                            <%-- Volver al login --%>
                            <div class="input-group mb-3">
                                <asp:HyperLink ID="lnkVolverLogin" runat="server" NavigateUrl="~/Login.aspx" CssClass="btn-lilaClaro w-100 fs-6 text-center text-decoration-none" Text="Volver al inicio de sesión" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
