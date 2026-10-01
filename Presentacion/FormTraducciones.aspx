<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="FormTraducciones.aspx.cs" Inherits="Presentacion.FormTraducciones" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">

        <h2 id="hTraducciones" runat="server" data-i18n="1">Traducciones</h2>

        <div class="abm-field">
            <asp:Label ID="lblIdiomaAEditar" CssClass="label-base" runat="server" data-i18n="1">Idioma a editar</asp:Label>
            <asp:DropDownList ID="ddlIdiomaEditar" CssClass="ddl-base" runat="server"
                AutoPostBack="true" OnSelectedIndexChanged="ddlIdiomaEditar_SelectedIndexChanged">
            </asp:DropDownList>
        </div>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvTraducciones" runat="server" AutoGenerateColumns="false"
                DataKeyNames="IdClave">
                <Columns>
                    <asp:BoundField DataField="Pagina" HeaderText="Pagina / Control" ReadOnly="True" />
                    <asp:BoundField DataField="ControlId" HeaderText="Elemento" ReadOnly="True" />
                    <asp:BoundField DataField="TextoBase" HeaderText="Texto original (Español)" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Traduccion">
                        <ItemTemplate>
                            <asp:TextBox ID="txtTexto" CssClass="input-base" runat="server" Text='<%# Eval("Texto") %>'></asp:TextBox>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="abm-actions">
            <asp:Button ID="btnGuardarTraducciones" CssClass="btn-base btn-success" runat="server" Text="Guardar Traducciones" data-i18n="1" OnClick="btnGuardarTraducciones_Click" />
        </div>
        <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form"></asp:Label>

    </div>

</asp:Content>
