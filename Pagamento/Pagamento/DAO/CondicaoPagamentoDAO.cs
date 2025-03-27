using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class CondicaoPagamentoDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<CondicaoPagamento> Listar()
        {
            List<CondicaoPagamento> lista = new List<CondicaoPagamento>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM CondicaoPagamento";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new CondicaoPagamento
                    {
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        Descricao = reader.GetString("Descricao"),
                        QuantidadeParcelas = reader.GetInt32("QuantidadeParcelas")
                    });
                }
            }

            return lista;
        }

        public int Inserir(CondicaoPagamento condicao)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO CondicaoPagamento (Descricao, QuantidadeParcelas) VALUES (@descricao, @quantidade);SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", condicao.Descricao);
                cmd.Parameters.AddWithValue("@quantidade", condicao.QuantidadeParcelas);

                condicao.IdCondPgto = Convert.ToInt32(cmd.ExecuteScalar()); 
                return condicao.IdCondPgto;
            }
        }

        public void Atualizar(CondicaoPagamento condicao)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE CondicaoPagamento SET Descricao = @descricao, QuantidadeParcelas = @quantidade WHERE IdCondPgto = @IdCondPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", condicao.Descricao);
                cmd.Parameters.AddWithValue("@quantidade", condicao.QuantidadeParcelas);
                cmd.Parameters.AddWithValue("@IdCondPgto", condicao.IdCondPgto);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM CondicaoPagamento WHERE IdCondPgto = @IdCondPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", id);
                cmd.ExecuteNonQuery();
            }
        }

        public CondicaoPagamento BuscarPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM CondicaoPagamento WHERE IdCondPgto = @IdCondPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new CondicaoPagamento
                    {
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        Descricao = reader.GetString("Descricao"),
                        QuantidadeParcelas = reader.GetInt32("QuantidadeParcelas")
                    };
                }
            }

            return null;  
        }
    }
}
