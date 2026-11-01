using BE.Seguridad;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public class PaginaSeguridadBase : PaginaBase
    {
        protected void CargarComboPaginas(DropDownList combo)
        {
            string paginaSeleccionada = combo.SelectedValue;

            combo.Items.Clear();

            foreach (string pagina in CatalogoPermisos.Todos.Select(p => p.Pagina).Distinct())
            {
                combo.Items.Add(new ListItem(pagina, pagina));
            }

            if (combo.Items.FindByValue(paginaSeleccionada) != null)
            {
                combo.SelectedValue = paginaSeleccionada;
            }
        }

        protected void CargarPermisosDisponibles(DropDownList combo, Button boton, IEnumerable<BE.BE_Permiso> permisos, string pagina, Func<string, bool> tienePermiso)
        {
            bool tienePaginaCompleta = CatalogoPermisos.DePagina(pagina).Any(p => p.Tipo == CatalogoPermisos.TipoPagina && tienePermiso(p.Codigo));

            Dictionary<string, int> idsPorNombre = permisos
                .GroupBy(p => p.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().IdPermiso, StringComparer.OrdinalIgnoreCase);

            combo.Items.Clear();

            foreach (PermisoDefinicion definicion in CatalogoPermisos.DePagina(pagina).OrderBy(p => p.Tipo == CatalogoPermisos.TipoPagina ? 0 : 1))
            {
                if (tienePaginaCompleta || tienePermiso(definicion.Codigo) || !idsPorNombre.TryGetValue(definicion.Codigo, out int idPermiso))
                {
                    continue;
                }

                string texto = definicion.Tipo == CatalogoPermisos.TipoPagina
                    ? T("Toda la pagina ({0})", definicion.Codigo)
                    : T("Solo: {0} ({1})", T(definicion.Descripcion), definicion.Codigo);

                combo.Items.Add(new ListItem(texto, idPermiso.ToString()));
            }

            IndicarSiNoHayOpciones(combo, boton, T("(ya tiene todos los permisos de esta pagina)"));
        }

        protected void IndicarSiNoHayOpciones(DropDownList combo, Button boton, string textoSinOpciones)
        {
            bool hayOpciones = combo.Items.Count > 0;

            if (!hayOpciones)
            {
                combo.Items.Add(new ListItem(textoSinOpciones, ""));
            }

            combo.Enabled = hayOpciones;
            boton.Enabled = hayOpciones;
        }

        protected static TreeNode CrearNodo(string texto)
        {
            return new TreeNode(texto) { SelectAction = TreeNodeSelectAction.None };
        }

        protected void AgregarNodosHijos(TreeNode nodoPadre, IEnumerable<IComponentePermiso> hijos, Func<string, string> textoPermiso)
        {
            foreach (IComponentePermiso hijo in hijos)
            {
                if (hijo is PermisoComponente permiso)
                {
                    nodoPadre.ChildNodes.Add(CrearNodo(textoPermiso(permiso.Codigo)));
                }
                else if (hijo is RolComponente rol)
                {
                    TreeNode nodoRol = CrearNodo("Rol: " + rol.Codigo);
                    AgregarNodosHijos(nodoRol, rol.Hijos, textoPermiso);
                    nodoPadre.ChildNodes.Add(nodoRol);
                }
            }
        }
    }
}
