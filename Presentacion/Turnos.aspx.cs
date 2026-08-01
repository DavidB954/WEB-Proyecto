using BE;
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
    public partial class Turnos : System.Web.UI.Page
    {
        private static readonly Dictionary<string, string[]> MedicosPorEspecialidad = new Dictionary<string, string[]>
        {
            { "Cardiología",    new[] { "Dr. Pérez",   "Dra. Salinas" } },
            { "Clínica Médica", new[] { "Dra. Romero", "Dr. Aguirre" } },
            { "Dermatología",   new[] { "Dra. Gómez",  "Dr. Ferreyra" } },
            { "Pediatría",      new[] { "Dr. López",   "Dra. Bianchi" } },
            { "Traumatología",  new[] { "Dr. Sosa",    "Dra. Vega" } }
        };

        private static readonly string[] DiasDisponibles = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes" };
        private static readonly string[] HorasDisponibles = { "08:00", "09:00", "10:00", "11:00", "12:00", "14:00", "15:00", "16:00", "17:00" };

        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "PACIENTE"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CargarEspecialidades();
                CargarMedicos();
                CargarDias();
                CargarHoras();
                BindTurnos();
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

                    dt.Rows.Add("Cardiología", "Dr. Pérez", "Lunes", "10:00", "Confirmado");
                    dt.Rows.Add("Dermatología", "Dra. Gómez", "Martes", "11:00", "Confirmado");
                    dt.Rows.Add("Pediatría", "Dr. López", "Miércoles", "14:00", "Pendiente");

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
            CargarMedicos();
        }

        protected void btnAgendar_Click(object sender, EventArgs e)
        {
            foreach (DataRow fila in TurnosDelPaciente.Rows)
            {
                if (fila["Medico"].ToString() == ddlMedico.SelectedValue
                    && fila["Dia"].ToString() == ddlDias.SelectedValue
                    && fila["Hora"].ToString() == ddlHoras.SelectedValue)
                {
                    MostrarMensaje("Ya tenés un turno con ese médico en ese día y horario.", false);
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

        protected void btnCancelar_Click(object sender, EventArgs e)
        {
            ddlEspecialidades.SelectedIndex = 0;
            CargarMedicos();
            ddlDias.SelectedIndex = 0;
            ddlHoras.SelectedIndex = 0;
            lblMensajeTurno.Text = "";
        }

        protected void gvTurnos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelarTurno")
            {
                int indice = Convert.ToInt32(e.CommandArgument);
                TurnosDelPaciente.Rows.RemoveAt(indice);
                BindTurnos();
                MostrarMensaje("El turno fue cancelado.", true);
            }
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensajeTurno.Text = texto;
            lblMensajeTurno.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
        }
    }
}
