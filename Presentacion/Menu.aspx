<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="Presentacion.Menu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 <div class="welcome-panel"> 
    <h1>Bienvenido al Sistema Medico</h1> 
    <p>Gestione turnos, medicos y pacientes facilmente.</p>
</div>

<section class="home-options">
    <div id="cardTurnos" runat="server" class="option-card">
        <i class="fas fa-calendar-alt"></i>
        <h2>Turnos</h2>
        <p>Agende y consulte sus turnos medicos.</p>
        <asp:Button runat="server" Text="Ir a Turnos" CssClass="btn-base btn-primary" PostBackUrl="~/Turnos.aspx" />
    </div>

    <div id="cardUsuarios" runat="server" class="option-card">
        <i class="fas fa-user"></i>
        <h2>Usuarios</h2>
        <p>Gestione la informacion de usuarios.</p>
        <asp:Button runat="server" Text="Ver Usuarios" CssClass="btn-base btn-primary" PostBackUrl="~/CRUD_Usuarios.aspx" />
    </div>

    <div id="cardRecetas" runat="server" class="option-card">
        <i class="fas fa-pills"></i>
        <h2>Mis Recetas</h2>
        <p>Revise las recetas emitidas por sus medicos.</p>
        <asp:Button runat="server" Text="Ver Recetas" CssClass="btn-base btn-success" PostBackUrl="~/Recetas.aspx" />
    </div>

    <div id="cardMedicos" runat="server" class="option-card">
        <i class="fas fa-user-md"></i>
        <h2>Medicos</h2>
        <p>Atienda los turnos del dia y registre las atenciones.</p>
        <asp:Button runat="server" Text="Ir a Mi Agenda" CssClass="btn-base btn-primary" PostBackUrl="~/Medicos.aspx" />
    </div>

    <div id="cardBitacora" runat="server" class="option-card">
        <i class="fas fa-clipboard-list"></i>
        <h2>Bitacora</h2>
        <p>Consulte el registro de eventos del sistema.</p>
        <asp:Button runat="server" Text="Ver Bitacora" CssClass="btn-base btn-danger" PostBackUrl="~/Bitacora.aspx" />
    </div>

    <div id="cardSeguridad" runat="server" class="option-card">
    <i class="fas fa-clipboard-list"></i>
    <h2>Seguridad</h2>
    <p>Opciones de seguridad del sistema</p>
    <asp:Button runat="server" Text="Ver Seguridad" CssClass="btn-base btn-danger" PostBackUrl="~/Seguridad.aspx" />
</div>


</section>
</asp:Content>
