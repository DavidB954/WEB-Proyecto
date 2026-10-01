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
    public partial class Recetas : PaginaBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;
            UsuarioComponente permisos = Session["Permisos"] as UsuarioComponente;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, permisos, "ACCESO_RECETAS"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarRecetas();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Recetas.Page_Load", ex);
                    lblMensaje.Text = "No se pudieron cargar las recetas. Intenta nuevamente mas tarde.";
                    lblMensaje.ForeColor = System.Drawing.Color.Red;
                }
            }
        }

        private void CargarRecetas()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Medico", typeof(string));
            dt.Columns.Add("Especialidad", typeof(string));
            dt.Columns.Add("Medicamento", typeof(string));
            dt.Columns.Add("Indicaciones", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            dt.Rows.Add(DateTime.Today.AddDays(-45), "Dr. Sosa", "Traumatologia", "Diclofenac 75mg", "Tomar cada 12 horas por 5 dias", "Finalizada");
            dt.Rows.Add(DateTime.Today.AddDays(-30), "Dra. Romero", "Clinica Medica", "Loratadina 10mg", "Tomar 1 comprimido por dia durante 10 dias", "Finalizada");
            dt.Rows.Add(DateTime.Today.AddDays(-10), "Dr. Perez", "Cardiologia", "Amoxicilina 500mg", "Tomar cada 8 horas por 7 dias", "Vigente");
            dt.Rows.Add(DateTime.Today.AddDays(-5), "Dra. Gomez", "Dermatologia", "Ibuprofeno 400mg", "Tomar cada 12 horas segun dolor", "Vigente");
            dt.Rows.Add(DateTime.Today.AddDays(-2), "Dr. Lopez", "Pediatria", "Paracetamol 1g", "Tomar cada 8 horas, maximo 3 veces al dia", "Vigente");

            gvRecetas.DataSource = dt;
            gvRecetas.DataBind();
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
