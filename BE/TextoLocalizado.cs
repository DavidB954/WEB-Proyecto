using System;

namespace BE
{
    // Puente para traducir mensajes desde BE y BLL sin que dependan de la capa web:
    // Presentacion registra el traductor al arrancar (Global.asax). Sin traductor
    // (ej. fuera del sitio) devuelve el texto en español con los argumentos aplicados.
    public static class TextoLocalizado
    {
        public static Func<string, object[], string> Traductor { get; set; }

        public static string T(string textoBase, params object[] argumentos)
        {
            if (Traductor != null)
            {
                return Traductor(textoBase, argumentos);
            }

            return argumentos.Length == 0 ? textoBase : string.Format(textoBase, argumentos);
        }
    }
}
