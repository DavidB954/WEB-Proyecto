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
                    bll_bitacora.RegistrarEvento(
                        null,
                        AccionBitacora.INTEGRIDAD_ERROR,
                        "SEGURIDAD",
                        $"Fallo de integridad del DVV en la tabla '{nombreTabla}' detectado al iniciar la aplicacion."
                    );
                }

                return integro;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar integridad: {ex.Message}", ex);
            }


        }

        public bool EstaIntegra(string nombreTabla)
        {
            try
            {
                return dal_dvv.ObtenerDVV(nombreTabla) == CalcularDVV(nombreTabla);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar integridad de '{nombreTabla}': {ex.Message}", ex);
            }
        }

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
                    throw new Exception($"Tabla no soportada para calculo de DV: {nombreTabla}");
            }
        }

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
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}|{bitacora.Criticidad}";
        }

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
                    throw new Exception($"Tabla no soportada para deteccion de cambios: {nombreTabla}");
            }

            if (mensajes.Count == 0)
            {
                mensajes.Add($"Se elimino un registro de la tabla '{nombreTabla}'.");
            }

            return mensajes;
        }

        private void DetectarCambiosUsuario(List<string> mensajes)
        {
            List<BE_Usuario> usuarios = dal_usuario.Usuarios();

            foreach (var usuario in usuarios)
            {
                string dvhEsperado = HashHelper.GenerarHash(CadenaUsuario(usuario));

                if (usuario.DVH == null)
                {
                    mensajes.Add($"El usuario {usuario.NombreApellido} (ID {usuario.IdUsuario}) no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (usuario.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modifico el usuario {usuario.NombreApellido} (ID {usuario.IdUsuario}).");
                }
            }

            DetectarUsuarioEliminado(usuarios, mensajes);
        }

        private Dictionary<int, string> ObtenerFantasmasUsuario(List<BE_Usuario> usuariosActuales)
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

            var fantasmas = new Dictionary<int, string>();

            foreach (var alta in altas)
            {
                bool tieneBajaLegitima = bajasIds.Contains(alta.Key);
                bool yaNoExiste = !idsActuales.Contains(alta.Key);

                if (yaNoExiste && !tieneBajaLegitima)
                {
                    fantasmas[alta.Key] = alta.Value;
                }
            }

            return fantasmas;
        }

        private void DetectarUsuarioEliminado(List<BE_Usuario> usuariosActuales, List<string> mensajes)
        {
            var fantasmas = ObtenerFantasmasUsuario(usuariosActuales);

            string idsAceptadosCsv = dal_dvv.ObtenerIdsVigentes("Usuario");
            HashSet<int> idsAceptados = string.IsNullOrWhiteSpace(idsAceptadosCsv)
                ? new HashSet<int>()
                : new HashSet<int>(idsAceptadosCsv.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse));

            foreach (var fantasma in fantasmas)
            {
                if (!idsAceptados.Contains(fantasma.Key))
                {
                    mensajes.Add($"Se elimino el usuario {fantasma.Value} (ID {fantasma.Key}) sin registro de baja en la bitacora.");
                }
            }
        }

        private void DetectarCambiosRol(List<string> mensajes)
        {
            List<BE_Rol> roles = dal_rol.ObtenerRoles();

            foreach (var rol in roles)
            {
                string dvhEsperado = HashHelper.GenerarHash(CadenaRol(rol));

                if (rol.DVH == null)
                {
                    mensajes.Add($"El rol {rol.Nombre} (ID {rol.IdRol}) no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (rol.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modifico el rol {rol.Nombre} (ID {rol.IdRol}).");
                }
            }

            DetectarRolEliminado(roles, mensajes);
        }

        private void DetectarRolEliminado(List<BE_Rol> rolesActuales, List<string> mensajes)
        {
            string idsVigentesCsv = dal_dvv.ObtenerIdsVigentes("Rol");

            if (string.IsNullOrWhiteSpace(idsVigentesCsv))
            {
                return;
            }

            var idsVigentes = new HashSet<int>(
                idsVigentesCsv.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse)
            );

            var idsActuales = new HashSet<int>(rolesActuales.Select(r => r.IdRol));

            var faltantes = idsVigentes.Where(id => !idsActuales.Contains(id)).OrderBy(id => id).ToList();

            if (faltantes.Count > 0)
            {
                mensajes.Add($"Falta(n) {faltantes.Count} rol(es) (IDs: {string.Join(", ", faltantes)}).");
            }
        }

        public void RecalcularDVHFilas(string nombreTabla)
        {
            switch (nombreTabla)
            {
                case "Usuario":
                    List<BE_Usuario> usuariosActuales = dal_usuario.Usuarios();

                    foreach (var usuario in usuariosActuales)
                    {
                        dal_usuario.ActualizarDVH(usuario.IdUsuario, HashHelper.GenerarHash(CadenaUsuario(usuario)));
                    }

                    var fantasmasActuales = ObtenerFantasmasUsuario(usuariosActuales).Keys;
                    dal_dvv.ActualizarIdsVigentes("Usuario", string.Join(",", fantasmasActuales));
                    break;

                case "Rol":
                    List<BE_Rol> rolesActuales = dal_rol.ObtenerRoles();

                    foreach (var rol in rolesActuales)
                    {
                        dal_rol.ActualizarDVH(rol.IdRol, HashHelper.GenerarHash(CadenaRol(rol)));
                    }

                    dal_dvv.ActualizarIdsVigentes("Rol", string.Join(",", rolesActuales.Select(r => r.IdRol)));
                    break;

                case "Bitacora":
                    List<BE_Bitacora> eventosBitacora = dal_bitacora.ObtenerBitacora();

                    foreach (var evento in eventosBitacora)
                    {
                        dal_bitacora.ActualizarDVH(evento.IdBitacora, EncryptionHelper.Encriptar(CadenaBitacora(evento)));
                    }

                    string idsVigentes = string.Join(",", eventosBitacora.Select(e => e.IdBitacora));
                    dal_dvv.ActualizarIdsVigentes("Bitacora", idsVigentes);
                    break;

                default:
                    throw new Exception($"Tabla no soportada para recalculo de DV: {nombreTabla}");
            }
        }

        private void DetectarCambiosBitacora(List<string> mensajes)
        {
            List<BE_Bitacora> eventos = dal_bitacora.ObtenerBitacora();

            foreach (var evento in eventos)
            {
                string dvhEsperado = EncryptionHelper.Encriptar(CadenaBitacora(evento));

                if (evento.DVH == null)
                {
                    mensajes.Add($"El registro de bitacora ID {evento.IdBitacora} no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (evento.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modifico el registro de bitacora ID {evento.IdBitacora}.");
                }
            }

            DetectarBitacoraEliminada(eventos, mensajes);
        }

        public List<string> DetectarCambiosBitacoraSiempre()
        {
            var mensajes = new List<string>();
            DetectarCambiosBitacora(mensajes);

            if (mensajes.Count > 0)
            {
                bll_bitacora.RegistrarEvento(
                    null,
                    AccionBitacora.INTEGRIDAD_ERROR,
                    "SEGURIDAD",
                    "Fallo de integridad del DVV en la tabla 'Bitacora' detectado al iniciar la aplicacion."
                );
            }

            return mensajes;
        }

        private void DetectarBitacoraEliminada(List<BE_Bitacora> eventos, List<string> mensajes)
        {
            string idsVigentesCsv = dal_dvv.ObtenerIdsVigentes("Bitacora");

            if (string.IsNullOrWhiteSpace(idsVigentesCsv))
            {
                return;
            }

            var idsVigentes = new HashSet<int>(
                idsVigentesCsv.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse)
            );

            var idsActuales = new HashSet<int>(eventos.Select(e => e.IdBitacora));

            var faltantes = idsVigentes.Where(id => !idsActuales.Contains(id)).OrderBy(id => id).ToList();

            if (faltantes.Count > 0)
            {
                mensajes.Add($"Faltan {faltantes.Count} registro(s) en la bitacora (IDs: {string.Join(", ", faltantes)}).");
            }
        }

        public string GenerarBackUp(BE_Usuario usuarioLogueado)
        {
            try
            {
                string carpetaBackup = dal_dvv.ObtenerCarpetaBackupPorDefecto();

                string nombreArchivo = $"GestionWEB_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                string rutaCompleta = System.IO.Path.Combine(carpetaBackup, nombreArchivo);

                dal_dvv.GenerarBackUp(rutaCompleta);

                bll_bitacora.RegistrarEvento(
                   usuarioLogueado?.IdUsuario,
                    AccionBitacora.BACKUP_GENERADO,
                    "SEGURIDAD",
                    $"Se genero un backup de la base de datos en: {rutaCompleta}"
                );

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar backup: {ex.Message}", ex);
            }

        }

        public List<string> ListarBackupsDisponibles()
        {
            return dal_dvv.ListarBackups();
        }

        public void RestaurarBackup(BE_Usuario usuarioLogueado, string nombreArchivo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreArchivo) || nombreArchivo.IndexOfAny(new[] { '\\', '/' }) >= 0 || nombreArchivo.Contains(".."))
                {
                    throw new Exception("Debe seleccionar un backup valido de la lista.");
                }

                string carpetaBackup = dal_dvv.ObtenerCarpetaBackupPorDefecto();
                string rutaCompleta = System.IO.Path.Combine(carpetaBackup, nombreArchivo);

                dal_dvv.RestaurarBackup(rutaCompleta);

                bll_bitacora.RegistrarEvento(
                   usuarioLogueado?.IdUsuario,
                    AccionBitacora.BACKUP_RESTAURADO,
                    "SEGURIDAD",
                    $"Se restauro la base de datos desde el backup: {rutaCompleta}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al restaurar backup: {ex.Message}", ex);
            }

        }
    }
}
