using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Presentacion
{
    // Pantalla del medico: ve los turnos de un dia de su agenda, atiende a los
    // pacientes (motivo, diagnostico, receta), marca ausentes y consulta la
    // historia clinica del paciente que esta atendiendo.
    public partial class WebForm2 : PaginaBase
    {
        BLL_AtencionMedica bll_atencion = new BLL_AtencionMedica();

        private const string FormatoFecha = "yyyy-MM-dd";

        private int? IdTurnoSeleccionado
        {
            get { return ViewState["IdTurnoSeleccionado"] as int?; }
            set { ViewState["IdTurnoSeleccionado"] = value; }
        }

        // Medicamentos agregados en pantalla que todavia no se guardaron.
        private List<BE_Receta> RecetaPendiente
        {
            get
            {
                List<BE_Receta> lista = ViewState["RecetaPendiente"] as List<BE_Receta>;
                if (lista == null)
                {
                    lista = new List<BE_Receta>();
                    ViewState["RecetaPendiente"] = lista;
                }
                return lista;
            }
        }

        private DateTime FechaSeleccionada
        {
            get
            {
                return DateTime.TryParseExact(txtFecha.Text, FormatoFecha, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha)
                    ? fecha
                    : DateTime.Today;
            }
            set
            {
                txtFecha.Text = value.ToString(FormatoFecha);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // El acceso a la pagina lo valida PaginaBase segun CatalogoPermisos.

            if (!IsPostBack)
            {
                try
                {
                    if (!bll_atencion.EsMedicoRegistrado(UsuarioLogueado.IdUsuario))
                    {
                        pnlMedico.Visible = false;
                        pnlSinMedico.Visible = true;
                        return;
                    }

                    FechaSeleccionada = DateTime.Today;
                    BindTurnos();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Medicos.Page_Load", ex);
                    MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos del dia. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        // ---- Turnos del dia ----

        private void BindTurnos()
        {
            gvTurnosMedicos.DataSource = bll_atencion.ObtenerTurnosDelDia(UsuarioLogueado, FechaSeleccionada);
            gvTurnosMedicos.DataBind();

            // Despues de rebindear se vuelve a marcar la fila que se esta atendiendo.
            gvTurnosMedicos.SelectedIndex = -1;
            for (int i = 0; i < gvTurnosMedicos.DataKeys.Count; i++)
            {
                if ((int)gvTurnosMedicos.DataKeys[i].Value == IdTurnoSeleccionado)
                {
                    gvTurnosMedicos.SelectedIndex = i;
                    break;
                }
            }
        }

        private void CambiarDia(DateTime fecha)
        {
            FechaSeleccionada = fecha;
            CerrarAtencion();
            BindTurnos();
        }

        protected void txtFecha_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CambiarDia(FechaSeleccionada);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.txtFecha_TextChanged", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos de ese dia. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnDiaAnterior_Click(object sender, EventArgs e)
        {
            try
            {
                CambiarDia(FechaSeleccionada.AddDays(-1));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnDiaAnterior_Click", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos de ese dia. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnDiaSiguiente_Click(object sender, EventArgs e)
        {
            try
            {
                CambiarDia(FechaSeleccionada.AddDays(1));
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnDiaSiguiente_Click", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos de ese dia. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnHoy_Click(object sender, EventArgs e)
        {
            try
            {
                CambiarDia(DateTime.Today);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnHoy_Click", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos del dia. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvTurnosMedicos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ExigirPermiso("MEDICO_ATENDER"))
            {
                return;
            }

            try
            {
                if (gvTurnosMedicos.SelectedIndex < 0 || gvTurnosMedicos.SelectedIndex >= gvTurnosMedicos.DataKeys.Count)
                {
                    return;
                }

                int idTurno = (int)gvTurnosMedicos.DataKeys[gvTurnosMedicos.SelectedIndex].Value;

                AbrirAtencion(bll_atencion.ObtenerTurnoParaAtender(UsuarioLogueado, idTurno));
            }
            catch (InvalidOperationException ex)
            {
                CerrarAtencion();
                BindTurnosSinFallar();
                MostrarMensaje(lblMensajeTurnos, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.gvTurnosMedicos_SelectedIndexChanged", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudo seleccionar el turno."), false);
            }
        }

        protected void gvTurnosMedicos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // "Select" lo resuelve gvTurnosMedicos_SelectedIndexChanged.
            if (e.CommandName != "Ausente")
            {
                return;
            }

            if (!ExigirPermiso("MEDICO_ATENDER"))
            {
                return;
            }

            try
            {
                if (!int.TryParse(Convert.ToString(e.CommandArgument), out int idTurno))
                {
                    MostrarMensaje(lblMensajeTurnos, T("El turno seleccionado ya no esta disponible."), false);
                    return;
                }

                bll_atencion.MarcarAusente(UsuarioLogueado, idTurno);

                if (IdTurnoSeleccionado == idTurno)
                {
                    CerrarAtencion();
                }

                BindTurnos();
                MostrarMensaje(lblMensajeTurnos, T("El turno quedo marcado como ausente."), true);
                MostrarToast(T("Paciente marcado como ausente."), true);
            }
            catch (InvalidOperationException ex)
            {
                BindTurnosSinFallar();
                MostrarMensaje(lblMensajeTurnos, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.gvTurnosMedicos_RowCommand", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudo marcar el turno como ausente. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Atencion ----

        private void AbrirAtencion(BE_Turno turno)
        {
            IdTurnoSeleccionado = turno.IdTurno;
            RecetaPendiente.Clear();

            lblPacienteSeleccionado.Text = System.Web.HttpUtility.HtmlEncode(T("Atendiendo a {0} ({1} anios, HC {2}) - turno de las {3} hs.", turno.NombrePaciente, turno.EdadPaciente, turno.HistoriaClinica, turno.HoraTexto));

            txtMotivoConsulta.Text = string.Empty;
            txtDiagnostico.Text = string.Empty;
            txtObservaciones.Text = string.Empty;
            txtMedicamento.Text = string.Empty;
            txtObservacionesReceta.Text = string.Empty;
            BindReceta();

            pnlAtencion.Visible = true;

            gvHistoria.DataSource = bll_atencion.ObtenerHistoriaClinica(UsuarioLogueado, turno.IdPaciente);
            gvHistoria.DataBind();
            pnlHistoria.Visible = true;
        }

        private void CerrarAtencion()
        {
            IdTurnoSeleccionado = null;
            RecetaPendiente.Clear();
            gvTurnosMedicos.SelectedIndex = -1;

            lblPacienteSeleccionado.Text = T("Ningun paciente seleccionado.");
            pnlAtencion.Visible = false;
            pnlHistoria.Visible = false;
        }

        private void BindReceta()
        {
            gvReceta.DataSource = RecetaPendiente;
            gvReceta.DataBind();
        }

        protected void btnAgregarMedicamento_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("MEDICO_ATENDER"))
            {
                return;
            }

            try
            {
                if (RecetaPendiente.Count >= BLL_AtencionMedica.MaximoMedicamentos)
                {
                    MostrarMensaje(lblMensajeAtencion, T("Una receta puede tener hasta {0} medicamentos.", BLL_AtencionMedica.MaximoMedicamentos), false);
                    return;
                }

                RecetaPendiente.Add(bll_atencion.ArmarMedicamento(txtMedicamento.Text, txtObservacionesReceta.Text));

                txtMedicamento.Text = string.Empty;
                txtObservacionesReceta.Text = string.Empty;
                BindReceta();
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAtencion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnAgregarMedicamento_Click", ex);
                MostrarMensaje(lblMensajeAtencion, T("No se pudo agregar el medicamento. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvReceta_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName != "Quitar")
                {
                    return;
                }

                if (int.TryParse(Convert.ToString(e.CommandArgument), out int indice) && indice >= 0 && indice < RecetaPendiente.Count)
                {
                    RecetaPendiente.RemoveAt(indice);
                }

                BindReceta();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.gvReceta_RowCommand", ex);
                MostrarMensaje(lblMensajeAtencion, T("No se pudo quitar el medicamento. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnGuardarAtencion_Click(object sender, EventArgs e)
        {
            if (!ExigirPermiso("MEDICO_ATENDER"))
            {
                return;
            }

            try
            {
                if (!IdTurnoSeleccionado.HasValue)
                {
                    MostrarMensaje(lblMensajeAtencion, T("Primero selecciona un paciente de la lista de turnos."), false);
                    return;
                }

                // Si quedo un medicamento escrito sin presionar "Agregar", se avisa
                // en lugar de perderlo en silencio.
                if (!string.IsNullOrWhiteSpace(txtMedicamento.Text))
                {
                    MostrarMensaje(lblMensajeAtencion, T("Quedo un medicamento sin agregar a la receta: presiona \"Agregar\" o borralo."), false);
                    return;
                }

                bll_atencion.RegistrarAtencion(UsuarioLogueado, IdTurnoSeleccionado.Value,
                    txtMotivoConsulta.Text, txtDiagnostico.Text, txtObservaciones.Text, RecetaPendiente);

                CerrarAtencion();
                BindTurnos();

                MostrarMensaje(lblMensajeAtencion, T("La atencion quedo registrada correctamente."), true);
                MostrarToast(T("Atencion registrada."), true);
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeAtencion, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnGuardarAtencion_Click", ex);
                MostrarMensaje(lblMensajeAtencion, T("No se pudo registrar la atencion. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void btnCancelarAtencion_Click(object sender, EventArgs e)
        {
            try
            {
                CerrarAtencion();
                BindTurnos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnCancelarAtencion_Click", ex);
                MostrarMensaje(lblMensajeAtencion, T("No se pudo cerrar la atencion. Intenta nuevamente mas tarde."), false);
            }
        }

        // ---- Auxiliares ----

        // Refresco despues de un error de negocio: si falla, se registra y se
        // deja el mensaje original en pantalla.
        private void BindTurnosSinFallar()
        {
            try
            {
                BindTurnos();
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.BindTurnosSinFallar", ex);
            }
        }

        private void MostrarMensaje(Label label, string texto, bool exito)
        {
            // Los mensajes pueden traer nombres cargados por usuarios.
            label.Text = System.Web.HttpUtility.HtmlEncode(texto);
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
