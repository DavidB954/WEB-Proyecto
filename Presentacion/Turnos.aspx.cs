using BE;
using BE.Seguridad;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Turnos : PaginaBase
    {
        private static readonly Dictionary<string, string[]> MedicosPorEspecialidad = new Dictionary<string, string[]>
        {
            { "Cardiologia",    new[] { "Dr. Perez",   "Dra. Salinas" } },
            { "Clinica Medica", new[] { "Dra. Romero", "Dr. Aguirre" } },
            { "Dermatologia",   new[] { "Dra. Gomez",  "Dr. Ferreyra" } },
            { "Pediatria",      new[] { "Dr. Lopez",   "Dra. Bianchi" } },
            { "Traumatologia",  new[] { "Dr. Sosa",    "Dra. Vega" } }
        };

        private static readonly string[] DiasDisponibles = { "Lunes", "Martes", "Miercoles", "Jueves", "Viernes" };
        private static readonly string[] HorasDisponibles = { "08:00", "09:00", "10:00", "11:00", "12:00", "14:00", "15:00", "16:00", "17:00" };

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_TURNOS"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarEspecialidades();
                    CargarMedicos();
                    CargarDias();
                    CargarHoras();
                    BindTurnos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Turnos.Page_Load", ex);
                    MostrarMensaje("No se pudieron cargar los turnos. Intenta nuevamente mas tarde.", false);
                }
            }
        }

        private DataTable TurnosDelPaciente
        {
            get
            {
                DataTable dt = Session["TurnosPaciente"] as DataTable;
                if (dt == null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Especialidad");
                    dt.Columns.Add("Medico");
                    dt.Columns.Add("Dia");
                    dt.Columns.Add("Hora");
                    dt.Columns.Add("Estado");

                    dt.Rows.Add("Cardiologia", "Dr. Perez", "Lunes", "10:00", "Confirmado");
                    dt.Rows.Add("Dermatologia", "Dra. Gomez", "Martes", "11:00", "Confirmado");
                    dt.Rows.Add("Pediatria", "Dr. Lopez", "Miercoles", "14:00", "Pendiente");

                    Session["TurnosPaciente"] = dt;
                }
                return dt;
            }
        }

        private void CargarEspecialidades()
        {
            ddlEspecialidades.Items.Clear();
            foreach (string especialidad in MedicosPorEspecialidad.Keys)
                ddlEspecialidades.Items.Add(especialidad);
        }

        private void CargarMedicos()
        {
            ddlMedico.Items.Clear();
            string especialidad = ddlEspecialidades.SelectedValue;

            if (MedicosPorEspecialidad.ContainsKey(especialidad))
            {
                foreach (string medico in MedicosPorEspecialidad[especialidad])
                    ddlMedico.Items.Add(medico);
            }
        }

        private void CargarDias()
        {
            ddlDias.Items.Clear();
            foreach (string dia in DiasDisponibles)
                ddlDias.Items.Add(dia);
        }

        private void CargarHoras()
        {
            ddlHoras.Items.Clear();
            foreach (string hora in HorasDisponibles)
                ddlHoras.Items.Add(hora);
        }

        private void BindTurnos()
        {
            gvTurnos.DataSource = TurnosDelPaciente;
            gvTurnos.DataBind();
        }

        protected void ddlEspecialidades_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CargarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.ddlEspecialidades_SelectedIndexChanged", ex);
                MostrarMensaje("No se pudieron cargar los medicos de esa especialidad. Intenta nuevamente mas tarde.", false);
            }
        }

        protected void btnAgendar_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataRow fila in TurnosDelPaciente.Rows)
                {
                    if (fila["Medico"].ToString() == ddlMedico.SelectedValue
                        && fila["Dia"].ToString() == ddlDias.SelectedValue
                        && fila["Hora"].ToString() == ddlHoras.SelectedValue)
                    {
                        MostrarMensaje("Ya tenes un turno con ese medico en ese dia y horario.", false);
                        return;
                    }
                }

                TurnosDelPaciente.Rows.Add(
                    ddlEspecialidades.SelectedValue,
                    ddlMedico.SelectedValue,
                    ddlDias.SelectedValue,
                    ddlHoras.SelectedValue,
                    "Pendiente");

                BindTurnos();
                MostrarMensaje("Turno agendado con " + ddlMedico.SelectedValue + " el " + ddlDias.SelectedValue + " a las " + ddlHoras.SelectedValue + " hs.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnAgendar_Click", ex);
                MostrarMensaje("No se pudo agendar el turno. Intenta nuevamente mas tarde.", false);
            }
        }

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            try
            {
                ddlEspecialidades.SelectedIndex = 0;
                CargarMedicos();
                ddlDias.SelectedIndex = 0;
                ddlHoras.SelectedIndex = 0;
                lblMensajeTurno.Text = "";
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.btnCancelar_Click", ex);
                MostrarMensaje("No se pudo limpiar el formulario. Intenta nuevamente mas tarde.", false);
            }
        }

        protected void gvTurnos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "CancelarTurno")
                {
                    int indice = Convert.ToInt32(e.CommandArgument);

                    if (indice < 0 || indice >= TurnosDelPaciente.Rows.Count)
                    {
                        MostrarMensaje("El turno seleccionado ya no esta disponible.", false);
                        return;
                    }

                    TurnosDelPaciente.Rows.RemoveAt(indice);
                    BindTurnos();
                    MostrarMensaje("El turno fue cancelado.", true);
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Turnos.gvTurnos_RowCommand", ex);
                MostrarMensaje("No se pudo cancelar el turno. Intenta nuevamente mas tarde.", false);
            }
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensajeTurno.Text = texto;
            lblMensajeTurno.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
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
