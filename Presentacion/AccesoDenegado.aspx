<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AccesoDenegado.aspx.cs" Inherits="Presentacion.AccesoDenegado" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="form-container form-cred">
        <h2 class="form-title">Acceso Denegado</h2>
        <p>No tenés permiso para acceder a esta sección con tu rol actual.</p>
        <a href="Menu.aspx">Volver al Menú</a>
    </div>

</asp:Content>
