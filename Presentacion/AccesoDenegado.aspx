<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="Presentacion.AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="acceso-denegado">
        <div class="acceso-denegado-icono">&#128274;</div>
        <h2 id="hAccesoDenegado" runat="server" data-i18n="1">Acceso Denegado</h2>
        <p id="pAccesoDenegado" runat="server" data-i18n="1">No tenes permiso para acceder a esta seccion con tu rol actual.</p>
        <a id="aVolverMenu" runat="server" class="btn-volver" data-i18n="1" href="Menu.aspx">Volver al Menu</a>
    </div>

</asp:Content>
