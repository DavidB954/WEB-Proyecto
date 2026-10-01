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
    public class DAL_Idioma
    {
        DAL_Conexion conex = new DAL_Conexion();

        public List<BE_Idioma> ObtenerIdiomas()
        {
            List<BE_Idioma> lista = new List<BE_Idioma>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdIdioma, Codigo, Nombre, Activo, PorDefecto From Idioma Order By Nombre", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(MapearIdioma(lector));
                }
            }

            return lista;
        }

        public List<BE_Idioma> ObtenerIdiomasActivos()
        {
            List<BE_Idioma> lista = new List<BE_Idioma>();

            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdIdioma, Codigo, Nombre, Activo, PorDefecto From Idioma Where Activo = 1 Order By Nombre", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                while (lector.Read())
                {
                    lista.Add(MapearIdioma(lector));
                }
            }

            return lista;
        }

        public BE_Idioma ObtenerIdiomaPorId(int idIdioma)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select IdIdioma, Codigo, Nombre, Activo, PorDefecto From Idioma Where IdIdioma = @idIdioma", cn);
                cmd.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;

                SqlDataReader lector = cmd.ExecuteReader();

                if (!lector.Read())
                {
                    return null;
                }

                return MapearIdioma(lector);
            }
        }

        public BE_Idioma ObtenerIdiomaPorDefecto()
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select Top 1 IdIdioma, Codigo, Nombre, Activo, PorDefecto From Idioma Where PorDefecto = 1", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                if (lector.Read())
                {
                    return MapearIdioma(lector);
                }
            }

            // Nadie quedo marcado como predeterminado (tabla recien creada, o el
            // idioma que lo era se borro/desactivo). Nos autocorregimos: tomamos el
            // primer idioma existente y lo dejamos marcado como predeterminado para
            // que la proxima vez ya este resuelto y no vuelva a fallar.
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    @"Update Idioma Set PorDefecto = 1
                      Where IdIdioma = (Select Top 1 IdIdioma From Idioma Order By IdIdioma);

                      Select Top 1 IdIdioma, Codigo, Nombre, Activo, PorDefecto From Idioma Where PorDefecto = 1;", cn);

                SqlDataReader lector = cmd.ExecuteReader();

                return lector.Read() ? MapearIdioma(lector) : null;
            }
        }

        public bool ExisteCodigo(string codigo)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Select Count(*) From Idioma Where Codigo = @codigo", cn);
                cmd.Parameters.Add("@codigo", SqlDbType.VarChar, 10).Value = codigo;

                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        public void AgregarIdioma(BE_Idioma idioma)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand(
                    "Insert Into Idioma (Codigo, Nombre, Activo, PorDefecto) Values (@codigo, @nombre, @activo, 0); Select Scope_Identity();", cn);

                cmd.Parameters.Add("@codigo", SqlDbType.VarChar, 10).Value = idioma.Codigo;
                cmd.Parameters.Add("@nombre", SqlDbType.VarChar, 50).Value = idioma.Nombre;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = idioma.Activo;

                idioma.IdIdioma = Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public void EliminarIdioma(int idIdioma)
        {
            using (SqlConnection cn = conex.ObtenerConexion())
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Delete From Idioma Where IdIdioma = @idIdioma", cn);
                cmd.Parameters.Add("@idIdioma", SqlDbType.Int).Value = idIdioma;

                cmd.ExecuteNonQuery();
            }
        }

        private BE_Idioma MapearIdioma(SqlDataReader lector)
        {
            return new BE_Idioma
            {
                IdIdioma = lector.GetInt32(0),
                Codigo = lector.GetString(1),
                Nombre = lector.GetString(2),
                Activo = lector.GetBoolean(3),
                PorDefecto = lector.GetBoolean(4)
            };
        }
    }
}
