using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace BLL
{
    public class BLL_DVV
    {
        DAL_DVV dal_dvv = new DAL_DVV();
        DAL_Usuario dal_usuario = new DAL_Usuario();
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_Bitacora dal_bitacora = new DAL_Bitacora();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public void ActualizarDVV(string nombreTabla)
        {
            try
            {
                string concatenacion = CalcularDVV(nombreTabla);

                dal_dvv.ActualizarDVV(concatenacion, nombreTabla);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar DVV: {ex.Message}", ex);
            }

        }

        public bool VerificarIntegridad(string nombreTabla)
        {
            try
            {
                string dvvAlmacenado = dal_dvv.ObtenerDVV(nombreTabla);

                string dvvRecalculado = CalcularDVV(nombreTabla);

                bool integro = dvvAlmacenado == dvvRecalculado;

                if (!integro)
                {
                    // Se corre antes del login (arranque de la app), por eso IdUsuario es null.
                    bll_bitacora.RegistrarEvento(
                        null,
                        AccionBitacora.INTEGRIDAD_ERROR,
                        "SEGURIDAD",
                        $"Fallo de integridad del DVV en la tabla '{nombreTabla}' detectado al iniciar la aplicación."
                    );
                }

                return integro;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar integridad: {ex.Message}", ex);
            }


        }

        //Calcula el DV de tabla (DVV): calcula el DV de cada fila (hash para Usuario/Rol, cifrado reversible para Bitacora) y luego hashea el total.
        //El resumen final es siempre hash: da un largo fijo de 64 caracteres que entra en la columna DVV (el cifrado AES de la concatenación de todas las filas crecería sin límite y se truncaría al guardarse).
        public string CalcularDVV(string nombreTabla)
        {
            try
            {
                List<string> hashesFila = ObtenerHashesFila(nombreTabla);

                StringBuilder concatenacion = new StringBuilder();
                foreach (var hash in hashesFila)
                {
                    concatenacion.Append(hash);
                }

                return HashHelper.GenerarHash(concatenacion.ToString());
            }
            catch (Exception ex)
            {

                throw new Exception($"Error al calcular DVV: {ex.Message}", ex);
            }

        }

        private List<string> ObtenerHashesFila(string nombreTabla)
        {
            switch (nombreTabla)
            {
                case "Usuario":
                    return dal_usuario.Usuarios().Select(u => HashHelper.GenerarHash(CadenaUsuario(u))).ToList();

                case "Rol":
                    return dal_rol.ObtenerRoles().Select(r => HashHelper.GenerarHash(CadenaRol(r))).ToList();

                case "Bitacora":
                    return dal_bitacora.ObtenerBitacora().Select(b => EncryptionHelper.Encriptar(CadenaBitacora(b))).ToList();

                default:
                    throw new Exception($"Tabla no soportada para cálculo de DV: {nombreTabla}");
            }
        }

        //Tiene que ser idéntica a BLL_Usuario.CadenaUsuario (incluye DNI, igual que la cadena que arma CRUD_Usuarios.aspx.cs al guardar).
        private string CadenaUsuario(BE_Usuario usuario)
        {
            string activo = usuario.Activo ? "1" : "0";
            return $"{usuario.Nombre}|{usuario.Apellido}|{usuario.Email}|{usuario.HashPassword}|{usuario.DNI}|{usuario.IntentosFallidos}|{activo}";
        }

        private string CadenaRol(BE_Rol rol)
        {
            return $"{rol.IdRol}|{rol.Nombre}";
        }

        private string CadenaBitacora(BE_Bitacora bitacora)
        {
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}";
        }

        //Mensajes de qué se modificó/eliminó en una tabla, para mostrarle al Webmaster cuando VerificarIntegridad da false.
        public List<string> DetectarCambios(string nombreTabla)
        {
            var mensajes = new List<string>();

            switch (nombreTabla)
            {
                case "Usuario":
                    DetectarCambiosUsuario(mensajes);
                    break;

                case "Rol":
                    DetectarCambiosRol(mensajes);
                    break;

                case "Bitacora":
                    DetectarCambiosBitacora(mensajes);
                    break;

                default:
                    throw new Exception($"Tabla no soportada para detección de cambios: {nombreTabla}");
            }

            //Si ninguna fila individual quedó marcada, el DVV igual falló: probablemente desapareció una fila entera.
            if (mensajes.Count == 0)
            {
                mensajes.Add($"Se eliminó un registro de la tabla '{nombreTabla}'.");
            }

            return mensajes;
        }

        private void DetectarCambiosUsuario(List<string> mensajes)
        {
            List<BE_Usuario> usuarios = dal_usuario.Usuarios();

            foreach (var usuario in usuarios)
            {
                string dvhEsperado = HashHelper.GenerarHash(CadenaUsuario(usuario));

                //DVH null = todavía no se calculó línea base (falta "Recalcular DV"), no es corrupción.
                if (usuario.DVH != null && usuario.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el usuario {usuario.NombreApellido} (ID {usuario.IdUsuario}).");
                }
            }

            if (mensajes.Count == 0)
            {
                DetectarUsuarioEliminado(usuarios, mensajes);
            }
        }

        //Cruza altas y bajas registradas en bitácora para reconstruir el nombre de un usuario que ya no existe y nunca tuvo una baja legítima.
        private void DetectarUsuarioEliminado(List<BE_Usuario> usuariosActuales, List<string> mensajes)
        {
            List<BE_Bitacora> bitacora = bll_bitacora.ObtenerBitacora();

            var altas = new Dictionary<int, string>();
            var bajasIds = new HashSet<int>();

            foreach (var evento in bitacora)
            {
                if (evento.Accion == AccionBitacora.USUARIO_ALTA)
                {
                    Match match = Regex.Match(evento.Descripcion, @"Se crea un nuevo usuario:\s*(.+?)\s*\(ID:\s*(\d+)\)");
                    if (match.Success)
                    {
                        int id = int.Parse(match.Groups[2].Value);
                        altas[id] = match.Groups[1].Value;
                    }
                }
                else if (evento.Accion == AccionBitacora.USUARIO_BAJA)
                {
                    Match match = Regex.Match(evento.Descripcion, @"con ID:\s*(\d+)");
                    if (match.Success)
                    {
                        bajasIds.Add(int.Parse(match.Groups[1].Value));
                    }
                }
            }

            var idsActuales = new HashSet<int>(usuariosActuales.Select(u => u.IdUsuario));

            foreach (var alta in altas)
            {
                bool tieneBajaLegitima = bajasIds.Contains(alta.Key);
                bool yaNoExiste = !idsActuales.Contains(alta.Key);

                if (yaNoExiste && !tieneBajaLegitima)
                {
                    mensajes.Add($"Se eliminó el usuario {alta.Value} (ID {alta.Key}) sin registro de baja en la bitácora.");
                }
            }
        }

        private void DetectarCambiosRol(List<string> mensajes)
        {
            List<BE_Rol> roles = dal_rol.ObtenerRoles();

            foreach (var rol in roles)
            {
                string dvhEsperado = HashHelper.GenerarHash(CadenaRol(rol));

                if (rol.DVH != null && rol.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el rol {rol.Nombre} (ID {rol.IdRol}).");
                }
            }
        }

        //Recalcula y persiste el DVH fila por fila con los valores actuales. Se usa desde "Recalcular DV" para fijar una nueva línea base (p.ej. después de un restore o de aceptar un cambio).
        public void RecalcularDVHFilas(string nombreTabla)
        {
            switch (nombreTabla)
            {
                case "Usuario":
                    foreach (var usuario in dal_usuario.Usuarios())
                    {
                        dal_usuario.ActualizarDVH(usuario.IdUsuario, HashHelper.GenerarHash(CadenaUsuario(usuario)));
                    }
                    break;

                case "Rol":
                    foreach (var rol in dal_rol.ObtenerRoles())
                    {
                        dal_rol.ActualizarDVH(rol.IdRol, HashHelper.GenerarHash(CadenaRol(rol)));
                    }
                    break;

                case "Bitacora":
                    foreach (var evento in dal_bitacora.ObtenerBitacora())
                    {
                        dal_bitacora.ActualizarDVH(evento.IdBitacora, EncryptionHelper.Encriptar(CadenaBitacora(evento)));
                    }
                    break;

                default:
                    throw new Exception($"Tabla no soportada para recálculo de DV: {nombreTabla}");
            }
        }

        private void DetectarCambiosBitacora(List<string> mensajes)
        {
            List<BE_Bitacora> eventos = dal_bitacora.ObtenerBitacora();

            foreach (var evento in eventos)
            {
                string dvhEsperado = EncryptionHelper.Encriptar(CadenaBitacora(evento));

                if (evento.DVH != null && evento.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el registro de bitácora ID {evento.IdBitacora}.");
                }
            }
        }

        public void GenerarBackUp(BE_Usuario usuarioLogueado, string rutaBackup)
        {
            try
            {
                dal_dvv.GenerarBackUp(rutaBackup);

                bll_bitacora.RegistrarEvento(
                   usuarioLogueado?.IdUsuario,
                    AccionBitacora.BACKUP_GENERADO,
                    "SEGURIDAD",
                    $"Se generó un backup de la base de datos en: {rutaBackup}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar backup: {ex.Message}", ex);
            }

        }

        public void RestaurarBackup(BE_Usuario usuarioLogueado, string rutaBackup)
        {
            try
            {
                dal_dvv.RestaurarBackup(rutaBackup);

                bll_bitacora.RegistrarEvento(
                   usuarioLogueado?.IdUsuario,
                    AccionBitacora.BACKUP_RESTAURADO,
                    "SEGURIDAD",
                    $"Se restauró la base de datos desde el backup: {rutaBackup}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al restaurar backup: {ex.Message}", ex);
            }

        }
    }
}
