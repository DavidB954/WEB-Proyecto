using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class bITACORA : System.Web.UI.Page
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        protected void Page_Load(object sender, EventArgs e)
        {
            gvBitacora.DataSource = bll_bitacora.ObtenerBitacora();
            gvBitacora.DataBind();
        }
    }
}