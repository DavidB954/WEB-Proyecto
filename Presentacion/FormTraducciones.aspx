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

        <div class="abm-field">
            <asp:Label ID="lblPaginaFiltro" CssClass="label-base" runat="server" data-i18n="1">Pagina</asp:Label>
            <asp:DropDownList ID="ddlPaginaFiltro" CssClass="ddl-base" runat="server"
                AutoPostBack="true" OnSelectedIndexChanged="ddlPaginaFiltro_SelectedIndexChanged">
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
            <asp:Button ID="btnGuardarTraducciones" CssClass="btn-base btn-success" runat="server" Text="Guardar Traducciones" data-i18n="1" OnClick="btnGuardarTraducciones_Click" data-permiso="TRADUCCION_EDITAR" />
            <asp:Button ID="btnExportarXml" CssClass="btn-base" runat="server" Text="Exportar XML" data-i18n="1" OnClick="btnExportarXml_Click" data-permiso="TRADUCCION_EXPORTAR" />
        </div>

        <asp:Panel ID="pnlImportar" runat="server" CssClass="abm-field" data-permiso="TRADUCCION_IMPORTAR">
            <asp:Label ID="lblImportarXml" CssClass="label-base" runat="server" data-i18n="1">Importar traducciones desde un archivo XML</asp:Label>
            <span id="spanAyudaImportar" runat="server" class="ayuda-campo" data-i18n="1">Usa un archivo generado con Exportar XML para este mismo idioma. Actualiza las traducciones del archivo; las vacias o desconocidas se omiten.</span>
            <asp:FileUpload ID="fuImportarXml" runat="server" CssClass="input-base" accept=".xml" />
            <asp:Button ID="btnImportarXml" CssClass="btn-base btn-success" runat="server" Text="Importar XML" data-i18n="1" OnClick="btnImportarXml_Click" />
        </asp:Panel>
        <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form"></asp:Label>

    </div>

</asp:Content>
