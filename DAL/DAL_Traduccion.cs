using BE;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class DAL_Traduccion
    {
        DAL_Conexion conex = new DAL_Conexion();

        public int? ObtenerIdClave(string pagina, string controlId)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdClave From ClaveTexto Where Pagina = @pagina And ControlId = @controlId", cn);
                cmd.Parameters.Add("@pagina", SqlDbType.VarChar, 100).Value = pagina;
                cmd.Parameters.Add("@controlId", SqlDbType.VarChar, 100).Value = controlId;

                object resultado = cmd.ExecuteScalar();

                return resultado == null ? (int?)null : Convert.ToInt32(resultado);
            }
        }

        public int RegistrarClave(string pagina, string controlId, string textoBase)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"If Not Exists (Select 1 From ClaveTexto Where Pagina = @pagina And ControlId = @controlId)
                      Insert Into ClaveTexto (Pagina, ControlId, TextoBase) Values (@pagina, @controlId, @textoBase);

                      Select IdClave From ClaveTexto Where Pagina = @pagina And ControlId = @controlId;", cn);

                cmd.Parameters.Add("@pagina", SqlDbType.VarChar, 100).Value = pagina;
                cmd.Parameters.Add("@controlId", SqlDbType.VarChar, 100).Value = controlId;
                cmd.Parameters.Add("@textoBase", SqlDbType.NVarChar, 300).Value = textoBase;

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void GuardarTraduccion(int idIdioma, int idClave, string texto)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"If Exists (Select 1 From Traduccion Where IdIdioma = @idIdioma And IdClave = @idClave)
                        Update Traduccion Set Texto = @texto Where IdIdioma = @idIdioma And IdClave = @idClave
                      Else
                        Insert Into Traduccion (IdIdioma, IdClave, Texto) Values (@idIdioma, @idClave, @texto)", cn);

                cmd.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;
                cmd.Parameters.Add("@idClave", SqlDbType.Int).Value = idClave;
                cmd.Parameters.Add("@texto", SqlDbType.NVarChar, 300).Value = texto ?? string.Empty;

                cmd.ExecuteNonQuery();
            }
        }

        public List<BE_Traduccion> ObtenerCatalogo()
        {
            List<BE_Traduccion> lista = new List<BE_Traduccion>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdClave, Pagina, ControlId, TextoBase From ClaveTexto Order By Pagina, ControlId", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Traduccion
                    {
                        IdClave = lector.GetInt32(0),
                        Pagina = lector.GetString(1),
                        ControlId = lector.GetString(2),
                        TextoBase = lector.GetString(3)
                    });
                }
            }

            return lista;
        }

        public List<BE_Traduccion> ObtenerTraduccionesParaGrilla(int idIdioma)
        {
            List<BE_Traduccion> lista = new List<BE_Traduccion>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"Select ct.IdClave, ct.Pagina, ct.ControlId, ct.TextoBase, t.Texto
                      From ClaveTexto ct
                      Left Join Traduccion t On t.IdClave = ct.IdClave And t.IdIdioma = @idIdioma
                      Order By ct.Pagina, ct.ControlId", cn);

                cmd.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(new BE_Traduccion
                    {
                        IdClave = lector.GetInt32(0),
                        Pagina = lector.GetString(1),
                        ControlId = lector.GetString(2),
                        TextoBase = lector.GetString(3),
                        Texto = lector.IsDBNull(4) ? null : lector.GetString(4)
                    });
                }
            }

            return lista;
        }

        public Dictionary<string, string> ObtenerDiccionarioTraducciones(int idIdioma)
        {
            Dictionary<string, string> diccionario = new Dictionary<string, string>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"Select ct.Pagina, ct.ControlId, IsNull(t.Texto, ct.TextoBase) As Texto
                      From ClaveTexto ct
                      Left Join Traduccion t On t.IdClave = ct.IdClave And t.IdIdioma = @idIdioma", cn);

                cmd.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    string clave = lector.GetString(0) + "." + lector.GetString(1);
                    diccionario[clave] = lector.GetString(2);
                }
            }

            return diccionario;
        }
    }
}
