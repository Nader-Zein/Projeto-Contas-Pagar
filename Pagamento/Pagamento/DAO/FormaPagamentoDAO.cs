using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class FormaPagamentoDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<FormaPagamento> Listar()
        {
            List<FormaPagamento> lista = new List<FormaPagamento>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM FormaPagamento";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new FormaPagamento
                    {
                        IdFormaPgto = reader.GetInt32("IdFormaPgto"),
                        Descricao = reader.GetString("Descricao"),
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

        public void Inserir(FormaPagamento forma)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO FormaPagamento (Descricao, Status, DataCriacao) VALUES (@descricao, @status, @dataCriacao)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", forma.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@status", forma.Status);
                cmd.Parameters.AddWithValue("@dataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();

                forma.IdFormaPgto = (int)cmd.LastInsertedId;

            }
        }

        public void Atualizar(FormaPagamento forma)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE FormaPagamento 
                               SET Descricao = @descricao, 
                                   Status = @status, 
                                   DataEdicao = @dataEdicao 
                               WHERE IdFormaPgto = @IdFormaPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", forma.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@status", forma.Status);
                cmd.Parameters.AddWithValue("@dataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdFormaPgto", forma.IdFormaPgto);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM FormaPagamento WHERE IdFormaPgto = @IdFormaPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdFormaPgto", id);
                cmd.ExecuteNonQuery();
            }
        }

        public FormaPagamento BuscarPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM FormaPagamento WHERE IdFormaPgto = @IdFormaPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdFormaPgto", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new FormaPagamento
                    {
                        IdFormaPgto = reader.GetInt32("IdFormaPgto"),
                        Descricao = reader.GetString("Descricao"),
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

        public bool ExisteForma(string descricao)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT COUNT(*) FROM FormaPagamento WHERE Descricao = @Descricao";
                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Descricao", descricao.Trim());
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

    }
}
