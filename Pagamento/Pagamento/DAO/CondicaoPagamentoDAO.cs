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
                        QuantidadeParcelas = reader.GetInt32("QuantidadeParcelas"),
                        Juros = reader.GetDecimal("Juros"),
                        Multa = reader.GetDecimal("Multa"),
                        Desconto = reader.GetDecimal("Desconto"),
                        Status = reader.GetBoolean("Status"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
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
                string sql = @"INSERT INTO CondicaoPagamento 
                               (Descricao, QuantidadeParcelas, Juros, Multa, Desconto, Status, DataCriacao) 
                               VALUES (@descricao, @quantidade, @juros, @multa, @desconto, @status, @dataCriacao);
                               SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", condicao.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@quantidade", condicao.QuantidadeParcelas);
                cmd.Parameters.AddWithValue("@juros", condicao.Juros);
                cmd.Parameters.AddWithValue("@multa", condicao.Multa);
                cmd.Parameters.AddWithValue("@desconto", condicao.Desconto);
                cmd.Parameters.AddWithValue("@status", condicao.Status);
                cmd.Parameters.AddWithValue("@dataCriacao", DateTime.Now);

                condicao.IdCondPgto = Convert.ToInt32(cmd.ExecuteScalar());
                return condicao.IdCondPgto;
            }
        }

        public void Atualizar(CondicaoPagamento condicao)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE CondicaoPagamento 
                               SET Descricao = @descricao, 
                                   QuantidadeParcelas = @quantidade,
                                   Juros = @juros,
                                   Multa = @multa,
                                   Desconto = @desconto,
                                   Status = @status, 
                                   DataEdicao = @dataEdicao 
                               WHERE IdCondPgto = @IdCondPgto";

                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", condicao.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@quantidade", condicao.QuantidadeParcelas);
                cmd.Parameters.AddWithValue("@juros", condicao.Juros);
                cmd.Parameters.AddWithValue("@multa", condicao.Multa);
                cmd.Parameters.AddWithValue("@desconto", condicao.Desconto);
                cmd.Parameters.AddWithValue("@status", condicao.Status);
                cmd.Parameters.AddWithValue("@dataEdicao", DateTime.Now);
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
                        QuantidadeParcelas = reader.GetInt32("QuantidadeParcelas"),
                        Juros = reader.GetDecimal("Juros"),
                        Multa = reader.GetDecimal("Multa"),
                        Desconto = reader.GetDecimal("Desconto"),
                        Status = reader.GetBoolean("Status"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
                    };
                }
            }

            return null;
        }
    }
}
