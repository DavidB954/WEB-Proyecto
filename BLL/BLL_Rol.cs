using BE;
using BE.Seguridad;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_Rol
    {
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_Permiso dal_permiso = new DAL_Permiso();
        DAL_DVV dal_dvv = new DAL_DVV();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

        private static readonly HashSet<string> RolesDelSistema = new HashSet<string>(
            new[] { RolesSistema.Medico, RolesSistema.Paciente, RolesSistema.Webmaster, RolesSistema.Administrador }, StringComparer.OrdinalIgnoreCase);

        private const int NombresEnMensaje = 5;

        public List<BE_Rol> ObtenerRoles()
        {
            try
            {
                return dal_rol.ObtenerRoles();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener roles: {ex.Message}", ex);
            }
        }

        public void AgregarRol(BE_Usuario usuarioLogueado, BE_Rol rol)
        {
            try
            {
                ValidarNombre(rol.Nombre);

                if (dal_rol.ObtenerRoles().Any(r => string.Equals(r.Nombre, rol.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ya existe un rol llamado '{0}'.", rol.Nombre));
                }

                dal_rol.AgregarRol(rol);

                rol.DVH = HashHelper.GenerarHash(CadenasIntegridad.CadenaRol(rol));
                dal_rol.ActualizarDVH(rol.IdRol, rol.DVH);

                RefrescarDVVRol();

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_ALTA, "ROL", $"Se crea el rol '{rol.Nombre}' (ID {rol.IdRol}), creado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el rol: {ex.Message}", ex);
            }
        }

        public void ModificarRol(BE_Usuario usuarioLogueado, BE_Rol rol)
        {
            try
            {
                ValidarNombre(rol.Nombre);

                if (dal_rol.ObtenerRoles().Any(r => r.IdRol != rol.IdRol && string.Equals(r.Nombre, rol.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Ya existe un rol llamado '{0}'.", rol.Nombre));
                }

                rol.DVH = HashHelper.GenerarHash(CadenasIntegridad.CadenaRol(rol));
                dal_rol.ModificarRol(rol);

                RefrescarDVVRol();

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_MODIFICACION, "ROL", $"Se modifica el rol ID {rol.IdRol}, nuevo nombre: '{rol.Nombre}'. Modificado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al modificar el rol: {ex.Message}", ex);
            }
        }

        public void EliminarRol(BE_Usuario usuarioLogueado, int idRol)
        {
            try
            {
                BE_Rol rol = dal_rol.ObtenerRoles().FirstOrDefault(r => r.IdRol == idRol);

                ValidarSePuedeEliminar(rol, idRol);

                dal_rol.EliminarRolPorId(idRol);

                RefrescarDVVRol();

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_BAJA, "ROL", $"Se elimina el rol '{rol.Nombre}' (ID {idRol}). Eliminado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el rol: {ex.Message}", ex);
            }
        }

        private void ValidarSePuedeEliminar(BE_Rol rol, int idRol)
        {
            if (rol == null)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El rol seleccionado ya no existe."));
            }

            if (RolesDelSistema.Contains(rol.Nombre))
            {
                throw new InvalidOperationException(TextoLocalizado.T("El rol '{0}' es un rol del sistema y no se puede eliminar.", rol.Nombre));
            }

            List<string> usuarios = dal_rol.ObtenerNombresUsuariosConRol(idRol);

            if (usuarios.Count > 0)
            {
                throw new InvalidOperationException(TextoLocalizado.T(
                    "No se puede eliminar el rol '{0}': lo tienen asignado {1} usuario(s) ({2}). Primero quitaselo o cambialo por otro desde Asignacion de Seguridad.",
                    rol.Nombre, usuarios.Count, ResumirNombres(usuarios)));
            }

            List<string> rolesPadre = dal_rol.ObtenerNombresRolesPadre(idRol);

            if (rolesPadre.Count > 0)
            {
                throw new InvalidOperationException(TextoLocalizado.T(
                    "No se puede eliminar el rol '{0}': es sub-rol de {1}. Primero quitalo de la composicion de esos roles.",
                    rol.Nombre, ResumirNombres(rolesPadre)));
            }
        }

        private string ResumirNombres(List<string> nombres)
        {
            string resumen = string.Join(", ", nombres.Take(NombresEnMensaje));

            return nombres.Count > NombresEnMensaje ? resumen + ", ..." : resumen;
        }

        private void RefrescarDVVRol()
        {
            List<BE_Rol> roles = dal_rol.ObtenerRoles();

            dal_dvv.ActualizarDVV(CadenasIntegridad.CalcularDVV(roles.Select(r => HashHelper.GenerarHash(CadenasIntegridad.CadenaRol(r)))), "Rol");
            dal_dvv.ActualizarIdsVigentes("Rol", string.Join(",", roles.Select(r => r.IdRol)));
        }

        private void ValidarNombre(string nombre)
        {
            if (!string.IsNullOrEmpty(nombre) && nombre.Length > 30)
            {
                throw new InvalidOperationException(TextoLocalizado.T("El nombre del rol no puede superar los 30 caracteres (tiene {0}).", nombre.Length));
            }
        }

        public RolComponente ConstruirArbolRol(int idRol)
        {
            List<BE_Rol> todosLosRoles = dal_rol.ObtenerRoles();
            List<BE_Permiso> todosLosPermisos = dal_permiso.ObtenerPermisos();

            return ConstruirArbolRol(idRol, todosLosRoles, todosLosPermisos, new HashSet<int>());
        }

        private RolComponente ConstruirArbolRol(int idRol, List<BE_Rol> todosLosRoles, List<BE_Permiso> todosLosPermisos, HashSet<int> enConstruccion)
        {
            BE_Rol rol = todosLosRoles.FirstOrDefault(r => r.IdRol == idRol);

            if (rol == null)
            {
                throw new Exception($"No existe el rol con ID {idRol}.");
            }

            RolComponente componente = new RolComponente(rol);

            if (!enConstruccion.Add(idRol))
            {
                return componente;
            }

            foreach (int idPermiso in dal_permiso.ObtenerIdsPermisoDeRol(idRol))
            {
                BE_Permiso permiso = todosLosPermisos.FirstOrDefault(p => p.IdPermiso == idPermiso);

                if (permiso != null)
                {
                    componente.CargarHijo(new PermisoComponente(permiso));
                }
            }

            foreach (int idSubRol in dal_rol.ObtenerIdsSubRol(idRol))
            {
                componente.CargarHijo(ConstruirArbolRol(idSubRol, todosLosRoles, todosLosPermisos, enConstruccion));
            }

            enConstruccion.Remove(idRol);

            return componente;
        }

        public void AgregarSubRol(BE_Usuario usuarioLogueado, int idRolPadre, int idRolHijo)
        {
            try
            {
                if (idRolPadre == idRolHijo)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Un rol no puede contenerse a si mismo."));
                }

                RolComponente arbolPadre = ConstruirArbolRol(idRolPadre);

                if (arbolPadre.SubRoles.Any(r => r.Rol.IdRol == idRolHijo))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El rol '{0}' ya contiene al rol seleccionado.", arbolPadre.Codigo));
                }

                RolComponente arbolHijo = ConstruirArbolRol(idRolHijo);

                arbolPadre.AgregarHijo(arbolHijo);

                dal_rol.AgregarSubRol(idRolPadre, idRolHijo);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_SUBROL_AGREGADO, "ROL",
                    $"Se agrega el rol '{arbolHijo.Codigo}' como sub-rol de '{arbolPadre.Codigo}'. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al componer el rol: {ex.Message}", ex);
            }
        }

        public void QuitarSubRol(BE_Usuario usuarioLogueado, int idRolPadre, int idRolHijo)
        {
            try
            {
                dal_rol.QuitarSubRol(idRolPadre, idRolHijo);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_SUBROL_QUITADO, "ROL",
                    $"Se quita el sub-rol ID {idRolHijo} del rol ID {idRolPadre}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar el sub-rol: {ex.Message}", ex);
            }
        }

        public void AgregarPermisoARol(BE_Usuario usuarioLogueado, int idRol, int idPermiso)
        {
            try
            {
                RolComponente arbolRol = ConstruirArbolRol(idRol);

                BE_Permiso permiso = dal_permiso.ObtenerPermisos().FirstOrDefault(p => p.IdPermiso == idPermiso);

                if (permiso == null)
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El permiso seleccionado no existe."));
                }

                if (arbolRol.TienePermiso(permiso.Nombre))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("El rol '{0}' ya tiene el permiso '{1}' (directo o a traves de un sub-rol).", arbolRol.Codigo, permiso.Nombre));
                }

                PermisoDefinicion definicion = CatalogoPermisos.Buscar(permiso.Nombre);

                if (definicion != null && definicion.Tipo == CatalogoPermisos.TipoAccion)
                {
                    PermisoDefinicion permisoPagina = CatalogoPermisos.DePagina(definicion.Pagina)
                        .FirstOrDefault(p => p.Tipo == CatalogoPermisos.TipoPagina && arbolRol.TienePermiso(p.Codigo));

                    if (permisoPagina != null)
                    {
                        throw new InvalidOperationException(TextoLocalizado.T("El rol '{0}' ya tiene la pagina completa '{1}' ({2}), que incluye la accion '{3}'.", arbolRol.Codigo, definicion.Pagina, permisoPagina.Codigo, permiso.Nombre));
                    }
                }

                dal_permiso.AgregarPermisoARol(idRol, idPermiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_PERMISO_AGREGADO, "ROL",
                    $"Se agrega el permiso '{permiso.Nombre}' al rol '{arbolRol.Codigo}'. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al componer el rol: {ex.Message}", ex);
            }
        }

        public void QuitarPermisoDeRol(BE_Usuario usuarioLogueado, int idRol, int idPermiso)
        {
            try
            {
                dal_permiso.QuitarPermisoDeRol(idRol, idPermiso);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_PERMISO_QUITADO, "ROL",
                    $"Se quita el permiso ID {idPermiso} del rol ID {idRol}. Realizado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al quitar el permiso del rol: {ex.Message}", ex);
            }
        }
    }
}
