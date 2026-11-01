using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class bITACORA : PaginaBase
    {
        BLL_Bitacora bll_bitacora = new BLL_Bitacora();
        BLL_Usuario bll_usuarios = new BLL_Usuario();

        private const string ClaveSesionImportada = "BitacoraImportada";

        private BE_BitacoraImportada Importada
        {
            get { return Session[ClaveSesionImportada] as BE_BitacoraImportada; }
            set { Session[ClaveSesionImportada] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    Importada = null;

                    CargarUsuarios();
                    CargarModulos();
                    CargarCriticidades();
                    CargarGrilla();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Bitacora.Page_Load", ex);
                    lblPaginas.Text = T("No se pudo cargar la bitacora. Intenta nuevamente mas tarde.");
                }
            }
        }

        private void CargarUsuarios()
        {
            List<BE_Usuario> usuarios = bll_usuarios.UsuariosConRol(UsuarioLogueado);

            ddlUsuarios.DataSource = usuarios;
            ddlUsuarios.DataTextField = "NombreApellido";
            ddlUsuarios.DataValueField = "IdUsuario";

            ddlUsuarios.DataBind();

            HashSet<int> idsExistentes = new HashSet<int>(usuarios.Select(u => u.IdUsuario));

            IEnumerable<int> idsEliminados = bll_bitacora.ConsultarBitacora(UsuarioLogueado, null, null, null, null, null, null)
                .Where(b => b.IdUsuario.HasValue && !idsExistentes.Contains(b.IdUsuario.Value))
                .Select(b => b.IdUsuario.Value)
                .Distinct()
                .OrderBy(id => id);

            foreach (int id in idsEliminados)
            {
                ddlUsuarios.Items.Add(new ListItem(T("Usuario eliminado (ID {0})", id), id.ToString()));
            }

            ddlUsuarios.Items.Insert(0, new ListItem(T("Todos"), ""));
        }

        private void CargarModulos()
        {
            ddlModulos.Items.Clear();

            ddlModulos.Items.Add(new ListItem(T("Todos"), ""));

            ddlModulos.Items.Add(new ListItem("AGENDA", "AGENDA"));
            ddlModulos.Items.Add(new ListItem("ATENCION", "ATENCION"));
            ddlModulos.Items.Add(new ListItem("IDIOMA", "IDIOMA"));
            ddlModulos.Items.Add(new ListItem("LOGIN", "LOGIN"));
            ddlModulos.Items.Add(new ListItem("MEDICO", "MEDICO"));
            ddlModulos.Items.Add(new ListItem("PERMISO", "PERMISO"));
            ddlModulos.Items.Add(new ListItem("ROL", "ROL"));
            ddlModulos.Items.Add(new ListItem("SEGURIDAD", "SEGURIDAD"));
            ddlModulos.Items.Add(new ListItem("TURNOS", "TURNOS"));
            ddlModulos.Items.Add(new ListItem("USUARIO", "USUARIO"));
        }

        private void CargarCriticidades()
        {
            ddlCriticidad.Items.Clear();

            ddlCriticidad.Items.Add(new ListItem(T("Todas"), ""));
            ddlCriticidad.Items.Add(new ListItem(T("ALTA"), CriticidadBitacora.ALTA));
            ddlCriticidad.Items.Add(new ListItem(T("MEDIA"), CriticidadBitacora.MEDIA));
            ddlCriticidad.Items.Add(new ListItem(T("BAJA"), CriticidadBitacora.BAJA));
        }

        private void CargarGrilla()
        {
            DateTime? desde = string.IsNullOrEmpty(fechaDesde.Text) ? (DateTime?)null : DateTime.Parse(fechaDesde.Text).Date;
            DateTime? hasta = string.IsNullOrEmpty(fechaHasta.Text) ? (DateTime?)null : DateTime.Parse(fechaHasta.Text).Date.AddDays(1).AddSeconds(-1);

            int? idUsuario = string.IsNullOrEmpty(ddlUsuarios.SelectedValue) ? (int?)null : int.Parse(ddlUsuarios.SelectedValue);
            string modulo = string.IsNullOrEmpty(ddlModulos.SelectedValue) ? null : ddlModulos.SelectedValue;
            string ip = string.IsNullOrEmpty(txtIP.Text) ? null : txtIP.Text;
            string criticidad = string.IsNullOrEmpty(ddlCriticidad.SelectedValue) ? null : ddlCriticidad.SelectedValue;

            BE_BitacoraImportada importada = Importada;

            MostrarOrigen(importada);

            gvBitacora.DataSource = importada != null
                ? bll_bitacora.FiltrarEventos(UsuarioLogueado, importada.Eventos, desde, hasta, idUsuario, modulo, ip, criticidad)
                : bll_bitacora.ConsultarBitacora(UsuarioLogueado, desde, hasta, idUsuario, modulo, ip, criticidad);

            gvBitacora.DataBind();

            if (gvBitacora.PageCount == 0)
            {
                lblPaginas.Text = T("Sin resultados");
            }
            else
            {
                lblPaginas.Text = T("Pagina {0} de {1}", gvBitacora.PageIndex + 1, gvBitacora.PageCount);
            }
        }

        private void MostrarOrigen(BE_BitacoraImportada importada)
        {
            pnlArchivoImportado.Visible = importada != null;
            btnExportarXml.Visible = importada == null;

            if (importada != null)
            {
                lblArchivoImportado.Text = HttpUtility.HtmlEncode(T(
                    "Estas viendo el archivo {0} ({1} eventos), generado el {2} por {3}. No son datos de la bitacora del sistema y su contenido no esta verificado.",
                    importada.NombreArchivo, importada.Eventos.Count, importada.Generado, importada.GeneradoPor));
            }
        }

        protected void btnImportarXml_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("BITACORA_IMPORTAR"))
            {
                return;
            }

            try
            {
                if (!fuImportarXml.HasFile || !string.Equals(System.IO.Path.GetExtension(fuImportarXml.FileName), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    lblMensajeImportar.Text = T("Selecciona un archivo .xml para importar.");
                    return;
                }

                BE_BitacoraImportada importada = bll_bitacora.ImportarXml(
                    UsuarioLogueado, System.IO.Path.GetFileName(fuImportarXml.FileName), fuImportarXml.PostedFile.InputStream);

                Importada = importada;

                AplicarFiltrosDelArchivo(importada);

                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeImportar, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnImportarXml_Click", ex);
                lblMensajeImportar.Text = T("No se pudo importar el archivo. Intenta nuevamente mas tarde.");
            }
        }

        protected void btnVolverBitacora_Click(object sender, EventArgs e)
        {
            try
            {
                Importada = null;

                LimpiarFiltros();

                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnVolverBitacora_Click", ex);
                lblPaginas.Text = T("No se pudo cargar la bitacora. Intenta nuevamente mas tarde.");
            }
        }

        private void AplicarFiltrosDelArchivo(BE_BitacoraImportada importada)
        {
            fechaDesde.Text = importada.Desde?.ToString("yyyy-MM-dd") ?? "";
            fechaHasta.Text = importada.Hasta?.ToString("yyyy-MM-dd") ?? "";
            txtIP.Text = importada.Ip ?? "";

            SeleccionarFiltro(ddlUsuarios, importada.IdUsuario?.ToString(), T("Usuario eliminado (ID {0})", importada.IdUsuario));
            SeleccionarFiltro(ddlModulos, importada.Modulo, importada.Modulo);
            SeleccionarFiltro(ddlCriticidad, importada.Criticidad, importada.Criticidad);
        }

        private static void SeleccionarFiltro(DropDownList combo, string valor, string textoSiFalta)
        {
            if (string.IsNullOrEmpty(valor))
            {
                combo.SelectedIndex = 0;
                return;
            }

            if (combo.Items.FindByValue(valor) == null)
            {
                combo.Items.Add(new ListItem(textoSiFalta, valor));
            }

            combo.SelectedValue = valor;
        }

        private void LimpiarFiltros()
        {
            fechaDesde.Text = "";
            fechaHasta.Text = "";
            ddlUsuarios.SelectedIndex = 0;
            ddlModulos.SelectedIndex = 0;
            ddlCriticidad.SelectedIndex = 0;
            txtIP.Text = "";
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
                lblPaginas.Text = T("No se pudo aplicar el filtro. Intenta nuevamente mas tarde.");
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            try
            {
                LimpiarFiltros();

                gvBitacora.PageIndex = 0;
                CargarGrilla();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnLimpiar_Click", ex);
                lblPaginas.Text = T("No se pudieron limpiar los filtros. Intenta nuevamente mas tarde.");
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
                WSBitacora servicio = new WSBitacora();

                xml = servicio.ExportarBitacoraXml(
                    fechaDesde.Text, fechaHasta.Text, ddlUsuarios.SelectedValue,
                    ddlModulos.SelectedValue, txtIP.Text, ddlCriticidad.SelectedValue);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Bitacora.btnExportarXml_Click", ex);
                lblPaginas.Text = T("No se pudo exportar la bitacora. Intenta nuevamente mas tarde.");
                return;
            }

            DescargarXml(xml, $"Bitacora_{DateTime.Now:yyyy-MM-dd_HHmm}.xml");
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
                lblPaginas.Text = T("No se pudo cambiar de pagina. Intenta nuevamente mas tarde.");
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

            celda.Text = T(celda.Text);
        }

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
    }
}