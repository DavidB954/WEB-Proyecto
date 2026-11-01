using BE;
using BLL;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;

namespace Presentacion
{
    public partial class WebForm2 : PaginaBase
    {
        BLL_AtencionMedica bll_atencion = new BLL_AtencionMedica();

        private const string FormatoFecha = "yyyy-MM-dd";

        private int? IdTurnoSeleccionado
        {
            get { return ViewState["IdTurnoSeleccionado"] as int?; }
            set { ViewState["IdTurnoSeleccionado"] = value; }
        }

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
                    BindPacientes();
                }
                catch (Exception ex)
                {
                    RegistrarErrorInterno("Medicos.Page_Load", ex);
                    MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos del dia. Intenta nuevamente mas tarde."), false);
                }
            }
        }

        private void BindTurnos()
        {
            gvTurnosMedicos.DataSource = bll_atencion.ObtenerTurnosDelDia(UsuarioLogueado, FechaSeleccionada);
            gvTurnosMedicos.DataBind();

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

        protected void btnProximoTurno_Click(object sender, EventArgs e)
        {
            try
            {
                BE_Turno proximoTurno = bll_atencion.ObtenerProximoTurno(UsuarioLogueado, FechaSeleccionada);

                if (proximoTurno == null)
                {
                    MostrarMensaje(lblMensajeTurnos, T("No tenes turnos reservados a partir de esta fecha."), false);
                    return;
                }

                CambiarDia(proximoTurno.FechaHora);
                MostrarMensaje(lblMensajeTurnos, T("Proximo turno: {0} a las {1} hs.", proximoTurno.FechaTexto, proximoTurno.HoraTexto), true);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnProximoTurno_Click", ex);
                MostrarMensaje(lblMensajeTurnos, T("No se pudieron cargar los turnos de ese dia. Intenta nuevamente mas tarde."), false);
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
            if (e.CommandName == "VerHistoria")
            {
                VerHistoria(e.CommandArgument);
                return;
            }

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

        private void BindPacientes()
        {
            gvPacientes.DataSource = bll_atencion.BuscarPacientes(UsuarioLogueado, txtBuscarPaciente.Text);
            gvPacientes.DataBind();
        }

        protected void btnBuscarPaciente_Click(object sender, EventArgs e)
        {
            try
            {
                BindPacientes();
                pnlHistoriaConsultada.Visible = false;
            }
            catch (InvalidOperationException ex)
            {
                MostrarMensaje(lblMensajeConsulta, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.btnBuscarPaciente_Click", ex);
                MostrarMensaje(lblMensajeConsulta, T("No se pudo realizar la busqueda. Intenta nuevamente mas tarde."), false);
            }
        }

        protected void gvPacientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "VerHistoria")
            {
                VerHistoria(e.CommandArgument);
            }
        }

        private void VerHistoria(object argumento)
        {
            try
            {
                int idPaciente = int.Parse(Convert.ToString(argumento));

                gvHistoriaConsultada.DataSource = bll_atencion.ConsultarHistoriaClinica(UsuarioLogueado, idPaciente);
                gvHistoriaConsultada.DataBind();

                BE_PacienteResumen paciente =bll_atencion.BuscarPacientes(UsuarioLogueado, null).Find(p => p.IdPaciente == idPaciente);

                lblPacienteConsultado.Text = System.Web.HttpUtility.HtmlEncode(T("Historia clinica de {0} ({1})", paciente?.NombrePaciente, paciente?.HistoriaClinica));
                pnlHistoriaConsultada.Visible = true;

                DesplazarHacia(pnlHistoriaConsultada);
            }
            catch (InvalidOperationException ex)
            {
                pnlHistoriaConsultada.Visible = false;
                MostrarMensaje(lblMensajeConsulta, ex.Message, false);
            }
            catch (Exception ex)
            {
                RegistrarErrorInterno("Medicos.VerHistoria", ex);
                pnlHistoriaConsultada.Visible = false;
                MostrarMensaje(lblMensajeConsulta, T("No se pudo consultar la historia clinica. Intenta nuevamente mas tarde."), false);
            }
        }

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
    }
}
