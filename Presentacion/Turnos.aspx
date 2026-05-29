<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div id="CrearTurnos">
    Seleccione Especialidad:
    <br />
    <asp:DropDownList ID="ddlEspecialidades" CssClass="ddl-turnos" runat="server"></asp:DropDownList>>
    <br />
    Seleccione el Medico:
    <br />
    <asp:DropDownList ID="ddlMedico" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
    <br></br>
    Seleccione el horario:
    <br />
    <asp:DropDownList ID="ddlDias" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
    <asp:DropDownList ID="ddlHoras" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
    <br />
    <asp:Button ID="btnAgendar" CssClass="btn-agendar" runat="server" Text="Agendar Turno" />
    <asp:Button ID="btnCancelar" CssClass="btn-cancelar" runat="server" Text="Cancelar"/>
       </div>

    <div id="Turnos">
        Mis Turnos:
        <asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="False" CssClass="grid-turnos">
            <Columns>
                <asp:BoundField DataField="Especialidad" HeaderText="Especialidad" />
                <asp:BoundField DataField="Medico" HeaderText="Médico" />
                <asp:BoundField DataField="Dia" HeaderText="Día" />
                <asp:BoundField DataField="Hora" HeaderText="Hora" />
                <asp:ButtonField ButtonType="Button" CommandName="CancelarTurno" Text="Cancelar Turno" />

            </Columns>
            </asp:GridView>
    </div>

</asp:Content>
