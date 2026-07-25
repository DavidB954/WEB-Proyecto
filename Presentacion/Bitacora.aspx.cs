using BE;
using BLL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class bITACORA : System.Web.UI.Page
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        BLL_Usuario bll_usuarios = new BLL_Usuario();
        protected void Page_Load(object sender, EventArgs e)
        {
            //Se revalida en CADA carga (incluidos los postbacks de Filtrar/Limpiar/paginado), no solo la primera vez:
            //si la sesión vence mientras el usuario está en esta pantalla, un postback no debe poder seguir
            //trayendo datos de la bitácora sin sesión válida.
            BE_Usuario usuarioLogueado = Session["Usuario"] as BE_Usuario;

            if (!SeguridadHelper.TieneAcceso(usuarioLogueado, "WEBMASTER", "ADMINISTRADOR"))
            {
                Response.Redirect(usuarioLogueado == null ? "Login.aspx" : "AccesoDenegado.aspx");
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    CargarUsuarios();
                    CargarModulos();
                    CargarCriticidades();
                    CargarGrilla();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Bitacora.Page_Load", ex);
                    lblPaginas.Text = "No se pudo cargar la bitácora. Intentá nuevamente más tarde.";
                }
            }
        }


        private void CargarUsuarios()
        {
            ddlUsuarios.DataSource = bll_usuarios.Usuarios();
            ddlUsuarios.DataTextField = "NombreApellido";
            ddlUsuarios.DataValueField = "IdUsuario";

            ddlUsuarios.DataBind();

            ddlUsuarios.Items.Insert(0, new ListItem("Todos", ""));
        }

        private void CargarModulos()
        {
            ddlModulos.Items.Clear(); 

            ddlModulos.Items.Add(new ListItem("Todos", ""));


            ddlModulos.Items.Add(new ListItem("LOGIN", "LOGIN"));
            ddlModulos.Items.Add(new ListItem("USUARIO", "USUARIO"));
            ddlModulos.Items.Add(new ListItem("SEGURIDAD", "SEGURIDAD"));
            ddlModulos.Items.Add(new ListItem("ROL", "ROL"));
        }

        private void CargarCriticidades()
        {
            ddlCriticidad.Items.Clear();

            ddlCriticidad.Items.Add(new ListItem("Todas", ""));
            ddlCriticidad.Items.Add(new ListItem("ALTA", CriticidadBitacora.ALTA));
            ddlCriticidad.Items.Add(new ListItem("MEDIA", CriticidadBitacora.MEDIA));
            ddlCriticidad.Items.Add(new ListItem("BAJA", CriticidadBitacora.BAJA));
        }

        //Carga la grilla respetando los filtros actuales. La usan el primer load, Filtrar, Limpiar y el cambio de página: así al pasar de página no se pierden los filtros aplicados.
        private void CargarGrilla()
        {
            DateTime? desde = string.IsNullOrEmpty(fechaDesde.Text) ? (DateTime?)null : DateTime.Parse(fechaDesde.Text).Date;
            DateTime? hasta = string.IsNullOrEmpty(fechaHasta.Text) ? (DateTime?)null : DateTime.Parse(fechaHasta.Text).Date.AddDays(1).AddSeconds(-1);

            int? idUsuario = string.IsNullOrEmpty(ddlUsuarios.SelectedValue) ? (int?)null : int.Parse(ddlUsuarios.SelectedValue);
            string modulo = string.IsNullOrEmpty(ddlModulos.SelectedValue) ? null : ddlModulos.SelectedValue;
            string ip = string.IsNullOrEmpty(txtIP.Text) ? null : txtIP.Text;
            string criticidad = string.IsNullOrEmpty(ddlCriticidad.SelectedValue) ? null : ddlCriticidad.SelectedValue;

            bool sinFiltros = desde == null && hasta == null && idUsuario == null && modulo == null && ip == null && criticidad == null;

            //Se ordena de más reciente a más antiguo SOLO para mostrar. No se cambia el orden en ObtenerBitacora
            //porque el cálculo del DVV concatena las filas en orden, y alterarlo haría fallar la verificación de integridad.
            if (sinFiltros)
            {
                gvBitacora.DataSource = bll_bitacora.ObtenerBitacora()
                    .OrderByDescending(b => b.IdBitacora)
                    .ToList();
            }
            else
            {
                DataTable dt = bll_bitacora.FiltrarBitacora(desde, hasta, idUsuario, modulo, ip, criticidad);
                dt.DefaultView.Sort = "IdBitacora DESC";
                gvBitacora.DataSource = dt.DefaultView;
            }

            gvBitacora.DataBind();

            //Indicador de paginación: cuántas páginas hay y en cuál estamos.
            if (gvBitacora.PageCount == 0)
            {
                lblPaginas.Text = "Sin resultados";
            }
            else
            {
                lblPaginas.Text = $"Página {gvBitacora.PageIndex + 1} de {gvBitacora.PageCount}";
            }
        }

        public void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                //Al cambiar el filtro se vuelve a la primera página, porque el resultado puede tener menos páginas que la actual.
                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnFiltrar_Click", ex);
                lblPaginas.Text = "No se pudo aplicar el filtro. Intentá nuevamente más tarde.";
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            try
            {
                fechaDesde.Text = "";
                fechaHasta.Text = "";
                ddlUsuarios.SelectedIndex = 0;
                ddlModulos.SelectedIndex = 0;
                ddlCriticidad.SelectedIndex = 0;
                txtIP.Text = "";

                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnLimpiar_Click", ex);
                lblPaginas.Text = "No se pudieron limpiar los filtros. Intentá nuevamente más tarde.";
            }
        }

        protected void gvBitacora_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                gvBitacora.PageIndex = e.NewPageIndex;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.gvBitacora_PageIndexChanging", ex);
                lblPaginas.Text = "No se pudo cambiar de página. Intentá nuevamente más tarde.";
            }
        }

        //Pinta la celda de Criticidad según su valor para identificar de un vistazo los eventos graves.
        protected void gvBitacora_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            //OJO: la columna "ID" (IdBitacora) tiene Visible="false" en el markup. Una columna invisible no genera
            //celda en absoluto (GridView la excluye de Cells), así que el índice NO es "ID=0, ID Usuario=1,
            //Fecha y Hora=2, Accion=3, Criticidad=4": es IdUsuario=0, FechaHora=1, Accion=2, Criticidad=3.
            TableCell celda = e.Row.Cells[3];

            switch (celda.Text)
            {
                case CriticidadBitacora.ALTA:
                    celda.CssClass = "criticidad-alta";
                    break;
                case CriticidadBitacora.MEDIA:
                    celda.CssClass = "criticidad-media";
                    break;
                case CriticidadBitacora.BAJA:
                    celda.CssClass = "criticidad-baja";
                    break;
            }
        }

        //Deja rastro en el mismo log que usa Global.asax, sin mostrarle al usuario el detalle interno de la excepción.
        private void RegistrarErrorInterno(string origen, Exception ex)
        {
            try
            {
                string carpetaLogs = Server.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(carpetaLogs);
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(carpetaLogs, "errores.log"),
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {origen} - {ex}{Environment.NewLine}");
            }
            catch
            {
                //Si ni el log funciona, no hay nada más para hacer acá.
            }
        }
    }
}