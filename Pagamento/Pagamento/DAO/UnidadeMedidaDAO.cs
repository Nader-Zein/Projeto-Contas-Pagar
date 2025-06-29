using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class UnidadeMedidaDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<UnidadeMedida> Listar()
        {
            List<UnidadeMedida> lista = new List<UnidadeMedida>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM UnidadeMedida";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new UnidadeMedida
                    {
                        IdUnidadeMedida = reader.GetInt32("IdUnidadeMedida"),
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

        public void Inserir(UnidadeMedida unidade)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO UnidadeMedida (Descricao, Status, DataCriacao) VALUES (@Descricao, @Status, @DataCriacao)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", unidade.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", unidade.Status);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();

                unidade.IdUnidadeMedida = (int)cmd.LastInsertedId;
            }
        }

        public void Atualizar(UnidadeMedida unidade)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE UnidadeMedida SET Descricao = @Descricao, Status = @Status, DataEdicao = @DataEdicao WHERE IdUnidadeMedida = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", unidade.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", unidade.Status);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", unidade.IdUnidadeMedida);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM UnidadeMedida WHERE IdUnidadeMedida = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public UnidadeMedida BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM UnidadeMedida WHERE IdUnidadeMedida = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new UnidadeMedida
                    {
                        IdUnidadeMedida = reader.GetInt32("IdUnidadeMedida"),
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
    }
}
