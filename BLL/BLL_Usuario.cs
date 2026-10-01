using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
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
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Debe ingresar contraseña" };
                }

                Password = HashHelper.GenerarHash(Password);

                BE_Usuario Usuario = dal_usuario.ObtenerUsuarioPorEmail(Email);

                if (Usuario == null)
                {
                    bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de Login usando el email: {Email}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
                }

                if (!Usuario.Activo)
                {
                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de sesion del usuario bloqueado: {Usuario.NombreApellido}, con IdUsuario = {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario bloqueado. Contactese con el Administrador" };
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

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario Bloqueado. Contacte Administrador" };
                    }
                    else
                    {

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario} con contraseña incorrecta");

                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        RefrescarDVH(Usuario);

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = $"Contraseña Incorrecta. Intentos fallidos: {Usuario.IntentosFallidos}" };
                    }

                }
                else
                {
                    Usuario.IntentosFallidos = 0;
                    dal_usuario.ActualizarIntentosFallidos(0, Usuario.IdUsuario);
                    RefrescarDVH(Usuario);


                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = true, Usuario = Usuario, Mensaje = "Login exitoso" };
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
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Debe ingresar contraseña" };
                }

                BE_Usuario usuario = dal_usuario.ObtenerUsuarioPorEmail(Email);

                if (usuario == null)
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
                }

                // No alcanza con usuario.NombreRol: ese campo trae "un" rol del
                // usuario (el primero que devuelva el join, sin orden definido),
                // y un usuario puede tener varios. Hay que preguntar si WEBMASTER
                // esta entre TODOS sus roles, no solo en el primero que aparezca.
                bool esWebmaster = dal_rol.ObtenerRolesDeUsuario(usuario.IdUsuario)
                    .Any(r => string.Equals(r.Nombre, "WEBMASTER", StringComparison.OrdinalIgnoreCase));

                if (!esWebmaster)
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Solo el Webmaster puede ingresar en este momento." };
                }

                if (usuario.HashPassword != HashHelper.GenerarHash(Password))
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
                }

                return new BE_LoginResultado { ExitoLogin = true, Usuario = usuario, Mensaje = "Login de Webmaster correcto" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar el login de webmaster: {ex.Message}", ex);
            }
        }


        private const string EMAIL_EMERGENCIA_WEBMASTER = "webmaster@system.com";
        private const string EMAIL_EMERGENCIA_ADMIN = "admin@system.com";
        private const string HASH_PASSWORD_EMERGENCIA = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3";

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
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
            }

            if (string.IsNullOrEmpty(Password) || HashHelper.GenerarHash(Password) != HASH_PASSWORD_EMERGENCIA)
            {
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
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

            return new BE_LoginResultado { ExitoLogin = true, Usuario = usuarioEmergencia, Mensaje = "Login de emergencia exitoso" };
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
                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                usuario.DVH = HashHelper.GenerarHash(CadenaUsuario(usuario));

                dal_usuario.AgregarUsuario(usuario);

                RefrescarDVVUsuario();


                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_ALTA, "USUARIO", $"Se crea un nuevo usuario: {usuario.Nombre} {usuario.Apellido} (ID: {usuario.IdUsuario}), creado por {UsuarioLogueado.NombreApellido}");
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
            catch (Exception ex)
            {
                throw new Exception("Error al modificar usuario: " + ex.Message, ex);
            }

        }

        public void EliminarUsuario(BE_Usuario UsuarioLogueado, int id)
        {
            try
            {
                List<int> idsBitacoraAfectados = bll_bitacora.ObtenerBitacora()
                    .Where(b => b.IdUsuario == id)
                    .Select(b => b.IdBitacora)
                    .ToList();

                dal_usuario.EliminarUsuario(id);

                bll_bitacora.RefrescarDVHDeFilas(idsBitacoraAfectados);

                RefrescarDVVUsuario();

                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_BAJA, "USUARIO", $"El Usuario {UsuarioLogueado.NombreApellido}, da de baja al Usuario con ID: {id}");
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
