<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="FormIdiomas.aspx.cs" Inherits="Presentacion.FormIdiomas" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">

        <h2 id="hGestionIdiomas" runat="server" data-i18n="1">Gestion de Idiomas</h2>

        <div class="abm-grid">

            <div class="abm-field">
                <asp:Label ID="lblCodigo" CssClass="label-base" runat="server" data-i18n="1">Codigo</asp:Label>
                <asp:TextBox ID="txtCodigo" CssClass="input-base" runat="server" MaxLength="10"></asp:TextBox>
            </div>

            <div class="abm-field">
                <asp:Label ID="lblNombreIdioma" CssClass="label-base" runat="server" data-i18n="1">Nombre</asp:Label>
                <asp:TextBox ID="txtNombre" CssClass="input-base" runat="server" MaxLength="50"></asp:TextBox>
            </div>

        </div>

        <div class="abm-actions">
            <asp:HiddenField ID="hiddenIdIdioma" runat="server" />
            <asp:Button ID="btnAgregar" CssClass="btn-base btn-success" runat="server" Text="Agregar Idioma" data-i18n="1" OnClick="btnAgregar_Click" />
            <asp:Button ID="btnEliminar" CssClass="btn-base btn-danger" runat="server" Text="Eliminar Idioma Seleccionado" data-i18n="1" OnClick="btnEliminar_Click" />
        </div>
        <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form"></asp:Label>

    </div>

    <div class="form-container">
        <h2 id="hIdiomasCargados" runat="server" data-i18n="1">Idiomas cargados</h2>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvIdiomas" runat="server" AutoGenerateColumns="false"
                OnSelectedIndexChanged="gvIdiomas_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                    <asp:BoundField DataField="IdIdioma" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Codigo" HeaderText="Codigo" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                    <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
                    <asp:CheckBoxField DataField="PorDefecto" HeaderText="Predeterminado" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

</asp:Content>
