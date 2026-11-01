<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Recetas.aspx.cs" Inherits="Presentacion.Recetas" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="grid-container misRecetas">
        <h2 id="hMisRecetas" runat="server" data-i18n="1">Mis Recetas</h2>
        <span id="spanAyudaRecetas" runat="server" class="ayuda-campo" data-i18n="1">Aca podes consultar las recetas que te emitieron tus medicos.</span>

        <asp:Panel ID="pnlRecetas" runat="server">
            <asp:RadioButtonList ID="rblVista" runat="server" data-i18n="1" CssClass="lista-opciones"
                RepeatLayout="Flow" RepeatDirection="Horizontal"
                AutoPostBack="true" OnSelectedIndexChanged="rblVista_SelectedIndexChanged">
                <asp:ListItem Value="Vigentes" Selected="True">Vigentes</asp:ListItem>
                <asp:ListItem Value="Todas">Todas</asp:ListItem>
            </asp:RadioButtonList>
            <span id="spanAyudaVigencia" runat="server" class="ayuda-campo"></span>

            <div class="grid-scroll">
                <asp:GridView ID="gvRecetas" runat="server" AutoGenerateColumns="False" CssClass="grid-crud" OnRowCommand="gvRecetas_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="FechaEmision" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="NombreMedico" HeaderText="Medico" />
                        <asp:BoundField DataField="EspecialidadesTexto" HeaderText="Especialidad" />
                        <asp:BoundField DataField="Medicamento" HeaderText="Medicamento" />
                        <asp:BoundField DataField="Observaciones" HeaderText="Indicaciones" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class='<%# "badge badge-" + Eval("EstadoCss") %>'><%# Eval("EstadoTexto") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Descargar">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDescargarPdf" runat="server" data-i18n="1" CommandName="DescargarPdf"
                                    CommandArgument='<%# Eval("IdReceta") %>' Text="PDF" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div id="divSinRecetas" runat="server" class="sin-datos" data-i18n="1">Todavia no tenes recetas emitidas.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>

    </div>

</asp:Content>
