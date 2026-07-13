<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Recetas.aspx.cs" Inherits="Presentacion.Recetas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="grid-container misRecetas">
        <h2>Mis Recetas</h2>
        <span class="ayuda-campo">Aca podes consultar las recetas que te emitieron tus medicos.</span>

        <asp:GridView ID="gvRecetas" runat="server" AutoGenerateColumns="False" CssClass="grid-crud">
            <Columns>
                <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                <asp:BoundField DataField="Medico" HeaderText="Medico" />
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="Medicamento" HeaderText="Medicamento" />
                <asp:BoundField DataField="Indicaciones" HeaderText="Indicaciones" />
                <asp:TemplateField HeaderText="Estado">
                    <ItemTemplate>
                        <span class='<%# "badge badge-" + Eval("Estado").ToString().ToLower() %>'><%# Eval("Estado") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="sin-datos">Todavia no tenés recetas emitidas.</div>
            </EmptyDataTemplate>
        </asp:GridView>

    </div>

</asp:Content>
