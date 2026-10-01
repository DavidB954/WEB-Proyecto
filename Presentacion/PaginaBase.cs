using SERVICIOS;
using System;
using System.Web.UI;

namespace Presentacion
{
    /// <summary>
    /// Base de la que heredan todas las paginas que usan Site1.Master. Se
    /// suscribe como Observer ante GestorIdioma: al suscribirse recibe una
    /// sincronizacion inmediata (idioma actual de la sesion) y, si el combo de
    /// idioma de la Master dispara un cambio durante este mismo postback,
    /// vuelve a recibir la notificacion y reaplica las traducciones.
    /// </summary>
    public class PaginaBase : Page, IIdiomaObservador
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

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

        public void ActualizarIdioma()
        {
            GestorIdioma.AplicarTraducciones(this);
        }
    }
}
