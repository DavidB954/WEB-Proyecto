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
                        dal_usuario.BloquearUsuario(Usuario.IdUsuario);
                        //Mandamos a bitacora que se bloquea el usuario por superar intentos fallidos.

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_BLOQUEADO, "LOGIN", $"Se bloquea al usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}, por superar la cantidad de intentos permitidos");

                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = "Usuario Bloqueado. Contacte Administrador" };
                    }
                    else
                    {
                        //Mandamos a bitacora el intento de login por contraseña incorrecta

                        bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_INCORRECTO, "LOGIN", $"Intento de inicio de sesion con el usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario} con contraseña incorrecta");

                        dal_usuario.ActualizarIntentosFallidos(Usuario.IntentosFallidos, Usuario.IdUsuario);
                        return new BE_LoginResultado { ExitoLogin = false, Mensaje = $"Contraseña Incorrecta. Intentos fallidos: {Usuario.IntentosFallidos}" };
                    }

                }
                //Si el login es exitoso, reseteamos los intentos fallidos a 0
                else
                {
                    dal_usuario.ActualizarIntentosFallidos(0, Usuario.IdUsuario);

                    //Mandamos a bitacora el login exitoso

                    bll_bitacora.RegistrarEvento(Usuario.IdUsuario, AccionBitacora.LOGIN_OK, "LOGIN", $"Login correcto del usuario: {Usuario.NombreApellido}, ID: {Usuario.IdUsuario}");

                    return new BE_LoginResultado { ExitoLogin = true, Usuario = Usuario, Mensaje = "Login exitoso" };
                }
            }
            catch (Exception ex)
            {

                throw new Exception( ex.Message);
            }
           
        }

        public List<BE_Usuario> Usuarios()
        {
            return dal_usuario.Usuarios();
        }

        public void AgregarUsuario(BE_Usuario UsuarioLogueado, BE_Usuario usuario)
        {
            try
            {
                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                usuario.DVH = HashHelper.GenerarHash(usuario.DVH);

                dal_usuario.AgregarUsuario(usuario);

                //Mandamos a bitacora la creacion del nuevo usuario


                bll_bitacora.RegistrarEvento(UsuarioLogueado.IdUsuario, AccionBitacora.USUARIO_ALTA, "USUARIO", $"Se crea un nuevo usuario: {usuario.Nombre}, {usuario.Apellido}, creado por {UsuarioLogueado.NombreApellido}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar usuario: " + ex.Message, ex);
            }
           
        }
        public void ModificarUsuario(BE_Usuario UsuarioLogueado, BE_Usuario usuario)
        {
            try
            {
                usuario.HashPassword = HashHelper.GenerarHash(usuario.HashPassword);

                usuario.DVH = HashHelper.GenerarHash(usuario.DVH);

                dal_usuario.ModificarUsuario(usuario);

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
                dal_usuario.EliminarUsuario(id);

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
            nuevoPass = HashHelper.GenerarHash(nuevoPass);

            dal_usuario.ResetearContrasena(id, nuevoPass);

            //Mandamos a bitacora el reseteo de contraseña
           //bll_bitacora.RegistrarEvento(Sesion.Instancia().UsuarioActual.IdUsuario, AccionBitacora.USUARIO_RESTABLECIMIENTO_PASSWORD, "USUARIO", $"El Usuario {Sesion.Instancia().UsuarioActual.Nombre}, resetea la contraseña del Usuario con ID: {id}");
        }
    }
}
