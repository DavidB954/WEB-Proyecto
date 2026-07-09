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
            if (!IsPostBack)
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "PACIENTE"))
                {
                    Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                    return;
                }

                CargarRecetas();
            }
        }


        private void CargarRecetas()
        {
            // Creamos una tabla en memoria
            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Medico", typeof(string));
            dt.Columns.Add("Medicamento", typeof(string));
            dt.Columns.Add("Indicaciones", typeof(string));

            // Datos de ejemplo
            dt.Rows.Add(DateTime.Now.AddDays(-10), "Dr. Pérez", "Amoxicilina 500mg", "Tomar cada 8 horas por 7 días");
            dt.Rows.Add(DateTime.Now.AddDays(-5), "Dra. Gómez", "Ibuprofeno 400mg", "Tomar cada 12 horas según dolor");
            dt.Rows.Add(DateTime.Now.AddDays(-2), "Dr. López", "Paracetamol 1g", "Tomar cada 8 horas, máximo 3 veces al día");

            // Enlazamos al GridView
            gvRecetas.DataSource = dt;
            gvRecetas.DataBind();
        }
    }
}