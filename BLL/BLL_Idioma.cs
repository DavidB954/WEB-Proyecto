using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace BLL
{
    public class BLL_Idioma
    {
        DAL_Idioma dal_idioma = new DAL_Idioma();
        DAL_Traduccion dal_traduccion = new DAL_Traduccion();

        private const string ElementoRaizXml = "Traducciones";
        private const string ElementoTraduccionXml = "Traduccion";
        private const int LargoMaximoTraduccion = 300; // Traduccion.Texto es NVARCHAR(300).
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public List<BE_Idioma> ObtenerIdiomas()
        {
            try
            {
                return dal_idioma.ObtenerIdiomas();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener idiomas: {ex.Message}", ex);
            }
        }

        public List<BE_Idioma> ObtenerIdiomasActivos()
        {
            try
            {
                return dal_idioma.ObtenerIdiomasActivos();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener idiomas activos: {ex.Message}", ex);
            }
        }

        public BE_Idioma ObtenerIdiomaPorDefecto()
        {
            try
            {
                return dal_idioma.ObtenerIdiomaPorDefecto();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el idioma predeterminado: {ex.Message}", ex);
            }
        }

        public void AgregarIdioma(BE_Usuario usuarioLogueado, BE_Idioma idioma)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idioma.Codigo) || string.IsNullOrWhiteSpace(idioma.Nombre))
                {
                    throw new Exception("Debe indicar codigo y nombre del idioma.");
                }

                if (dal_idioma.ExisteCodigo(idioma.Codigo))
                {
                    throw new Exception($"Ya existe un idioma con el codigo '{idioma.Codigo}'.");
                }

                idioma.Activo = true;

                dal_idioma.AgregarIdioma(idioma);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.IDIOMA_ALTA, "IDIOMA", $"Se agrego el idioma {idioma.Nombre} ({idioma.Codigo})");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar idioma: {ex.Message}", ex);
            }
        }

        public void EliminarIdioma(BE_Usuario usuarioLogueado, int idIdioma, int idIdiomaEnUso)
        {
            try
            {
                if (idIdioma == idIdiomaEnUso)
                {
                    throw new Exception("No se puede borrar un idioma en uso.");
                }

                BE_Idioma idioma = dal_idioma.ObtenerIdiomaPorId(idIdioma);

                if (idioma == null)
                {
                    throw new Exception("El idioma no existe.");
                }

                dal_idioma.EliminarIdioma(idIdioma);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.IDIOMA_BAJA, "IDIOMA", $"Se elimino el idioma {idioma.Nombre} ({idioma.Codigo})");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar idioma: {ex.Message}", ex);
            }
        }

        public List<BE_Traduccion> ObtenerTraduccionesParaGrilla(int idIdioma)
        {
            try
            {
                return dal_traduccion.ObtenerTraduccionesParaGrilla(idIdioma);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las traducciones: {ex.Message}", ex);
            }
        }

        public Dictionary<string, string> ObtenerDiccionarioTraducciones(int idIdioma)
        {
            try
            {
                return dal_traduccion.ObtenerDiccionarioTraducciones(idIdioma);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el diccionario de traducciones: {ex.Message}", ex);
            }
        }

        public void GuardarTraduccion(BE_Usuario usuarioLogueado, int idIdioma, int idClave, string texto)
        {
            try
            {
                dal_traduccion.GuardarTraduccion(idIdioma, idClave, texto);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TRADUCCION_MODIFICACION, "IDIOMA", $"Se modifico la traduccion IdClave={idClave} para el idioma IdIdioma={idIdioma}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar traduccion: {ex.Message}", ex);
            }
        }

        public int RegistrarClaveSiNoExiste(string pagina, string controlId, string textoBase)
        {
            try
            {
                int? idClaveExistente = dal_traduccion.ObtenerIdClave(pagina, controlId);

                if (idClaveExistente.HasValue)
                {
                    return idClaveExistente.Value;
                }

                int idClave = dal_traduccion.RegistrarClave(pagina, controlId, textoBase);

                BE_Idioma idiomaPorDefecto = dal_idioma.ObtenerIdiomaPorDefecto();

                if (idiomaPorDefecto != null)
                {
                    dal_traduccion.GuardarTraduccion(idiomaPorDefecto.IdIdioma, idClave, textoBase);
                }

                return idClave;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar la clave de traduccion: {ex.Message}", ex);
            }
        }

        // ---- Exportacion e importacion de traducciones en XML ----

        public string GenerarXmlTraducciones(BE_Usuario usuarioLogueado, int idIdioma)
        {
            try
            {
                BE_Idioma idioma = dal_idioma.ObtenerIdiomaPorId(idIdioma);

                if (idioma == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El idioma seleccionado ya no existe."));
                }

                List<BE_Traduccion> traducciones = dal_traduccion.ObtenerTraduccionesParaGrilla(idIdioma);

                XDocument documento = new XDocument(
                    new XDeclaration("1.0", "utf-8", null),
                    new XElement(ElementoRaizXml,
                        new XAttribute("codigoIdioma", idioma.Codigo),
                        new XAttribute("nombreIdioma", idioma.Nombre),
                        new XAttribute("fechaExportacion", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss")),
                        traducciones.Select(t => new XElement(ElementoTraduccionXml,
                            new XAttribute("pagina", t.Pagina),
                            new XAttribute("control", t.ControlId),
                            new XElement("TextoBase", t.TextoBase),
                            new XElement("Texto", t.Texto ?? string.Empty)))));

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TRADUCCION_EXPORTADA, "IDIOMA",
                    $"{usuarioLogueado.NombreApellido} exporto {traducciones.Count} traducciones del idioma '{idioma.Codigo}' a XML.");

                // XDocument.ToString() omite la declaracion <?xml ...?>: se agrega a mano.
                return documento.Declaration + Environment.NewLine + documento;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al exportar las traducciones: {ex.Message}", ex);
            }
        }

        // Todo o nada: primero se valida el archivo completo y recien despues se
        // guarda, en una sola transaccion. Solo actualiza claves que ya existen en
        // el catalogo; las desconocidas y las vacias se omiten y se informan.
        public BE_ResultadoImportacion ImportarXmlTraducciones(BE_Usuario usuarioLogueado, int idIdioma, Stream contenidoXml)
        {
            try
            {
                BE_Idioma idioma = dal_idioma.ObtenerIdiomaPorId(idIdioma);

                if (idioma == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El idioma seleccionado ya no existe."));
                }

                XElement raiz = LeerXmlTraducciones(contenidoXml);

                string codigoArchivo = (string)raiz.Attribute("codigoIdioma");

                if (!string.Equals(codigoArchivo, idioma.Codigo, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(TextoLocalizado.T(
                        "El archivo corresponde al idioma '{0}' y no al idioma seleccionado ('{1}').", codigoArchivo, idioma.Codigo));
                }

                List<BE_Traduccion> catalogo = dal_traduccion.ObtenerTraduccionesParaGrilla(idIdioma);
                Dictionary<string, BE_Traduccion> porClave = catalogo.ToDictionary(t => t.Pagina + "." + t.ControlId);

                BE_ResultadoImportacion resultado = new BE_ResultadoImportacion();
                Dictionary<int, string> aGuardar = new Dictionary<int, string>();
                int numero = 0;

                foreach (XElement elemento in raiz.Elements(ElementoTraduccionXml))
                {
                    numero++;

                    string pagina = (string)elemento.Attribute("pagina");
                    string control = (string)elemento.Attribute("control");
                    string texto = ((string)elemento.Element("Texto"))?.Trim();

                    if (string.IsNullOrWhiteSpace(pagina) || string.IsNullOrWhiteSpace(control))
                    {
                        throw new InvalidOperationException(TextoLocalizado.T("La traduccion numero {0} no indica pagina y control.", numero));
                    }

                    if (texto != null && texto.Length > LargoMaximoTraduccion)
                    {
                        throw new InvalidOperationException(TextoLocalizado.T(
                            "La traduccion de {0}.{1} supera los {2} caracteres.", pagina, control, LargoMaximoTraduccion));
                    }

                    if (string.IsNullOrEmpty(texto) || !porClave.TryGetValue(pagina + "." + control, out BE_Traduccion existente))
                    {
                        resultado.Omitidas++;
                    }
                    else if (texto == existente.Texto)
                    {
                        resultado.SinCambios++;
                    }
                    else
                    {
                        aGuardar[existente.IdClave] = texto;
                    }
                }

                if (numero == 0)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El archivo no contiene traducciones."));
                }

                dal_traduccion.GuardarTraducciones(idIdioma, aGuardar);
                resultado.Actualizadas = aGuardar.Count;

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.TRADUCCION_IMPORTADA, "IDIOMA",
                    $"{usuarioLogueado.NombreApellido} importo traducciones del idioma '{idioma.Codigo}' desde XML: {resultado.Actualizadas} actualizadas, {resultado.SinCambios} sin cambios, {resultado.Omitidas} omitidas.");

                return resultado;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al importar las traducciones: {ex.Message}", ex);
            }
        }

        // El archivo lo sube un usuario: se prohibe el DTD y no se resuelven
        // referencias externas, para evitar ataques XXE y de expansion de entidades.
        private XElement LeerXmlTraducciones(Stream contenidoXml)
        {
            XmlReaderSettings opciones = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            XDocument documento;

            try
            {
                using (XmlReader lector = XmlReader.Create(contenidoXml, opciones))
                {
                    documento = XDocument.Load(lector);
                }
            }
            catch (XmlException ex)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo no es un XML valido."), ex);
            }

            if (documento.Root == null || documento.Root.Name != ElementoRaizXml)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El archivo no tiene el formato de traducciones del sistema."));
            }

            return documento.Root;
        }
    }
}
