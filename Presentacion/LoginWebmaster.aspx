<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LoginWebmaster.aspx.cs" Inherits="Presentacion.LoginWebmaster" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login Webmaster</title>
    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600;700&family=Source+Serif+4:opsz,wght@8..60,500;8..60,600;8..60,700&display=swap" rel="stylesheet" />
    <link href="Styles/StyleSheet.css" rel="stylesheet" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="loginBody" runat="server">
            <div class="form-container">
            <div class="loginContainer" runat="server" >
                <h2>Se detecto un problema de integridad en la base de datos</h2>
                <p>Solo el Webmaster puede ingresar mientras la base esté comprometida.</p>
                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    Placeholder="Email">
                </asp:TextBox>
                <br /><br />
                <asp:TextBox
                    ID="txtPassword"
                    runat="server"
                    PlaceHolder="Contraseña"
                    TextMode="Password">
                </asp:TextBox>
                <br /><br />
                <asp:Button
                    Class="btn"
                    ID="btnLogin"
                    runat="server"
                    Text="Iniciar Sesion"
                    OnClick="btnLogin_Click" />
                <br /><br />
                <asp:Label ID="lblMensaje" runat="server"></asp:Label>

            </div>
            </div>
       </div>
    </form>

</body>
</html>
