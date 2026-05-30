using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace BLL
{
    public class BLL_Usuario
    {
        DAL_Usuario dal_usuario = new DAL_Usuario();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        

        //Obtenemos el objeto usuario con el mail 
        public BE_LoginResultado ObtenerUsuarioPorEmail(string Email, string Password)
        {
            //Password = HashHelper.GenerarHash(Password);

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
                bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INTENTO, "LOGIN", $"Intento de sesion del usuario bloqueado: {Usuario.Nombre}, con IdUsuario = {Usuario.IdUsuario}");

                return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario bloqueado. Contactese con el Administrador" };
            }

            //Si la contraseña es incorrecta, incrementamos el contador de IntentosFallidos y bloqueamos el usuario si supera los 3 intentos. 
            if (Usuario.HashPassword != Password)
            {
                Usuario.IntentosFallidos++;

                if (Usuario.IntentosFallidos > 3)
                {
                    dal_usuario.BloquearUsuario(Usuario.IdUsuario);
                    //Mandamos a bitacora que se bloquea el usuario por superar intentos fallidos.

                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_BLOQUEADO, "LOGIN", $"Se bloquea al usuario: {Usuario.Nombre}, ID: {Usuario.IdUsuario}, por superar la cantidad de intentos permitidos");

                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario Bloqueado. Contacte Administrador" };
                }
                else
                {
                    //Mandamos a bitacora el intento de login por contraseña incorrecta

                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {Usuario.Nombre}, ID: {Usuario.IdUsuario} con contraseña incorrecta");

                    dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                    return new BE_LoginResultado { ExitoLogin = false, Mensaje = $"Contraseña Incorrecta. Intentos fallidos: {Usuario.IntentosFallidos}" };
                }

            }
            //Si el login es exitoso, reseteamos los intentos fallidos a 0
            else
            {
                dal_usuario.ActualizarIntentosFallidos(0, Usuario.IdUsuario);

                //Mandamos a bitacora el login exitoso

                bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {Usuario.Nombre}, ID: {Usuario.IdUsuario}");

                return new BE_LoginResultado { ExitoLogin = true, Usuario = Usuario, Mensaje = "Login exitoso" };
            }
        }

        public List<BE_Usuario> Usuarios()
        {
            return dal_usuario.Usuarios();
        }

        public void AgregarUsuario(BE_Usuario usuario)
        {
            usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);
            dal_usuario.AgregarUsuario(usuario);

            //Mandamos a bitacora la creacion del nuevo usuario

           /* bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_ALTA, "USUARIO", $"Se crea un nuevo usuario: {usuario.Nombre}, {usuario.Apellido}, creado por {Sesion.Instancia().UsuarioActual.Nombre}");*/
        }
        public void ModificarUsuario(BE_Usuario usuario)
        {
            dal_usuario.ModificarUsuario(usuario);

            //Mandamos a bitacora la modificacion del usuario

            //bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_MODIFICACION, "USUARIO", $"Se modifica al usuario: {usuario.Nombre}, ID: {usuario.IdUsuario}. Modificado por: {Sesion.Instancia().UsuarioActual.Nombre}");
        }

        public void EliminarUsuario(int id)
        {
            dal_usuario.EliminarUsuario(id);

            //Mandamos a bitacora la eliminacion del usuario
            //bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_BAJA, "USUARIO", $"El Usuario {Sesion.Instancia().UsuarioActual.Nombre}, da de baja al Usuario con ID: {id}");
        }

        public void ResetearPassword(int id, string nuevoPass)
        {
            nuevoPass = HashHelper.GenerarHash(nuevoPass);

            dal_usuario.ResetearContrasena(id, nuevoPass);

            //Mandamos a bitacora el reseteo de contraseña
            //bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_RESTABLECIMIENTO_PASSWORD, "USUARIO", $"El Usuario {Sesion.Instancia().UsuarioActual.Nombre}, resetea la contraseña del Usuario con ID: {id}");
        }
    }
}
