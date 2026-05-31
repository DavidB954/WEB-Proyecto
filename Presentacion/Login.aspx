<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Presentacion.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login</title>
    <link href="Styles/StyleSheet.css" rel="stylesheet" />
</head>

<body> 
    <form id="form1" runat="server">
        <div class="loginBody" runat="server">
            <div class="form-container">
            <div class="loginContainer" runat="server" >
                <h2>Iniciar Sesión</h2>
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
                    Text="Iniciar Sesión" 
                    OnClick="btnLogin_Click" />
                <br /><br />
                <asp:Label ID="lblMensaje" runat="server"></asp:Label>
                
            </div>
            </div>
       </div>
    </form>
         
</body>
</html>