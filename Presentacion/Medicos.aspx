<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Medicos.aspx.cs" Inherits="Presentacion.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="contenedor">
            
        <div class="grid-container">
        <h2>Turnos del dia</h2>
            <asp:GridView ID="gvTurnosMedicos" runat="server" AutoGenerateColumns="false"  CssClass="grid-crud">
              <Columns >
                  <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                  <asp:BoundField DataField="IdPaciente" HeaderText="Paciente" ReadOnly="True" />
                  <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
                  <asp:BoundField DataField="Hora" HeaderText="Hora" DataFormatString="{0:HH:mm:ss}" />           
               </Columns>
              </asp:GridView>
        </div>

         <div class="form-container AtencionMedica">
             <h2>Formulario medico</h2>
             <asp:Label  runat="server" CssClass="label-base" Text="Motivo de la consulta:"></asp:Label>
                <asp:TextBox ID="txtMotivoConsulta" CssClass="input-base" runat="server" TextMode="MultiLine"></asp:TextBox>
                
             <asp:Label  runat="server" CssClass="label-base" Text="Diagnostico:"></asp:Label>
                <asp:TextBox ID="txtDiagnostico" CssClass="input-base" runat="server" TextMode="MultiLine"></asp:TextBox>
                
             <asp:Label  runat="server" CssClass="label-base" Text="Observaciones:"></asp:Label>
                <asp:TextBox ID="txtObservaciones" CssClass="input-base" runat="server" TextMode="MultiLine"></asp:TextBox>
                <br />
                <div id="Receta">
                    <h2>Receta</h2>
                    <br />
                    <h3 class="label-base">Medicamento</h3>        
                    <asp:TextBox ID="txtMedicamento" CssClass="input-base" runat="server"></asp:TextBox>
                    <h3 class="label-base">Observaciones</h3>
                    <asp:TextBox ID="txtObservacionesReceta" CssClass="input-base" runat="server" TextMode="MultiLine"></asp:TextBox>
                 </div>
                <br />
                <asp:Button ID="btnGuardarAtencion" CssClass="btn-base btn-success" runat="server" Text="Guardar Atencion Medica" />
          </div>
    </div>
</asp:Content>
