using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class MarcaDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Marca> Listar()
        {
            List<Marca> lista = new List<Marca>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Marca";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Marca
                    {
                        IdMarca = reader.GetInt32("IdMarca"),
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

        public void Inserir(Marca marca)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Marca (Descricao, Status, DataCriacao) VALUES (@Descricao, @Status, @DataCriacao)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", marca.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", marca.Status);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();

                marca.IdMarca = (int)cmd.LastInsertedId;
            }
        }

        public void Atualizar(Marca marca)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Marca SET Descricao = @Descricao, Status = @Status, DataEdicao = @DataEdicao WHERE IdMarca = @IdMarca";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", marca.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Status", marca.Status);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdMarca", marca.IdMarca);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Marca WHERE IdMarca = @IdMarca";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdMarca", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Marca BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Marca WHERE IdMarca = @IdMarca";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdMarca", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Marca
                    {
                        IdMarca = reader.GetInt32("IdMarca"),
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

        public bool MarcaDuplicada(string descricao)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                var cmd = new MySqlCommand("SELECT COUNT(*) FROM Marca WHERE Descricao = @Descricao", conexao);
                cmd.Parameters.AddWithValue("@Descricao", descricao.ToUpper());

                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

    }
}
