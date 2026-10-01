<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Menu.aspx.cs" Inherits="Presentacion.Menu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
 <div class="welcome-panel">
    <h1 id="hBienvenida" runat="server" data-i18n="1">Bienvenido al Sistema Medico</h1>
    <p id="pBienvenida" runat="server" data-i18n="1">Gestione turnos, medicos y pacientes facilmente.</p>
</div>

<section class="home-options">
    <div id="cardTurnos" runat="server" class="option-card">
        <i class="fas fa-calendar-alt"></i>
        <h2 id="hTurnos" runat="server" data-i18n="1">Turnos</h2>
        <p id="pTurnos" runat="server" data-i18n="1">Agende y consulte sus turnos medicos.</p>
        <asp:Button ID="btnIrTurnos" runat="server" Text="Ir a Turnos" data-i18n="1" CssClass="btn-base btn-primary" PostBackUrl="~/Turnos.aspx" />
    </div>

    <div id="cardUsuarios" runat="server" class="option-card">
        <i class="fas fa-user"></i>
        <h2 id="hUsuarios" runat="server" data-i18n="1">Usuarios</h2>
        <p id="pUsuarios" runat="server" data-i18n="1">Gestione la informacion de usuarios.</p>
        <asp:Button ID="btnVerUsuarios" runat="server" Text="Ver Usuarios" data-i18n="1" CssClass="btn-base btn-primary" PostBackUrl="~/CRUD_Usuarios.aspx" />
    </div>

    <div id="cardRecetas" runat="server" class="option-card">
        <i class="fas fa-pills"></i>
        <h2 id="hRecetas" runat="server" data-i18n="1">Mis Recetas</h2>
        <p id="pRecetas" runat="server" data-i18n="1">Revise las recetas emitidas por sus medicos.</p>
        <asp:Button ID="btnVerRecetas" runat="server" Text="Ver Recetas" data-i18n="1" CssClass="btn-base btn-success" PostBackUrl="~/Recetas.aspx" />
    </div>

    <div id="cardMedicos" runat="server" class="option-card">
        <i class="fas fa-user-md"></i>
        <h2 id="hMedicos" runat="server" data-i18n="1">Medicos</h2>
        <p id="pMedicos" runat="server" data-i18n="1">Atienda los turnos del dia y registre las atenciones.</p>
        <asp:Button ID="btnIrAgenda" runat="server" Text="Ir a Mi Agenda" data-i18n="1" CssClass="btn-base btn-primary" PostBackUrl="~/Medicos.aspx" />
    </div>

    <div id="cardBitacora" runat="server" class="option-card">
        <i class="fas fa-clipboard-list"></i>
        <h2 id="hBitacora" runat="server" data-i18n="1">Bitacora</h2>
        <p id="pBitacora" runat="server" data-i18n="1">Consulte el registro de eventos del sistema.</p>
        <asp:Button ID="btnVerBitacora" runat="server" Text="Ver Bitacora" data-i18n="1" CssClass="btn-base btn-danger" PostBackUrl="~/Bitacora.aspx" />
    </div>

    <div id="cardSeguridad" runat="server" class="option-card">
    <i class="fas fa-clipboard-list"></i>
    <h2 id="hSeguridad" runat="server" data-i18n="1">Seguridad</h2>
    <p id="pSeguridad" runat="server" data-i18n="1">Opciones de seguridad del sistema</p>
    <asp:Button ID="btnVerSeguridad" runat="server" Text="Ver Seguridad" data-i18n="1" CssClass="btn-base btn-danger" PostBackUrl="~/Seguridad.aspx" />
</div>

    <div id="cardRolesPermisos" runat="server" class="option-card">
        <i class="fas fa-sitemap"></i>
        <h2 id="hRolesPermisos" runat="server" data-i18n="1">Roles y Permisos</h2>
        <p id="pRolesPermisos" runat="server" data-i18n="1">Cree y componga roles y permisos del sistema.</p>
        <asp:Button ID="btnVerRolesPermisos" runat="server" Text="Ver Roles y Permisos" data-i18n="1" CssClass="btn-base btn-primary" PostBackUrl="~/GestionRolesPermisos.aspx" />
    </div>

    <div id="cardAsignacionSeguridad" runat="server" class="option-card">
        <i class="fas fa-user-shield"></i>
        <h2 id="hAsignacionSeguridad" runat="server" data-i18n="1">Asignacion de Seguridad</h2>
        <p id="pAsignacionSeguridad" runat="server" data-i18n="1">Asigne roles y permisos a cada usuario.</p>
        <asp:Button ID="btnVerAsignacionSeguridad" runat="server" Text="Ver Asignacion de Seguridad" data-i18n="1" CssClass="btn-base btn-primary" PostBackUrl="~/AsignacionSeguridad.aspx" />
    </div>

</section>
</asp:Content>
