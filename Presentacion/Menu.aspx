<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="Presentacion.Menu" %>


<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <nav class="navbar-custom">
        <ul>
            <li>
                <a href="CRUD_Usuarios.aspx">Gestión Usuarios</a>
            </li>

            <li>
                <a href="Turnos.aspx">Turnos</a>
            </li>

            <li>
                <a href="Medico.aspx">Médicos</a>
            </li>

            <li>
                <a href="Bitacora.aspx">Bitácora</a>
            </li>

            <!-- Opcionales -->
            <li>
                <a href="Perfil.aspx">Mi Perfil</a>
            </li>

            <li>
                <a href="CerrarSesion.aspx">Cerrar Sesión</a>
            </li>
        </ul>
    </nav>

</asp:Content>