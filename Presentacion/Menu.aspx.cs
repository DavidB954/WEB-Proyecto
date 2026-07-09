using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Menu : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (usuarioLogueado == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            //Cada tarjeta se muestra solo para los roles a los que les sirve esa pantalla.
            cardTurnos.Visible = usuarioLogueado.NombreRol == "MEDICO" || usuarioLogueado.NombreRol == "PACIENTE";
            cardUsuarios.Visible = usuarioLogueado.NombreRol == "ADMINISTRADOR";
            cardRecetas.Visible = usuarioLogueado.NombreRol == "PACIENTE";
            cardMedicos.Visible = usuarioLogueado.NombreRol == "MEDICO";
            cardBitacora.Visible = usuarioLogueado.NombreRol == "WEBMASTER" || usuarioLogueado.NombreRol == "ADMINISTRADOR";
            cardSeguridad.Visible = usuarioLogueado.NombreRol == "WEBMASTER";
        }
    }
}