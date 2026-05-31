<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Recetas.aspx.cs" Inherits="Presentacion.Recetas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="grid-container misRecetas">
        <h2>Mis Recetas</h2>
        <asp:GridView ID="gvRecetas" runat="server" AutoGenerateColumns="False" CssClass="grid-crud">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                <asp:BoundField DataField="Medico" HeaderText="Médico" />
                <asp:BoundField DataField="Medicamento" HeaderText="Medicamento" />
                <asp:BoundField DataField="Indicaciones" HeaderText="Indicaciones" />

            </Columns>

        </asp:GridView>

    </div>

</asp:Content>
