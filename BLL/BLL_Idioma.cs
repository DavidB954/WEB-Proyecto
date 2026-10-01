using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_Idioma
    {
        DAL_Idioma dal_idioma = new DAL_Idioma();
        DAL_Traduccion dal_traduccion = new DAL_Traduccion();
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
    }
}
