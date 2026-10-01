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
    public partial class WebForm2 : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_MEDICOS"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    BindTurnos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Medicos.Page_Load", ex);
                    MostrarMensaje("No se pudieron cargar los turnos del dia. Intenta nuevamente mas tarde.", false);
                }
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

                    dt.Rows.Add("08:30", "Carlos Perez", "OSDE", "En espera");
                    dt.Rows.Add("09:00", "Ana Gomez", "Swiss Medical", "En espera");
                    dt.Rows.Add("09:30", "Juan Martinez", "PAMI", "En espera");
                    dt.Rows.Add("10:00", "Lucia Fernandez", "Galeno", "En espera");
                    dt.Rows.Add("10:30", "Roberto Diaz", "IOMA", "En espera");
                    dt.Rows.Add("11:00", "Marta Suarez", "OSDE", "En espera");

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
            try
            {
                if (gvTurnosMedicos.SelectedIndex < 0 || gvTurnosMedicos.SelectedIndex >= TurnosDelDia.Rows.Count)
                {
                    return;
                }

                DataRow turno = TurnosDelDia.Rows[gvTurnosMedicos.SelectedIndex];
                lblPacienteSeleccionado.Text = "Atendiendo a " + turno["Paciente"] + " - turno de las " + turno["Hora"] + " hs (" + turno["ObraSocial"] + ").";
                lblMensajeAtencion.Text = "";
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.gvTurnosMedicos_SelectedIndexChanged", ex);
                MostrarMensaje("No se pudo seleccionar el turno.", false);
            }
        }

        protected void btnGuardarAtencion_Click(object sender, EventArgs e)
        {
            try
            {
                if (gvTurnosMedicos.SelectedIndex < 0 || gvTurnosMedicos.SelectedIndex >= TurnosDelDia.Rows.Count)
                {
                    MostrarMensaje("Primero selecciona un paciente de la lista de turnos.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMotivoConsulta.Text) || string.IsNullOrWhiteSpace(txtDiagnostico.Text))
                {
                    MostrarMensaje("Completa al menos el motivo de la consulta y el diagnostico.", false);
                    return;
                }

                DataRow turno = TurnosDelDia.Rows[gvTurnosMedicos.SelectedIndex];
                string paciente = turno["Paciente"].ToString();
                turno["Estado"] = "Atendido";

                gvTurnosMedicos.SelectedIndex = -1;
                BindTurnos();
                LimpiarFormulario();

                MostrarMensaje("La atencion de " + paciente + " quedo registrada correctamente.", true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnGuardarAtencion_Click", ex);
                MostrarMensaje("No se pudo registrar la atencion. Intenta nuevamente mas tarde.", false);
            }
        }

        private void LimpiarFormulario()
        {
            txtMotivoConsulta.Text = "";
            txtDiagnostico.Text = "";
            txtObservaciones.Text = "";
            txtMedicamento.Text = "";
            txtObservacionesReceta.Text = "";
            lblPacienteSeleccionado.Text = "Ningun paciente seleccionado.";
        }

        private void MostrarMensaje(string texto, bool exito)
        {
            lblMensajeAtencion.Text = texto;
            lblMensajeAtencion.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
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
