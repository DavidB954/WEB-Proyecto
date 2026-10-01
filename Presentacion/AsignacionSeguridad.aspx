<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AsignacionSeguridad.aspx.cs" Inherits="Presentacion.AsignacionSeguridad" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">
        <h2 id="hAsignacionSeguridad" runat="server" data-i18n="1">Asignacion de Seguridad</h2>
        <p data-i18n="1">Elegi un usuario de la lista para asignarle o quitarle roles y permisos.</p>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvUsuarios" runat="server" AutoGenerateColumns="false"
                DataKeyNames="IdUsuario" OnSelectedIndexChanged="gvUsuarios_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                    <asp:BoundField DataField="IdUsuario" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                    <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
                    <asp:BoundField DataField="NombreRol" HeaderText="Roles" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="form-container">

        <asp:Panel ID="pnlSinUsuarioSeleccionado" runat="server">
            <p data-i18n="1">Selecciona un usuario de la grilla de arriba para ver y editar su seguridad.</p>
        </asp:Panel>

        <asp:Panel ID="pnlAsignacion" runat="server" Visible="false">

            <div class="abm-field">
                <asp:Label ID="lblUsuarioSeleccionado" CssClass="label-base" runat="server" data-i18n="1">Usuario</asp:Label>
                <asp:TextBox ID="txtUsuarioSeleccionado" CssClass="input-base" runat="server" ReadOnly="true"></asp:TextBox>
            </div>

            <h3 data-i18n="1">Roles asignados</h3>
            <div class="abm-field">
                <asp:DropDownList ID="ddlRolAAsignar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                <asp:Button ID="btnAsignarRol" CssClass="btn-base btn-success" runat="server" Text="Asignar Rol" data-i18n="1" OnClick="btnAsignarRol_Click" CausesValidation="false" />
            </div>
            <div class="grid-scroll">
                <asp:GridView CssClass="grid-crud" ID="gvRolesDelUsuario" runat="server" AutoGenerateColumns="false"
                    DataKeyNames="IdRol" OnRowCommand="gvRolesDelUsuario_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="Rol" />
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CommandName="Quitar" CommandArgument='<%# Eval("IdRol") %>' Text="Quitar" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <h3 data-i18n="1">Permisos directos (sin pasar por un rol)</h3>
            <div class="abm-field">
                <asp:DropDownList ID="ddlPermisoAAsignar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                <asp:Button ID="btnAsignarPermiso" CssClass="btn-base btn-success" runat="server" Text="Asignar Permiso" data-i18n="1" OnClick="btnAsignarPermiso_Click" CausesValidation="false" />
            </div>
            <div class="grid-scroll">
                <asp:GridView CssClass="grid-crud" ID="gvPermisosDelUsuario" runat="server" AutoGenerateColumns="false"
                    DataKeyNames="IdPermiso" OnRowCommand="gvPermisosDelUsuario_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="Permiso" />
                        <asp:BoundField DataField="Tipo" HeaderText="Tipo" />
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CommandName="Quitar" CommandArgument='<%# Eval("IdPermiso") %>' Text="Quitar" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <asp:Label ID="lblMensajeAsignacion" runat="server" CssClass="msg-form"></asp:Label>

            <h3 data-i18n="1">Arbol de seguridad resultante</h3>
            <asp:TreeView ID="trvArbolUsuario" runat="server"></asp:TreeView>

        </asp:Panel>
    </div>

</asp:Content>
