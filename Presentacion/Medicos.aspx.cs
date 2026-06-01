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
            if (!IsPostBack)
            {
                gvTurnosMedicos.DataSource = GetTurnosDummy();
                gvTurnosMedicos.DataBind();
                if (Session["Usuario"] == null)
                {
                    Response.Redirect("Login.aspx");
                }
            }
            
        }


        private DataTable GetTurnosDummy()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Paciente", typeof(string));
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Hora", typeof(DateTime));

            dt.Rows.Add("Carlos Perez", DateTime.Today, DateTime.Now);
            dt.Rows.Add("Juan Martinez", DateTime.Today.AddDays(1), DateTime.Now.AddHours(1));
            dt.Rows.Add("Ana Gomez", DateTime.Today.AddDays(2), DateTime.Now.AddHours(2));

            return dt;
        }
    }
}