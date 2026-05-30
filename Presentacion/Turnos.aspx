<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    
    <div class="CrearTurnos">
        <h2>Crear turno medico</h2>
            Seleccione Especialidad:
            <br />
            <asp:DropDownList ID="ddlEspecialidades" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
            <br />
            Seleccione el Medico:
            <br />
            <asp:DropDownList ID="ddlMedico" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
            <br>
            <div class="horario-turno">
                <label for="ddlDias">Día:</label>
                <asp:DropDownList ID="ddlDias" CssClass="ddl-turnos" runat="server"></asp:DropDownList>

                <label for="ddlHoras">Hora:</label>
                <asp:DropDownList ID="ddlHoras" CssClass="ddl-turnos" runat="server"></asp:DropDownList>
            </div>
            <asp:Button ID="btnAgendar" CssClass="btn-crud" runat="server" Text="Agendar Turno" />
            <asp:Button ID="btnCancelar" CssClass="btn-crud" runat="server" Text="Cancelar"/>
       </div>

    <div class="misTurnos">
        <h2>Mis Turnos</h2>
        <asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="False" CssClass="grid-crud">
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
