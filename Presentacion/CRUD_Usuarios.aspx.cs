using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        BLL_Usuario bll_usuario = new BLL_Usuario();
        protected void Page_Load(object sender, EventArgs e)
        {
            gvUsuarios.DataSource = bll_usuario.Usuarios();
            gvUsuarios.DataBind();
        }
    }
}