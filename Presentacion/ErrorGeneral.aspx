<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorGeneral.aspx.cs" Inherits="Presentacion.ErrorGeneral" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Error</title>

    <link rel="preconnect" href="https://fonts.googleapis.com" />
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
    <link href="https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600;700&family=Source+Serif+4:opsz,wght@8..60,500;8..60,600;8..60,700&display=swap" rel="stylesheet" />
    <link href="Styles/StyleSheet.css" rel="stylesheet" />
</head>

<body>
    <form id="form1" runat="server">
        <div class="loginBody">
            <div class="acceso-denegado">
                <div class="acceso-denegado-icono">&#9888;</div>
                <h2>Ocurrió un error inesperado</h2>
                <p>El sistema no pudo completar la operación. Si el problema persiste, contactá al administrador.</p>
                <a class="btn-volver" href="Login.aspx">Volver al inicio</a>
            </div>
        </div>
    </form>
</body>
</html>
