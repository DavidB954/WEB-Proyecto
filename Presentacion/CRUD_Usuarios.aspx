<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="CRUD_Usuarios.aspx.cs" Inherits="Presentacion.WebForm1" %>

<asp:Content 
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

    <!-- TODO TU CONTENIDO -->


        <div class="form-cred">
            <asp:Label CssClass="lbl-crud" runat="server">Nombre: </asp:Label>
            <asp:TextBox ID="txtNombre" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />
            <asp:Label CssClass="lbl-crud" runat="server">Apellido: </asp:Label>
            <asp:TextBox ID="txtApellido" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />
            <asp:Label CssClass="lbl-crud" runat="server">DNI: </asp:Label>
            <asp:TextBox ID="txtDNI" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />
            <asp:Label CssClass="lbl-crud" runat="server">Tipo de Usuario: </asp:Label>
            <asp:DropDownList CssClass="ddl-crud" runat="server" ID="ddlTipo"> </asp:DropDownList>
            <br />
           <!--Si es un paciente -->
            <asp:Label ID="lblHistoriaClinica" CssClass="lbl-crud" runat="server" Visible="false">Numero de Historia Clínica: </asp:Label>
            <asp:TextBox ID="txtHistoriaClinica" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />
            <asp:Label ID="lblFechaNacimiento" CssClass="lbl-crud" runat="server" Visible="false">Fecha de Nacimiento: </asp:Label>
            <asp:Calendar ID="cldFechaNac" CssClass="input-crud" runat="server"></asp:Calendar>
            <br />
            <asp:Label ID="lblTelefono" CssClass="lbl-crud" runat="server" Visible="false">Telefono: </asp:Label>
            <asp:TextBox ID="txtTelefono" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />

            <!--Si es un Medico -->
            <asp:Label ID="lblMatricula" CssClass="lbl-crud" runat="server" Visible="false">Matricula: </asp:Label>
            <asp:TextBox ID="txtMatricula" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />

             <asp:Label ID="lblEspecialidad" CssClass="lbl-crud" runat="server" Visible="false">Especialidad: </asp:Label>
            <asp:TextBox ID="txtEspecialidad" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />

            <asp:CheckBoxList ID="chkDias" CssClass="input-crud" runat="server" Visible="false">
                <asp:ListItem Value="Lunes">Lunes</asp:ListItem>
                <asp:ListItem Value="Martes">Martes</asp:ListItem>
                <asp:ListItem Value="Miercoles">Miércoles</asp:ListItem>
                <asp:ListItem value="Jueves">Jueves</asp:ListItem>  
                <asp:ListItem Value="Viernes">Viernes</asp:ListItem>
                <asp:ListItem Value="Sabado">Sábado</asp:ListItem>
            </asp:CheckBoxList>


            <!-- Hora de inicio-->
            <asp:Label ID="lblHoraInicio" CssClass="lbl-crud" runat="server" Visible="false">Hora Inicio: </asp:Label>
            <asp:TextBox ID="txtHoraInicio" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />

            <!-- Hora de fin-->
            <asp:Label ID="lblHoraFin" CssClass="lbl-crud" runat="server" Visible="false">Hora Fin: </asp:Label>
            <asp:TextBox ID="txtHoraFin" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />

            <!-- Resto -->

            <asp:Label CssClass="lbl-crud" runat="server">Email: </asp:Label>
            <asp:TextBox ID="txtEmail" CssClass="input-crud" runat="server"></asp:TextBox>
            <br />
            <asp:Label CssClass="lbl-crud" runat="server">Contraseña: </asp:Label>
            <asp:TextBox ID="txtPassword" CssClass="input-crud" runat="server" TextMode="Password"></asp:TextBox>
            <br />
            <asp:Button ID="btnGuardar" CssClass="btn-crud" runat="server" Text="Guardar"/>
            <asp:Button ID="btnModificar" CssClass="btn-crud" runat="server" Text="Modificar"/>
            <asp:Button ID="btnEliminar" CssClass="btn-crud" runat="server" Text="Eliminar"/>

        </div>
    

            <asp:GridView ID="gvUsuarios" runat="server" AutoGenerateColumns="false">
                <Columns>
                    <asp:BoundField DataField="IdUsuario" HeaderText="ID" ReadOnly="True" />
                    <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                    <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
                    <asp:BoundField DataField="DNI" HeaderText="DNI" />
                    <asp:BoundField DataField="Email" HeaderText="Email" />
                    <asp:BoundField DataField="HashPassword" HeaderText="Contraseña" />
                    <asp:BoundField DataField="IntentosFallidos" HeaderText="Intentos Fallidos" />
                    <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
      
    </asp:Content>