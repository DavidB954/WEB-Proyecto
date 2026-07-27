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

        //Chequeo de integridad de SOLO LECTURA: compara el DVV guardado contra el recalculado, sin registrar nada en bitácora.
        //Se usa para decisiones de UI (ej. deshabilitar "Generar BackUp" si la base está comprometida).
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
            return $"{bitacora.IdUsuario}|{bitacora.FechaHora}|{bitacora.Accion}|{bitacora.Modulo}|{bitacora.IP}|{bitacora.Descripcion}|{bitacora.NombreMaquina}|{bitacora.Criticidad}";
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

                //DVH null: no es una fila vieja anterior a la funcionalidad (ObtenerUsuarioPorEmail y
                //RecalcularDVHFilas garantizan que toda fila real siempre tenga uno), así que solo puede
                //ser una fila insertada por fuera del sistema (ej. SQL directo) sin pasar por el cálculo.
                //Antes esto se ignoraba en silencio (la fila no generaba mensaje) y, si ninguna otra fila
                //tenía problemas, el fallback genérico de DetectarCambios terminaba diciendo "se eliminó
                //un registro" para lo que en realidad era una inserción.
                if (usuario.DVH == null)
                {
                    mensajes.Add($"El usuario {usuario.NombreApellido} (ID {usuario.IdUsuario}) no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (usuario.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el usuario {usuario.NombreApellido} (ID {usuario.IdUsuario}).");
                }
            }

            //Se corre siempre, no solo cuando no hubo modificaciones: una fila modificada y una fila eliminada
            //pueden pasar al mismo tiempo, y antes la segunda quedaba enmascarada por la primera.
            DetectarUsuarioEliminado(usuarios, mensajes);
        }

        //Cruza altas y bajas registradas en bitácora para reconstruir qué usuarios ya no existen y nunca
        //tuvieron una baja legítima ("fantasmas"): ID -> nombre reconstruido desde el evento de alta.
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

            //IDs de fantasma YA reportados y aceptados en el último "Recalcular DV" sobre Usuario (ver
            //RecalcularDVHFilas). Ojo: esto NO es "IDs que existían en el último recálculo" (ese era el bug:
            //un usuario creado y borrado por SQL DESPUÉS del recálculo tampoco está en ese conjunto, y quedaba
            //sin reportar igual que uno viejo ya aceptado). Es la lista de fantasmas puntuales que el webmaster
            //ya vio y aceptó; cualquier fantasma que no esté ahí es una novedad y se reporta.
            string idsAceptadosCsv = dal_dvv.ObtenerIdsVigentes("Usuario");
            HashSet<int> idsAceptados = string.IsNullOrWhiteSpace(idsAceptadosCsv)
                ? new HashSet<int>()
                : new HashSet<int>(idsAceptadosCsv.Split(',').Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse));

            foreach (var fantasma in fantasmas)
            {
                if (!idsAceptados.Contains(fantasma.Key))
                {
                    mensajes.Add($"Se eliminó el usuario {fantasma.Value} (ID {fantasma.Key}) sin registro de baja en la bitácora.");
                }
            }
        }

        private void DetectarCambiosRol(List<string> mensajes)
        {
            List<BE_Rol> roles = dal_rol.ObtenerRoles();

            foreach (var rol in roles)
            {
                string dvhEsperado = HashHelper.GenerarHash(CadenaRol(rol));

                //Ver el mismo comentario en DetectarCambiosUsuario: DVH null solo puede ser una fila
                //insertada por fuera del sistema, no una fila vieja legítima.
                if (rol.DVH == null)
                {
                    mensajes.Add($"El rol {rol.Nombre} (ID {rol.IdRol}) no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (rol.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el rol {rol.Nombre} (ID {rol.IdRol}).");
                }
            }

            DetectarRolEliminado(roles, mensajes);
        }

        //A diferencia de Usuario, Rol no tiene eventos de alta/baja en bitácora (AccionBitacora.ROL_ALTA/
        //ROL_BAJA existen en el enum pero nunca se registran: BLL_Rol solo loguea asignación/quite de rol a
        //un usuario), así que no hay forma de reconstruir el nombre de un rol borrado cruzando bitácora como
        //en DetectarUsuarioEliminado. Se usa el mismo mecanismo que Bitácora (IdsVigentes): compara los IDs
        //actuales contra los aceptados la última vez que se corrió "Recalcular DV", y reporta los que
        //desaparecieron desde entonces. Sin esto, borrar un rol entero por SQL era indetectable: la fila
        //simplemente dejaba de aparecer en dal_rol.ObtenerRoles() y no había ningún control que la extrañara.
        private void DetectarRolEliminado(List<BE_Rol> rolesActuales, List<string> mensajes)
        {
            string idsVigentesCsv = dal_dvv.ObtenerIdsVigentes("Rol");

            //Todavía no hay una base aceptada (recién migrado, nunca se corrió "Recalcular DV"): no hay contra qué comparar.
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

        //Recalcula y persiste el DVH fila por fila con los valores actuales.
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

                    //Acepta los fantasmas detectados HOY (usuarios eliminados sin baja) como ya revisados:
                    //a partir de acá, DetectarUsuarioEliminado solo va a reportar fantasmas NUEVOS que
                    //aparezcan después de este recálculo, no los que el webmaster ya vio y aceptó ahora.
                    var fantasmasActuales = ObtenerFantasmasUsuario(usuariosActuales).Keys;
                    dal_dvv.ActualizarIdsVigentes("Usuario", string.Join(",", fantasmasActuales));
                    break;

                case "Rol":
                    List<BE_Rol> rolesActuales = dal_rol.ObtenerRoles();

                    foreach (var rol in rolesActuales)
                    {
                        dal_rol.ActualizarDVH(rol.IdRol, HashHelper.GenerarHash(CadenaRol(rol)));
                    }

                    //Igual que con Bitácora: acepta el estado actual de IDs como la nueva base "válida" para
                    //DetectarRolEliminado, así un rol borrado ya visto/aceptado no se vuelve a reportar.
                    dal_dvv.ActualizarIdsVigentes("Rol", string.Join(",", rolesActuales.Select(r => r.IdRol)));
                    break;

                case "Bitacora":
                    List<BE_Bitacora> eventosBitacora = dal_bitacora.ObtenerBitacora();

                    foreach (var evento in eventosBitacora)
                    {
                        dal_bitacora.ActualizarDVH(evento.IdBitacora, EncryptionHelper.Encriptar(CadenaBitacora(evento)));
                    }

                    //Acepta el estado actual de IDs como la nueva base "válida": a partir de acá, DetectarBitacoraEliminada
                    //solo va a reportar borrados posteriores a este recálculo, no huecos ya conocidos/aceptados.
                    string idsVigentes = string.Join(",", eventosBitacora.Select(e => e.IdBitacora));
                    dal_dvv.ActualizarIdsVigentes("Bitacora", idsVigentes);
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

                //Ver el mismo comentario en DetectarCambiosUsuario: DVH null solo puede ser una fila
                //insertada por fuera del sistema, no una fila vieja legítima (RegistrarEvento siempre calcula DVH).
                if (evento.DVH == null)
                {
                    mensajes.Add($"El registro de bitácora ID {evento.IdBitacora} no tiene firma de integridad: parece haber sido insertado por fuera del sistema.");
                }
                else if (evento.DVH != dvhEsperado)
                {
                    mensajes.Add($"Se modificó el registro de bitácora ID {evento.IdBitacora}.");
                }
            }

            //Se corre siempre, no solo cuando ninguna fila quedó marcada como modificada: pueden pasar las dos cosas
            //a la vez (ej. filas "modificadas" en cascada por el ON DELETE SET NULL de un usuario borrado, más una
            //fila borrada de verdad), y antes la segunda quedaba enmascarada por la primera.
            DetectarBitacoraEliminada(eventos, mensajes);
        }

        //Chequeo de Bitácora que NO depende de que VerificarIntegridad("Bitacora") haya dado false. A diferencia
        //de Usuario/Rol, el DVV agregado de Bitácora se auto-repara con CUALQUIER evento nuevo que se registre
        //(incluidos los propios eventos de error de integridad que dispara esta misma verificación, o cualquier
        //login posterior de cualquier usuario - ver BLL_Bitacora.RegistrarEvento/RefrescarDVVBitacora), así que
        //deja de servir como aviso de manipulación de fila apenas se registra el próximo evento. El DVH de cada
        //fila individual, en cambio, nadie lo toca salvo "Recalcular DV", así que compararlo fila por fila sigue
        //siendo confiable siempre. Por eso Login.aspx.cs llama a este método en cada intento, sin usar
        //VerificarIntegridad como gate (a diferencia de Usuario/Rol, que si dependen de ese gate).
        public List<string> DetectarCambiosBitacoraSiempre()
        {
            var mensajes = new List<string>();
            DetectarCambiosBitacora(mensajes);

            //El evento INTEGRIDAD_ERROR para Usuario/Rol lo loguea VerificarIntegridad cuando el agregado da
            //mal. Para Bitácora ese agregado no sirve como señal (ver comentario arriba), así que el log de
            //auditoría tiene que colgar de ESTE chequeo (el confiable) en vez de VerificarIntegridad("Bitacora"):
            //si no, nunca quedaba registro en la propia bitácora de que hubo una manipulación de fila detectada,
            //aunque el reporte en pantalla del webmaster sí la mostrara.
            if (mensajes.Count > 0)
            {
                bll_bitacora.RegistrarEvento(
                    null,
                    AccionBitacora.INTEGRIDAD_ERROR,
                    "SEGURIDAD",
                    "Fallo de integridad del DVV en la tabla 'Bitacora' detectado al iniciar la aplicación."
                );
            }

            return mensajes;
        }

        //Detecta bitácoras borradas comparando los IDs actuales contra "IdsVigentes": la lista de IDs aceptada
        //como válida la última vez que se ejecutó "Recalcular DV" (o al restaurar un backup, que trae su propia
        //lista guardada). A diferencia de escanear huecos desde el ID 1, esto no vuelve a marcar para siempre
        //un hueco que el webmaster ya aceptó (ej. un borrado legítimo ya recalculado).
        private void DetectarBitacoraEliminada(List<BE_Bitacora> eventos, List<string> mensajes)
        {
            string idsVigentesCsv = dal_dvv.ObtenerIdsVigentes("Bitacora");

            //Todavía no hay una base aceptada (recién migrado, nunca se corrió "Recalcular DV"): no hay contra qué comparar.
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
                mensajes.Add($"Faltan {faltantes.Count} registro(s) en la bitácora (IDs: {string.Join(", ", faltantes)}).");
            }
        }

        //Usa siempre la carpeta de backups propia de SQL Server (no una carpeta elegida por el usuario):
        //esa es la única que la cuenta de servicio de SQL Server tiene garantizado poder escribir.
        //Arma el nombre del archivo con fecha/hora y devuelve la ruta completa para mostrársela al usuario.
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
                    $"Se generó un backup de la base de datos en: {rutaCompleta}"
                );

                return rutaCompleta;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar backup: {ex.Message}", ex);
            }

        }

        //Backups disponibles para restaurar, para completar el desplegable de Seguridad.aspx (nada de tipear una ruta a mano).
        public List<string> ListarBackupsDisponibles()
        {
            return dal_dvv.ListarBackups();
        }

        public void RestaurarBackup(BE_Usuario usuarioLogueado, string nombreArchivo)
        {
            try
            {
                //El nombre viene de un desplegable armado con ListarBackupsDisponibles, pero igual se valida
                //acá por si el POST se manipula: sin barras ni "..", no puede escapar de la carpeta de backups.
                if (string.IsNullOrWhiteSpace(nombreArchivo) || nombreArchivo.IndexOfAny(new[] { '\\', '/' }) >= 0 || nombreArchivo.Contains(".."))
                {
                    throw new Exception("Debe seleccionar un backup válido de la lista.");
                }

                string carpetaBackup = dal_dvv.ObtenerCarpetaBackupPorDefecto();
                string rutaCompleta = System.IO.Path.Combine(carpetaBackup, nombreArchivo);

                dal_dvv.RestaurarBackup(rutaCompleta);

                bll_bitacora.RegistrarEvento(
                   usuarioLogueado?.IdUsuario,
                    AccionBitacora.BACKUP_RESTAURADO,
                    "SEGURIDAD",
                    $"Se restauró la base de datos desde el backup: {rutaCompleta}"
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al restaurar backup: {ex.Message}", ex);
            }

        }
    }
}
