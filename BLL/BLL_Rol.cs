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
    public class BLL_Rol
    {
        DAL_Rol dal_rol = new DAL_Rol();
        DAL_Permiso dal_permiso = new DAL_Permiso();
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();

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

        public List<BE_Rol> ObtenerRolesDeUsuario(int idUsuario)
        {
            try
            {
                return dal_rol.ObtenerRolesDeUsuario(idUsuario);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los roles del usuario: {ex.Message}", ex);
            }
        }

        public void AgregarRol(BE_Usuario usuarioLogueado, BE_Rol rol)
        {
            try
            {
                ValidarNombre(rol.Nombre);

                if (dal_rol.ObtenerRoles().Any(r => string.Equals(r.Nombre, rol.Nombre, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException($"Ya existe un rol llamado '{rol.Nombre}'.");
                }

                dal_rol.AgregarRol(rol);

                rol.DVH = HashHelper.GenerarHash(CadenaRol(rol));
                dal_rol.ActualizarDVH(rol.IdRol, rol.DVH);

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
                    throw new InvalidOperationException($"Ya existe un rol llamado '{rol.Nombre}'.");
                }

                rol.DVH = HashHelper.GenerarHash(CadenaRol(rol));
                dal_rol.ModificarRol(rol);

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

                dal_rol.EliminarRolPorId(idRol);

                bll_bitacora.RegistrarEvento(usuarioLogueado.IdUsuario, AccionBitacora.ROL_BAJA, "ROL", $"Se elimina el rol '{rol?.Nombre}' (ID {idRol}). Eliminado por {usuarioLogueado.NombreApellido}.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar el rol: {ex.Message}", ex);
            }
        }

        private string CadenaRol(BE_Rol rol) => $"{rol.IdRol}|{rol.Nombre}";

        // La columna Rol.Nombre es VARCHAR(30): sin este chequeo, un nombre mas
        // largo se trunca en silencio del lado del cliente SQL (sin excepcion),
        // lo que podria hacer que dos roles con nombres distintos colisionen.
        private void ValidarNombre(string nombre)
        {
            if (!string.IsNullOrEmpty(nombre) && nombre.Length > 30)
            {
                throw new InvalidOperationException($"El nombre del rol no puede superar los 30 caracteres (tiene {nombre.Length}).");
            }
        }

        // ---- Composicion del arbol Composite de un Rol (sub-roles + permisos) ----

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
                // Este rol ya se esta construyendo mas arriba en la misma cadena:
                // hay un ciclo cargado en la base. No deberia pasar si todo se creo
                // a traves de AgregarSubRol, pero se corta aca para no recursionar
                // infinito en vez de tirar un StackOverflow.
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
                    throw new InvalidOperationException("Un rol no puede contenerse a si mismo.");
                }

                RolComponente arbolPadre = ConstruirArbolRol(idRolPadre);

                if (arbolPadre.SubRoles.Any(r => r.Rol.IdRol == idRolHijo))
                {
                    throw new InvalidOperationException($"El rol '{arbolPadre.Codigo}' ya contiene al rol seleccionado.");
                }

                RolComponente arbolHijo = ConstruirArbolRol(idRolHijo);

                // AgregarHijo valida que esto no cierre un ciclo (idRolPadre no
                // puede aparecer ya dentro del arbol de idRolHijo).
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
                    throw new InvalidOperationException("El permiso seleccionado no existe.");
                }

                if (arbolRol.TienePermiso(permiso.Nombre))
                {
                    throw new InvalidOperationException($"El rol '{arbolRol.Codigo}' ya tiene el permiso '{permiso.Nombre}' (directo o a traves de un sub-rol).");
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
