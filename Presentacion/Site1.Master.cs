using BE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Site1 : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["Usuario"]!= null)
                {
                    BE_Usuario usuario = (BE_Usuario)Session["Usuario"];
                    lblUsuario.Text = usuario.Nombre;

                    //Cada link del navbar se muestra solo para los roles a los que les sirve esa pantalla.
                    liUsuarios.Visible = usuario.NombreRol == "ADMINISTRADOR";
                    liTurnos.Visible = usuario.NombreRol == "PACIENTE";
                    liRecetas.Visible = usuario.NombreRol == "PACIENTE";
                    liMedicos.Visible = usuario.NombreRol == "MEDICO";
                    liBitacora.Visible = usuario.NombreRol == "WEBMASTER" || usuario.NombreRol == "ADMINISTRADOR";
                    liSeguridad.Visible = usuario.NombreRol == "WEBMASTER";
                }
            }
        }
    }
}