using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SERVICIOS
{
    public class DocumentoPdf
    {
        private const float AnchoPagina = 595f;
        private const float AltoPagina = 842f;
        private const float Margen = 56f;
        private const float Interlineado = 1.35f;

        private const float AnchoPromedioCaracter = 0.54f;

        private static readonly Encoding WinAnsi = Encoding.GetEncoding(1252);

        private readonly StringBuilder contenido = new StringBuilder();
        private float y = AltoPagina - Margen;

        public void AgregarTexto(string texto, float tamano = 11f, bool negrita = false, float espacioAntes = 0f)
        {
            y -= espacioAntes;

            foreach (string linea in PartirEnLineas(texto, tamano))
            {
                BajarCursor(tamano * Interlineado);

                contenido.Append(string.Format(CultureInfo.InvariantCulture,
                    "BT /{0} {1:0.##} Tf {2:0.##} {3:0.##} Td ({4}) Tj ET\n",
                    negrita ? "F2" : "F1", tamano, Margen, y, Escapar(linea)));
            }
        }

        public void AgregarSeparador(float espacioAntes = 10f)
        {
            BajarCursor(espacioAntes);

            contenido.Append(string.Format(CultureInfo.InvariantCulture,
                "0.6 w {0:0.##} {2:0.##} m {1:0.##} {2:0.##} l S\n", Margen, AnchoPagina - Margen, y));
        }

        public byte[] Generar()
        {
            byte[] flujo = WinAnsi.GetBytes(contenido.ToString());

            using (MemoryStream salida = new MemoryStream())
            {
                List<long> posiciones = new List<long>();

                Escribir(salida, "%PDF-1.4\n");

                string[] objetos =
                {
                    "<< /Type /Catalog /Pages 2 0 R >>",
                    "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                    string.Format(CultureInfo.InvariantCulture,
                        "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {0:0} {1:0}] /Contents 4 0 R /Resources << /Font << /F1 5 0 R /F2 6 0 R >> >> >>",
                        AnchoPagina, AltoPagina),
                    null,
                    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>",
                    "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>"
                };

                for (int i = 0; i < objetos.Length; i++)
                {
                    posiciones.Add(salida.Position);
                    Escribir(salida, (i + 1) + " 0 obj\n");

                    if (objetos[i] != null)
                    {
                        Escribir(salida, objetos[i]);
                    }
                    else
                    {
                        Escribir(salida, "<< /Length " + flujo.Length + " >>\nstream\n");
                        salida.Write(flujo, 0, flujo.Length);
                        Escribir(salida, "\nendstream");
                    }

                    Escribir(salida, "\nendobj\n");
                }

                long inicioTabla = salida.Position;

                Escribir(salida, "xref\n0 " + (objetos.Length + 1) + "\n0000000000 65535 f \n");

                foreach (long posicion in posiciones)
                {
                    Escribir(salida, posicion.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
                }

                Escribir(salida, "trailer\n<< /Size " + (objetos.Length + 1) + " /Root 1 0 R >>\nstartxref\n" + inicioTabla + "\n%%EOF\n");

                return salida.ToArray();
            }
        }

        private void BajarCursor(float cantidad)
        {
            y -= cantidad;

            if (y < Margen)
            {
                throw new InvalidOperationException("El contenido no entra en una pagina del PDF.");
            }
        }

        private static IEnumerable<string> PartirEnLineas(string texto, float tamano)
        {
            int maximo = (int)((AnchoPagina - 2 * Margen) / (tamano * AnchoPromedioCaracter));

            StringBuilder linea = new StringBuilder();

            foreach (string palabraEntera in (texto ?? string.Empty).Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string palabra = palabraEntera;

                if (linea.Length > 0 && linea.Length + 1 + palabra.Length > maximo)
                {
                    yield return linea.ToString();
                    linea.Clear();
                }

                while (palabra.Length > maximo)
                {
                    yield return palabra.Substring(0, maximo);
                    palabra = palabra.Substring(maximo);
                }

                if (linea.Length > 0)
                {
                    linea.Append(' ');
                }

                linea.Append(palabra);
            }

            if (linea.Length > 0)
            {
                yield return linea.ToString();
            }
        }

        private static string Escapar(string texto)
        {
            return texto.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static void Escribir(Stream salida, string texto)
        {
            byte[] bytes = WinAnsi.GetBytes(texto);
            salida.Write(bytes, 0, bytes.Length);
        }
    }
}
