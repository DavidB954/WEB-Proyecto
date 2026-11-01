using BE;
using BE.Seguridad;
using SERVICIOS;
using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public class PaginaBase : Page, IIdiomaObservador
    {
        private const string AtributoPermiso = "data-permiso";

        protected BE_Usuario UsuarioLogueado => Session["Usuario"] as BE_Usuario;

        protected UsuarioComponente PermisosUsuario => Session["Permisos"] as UsuarioComponente;

        protected string NombrePagina => Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);

        protected string T(string textoBase, params object[] argumentos)
        {
            return GestorIdioma.Traducir(textoBase, argumentos);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ValidarAccesoPagina();

            GestorIdioma.Suscribir(this);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            GestorIdioma.AplicarTraduccionesGrillas(this);
            GestorIdioma.AplicarTituloPagina(this);

            try
            {
                OcultarControlesSinPermiso(this);
            }
            catch
            {
            }
        }

        public void ActualizarIdioma()
        {
            GestorIdioma.AplicarTraducciones(this);
        }

        protected bool TienePermiso(string codigoPermiso)
        {
            return SeguridadHelper.TieneAccesoAccion(UsuarioLogueado, PermisosUsuario, codigoPermiso);
        }

        protected bool ExigirPermiso(string codigoPermiso)
        {
            if (TienePermiso(codigoPermiso))
            {
                return true;
            }

            string mensaje = System.Web.HttpUtility.JavaScriptStringEncode(T("No tiene permiso para realizar esta accion."), true);
            ScriptManager.RegisterStartupScript(this, GetType(), "sinPermiso_" + Guid.NewGuid().ToString("N"), $"mostrarAviso({mensaje});", true);

            return false;
        }

        protected void RegistrarErrorInterno(string origen, Exception ex)
        {
            RegistroErrores.Registrar(origen, ex);
        }

        protected void MostrarMensaje(Label label, string texto, bool exito)
        {
            label.Text = System.Web.HttpUtility.HtmlEncode(texto);
            label.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
        }

        protected void MostrarToast(string mensaje, bool exito, int duracionMs = 3000)
        {
            string clase = exito ? "toast-exito" : "toast-error";
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast " + clase + "';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';}," + duracionMs + ");" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}}," + (duracionMs + 600) + ");})();";

            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
        }

        // MaintainScrollPositionOnPostback restaura la posicion en el load: el setTimeout hace que este desplazamiento ocurra despues.
        protected void DesplazarHacia(Control destino)
        {
            string script = "window.addEventListener('load',function(){setTimeout(function(){document.getElementById('" + destino.ClientID + "').scrollIntoView({behavior:'smooth'});},0);});";

            ScriptManager.RegisterStartupScript(this, GetType(), "desplazarHacia", script, true);
        }

        private void DescargarArchivo(string tipoContenido, string nombreArchivo, Action escribirContenido)
        {
            Response.Clear();
            Response.ContentType = tipoContenido;
            Response.AddHeader("Content-Disposition", $"attachment; filename={nombreArchivo}");
            escribirContenido();
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void DescargarXml(string xml, string nombreArchivo)
        {
            DescargarArchivo("application/xml", nombreArchivo, () =>
            {
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.Write(xml);
            });
        }

        protected void DescargarPdf(byte[] pdf, string nombreArchivo)
        {
            DescargarArchivo("application/pdf", nombreArchivo, () => Response.BinaryWrite(pdf));
        }

        private void ValidarAccesoPagina()
        {
            if (!CatalogoPermisos.EsPaginaProtegida(NombrePagina))
            {
                return;
            }

            if (UsuarioLogueado == null)
            {
                Response.Redirect("Login.aspx");
            }
            else if (!SeguridadHelper.TieneAccesoPagina(UsuarioLogueado, PermisosUsuario, NombrePagina))
            {
                Response.Redirect("AccesoDenegado.aspx");
            }
        }

        private void OcultarControlesSinPermiso(Control padre)
        {
            foreach (Control control in padre.Controls)
            {
                string codigoPermiso = ObtenerPermisoDeclarado(control);

                if (codigoPermiso != null && !TienePermiso(codigoPermiso))
                {
                    control.Visible = false;
                    continue;
                }

                if (control.HasControls())
                {
                    OcultarControlesSinPermiso(control);
                }
            }
        }

        private static string ObtenerPermisoDeclarado(Control control)
        {
            string valor = null;

            if (control is WebControl webControl)
            {
                valor = webControl.Attributes[AtributoPermiso];
            }
            else if (control is HtmlControl htmlControl)
            {
                valor = htmlControl.Attributes[AtributoPermiso];
            }

            return string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
        }
    }
}
