using System;

namespace BE
{
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
