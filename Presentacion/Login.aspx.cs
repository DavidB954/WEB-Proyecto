using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class Login : System.Web.UI.Page
    {
        BLL_Usuario bll_Usu = new BLL_Usuario();
        BLL_DVV bll_dvv = new BLL_DVV();
        BE_LoginResultado Obj_Usuario = new BE_LoginResultado();

        protected void Page_Load(object sender, EventArgs e)
        {
            //Sin IsPostBack acá: si esto solo corriera en la carga inicial, alcanzaría con manipular
            //la base con la página de Login ya abierta en el navegador y tocar "Ingresar" (eso es un
            //postback) para que btnLogin_Click autentique contra datos corruptos sin pasar por este control,
            //y encima RefrescarDVH terminaría "curando" el DVH manipulado con los datos ya alterados.
            if (!VerificarIntegridadOAbortar())
            {
                return;
            }

            if (!IsPostBack)
            {
                //Si la integridad está OK, mostramos el aviso de "volvé a loguearte" que dejó Seguridad.aspx tras Recalcular/BackUp/Restore.
                //Se muestra como toast flotante (no como texto fijo en el formulario de login).
                if (Session["MensajeLogout"] != null)
                {
                    string mensajeLogout = Session["MensajeLogout"] as string;
                    Session.Remove("MensajeLogout");
                    MostrarToast(mensajeLogout);
                }
            }
        }

        //Devuelve true si la base está íntegra y el request puede seguir su curso normal.
        //Devuelve false (y ya dejó la respuesta lista: mensaje de error o redirect a LoginWebmaster)
        //cuando hay que frenar acá, sea por corrupción detectada o por no poder verificarla.
        private bool VerificarIntegridadOAbortar()
        {
            var mensajes = new List<string>();

            try
            {
                //Bitácora se chequea aparte y siempre, sin usar VerificarIntegridad como gate: su DVV agregado
                //se auto-repara con cualquier evento nuevo (ver comentario en BLL_DVV.DetectarCambiosBitacoraSiempre),
                //así que ese agregado no es confiable ni para decidir si vale la pena mirar fila por fila ni para
                //decidir si corresponde loguear el evento INTEGRIDAD_ERROR (ese log ahora cuelga del chequeo por
                //fila, dentro de DetectarCambiosBitacoraSiempre, que es el que realmente detecta algo).
                mensajes.AddRange(bll_dvv.DetectarCambiosBitacoraSiempre());

                foreach (var tabla in new[] { "Usuario", "Rol" })
                {
                    if (!bll_dvv.VerificarIntegridad(tabla))
                    {
                        mensajes.AddRange(bll_dvv.DetectarCambios(tabla));
                    }
                }
            }
            catch (Exception)
            {
                //Sin esto, una base caída o inaccesible tumbaba la página de Login con pantalla amarilla
                //antes de que nadie pudiera siquiera ver el formulario.
                lblMensaje.Text = "No se pudo verificar la integridad del sistema. Intentá nuevamente más tarde.";
                return false;
            }

            if (mensajes.Count > 0)
            {
                Session["MensajesIntegridad"] = mensajes;
                Response.Redirect("LoginWebmaster.aspx");
                return false;
            }

            return true;
        }
        //Muestra un mensaje flotante (toast) que se cierra solo
        private void MostrarToast(string mensaje)
        {
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast toast-exito';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},4000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},4600);})();";

            ClientScript.RegisterStartupScript(GetType(), "toastLogout", script, true);
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {

                Obj_Usuario = bll_Usu.ObtenerUsuarioPorEmail(txtEmail.Text, txtPassword.Text);

                //Si el login normal falla, probamos las credenciales de emergencia (por si se eliminó al único ADMINISTRADOR o WEBMASTER del sistema).
                if (Obj_Usuario.Usuario == null)
                {
                    BE_LoginResultado emergencia = bll_Usu.LoginEmergencia(txtEmail.Text, txtPassword.Text);

                    if (emergencia.Usuario != null)
                    {
                        Obj_Usuario = emergencia;
                    }
                }

                if (Obj_Usuario.Usuario != null)
                {
                    // Guardamos el usuario en sesión
                    Session["Usuario"] = Obj_Usuario.Usuario;


                    Response.Redirect("Menu.aspx");
                }
                else
                {
                    lblMensaje.Text = Obj_Usuario.Mensaje;
                    return;
                }
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Login.btnLogin_Click", ex);
                lblMensaje.Text = "No se pudo iniciar sesión. Intentá nuevamente más tarde.";
            }

        }

        //Deja rastro en el mismo log que usa Global.asax, sin mostrarle al usuario el detalle interno de la excepción.
        private void RegistrarErrorInterno(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = Server.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(carpetaLogs);
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
                //Si ni el log funciona, no hay nada más para hacer acá.
            }
        }
    }
}