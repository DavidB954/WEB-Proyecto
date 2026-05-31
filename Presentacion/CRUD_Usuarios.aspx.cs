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
            if (!IsPostBack)
            {
                gvUsuarios.DataSource = bll_usuario.Usuarios();
                CargarTipos();
                gvUsuarios.DataBind();
            }
            
        }


        public void CargarTipos()
        {
            ddlTipo.Items.Add(new ListItem("MEDICO", "1"));
            ddlTipo.Items.Add(new ListItem("PACIENTE", "2"));
        }

        protected void ddlTipo_SelectedIndexChanged(object sender, EventArgs e)
        {

            // 🔹 Ocultar todo primero
            divMedico.Visible = false;
            lblMatricula.Visible = false;
            txtMatricula.Visible = false;
            lblEspecialidad.Visible = false;
            txtEspecialidad.Visible = false;
            chkDias.Visible = false;
            lblHoraInicio.Visible = false;
            txtHoraInicio.Visible = false;
            lblHoraFin.Visible = false;
            txtHoraFin.Visible = false;

            divPaciente.Visible = false;
            lblHistoriaClinica.Visible = false;
            txtHistoriaClinica.Visible = false;
            lblFechaNacimiento.Visible = false;
            txtFechaNac.Visible = false;
            lblTelefono.Visible = false;
            txtTelefono.Visible = false;

            if (ddlTipo.SelectedValue == "1")
            {
                divMedico.Visible = true;
                lblMatricula.Visible = true;
                txtMatricula.Visible = true;

                lblEspecialidad.Visible = true;
                txtEspecialidad.Visible = true;

                chkDias.Visible = true;

                lblHoraInicio.Visible = true;
                txtHoraInicio.Visible = true;

                lblHoraFin.Visible = true;
                txtHoraFin.Visible = true;
            }
            else if (ddlTipo.SelectedValue == "2")
            {
                divPaciente.Visible = true;
                lblHistoriaClinica.Visible = true;
                txtHistoriaClinica.Visible = true;

                lblFechaNacimiento.Visible = true;
                txtFechaNac.Visible = true;

                lblTelefono.Visible = true;
                txtTelefono.Visible = true;
            }
        }
    }
}