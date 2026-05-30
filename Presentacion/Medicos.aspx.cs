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
            gvTurnosMedicos.DataSource = GetTurnosDummy();
            gvTurnosMedicos.DataBind();
        }


        private DataTable GetTurnosDummy()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IdPaciente", typeof(int));
            dt.Columns.Add("Fecha", typeof(DateTime));
            dt.Columns.Add("Hora", typeof(DateTime));

            dt.Rows.Add(1, DateTime.Today, DateTime.Now);
            dt.Rows.Add(2, DateTime.Today.AddDays(1), DateTime.Now.AddHours(1));
            dt.Rows.Add(3, DateTime.Today.AddDays(2), DateTime.Now.AddHours(2));

            return dt;
        }
    }
}