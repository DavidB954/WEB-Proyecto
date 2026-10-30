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

        // Da de alta en la tabla Permiso todo lo que esta en CatalogoPermisos y
        // todavia no existe (y corrige el Tipo si no coincide). Los permisos no se
        // crean a mano: Rol_Permiso y UsuarioPermiso necesitan su IdPermiso.
        public void SincronizarCatalogo(BE_Usuario usuarioLogueado)
        {
            try
            {
                List<BE_Permiso> existentes = dal_permiso.ObtenerPermisos();

                foreach (PermisoDefinicion definicion in CatalogoPermisos.Todos)
                {
                    BE_Permiso permiso = existentes.FirstOrDefault(p => string.Equals(p.Nombre, definicion.Codigo, StringComparison.OrdinalIgnoreCase));

                    if (permiso == null)
                    {
                        permiso = new BE_Permiso { Nombre = definicion.Codigo, Tipo = definicion.Tipo };

                        dal_permiso.AgregarPermiso(permiso);

                        permiso.DVH = HashHelper.GenerarHash(CadenaPermiso(permiso));
                        dal_permiso.ActualizarDVH(permiso.IdPermiso, permiso.DVH);

                        bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_ALTA, "PERMISO",
                            $"Se da de alta el permiso '{permiso.Nombre}' (ID {permiso.IdPermiso}) desde el catalogo del sistema.");
                    }
                    else if (!string.Equals(permiso.Tipo, definicion.Tipo, StringComparison.OrdinalIgnoreCase))
                    {
                        permiso.Tipo = definicion.Tipo;
                        permiso.DVH = HashHelper.GenerarHash(CadenaPermiso(permiso));
                        dal_permiso.ModificarPermiso(permiso);

                        bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.PERMISO_MODIFICACION, "PERMISO",
                            $"Se corrige el tipo del permiso '{permiso.Nombre}' a {permiso.Tipo} segun el catalogo del sistema.");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al sincronizar el catalogo de permisos: {ex.Message}", ex);
            }
        }

        private string CadenaPermiso(BE_Permiso permiso) => $"{permiso.IdPermiso}|{permiso.Nombre}|{permiso.Tipo}";

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
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario seleccionado no existe."));
                }

                UsuarioComponente arbolUsuario = ConstruirArbolUsuario(usuarioDestino);

                if (arbolUsuario.RolesAsignados.Any(r => r.Rol.IdRol == idRol))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario {0} ya tiene asignado ese rol.", usuarioDestino.NombreApellido));
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
                    throw new InvalidOperationException(TextoLocalizado.T("El usuario seleccionado no existe."));
                }

                BE_Permiso permiso = dal_permiso.ObtenerPermisos().FirstOrDefault(p => p.IdPermiso == idPermiso);

                if (permiso == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El permiso seleccionado no existe."));
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
