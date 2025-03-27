using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class PaisDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Pais> Listar()
        {
            List<Pais> lista = new List<Pais>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Pais";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Pais
                    {
                        IdPais = reader.GetInt32("IdPais"),
                        NomePais = reader.GetString("NomePais")
                    });
                }
            }

            return lista;
        }

        public void Inserir(Pais pais)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Pais (NomePais) VALUES (@NomePais)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomePais", pais.NomePais);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Pais pais)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Pais SET NomePais = @NomePais WHERE IdPais = @IdPais";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdPais", pais.IdPais);
                cmd.Parameters.AddWithValue("@NomePais", pais.NomePais);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Pais WHERE IdPais = @IdPais";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdPais", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Pais BuscarPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Pais WHERE IdPais = @IdPais";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdPais", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Pais
                    {
                        IdPais = reader.GetInt32("IdPais"),
                        NomePais = reader.GetString("NomePais")
                    };
                }
            }

            return null;
        }
    }
}
