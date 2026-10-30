using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_Usuario
    {
        DAL_Usuario dal_usuario = new DAL_Usuario();
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_DVV dal_dvv = new DAL_DVV();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();


        public BE_LoginResultado ObtenerUsuarioPorEmail(string Email, string Password)
        {
            try
            {

                if (string.IsNullOrEmpty(Password))
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Debe ingresar contraseña") };
                }

                Password = HashHelper.GenerarHash(Password);

                BE_Usuario Usuario = dal_usuario.ObtenerUsuarioPorEmail(Email);

                if (Usuario == null)
                {
                    bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de Login usando el email: {Email}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                if (!Usuario.Activo)
                {
                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de sesion del usuario bloqueado: {Usuario.NombreApellido}, con IdUsuario = {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario bloqueado. Contactese con el Administrador") };
                }

                if (Usuario.HashPassword != Password)
                {
                    Usuario.IntentosFallidos++;

                    if (Usuario.IntentosFallidos > 3)
                    {
                        Usuario.Activo = false;

                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        dal_usuario.BloquearUsuario(Usuario.IdUsuario);
                        RefrescarDVH(Usuario);


                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_BLOQUEADO, "LOGIN", $"Se bloquea al usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}, por superar la cantidad de intentos permitidos");

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario Bloqueado. Contacte Administrador") };
                    }
                    else
                    {

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario} con contraseña incorrecta");

                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        RefrescarDVH(Usuario);

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Contraseña Incorrecta. Intentos fallidos: {0}", Usuario.IntentosFallidos) };
                    }

                }
                else
                {
                    Usuario.IntentosFallidos = 0;
                    dal_usuario.ActualizarIntentosFallidos(0, Usuario.IdUsuario);
                    RefrescarDVH(Usuario);


                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = true, Usuario = Usuario, Mensaje = TextoLocalizado.T("Login exitoso") };
                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }

        }

        public BE_LoginResultado ValidarWebmaster(string Email, string Password)
        {
            try
            {
                if (string.IsNullOrEmpty(Password))
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Debe ingresar contraseña") };
                }

                BE_Usuario usuario = dal_usuario.ObtenerUsuarioPorEmail(Email);

                if (usuario == null)
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                // No alcanza con usuario.NombreRol: ese campo trae "un" rol del
                // usuario (el primero que devuelva el join, sin orden definido),
                // y un usuario puede tener varios. Hay que preguntar si WEBMASTER
                // esta entre TODOS sus roles, no solo en el primero que aparezca.
                bool esWebmaster = dal_rol.ObtenerRolesDeUsuario(usuario.IdUsuario)
                    .Any(r => string.Equals(r.Nombre, "WEBMASTER", StringComparison.OrdinalIgnoreCase));

                if (!esWebmaster)
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Solo el Webmaster puede ingresar en este momento.") };
                }

                if (usuario.HashPassword != HashHelper.GenerarHash(Password))
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
                }

                return new BE_LoginResultado { ExitoLogin = true, Usuario = usuario, Mensaje = TextoLocalizado.T("Login de Webmaster correcto") };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar el login de webmaster: {ex.Message}", ex);
            }
        }


        private const string EMAIL_EMERGENCIA_WEBMASTER = "REMOVIDO";
        private const string EMAIL_EMERGENCIA_ADMIN = "REMOVIDO";
        private const string HASH_PASSWORD_EMERGENCIA = "";

        public BE_LoginResultado LoginEmergencia(string Email, string Password)
        {
            string rol;

            if (Email == EMAIL_EMERGENCIA_WEBMASTER)
            {
                rol = "WEBMASTER";
            }
            else if (Email == EMAIL_EMERGENCIA_ADMIN)
            {
                rol = "ADMINISTRADOR";
            }
            else
            {
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
            }

            if (string.IsNullOrEmpty(Password) || HashHelper.GenerarHash(Password) != HASH_PASSWORD_EMERGENCIA)
            {
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = TextoLocalizado.T("Usuario o Contraseña Incorrecto") };
            }

            BE_Usuario usuarioEmergencia = new BE_Usuario
            {
                IdUsuario = 0,
                Nombre = rol == "WEBMASTER" ? "Webmaster" : "Administrador",
                Apellido = "de Emergencia",
                Email = Email,
                NombreRol = rol,
                Activo = true
            };

            bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_OK, "LOGIN", $"Login de EMERGENCIA con rol {rol} usando el email: {Email}");

            return new BE_LoginResultado { ExitoLogin = true, Usuario = usuarioEmergencia, Mensaje = TextoLocalizado.T("Login de emergencia exitoso") };
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

        public List<BE_Usuario> UsuariosConRol()
        {
            try
            {
                return dal_usuario.UsuariosConRol();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener usuarios con rol: {ex.Message}", ex);
            }
        }

        public void AgregarUsuario(BE_Usuario UsuarioLogueado, BE_Usuario usuario)
        {
            try
            {
                ValidarDniYEmailUnicos(usuario);

                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                usuario.DVH = HashHelper.GenerarHash(CadenaUsuario(usuario));

                dal_usuario.AgregarUsuario(usuario);

                RefrescarDVVUsuario();


                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_ALTA, "USUARIO", $"Se crea un nuevo usuario: {usuario.Nombre} {usuario.Apellido} (ID: {usuario.IdUsuario}), creado por {UsuarioLogueado.NombreApellido}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (SqlException ex) when (EsClaveDuplicada(ex))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con ese DNI o email."), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar usuario: " + ex.Message, ex);
            }

        }
        public void ModificarUsuario(BE_Usuario UsuarioLogueado, BE_Usuario usuario, bool actualizarPassword)
        {
            try
            {
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

                usuario.DVH = HashHelper.GenerarHash(CadenaUsuario(usuario));

                dal_usuario.ModificarUsuario(usuario);

                RefrescarDVVUsuario();


                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_MODIFICACION, "USUARIO", $"Se modifica al usuario: {usuario.Nombre}, ID: {usuario.IdUsuario}. Modificado por: {UsuarioLogueado.NombreApellido}");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (SqlException ex) when (EsClaveDuplicada(ex))
            {
                throw new InvalidOperationException(TextoLocalizado.T("Ya existe un usuario con ese DNI o email."), ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar usuario: " + ex.Message, ex);
            }

        }

        public void EliminarUsuario(BE_Usuario UsuarioLogueado, int id)
        {
            try
            {
                if (UsuarioLogueado != null && UsuarioLogueado.IdUsuario == id)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("No podes eliminar tu propio usuario."));
                }

                BE_Usuario eliminado = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == id);

                if (eliminado == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario seleccionado ya no existe."));
                }

                // Los turnos reservados del medico pasan a Cancelado: su DVH se arma
                // aca porque la DAL no conoce el formato de la cadena.
                if (!dal_usuario.EliminarUsuario(id, t => HashHelper.GenerarHash(BLL_Turno.CadenaTurno(t)),
                        out int turnosCancelados, out int turnosPacienteEliminados))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("No se pudo eliminar el usuario porque todavia tiene registros asociados. No se borro nada."));
                }

                RefrescarDVVUsuario();

                // Las filas de bitacora del usuario se conservan con su IdUsuario: el
                // evento de baja guarda nombre, DNI y email para poder identificarlo.
                string detalle = $"El Usuario {UsuarioLogueado.NombreApellido}, da de baja al Usuario con ID: {id} ({eliminado.Apellido}, {eliminado.Nombre} - DNI {eliminado.DNI} - {eliminado.Email}).";

                if (turnosPacienteEliminados > 0)
                {
                    detalle += $" Se eliminaron {turnosPacienteEliminados} turno(s) e historia clinica del paciente.";
                }

                if (turnosCancelados > 0)
                {
                    detalle += $" Se cancelaron {turnosCancelados} turno(s) reservado(s) con el medico.";
                }

                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_BAJA, "USUARIO", detalle);
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

        public void ResetearPassword(int id, string nuevoPass)
        {
            try
            {
                nuevoPass = HashHelper.GenerarHash(nuevoPass);

                dal_usuario.ResetearContrasena(id, nuevoPass);

                BE_Usuario usuario = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == id);
                if (usuario != null)
                {
                    RefrescarDVH(usuario);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Error al resetear contraseña: {ex.Message}", ex);
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

        // DNI y Email tienen UNIQUE en la tabla Usuario: se valida antes de ir a la
        // base para poder decirle al usuario cual de los dos esta repetido. Al
        // modificar, se excluye al propio usuario (puede conservar sus datos).
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

        // Respaldo de la validacion de arriba: si otro usuario se carga con el mismo
        // dato entre la validacion y el insert, la base lo rechaza (2627 = UNIQUE
        // constraint, 2601 = indice unico).
        private static bool EsClaveDuplicada(SqlException ex)
        {
            return ex.Number == 2627 || ex.Number == 2601;
        }

        private void RefrescarDVH(BE_Usuario usuario)
        {
            dal_usuario.ActualizarDVH(usuario.IdUsuario, HashHelper.GenerarHash(CadenaUsuario(usuario)));
            RefrescarDVVUsuario();
        }

        private void RefrescarDVVUsuario()
        {
            List<string> hashesFila = dal_usuario.Usuarios().Select(u => HashHelper.GenerarHash(CadenaUsuario(u))).ToList();

            StringBuilder concatenacion = new StringBuilder();
            foreach (var hash in hashesFila)
            {
                concatenacion.Append(hash);
            }

            dal_dvv.ActualizarDVV(HashHelper.GenerarHash(concatenacion.ToString()), "Usuario");
        }

        private string CadenaUsuario(BE_Usuario usuario)
        {
            string activo = usuario.Activo ? "1" : "0";
            return $"{usuario.Nombre}|{usuario.Apellido}|{usuario.Email}|{usuario.HashPassword}|{usuario.DNI}|{usuario.IntentosFallidos}|{activo}";
        }
    }
}
