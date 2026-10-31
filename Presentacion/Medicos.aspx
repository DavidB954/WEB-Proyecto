<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Medicos.aspx.cs" Inherits="Presentacion.WebForm2" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <%-- El usuario no tiene fila en Medico (falta la matricula): no tiene agenda. --%>
    <asp:Panel ID="pnlSinMedico" runat="server" Visible="false" CssClass="form-container">
        <p id="pSinMedico" runat="server" class="sin-datos" data-i18n="1">Tu usuario no esta registrado como medico. Pedile al administrativo que cargue tu matricula desde la agenda de medicos.</p>
    </asp:Panel>

    <asp:Panel ID="pnlMedico" runat="server" CssClass="medicos-layout">

        <%-- ===== Columna izquierda: turnos del dia ===== --%>
        <div class="grid-container turnos-dia">
            <h2 id="hTurnosDelDia" runat="server" data-i18n="1">Turnos del dia</h2>

            <div class="navegacion-dia">
                <asp:Button ID="btnDiaAnterior" runat="server" CssClass="btn-base btn-primary" Text="&lsaquo;" ToolTip="Dia anterior"
                    OnClick="btnDiaAnterior_Click" CausesValidation="false" />
                <asp:TextBox ID="txtFecha" runat="server" CssClass="input-base" TextMode="Date"
                    AutoPostBack="true" OnTextChanged="txtFecha_TextChanged"></asp:TextBox>
                <asp:Button ID="btnDiaSiguiente" runat="server" CssClass="btn-base btn-primary" Text="&rsaquo;" ToolTip="Dia siguiente"
                    OnClick="btnDiaSiguiente_Click" CausesValidation="false" />
                <asp:Button ID="btnHoy" runat="server" CssClass="btn-base btn-warning" Text="Hoy" data-i18n="1"
                    OnClick="btnHoy_Click" CausesValidation="false" />
            </div>

            <span id="spanAyudaAtender" runat="server" class="ayuda-campo" data-i18n="1">Selecciona un paciente con "Atender" para registrar su atencion.</span>

            <div class="grid-scroll">
                <asp:GridView ID="gvTurnosMedicos" runat="server" AutoGenerateColumns="false" CssClass="grid-crud"
                    DataKeyNames="IdTurno" OnSelectedIndexChanged="gvTurnosMedicos_SelectedIndexChanged"
                    OnRowCommand="gvTurnosMedicos_RowCommand">
                    <Columns>
                        <%-- Mismo orden de columnas que la version anterior: las traducciones se guardan por indice (gvTurnosMedicos_colN). --%>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkAtender" runat="server" data-i18n="1" CommandName="Select" Text="Atender" CausesValidation="false"
                                    Visible='<%# (bool)Eval("PuedeAtender") %>' data-permiso="MEDICO_ATENDER" />
                                <asp:LinkButton ID="lnkAusente" runat="server" data-i18n="1" CommandName="Ausente" Text="Ausente" CausesValidation="false"
                                    CommandArgument='<%# Eval("IdTurno") %>' Visible='<%# (bool)Eval("PuedeMarcarAusente") %>' data-permiso="MEDICO_ATENDER"
                                    OnClientClick="return confirm('Marcar al paciente como ausente?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="HoraTexto" HeaderText="Hora" />
                        <asp:BoundField DataField="NombrePaciente" HeaderText="Paciente" />
                        <asp:BoundField DataField="HistoriaClinica" HeaderText="Historia clinica" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class='<%# "badge badge-" + Eval("EstadoCss") %>'><%# Eval("EstadoTexto") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <SelectedRowStyle CssClass="fila-seleccionada" />
                    <EmptyDataTemplate>
                        <div id="divSinTurnosDia" runat="server" class="sin-datos" data-i18n="1">No tenes turnos para este dia.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <asp:Label ID="lblMensajeTurnos" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </div>

        <%-- ===== Columna derecha: atencion del turno seleccionado + historia clinica ===== --%>
        <div class="form-container atencion-medica">
            <h2 id="hAtencionMedica" runat="server" data-i18n="1">Atencion medica</h2>

            <asp:Label ID="lblPacienteSeleccionado" runat="server" CssClass="paciente-seleccionado"
                Text="Ningun paciente seleccionado." data-i18n="1"></asp:Label>

            <asp:Panel ID="pnlAtencion" runat="server" Visible="false">

                <asp:Label ID="lblMotivoConsulta" runat="server" CssClass="label-base" Text="Motivo de la consulta:" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtMotivoConsulta" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2" MaxLength="255" ValidateRequestMode="Disabled"></asp:TextBox>

                <asp:Label ID="lblDiagnostico" runat="server" CssClass="label-base" Text="Diagnostico:" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtDiagnostico" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2" MaxLength="500" ValidateRequestMode="Disabled"></asp:TextBox>

                <asp:Label ID="lblObservaciones" runat="server" CssClass="label-base" Text="Observaciones:" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtObservaciones" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000" ValidateRequestMode="Disabled"></asp:TextBox>

                <div id="Receta" class="section-card">
                    <h3 id="hReceta" runat="server" data-i18n="1">Receta</h3>
                    <span id="spanAyudaReceta" runat="server" class="ayuda-campo" data-i18n="1">Opcional. Agrega cada medicamento con "Agregar"; se guardan junto con la atencion.</span>

                    <asp:Label ID="lblMedicamento" runat="server" CssClass="label-base" Text="Medicamento:" data-i18n="1"></asp:Label>
                    <asp:TextBox ID="txtMedicamento" CssClass="input-base" runat="server" MaxLength="30" ValidateRequestMode="Disabled"></asp:TextBox>

                    <asp:Label ID="lblIndicacionesReceta" runat="server" CssClass="label-base" Text="Indicaciones:" data-i18n="1"></asp:Label>
                    <asp:TextBox ID="txtObservacionesReceta" CssClass="input-base" runat="server" TextMode="MultiLine" Rows="2" MaxLength="255" ValidateRequestMode="Disabled"></asp:TextBox>

                    <div class="abm-actions">
                        <asp:Button ID="btnAgregarMedicamento" runat="server" CssClass="btn-base btn-primary" Text="Agregar" data-i18n="1"
                            OnClick="btnAgregarMedicamento_Click" CausesValidation="false" data-permiso="MEDICO_ATENDER" />
                    </div>

                    <div class="grid-scroll">
                        <asp:GridView ID="gvReceta" runat="server" CssClass="grid-crud" AutoGenerateColumns="false"
                            OnRowCommand="gvReceta_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="Medicamento" HeaderText="Medicamento" />
                                <asp:BoundField DataField="Observaciones" HeaderText="Indicaciones" />
                                <asp:TemplateField HeaderText="Accion">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkQuitarMedicamento" runat="server" data-i18n="1" CommandName="Quitar"
                                            CommandArgument='<%# Container.DataItemIndex %>' Text="Quitar" CausesValidation="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div id="divSinMedicamentos" runat="server" class="sin-datos" data-i18n="1">Sin medicamentos recetados.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <div class="acciones-turno">
                    <asp:Button ID="btnGuardarAtencion" CssClass="btn-base btn-success" runat="server"
                        Text="Guardar Atencion Medica" data-i18n="1" OnClick="btnGuardarAtencion_Click" CausesValidation="false" data-permiso="MEDICO_ATENDER"
                        OnClientClick="return confirm('Guardar la atencion? Despues no se puede modificar.');" />
                    <asp:Button ID="btnCancelarAtencion" CssClass="btn-base btn-warning" runat="server"
                        Text="Cancelar" data-i18n="1" OnClick="btnCancelarAtencion_Click" CausesValidation="false" />
                </div>

            </asp:Panel>

            <asp:Label ID="lblMensajeAtencion" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>

            <asp:Panel ID="pnlHistoria" runat="server" Visible="false" CssClass="section-card">
                <h3 id="hHistoriaClinica" runat="server" data-i18n="1">Historia clinica</h3>

                <div class="grid-scroll">
                    <asp:GridView ID="gvHistoria" runat="server" CssClass="grid-crud" AutoGenerateColumns="false">
                        <Columns>
                            <asp:BoundField DataField="FechaTexto" HeaderText="Fecha" />
                            <asp:BoundField DataField="NombreMedico" HeaderText="Medico" />
                            <asp:BoundField DataField="MotivoConsulta" HeaderText="Motivo" />
                            <asp:BoundField DataField="Diagnostico" HeaderText="Diagnostico" />
                            <asp:BoundField DataField="Observaciones" HeaderText="Observaciones" />
                            <asp:BoundField DataField="RecetasTexto" HeaderText="Receta" />
                        </Columns>
                        <EmptyDataTemplate>
                            <div id="divSinHistoria" runat="server" class="sin-datos" data-i18n="1">El paciente no tiene atenciones anteriores.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </asp:Panel>
        </div>

    </asp:Panel>

</asp:Content>
