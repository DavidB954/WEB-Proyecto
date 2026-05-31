<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    
        <div class="form-container CrearTurnos">
            <h2>Crear turno medico</h2>
            <asp:Label  runat="server" CssClass="label-base" Text="Seleccione Especialidad"></asp:Label>
            <asp:DropDownList ID="ddlEspecialidades" CssClass="ddl-base" runat="server"></asp:DropDownList>
            <br />
            <asp:Label  runat="server" CssClass="label-base" Text="Seleccione el medico"></asp:Label>
            <asp:DropDownList ID="ddlMedico" CssClass="ddl-base" runat="server"></asp:DropDownList>
            <div class="horario-turno">
                <asp:label runat="server" for="ddlDias" CssClass="label-base">Día:</asp:label>
                <asp:DropDownList ID="ddlDias" CssClass="ddl-base" runat="server"></asp:DropDownList>
                <asp:label runat="server" for="ddlHoras" CssClass="label-base">Hora:</asp:label>
                <asp:DropDownList ID="ddlHoras" CssClass="ddl-base" runat="server"></asp:DropDownList>

            </div>


            <asp:Button ID="btnAgendar" CssClass="btn-base btn-success" runat="server" Text="Agendar Turno" />
            <asp:Button ID="btnCancelar" CssClass="btn-base btn-danger" runat="server" Text="Cancelar"/>

        </div>
        <div class="grid-container misTurnos">
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
