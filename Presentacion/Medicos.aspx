<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Medicos.aspx.cs" Inherits="Presentacion.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
      <asp:GridView ID="gvTurnosMedicos" runat="server" AutoGenerateColumns="false"  CssClass="grid-crud">
      <Columns >
          <asp:BoundField DataField="IdPaciente" HeaderText="Paciente" ReadOnly="True" />
          <asp:BoundField DataField="Fecha" HeaderText="Fecha" />
          <asp:BoundField DataField="Hora" HeaderText="Hora" DataFormatString="{0:HH:mm:ss}" />
           <asp:ButtonField ButtonType="Button" CommandName="btnAtencionMedica" Text="Atencion Medica" />
          </Columns>

  </asp:GridView>

 <div id="AtencionMedica">
     Motivo de la consulta:
     <br />
     <asp:TextBox ID="txtMotivoConsulta" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
     <br />
     Diagnostico:
     <asp:TextBox ID="txtDiagnostico" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
     <br />
     Observaciones:
     <asp:TextBox ID="txtObservacione" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
     <div id="Receta">
         Receta:
         <br />
         Medicamento: 
         <asp:TextBox ID="txtMedicamento" CssClass="input-crud" runat="server"></asp:TextBox>
         Observaciones:
         <asp:TextBox ID="txtObservacionesReceta" CssClass="input-crud" runat="server" TextMode="MultiLine"></asp:TextBox>
      </div>
     <asp:Button ID="btnGuardarAtencion" CssClass="btn-agendar" runat="server" Text="Guardar Atencion Medica" />
 </div>



</asp:Content>
