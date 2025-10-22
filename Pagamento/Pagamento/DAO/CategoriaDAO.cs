using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class CategoriaDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Categoria> Listar()
        {
            List<Categoria> lista = new List<Categoria>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Categoria";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Categoria
                    {
                        IdCategoria = reader.GetInt32("IdCategoria"),
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

        public void Inserir(Categoria categoria)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Categoria (Descricao, Status, DataCriacao) VALUES (@Descricao, @Status, @DataCriacao)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", categoria.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", categoria.Status);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();

                categoria.IdCategoria = (int)cmd.LastInsertedId;
            }
        }

        public void Atualizar(Categoria categoria)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Categoria SET Descricao = @Descricao, Status = @Status, DataEdicao = @DataEdicao WHERE IdCategoria = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", categoria.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", categoria.Status);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", categoria.IdCategoria);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Categoria WHERE IdCategoria = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Categoria BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Categoria WHERE IdCategoria = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Categoria
                    {
                        IdCategoria = reader.GetInt32("IdCategoria"),
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

        public bool ExisteCategoriao(string descricao)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT COUNT(*) FROM Categoria WHERE Descricao = @Descricao";
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
