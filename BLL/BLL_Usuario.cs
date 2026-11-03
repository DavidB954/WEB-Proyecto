using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BLL
{
    public class BLL_Usuario
    {
        DAL_Usuario dal_usuario = new DAL_Usuario();
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_DVV dal_dvv = new DAL_DVV();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        BLL_Permiso bll_permiso = new BLL_Permiso();

        public BE_LoginResultado IniciarSesion(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Debe ingresar contraseña") };
                }

                password = HashHelper.GenerarHash(password);

                BE_Usuario usuario = dal_usuario.ObtenerUsuarioPorEmail(email);

                if (usuario == null)
                {
                    bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de Login usando el email: {email}");

                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                if (!usuario.Activo)
                {
                    bll_bitacora.RegistrarEvento(usuario.IdUsuario, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de sesion del usuario bloqueado: {usuario.NombreApellido}, con IdUsuario = {usuario.IdUsuario}");

                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Usuario bloqueado. Contactese con el Administrador") };
                }

                if (usuario.HashPassword != password)
                {
                    usuario.IntentosFallidos++;

                    if (usuario.IntentosFallidos > 3)
                    {
                        usuario.Activo = false;

                        dal_usuario.ActualizarIntentosFallidos(usuario.IntentosFallidos, usuario.IdUsuario);
                        dal_usuario.BloquearUsuario(usuario.IdUsuario);
                        RefrescarDVH(usuario);

                        bll_bitacora.RegistrarEvento(usuario.IdUsuario, AccionBitacora.LOGIN_BLOQUEADO, "LOGIN", $"Se bloquea al usuario: {usuario.NombreApellido}, ID: {usuario.IdUsuario}, por superar la cantidad de intentos permitidos");

                        return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Usuario Bloqueado. Contacte Administrador") };
                    }
                    else
                    {
                        bll_bitacora.RegistrarEvento(usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {usuario.NombreApellido}, ID: {usuario.IdUsuario} con contraseña incorrecta");

                        dal_usuario.ActualizarIntentosFallidos(usuario.IntentosFallidos, usuario.IdUsuario);
                        RefrescarDVH(usuario);

                        return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Contraseña Incorrecta. Intentos fallidos: {0}", usuario.IntentosFallidos) };
                    }
                }
                else
                {
                    usuario.IntentosFallidos = 0;
                    dal_usuario.ActualizarIntentosFallidos(0, usuario.IdUsuario);
                    RefrescarDVH(usuario);

                    bll_bitacora.RegistrarEvento(usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {usuario.NombreApellido}, ID: {usuario.IdUsuario}");

                    return new BE_LoginResultado { Usuario = usuario, Mensaje = TextoLocalizado.T("Login exitoso") };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }
        }

        public BE_LoginResultado ValidarWebmaster(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Debe ingresar contraseña") };
                }

                BE_Usuario usuario = dal_usuario.ObtenerUsuarioPorEmail(email);

                if (usuario == null)
                {
                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                bool esWebmaster = dal_rol.ObtenerRolesDeUsuario(usuario.IdUsuario)
                    .Any(r => string.Equals(r.Nombre, RolesSistema.Webmaster, StringComparison.OrdinalIgnoreCase));

                if (!esWebmaster)
                {
                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Solo el Webmaster puede ingresar en este momento.") };
                }

                if (usuario.HashPassword != HashHelper.GenerarHash(password))
                {
                    return new BE_LoginResultado { Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                return new BE_LoginResultado { Usuario = usuario, Mensaje = TextoLocalizado.T("Login de Webmaster correcto") };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar el login de webmaster: {ex.Message}", ex);
            }
        }

        public List<BE_Usuario> Usuarios()
        {
            try
            {
                return dal_usuario.Usuarios();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuarios: {ex.Message}", ex);
            }
        }

        public List<BE_Usuario> UsuariosConRol(BE_Usuario usuarioLogueado)
        {
            try
            {
                List<BE_Usuario> usuarios = dal_usuario.UsuariosConRol();

                if (bll_permiso.EsWebmaster(usuarioLogueado))
                {
                    return usuarios;
                }

                HashSet<int> idsWebmaster = bll_permiso.ObtenerIdsUsuariosWebmaster();

                return usuarios.Where(u => !idsWebmaster.Contains(u.IdUsuario)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuarios con rol: {ex.Message}", ex);
            }
        }

        public void AgregarUsuario(BE_Usuario usuarioLogueado, BE_Usuario usuario)
        {
            try
            {
                ValidarDniYEmailUnicos(usuario);

                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                usuario.DVH = HashHelper.GenerarHash(CadenasIntegridad.CadenaUsuario(usuario));

                dal_usuario.AgregarUsuario(usuario);

                RefrescarDVVUsuario();

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.USUARIO_ALTA, "USUARIO", $"Se crea un nuevo usuario: {usuario.Nombre} {usuario.Apellido} (ID: {usuario.IdUsuario}), creado por {usuarioLogueado.NombreApellido}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (SqlException ex) when (ErroresSql.EsClaveDuplicada(ex))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con ese DNI o email."), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar usuario: " + ex.Message, ex);
            }
        }

        public void ModificarUsuario(BE_Usuario usuarioLogueado, BE_Usuario usuario, bool actualizarPassword)
        {
            try
            {
                ValidarPuedeGestionarUsuario(usuarioLogueado, usuario.IdUsuario);

                ValidarDniYEmailUnicos(usuario);

                if (actualizarPassword)
                {
                    usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);
                }
                else
                {
                    BE_Usuario usuarioActual = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == usuario.IdUsuario);
                    usuario.HashPassword = usuarioActual?.HashPassword;
                }

                usuario.DVH = HashHelper.GenerarHash(CadenasIntegridad.CadenaUsuario(usuario));

                dal_usuario.ModificarUsuario(usuario);

                RefrescarDVVUsuario();

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.USUARIO_MODIFICACION, "USUARIO", $"Se modifica al usuario: {usuario.Nombre}, ID: {usuario.IdUsuario}. Modificado por: {usuarioLogueado.NombreApellido}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (SqlException ex) when (ErroresSql.EsClaveDuplicada(ex))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con ese DNI o email."), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar usuario: " + ex.Message, ex);
            }
        }

        public void EliminarUsuario(BE_Usuario usuarioLogueado, int id)
        {
            try
            {
                if (usuarioLogueado != null && usuarioLogueado.IdUsuario == id)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("No podes eliminar tu propio usuario."));
                }

                ValidarPuedeGestionarUsuario(usuarioLogueado, id);

                BE_Usuario eliminado = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == id);

                if (eliminado == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario seleccionado ya no existe."));
                }

                if (!dal_usuario.EliminarUsuario(id, t => HashHelper.GenerarHash(BLL_Turno.CadenaTurno(t)),
                        out int turnosCancelados, out int turnosPacienteEliminados))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("No se pudo eliminar el usuario porque todavia tiene registros asociados. No se borro nada."));
                }

                RefrescarDVVUsuario();

                string detalle = $"El Usuario {usuarioLogueado.NombreApellido}, da de baja al Usuario con ID: {id} ({eliminado.Apellido}, {eliminado.Nombre} - DNI {eliminado.DNI} - {eliminado.Email}).";

                if (turnosPacienteEliminados > 0)
                {
                    detalle += $" Se eliminaron {turnosPacienteEliminados} turno(s) e historia clinica del paciente.";
                }

                if (turnosCancelados > 0)
                {
                    detalle += $" Se cancelaron {turnosCancelados} turno(s) reservado(s) con el medico.";
                }

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.USUARIO_BAJA, "USUARIO", detalle);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar usuario: " + ex.Message, ex);
            }
        }

        public void ActualizarIdiomaPreferido(int idUsuario, int idIdioma)
        {
            try
            {
                dal_usuario.ActualizarIdiomaPreferido(idUsuario, idIdioma);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar el idioma preferido: {ex.Message}", ex);
            }
        }

        private void ValidarPuedeGestionarUsuario(BE_Usuario usuarioLogueado, int idUsuarioDestino)
        {
            if (!bll_permiso.PuedeGestionarUsuario(usuarioLogueado, idUsuarioDestino))
            {
                throw new InvalidOperationException(TextoLocalizado.T("No tiene permiso para realizar esta accion."));
            }
        }

        private void ValidarDniYEmailUnicos(BE_Usuario usuario)
        {
            usuario.DNI = usuario.DNI?.Trim();
            usuario.Email = usuario.Email?.Trim();

            List<BE_Usuario> otros = dal_usuario.Usuarios().Where(u => u.IdUsuario != usuario.IdUsuario).ToList();

            if (otros.Any(u => string.Equals(u.DNI?.Trim(), usuario.DNI, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con el DNI {0}.", usuario.DNI));
            }

            if (otros.Any(u => string.Equals(u.Email?.Trim(), usuario.Email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con el email {0}.", usuario.Email));
            }
        }

        private void RefrescarDVH(BE_Usuario usuario)
        {
            dal_usuario.ActualizarDVH(usuario.IdUsuario, HashHelper.GenerarHash(CadenasIntegridad.CadenaUsuario(usuario)));
            RefrescarDVVUsuario();
        }

        private void RefrescarDVVUsuario()
        {
            IEnumerable<string> firmas = dal_usuario.Usuarios().Select(u => HashHelper.GenerarHash(CadenasIntegridad.CadenaUsuario(u)));

            dal_dvv.ActualizarDVV(CadenasIntegridad.CalcularDVV(firmas), "Usuario");
        }
    }
}
