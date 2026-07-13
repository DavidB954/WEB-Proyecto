<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="Presentacion.AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="acceso-denegado">
        <div class="acceso-denegado-icono">&#128274;</div>
        <h2>Acceso Denegado</h2>
        <p>No tenes permiso para acceder a esta seccion con tu rol actual.</p>
        <a class="btn-volver" href="Menu.aspx">Volver al Menu</a>
    </div>

</asp:Content>
