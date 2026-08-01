using BE;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_Rol
    {
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_Usuario dal_usuario = new DAL_Usuario();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public List<BE_Rol> ObtenerRoles()
        {
            return dal_rol.ObtenerRoles();
        }

        public BE_Rol ObtenerRolDeUsuario(int idUsuario)
        {
            return dal_rol.ObtenerRolPorUsuario(idUsuario);
        }

        public void AsignarRol(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idRol)
        {
            try
            {
                dal_rol.AsignarRol(idUsuarioDestino, idRol);

                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);
                BE_Rol rol = dal_rol.ObtenerRolPorUsuario(idUsuarioDestino);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_ASIGNADO_USUARIO, "ROL", $"Se asignó el rol {rol?.Nombre} al usuario {usuarioDestino?.NombreApellido}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al asignar rol: {ex.Message}", ex);
            }
        }

        public void ModificarRol(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idRol)
        {
            AsignarRol(usuarioLogueado, idUsuarioDestino, idRol);
        }

        public void EliminarRol(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idRol)
        {
            try
            {
                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);
                BE_Rol rol = dal_rol.ObtenerRolPorUsuario(idUsuarioDestino);

                dal_rol.EliminarRol(idUsuarioDestino, idRol);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_QUITADO_USUARIO, "ROL", $"Se quitó el rol {rol?.Nombre} al usuario {usuarioDestino?.NombreApellido}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar rol: {ex.Message}", ex);
            }
        }
    }
}
