using BE;
using BLL;
using SERVICIOS;
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

        public void CargarGrilla()
        {
            gvUsuarios.DataSource = bll_usuario.Usuarios();
            gvUsuarios.DataBind();

            txtNombre.Text = string.Empty;
            txtApellido.Text = string.Empty;
            txtDNI.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
        }

        protected void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuario = new BE_Usuario();

                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;

                usuario.HashPassword = txtPassword.Text;

                usuario.DNI = txtDNI.Text;

                usuario.IntentosFallidos = 0;

                if (btnActivo.Text=="Activo")
                {
                    usuario.Activo = true;
                }

                else
                {
                    usuario.Activo = false;
                }

                string datosConcatenados = $"{usuario.Nombre}|{usuario.Apellido}|{usuario.Email}|{usuario.HashPassword}|{usuario.DNI}|{usuario.IntentosFallidos}|{(usuario.Activo ? 1 : 0)}";

                usuario.DVH = datosConcatenados;

                bll_usuario.AgregarUsuario(usuario);

                CargarGrilla();
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "errorAlert", $"alert('Error: {ex.Message}');", true);
            }
           


        }

        protected void gvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow fila = gvUsuarios.SelectedRow;

            hiddenIdUsuario.Value = fila.Cells[1].Text; 
            txtNombre.Text = fila.Cells[2].Text;
            txtApellido.Text = fila.Cells[3].Text;
            txtDNI.Text = fila.Cells[4].Text;
            txtEmail.Text = fila.Cells[5].Text;
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Usuario usuario = new BE_Usuario();

                usuario.IdUsuario = int.Parse(hiddenIdUsuario.Value);
                usuario.Nombre = txtNombre.Text;
                usuario.Apellido = txtApellido.Text;
                usuario.Email = txtEmail.Text;
                usuario.HashPassword = txtPassword.Text;
                usuario.DNI = txtDNI.Text;

                if (btnActivo.Text == "Activo")
                {
                    usuario.Activo = true;
                }

                else
                {
                    usuario.Activo = false;
                }

                string datosConcatenados = $"{usuario.Nombre}|{usuario.Apellido}|{usuario.Email}|{usuario.HashPassword}|{usuario.DNI}|{usuario.IntentosFallidos}|{(usuario.Activo ? 1 : 0)}";

                usuario.DVH = datosConcatenados;
                
                bll_usuario.ModificarUsuario(usuario);

                CargarGrilla();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
            }
        }

        protected void btnActivo_Click(object sender, EventArgs e)
        {
            if (btnActivo.Text == "Inactivo")
            {
                btnActivo.Text = "Activo";
                btnActivo.CssClass = "btn-toggle btn-activo";
            }
            else
            {
                btnActivo.Text = "Inactivo";
                btnActivo.CssClass = "btn-toggle btn-inactivo";
            }
        }

        protected void btnEliminar_Click(object sender, EventArgs e)
        {
            try
            {
                int id = int.Parse(hiddenIdUsuario.Value);

                bll_usuario.EliminarUsuario(id);

                CargarGrilla();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = ex.Message;
            }
        }
    }
}