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
                        Descricao = reader.GetString("Descricao")
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
                string sql = "INSERT INTO FormaPagamento (Descricao) VALUES (@descricao)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", forma.Descricao);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(FormaPagamento forma)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE FormaPagamento SET Descricao = @descricao WHERE IdFormaPgto = @IdFormaPgto";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@descricao", forma.Descricao);
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
                        Descricao = reader.GetString("Descricao")
                    };
                }
            }

            return null;
        }
    }
}
