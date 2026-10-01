<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="turnos-layout">

        <div class="form-container crear-turno">
            <h2 id="hCrearTurno" runat="server" data-i18n="1">Crear turno medico</h2>

            <asp:Label ID="lblEspecialidad" runat="server" CssClass="label-base" Text="Especialidad" data-i18n="1"></asp:Label>
            <asp:DropDownList ID="ddlEspecialidades" CssClass="ddl-base" runat="server"
                AutoPostBack="true" OnSelectedIndexChanged="ddlEspecialidades_SelectedIndexChanged">
            </asp:DropDownList>

            <asp:Label ID="lblMedico" runat="server" CssClass="label-base" Text="Medico" data-i18n="1"></asp:Label>
            <asp:DropDownList ID="ddlMedico" CssClass="ddl-base" runat="server"></asp:DropDownList>

            <div class="horario-turno">
                <asp:Label ID="lblDia" runat="server" CssClass="label-base" Text="Dia:" data-i18n="1"></asp:Label>
                <asp:DropDownList ID="ddlDias" CssClass="ddl-base" runat="server"></asp:DropDownList>

                <asp:Label ID="lblHora" runat="server" CssClass="label-base" Text="Hora:" data-i18n="1"></asp:Label>
                <asp:DropDownList ID="ddlHoras" CssClass="ddl-base" runat="server"></asp:DropDownList>
            </div>

            <div class="acciones-turno">
                <asp:Button ID="btnAgendar" CssClass="btn-base btn-success" runat="server" Text="Agendar Turno" data-i18n="1" OnClick="btnAgendar_Click" />
                <asp:Button ID="btnCancelar" CssClass="btn-base btn-danger" runat="server" Text="Limpiar" data-i18n="1" OnClick="btnCancelar_Click" />
            </div>

            <asp:Label ID="lblMensajeTurno" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </div>

        <div class="grid-container mis-turnos">
            <h2 id="hMisTurnos" runat="server" data-i18n="1">Mis Turnos</h2>
            <asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="False" CssClass="grid-crud"
                OnRowCommand="gvTurnos_RowCommand">
                <Columns>
                    <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                    <asp:BoundField DataField="Medico" HeaderText="Medico" />
                    <asp:BoundField DataField="Dia" HeaderText="Dia" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <span class='<%# "badge badge-" + Eval("Estado").ToString().ToLower() %>'><%# Eval("Estado") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:ButtonField ButtonType="Button" CommandName="CancelarTurno" Text="Cancelar Turno" />
                </Columns>
                <EmptyDataTemplate>
                    <div class="sin-datos">No tenes turnos agendados. Crea uno desde el formulario.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

    </div>

</asp:Content>
