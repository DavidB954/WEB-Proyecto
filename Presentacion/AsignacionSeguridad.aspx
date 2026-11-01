<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AsignacionSeguridad.aspx.cs" Inherits="Presentacion.AsignacionSeguridad" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">
        <h2 id="hAsignacionSeguridad" runat="server" data-i18n="1">Asignacion de Seguridad</h2>
        <p id="pAyudaAsignacion" runat="server" data-i18n="1">Elegi un usuario de la lista para asignarle o quitarle roles y permisos.</p>

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
            <p id="pSinUsuario" runat="server" data-i18n="1">Selecciona un usuario de la grilla de arriba para ver y editar su seguridad.</p>
        </asp:Panel>

        <asp:Panel ID="pnlAsignacion" runat="server" Visible="false">

            <div class="abm-field">
                <asp:Label ID="lblUsuarioSeleccionado" CssClass="label-base" runat="server" data-i18n="1">Usuario</asp:Label>
                <asp:TextBox ID="txtUsuarioSeleccionado" CssClass="input-base" runat="server" ReadOnly="true"></asp:TextBox>
            </div>

            <asp:Label ID="lblSoloLectura" runat="server" CssClass="msg-form"></asp:Label>

            <h3 id="hRolesAsignados" runat="server" data-i18n="1">Roles asignados</h3>
            <asp:Panel ID="pnlAsignarRol" runat="server" CssClass="abm-field">
                <asp:DropDownList ID="ddlRolAAsignar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                <asp:Button ID="btnAsignarRol" CssClass="btn-base btn-success" runat="server" Text="Asignar Rol" data-i18n="1" OnClick="btnAsignarRol_Click" CausesValidation="false" data-permiso="ASIGNAR_ROL_USUARIO" />
            </asp:Panel>
            <div class="grid-scroll">
                <asp:GridView CssClass="grid-crud" ID="gvRolesDelUsuario" runat="server" AutoGenerateColumns="false"
                    DataKeyNames="IdRol" OnRowCommand="gvRolesDelUsuario_RowCommand">
                    <Columns>
                        <asp:BoundField DataField="Nombre" HeaderText="Rol" />
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkQuitar" runat="server" data-i18n="1" CommandName="Quitar" CommandArgument='<%# Eval("IdRol") %>' data-permiso="ASIGNAR_ROL_USUARIO" Text="Quitar" CausesValidation="false" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <asp:Panel ID="pnlPermisosDirectos" runat="server" data-permiso="ASIGNAR_PERMISO_USUARIO">
                <h3 id="hPermisosDirectos" runat="server" data-i18n="1">Permisos directos (sin pasar por un rol)</h3>
                <p id="pAyudaPermisosDirectos" runat="server" class="ayuda-campo" data-i18n="1">Le dan solo a este usuario el acceso a una pagina completa o a un boton puntual, sin cambiarle el rol. Se suman a lo que recibe por sus roles.</p>

                <asp:Panel ID="pnlAgregarPermiso" runat="server">
                <div class="abm-grid">
                    <div class="abm-field">
                        <asp:Label ID="lblPaginaAAsignar" CssClass="label-base" runat="server" data-i18n="1">Pagina</asp:Label>
                        <asp:DropDownList ID="ddlPaginaAAsignar" CssClass="ddl-base" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlPaginaAAsignar_SelectedIndexChanged" CausesValidation="false"></asp:DropDownList>
                    </div>
                    <div class="abm-field">
                        <asp:Label ID="lblPermisoAAsignar" CssClass="label-base" runat="server" data-i18n="1">Permiso</asp:Label>
                        <asp:DropDownList ID="ddlPermisoAAsignar" CssClass="ddl-base" runat="server"></asp:DropDownList>
                    </div>
                </div>
                <div class="abm-actions">
                    <asp:Button ID="btnAsignarPermiso" CssClass="btn-base btn-success" runat="server" Text="Asignar Permiso" data-i18n="1" OnClick="btnAsignarPermiso_Click" CausesValidation="false" data-permiso="ASIGNAR_PERMISO_USUARIO" />
                </div>
                </asp:Panel>

                <div class="grid-scroll">
                    <asp:GridView CssClass="grid-crud" ID="gvPermisosDelUsuario" runat="server" AutoGenerateColumns="false"
                        DataKeyNames="IdPermiso" OnRowCommand="gvPermisosDelUsuario_RowCommand">
                        <Columns>
                            <asp:TemplateField HeaderText="Permiso">
                                <ItemTemplate>
                                    <%#: Eval("Nombre") %>
                                    <span class="ayuda-permiso"><%#: Eval("Detalle") %></span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Tipo" HeaderText="Tipo" />
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkQuitar" runat="server" data-i18n="1" CommandName="Quitar" CommandArgument='<%# Eval("IdPermiso") %>' data-permiso="ASIGNAR_PERMISO_USUARIO" Text="Quitar" CausesValidation="false" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <asp:Label ID="lblMensajeAsignacion" runat="server" CssClass="msg-form"></asp:Label>

            <h3 id="hArbolSeguridad" runat="server" data-i18n="1">Arbol de seguridad resultante</h3>
            <asp:TreeView ID="trvArbolUsuario" runat="server" SkipLinkText=""></asp:TreeView>

        </asp:Panel>
    </div>

</asp:Content>
