using BE;
using BE.Seguridad;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class BLL_Permiso
    {
        DAL_Permiso dal_permiso = new DAL_Permiso();
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_Usuario dal_usuario = new DAL_Usuario();
        BLL_Rol bll_rol = new BLL_Rol();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        public List<BE_Permiso> ObtenerPermisos()
        {
            try
            {
                return dal_permiso.ObtenerPermisos();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener permisos: {ex.Message}", ex);
            }
        }

        public void AgregarPermiso(BE_Usuario usuarioLogueado, BE_Permiso permiso)
        {
            try
            {
                ValidarNombre(permiso.Nombre);

                if (dal_permiso.ObtenerPermisos().Any(p => string.Equals(p.Nombre, permiso.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Ya existe un permiso llamado '{permiso.Nombre}'.");
                }

                dal_permiso.AgregarPermiso(permiso);

                permiso.DVH = HashHelper.GenerarHash(CadenaPermiso(permiso));
                dal_permiso.ActualizarDVH(permiso.IdPermiso, permiso.DVH);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_ALTA, "PERMISO",
                    $"Se crea el permiso '{permiso.Nombre}' (ID {permiso.IdPermiso}), creado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el permiso: {ex.Message}", ex);
            }
        }

        public void ModificarPermiso(BE_Usuario usuarioLogueado, BE_Permiso permiso)
        {
            try
            {
                ValidarNombre(permiso.Nombre);

                if (dal_permiso.ObtenerPermisos().Any(p => p.IdPermiso != permiso.IdPermiso && string.Equals(p.Nombre, permiso.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Ya existe un permiso llamado '{permiso.Nombre}'.");
                }

                permiso.DVH = HashHelper.GenerarHash(CadenaPermiso(permiso));
                dal_permiso.ModificarPermiso(permiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_MODIFICACION, "PERMISO",
                    $"Se modifica el permiso ID {permiso.IdPermiso}, nuevo nombre: '{permiso.Nombre}'. Modificado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al modificar el permiso: {ex.Message}", ex);
            }
        }

        public void EliminarPermiso(BE_Usuario usuarioLogueado, int idPermiso)
        {
            try
            {
                BE_Permiso permiso = dal_permiso.ObtenerPermisos().FirstOrDefault(p => p.IdPermiso == idPermiso);

                dal_permiso.EliminarPermiso(idPermiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_BAJA, "PERMISO",
                    $"Se elimina el permiso '{permiso?.Nombre}' (ID {idPermiso}). Eliminado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el permiso: {ex.Message}", ex);
            }
        }

        private string CadenaPermiso(BE_Permiso permiso) => $"{permiso.IdPermiso}|{permiso.Nombre}|{permiso.Tipo}";

        // Permiso.Nombre es VARCHAR(30): igual que en BLL_Rol, sin este chequeo
        // un nombre mas largo se trunca en silencio al insertarlo.
        private void ValidarNombre(string nombre)
        {
            if (!string.IsNullOrEmpty(nombre) && nombre.Length > 30)
            {
                throw new InvalidOperationException($"El nombre del permiso no puede superar los 30 caracteres (tiene {nombre.Length}).");
            }
        }

        // ---- Arbol Composite completo de un usuario (raiz): sus Roles + sus permisos directos ----

        public UsuarioComponente ConstruirArbolUsuario(BE_Usuario usuario)
        {
            UsuarioComponente componente = new UsuarioComponente(usuario);

            foreach (BE_Rol rol in dal_rol.ObtenerRolesDeUsuario(usuario.IdUsuario))
            {
                componente.CargarAsignacion(bll_rol.ConstruirArbolRol(rol.IdRol));
            }

            List<BE_Permiso> todosLosPermisos = dal_permiso.ObtenerPermisos();

            foreach (int idPermiso in dal_permiso.ObtenerIdsPermisoDeUsuario(usuario.IdUsuario))
            {
                BE_Permiso permiso = todosLosPermisos.FirstOrDefault(p => p.IdPermiso == idPermiso);

                if (permiso != null)
                {
                    componente.CargarAsignacion(new PermisoComponente(permiso));
                }
            }

            return componente;
        }

        public void AsignarRolAUsuario(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idRol)
        {
            try
            {
                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);

                if (usuarioDestino == null)
                {
                    throw new InvalidOperationException("El usuario seleccionado no existe.");
                }

                UsuarioComponente arbolUsuario = ConstruirArbolUsuario(usuarioDestino);

                if (arbolUsuario.RolesAsignados.Any(r => r.Rol.IdRol == idRol))
                {
                    throw new InvalidOperationException($"El usuario {usuarioDestino.NombreApellido} ya tiene asignado ese rol.");
                }

                RolComponente rol = bll_rol.ConstruirArbolRol(idRol);

                // Valida el caso del enunciado: si el usuario ya tiene, suelto, un
                // permiso que este rol tambien otorga, se rechaza.
                arbolUsuario.AsignarRol(rol);

                dal_rol.AgregarRolAUsuario(idUsuarioDestino, idRol);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_ASIGNADO_USUARIO, "ROL",
                    $"Se asigna el rol '{rol.Codigo}' al usuario {usuarioDestino.NombreApellido}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al asignar el rol: {ex.Message}", ex);
            }
        }

        public void QuitarRolDeUsuario(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idRol)
        {
            try
            {
                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);
                BE_Rol rol = dal_rol.ObtenerRoles().FirstOrDefault(r => r.IdRol == idRol);

                dal_rol.QuitarRolDeUsuario(idUsuarioDestino, idRol);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_QUITADO_USUARIO, "ROL",
                    $"Se quita el rol '{rol?.Nombre}' al usuario {usuarioDestino?.NombreApellido}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar el rol: {ex.Message}", ex);
            }
        }

        public void AsignarPermisoAUsuario(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idPermiso)
        {
            try
            {
                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);

                if (usuarioDestino == null)
                {
                    throw new InvalidOperationException("El usuario seleccionado no existe.");
                }

                BE_Permiso permiso = dal_permiso.ObtenerPermisos().FirstOrDefault(p => p.IdPermiso == idPermiso);

                if (permiso == null)
                {
                    throw new InvalidOperationException("El permiso seleccionado no existe.");
                }

                UsuarioComponente arbolUsuario = ConstruirArbolUsuario(usuarioDestino);

                // Valida el caso del enunciado: si el permiso ya llega al usuario
                // (directo o via cualquier rol), se rechaza la asignacion duplicada.
                arbolUsuario.AsignarPermiso(new PermisoComponente(permiso));

                dal_permiso.AgregarPermisoAUsuario(idUsuarioDestino, idPermiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_ASIGNADO_USUARIO, "PERMISO",
                    $"Se asigna el permiso '{permiso.Nombre}' al usuario {usuarioDestino.NombreApellido}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al asignar el permiso: {ex.Message}", ex);
            }
        }

        public void QuitarPermisoDeUsuario(BE_Usuario usuarioLogueado, int idUsuarioDestino, int idPermiso)
        {
            try
            {
                BE_Usuario usuarioDestino = dal_usuario.Usuarios().FirstOrDefault(u => u.IdUsuario == idUsuarioDestino);
                BE_Permiso permiso = dal_permiso.ObtenerPermisos().FirstOrDefault(p => p.IdPermiso == idPermiso);

                dal_permiso.QuitarPermisoDeUsuario(idUsuarioDestino, idPermiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_QUITADO_USUARIO, "PERMISO",
                    $"Se quita el permiso '{permiso?.Nombre}' al usuario {usuarioDestino?.NombreApellido}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar el permiso: {ex.Message}", ex);
            }
        }
    }
}
