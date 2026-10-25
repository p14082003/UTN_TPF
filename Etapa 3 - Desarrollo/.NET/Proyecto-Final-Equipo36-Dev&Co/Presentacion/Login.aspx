<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Presentacion.Login" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
<title>InstiRed - Inicio de sesión</title>
<link href="~/EstilosInstiRed.css" rel="stylesheet" type="text/css" runat="server" />
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />

</head>
<body id="bg-indigo">
    <form id="Login" runat="server">
        
        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <div class="w-100 p-4" style="max-width: 1100px;">
                <div class="row border rounded-5 p-3 bg-white shadow">
                      <%-- Imagen --%>
                      <div class="col-md-6 rounded-4 d-flex justify-content-center align-items-center flex-column">
                         <div class="featured-image mb-3">
                            <img src="Imagenes/Login.png" class="img-fluid" style="width: 350px;" alt="Imagen de login"/>
                         </div>  
                      </div>
                  
                     <%-- Formulario Login --%>
                     <div class="col-md-6" style="padding: 20px;">

                         <div class="row align-items-center">

                             <div class="header-text mb-4">
                                 <h3>¡Bienvenido!</h3>
                                 <p>Inicie sesión en su cuenta.</p>
                             </div>

                             <div class="input-group mb-3">
                                 <asp:TextBox runat="server" ID="txtUsuario" CssClass="form-control form-control-lg bg-light fs-6" placeholder="Usuario"/>
                             </div>
                  
                             <div class="input-group mb-3">
                                 <asp:TextBox runat="server" ID="txtPassword" CssClass="form-control form-control-lg bg-light fs-6" placeholder="Contraseña" TextMode="Password"/>
                             </div>

                             <div class="input-group mb-3">
                                 <asp:Button CssClass="btn-lilaOscuro w-100 fs-6" ID="btnLogin" runat="server" Text="Ingresar"/>
                             </div>

                             <div class="input-group mb-3">
                                 <asp:Button CssClass="btn-lilaClaro w-100 fs-6" ID="btnRegistrarse" OnClick="btnRegistrarse_Click" runat="server" Text="Registrarse" CausesValidation="False" />
                             </div>

                             <div class="text-end mb-3">
                                <asp:HyperLink ID="lnkOlvidePassword" runat="server" CssClass="link-lilaClaro" Text="¿Olvidaste tu contraseña?" />
                             </div>
                         </div>
                     </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
