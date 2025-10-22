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
                    var estado = new Estado
                    {
                        IdEstado = reader.GetInt32("IdEstado"),
                        NomeEstado = reader.GetString("NomeEstado"),
                        Uf = reader.GetString("Uf"),
                        Status = reader.GetBoolean("Status"),
                        IdPais = reader.GetInt32("IdPais"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
                    };

                    lista.Add(estado);
                }

                reader.Close();

                string sqlView = "SELECT IdEstado, NomePais FROM vw_estado_pais";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var nomesPaisPorEstado = new Dictionary<int, string>();

                while (viewReader.Read())
                {
                    int idEstado = viewReader.GetInt32("IdEstado");
                    string nomePais = viewReader.GetString("NomePais");

                    nomesPaisPorEstado[idEstado] = nomePais;
                }

                viewReader.Close();

                foreach (var estado in lista)
                {
                    if (nomesPaisPorEstado.TryGetValue(estado.IdEstado, out string nomePais))
                    {
                        estado.NomePais = nomePais;
                    }
                }
            }

            return lista;
        }


        public void Inserir(Estado estado)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "INSERT INTO Estado (NomeEstado, Uf, Status, IdPais, DataCriacao) VALUES (@NomeEstado, @Uf, @Status, @IdPais, @DataCriacao)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeEstado", estado.NomeEstado.ToUpper());
                cmd.Parameters.AddWithValue("@Uf", estado.Uf);
                cmd.Parameters.AddWithValue("@Status", estado.Status);
                cmd.Parameters.AddWithValue("@IdPais", estado.IdPais);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now); 
                cmd.ExecuteNonQuery();

                estado.IdEstado = (int)cmd.LastInsertedId;

            }
        }


        public void Atualizar(Estado estado)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Estado SET NomeEstado = @NomeEstado, Uf = @Uf, Status = @Status, IdPais = @IdPais, DataEdicao = @DataEdicao WHERE IdEstado = @IdEstado";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeEstado", estado.NomeEstado.ToUpper());
                cmd.Parameters.AddWithValue("@Uf", estado.Uf);
                cmd.Parameters.AddWithValue("@Status", estado.Status);
                cmd.Parameters.AddWithValue("@IdPais", estado.IdPais);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now); 
                cmd.Parameters.AddWithValue("@IdEstado", estado.IdEstado);
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
                        Uf = reader.GetString("Uf"),
                        Status = reader.GetBoolean("Status"),
                        IdPais = reader.GetInt32("IdPais"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
                    };
                }
            }

            return null;
        }

        public bool ExisteEstadoPorNome(string nome)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                var query = "SELECT COUNT(*) FROM Estado WHERE NomeEstado = @NomeEstado";
                using (var cmd = new MySqlCommand(query, conexao))
                {
                    cmd.Parameters.AddWithValue("@NomeEstado", nome);

                    var resultado = Convert.ToInt32(cmd.ExecuteScalar());
                    return resultado > 0;
                }
            }
        }



    }
}
