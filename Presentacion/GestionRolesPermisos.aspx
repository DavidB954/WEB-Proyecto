<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="GestionRolesPermisos.aspx.cs" Inherits="Presentacion.GestionRolesPermisos" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">
        <h2 id="hGestionRoles" runat="server" data-i18n="1">Roles</h2>

        <div class="abm-grid">
            <div class="abm-field">
                <asp:Label ID="lblNombreRol" CssClass="label-base" runat="server" data-i18n="1">Nombre</asp:Label>
                <asp:TextBox ID="txtNombreRol" CssClass="input-base" runat="server" MaxLength="30"></asp:TextBox>
            </div>
        </div>

        <div class="abm-actions">
            <asp:HiddenField ID="hiddenIdRol" runat="server" />
            <asp:Button ID="btnGuardarRol" CssClass="btn-base btn-success" runat="server" Text="Guardar" data-i18n="1" OnClick="btnGuardarRol_Click" data-permiso="ROL_ALTA" />
            <asp:Button ID="btnModificarRol" CssClass="btn-base btn-primary" runat="server" Text="Modificar" data-i18n="1" OnClick="btnModificarRol_Click" data-permiso="ROL_MODIFICACION" />
            <asp:Button ID="btnEliminarRol" CssClass="btn-base btn-danger" runat="server" Text="Eliminar" data-i18n="1" OnClick="btnEliminarRol_Click" data-permiso="ROL_BAJA" />
            <asp:Button ID="btnLimpiarRol" CssClass="btn-base" runat="server" Text="Limpiar" data-i18n="1" OnClick="btnLimpiarRol_Click" CausesValidation="false" />
        </div>
        <asp:Label ID="lblMensajeRol" runat="server" CssClass="msg-form"></asp:Label>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvRoles" runat="server" AutoGenerateColumns="false"
                DataKeyNames="IdRol" OnSelectedIndexChanged="gvRoles_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Componer" />
                    <asp:BoundField DataField="IdRol" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="form-container">
        <h2 id="hComposicionRol" runat="server" data-i18n="1">Composicion del rol seleccionado</h2>

        <asp:Panel ID="pnlSinRolSeleccionado" runat="server">
            <p id="pSinRol" runat="server" data-i18n="1">Selecciona un rol de la grilla de arriba (columna "Componer") para agregarle permisos y/o sub-roles.</p>
        </asp:Panel>

        <asp:Panel ID="pnlComposicion" runat="server" Visible="false">

            <div class="abm-field">
                <asp:Label ID="lblRolComponiendo" CssClass="label-base" runat="server" data-i18n="1">Rol</asp:Label>
                <asp:TextBox ID="txtRolComponiendo" CssClass="input-base" runat="server" ReadOnly="true"></asp:TextBox>
            </div>

            <h3 id="hPermisosDelRol" runat="server" data-i18n="1">Permisos del rol</h3>
            <div class="abm-grid">
                <div class="abm-field">
                    <asp:Label ID="lblPaginaAAgregar" CssClass="label-base" runat="server" data-i18n="1">Pagina</asp:Label>
                    <asp:DropDownList ID="ddlPaginaAAgregar" CssClass="ddl-base" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPaginaAAgregar_SelectedIndexChanged" CausesValidation="false"></asp:DropDownList>
                </div>
                <div class="abm-field">
                    <asp:Label ID="lblPermisoAAgregar" CssClass="label-base" runat="server" data-i18n="1">Permiso</asp:Label>
                    <asp:DropDownList ID="ddlPermisoAAgregar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                </div>
            </div>
            <div class="abm-actions">
                <asp:Button ID="btnAgregarPermisoARol" CssClass="btn-base btn-success" runat="server" Text="Agregar Permiso" data-i18n="1" OnClick="btnAgregarPermisoARol_Click" CausesValidation="false" data-permiso="ROL_COMPONER" />
            </div>
            <div class="grid-scroll">
                <asp:GridView CssClass="grid-crud" ID="gvPermisosDelRol" runat="server" AutoGenerateColumns="false"
                    DataKeyNames="IdPermiso" OnRowCommand="gvPermisosDelRol_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Pagina" HeaderText="Pagina" />
                        <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                        <asp:BoundField DataField="Tipo" HeaderText="Tipo" />
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkQuitar" runat="server" data-i18n="1" CommandName="Quitar" CommandArgument='<%# Eval("IdPermiso") %>' data-permiso="ROL_COMPONER" Text="Quitar" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <h3 id="hSubRoles" runat="server" data-i18n="1">Sub-roles (roles que este rol contiene)</h3>
            <div class="abm-field">
                <asp:DropDownList ID="ddlSubRolAAgregar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                <asp:Button ID="btnAgregarSubRol" CssClass="btn-base btn-success" runat="server" Text="Agregar Sub-rol" data-i18n="1" OnClick="btnAgregarSubRol_Click" CausesValidation="false" data-permiso="ROL_COMPONER" />
            </div>
            <div class="grid-scroll">
                <asp:GridView CssClass="grid-crud" ID="gvSubRolesDelRol" runat="server" AutoGenerateColumns="false"
                    DataKeyNames="IdRol" OnRowCommand="gvSubRolesDelRol_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkQuitar" runat="server" data-i18n="1" CommandName="Quitar" CommandArgument='<%# Eval("IdRol") %>' data-permiso="ROL_COMPONER" Text="Quitar" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <asp:Label ID="lblMensajeComposicion" runat="server" CssClass="msg-form"></asp:Label>

            <h3 id="hArbolRol" runat="server" data-i18n="1">Arbol resultante (permisos efectivos)</h3>
            <asp:TreeView ID="trvArbolRol" runat="server"></asp:TreeView>

        </asp:Panel>
    </div>

</asp:Content>
