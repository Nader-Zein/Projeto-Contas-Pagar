using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class CidadeDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Cidade> Listar()
        {
            List<Cidade> lista = new List<Cidade>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Cidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Cidade
                    {
                        IdCidade = reader.GetInt32("IdCidade"),
                        NomeCidade = reader.GetString("NomeCidade"),
                        IdEstado = reader.GetInt32("IdEstado")
                    });
                }
            }

            return lista;
        }

        public void Inserir(Cidade cidade)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Cidade (NomeCidade, IdEstado) VALUES (@NomeCidade, @IdEstado)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeCidade", cidade.NomeCidade);
                cmd.Parameters.AddWithValue("@IdEstado", cidade.IdEstado);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Cidade WHERE IdCidade = @IdCidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCidade", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Cidade cidade)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Cidade SET NomeCidade = @NomeCidade, IdEstado = @IdEstado WHERE IdCidade = @IdCidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeCidade", cidade.NomeCidade);
                cmd.Parameters.AddWithValue("@IdEstado", cidade.IdEstado);
                cmd.Parameters.AddWithValue("@IdCidade", cidade.IdCidade);
                cmd.ExecuteNonQuery();
            }
        }

    }
}
