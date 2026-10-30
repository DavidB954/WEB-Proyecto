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
    /// <summary>
    /// Base de la que heredan todas las paginas que usan Site1.Master. Se
    /// suscribe como Observer ante GestorIdioma: al suscribirse recibe una
    /// sincronizacion inmediata (idioma actual de la sesion) y, si el combo de
    /// idioma de la Master dispara un cambio durante este mismo postback,
    /// vuelve a recibir la notificacion y reaplica las traducciones.
    ///
    /// Tambien centraliza la seguridad por permisos (ver CatalogoPermisos):
    ///   - Si la pagina tiene permisos en el catalogo, solo entra quien tenga
    ///     alguno de ellos (el de PAGINA o alguna ACCION).
    ///   - Todo control con data-permiso="CODIGO" se oculta si el usuario no
    ///     tiene ese permiso ni el permiso PAGINA de esa pagina.
    ///   - ExigirPermiso("CODIGO") se llama al inicio de cada OnClick protegido.
    /// </summary>
    public class PaginaBase : Page, IIdiomaObservador
    {
        private const string AtributoPermiso = "data-permiso";

        protected BE_Usuario UsuarioLogueado => Session["Usuario"] as BE_Usuario;

        protected UsuarioComponente PermisosUsuario => Session["Permisos"] as UsuarioComponente;

        protected string NombrePagina => Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);

        // Texto escrito en C# traducido al idioma activo. El texto en español es la clave.
        protected string T(string textoBase, params object[] argumentos)
        {
            return GestorIdioma.Traducir(textoBase, argumentos);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Fuera del try/catch de abajo: el Response.Redirect corta la pagina
            // con ThreadAbortException y no tiene que confundirse con un error.
            ValidarAccesoPagina();

            try
            {
                GestorIdioma.Suscribir(this);
            }
            catch
            {
                // OnInit corre antes que cualquier try/catch propio de la pagina en
                // Page_Load: una falla aca no puede tirar abajo la pagina entera.
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Las filas de las grillas recien existen despues del DataBind: se
            // traducen aca y no en OnInit como el resto de la pagina.
            GestorIdioma.AplicarTraduccionesGrillas(this);
            GestorIdioma.AplicarTituloPagina(this);

            // En PreRender (y no en Load) para cubrir tambien las filas que las
            // grillas generan al hacer DataBind dentro de los eventos.
            try
            {
                OcultarControlesSinPermiso(this);
            }
            catch
            {
                // Si falla el recorrido no se puede dejar visible lo protegido:
                // ExigirPermiso en cada OnClick sigue bloqueando la accion.
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

        // Ocultar un boton no alcanza: un postback se puede forzar a mano. Cada
        // OnClick protegido arranca con: if (!ExigirPermiso("CODIGO")) return;
        // No redirige: un Response.Redirect dentro del try/catch del OnClick
        // terminaria registrado como error.
        protected bool ExigirPermiso(string codigoPermiso)
        {
            if (TienePermiso(codigoPermiso))
            {
                return true;
            }

            string mensaje = System.Web.HttpUtility.JavaScriptStringEncode(T("No tiene permiso para realizar esta accion."), true);
            ScriptManager.RegisterStartupScript(this, GetType(), "sinPermiso_" + Guid.NewGuid().ToString("N"), $"alert({mensaje});", true);

            return false;
        }

        // Reemplaza la respuesta de la pagina por el archivo. Se evita Response.End()
        // (corta con ThreadAbortException); SuppressContent descarta el HTML que la
        // pagina igual va a generar despues de este evento.
        protected void DescargarXml(string xml, string nombreArchivo)
        {
            Response.Clear();
            Response.ContentType = "application/xml";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.AddHeader("Content-Disposition", $"attachment; filename={nombreArchivo}");
            Response.Write(xml);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
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
