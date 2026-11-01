using BE;
using DAL;
using SERVICIOS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class BLL_Receta
    {
        public const int DiasVigencia = 30;

        DAL_AtencionMedica dal_atencion = new DAL_AtencionMedica();
        DAL_Paciente dal_paciente = new DAL_Paciente();

        public List<BE_Receta> ObtenerRecetasPaciente(BE_Usuario usuarioLogueado)
        {
            try
            {
                if (!dal_paciente.EsUsuarioPaciente(usuarioLogueado.IdUsuario))
                {
                    throw new InvalidOperationException(TextoLocalizado.T("Tu usuario no tiene el rol PACIENTE."));
                }

                List<BE_Receta> recetas = dal_atencion.ObtenerRecetasPaciente(usuarioLogueado.IdUsuario);

                foreach (BE_Receta receta in recetas)
                {
                    receta.Vigente = receta.FechaEmision.Date.AddDays(DiasVigencia) >= DateTime.Today;
                }

                return recetas;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener las recetas: {ex.Message}", ex);
            }
        }

        public BE_Receta ObtenerRecetaPaciente(BE_Usuario usuarioLogueado, int idReceta)
        {
            BE_Receta receta = ObtenerRecetasPaciente(usuarioLogueado).FirstOrDefault(r => r.IdReceta == idReceta);

            if (receta == null)
            {
                throw new InvalidOperationException(TextoLocalizado.T("La receta seleccionada ya no existe."));
            }

            return receta;
        }

        public byte[] GenerarPdfReceta(BE_Usuario usuarioLogueado, int idReceta)
        {
            BE_Receta receta = ObtenerRecetaPaciente(usuarioLogueado, idReceta);
            BE_Paciente paciente = dal_paciente.ObtenerPaciente(usuarioLogueado.IdUsuario);

            DocumentoPdf documento = new DocumentoPdf();

            documento.AgregarTexto(TextoLocalizado.T("Receta medica"), 20, true);
            documento.AgregarSeparador();

            AgregarDato(documento, "Paciente", usuarioLogueado.NombreApellido);

            if (paciente != null)
            {
                AgregarDato(documento, "Historia clinica", paciente.NumeroHistoriaClinica.ToString());
            }

            AgregarDato(documento, "Medico", receta.NombreMedico);
            AgregarDato(documento, "Especialidad", receta.EspecialidadesTexto);
            AgregarDato(documento, "Fecha de emision", receta.FechaEmision.ToString("dd/MM/yyyy"));
            AgregarDato(documento, "Valida hasta", receta.FechaEmision.AddDays(DiasVigencia).ToString("dd/MM/yyyy"));
            AgregarDato(documento, "Estado", receta.EstadoTexto);
            documento.AgregarSeparador();

            documento.AgregarTexto(TextoLocalizado.T("Medicamento"), 13, true, 8);
            documento.AgregarTexto(receta.Medicamento, 12, false, 2);

            documento.AgregarTexto(TextoLocalizado.T("Indicaciones"), 13, true, 14);
            documento.AgregarTexto(string.IsNullOrWhiteSpace(receta.Observaciones) ? TextoLocalizado.T("Sin indicaciones.") : receta.Observaciones, 12, false, 2);

            documento.AgregarSeparador(24);
            documento.AgregarTexto(TextoLocalizado.T("Documento generado por Gestion Medica el {0:dd/MM/yyyy HH:mm} hs.", DateTime.Now), 9, false, 4);

            return documento.Generar();
        }

        private static void AgregarDato(DocumentoPdf documento, string etiqueta, string valor)
        {
            documento.AgregarTexto(TextoLocalizado.T(etiqueta) + ": " + valor, 11, false, 3);
        }
    }
}
