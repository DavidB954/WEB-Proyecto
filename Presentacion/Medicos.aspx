<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Medicos.aspx.cs" Inherits="Presentacion.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="contenedor">
            <h2>Turnos del dia</h2>
            <asp:GridView ID="gvTurnosMedicos" runat="server" AutoGenerateColumns="false"  CssClass="grid-crud">
              <Columns >
                  <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                  <asp:BoundField DataField="IdPaciente" HeaderText="Paciente" ReadOnly="True" />
                  <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                  <asp:BoundField DataField="Hora" HeaderText="Hora" DataFormatString="{0:HH:mm:ss}" />           
               </Columns>
              </asp:GridView>
         <div class="AtencionMedica">
             <h2>Formulario medico</h2>
                Motivo de la consulta:
                <br />
                <asp:TextBox ID="txtMotivoConsulta" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
                <br />
                Diagnostico:
                <asp:TextBox ID="txtDiagnostico" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
                <br />
                Observaciones:
                <asp:TextBox ID="txtObservaciones" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
                <br />
                <div id="Receta">
                    <h2>Receta</h2>
                    <br />
                    <h3>Medicamento</h3>        
                    <asp:TextBox ID="txtMedicamento" CssClass="input-crud" runat="server"></asp:TextBox>
                    <h3>Observaciones</h3>
                    <asp:TextBox ID="txtObservacionesReceta" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
                 </div>
                <br />
                <asp:Button ID="btnGuardarAtencion" CssClass="btn-agendar" runat="server" Text="Guardar Atencion Medica" />
          </div>
    </div>
</asp:Content>
