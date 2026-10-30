using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    // Pantalla del paciente: completa sus datos, pide turnos segun la agenda
    // real de los medicos (AgendaMedicos.aspx) y consulta o cancela los suyos.
    public partial class Turnos : PaginaBase
    {
        BLL_Turno bll_turno = new BLL_Turno();
        BLL_Paciente bll_paciente = new BLL_Paciente();
        BLL_Especialidad bll_especialidad = new BLL_Especialidad();

        private const string FormatoFecha = "yyyy-MM-dd";
        private const string FormatoFechaHora = "yyyy-MM-dd HH:mm";

        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

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
                    BuscarMedicos();
                    BindTurnos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Turnos.Page_Load", ex);
                    MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los turnos. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        // ---- Carga de datos ----

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

            // Sin fila en Paciente no se puede reservar (Turno.IdPaciente la referencia).
            pnlSolicitar.Visible = paciente != null;
            spanAyudaDatos.Visible = paciente == null;
        }

        private void CargarEspecialidades()
        {
            ddlEspecialidades.DataSource = bll_especialidad.ObtenerEspecialidades();
            ddlEspecialidades.DataBind();
            ddlEspecialidades.Items.Insert(0, new ListItem(T("Todas"), ""));
        }

        private void BuscarMedicos()
        {
            int? idEspecialidad = null;
            if (int.TryParse(ddlEspecialidades.SelectedValue, out int id))
            {
                idEspecialidad = id;
            }

            string medicoAnterior = ddlMedico.SelectedValue;

            ddlMedico.Items.Clear();

            foreach (BE_Medico medico in bll_turno.BuscarMedicosConAgenda(UsuarioLogueado, txtBuscarNombre.Text, idEspecialidad))
            {
                ddlMedico.Items.Add(new ListItem(medico.NombreApellido + " - " + medico.EspecialidadesTexto, medico.IdMedico.ToString()));
            }

            if (ddlMedico.Items.Count == 0)
            {
                ddlMedico.Items.Add(new ListItem(T("(no hay medicos con turnos para ese criterio)"), ""));
            }
            else if (ddlMedico.Items.FindByValue(medicoAnterior) != null)
            {
                ddlMedico.SelectedValue = medicoAnterior;
            }

            ddlMedico.Enabled = ddlMedico.SelectedValue != "";

            CargarFechas();
        }

        // Llena el combo de dias y el de horarios a partir de una sola consulta
        // de turnos libres del medico seleccionado.
        private void CargarFechas()
        {
            string fechaAnterior = ddlFecha.SelectedValue;

            ddlFecha.Items.Clear();

            List<DateTime> libres = ObtenerTurnosLibresMedicoSeleccionado();

            foreach (IGrouping<DateTime, DateTime> dia in libres.GroupBy(t => t.Date))
            {
                string texto = BE_MedicoHorario.ObtenerNombreDia(BE_MedicoHorario.DiaSemanaDe(dia.Key)) + " " + dia.Key.ToString("dd/MM/yyyy")
                    + " (" + (dia.Count() == 1 ? T("1 turno") : T("{0} turnos", dia.Count())) + ")";

                ddlFecha.Items.Add(new ListItem(texto, dia.Key.ToString(FormatoFecha)));
            }

            if (ddlFecha.Items.FindByValue(fechaAnterior) != null)
            {
                ddlFecha.SelectedValue = fechaAnterior;
            }

            ddlFecha.Enabled = ddlFecha.Items.Count > 0;

            CargarHorarios(libres);
        }

        private void CargarHorarios(List<DateTime> libres)
        {
            rblHorarios.Items.Clear();

            if (DateTime.TryParseExact(ddlFecha.SelectedValue, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                foreach (DateTime turno in libres.Where(t => t.Date == fecha))
                {
                    rblHorarios.Items.Add(new ListItem(turno.ToString("HH:mm"), turno.ToString(FormatoFechaHora)));
                }
            }

            bool hayHorarios = rblHorarios.Items.Count > 0;
            rblHorarios.Visible = hayHorarios;
            pSinHorarios.Visible = !hayHorarios;
            btnAgendar.Enabled = hayHorarios;
        }

        private List<DateTime> ObtenerTurnosLibresMedicoSeleccionado()
        {
            if (!int.TryParse(ddlMedico.SelectedValue, out int idMedico))
            {
                return new List<DateTime>();
            }

            return bll_turno.ObtenerTurnosLibres(UsuarioLogueado, idMedico);
        }

        private void BindTurnos()
        {
            List<BE_Turno> turnos = bll_turno.ObtenerTurnosPaciente(UsuarioLogueado);

            // Proximos: los reservados de hoy en adelante, del mas cercano al mas
            // lejano. Historial: todo lo demas, del mas reciente al mas viejo.
            if (rblVista.SelectedValue == "Historial")
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

        // ---- Datos del paciente ----

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
                MostrarMensaje(lblMensajeDatos, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnGuardarDatos_Click", ex);
                MostrarMensaje(lblMensajeDatos, T("No se pudieron guardar tus datos. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Solicitud de turno ----

        protected void ddlEspecialidades_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                BuscarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.ddlEspecialidades_SelectedIndexChanged", ex);
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
                CargarFechas();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.ddlMedico_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los dias disponibles. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void ddlFecha_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CargarHorarios(ObtenerTurnosLibresMedicoSeleccionado());
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.ddlFecha_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudieron cargar los horarios. Intenta nuevamente mas tarde."), false);
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
                CargarFechas();

                MostrarMensaje(lblMensajeTurno, T("Turno reservado con {0} el {1:dd/MM/yyyy} a las {2:HH:mm} hs.", medico, fechaHora, fechaHora), true);
                MostrarToast(T("Turno reservado."), true);
            }
            catch (InvalidOperationException ex)
            {
                // Si el horario se ocupo mientras tanto, se refrescan las opciones.
                CargarFechasSinFallar();
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
                txtBuscarNombre.Text = string.Empty;
                ddlEspecialidades.SelectedIndex = 0;
                ddlMedico.Items.Clear();
                ddlFecha.Items.Clear();
                BuscarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnCancelar_Click", ex);
                MostrarMensaje(lblMensajeTurno, T("No se pudo limpiar el formulario. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Mis turnos ----

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

                // El horario liberado vuelve a aparecer si es del medico elegido.
                if (pnlSolicitar.Visible)
                {
                    CargarFechas();
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

        // ---- Auxiliares ----

        // Refrescos despues de un error de negocio: si fallan, se registra y se
        // deja el mensaje original en pantalla.
        private void CargarFechasSinFallar()
        {
            try
            {
                CargarFechas();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.CargarFechasSinFallar", ex);
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

        private void MostrarMensaje(Label label, string texto, bool exito)
        {
            // Los mensajes pueden traer nombres cargados por usuarios.
            label.Text = System.Web.HttpUtility.HtmlEncode(texto);
            label.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
        }

        private void MostrarToast(string mensaje, bool exito)
        {
            string clase = exito ? "toast-exito" : "toast-error";
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast " + clase + "';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},3000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},3600);})();";

            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        private void RegistrarErrorInterno(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = Server.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(carpetaLogs);
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
            }
        }
    }
}
