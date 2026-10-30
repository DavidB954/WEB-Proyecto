<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="AgendaMedicos.aspx.cs" Inherits="Presentacion.AgendaMedicos" UnobtrusiveValidationMode="none" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="agenda-layout">

        <%-- ===== Columna izquierda: buscador de medicos + catalogo de especialidades ===== --%>
        <div class="agenda-col">

            <div class="form-container">
                <h2 id="hMedicos" runat="server" data-i18n="1">Medicos</h2>
                <span id="spanAyudaMedicos" runat="server" class="ayuda-campo" data-i18n="1">Se listan los usuarios con rol MEDICO. Selecciona uno para cargar su agenda.</span>

                <div class="filtros-agenda">
                    <div class="campo-filtro">
                        <asp:Label ID="lblBuscarNombre" runat="server" CssClass="label-base" Text="Nombre o apellido" data-i18n="1"></asp:Label>
                        <asp:TextBox ID="txtBuscarNombre" runat="server" CssClass="input-base" MaxLength="60"></asp:TextBox>
                    </div>
                    <div class="campo-filtro">
                        <asp:Label ID="lblFiltroEspecialidad" runat="server" CssClass="label-base" Text="Especialidad" data-i18n="1"></asp:Label>
                        <asp:DropDownList ID="ddlFiltroEspecialidad" runat="server" CssClass="ddl-base"
                            DataTextField="Nombre" DataValueField="IdEspecialidad">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="acciones-filtro">
                    <asp:Button ID="btnBuscar" runat="server" CssClass="btn-base btn-primary" Text="Buscar" data-i18n="1" OnClick="btnBuscar_Click" CausesValidation="false" />
                    <asp:Button ID="btnLimpiarBusqueda" runat="server" CssClass="btn-base btn-warning" Text="Limpiar" data-i18n="1" OnClick="btnLimpiarBusqueda_Click" CausesValidation="false" />
                </div>

                <div class="grid-scroll">
                    <asp:GridView ID="gvMedicos" runat="server" CssClass="grid-crud" AutoGenerateColumns="false"
                        DataKeyNames="IdMedico" OnSelectedIndexChanged="gvMedicos_SelectedIndexChanged">
                        <Columns>
                            <asp:CommandField ShowSelectButton="true" SelectText="Seleccionar" />
                            <asp:BoundField DataField="NombreApellido" HeaderText="Medico" />
                            <asp:BoundField DataField="MatriculaTexto" HeaderText="Matricula" />
                            <asp:BoundField DataField="EspecialidadesTexto" HeaderText="Especialidades" />
                            <asp:BoundField DataField="FranjasActivas" HeaderText="Franjas activas" />
                        </Columns>
                        <SelectedRowStyle CssClass="fila-seleccionada" />
                        <EmptyDataTemplate>
                            <div id="divSinMedicos" runat="server" class="sin-datos" data-i18n="1">No se encontraron medicos con ese criterio.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <div class="form-container" id="divEspecialidades" runat="server">
                <h2 id="hEspecialidades" runat="server" data-i18n="1">Especialidades</h2>

                <div class="fila-inline" id="divAltaEspecialidad" runat="server" data-permiso="ESPECIALIDAD_ABM">
                    <asp:TextBox ID="txtNuevaEspecialidad" runat="server" CssClass="input-base" MaxLength="30" placeholder="Nueva especialidad" data-i18n="1"></asp:TextBox>
                    <asp:Button ID="btnAgregarEspecialidad" runat="server" CssClass="btn-base btn-success" Text="Agregar" data-i18n="1" OnClick="btnAgregarEspecialidad_Click" CausesValidation="false" data-permiso="ESPECIALIDAD_ABM" />
                </div>

                <div class="grid-scroll">
                    <asp:GridView ID="gvEspecialidades" runat="server" CssClass="grid-crud" AutoGenerateColumns="false"
                        DataKeyNames="IdEspecialidad" OnRowCommand="gvEspecialidades_RowCommand">
                        <Columns>
                            <asp:BoundField DataField="Nombre" HeaderText="Especialidad" />
                            <asp:TemplateField HeaderText="Accion">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEliminarEspecialidad" runat="server" data-i18n="1" CommandName="Eliminar"
                                        CommandArgument='<%# Eval("IdEspecialidad") %>' Text="Eliminar" CausesValidation="false"
                                        data-permiso="ESPECIALIDAD_ABM" OnClientClick="return confirm('Eliminar esta especialidad?');" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div id="divSinEspecialidades" runat="server" class="sin-datos" data-i18n="1">Todavia no hay especialidades cargadas.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>

                <asp:Label ID="lblMensajeEspecialidad" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>
            </div>

        </div>

        <%-- ===== Columna derecha: agenda del medico seleccionado ===== --%>
        <div class="agenda-col">

            <asp:Panel ID="pnlSinMedico" runat="server" CssClass="form-container">
                <p id="pSinMedico" runat="server" class="sin-datos" data-i18n="1">Selecciona un medico de la lista para ver y editar sus especialidades y horarios.</p>
            </asp:Panel>

            <asp:Panel ID="pnlMedico" runat="server" Visible="false" CssClass="form-container">

                <asp:Label ID="lblMedicoSeleccionado" runat="server" CssClass="paciente-seleccionado"></asp:Label>
                <asp:Label ID="lblMensaje" runat="server" CssClass="msg-form" EnableViewState="false"></asp:Label>

                <%-- Datos del medico --%>
                <div class="section-card">
                    <h3 id="hDatosMedico" runat="server" data-i18n="1">Matricula y especialidades</h3>

                    <asp:Label ID="lblMatricula" runat="server" CssClass="label-base" Text="Matricula" data-i18n="1"></asp:Label>
                    <asp:TextBox ID="txtMatricula" runat="server" CssClass="input-base" MaxLength="9" TextMode="Number"></asp:TextBox>

                    <asp:Label ID="lblEspecialidadesMedico" runat="server" CssClass="label-base" Text="Especialidades que atiende" data-i18n="1"></asp:Label>
                    <span id="spanAyudaEspecialidades" runat="server" class="ayuda-campo" data-i18n="1">Marca o destilda especialidades y presiona "Guardar datos": el cambio aplica solo a este medico. Tiene que quedar al menos una.</span>
                    <asp:CheckBoxList ID="cblEspecialidades" runat="server" CssClass="lista-opciones"
                        RepeatLayout="Flow" RepeatDirection="Horizontal"
                        DataTextField="Nombre" DataValueField="IdEspecialidad">
                    </asp:CheckBoxList>

                    <div class="abm-actions">
                        <asp:Button ID="btnGuardarDatos" runat="server" CssClass="btn-base btn-success" Text="Guardar datos" data-i18n="1"
                            OnClick="btnGuardarDatos_Click" CausesValidation="false" data-permiso="MEDICO_DATOS_EDITAR" />
                    </div>
                </div>

                <%-- Franjas semanales --%>
                <div class="section-card">
                    <h3 id="hHorarios" runat="server" data-i18n="1">Horarios de atencion semanales</h3>
                    <span id="spanAyudaHorarios" runat="server" class="ayuda-campo" data-i18n="1">Marca uno o varios dias para cargarles la misma franja. Los turnos se generan cada "Duracion" minutos dentro del rango.</span>

                    <div id="divAltaHorario" runat="server" data-permiso="AGENDA_HORARIO_EDITAR">
                        <asp:Label ID="lblDias" runat="server" CssClass="label-base" Text="Dias" data-i18n="1"></asp:Label>
                        <asp:CheckBoxList ID="cblDias" runat="server" CssClass="lista-opciones"
                            RepeatLayout="Flow" RepeatDirection="Horizontal">
                        </asp:CheckBoxList>

                        <div class="horario-franja">
                            <div>
                                <asp:Label ID="lblHoraDesde" runat="server" CssClass="label-base" Text="Desde" data-i18n="1"></asp:Label>
                                <asp:DropDownList ID="ddlHoraDesde" runat="server" CssClass="ddl-base"></asp:DropDownList>
                            </div>
                            <div>
                                <asp:Label ID="lblHoraHasta" runat="server" CssClass="label-base" Text="Hasta" data-i18n="1"></asp:Label>
                                <asp:DropDownList ID="ddlHoraHasta" runat="server" CssClass="ddl-base"></asp:DropDownList>
                            </div>
                            <div>
                                <asp:Label ID="lblDuracion" runat="server" CssClass="label-base" Text="Duracion del turno" data-i18n="1"></asp:Label>
                                <asp:DropDownList ID="ddlDuracion" runat="server" CssClass="ddl-base"></asp:DropDownList>
                            </div>
                        </div>

                        <div class="abm-actions">
                            <asp:Button ID="btnAgregarHorario" runat="server" CssClass="btn-base btn-success" Text="Agregar franja" data-i18n="1"
                                OnClick="btnAgregarHorario_Click" CausesValidation="false" data-permiso="AGENDA_HORARIO_EDITAR" />
                        </div>
                    </div>

                    <div class="grid-scroll">
                        <asp:GridView ID="gvHorarios" runat="server" CssClass="grid-crud" AutoGenerateColumns="false"
                            DataKeyNames="IdHorario" OnRowCommand="gvHorarios_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="NombreDia" HeaderText="Dia" />
                                <asp:BoundField DataField="HorarioTexto" HeaderText="Horario" />
                                <asp:BoundField DataField="DuracionTurno" HeaderText="Duracion (min)" />
                                <asp:BoundField DataField="CantidadTurnos" HeaderText="Turnos" />
                                <asp:TemplateField HeaderText="Estado">
                                    <ItemTemplate>
                                        <span class='<%# "badge badge-" + Eval("EstadoCss") %>'><%# Eval("Estado") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Accion">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkDesactivar" runat="server" data-i18n="1" CommandName="Desactivar"
                                            CommandArgument='<%# Eval("IdHorario") %>' Text="Desactivar" CausesValidation="false"
                                            Visible='<%# (bool)Eval("Activo") %>' data-permiso="AGENDA_HORARIO_EDITAR"
                                            OnClientClick="return confirm('Desactivar esta franja horaria? Se cancelan los turnos reservados que caen en ella.');" />
                                        <asp:LinkButton ID="lnkActivar" runat="server" data-i18n="1" CommandName="Activar"
                                            CommandArgument='<%# Eval("IdHorario") %>' Text="Activar" CausesValidation="false"
                                            Visible='<%# !(bool)Eval("Activo") %>' data-permiso="AGENDA_HORARIO_EDITAR" />
                                        <asp:LinkButton ID="lnkEliminarHorario" runat="server" data-i18n="1" CommandName="Eliminar"
                                            CommandArgument='<%# Eval("IdHorario") %>' Text="Eliminar" CausesValidation="false"
                                            data-permiso="AGENDA_HORARIO_EDITAR" OnClientClick="return confirm('Eliminar esta franja horaria? Se cancelan los turnos reservados que caen en ella.');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div id="divSinHorarios" runat="server" class="sin-datos" data-i18n="1">El medico todavia no tiene horarios cargados.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <%-- Bloqueos puntuales --%>
                <div class="section-card">
                    <h3 id="hBloqueos" runat="server" data-i18n="1">Dias y horarios no disponibles</h3>
                    <span id="spanAyudaBloqueos" runat="server" class="ayuda-campo" data-i18n="1">Para vacaciones, licencias o ausencias puntuales. Durante el bloqueo no se ofrecen turnos aunque caiga en una franja activa.</span>

                    <div id="divAltaBloqueo" runat="server" data-permiso="AGENDA_BLOQUEO_EDITAR">
                        <div class="horario-franja">
                            <div>
                                <asp:Label ID="lblFechaBloqueo" runat="server" CssClass="label-base" Text="Fecha" data-i18n="1"></asp:Label>
                                <asp:TextBox ID="txtFechaBloqueo" runat="server" CssClass="input-base" TextMode="Date"></asp:TextBox>
                            </div>
                            <div>
                                <asp:Label ID="lblBloqueoDesde" runat="server" CssClass="label-base" Text="Desde" data-i18n="1"></asp:Label>
                                <asp:DropDownList ID="ddlBloqueoDesde" runat="server" CssClass="ddl-base"></asp:DropDownList>
                            </div>
                            <div>
                                <asp:Label ID="lblBloqueoHasta" runat="server" CssClass="label-base" Text="Hasta" data-i18n="1"></asp:Label>
                                <asp:DropDownList ID="ddlBloqueoHasta" runat="server" CssClass="ddl-base"></asp:DropDownList>
                            </div>
                        </div>

                        <asp:CheckBox ID="chkDiaCompleto" runat="server" Text="Bloquear el dia completo" data-i18n="1" CssClass="chk-reset"
                            Checked="true" AutoPostBack="true" OnCheckedChanged="chkDiaCompleto_CheckedChanged" />

                        <asp:Label ID="lblMotivoBloqueo" runat="server" CssClass="label-base" Text="Motivo (opcional)" data-i18n="1"></asp:Label>
                        <asp:TextBox ID="txtMotivoBloqueo" runat="server" CssClass="input-base" MaxLength="100"></asp:TextBox>

                        <div class="abm-actions">
                            <asp:Button ID="btnAgregarBloqueo" runat="server" CssClass="btn-base btn-danger" Text="Bloquear" data-i18n="1"
                                OnClick="btnAgregarBloqueo_Click" CausesValidation="false" data-permiso="AGENDA_BLOQUEO_EDITAR"
                                OnClientClick="return confirm('Bloquear la agenda? Se cancelan los turnos reservados en ese horario.');" />
                        </div>
                    </div>

                    <div class="grid-scroll">
                        <asp:GridView ID="gvBloqueos" runat="server" CssClass="grid-crud" AutoGenerateColumns="false"
                            DataKeyNames="IdBloqueo" OnRowCommand="gvBloqueos_RowCommand">
                            <Columns>
                                <asp:BoundField DataField="FechaTexto" HeaderText="Fecha" />
                                <asp:BoundField DataField="HorarioTexto" HeaderText="Horario" />
                                <asp:BoundField DataField="Motivo" HeaderText="Motivo" />
                                <asp:TemplateField HeaderText="Accion">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lnkQuitarBloqueo" runat="server" data-i18n="1" CommandName="Quitar"
                                            CommandArgument='<%# Eval("IdBloqueo") %>' Text="Quitar" CausesValidation="false"
                                            data-permiso="AGENDA_BLOQUEO_EDITAR" OnClientClick="return confirm('Quitar este bloqueo?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div id="divSinBloqueos" runat="server" class="sin-datos" data-i18n="1">No hay bloqueos de hoy en adelante.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

            </asp:Panel>
        </div>

    </div>

</asp:Content>
