<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Medicos.aspx.cs" Inherits="Presentacion.WebForm2" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="medicos-layout">

        <div class="grid-container turnos-dia">
            <h2 id="hTurnosDelDia" runat="server" data-i18n="1">Turnos del dia</h2>
            <span id="spanAyudaAtender" runat="server" class="ayuda-campo" data-i18n="1">Selecciona un paciente con "Atender" para registrar su atencion.</span>

            <asp:GridView ID="gvTurnosMedicos" runat="server" AutoGenerateColumns="false" CssClass="grid-crud"
                OnSelectedIndexChanged="gvTurnosMedicos_SelectedIndexChanged">
                <Columns>
                    <asp:CommandField ShowSelectButton="true" SelectText="Atender" />
                    <asp:BoundField DataField="Hora" HeaderText="Hora" />
                    <asp:BoundField DataField="Paciente" HeaderText="Paciente" />
                    <asp:BoundField DataField="ObraSocial" HeaderText="Obra Social" />
                    <asp:TemplateField HeaderText="Estado">
                        <ItemTemplate>
                            <span class='<%# "badge badge-" + Eval("Estado").ToString().ToLower().Replace(" ", "-") %>'><%# Eval("Estado") %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <SelectedRowStyle CssClass="fila-seleccionada" />
            </asp:GridView>
        </div>

        <div class="form-container atencion-medica">
            <h2 id="hAtencionMedica" runat="server" data-i18n="1">Atencion medica</h2>

            <asp:Label ID="lblPacienteSeleccionado" runat="server" CssClass="paciente-seleccionado"
                Text="Ningun paciente seleccionado."></asp:Label>

            <asp:Label ID="lblMotivoConsulta" runat="server" CssClass="label-base" Text="Motivo de la consulta:" data-i18n="1"></asp:Label>
            <asp:TextBox ID="txtMotivoConsulta" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2"></asp:TextBox>

            <asp:Label ID="lblDiagnostico" runat="server" CssClass="label-base" Text="Diagnostico:" data-i18n="1"></asp:Label>
            <asp:TextBox ID="txtDiagnostico" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2"></asp:TextBox>

            <asp:Label ID="lblObservaciones" runat="server" CssClass="label-base" Text="Observaciones:" data-i18n="1"></asp:Label>
            <asp:TextBox ID="txtObservaciones" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2"></asp:TextBox>

            <div id="Receta" class="section-card">
                <h3 id="hReceta" runat="server" data-i18n="1">Receta</h3>

                <asp:Label ID="lblMedicamento" runat="server" CssClass="label-base" Text="Medicamento:" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtMedicamento" CssClass="input-base" runat="server"></asp:TextBox>

                <asp:Label ID="lblIndicacionesReceta" runat="server" CssClass="label-base" Text="Indicaciones:" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtObservacionesReceta" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2"></asp:TextBox>
            </div>

            <div class="acciones-turno">
                <asp:Button ID="btnGuardarAtencion" CssClass="btn-base btn-success" runat="server"
                    Text="Guardar Atencion Medica" data-i18n="1" OnClick="btnGuardarAtencion_Click" />
            </div>

            <asp:Label ID="lblMensajeAtencion" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </div>

    </div>

</asp:Content>
