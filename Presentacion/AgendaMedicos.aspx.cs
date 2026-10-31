using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class AgendaMedicos : PaginaBase
    {
        BLL_Medico bll_medico = new BLL_Medico();
        BLL_Especialidad bll_especialidad = new BLL_Especialidad();
        BLL_AgendaMedico bll_agenda = new BLL_AgendaMedico();

        private const string FormatoHora = @"hh\:mm";

        // Los combos de hora ofrecen intervalos de 15 minutos.
        private const int PasoMinutos = 15;

        private int? IdMedicoSeleccionado
        {
            get { return ViewState["IdMedicoSeleccionado"] as int?; }
            set { ViewState["IdMedicoSeleccionado"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

            if (!IsPostBack)
            {
                try
                {
                    CargarCombosHorario();
                    CargarEspecialidades();
                    RefrescarMedicos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("AgendaMedicos.Page_Load", ex);
                    MostrarMensaje(lblMensajeEspecialidad, T("No se pudieron cargar los medicos. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        // ---- Carga de datos ----

        private void CargarCombosHorario()
        {
            cblDias.Items.Clear();
            for (int dia = 1; dia <= 7; dia++)
            {
                cblDias.Items.Add(new ListItem(BE_MedicoHorario.ObtenerNombreDia(dia), dia.ToString()));
            }

            foreach (DropDownList ddl in new[] { ddlHoraDesde, ddlHoraHasta, ddlBloqueoDesde, ddlBloqueoHasta })
            {
                ddl.Items.Clear();
                for (TimeSpan hora = TimeSpan.Zero; hora < TimeSpan.FromDays(1); hora = hora.Add(TimeSpan.FromMinutes(PasoMinutos)))
                {
                    string texto = hora.ToString(FormatoHora);
                    ddl.Items.Add(new ListItem(texto, texto));
                }
            }

            ddlHoraDesde.SelectedValue = "08:00";
            ddlHoraHasta.SelectedValue = "12:00";
            ddlBloqueoDesde.SelectedValue = "08:00";
            ddlBloqueoHasta.SelectedValue = "12:00";

            ddlDuracion.Items.Clear();
            foreach (int minutos in BLL_AgendaMedico.DuracionesPermitidas)
            {
                ddlDuracion.Items.Add(new ListItem(minutos + " min", minutos.ToString()));
            }
            ddlDuracion.SelectedValue = "15";

            HabilitarHorasBloqueo();
        }

        private void CargarEspecialidades()
        {
            List<BE_Especialidad> especialidades = bll_especialidad.ObtenerEspecialidades();

            string filtroActual = ddlFiltroEspecialidad.SelectedValue;
            ddlFiltroEspecialidad.DataSource = especialidades;
            ddlFiltroEspecialidad.DataBind();
            ddlFiltroEspecialidad.Items.Insert(0, new ListItem(T("Todas"), ""));
            if (ddlFiltroEspecialidad.Items.FindByValue(filtroActual) != null)
            {
                ddlFiltroEspecialidad.SelectedValue = filtroActual;
            }

            cblEspecialidades.DataSource = especialidades;
            cblEspecialidades.DataBind();

            gvEspecialidades.DataSource = especialidades;
            gvEspecialidades.DataBind();
        }

        private void RefrescarMedicos()
        {
            int? idEspecialidad = null;
            if (int.TryParse(ddlFiltroEspecialidad.SelectedValue, out int id))
            {
                idEspecialidad = id;
            }

            gvMedicos.DataSource = bll_medico.BuscarMedicos(txtBuscarNombre.Text, idEspecialidad);
            gvMedicos.DataBind();

            // Despues de rebindear, se vuelve a marcar la fila del medico que se
            // esta editando (si sigue apareciendo con el filtro actual).
            gvMedicos.SelectedIndex = -1;
            for (int i = 0; i < gvMedicos.DataKeys.Count; i++)
            {
                if ((int)gvMedicos.DataKeys[i].Value == IdMedicoSeleccionado)
                {
                    gvMedicos.SelectedIndex = i;
                    break;
                }
            }
        }

        private void MostrarMedico(int idMedico)
        {
            BE_Medico medico = bll_medico.BuscarMedicos(null, null).FirstOrDefault(m => m.IdMedico == idMedico);

            if (medico == null)
            {
                IdMedicoSeleccionado = null;
                pnlMedico.Visible = false;
                pnlSinMedico.Visible = true;
                return;
            }

            IdMedicoSeleccionado = idMedico;
            pnlSinMedico.Visible = false;
            pnlMedico.Visible = true;

            lblMedicoSeleccionado.Text = T("Medico: {0}", medico.NombreApellido)
                + " - " + (medico.Matricula.HasValue ? T("Matricula {0}", medico.Matricula.Value) : T("sin matricula registrada"))
                + (medico.UsuarioActivo ? "" : " (" + T("usuario inactivo") + ")");

            txtMatricula.Text = medico.Matricula.HasValue ? medico.Matricula.Value.ToString() : string.Empty;

            List<int> idsEspecialidad = bll_medico.ObtenerIdsEspecialidad(idMedico);
            foreach (ListItem item in cblEspecialidades.Items)
            {
                item.Selected = idsEspecialidad.Contains(int.Parse(item.Value));
            }

            BindHorarios();
            BindBloqueos();
        }

        private void BindHorarios()
        {
            gvHorarios.DataSource = bll_agenda.ObtenerHorarios(IdMedicoSeleccionado.Value);
            gvHorarios.DataBind();
        }

        private void BindBloqueos()
        {
            gvBloqueos.DataSource = bll_agenda.ObtenerBloqueosVigentes(IdMedicoSeleccionado.Value);
            gvBloqueos.DataBind();
        }

        // ---- Buscador ----

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            try
            {
                RefrescarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnBuscar_Click", ex);
                MostrarMensaje(lblMensajeEspecialidad, T("No se pudo realizar la busqueda. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            try
            {
                txtBuscarNombre.Text = string.Empty;
                ddlFiltroEspecialidad.SelectedIndex = 0;
                RefrescarMedicos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnLimpiarBusqueda_Click", ex);
                MostrarMensaje(lblMensajeEspecialidad, T("No se pudo limpiar la busqueda. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idMedico = (int)gvMedicos.SelectedDataKey.Value;
                MostrarMedico(idMedico);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.gvMedicos_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeEspecialidad, T("No se pudo cargar el medico seleccionado."), false);
            }
        }

        // ---- Matricula y especialidades ----

        protected void btnGuardarDatos_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("MEDICO_DATOS_EDITAR"))
            {
                return;
            }

            try
            {
                if (!IdMedicoSeleccionado.HasValue)
                {
                    MostrarMensaje(lblMensaje, T("Selecciona un medico de la lista."), false);
                    return;
                }

                if (!int.TryParse(txtMatricula.Text.Trim(), out int matricula))
                {
                    MostrarMensaje(lblMensaje, T("La matricula debe ser un numero entero."), false);
                    return;
                }

                List<int> idsEspecialidad = cblEspecialidades.Items.Cast<ListItem>()
                    .Where(i => i.Selected)
                    .Select(i => int.Parse(i.Value))
                    .ToList();

                bll_medico.GuardarDatosMedico(UsuarioLogueado, IdMedicoSeleccionado.Value, matricula, idsEspecialidad);

                RefrescarMedicos();
                MostrarMedico(IdMedicoSeleccionado.Value);
                MostrarToast(T("Datos del medico guardados correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnGuardarDatos_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudieron guardar los datos del medico. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Franjas semanales ----

        protected void btnAgregarHorario_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("AGENDA_HORARIO_EDITAR"))
            {
                return;
            }

            try
            {
                if (!IdMedicoSeleccionado.HasValue)
                {
                    MostrarMensaje(lblMensaje, T("Selecciona un medico de la lista."), false);
                    return;
                }

                List<int> dias = cblDias.Items.Cast<ListItem>()
                    .Where(i => i.Selected)
                    .Select(i => int.Parse(i.Value))
                    .ToList();

                TimeSpan desde = LeerHora(ddlHoraDesde);
                TimeSpan hasta = LeerHora(ddlHoraHasta);
                int duracion = int.Parse(ddlDuracion.SelectedValue);

                bll_agenda.AgregarHorarios(UsuarioLogueado, IdMedicoSeleccionado.Value, dias, desde, hasta, duracion);

                foreach (ListItem item in cblDias.Items)
                {
                    item.Selected = false;
                }

                BindHorarios();
                RefrescarMedicos();
                MostrarToast(T("Franja horaria agregada correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnAgregarHorario_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo agregar la franja horaria. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvHorarios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Activar" && e.CommandName != "Desactivar" && e.CommandName != "Eliminar")
            {
                return;
            }

            if (!ExigirPermiso("AGENDA_HORARIO_EDITAR"))
            {
                return;
            }

            try
            {
                if (!IdMedicoSeleccionado.HasValue)
                {
                    return;
                }

                int idHorario = Convert.ToInt32(e.CommandArgument);

                if (e.CommandName == "Eliminar")
                {
                    int cancelados = bll_agenda.EliminarHorario(UsuarioLogueado, IdMedicoSeleccionado.Value, idHorario);
                    MostrarToast(T("Franja horaria eliminada.") + TextoTurnosCancelados(cancelados), true);
                }
                else
                {
                    bool activar = e.CommandName == "Activar";
                    int cancelados = bll_agenda.CambiarEstadoHorario(UsuarioLogueado, IdMedicoSeleccionado.Value, idHorario, activar);
                    MostrarToast((activar ? T("Franja horaria activada.") : T("Franja horaria desactivada.")) + TextoTurnosCancelados(cancelados), true);
                }

                BindHorarios();
                RefrescarMedicos();
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.gvHorarios_RowCommand", ex);
                MostrarMensaje(lblMensaje, T("No se pudo actualizar la franja horaria. Intenta nuevamente mas tarde."), false);
            }
        }

        private string TextoTurnosCancelados(int cancelados)
        {
            return cancelados == 0 ? "" : " " + T("Se cancelaron {0} turno(s) reservado(s).", cancelados);
        }

        // ---- Bloqueos puntuales ----

        protected void chkDiaCompleto_CheckedChanged(object sender, EventArgs e)
        {
            HabilitarHorasBloqueo();
        }

        private void HabilitarHorasBloqueo()
        {
            ddlBloqueoDesde.Enabled = !chkDiaCompleto.Checked;
            ddlBloqueoHasta.Enabled = !chkDiaCompleto.Checked;
        }

        protected void btnAgregarBloqueo_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("AGENDA_BLOQUEO_EDITAR"))
            {
                return;
            }

            try
            {
                if (!IdMedicoSeleccionado.HasValue)
                {
                    MostrarMensaje(lblMensaje, T("Selecciona un medico de la lista."), false);
                    return;
                }

                if (!DateTime.TryParseExact(txtFechaBloqueo.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
                {
                    MostrarMensaje(lblMensaje, T("Selecciona la fecha a bloquear."), false);
                    return;
                }

                TimeSpan? desde = null;
                TimeSpan? hasta = null;

                if (!chkDiaCompleto.Checked)
                {
                    desde = LeerHora(ddlBloqueoDesde);
                    hasta = LeerHora(ddlBloqueoHasta);
                }

                int cancelados = bll_agenda.AgregarBloqueo(UsuarioLogueado, IdMedicoSeleccionado.Value, fecha, desde, hasta, txtMotivoBloqueo.Text);

                txtFechaBloqueo.Text = string.Empty;
                txtMotivoBloqueo.Text = string.Empty;

                BindBloqueos();
                MostrarToast(T("Bloqueo registrado correctamente.") + TextoTurnosCancelados(cancelados), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnAgregarBloqueo_Click", ex);
                MostrarMensaje(lblMensaje, T("No se pudo registrar el bloqueo. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvBloqueos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Quitar")
            {
                return;
            }

            if (!ExigirPermiso("AGENDA_BLOQUEO_EDITAR"))
            {
                return;
            }

            try
            {
                if (!IdMedicoSeleccionado.HasValue)
                {
                    return;
                }

                bll_agenda.EliminarBloqueo(UsuarioLogueado, IdMedicoSeleccionado.Value, Convert.ToInt32(e.CommandArgument));

                BindBloqueos();
                MostrarToast(T("Bloqueo quitado."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensaje, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.gvBloqueos_RowCommand", ex);
                MostrarMensaje(lblMensaje, T("No se pudo quitar el bloqueo. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Catalogo de especialidades ----

        protected void btnAgregarEspecialidad_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("ESPECIALIDAD_ABM"))
            {
                return;
            }

            try
            {
                bll_especialidad.AgregarEspecialidad(UsuarioLogueado, txtNuevaEspecialidad.Text);

                txtNuevaEspecialidad.Text = string.Empty;
                RecargarTrasCambioDeEspecialidades();
                MostrarToast(T("Especialidad agregada correctamente."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeEspecialidad, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.btnAgregarEspecialidad_Click", ex);
                MostrarMensaje(lblMensajeEspecialidad, T("No se pudo agregar la especialidad. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvEspecialidades_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Eliminar")
            {
                return;
            }

            if (!ExigirPermiso("ESPECIALIDAD_ABM"))
            {
                return;
            }

            try
            {
                bll_especialidad.EliminarEspecialidad(UsuarioLogueado, Convert.ToInt32(e.CommandArgument));

                RecargarTrasCambioDeEspecialidades();
                MostrarToast(T("Especialidad eliminada."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeEspecialidad, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("AgendaMedicos.gvEspecialidades_RowCommand", ex);
                MostrarMensaje(lblMensajeEspecialidad, T("No se pudo eliminar la especialidad. Intenta nuevamente mas tarde."), false);
            }
        }

        // El combo de filtro y los checks del medico dependen del catalogo: se
        // recargan y se vuelven a marcar las especialidades del medico abierto.
        private void RecargarTrasCambioDeEspecialidades()
        {
            CargarEspecialidades();
            RefrescarMedicos();

            if (IdMedicoSeleccionado.HasValue)
            {
                MostrarMedico(IdMedicoSeleccionado.Value);
            }
        }

        // ---- Utilidades ----

        private static TimeSpan LeerHora(DropDownList ddl)
        {
            return TimeSpan.ParseExact(ddl.SelectedValue, FormatoHora, CultureInfo.InvariantCulture);
        }

        private void MostrarMensaje(Label label, string texto, bool exito)
        {
            label.Text = texto;
            label.CssClass = exito ? "msg-form msg-exito" : "msg-form msg-error";
        }

        private void MostrarToast(string mensaje, bool exito)
        {
            string clase = exito ? "toast-exito" : "toast-error";
            string texto = System.Web.HttpUtility.JavaScriptStringEncode(mensaje, true);

            string script =
                "(function(){var t=document.createElement('div');t.className='toast " + clase + "';" +
                "t.textContent=" + texto + ";document.body.appendChild(t);" +
                "setTimeout(function(){t.className+=' toast-hide';},3000);" +
                "setTimeout(function(){if(t.parentNode){t.parentNode.removeChild(t);}},3600);})();";

            ScriptManager.RegisterStartupScript(this, GetType(), "toast_" + Guid.NewGuid().ToString("N"), script, true);
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
