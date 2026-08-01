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
    public partial class Recetas : System.Web.UI.Page
    {
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
                CargarRecetas();
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

            dt.Rows.Add(DateTime.Today.AddDays(-45), "Dr. Sosa", "Traumatología", "Diclofenac 75mg", "Tomar cada 12 horas por 5 días", "Finalizada");
            dt.Rows.Add(DateTime.Today.AddDays(-30), "Dra. Romero", "Clínica Médica", "Loratadina 10mg", "Tomar 1 comprimido por día durante 10 días", "Finalizada");
            dt.Rows.Add(DateTime.Today.AddDays(-10), "Dr. Pérez", "Cardiología", "Amoxicilina 500mg", "Tomar cada 8 horas por 7 días", "Vigente");
            dt.Rows.Add(DateTime.Today.AddDays(-5), "Dra. Gómez", "Dermatología", "Ibuprofeno 400mg", "Tomar cada 12 horas según dolor", "Vigente");
            dt.Rows.Add(DateTime.Today.AddDays(-2), "Dr. López", "Pediatría", "Paracetamol 1g", "Tomar cada 8 horas, máximo 3 veces al día", "Vigente");

            gvRecetas.DataSource = dt;
            gvRecetas.DataBind();
        }
    }
}
