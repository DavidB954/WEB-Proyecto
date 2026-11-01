using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string ClaveIdiomaPorDefectoRequest = "IdiomaPorDefectoRequest";
        private const string ClaveMarcador = "data-i18n";
        private const string PaginaMaster = "Site1Master";
        private const string PaginaMensajes = "Mensajes";

        private static readonly Regex RegexConfirmacion = new Regex(@"confirmarAccion\(this,\s*'((?:[^'\\]|\\.)*)'\)", RegexOptions.Compiled);

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
                RegistroErrores.Registrar("GestorIdioma.Suscribir", ex);
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
                    RegistroErrores.Registrar("GestorIdioma.CambiarIdioma", ex);
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

            object idiomaDelRequest = HttpContext.Current.Items[ClaveIdiomaPorDefectoRequest];

            if (idiomaDelRequest is int idIdiomaDelRequest)
            {
                return idIdiomaDelRequest;
            }

            try
            {
                int idIdiomaPorDefecto = bll_idioma.ObtenerIdiomaPorDefecto()?.IdIdioma ?? 0;

                HttpContext.Current.Items[ClaveIdiomaPorDefectoRequest] = idIdiomaPorDefecto;

                return idIdiomaPorDefecto;
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("GestorIdioma.ObtenerIdiomaActual", ex);
                return 0;
            }
        }

        public static string Traducir(string textoBase, params object[] argumentos)
        {
            string texto = textoBase;

            try
            {
                if (HttpContext.Current != null && !string.IsNullOrWhiteSpace(textoBase))
                {
                    texto = ResolverTexto(
                        PaginaMensajes + ".m_" + HashHelper.GenerarHash(textoBase).Substring(0, 16),
                        textoBase,
                        ObtenerTraduccionesDeCache(ObtenerIdiomaActual()));
                }
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("GestorIdioma.Traducir", ex);
            }

            if (argumentos == null || argumentos.Length == 0)
            {
                return texto;
            }

            try
            {
                return string.Format(texto, argumentos);
            }
            catch (FormatException)
            {
                return string.Format(textoBase, argumentos);
            }
        }

        public static void CargarSelector(DropDownList selector)
        {
            selector.DataSource = bll_idioma.ObtenerIdiomasActivos();
            selector.DataBind();

            string idIdiomaActual = ObtenerIdiomaActual().ToString();

            if (selector.Items.FindByValue(idIdiomaActual) != null)
            {
                selector.SelectedValue = idIdiomaActual;
            }
        }

        public static void AplicarTituloPagina(Page pagina)
        {
            try
            {
                if (pagina.Header == null)
                {
                    return;
                }

                HtmlTitle controlTitulo = pagina.Header.Controls.OfType<HtmlTitle>().FirstOrDefault();
                string titulo = string.IsNullOrWhiteSpace(pagina.Title) ? controlTitulo?.Text : pagina.Title;

                if (string.IsNullOrWhiteSpace(titulo))
                {
                    return;
                }

                string nombrePagina = pagina.Master != null ? PaginaMaster : (pagina.GetType().BaseType ?? pagina.GetType()).Name;

                pagina.Title = ResolverTexto(nombrePagina + "._titulo", titulo.Trim(), ObtenerTraduccionesDeCache(ObtenerIdiomaActual()));
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("GestorIdioma.AplicarTituloPagina", ex);
            }
        }

        public static void InvalidarCache(int idIdioma)
        {
            lock (candado)
            {
                cacheTraducciones.Remove(idIdioma);
            }
        }

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
                RegistroErrores.Registrar("GestorIdioma.AplicarTraducciones", ex);
            }
        }

        public static void AplicarTraduccionesGrillas(Control pagina)
        {
            try
            {
                string nombrePagina = (pagina.GetType().BaseType ?? pagina.GetType()).Name;
                Dictionary<string, string> traducciones = ObtenerTraduccionesDeCache(ObtenerIdiomaActual());

                RecorrerGrillas(pagina, nombrePagina, nombrePagina, traducciones);
            }
            catch (Exception ex)
            {
                RegistroErrores.Registrar("GestorIdioma.AplicarTraduccionesGrillas", ex);
            }
        }

        private static void RecorrerGrillas(Control control, string paginaActual, string paginaContenido, Dictionary<string, string> traducciones)
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
                foreach (Control fila in gridView.Controls)
                {
                    TraducirContenidoGrilla(fila, gridView.ID, paginaActual, traducciones);
                }

                return;
            }

            foreach (Control hijo in control.Controls)
            {
                RecorrerGrillas(hijo, paginaActual, paginaContenido, traducciones);
            }
        }

        private static void TraducirContenidoGrilla(Control control, string idGrilla, string pagina, Dictionary<string, string> traducciones)
        {
            if (EsTraducible(control))
            {
                AplicarAlControl(control, pagina, idGrilla + "_" + control.ID, traducciones);
            }

            TraducirAvisos(control, pagina, idGrilla + "_" + control.ID, traducciones);

            foreach (Control hijo in control.Controls)
            {
                TraducirContenidoGrilla(hijo, idGrilla, pagina, traducciones);
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

            if (control is Repeater)
            {
                return;
            }

            if (EsTraducible(control))
            {
                AplicarAlControl(control, paginaActual, control.ID, traducciones);
            }

            TraducirAvisos(control, paginaActual, control.ID, traducciones);

            foreach (Control hijo in control.Controls)
            {
                RecorrerControl(hijo, paginaActual, paginaContenido, traducciones);
            }
        }

        private static void TraducirAvisos(Control control, string pagina, string controlId, Dictionary<string, string> traducciones)
        {
            if (string.IsNullOrEmpty(control.ID))
            {
                return;
            }

            if (control is WebControl webControl && !string.IsNullOrWhiteSpace(webControl.ToolTip))
            {
                webControl.ToolTip = ResolverTexto($"{pagina}.{controlId}_tooltip", webControl.ToolTip, traducciones);
            }

            switch (control)
            {
                case Button btn:
                    btn.OnClientClick = TraducirConfirmacion(btn.OnClientClick, pagina, controlId, traducciones);
                    break;
                case LinkButton lnk:
                    lnk.OnClientClick = TraducirConfirmacion(lnk.OnClientClick, pagina, controlId, traducciones);
                    break;
            }
        }

        private static string TraducirConfirmacion(string script, string pagina, string controlId, Dictionary<string, string> traducciones)
        {
            if (string.IsNullOrEmpty(script))
            {
                return script;
            }

            return RegexConfirmacion.Replace(script, coincidencia =>
            {
                string mensaje = coincidencia.Groups[1].Value.Replace("\\'", "'");
                string traducido = ResolverTexto($"{pagina}.{controlId}_confirm", mensaje, traducciones);

                return "confirmarAccion(this, '" + HttpUtility.JavaScriptStringEncode(traducido) + "')";
            });
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

        private static void AplicarAlControl(Control control, string pagina, string controlId, Dictionary<string, string> traducciones)
        {
            string clave = pagina + "." + controlId;

            switch (control)
            {
                case BaseValidator validador:
                    validador.ErrorMessage = ResolverTexto(clave, validador.ErrorMessage, traducciones);
                    break;
                case Label lbl:
                    lbl.Text = ResolverTexto(clave, lbl.Text, traducciones);
                    break;
                case TextBox txt:
                    txt.Attributes["placeholder"] = ResolverTexto(clave, txt.Attributes["placeholder"], traducciones);
                    break;
                case ListControl lista:
                    foreach (ListItem item in lista.Items)
                    {
                        item.Text = ResolverTexto(clave + "_" + item.Value, item.Text, traducciones);
                    }
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
            if (gridView.AllowPaging)
            {
                PagerSettings paginador = gridView.PagerSettings;
                string prefijo = $"{pagina}.{gridView.ID}_pag";

                paginador.FirstPageText = ResolverTexto(prefijo + "Primera", paginador.FirstPageText, traducciones);
                paginador.PreviousPageText = ResolverTexto(prefijo + "Anterior", paginador.PreviousPageText, traducciones);
                paginador.NextPageText = ResolverTexto(prefijo + "Siguiente", paginador.NextPageText, traducciones);
                paginador.LastPageText = ResolverTexto(prefijo + "Ultima", paginador.LastPageText, traducciones);
            }

            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                DataControlField columna = gridView.Columns[i];

                if (columna is CommandField comando && comando.ShowSelectButton)
                {
                    comando.SelectText = ResolverTexto($"{pagina}.{gridView.ID}_col{i}_sel", comando.SelectText, traducciones);
                }

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
                    RegistroErrores.Registrar("GestorIdioma.ObtenerTraduccionesDeCache", ex);
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
    }
}
