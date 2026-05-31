<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="CRUD_Usuarios.aspx.cs" Inherits="Presentacion.WebForm1" %>

<asp:Content 
    ID="Content1"
    ContentPlaceHolderID="ContentPlaceHolder1"
    runat="server">

     
        <div class="form-container form-cred">
           
            <h2 class="form-title">Gestión de Usuarios</h2>

            <div class="form-col">                
                <asp:Label CssClass="label-base" runat="server">Nombre: </asp:Label>
                <asp:TextBox ID="txtNombre" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:Label CssClass="label-base" runat="server">Apellido: </asp:Label>
                <asp:TextBox ID="txtApellido" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:Label CssClass="label-base" runat="server">DNI: </asp:Label>
                <asp:TextBox ID="txtDNI" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:Label CssClass="label-base" runat="server">Email: </asp:Label>
                <asp:TextBox ID="txtEmail" CssClass="input-base" runat="server"></asp:TextBox>
                <asp:Label CssClass="label-base" runat="server">Contraseña: </asp:Label>
                <asp:TextBox ID="txtPassword" CssClass="input-base" runat="server" TextMode="Password"></asp:TextBox>

            </div>

             <div class="form-col">
                 <asp:Label CssClass="label-base" runat="server">Tipo de Usuario: </asp:Label>
                  <asp:DropDownList CssClass="ddl-base" runat="server" ID="ddlTipo" AutoPostBack="true" OnSelectedIndexChanged="ddlTipo_SelectedIndexChanged"> </asp:DropDownList>

                   <!--Si es un paciente -->
                     <div id="divPaciente" runat="server" Visible="false" class="section-card">
                        <asp:Label ID="lblHistoriaClinica" class="label-base" runat="server" Visible="false">Numero de Historia Clínica: </asp:Label>
                        <asp:TextBox ID="txtHistoriaClinica" class="input-base" runat="server" Visible="false"></asp:TextBox>

                        <asp:Label ID="lblFechaNacimiento" class="label-base" runat="server" Visible="false">Fecha de Nacimiento: </asp:Label>
                        <asp:TextBox ID="txtFechaNac" class="input-base" runat="server" TextMode="Date" Visible="false" ></asp:TextBox>

                        <asp:Label ID="lblTelefono" class="label-base" runat="server" Visible="false">Telefono: </asp:Label>
                        <asp:TextBox ID="txtTelefono" class="input-base" runat="server" Visible="false"></asp:TextBox>
                    </div>

                    <!--Si es un Medico -->

                 <div id="divMedico" runat="server" Visible="false" class="section-card">
                    <asp:Label ID="lblMatricula" class="label-base" runat="server" Visible="false">Matricula: </asp:Label>
                    <asp:TextBox ID="txtMatricula" class="input-base" runat="server" Visible="false"></asp:TextBox>

                     <asp:Label ID="lblEspecialidad" class="label-base" runat="server" Visible="false">Especialidad: </asp:Label>
                    <asp:TextBox ID="txtEspecialidad" class="input-base" runat="server" Visible="false"></asp:TextBox>

                    <asp:CheckBoxList ID="chkDias" class="section-card" runat="server" Visible="false">
                        <asp:ListItem Value="Lunes">Lunes</asp:ListItem>
                        <asp:ListItem Value="Martes">Martes</asp:ListItem>
                        <asp:ListItem Value="Miercoles">Miércoles</asp:ListItem>
                        <asp:ListItem value="Jueves">Jueves</asp:ListItem>  
                        <asp:ListItem Value="Viernes">Viernes</asp:ListItem>
                        <asp:ListItem Value="Sabado">Sábado</asp:ListItem>
                    </asp:CheckBoxList>
                     </div>

                    <!-- Hora de inicio-->
                    <asp:Label ID="lblHoraInicio" class="label-base" runat="server" Visible="false">Hora Inicio: </asp:Label>
                    <asp:TextBox ID="txtHoraInicio" class="input-base" runat="server" Visible="false"></asp:TextBox>

                    <!-- Hora de fin-->
                    <asp:Label ID="lblHoraFin" class="label-base" runat="server" Visible="false">Hora Fin: </asp:Label>
                    <asp:TextBox ID="txtHoraFin" class="input-base" runat="server" Visible="false"></asp:TextBox>
             </div>
          <div class="form-actions">
            <asp:Button ID="btnGuardar" CssClass="btn-base btn-success" runat="server" Text="Guardar"/>
            <asp:Button ID="btnModificar" CssClass="btn-base btn-primary" runat="server" Text="Modificar"/>
            <asp:Button ID="btnEliminar" CssClass="btn-base btn-danger" runat="server" Text="Eliminar"/>
           </div>

        </div>
    

    <div class="grid-container">   
          <asp:GridView CssClass="grid-crud" ID="gvUsuarios" runat="server" AutoGenerateColumns="false">
                  <Columns>
                      <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                      <asp:BoundField DataField="IdUsuario" HeaderText="ID" ReadOnly="True" />
                      <asp:BoundField DataField="Nombre" HeaderText="Nombre" />
                      <asp:BoundField DataField="Apellido" HeaderText="Apellido" />
                      <asp:BoundField DataField="DNI" HeaderText="DNI" />
                      <asp:BoundField DataField="Email" HeaderText="Email" />
                      <asp:BoundField DataField="HashPassword" HeaderText="Contraseña" />
                      <asp:BoundField DataField="IntentosFallidos" HeaderText="Intentos Fallidos" />
                      <asp:CheckBoxField DataField="Activo" HeaderText="Activo" />
                  
                  </Columns>
            </asp:GridView>
    </div>

          
      
    </asp:Content>