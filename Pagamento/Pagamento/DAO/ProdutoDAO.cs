using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ProdutoDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Produto> Listar()
        {
            var lista = new List<Produto>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Produto";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Produto
                    {
                        IdProduto = reader.GetInt32("IdProduto"),
                        Descricao = reader.GetString("Descricao"),
                        Codigo_Barras = reader.GetString("Codigo_Barras"),
                        Referencia = reader.GetString("Referencia"),
                        MarcaId = reader.GetInt32("MarcaId"),
                        UnidadeMedidaId = reader.GetInt32("UnidadeMedidaId"),
                        FornecedorId = reader.GetInt32("FornecedorId"),
                        ValorCompra = reader.GetDecimal("ValorCompra"),
                        ValorVenda = reader.GetDecimal("ValorVenda"),
                        Quantidade = reader.GetInt32("Quantidade"),
                        QuantidadeMinima = reader.GetInt32("QuantidadeMinima"),
                        PercentualLucro = reader.GetDecimal("PercentualLucro"),
                        Observacoes = reader.IsDBNull(reader.GetOrdinal("Observacoes")) ? null : reader.GetString("Observacoes"),
                        Status = reader.GetBoolean("Status"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? (DateTime?)null : reader.GetDateTime("DataEdicao")
                    });
                }
                reader.Close();

                string sqlView = "SELECT IdProduto, NomeMarca, NomeUnidade FROM vw_produto_marca_unidade";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var dadosExtras = new Dictionary<int, (string NomeMarca, string NomeUnidade)>();

                while (viewReader.Read())
                {
                    int idProduto = viewReader.GetInt32("IdProduto");
                    string nomeMarca = viewReader.GetString("NomeMarca");
                    string nomeUnidade = viewReader.GetString("NomeUnidade");

                    dadosExtras[idProduto] = (nomeMarca, nomeUnidade);
                }

                viewReader.Close();

                foreach (var produto in lista)
                {
                    if (dadosExtras.TryGetValue(produto.IdProduto, out var dados))
                    {
                        produto.NomeMarca = dados.NomeMarca;
                        produto.NomeUnidade = dados.NomeUnidade;
                    }
                }
            }

            return lista;
        }

        public void Inserir(Produto produto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Produto 
                    (Descricao, Codigo_Barras, Referencia, MarcaId, UnidadeMedidaId, FornecedorId, ValorCompra, ValorVenda, 
                    Quantidade, QuantidadeMinima, PercentualLucro, Observacoes, Status, DataCriacao) 
                    VALUES 
                    (@Descricao, @Codigo_Barras, @Referencia, @MarcaId, @UnidadeMedidaId, @FornecedorId, @ValorCompra, 
                    @ValorVenda, @Quantidade, @QuantidadeMinima, @PercentualLucro, @Observacoes, @Status, @DataCriacao)";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", produto.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Codigo_Barras", produto.Codigo_Barras);
                cmd.Parameters.AddWithValue("@Referencia", produto.Referencia.ToUpper());
                cmd.Parameters.AddWithValue("@MarcaId", produto.MarcaId);
                cmd.Parameters.AddWithValue("@UnidadeMedidaId", produto.UnidadeMedidaId);
                cmd.Parameters.AddWithValue("@FornecedorId", produto.FornecedorId);
                cmd.Parameters.AddWithValue("@ValorCompra", 0.00);
                cmd.Parameters.AddWithValue("@ValorVenda", produto.ValorVenda);
                cmd.Parameters.AddWithValue("@Quantidade", 0);
                cmd.Parameters.AddWithValue("@QuantidadeMinima", produto.QuantidadeMinima);
                cmd.Parameters.AddWithValue("@PercentualLucro", produto.PercentualLucro);
                cmd.Parameters.AddWithValue("@Observacoes", string.IsNullOrEmpty(produto.Observacoes) ? DBNull.Value : produto.Observacoes.ToUpper());
                cmd.Parameters.AddWithValue("@Status", produto.Status);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public Produto BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Produto WHERE IdProduto = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Produto
                    {
                        IdProduto = reader.GetInt32("IdProduto"),
                        Descricao = reader.GetString("Descricao"),
                        Codigo_Barras = reader.GetString("Codigo_Barras"),
                        Referencia = reader.GetString("Referencia"),
                        MarcaId = reader.GetInt32("MarcaId"),
                        UnidadeMedidaId = reader.GetInt32("UnidadeMedidaId"),
                        FornecedorId = reader.GetInt32("FornecedorId"),
                        ValorCompra = reader.GetDecimal("ValorCompra"),
                        ValorVenda = reader.GetDecimal("ValorVenda"),
                        Quantidade = reader.GetInt32("Quantidade"),
                        QuantidadeMinima = reader.GetInt32("QuantidadeMinima"),
                        PercentualLucro = reader.GetDecimal("PercentualLucro"),
                        Observacoes = reader.IsDBNull(reader.GetOrdinal("Observacoes")) ? null : reader.GetString("Observacoes"),
                        Status = reader.GetBoolean("Status"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? (DateTime?)null : reader.GetDateTime("DataEdicao")
                    };
                }
            }

            return null;
        }

        public void Atualizar(Produto produto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Produto 
                    SET Descricao = @Descricao, Codigo_Barras = @Codigo_Barras, Referencia = @Referencia, 
                        MarcaId = @MarcaId, UnidadeMedidaId = @UnidadeMedidaId, FornecedorId = @FornecedorId, ValorCompra = @ValorCompra, 
                        ValorVenda = @ValorVenda, Quantidade = @Quantidade, QuantidadeMinima = @QuantidadeMinima, 
                        PercentualLucro = @PercentualLucro, Observacoes = @Observacoes, Status = @Status, 
                        DataEdicao = @DataEdicao 
                    WHERE IdProduto = @Id";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Descricao", produto.Descricao.ToUpper());
                cmd.Parameters.AddWithValue("@Codigo_Barras", produto.Codigo_Barras);
                cmd.Parameters.AddWithValue("@Referencia", produto.Referencia.ToUpper());
                cmd.Parameters.AddWithValue("@MarcaId", produto.MarcaId);
                cmd.Parameters.AddWithValue("@UnidadeMedidaId", produto.UnidadeMedidaId);
                cmd.Parameters.AddWithValue("@FornecedorId", produto.FornecedorId);
                cmd.Parameters.AddWithValue("@ValorCompra", produto.ValorCompra);
                cmd.Parameters.AddWithValue("@ValorVenda", produto.ValorVenda);
                cmd.Parameters.AddWithValue("@Quantidade", produto.Quantidade);
                cmd.Parameters.AddWithValue("@QuantidadeMinima", produto.QuantidadeMinima);
                cmd.Parameters.AddWithValue("@PercentualLucro", produto.PercentualLucro);
                cmd.Parameters.AddWithValue("@Observacoes", string.IsNullOrEmpty(produto.Observacoes) ? DBNull.Value : produto.Observacoes.ToUpper());
                cmd.Parameters.AddWithValue("@Status", produto.Status);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", produto.IdProduto);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Produto WHERE IdProduto = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
