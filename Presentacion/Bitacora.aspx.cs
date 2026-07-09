using BE;
using BLL;
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
    public partial class bITACORA : System.Web.UI.Page
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        BLL_Usuario bll_usuarios = new BLL_Usuario();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

                if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "WEBMASTER", "ADMINISTRADOR"))
                {
                    Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                    return;
                }

                CargarUsuarios();
                CargarModulos();
                gvBitacora.DataSource = bll_bitacora.ObtenerBitacora();
                gvBitacora.DataBind();
            }
        }


        private void CargarUsuarios()
        {
            ddlUsuarios.DataSource = bll_usuarios.Usuarios();
            ddlUsuarios.DataTextField = "NombreApellido";
            ddlUsuarios.DataValueField = "IdUsuario";

            ddlUsuarios.DataBind();

            ddlUsuarios.Items.Insert(0, new ListItem("Todos", ""));
        }

        private void CargarModulos()
        {
            ddlModulos.Items.Clear(); 

            ddlModulos.Items.Add(new ListItem("Todos", ""));


            ddlModulos.Items.Add(new ListItem("LOGIN", "LOGIN"));
            ddlModulos.Items.Add(new ListItem("USUARIO", "USUARIO"));
        }

        public void btnFiltrar_Click(object sender, EventArgs e)
        {
            DateTime? desde = string.IsNullOrEmpty(fechaDesde.Text) ? (DateTime?)null : DateTime.Parse(fechaDesde.Text).Date;
            DateTime? hasta = string.IsNullOrEmpty(fechaHasta.Text) ? (DateTime?)null : DateTime.Parse(fechaHasta.Text).Date.AddDays(1).AddSeconds(-1);

            int? idUsuario = string.IsNullOrEmpty(ddlUsuarios.SelectedValue) ? (int?)null : int.Parse(ddlUsuarios.SelectedValue);
            string modulo = string.IsNullOrEmpty(ddlModulos.SelectedValue) ? null : ddlModulos.SelectedValue;
            string ip = string.IsNullOrEmpty(txtIP.Text) ? null : txtIP.Text;

            DataTable dt = bll_bitacora.FiltrarBitacora(desde, hasta, idUsuario, modulo, ip);

            gvBitacora.DataSource = dt;
            gvBitacora.DataBind();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            fechaDesde.Text = "";
            fechaHasta.Text = "";
            ddlUsuarios.SelectedIndex = 0;
            ddlModulos.SelectedIndex = 0; 
            txtIP.Text = "";

            gvBitacora.DataSource = bll_bitacora.ObtenerBitacora();
            gvBitacora.DataBind();
        }
    }
}