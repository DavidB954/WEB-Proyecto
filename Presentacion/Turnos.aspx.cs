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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                gvTurnos.DataSource = GetTurnosDummy();
                gvTurnos.DataBind();
                if (Session["Usuario"] == null)
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        private DataTable GetTurnosDummy()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Especialidad");
            dt.Columns.Add("Medico");
            dt.Columns.Add("Dia");
            dt.Columns.Add("Hora");

            dt.Rows.Add("Cardiología", "Dr. Pérez", "Lunes", "10:00");
            dt.Rows.Add("Dermatología", "Dra. Gómez", "Martes", "11:30");
            dt.Rows.Add("Pediatría", "Dr. López", "Miércoles", "14:00");

            return dt;
        }
    }
}