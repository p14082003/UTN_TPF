<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Presentacion.Login" %>

<!DOCTYPE html>

<html lang="en">
<head runat="server">
<meta charset="utf-8" />
<meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>InstiRed - Inicio de sesión</title>
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css" rel="stylesheet" />
<style>
        body {
            background-color: #F7F4FC;
        }
     
        .btn-instired {
            background-color: #373A72;
            color: #FFFFFF;
            border: 1px solid #373A72;
            border-radius: 15px;
            padding: 14px;
            font-weight: 600;
            transition: background-color 0.2s ease;
        }

        .btn-instired:hover {
            background-color: #2D305F;
            color: #FFFFFF;
        }


        .btn-register {
            background-color: #F7F5FC;
            color: #373A72;
            border: 1px solid #E4E0F0;
            border-radius: 15px;
            padding: 14px;
            font-weight: 500;
            transition: background-color 0.2s ease;
        }

        .btn-register:hover {
            background-color: #E4E0F0;
            color: #373A72;
        }

        .login-container {
            width: 100%;
             max-width: 1100px;
            padding: 32px;
        }

        .link-recuperar {
            color: #8D91D5;
            font-size: 14px;
            text-decoration: none;
        }

        .link-recuperar:hover {
            color: #373A72;
            text-decoration: underline;
        }
</style>

</head>
<body>
    <form id="Login" runat="server">
        
        <div class="container d-flex justify-content-center align-items-center min-vh-100">
            <div class="login-container">
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
                                 <asp:Button CssClass="btn-instired w-100 fs-6" ID="btnLogin" runat="server" Text="Ingresar"/>
                             </div>

                             <div class="input-group mb-3">
                                 <asp:Button CssClass="btn-register w-100 fs-6" ID="btnRegistrarse" runat="server" Text="Registrarse" CausesValidation="False" />
                             </div>

                             <div class="text-end mb-3">
                                <asp:HyperLink ID="lnkOlvidePassword" runat="server" CssClass="link-recuperar" Text="¿Olvidaste tu contraseña?" />
                             </div>
                         </div>
                     </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
