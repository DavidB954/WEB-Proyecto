using BE;
using BE.Seguridad;
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
    public partial class bITACORA : PaginaBase
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        BLL_Usuario bll_usuarios = new BLL_Usuario();
        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

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
                    lblPaginas.Text = "No se pudo cargar la bitacora. Intenta nuevamente mas tarde.";
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

        private void CargarGrilla()
        {
            DateTime? desde = string.IsNullOrEmpty(fechaDesde.Text) ? (DateTime?)null : DateTime.Parse(fechaDesde.Text).Date;
            DateTime? hasta = string.IsNullOrEmpty(fechaHasta.Text) ? (DateTime?)null : DateTime.Parse(fechaHasta.Text).Date.AddDays(1).AddSeconds(-1);

            int? idUsuario = string.IsNullOrEmpty(ddlUsuarios.SelectedValue) ? (int?)null : int.Parse(ddlUsuarios.SelectedValue);
            string modulo = string.IsNullOrEmpty(ddlModulos.SelectedValue) ? null : ddlModulos.SelectedValue;
            string ip = string.IsNullOrEmpty(txtIP.Text) ? null : txtIP.Text;
            string criticidad = string.IsNullOrEmpty(ddlCriticidad.SelectedValue) ? null : ddlCriticidad.SelectedValue;

            bool sinFiltros = desde == null && hasta == null && idUsuario == null && modulo == null && ip == null && criticidad == null;

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

            if (gvBitacora.PageCount == 0)
            {
                lblPaginas.Text = "Sin resultados";
            }
            else
            {
                lblPaginas.Text = $"Pagina {gvBitacora.PageIndex + 1} de {gvBitacora.PageCount}";
            }
        }

        public void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnFiltrar_Click", ex);
                lblPaginas.Text = "No se pudo aplicar el filtro. Intenta nuevamente mas tarde.";
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
                lblPaginas.Text = "No se pudieron limpiar los filtros. Intenta nuevamente mas tarde.";
            }
        }

        protected void btnExportarXml_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("BITACORA_EXPORTAR"))
            {
                return;
            }

            string xml;

            try
            {
                // Se usa la clase del Web Service (WSBitacora.asmx) en el mismo
                // proceso y no por HTTP: este request tiene tomada la sesion del
                // usuario, y una llamada HTTP al servicio (que tambien la necesita
                // para validar permisos) quedaria esperando a que se libere.
                WSBitacora servicio = new WSBitacora();

                xml = servicio.ExportarBitacoraXml(
                    fechaDesde.Text, fechaHasta.Text, ddlUsuarios.SelectedValue,
                    ddlModulos.SelectedValue, txtIP.Text, ddlCriticidad.SelectedValue);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnExportarXml_Click", ex);
                lblPaginas.Text = "No se pudo exportar la bitacora. Intenta nuevamente mas tarde.";
                return;
            }

            DescargarXml(xml, $"Bitacora_{DateTime.Now:yyyy-MM-dd_HHmm}.xml");
        }

        // Reemplaza la respuesta de la pagina por el archivo. Se evita Response.End()
        // (corta con ThreadAbortException); SuppressContent descarta el HTML que la
        // pagina igual va a generar despues de este evento.
        private void DescargarXml(string xml, string nombreArchivo)
        {
            Response.Clear();
            Response.ContentType = "application/xml";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            Response.AddHeader("Content-Disposition", $"attachment; filename={nombreArchivo}");
            Response.Write(xml);
            Response.Flush();
            Response.SuppressContent = true;
            Context.ApplicationInstance.CompleteRequest();
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
                lblPaginas.Text = "No se pudo cambiar de pagina. Intenta nuevamente mas tarde.";
            }
        }

        protected void gvBitacora_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            int indiceCriticidad = IndiceColumna("Criticidad");

            if (indiceCriticidad < 0)
            {
                return;
            }

            TableCell celda = e.Row.Cells[indiceCriticidad];

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

        // Por DataField y no por posicion fija: las columnas ocultas (ID) tambien
        // tienen celda en cada fila, y el indice se corre si se agregan columnas.
        private int IndiceColumna(string dataField)
        {
            for (int i = 0; i < gvBitacora.Columns.Count; i++)
            {
                if (gvBitacora.Columns[i] is BoundField columna && columna.DataField == dataField)
                {
                    return i;
                }
            }

            return -1;
        }

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
            }
        }
    }
}