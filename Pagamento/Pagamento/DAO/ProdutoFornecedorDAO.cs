using MySql.Data.MySqlClient;

namespace Pagamento.DAO
{
    public class ProdutoFornecedorDAO
    {
        private readonly string connectionString;

        public ProdutoFornecedorDAO(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PagamentoDB");
        }

        public void InserirOuAtualizarAssociacao(int idProduto, int idFornecedor, string? observacao)
        {
            using var conexao = new MySqlConnection(connectionString);
            conexao.Open();

            const string sql = @"
                                INSERT INTO ProdutoFornecedor (IdProduto, IdFornecedor, Observacao)
                                VALUES (@IdProduto, @IdFornecedor, @Observacao)
                                ON DUPLICATE KEY UPDATE
                                    Observacao = COALESCE(VALUES(Observacao), Observacao);  -- preserva CustoUltimaCompra / DataUltimaCompra
                                ";

            using var cmd = new MySqlCommand(sql, conexao);
            cmd.Parameters.AddWithValue("@IdProduto", idProduto);
            cmd.Parameters.AddWithValue("@IdFornecedor", idFornecedor);
            cmd.Parameters.AddWithValue("@Observacao", string.IsNullOrWhiteSpace(observacao) ? (object)DBNull.Value : observacao.ToUpper());
            cmd.ExecuteNonQuery();
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

        public void RemoverNaoSelecionados(int idProduto, IEnumerable<int> idsFornecedoresMantidos)
        {
            using var conexao = new MySqlConnection(connectionString);
            conexao.Open();

            if (idsFornecedoresMantidos == null || !idsFornecedoresMantidos.Any())
            {
                using var delAll = new MySqlCommand("DELETE FROM ProdutoFornecedor WHERE IdProduto=@IdProduto", conexao);
                delAll.Parameters.AddWithValue("@IdProduto", idProduto);
                delAll.ExecuteNonQuery();
                return;
            }

            var ids = idsFornecedoresMantidos.Distinct().ToList();
            var inParams = string.Join(",", ids.Select((_, i) => $"@f{i}"));
            var sql = $@"DELETE FROM ProdutoFornecedor 
                 WHERE IdProduto=@IdProduto AND IdFornecedor NOT IN ({inParams})";

            using var cmd = new MySqlCommand(sql, conexao);
            cmd.Parameters.AddWithValue("@IdProduto", idProduto);
            for (int i = 0; i < ids.Count; i++)
                cmd.Parameters.AddWithValue($"@f{i}", ids[i]);
            cmd.ExecuteNonQuery();
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

        public void AtualizarDadosCompra(int idProduto, int idFornecedor, decimal precoRateado, DateTime dataCompra)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE ProdutoFornecedor SET
                                CustoUltimaCompra  = @CustoUltimaCompra ,
                                DataUltimaCompra = @DataUltimaCompra
                               WHERE 
                                IdProduto = @IdProduto AND IdFornecedor = @IdFornecedor";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@CustoUltimaCompra", precoRateado);
                cmd.Parameters.AddWithValue("@DataUltimaCompra", dataCompra);
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
