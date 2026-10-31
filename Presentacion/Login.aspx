<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Presentacion.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login</title>
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
                <div class="selector-idioma-login">
                    <asp:DropDownList ID="ddlIdioma" runat="server" CssClass="ddl-idioma"
                        DataTextField="Nombre" DataValueField="IdIdioma"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlIdioma_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>
                <h2 id="hBienvenido" runat="server" data-i18n="1">Bienvenido</h2>
                <p id="pIniciaSesion" runat="server" data-i18n="1">Inicia sesion para continuar</p>
                <asp:TextBox
                    ID="txtEmail"
                    runat="server"
                    Placeholder="Email" data-i18n="1">                   
                </asp:TextBox>
                <br /><br />
                <asp:TextBox                     
                    ID="txtPassword" 
                    runat="server" 
                    PlaceHolder="Contraseña" data-i18n="1"
                    TextMode="Password">
                </asp:TextBox>
                <br /><br />
                <asp:Button 
                    Class="btn"
                    ID="btnLogin"                 
                    runat="server" 
                    Text="Iniciar Sesion" data-i18n="1" 
                    OnClick="btnLogin_Click" />
                <br /><br />
                <asp:Label ID="lblMensaje" runat="server"></asp:Label>
                
            </div>
            </div>
       </div>
    </form>
         
</body>
</html>