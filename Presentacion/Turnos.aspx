<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <%-- El usuario no tiene el rol PACIENTE: no hay nada que pueda hacer aca. --%>
    <asp:Panel ID="pnlSinRolPaciente" runat="server" Visible="false" CssClass="form-container">
        <p id="pSinRolPaciente" runat="server" class="sin-datos" data-i18n="1">Esta pantalla es para pacientes. Tu usuario no tiene el rol PACIENTE.</p>
    </asp:Panel>

    <asp:Panel ID="pnlPaciente" runat="server" CssClass="turnos-layout">

        <%-- ===== Columna izquierda: datos del paciente + solicitud de turno ===== --%>
        <div class="turnos-col">

            <div class="form-container">
                <h2 id="hMisDatos" runat="server" data-i18n="1">Mis datos</h2>

                <asp:Label ID="lblHistoriaClinica" runat="server" CssClass="paciente-seleccionado"></asp:Label>
                <span id="spanAyudaDatos" runat="server" class="ayuda-campo" data-i18n="1">Completa tu fecha de nacimiento para poder pedir turnos. El telefono es opcional.</span>

                <asp:Label ID="lblFechaNacimiento" runat="server" CssClass="label-base" Text="Fecha de nacimiento" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="input-base" TextMode="Date"></asp:TextBox>

                <asp:Label ID="lblTelefono" runat="server" CssClass="label-base" Text="Telefono" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtTelefono" runat="server" CssClass="input-base" MaxLength="20"></asp:TextBox>

                <div class="acciones-turno">
                    <asp:Button ID="btnGuardarDatos" runat="server" CssClass="btn-base btn-success" Text="Guardar mis datos" data-i18n="1"
                        OnClick="btnGuardarDatos_Click" CausesValidation="false" data-permiso="TURNO_ALTA" />
                </div>

                <asp:Label ID="lblMensajeDatos" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
            </div>

            <asp:Panel ID="pnlSolicitar" runat="server" CssClass="form-container crear-turno">
                <h2 id="hCrearTurno" runat="server" data-i18n="1">Crear turno medico</h2>

                <div class="filtros-agenda">
                    <div class="campo-filtro">
                        <asp:Label ID="lblEspecialidad" runat="server" CssClass="label-base" Text="Especialidad" data-i18n="1"></asp:Label>
                        <asp:DropDownList ID="ddlEspecialidades" runat="server" CssClass="ddl-base"
                            DataTextField="Nombre" DataValueField="IdEspecialidad"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlEspecialidades_SelectedIndexChanged">
                        </asp:DropDownList>
                    </div>
                    <div class="campo-filtro">
                        <asp:Label ID="lblBuscarNombre" runat="server" CssClass="label-base" Text="Nombre del medico" data-i18n="1"></asp:Label>
                        <div class="fila-inline">
                            <asp:TextBox ID="txtBuscarNombre" runat="server" CssClass="input-base" MaxLength="60"></asp:TextBox>
                            <asp:Button ID="btnBuscar" runat="server" CssClass="btn-base btn-primary" Text="Buscar" data-i18n="1" OnClick="btnBuscar_Click" CausesValidation="false" />
                        </div>
                    </div>
                </div>

                <asp:Label ID="lblMedico" runat="server" CssClass="label-base" Text="Medico" data-i18n="1"></asp:Label>
                <asp:DropDownList ID="ddlMedico" runat="server" CssClass="ddl-base"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlMedico_SelectedIndexChanged">
                </asp:DropDownList>

                <asp:Label ID="lblDia" runat="server" CssClass="label-base" Text="Dia:" data-i18n="1"></asp:Label>
                <asp:DropDownList ID="ddlFecha" runat="server" CssClass="ddl-base"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlFecha_SelectedIndexChanged">
                </asp:DropDownList>
                <span id="spanAyudaFechas" runat="server" class="ayuda-campo" data-i18n="1">Solo se muestran los dias con turnos libres. No aparecen los dias en que ya tenes turno con ese medico.</span>

                <asp:Label ID="lblHora" runat="server" CssClass="label-base" Text="Hora:" data-i18n="1"></asp:Label>
                <asp:RadioButtonList ID="rblHorarios" runat="server" CssClass="lista-opciones"
                    RepeatLayout="Flow" RepeatDirection="Horizontal">
                </asp:RadioButtonList>
                <p id="pSinHorarios" runat="server" class="sin-datos" visible="false" data-i18n="1">No hay turnos libres para el medico seleccionado.</p>

                <div class="acciones-turno">
                    <asp:Button ID="btnAgendar" runat="server" CssClass="btn-base btn-success" Text="Agendar Turno" data-i18n="1"
                        OnClick="btnAgendar_Click" CausesValidation="false" data-permiso="TURNO_ALTA"
                        OnClientClick="return confirm('Confirmas la reserva de este turno?');" />
                    <asp:Button ID="btnCancelar" runat="server" CssClass="btn-base btn-warning" Text="Limpiar" data-i18n="1"
                        OnClick="btnCancelar_Click" CausesValidation="false" />
                </div>

                <asp:Label ID="lblMensajeTurno" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
            </asp:Panel>

        </div>

        <%-- ===== Columna derecha: turnos del paciente ===== --%>
        <div class="grid-container mis-turnos">
            <h2 id="hMisTurnos" runat="server" data-i18n="1">Mis Turnos</h2>

            <asp:RadioButtonList ID="rblVista" runat="server" data-i18n="1" CssClass="lista-opciones"
                RepeatLayout="Flow" RepeatDirection="Horizontal"
                AutoPostBack="true" OnSelectedIndexChanged="rblVista_SelectedIndexChanged">
                <asp:ListItem Value="Proximos" Selected="True">Proximos</asp:ListItem>
                <asp:ListItem Value="Historial">Historial</asp:ListItem>
            </asp:RadioButtonList>
            <span id="spanAyudaCancelar" runat="server" class="ayuda-campo"></span>

            <div class="grid-scroll">
                <asp:GridView ID="gvTurnos" runat="server" AutoGenerateColumns="False" CssClass="grid-crud"
                    DataKeyNames="IdTurno" OnRowCommand="gvTurnos_RowCommand">
                    <Columns>
                        <%-- Mismo orden de columnas que la version anterior: las traducciones se guardan por indice (gvTurnos_colN). --%>
                        <asp:BoundField DataField="EspecialidadesTexto" HeaderText="Especialidad" />
                        <asp:BoundField DataField="NombreMedico" HeaderText="Medico" />
                        <asp:BoundField DataField="FechaTexto" HeaderText="Dia" />
                        <asp:BoundField DataField="HoraTexto" HeaderText="Hora" />
                        <asp:TemplateField HeaderText="Estado">
                            <ItemTemplate>
                                <span class='<%# "badge badge-" + Eval("EstadoCss") %>'><%# Eval("EstadoTexto") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Accion">
                            <ItemTemplate>
                                <asp:LinkButton ID="btnCancelarTurno" runat="server" data-i18n="1" CommandName="CancelarTurno"
                                    CommandArgument='<%# Eval("IdTurno") %>' Text="Cancelar Turno" CausesValidation="false"
                                    Visible='<%# (bool)Eval("PuedeCancelar") %>' data-permiso="TURNO_CANCELAR"
                                    OnClientClick="return confirm('Cancelar este turno?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div id="divSinTurnosVista" runat="server" class="sin-datos" data-i18n="1">No hay turnos para mostrar.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <asp:Label ID="lblMensajeMisTurnos" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </div>

    </asp:Panel>

</asp:Content>
