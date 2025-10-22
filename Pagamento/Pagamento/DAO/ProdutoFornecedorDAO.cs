using MySql.Data.MySqlClient;

namespace Pagamento.DAO
{
    public class ProdutoFornecedorDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public void Inserir(int idProduto, int idFornecedor)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO ProdutoFornecedor (IdProduto, IdFornecedor)
                               VALUES (@IdProduto, @IdFornecedor)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdProduto", idProduto);
                cmd.Parameters.AddWithValue("@IdFornecedor", idFornecedor);
                cmd.ExecuteNonQuery();
            }
        }

        public void RemoverTodos(int idProduto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM ProdutoFornecedor WHERE IdProduto = @IdProduto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdProduto", idProduto);
                cmd.ExecuteNonQuery();
            }
        }

        public List<int> ListarFornecedoresIds(int idProduto)
        {
            var lista = new List<int>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT IdFornecedor FROM ProdutoFornecedor WHERE IdProduto = @IdProduto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdProduto", idProduto);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(reader.GetInt32("IdFornecedor"));
                }
            }
            return lista;
        }

        public void AtualizarDadosCompra(int idProduto, int idFornecedor, decimal precoRateado, DateTime dataCompra, string? observacao)
        {

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE ProdutoFornecedor SET
                                PrecoUltimaCompra = @PrecoUltimaCompra,
                                DataUltimaCompra = @DataUltimaCompra,
                                Observacao = @Observacao
                               WHERE 
                                IdProduto = @IdProduto AND IdFornecedor = @IdFornecedor";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@PrecoUltimaCompra", precoRateado);
                cmd.Parameters.AddWithValue("@DataUltimaCompra", dataCompra);
                cmd.Parameters.AddWithValue("@Observacao", observacao ?? (object)DBNull.Value); 
                cmd.Parameters.AddWithValue("@IdProduto", idProduto);
                cmd.Parameters.AddWithValue("@IdFornecedor", idFornecedor);

                cmd.ExecuteNonQuery();
            }
        }


       
        public void GarantirAssociacao(int produtoId, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"INSERT IGNORE INTO ProdutoFornecedor 
                                   (IdProduto, IdFornecedor) 
                                   VALUES 
                                   (@ProdutoId, @FornecedorId)";

                    var cmd = new MySqlCommand(sql, conexao);
                    cmd.Parameters.AddWithValue("@ProdutoId", produtoId);
                    cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao garantir a associação produto-fornecedor. Detalhes: {ex.Message}");
                }
            }
        }
    }
}
