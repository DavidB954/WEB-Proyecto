using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Turnos : PaginaBase
    {
        BLL_Turno bll_turno = new BLL_Turno();
        BLL_Paciente bll_paciente = new BLL_Paciente();
        BLL_Especialidad bll_especialidad = new BLL_Especialidad();

        private const string FormatoFecha = "yyyy-MM-dd";
        private const string FormatoFechaHora = "yyyy-MM-dd HH:mm";
        private const int IndiceColumnaAccion = 5;
        private const string BuscarPorProfesional = "Profesional";

        private static readonly string[] NombresMeses =
        {
            "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio", "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre"
        };

        private DateTime? DiaElegido
        {
            get { return ViewState["DiaElegido"] as DateTime?; }
            set { ViewState["DiaElegido"] = value; }
        }

        private DateTime MesVisible
        {
            get { return ViewState["MesVisible"] as DateTime? ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); }
            set { ViewState["MesVisible"] = new DateTime(value.Year, value.Month, 1); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    spanAyudaCancelar.InnerText = T("Podes cancelar un turno hasta {0} horas antes.", BLL_Turno.HorasMinimasCancelacion);

                    if (!bll_paciente.EsPaciente(UsuarioLogueado.IdUsuario))
                    {
                        pnlPaciente.Visible = false;
                        pnlSinRolPaciente.Visible = true;
                        return;
                    }

                    CargarDatosPaciente();
                    CargarEspecialidades();
                    BindTurnos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Turnos.Page_Load", ex);
                    MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los turnos. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void CargarDatosPaciente()
        {
            BE_Paciente paciente = bll_paciente.ObtenerPaciente(UsuarioLogueado.IdUsuario);

            if (paciente == null)
            {
                lblHistoriaClinica.Text = System.Web.HttpUtility.HtmlEncode(T("{0} - todavia no completaste tus datos.", UsuarioLogueado.NombreApellido));
                txtFechaNacimiento.Text = string.Empty;
                txtTelefono.Text = string.Empty;
            }
            else
            {
                lblHistoriaClinica.Text = System.Web.HttpUtility.HtmlEncode(T("{0} - Historia clinica {1}", UsuarioLogueado.NombreApellido, paciente.NumeroHistoriaClinica));
                txtFechaNacimiento.Text = paciente.FechaNacimiento.ToString(FormatoFecha);
                txtTelefono.Text = paciente.Telefono ?? string.Empty;
            }

            pnlSolicitar.Visible = paciente != null;
            spanAyudaDatos.Visible = paciente == null;

            pnlPaciente.CssClass = paciente == null ? "turnos-layout turnos-datos-pendientes" : "turnos-layout";

            if (paciente == null)
            {
                detMisDatos.Attributes["class"] = "datos-plegables datos-obligatorios";
                AbrirMisDatos();
            }
            else
            {
                detMisDatos.Attributes["class"] = "datos-plegables";
                detMisDatos.Attributes.Remove("open");
            }
        }

        private void AbrirMisDatos()
        {
            detMisDatos.Attributes["open"] = "open";
        }

        private void CargarEspecialidades()
        {
            rblEspecialidades.DataSource = bll_especialidad.ObtenerEspecialidades();
            rblEspecialidades.DataBind();
        }

        private void OcultarAgenda()
        {
            DiaElegido = null;
            pnlProfesional.Visible = false;
            pnlAgenda.Visible = false;
        }

        private void BuscarMedicos()
        {
            int? idEspecialidad = null;
            string nombre = null;

            if (rblBuscarPor.SelectedValue == BuscarPorProfesional)
            {
                nombre = txtBuscarProfesional.Text;
            }
            else if (int.TryParse(rblEspecialidades.SelectedValue, out int id))
            {
                idEspecialidad = id;
            }
            else
            {
                OcultarAgenda();
                return;
            }

            ddlMedico.Items.Clear();

            foreach (BE_Medico medico in bll_turno.BuscarMedicosParaTurno(UsuarioLogueado, nombre, idEspecialidad))
            {
                ddlMedico.Items.Add(new ListItem(medico.NombreApellido + " - " + medico.EspecialidadesTexto, medico.IdMedico.ToString()));
            }

            if (ddlMedico.Items.Count == 0)
            {
                ddlMedico.Items.Add(new ListItem(T("(no hay medicos con turnos para ese criterio)"), ""));
            }

            ddlMedico.Enabled = ddlMedico.SelectedValue != "";
            pnlProfesional.Visible = true;

            DiaElegido = null;
            CargarAgenda();
        }

        private void CargarAgenda()
        {
            if (!int.TryParse(ddlMedico.SelectedValue, out int idMedico))
            {
                pnlAgenda.Visible = false;
                return;
            }

            BE_AgendaDisponible agenda = bll_turno.ObtenerAgendaDisponible(UsuarioLogueado, idMedico);

            pnlAgenda.Visible = true;

            if (DiaElegido == null || !agenda.TurnosLibres.Exists(t => t.Date == DiaElegido))
            {
                DiaElegido = agenda.TurnosLibres.Count > 0 ? agenda.TurnosLibres[0].Date : (DateTime?)null;
                MesVisible = DiaElegido ?? DateTime.Today;
            }

            if (agenda.TurnosLibres.Count > 0)
            {
                DateTime primero = agenda.TurnosLibres[0];

                lblPrimerTurno.Text = System.Web.HttpUtility.HtmlEncode(T("Primer turno disponible: {0} {1:dd/MM/yyyy} a las {2:HH:mm} hs.",
                    BE_MedicoHorario.ObtenerNombreDia(BE_MedicoHorario.DiaSemanaDe(primero)), primero, primero));
            }

            lblPrimerTurno.Visible = agenda.TurnosLibres.Count > 0;

            BindCalendario(agenda);
            CargarHorarios(agenda.TurnosLibres);
        }

        private void BindCalendario(BE_AgendaDisponible agenda)
        {
            DateTime mes = MesVisible;
            HashSet<DateTime> diasConTurnos = new HashSet<DateTime>(agenda.TurnosLibres.Select(t => t.Date));

            lblMes.Text = T(NombresMeses[mes.Month - 1]) + " " + mes.Year;

            btnMesAnterior.Enabled = mes > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            btnMesSiguiente.Enabled = mes.AddMonths(1) <= DateTime.Today.AddDays(BLL_Turno.DiasAnticipacionMaxima);

            var dias = Enumerable.Repeat(new { Valor = "", Numero = "", Css = "calendario-vacio", Titulo = "", Elegible = false }, BE_MedicoHorario.DiaSemanaDe(mes) - 1).ToList();

            for (DateTime fecha = mes; fecha.Month == mes.Month; fecha = fecha.AddDays(1))
            {
                bool conTurnos = diasConTurnos.Contains(fecha);
                bool atiende = agenda.DiasDeAtencion.Contains(fecha);

                string css = conTurnos ? "dia-con-turnos" : atiende ? "dia-sin-turnos" : "dia-sin-atencion";

                if (fecha == DiaElegido)
                {
                    css += " dia-elegido";
                }

                dias.Add(new
                {
                    Valor = fecha.ToString(FormatoFecha),
                    Numero = fecha.Day.ToString(),
                    Css = "calendario-dia " + css,
                    Titulo = conTurnos ? T("Hay turnos disponibles") : atiende ? T("El medico atiende este dia, pero no hay turnos disponibles") : string.Empty,
                    Elegible = conTurnos
                });
            }

            rptDias.DataSource = dias;
            rptDias.DataBind();
        }

        private void CargarHorarios(List<DateTime> libres)
        {
            rblHorarios.Items.Clear();

            foreach (DateTime turno in libres.Where(t => t.Date == DiaElegido))
            {
                rblHorarios.Items.Add(new ListItem(turno.ToString("HH:mm"), turno.ToString(FormatoFechaHora)));
            }

            bool hayHorarios = rblHorarios.Items.Count > 0;

            if (hayHorarios)
            {
                rblHorarios.SelectedIndex = 0;

                DateTime dia = DiaElegido.Value;

                lblHoraTurno.Text = System.Web.HttpUtility.HtmlEncode(T("Horarios del {0} {1:dd/MM/yyyy}:", BE_MedicoHorario.ObtenerNombreDia(BE_MedicoHorario.DiaSemanaDe(dia)), dia));
            }

            lblHoraTurno.Visible = hayHorarios;
            rblHorarios.Visible = hayHorarios;
            pSinHorarios.Visible = !hayHorarios;
            btnAgendar.Enabled = hayHorarios;
        }

        private void BindTurnos()
        {
            List<BE_Turno> turnos = bll_turno.ObtenerTurnosPaciente(UsuarioLogueado);

            bool esHistorial = rblVista.SelectedValue == "Historial";
            gvTurnos.Columns[IndiceColumnaAccion].Visible = !esHistorial;
            spanAyudaCancelar.Visible = !esHistorial;

            if (esHistorial)
            {
                gvTurnos.DataSource = turnos
                    .Where(t => !(t.Estado == EstadoTurno.Reservado && t.FechaHora.Date >= DateTime.Today))
                    .OrderByDescending(t => t.FechaHora)
                    .ToList();
            }
            else
            {
                gvTurnos.DataSource = turnos
                    .Where(t => t.Estado == EstadoTurno.Reservado && t.FechaHora.Date >= DateTime.Today)
                    .OrderBy(t => t.FechaHora)
                    .ToList();
            }

            gvTurnos.DataBind();
        }

        protected void btnGuardarDatos_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("TURNO_ALTA"))
            {
                return;
            }

            try
            {
                if (!DateTime.TryParseExact(txtFechaNacimiento.Text, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaNacimiento))
                {
                    AbrirMisDatos();
                    MostrarMensaje(lblMensajeDatos, T("Ingresa una fecha de nacimiento valida."), false);
                    return;
                }

                bll_paciente.GuardarDatos(UsuarioLogueado, fechaNacimiento, txtTelefono.Text);

                CargarDatosPaciente();
                MostrarMensaje(lblMensajeDatos, T("Tus datos se guardaron correctamente."), true);
                MostrarToast(T("Datos guardados."), true);
            }
            catch (InvalidOperationException ex)
            {
                AbrirMisDatos();
                MostrarMensaje(lblMensajeDatos, ex.Message, false);
            }
            catch (Exception ex)
            {
                AbrirMisDatos();
                RegistrarErrorInterno("Turnos.btnGuardarDatos_Click", ex);
                MostrarMensaje(lblMensajeDatos, T("No se pudieron guardar tus datos. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void rblBuscarPor_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                bool porProfesional = rblBuscarPor.SelectedValue == BuscarPorProfesional;

                pnlBuscarEspecialidad.Visible = !porProfesional;
                pnlBuscarProfesional.Visible = porProfesional;

                BuscarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.rblBuscarPor_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudo realizar la busqueda. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void rblEspecialidades_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                BuscarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.rblEspecialidades_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los medicos de esa especialidad. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                BuscarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnBuscar_Click", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudo realizar la busqueda. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void ddlMedico_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DiaElegido = null;
                CargarAgenda();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.ddlMedico_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los dias disponibles. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void rptDias_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "ElegirDia" && DateTime.TryParseExact(Convert.ToString(e.CommandArgument), FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dia))
                {
                    DiaElegido = dia;
                    CargarAgenda();
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.rptDias_ItemCommand", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los horarios. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnMesAnterior_Click(object sender, EventArgs e)
        {
            CambiarMes(-1);
        }

        protected void btnMesSiguiente_Click(object sender, EventArgs e)
        {
            CambiarMes(1);
        }

        private void CambiarMes(int meses)
        {
            try
            {
                MesVisible = MesVisible.AddMonths(meses);
                BindCalendario(bll_turno.ObtenerAgendaDisponible(UsuarioLogueado, int.Parse(ddlMedico.SelectedValue)));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.CambiarMes", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los dias disponibles. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnAgendar_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("TURNO_ALTA"))
            {
                return;
            }

            try
            {
                if (!int.TryParse(ddlMedico.SelectedValue, out int idMedico))
                {
                    MostrarMensaje(lblMensajeTurno, T("Selecciona un medico."), false);
                    return;
                }

                if (!DateTime.TryParseExact(rblHorarios.SelectedValue, FormatoFechaHora, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fechaHora))
                {
                    MostrarMensaje(lblMensajeTurno, T("Selecciona un horario."), false);
                    return;
                }

                bll_turno.SolicitarTurno(UsuarioLogueado, idMedico, fechaHora);

                string medico = ddlMedico.SelectedItem.Text;

                rblVista.SelectedValue = "Proximos";
                BindTurnos();
                CargarAgenda();

                MostrarMensaje(lblMensajeTurno, T("Turno reservado con {0} el {1:dd/MM/yyyy} a las {2:HH:mm} hs.", medico, fechaHora, fechaHora), true);
                MostrarToast(T("Turno reservado."), true);
            }
            catch (InvalidOperationException ex)
            {
                CargarAgendaSinFallar();
                MostrarMensaje(lblMensajeTurno, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnAgendar_Click", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudo reservar el turno. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                txtBuscarEspecialidad.Text = string.Empty;
                txtBuscarProfesional.Text = string.Empty;
                rblEspecialidades.ClearSelection();

                if (rblBuscarPor.SelectedValue == BuscarPorProfesional)
                {
                    BuscarMedicos();
                }
                else
                {
                    OcultarAgenda();
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnCancelar_Click", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudo limpiar el formulario. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void rblVista_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                BindTurnos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.rblVista_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeMisTurnos, T("No se pudieron cargar los turnos. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvTurnos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "CancelarTurno")
            {
                return;
            }

            if (!ExigirPermiso("TURNO_CANCELAR"))
            {
                return;
            }

            try
            {
                if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idTurno))
                {
                    MostrarMensaje(lblMensajeMisTurnos, T("El turno seleccionado ya no esta disponible."), false);
                    return;
                }

                bll_turno.CancelarTurno(UsuarioLogueado, idTurno);

                BindTurnos();

                if (pnlSolicitar.Visible)
                {
                    CargarAgenda();
                }

                MostrarMensaje(lblMensajeMisTurnos, T("El turno fue cancelado."), true);
                MostrarToast(T("Turno cancelado."), true);
            }
            catch (InvalidOperationException ex)
            {
                BindTurnosSinFallar();
                MostrarMensaje(lblMensajeMisTurnos, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.gvTurnos_RowCommand", ex);
                MostrarMensaje(lblMensajeMisTurnos, T("No se pudo cancelar el turno. Intenta nuevamente mas tarde."), false);
            }
        }

        private void CargarAgendaSinFallar()
        {
            try
            {
                CargarAgenda();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.CargarAgendaSinFallar", ex);
            }
        }

        private void BindTurnosSinFallar()
        {
            try
            {
                BindTurnos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.BindTurnosSinFallar", ex);
            }
        }
    }
}
