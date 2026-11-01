<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Turnos.aspx.cs" Inherits="Presentacion.Turnos" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script type="text/javascript">
        document.addEventListener('DOMContentLoaded', function () {
            var buscador = document.querySelector('.js-buscar-especialidad');

            if (!buscador) {
                return;
            }

            function normalizar(texto) {
                return texto.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
            }

            function filtrar() {
                var busqueda = normalizar(buscador.value.trim());
                var hayCoincidencias = false;

                document.querySelectorAll('.lista-especialidades label').forEach(function (opcion) {
                    var coincide = normalizar(opcion.textContent).indexOf(busqueda) >= 0;

                    opcion.style.display = coincide ? '' : 'none';
                    hayCoincidencias = hayCoincidencias || coincide;
                });

                document.querySelector('.js-sin-especialidades').style.display = hayCoincidencias ? 'none' : '';
            }

            buscador.addEventListener('input', filtrar);

            buscador.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                }
            });

            filtrar();
        });
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <asp:Panel ID="pnlSinRolPaciente" runat="server" Visible="false" CssClass="form-container">
        <p id="pSinRolPaciente" runat="server" class="sin-datos" data-i18n="1">Esta pantalla es para pacientes. Tu usuario no tiene el rol PACIENTE.</p>
    </asp:Panel>

    <asp:Panel ID="pnlPaciente" runat="server" CssClass="turnos-layout">

        <asp:Panel ID="pnlSolicitar" runat="server" CssClass="form-container crear-turno">
            <h2 id="hSolicitarTurno" runat="server" data-i18n="1">Solicitar turno</h2>

            <asp:RadioButtonList ID="rblBuscarPor" runat="server" data-i18n="1" CssClass="lista-opciones opciones-texto"
                RepeatLayout="Flow" RepeatDirection="Horizontal"
                AutoPostBack="true" OnSelectedIndexChanged="rblBuscarPor_SelectedIndexChanged">
                <asp:ListItem Value="Especialidad" Selected="True">Especialidad medica</asp:ListItem>
                <asp:ListItem Value="Profesional">Profesional</asp:ListItem>
            </asp:RadioButtonList>

            <asp:Panel ID="pnlBuscarEspecialidad" runat="server">
                <asp:TextBox ID="txtBuscarEspecialidad" runat="server" CssClass="input-base js-buscar-especialidad" MaxLength="60"
                    data-i18n="1" placeholder="Buscar especialidad"></asp:TextBox>
                <asp:RadioButtonList ID="rblEspecialidades" runat="server" CssClass="lista-opciones opciones-texto lista-especialidades"
                    DataTextField="Nombre" DataValueField="IdEspecialidad" RepeatLayout="Flow" RepeatDirection="Horizontal"
                    AutoPostBack="true" OnSelectedIndexChanged="rblEspecialidades_SelectedIndexChanged">
                </asp:RadioButtonList>
                <p id="pSinEspecialidades" runat="server" class="sin-datos js-sin-especialidades" style="display:none" data-i18n="1">No hay especialidades con ese nombre.</p>
            </asp:Panel>

            <asp:Panel ID="pnlBuscarProfesional" runat="server" Visible="false" DefaultButton="btnBuscar">
                <div class="fila-inline">
                    <asp:TextBox ID="txtBuscarProfesional" runat="server" CssClass="input-base" MaxLength="60"
                        data-i18n="1" placeholder="Buscar profesional"></asp:TextBox>
                    <asp:Button ID="btnBuscar" runat="server" CssClass="btn-base btn-primary" Text="Buscar" data-i18n="1" OnClick="btnBuscar_Click" CausesValidation="false" />
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlProfesional" runat="server" Visible="false">
                <asp:Label ID="lblProfesional" runat="server" CssClass="label-base" Text="Profesional" data-i18n="1"></asp:Label>
                <asp:DropDownList ID="ddlMedico" runat="server" CssClass="ddl-base"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlMedico_SelectedIndexChanged">
                </asp:DropDownList>
            </asp:Panel>

            <asp:Panel ID="pnlAgenda" runat="server" Visible="false" CssClass="agenda-turno">
                <div class="calendario">
                    <div class="calendario-cabecera">
                        <asp:LinkButton ID="btnMesAnterior" runat="server" CssClass="calendario-mes" Text="&lsaquo;" OnClick="btnMesAnterior_Click" CausesValidation="false" />
                        <asp:Label ID="lblMes" runat="server" CssClass="calendario-titulo"></asp:Label>
                        <asp:LinkButton ID="btnMesSiguiente" runat="server" CssClass="calendario-mes" Text="&rsaquo;" OnClick="btnMesSiguiente_Click" CausesValidation="false" />
                    </div>
                    <div class="calendario-grilla">
                        <span id="spanLun" runat="server" class="calendario-semana" data-i18n="1">Lun</span>
                        <span id="spanMar" runat="server" class="calendario-semana" data-i18n="1">Mar</span>
                        <span id="spanMie" runat="server" class="calendario-semana" data-i18n="1">Mie</span>
                        <span id="spanJue" runat="server" class="calendario-semana" data-i18n="1">Jue</span>
                        <span id="spanVie" runat="server" class="calendario-semana" data-i18n="1">Vie</span>
                        <span id="spanSab" runat="server" class="calendario-semana" data-i18n="1">Sab</span>
                        <span id="spanDom" runat="server" class="calendario-semana" data-i18n="1">Dom</span>
                        <asp:Repeater ID="rptDias" runat="server" OnItemCommand="rptDias_ItemCommand">
                            <ItemTemplate>
                                <asp:LinkButton runat="server" CommandName="ElegirDia" CommandArgument='<%# Eval("Valor") %>' CausesValidation="false"
                                    Visible='<%# (bool)Eval("Elegible") %>' CssClass='<%# Eval("Css") %>' Text='<%# Eval("Numero") %>' ToolTip='<%# Eval("Titulo") %>' />
                                <asp:Label runat="server" Visible='<%# !(bool)Eval("Elegible") %>' CssClass='<%# Eval("Css") %>' Text='<%# Eval("Numero") %>' ToolTip='<%# Eval("Titulo") %>' />
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="calendario-referencias">
                        <span class="calendario-referencia"><i class="calendario-dia dia-con-turnos" aria-hidden="true"></i><span id="spanRefConTurnos" runat="server" data-i18n="1">Con turnos</span></span>
                        <span class="calendario-referencia"><i class="calendario-dia dia-sin-turnos" aria-hidden="true"></i><span id="spanRefSinTurnos" runat="server" data-i18n="1">Atiende, sin turnos</span></span>
                    </div>
                </div>

                <div class="agenda-horarios">
                    <asp:Label ID="lblPrimerTurno" runat="server" CssClass="paciente-seleccionado"></asp:Label>

                    <asp:Label ID="lblHoraTurno" runat="server" CssClass="label-base"></asp:Label>
                    <asp:RadioButtonList ID="rblHorarios" runat="server" CssClass="lista-opciones"
                        RepeatLayout="Flow" RepeatDirection="Horizontal">
                    </asp:RadioButtonList>
                    <p id="pSinHorarios" runat="server" class="sin-datos" visible="false" data-i18n="1">No hay turnos libres para el medico seleccionado.</p>

                    <div class="acciones-turno">
                        <asp:Button ID="btnAgendar" runat="server" CssClass="btn-base btn-success" Text="Agendar Turno" data-i18n="1"
                            OnClick="btnAgendar_Click" CausesValidation="false" data-permiso="TURNO_ALTA"
                            OnClientClick="return confirmarAccion(this, 'Confirmas la reserva de este turno?');" />
                        <asp:Button ID="btnCancelar" runat="server" CssClass="btn-base btn-warning" Text="Limpiar" data-i18n="1"
                            OnClick="btnCancelar_Click" CausesValidation="false" />
                    </div>
                </div>
            </asp:Panel>

            <asp:Label ID="lblMensajeTurno" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </asp:Panel>

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
                                    OnClientClick="return confirmarAccion(this, 'Cancelar este turno?');" />
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

        <div class="form-container mis-datos">
            <h2 id="hMisDatos" runat="server" data-i18n="1">Mis datos</h2>

            <asp:Label ID="lblHistoriaClinica" runat="server" CssClass="paciente-seleccionado"></asp:Label>

            <details id="detMisDatos" runat="server" class="datos-plegables">
                <summary id="sumEditarDatos" runat="server" data-i18n="1">Editar mis datos</summary>

                <span id="spanAyudaDatos" runat="server" class="ayuda-campo" data-i18n="1">Completa tu fecha de nacimiento para poder pedir turnos. El telefono es opcional.</span>

                <asp:Label ID="lblFechaNacimiento" runat="server" CssClass="label-base" Text="Fecha de nacimiento" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtFechaNacimiento" runat="server" CssClass="input-base" TextMode="Date"></asp:TextBox>

                <asp:Label ID="lblTelefono" runat="server" CssClass="label-base" Text="Telefono" data-i18n="1"></asp:Label>
                <asp:TextBox ID="txtTelefono" runat="server" CssClass="input-base" MaxLength="20"></asp:TextBox>

                <div class="acciones-turno">
                    <asp:Button ID="btnGuardarDatos" runat="server" CssClass="btn-base btn-success" Text="Guardar mis datos" data-i18n="1"
                        OnClick="btnGuardarDatos_Click" CausesValidation="false" data-permiso="TURNO_ALTA" />
                </div>
            </details>

            <asp:Label ID="lblMensajeDatos" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
        </div>

    </asp:Panel>

</asp:Content>
