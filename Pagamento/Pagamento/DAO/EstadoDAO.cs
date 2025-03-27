using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class EstadoDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Estado> Listar()
        {
            List<Estado> lista = new List<Estado>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Estado";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Estado
                    {
                        IdEstado = reader.GetInt32("IdEstado"),
                        NomeEstado = reader.GetString("NomeEstado"),
                        IdPais = reader.GetInt32("IdPais")
                    });
                }
            }

            return lista;
        }

        public void Inserir(Estado estado)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Estado (NomeEstado, IdPais) VALUES (@NomeEstado, @IdPais)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeEstado", estado.NomeEstado);
                cmd.Parameters.AddWithValue("@IdPais", estado.IdPais);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Estado estado)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Estado SET NomeEstado = @NomeEstado, IdPais = @IdPais WHERE IdEstado = @IdEstado";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdEstado", estado.IdEstado);
                cmd.Parameters.AddWithValue("@NomeEstado", estado.NomeEstado);
                cmd.Parameters.AddWithValue("@IdPais", estado.IdPais);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Estado WHERE IdEstado = @IdEstado";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdEstado", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Estado BuscarPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Estado WHERE IdEstado = @IdEstado";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdEstado", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Estado
                    {
                        IdEstado = reader.GetInt32("IdEstado"),
                        NomeEstado = reader.GetString("NomeEstado"),
                        IdPais = reader.GetInt32("IdPais")
                    };
                }
            }

            return null;
        }
    }
}
