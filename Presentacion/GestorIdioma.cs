using BE;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public static class GestorIdioma
    {
        private const string ClaveSesionIdioma = "IdiomaActual";
        private const string ClaveObservadoresRequest = "IdiomaObservadoresRequest";
        private const string ClaveMarcador = "data-i18n";
        private const string PaginaMaster = "Site1Master";

        private static readonly object candado = new object();
        private static readonly Dictionary<int, Dictionary<string, string>> cacheTraducciones = new Dictionary<int, Dictionary<string, string>>();
        private static readonly HashSet<string> clavesConocidas = new HashSet<string>();

        private static readonly BLL_Idioma bll_idioma = new BLL_Idioma();

        public static void Suscribir(IIdiomaObservador observador)
        {
            try
            {
                ObtenerObservadoresDelRequest().Add(observador);
                observador.ActualizarIdioma();
            }
            catch (Exception ex)
            {
                // Si un observador falla al sincronizarse no debe romper el render de
                // la pagina: en el peor caso queda sin traducir (texto base en español).
                RegistrarErrorSilencioso("GestorIdioma.Suscribir", ex);
            }
        }

        public static void CambiarIdioma(int idIdioma)
        {
            HttpContext.Current.Session[ClaveSesionIdioma] = idIdioma;

            foreach (IIdiomaObservador observador in ObtenerObservadoresDelRequest())
            {
                try
                {
                    observador.ActualizarIdioma();
                }
                catch (Exception ex)
                {
                    RegistrarErrorSilencioso("GestorIdioma.CambiarIdioma", ex);
                }
            }
        }

        public static int ObtenerIdiomaActual()
        {
            object valorSesion = HttpContext.Current.Session?[ClaveSesionIdioma];

            if (valorSesion is int idIdiomaSesion)
            {
                return idIdiomaSesion;
            }

            try
            {
                BE_Idioma idiomaPorDefecto = bll_idioma.ObtenerIdiomaPorDefecto();

                return idiomaPorDefecto?.IdIdioma ?? 0;
            }
            catch (Exception ex)
            {
                // Sin idioma resuelto la pagina sigue renderizando con el texto base
                // (español) en vez de romperse: es preferible a tirar la pagina abajo.
                RegistrarErrorSilencioso("GestorIdioma.ObtenerIdiomaActual", ex);
                return 0;
            }
        }

        public static void InvalidarCache(int idIdioma)
        {
            lock (candado)
            {
                cacheTraducciones.Remove(idIdioma);
            }
        }

        /// <summary>
        /// Recorre el arbol de controles de la pagina (y, dentro de el, el de la
        /// Master Page) aplicando la traduccion del idioma activo a cada control
        /// marcado con data-i18n. Si un control marcado no tiene todavia una fila
        /// en el catalogo, la registra usando su texto actual (el que esta escrito
        /// en el .aspx, en español) como texto base.
        /// </summary>
        public static void AplicarTraducciones(Control pagina)
        {
            try
            {
               
                string nombrePagina = (pagina.GetType().BaseType ?? pagina.GetType()).Name;
                Dictionary<string, string> traducciones = ObtenerTraduccionesDeCache(ObtenerIdiomaActual());

                RecorrerControl(pagina, nombrePagina, nombrePagina, traducciones);
            }
            catch (Exception ex)
            {
                // El multiidioma es una capa cosmetica sobre la pagina: si falla,
                // la pagina tiene que seguir mostrandose (con el texto base en
                // español), nunca romperse.
                RegistrarErrorSilencioso("GestorIdioma.AplicarTraducciones", ex);
            }
        }

        private static void RecorrerControl(Control control, string paginaActual, string paginaContenido, Dictionary<string, string> traducciones)
        {
            if (control is MasterPage)
            {
                paginaActual = PaginaMaster;
            }
            else if (control is ContentPlaceHolder)
            {
                paginaActual = paginaContenido;
            }

            if (control is GridView gridView)
            {
                TraducirEncabezadosGrilla(gridView, paginaActual, traducciones);
                return;
            }

            if (EsTraducible(control))
            {
                AplicarAlControl(control, paginaActual, traducciones);
            }

            foreach (Control hijo in control.Controls)
            {
                RecorrerControl(hijo, paginaActual, paginaContenido, traducciones);
            }
        }

        private static bool EsTraducible(Control control)
        {
            if (string.IsNullOrEmpty(control.ID))
            {
                return false;
            }

            if (control is WebControl webControl)
            {
                return webControl.Attributes[ClaveMarcador] != null;
            }

            if (control is HtmlControl htmlControl)
            {
                return htmlControl.Attributes[ClaveMarcador] != null;
            }

            return false;
        }

        private static void AplicarAlControl(Control control, string pagina, Dictionary<string, string> traducciones)
        {
            string clave = pagina + "." + control.ID;

            switch (control)
            {
                case Label lbl:
                    lbl.Text = ResolverTexto(clave, lbl.Text, traducciones);
                    break;
                case CheckBox chk:
                    chk.Text = ResolverTexto(clave, chk.Text, traducciones);
                    break;
                case LinkButton lnk:
                    lnk.Text = ResolverTexto(clave, lnk.Text, traducciones);
                    break;
                case Button btn:
                    btn.Text = ResolverTexto(clave, btn.Text, traducciones);
                    break;
                case HyperLink hl:
                    hl.Text = ResolverTexto(clave, hl.Text, traducciones);
                    break;
                case HtmlAnchor a:
                    a.InnerText = ResolverTexto(clave, a.InnerText, traducciones);
                    break;
                case HtmlGenericControl generico:
                    generico.InnerText = ResolverTexto(clave, generico.InnerText, traducciones);
                    break;
            }
        }

        private static void TraducirEncabezadosGrilla(GridView gridView, string pagina, Dictionary<string, string> traducciones)
        {
            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                DataControlField columna = gridView.Columns[i];

                if (string.IsNullOrEmpty(columna.HeaderText))
                {
                    continue;
                }

                string clave = $"{pagina}.{gridView.ID}_col{i}";
                columna.HeaderText = ResolverTexto(clave, columna.HeaderText, traducciones);
            }
        }

        private static string ResolverTexto(string clave, string textoActual, Dictionary<string, string> traducciones)
        {
            if (string.IsNullOrWhiteSpace(textoActual))
            {
                return textoActual;
            }

            if (traducciones.TryGetValue(clave, out string textoTraducido))
            {
                return textoTraducido;
            }

            RegistrarClaveNueva(clave, textoActual);

            return textoActual;
        }

        private static void RegistrarClaveNueva(string clave, string textoBase)
        {
            lock (candado)
            {
                if (clavesConocidas.Contains(clave))
                {
                    return;
                }

                clavesConocidas.Add(clave);
            }

            int separador = clave.IndexOf('.');
            string pagina = separador >= 0 ? clave.Substring(0, separador) : clave;
            string controlId = separador >= 0 ? clave.Substring(separador + 1) : clave;

            try
            {
                bll_idioma.RegistrarClaveSiNoExiste(pagina, controlId, textoBase);
            }
            catch
            {
                // El auto-descubrimiento de claves nunca debe romper el render de la pagina.
            }
        }

        private static Dictionary<string, string> ObtenerTraduccionesDeCache(int idIdioma)
        {
            lock (candado)
            {
                if (cacheTraducciones.TryGetValue(idIdioma, out Dictionary<string, string> diccionario))
                {
                    return diccionario;
                }

                try
                {
                    diccionario = bll_idioma.ObtenerDiccionarioTraducciones(idIdioma);
                    cacheTraducciones[idIdioma] = diccionario;
                    return diccionario;
                }
                catch (Exception ex)
                {
                    // No se cachea el fallo: el proximo request vuelve a intentar
                    // contra la base en vez de quedar "roto" hasta reciclar el pool.
                    RegistrarErrorSilencioso("GestorIdioma.ObtenerTraduccionesDeCache", ex);
                    return new Dictionary<string, string>();
                }
            }
        }

        private static List<IIdiomaObservador> ObtenerObservadoresDelRequest()
        {
            if (HttpContext.Current.Items[ClaveObservadoresRequest] == null)
            {
                HttpContext.Current.Items[ClaveObservadoresRequest] = new List<IIdiomaObservador>();
            }

            return (List<IIdiomaObservador>)HttpContext.Current.Items[ClaveObservadoresRequest];
        }

        private static void RegistrarErrorSilencioso(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = HttpContext.Current.Server.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(carpetaLogs);
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
               
            }
        }
    }
}
