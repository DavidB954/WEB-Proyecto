<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="CRUD_Usuarios.aspx.cs" Inherits="Presentacion.WebForm1" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>

<asp:Content
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <div class="form-container">

        <h2>Gestion de Usuarios</h2>

        <div class="abm-grid">

            <div class="abm-field">
                <asp:Label CssClass="label-base" runat="server">Nombre</asp:Label>
                <asp:TextBox ID="txtNombre" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:RegularExpressionValidator ID="valNombre" runat="server"
                    ControlToValidate="txtNombre"
                    ErrorMessage="El nombre debe contener solo letras y espacios."
                    ValidationExpression="^[a-zA-Z\s]+$" ForeColor="Red" Display="Dynamic">
                </asp:RegularExpressionValidator>
            </div>

            <div class="abm-field">
                <asp:Label CssClass="label-base" runat="server">Apellido</asp:Label>
                <asp:TextBox ID="txtApellido" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:RegularExpressionValidator ID="valApellido" runat="server"
                    ControlToValidate="txtApellido"
                    ErrorMessage="El apellido debe contener solo letras y espacios."
                    ValidationExpression="^[a-zA-Z\s]+$" ForeColor="Red" Display="Dynamic">
                </asp:RegularExpressionValidator>
            </div>

            <div class="abm-field">
                <asp:Label CssClass="label-base" runat="server">DNI</asp:Label>
                <asp:TextBox ID="txtDNI" CssClass="input-base" runat="server" autocomplete="off"></asp:TextBox>
                <asp:RegularExpressionValidator ID="valDNI" runat="server"
                    ControlToValidate="txtDNI"
                    ErrorMessage="El DNI debe contener solo numeros sin punto."
                    ValidationExpression="^\s*\d{7,8}\s*$" ForeColor="Red" Display="Dynamic">
                </asp:RegularExpressionValidator>
            </div>
            <div class="abm-field">
                <asp:Label CssClass="label-base" runat="server">Email</asp:Label>
                <asp:TextBox ID="txtEmail" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:RegularExpressionValidator ID="valEmail" runat="server"
                    ControlToValidate="txtEmail"
                    ErrorMessage="Formato de correo invalido. Ejemplo: usuario@gmail.com"
                    ValidationExpression="^[^@\s]+@[^@\s]+\.[^@\s]+$" ForeColor="Red" Display="Dynamic">
                </asp:RegularExpressionValidator>
            </div>
            <div class="abm-field">
                <asp:Label CssClass="label-base" runat="server">Contrasena</asp:Label>
                <asp:TextBox ID="txtPassword" CssClass="input-base" runat="server" TextMode="Password"></asp:TextBox>
                <asp:RegularExpressionValidator ID="valPassword" runat="server"
                    ControlToValidate="txtPassword"
                    ErrorMessage="La contraseña debe tener al menos 8 caracteres, una mayuscula, una minuscula, un numero y un simbolo."
                    ValidationExpression="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$" ForeColor="Red" Display="Dynamic">
                </asp:RegularExpressionValidator>
                <asp:CheckBox ID="chkResetearPassword" runat="server" Text="Restablecer contrasena" CssClass="chk-reset" />
            </div>
            <div class="abm-field abm-field-activo">
                <asp:Label CssClass="label-base" runat="server">Estado</asp:Label>
                <div class="fila-estado">
                    <asp:CheckBox ID="chkActivo" runat="server" Text="Usuario activo" CssClass="chk-activo" Checked="true" />
                    <asp:CheckBox ID="chkResetearIntentos" runat="server" Text="Resetear intentos fallidos" CssClass="chk-reset" />
                </div>
            </div>
        </div>

        <div class="abm-actions">
            <asp:HiddenField ID="hiddenIdUsuario" runat="server" />
            <asp:Button ID="btnGuardar" CssClass="btn-base btn-success" runat="server" Text="Guardar" OnClick="btnGuardar_Click" />
            <asp:Button ID="btnModificar" CssClass="btn-base btn-primary" runat="server" Text="Modificar" OnClick="btnModificar_Click" />
            <asp:Button ID="btnEliminar" CssClass="btn-base btn-danger" runat="server" Text="Eliminar" OnClick="btnEliminar_Click" />
        </div>
        <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form"></asp:Label>

    </div>

    <div class="form-container">
        <h2>Usuarios registrados</h2>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvUsuarios" runat="server" AutoGenerateColumns="false"
                OnSelectedIndexChanged="gvUsuarios_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                    <asp:BoundField DataField="IdUsuario" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                    <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
                    <asp:BoundField DataField="DNI" HeaderText="DNI" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="NombreRol" HeaderText="Rol" />
                    <asp:BoundField DataField="IntentosFallidos" HeaderText="Intentos Fallidos" />
                    <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
                </Columns>
            </asp:GridView>
        </div>
    </div>

    <div class="form-container">
        <h2>Asignacion de Roles</h2>

        <div class="grid-scroll">
            <asp:GridView CssClass="grid-crud" ID="gvUsuariosRoles" runat="server" AutoGenerateColumns="false"
                DataKeyNames="IdUsuario" OnSelectedIndexChanged="gvUsuariosRoles_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                    <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
                    <asp:BoundField DataField="NombreRol" HeaderText="Rol" />
                </Columns>
            </asp:GridView>
        </div>
        <div class="abm-field">
            <asp:Label CssClass="label-base" runat="server">Usuario</asp:Label>
            <asp:TextBox ID="txtUsuarioRol" CssClass="input-base" runat="server" ReadOnly="true"></asp:TextBox>
        </div>
        <div class="abm-field">
            <asp:Label CssClass="label-base" runat="server">Rol</asp:Label>
            <asp:DropDownList CssClass="ddl-base" runat="server" ID="ddlRoles"></asp:DropDownList>
        </div>
        <div class="abm-actions">
            <asp:HiddenField ID="hiddenIdUsuarioRol" runat="server" />
            <asp:Button ID="btnGuardarRol" CssClass="btn-base btn-success" runat="server" Text="Asignar Rol" OnClick="btnGuardarRol_Click" />
            <asp:Button ID="btnModificarRol" CssClass="btn-base btn-primary" runat="server" Text="Modificar Rol" OnClick="btnModificarRol_Click" />
            <asp:Button ID="btnEliminarRol" CssClass="btn-base btn-danger" runat="server" Text="Eliminar Rol" OnClick="btnEliminarRol_Click" />
        </div>
        <asp:Label ID="lblMensajeRol" runat="server" CssClass="msg-form"></asp:Label>

    </div>

</asp:Content>
