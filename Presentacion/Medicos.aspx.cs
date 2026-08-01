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
    public partial class WebForm2 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "MEDICO"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                BindTurnos();
            }
        }

        private DataTable TurnosDelDia
        {
            get
            {
                DataTable dt = Session["TurnosMedico"] as DataTable;
                if (dt == null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Hora", typeof(string));
                    dt.Columns.Add("Paciente", typeof(string));
                    dt.Columns.Add("ObraSocial", typeof(string));
                    dt.Columns.Add("Estado", typeof(string));

                    dt.Rows.Add("08:30", "Carlos Pérez", "OSDE", "En espera");
                    dt.Rows.Add("09:00", "Ana Gómez", "Swiss Medical", "En espera");
                    dt.Rows.Add("09:30", "Juan Martínez", "PAMI", "En espera");
                    dt.Rows.Add("10:00", "Lucía Fernández", "Galeno", "En espera");
                    dt.Rows.Add("10:30", "Roberto Díaz", "IOMA", "En espera");
                    dt.Rows.Add("11:00", "Marta Suárez", "OSDE", "En espera");

                    Session["TurnosMedico"] = dt;
                }
                return dt;
            }
        }

        private void BindTurnos()
        {
            gvTurnosMedicos.DataSource = TurnosDelDia;
            gvTurnosMedicos.DataBind();
        }

        protected void gvTurnosMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRow turno = TurnosDelDia.Rows[gvTurnosMedicos.SelectedIndex];
            lblPacienteSeleccionado.Text = "Atendiendo a " + turno["Paciente"] + " - turno de las " + turno["Hora"] + " hs (" + turno["ObraSocial"] + ").";
            lblMensajeAtencion.Text = "";
        }

        protected void btnGuardarAtencion_Click(object sender, EventArgs e)
        {
            if (gvTurnosMedicos.SelectedIndex < 0)
            {
                MostrarMensaje("Primero seleccioná un paciente de la lista de turnos.", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtMotivoConsulta.Text) || string.IsNullOrWhiteSpace(txtDiagnostico.Text))
            {
                MostrarMensaje("Completá al menos el motivo de la consulta y el diagnóstico.", false);
                return;
            }

            DataRow turno = TurnosDelDia.Rows[gvTurnosMedicos.SelectedIndex];
            string paciente = turno["Paciente"].ToString();
            turno["Estado"] = "Atendido";

            gvTurnosMedicos.SelectedIndex = -1;
            BindTurnos();
            LimpiarFormulario();

            MostrarMensaje("La atención de " + paciente + " quedó registrada correctamente.", true);
        }

        private void LimpiarFormulario()
        {
            txtMotivoConsulta.Text = "";
            txtDiagnostico.Text = "";
            txtObservaciones.Text = "";
            txtMedicamento.Text = "";
            txtObservacionesReceta.Text = "";
            lblPacienteSeleccionado.Text = "Ningún paciente seleccionado.";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensajeAtencion.Text = texto;
            lblMensajeAtencion.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
        }
    }
}
