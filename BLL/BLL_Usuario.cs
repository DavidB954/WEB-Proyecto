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
        DAL_DVV dal_dvv = new DAL_DVV();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();


        //Obtenemos el objeto usuario con el mail
        public BE_LoginResultado ObtenerUsuarioPorEmail(string Email, string Password)
        {
            try
            {

                if (string.IsNullOrEmpty(Password))
                {
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Debe ingresar contraseña" };
                }

                Password = HashHelper.GenerarHash(Password);

                //Obtenemos el objeto Usuario con el mail
                BE_Usuario Usuario = dal_usuario.ObtenerUsuarioPorEmail(Email);

                //Validamos si el usuario existe
                if (Usuario == null)
                {
                    //No existe el usuario. Entonces mandamos a bitacora el intento de login con ese email.
                    bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de Login usando el email: {Email}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
                }

                //Si no esta activo
                if (!Usuario.Activo)
                {
                    //Existe el usuario pero no esta activo. Entonces mandamos a Bitacora el intento de login.
                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de sesion del usuario bloqueado: {Usuario.NombreApellido}, con IdUsuario = {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario bloqueado. Contactese con el Administrador" };
                }

                //Si la contraseña es incorrecta, incrementamos el contador de IntentosFallidos y bloqueamos el usuario si supera los 3 intentos.
                if (Usuario.HashPassword != Password)
                {
                    Usuario.IntentosFallidos++;

                    if (Usuario.IntentosFallidos > 3)
                    {
                        Usuario.Activo = false;

                        //BloquearUsuario solo toca Activo: hay que persistir también el contador de intentos fallidos
                        //(que en este punto ya está incrementado en memoria), si no, el DVH que graba RefrescarDVH
                        //queda calculado con un IntentosFallidos que nunca se guardó en la fila real.
                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        dal_usuario.BloquearUsuario(Usuario.IdUsuario);
                        RefrescarDVH(Usuario);

                        //Mandamos a bitacora que se bloquea el usuario por superar intentos fallidos.

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_BLOQUEADO, "LOGIN", $"Se bloquea al usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}, por superar la cantidad de intentos permitidos");

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario Bloqueado. Contacte Administrador" };
                    }
                    else
                    {
                        //Mandamos a bitacora el intento de login por contraseña incorrecta

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario} con contraseña incorrecta");

                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        RefrescarDVH(Usuario);

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = $"Contraseña Incorrecta. Intentos fallidos: {Usuario.IntentosFallidos}" };
                    }

                }
                //Si el login es exitoso, reseteamos los intentos fallidos a 0
                else
                {
                    Usuario.IntentosFallidos = 0;
                    dal_usuario.ActualizarIntentosFallidos(0, Usuario.IdUsuario);
                    RefrescarDVH(Usuario);

                    //Mandamos a bitacora el login exitoso

                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = true, Usuario = Usuario, Mensaje = "Login exitoso" };
                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Error al obtener usuario: {ex.Message}", ex);
            }

        }

        //Login exclusivo del Webmaster para cuando la base está comprometida.
        //Solo valida: que el usuario exista, que su contraseña sea correcta y que tenga el rol WEBMASTER.
        public BE_LoginResultado ValidarWebmaster(string Email, string Password)
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

            if (usuario.NombreRol != "WEBMASTER")
            {
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Solo el Webmaster puede ingresar en este momento." };
            }

            if (usuario.HashPassword != HashHelper.GenerarHash(Password))
            {
                return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario o Contraseña Incorrecto" };
            }

            return new BE_LoginResultado { ExitoLogin = true, Usuario = usuario, Mensaje = "Login de Webmaster correcto" };
        }

        //Credenciales de emergencia hardcodeadas: permiten ingresar al sistema si se eliminó al único WEBMASTER o al único ADMINISTRADOR.

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

            //Mandamos a bitacora el ingreso por emergencia. IdUsuario null porque este usuario no existe en la tabla Usuario.
            bll_bitacora.RegistrarEvento(null, AccionBitacora.LOGIN_OK, "LOGIN", $"Login de EMERGENCIA con rol {rol} usando el email: {Email}");

            return new BE_LoginResultado { ExitoLogin = true, Usuario = usuarioEmergencia, Mensaje = "Login de emergencia exitoso" };
        }

        public List<BE_Usuario> Usuarios()
        {
            return dal_usuario.Usuarios();
        }

        public List<BE_Usuario> UsuariosConRol()
        {
            return dal_usuario.UsuariosConRol();
        }

        public void AgregarUsuario(BE_Usuario UsuarioLogueado, BE_Usuario usuario)
        {
            try
            {
                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                //El DVH se calcula DESPUÉS de hashear la contraseña, con la misma fórmula que usa la verificación (BLL_DVV.CadenaUsuario).
                //Si se calculara antes (con la contraseña en texto plano), no coincidiría al verificar y marcaría la fila como modificada siempre.
                usuario.DVH = HashHelper.GenerarHash(CadenaUsuario(usuario));

                dal_usuario.AgregarUsuario(usuario);

                RefrescarDVVUsuario();

                //Mandamos a bitacora la creacion del nuevo usuario. Incluimos el ID para poder detectar más adelante si este usuario fue eliminado por fuera del sistema (ver BLL_DVV.DetectarCambios).

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
                    //No se restablece la contraseña: se preserva tal cual está en la base (ya hasheada). Si se
                    //rehasheara acá el valor entrante, quedaría un hash-del-hash y el usuario no podría loguearse
                    //nunca más con su contraseña real.
                    BE_Usuario usuarioActual = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == usuario.IdUsuario);
                    usuario.HashPassword = usuarioActual?.HashPassword;
                }

                //Igual que en el alta: el DVH se calcula con la contraseña ya hasheada, para que coincida con la verificación.
                usuario.DVH = HashHelper.GenerarHash(CadenaUsuario(usuario));

                dal_usuario.ModificarUsuario(usuario);

                RefrescarDVVUsuario();

                //Mandamos a bitacora la modificacion del usuario

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
                //Se guardan antes de borrar: son las filas de bitácora del propio usuario, que el FK va a dejar
                //en NULL en cascada (ON DELETE SET NULL). Hay que recalcular su DVH después, porque su contenido
                //cambia legítimamente y si no se refresca la próxima verificación las marca como "modificadas".
                List<int> idsBitacoraAfectados = bll_bitacora.ObtenerBitacora()
                    .Where(b => b.IdUsuario == id)
                    .Select(b => b.IdBitacora)
                    .ToList();

                dal_usuario.EliminarUsuario(id);

                bll_bitacora.RefrescarDVHDeFilas(idsBitacoraAfectados);

                RefrescarDVVUsuario();

                //Mandamos a bitacora la eliminacion del usuario
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

                //Mandamos a bitacora el reseteo de contraseña
                //bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_RESTABLECIMIENTO_PASSWORD, "USUARIO", $"El Usuario {Sesion.Instancia().UsuarioActual.Nombre}, resetea la contraseña del Usuario con ID: {id}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al resetear contraseña: {ex.Message}", ex);
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
