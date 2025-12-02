using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class VendaDAO
    {
        private readonly string connectionString;

        private readonly ProdutoDAO _produtoDAO;
        private readonly ParcelaCondicaoPagamentoDAO _parcelaCondPgtoDAO;
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO;
        private readonly ContaAReceberDAO _contaAReceberDAO;

        public VendaDAO(
            IConfiguration configuration,
            ProdutoDAO produtoDAO,
            ParcelaCondicaoPagamentoDAO parcelaCondPgtoDAO,
            CondicaoPagamentoDAO condicaoPagamentoDAO,
            ContaAReceberDAO contaAReceberDAO)
        {
            connectionString = configuration.GetConnectionString("PagamentoDB");

            _produtoDAO = produtoDAO;
            _parcelaCondPgtoDAO = parcelaCondPgtoDAO;
            _condicaoPagamentoDAO = condicaoPagamentoDAO;
            _contaAReceberDAO = contaAReceberDAO;
        }
        public List<Venda> Listar()
        {
            var lista = new List<Venda>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"SELECT 
                                    v.*, 
                                    c.Nome_RazaoSocial AS NomeCliente,
                                    f.Nome_RazaoSocial AS NomeFuncionario
                                   FROM Venda v
                                   JOIN Cliente c ON v.ClienteId = c.IdCliente
                                   JOIN Funcionario f ON v.FuncionarioId = f.IdFuncionario
                                   ORDER BY v.DataEmissao DESC";

                    var cmd = new MySqlCommand(sql, conexao);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Venda
                        {
                            Modelo = reader["Modelo"].ToString(),
                            Serie = reader["Serie"].ToString(),
                            NumeroNota = Convert.ToInt32(reader["NumeroNota"]),
                            ClienteId = Convert.ToInt32(reader["ClienteId"]),
                            FuncionarioId = Convert.ToInt32(reader["FuncionarioId"]),
                            DataEmissao = Convert.ToDateTime(reader["DataEmissao"]),
                            CondicaoPagamentoId = Convert.ToInt32(reader["CondicaoPagamentoId"]),
                            Status = Convert.ToBoolean(reader["Status"]),
                            NomeCliente = reader["NomeCliente"].ToString(),
                            NomeFuncionario = reader["NomeFuncionario"].ToString(),
                            Observacoes = reader.IsDBNull(reader.GetOrdinal("Observacao")) ? null : reader.GetString("Observacao"),
                            Motivo_Cancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento"))
                                                ? null
                                                : reader.GetString("MotivoCancelamento")
                        });
                    }
                    reader.Close();

                    foreach (var venda in lista)
                    {
                        string sqlItens = @"SELECT 
                                                iv.*, 
                                                p.Descricao AS NomeProduto,
                                                um.Descricao AS NomeUnidade
                                            FROM ItemVenda iv
                                            JOIN Produto p ON iv.ProdutoId = p.IdProduto
                                            JOIN UnidadeMedida um ON p.UnidadeMedidaId = um.IdUnidadeMedida
                                            WHERE 
                                                iv.VendaModelo = @Modelo AND
                                                iv.VendaSerie = @Serie AND
                                                iv.VendaNumeroNota = @NumeroNota AND
                                                iv.VendaClienteId = @ClienteId";

                        var cmdItens = new MySqlCommand(sqlItens, conexao);
                        cmdItens.Parameters.AddWithValue("@Modelo", venda.Modelo);
                        cmdItens.Parameters.AddWithValue("@Serie", venda.Serie);
                        cmdItens.Parameters.AddWithValue("@NumeroNota", venda.NumeroNota);
                        cmdItens.Parameters.AddWithValue("@ClienteId", venda.ClienteId);

                        var readerItens = cmdItens.ExecuteReader();
                        while (readerItens.Read())
                        {
                            venda.Itens.Add(new ItemVenda
                            {
                                ProdutoId = Convert.ToInt32(readerItens["ProdutoId"]),
                                Quantidade = Convert.ToInt32(readerItens["Quantidade"]),
                                ValorUnitario = Convert.ToDecimal(readerItens["ValorUnitario"]),
                                CustoUnitario = Convert.ToDecimal(readerItens["CustoUnitario"]),    
                                NomeProduto = readerItens["NomeProduto"].ToString(),
                                NomeUnidade = readerItens["NomeUnidade"].ToString()
                            });
                        }
                        readerItens.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao listar vendas: {ex.Message}");
                }
            }
            return lista;
        }

        public void Inserir(Venda venda)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();

                    string sqlVenda = @"INSERT INTO Venda 
                                        (Modelo, Serie, NumeroNota, ClienteId, FuncionarioId, DataEmissao, CondicaoPagamentoId, Status, Observacao, DataCriacao) 
                                        VALUES 
                                        (@Modelo, @Serie, @NumeroNota, @ClienteId, @FuncionarioId, @DataEmissao, @CondicaoPagamentoId, @Status, @Observacao, @DataCriacao)";

                    MySqlCommand cmdVenda = new MySqlCommand(sqlVenda, conexao);
                    cmdVenda.Parameters.AddWithValue("@Modelo", venda.Modelo);
                    cmdVenda.Parameters.AddWithValue("@Serie", venda.Serie);
                    cmdVenda.Parameters.AddWithValue("@NumeroNota", venda.NumeroNota);
                    cmdVenda.Parameters.AddWithValue("@ClienteId", venda.ClienteId);
                    cmdVenda.Parameters.AddWithValue("@FuncionarioId", venda.FuncionarioId);
                    cmdVenda.Parameters.AddWithValue("@DataEmissao", venda.DataEmissao);
                    cmdVenda.Parameters.AddWithValue("@CondicaoPagamentoId", venda.CondicaoPagamentoId);
                    cmdVenda.Parameters.AddWithValue("@Status", venda.Status);
                    cmdVenda.Parameters.AddWithValue("@Observacao", venda.Observacoes ?? (object)DBNull.Value);
                    cmdVenda.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                    cmdVenda.ExecuteNonQuery();

                    foreach (var item in venda.Itens)
                    {
                        Produto produtoAtual = _produtoDAO.BuscarPorId(item.ProdutoId);
                        if (produtoAtual == null) throw new Exception($"Produto ID {item.ProdutoId} não encontrado.");

                        decimal custoMedioSnapshot = produtoAtual.CustoMedio;

                        string sqlItem = @"INSERT INTO ItemVenda 
                                           (VendaModelo, VendaSerie, VendaNumeroNota, VendaClienteId, ProdutoId, Quantidade, ValorUnitario, CustoUnitario) 
                                           VALUES 
                                           (@Modelo, @Serie, @NumeroNota, @ClienteId, @ProdutoId, @Qtd, @Valor, @Custo)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conexao);
                        cmdItem.Parameters.AddWithValue("@Modelo", venda.Modelo);
                        cmdItem.Parameters.AddWithValue("@Serie", venda.Serie);
                        cmdItem.Parameters.AddWithValue("@NumeroNota", venda.NumeroNota);
                        cmdItem.Parameters.AddWithValue("@ClienteId", venda.ClienteId);
                        cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@Qtd", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@Valor", item.ValorUnitario);
                        cmdItem.Parameters.AddWithValue("@Custo", custoMedioSnapshot);      
                        cmdItem.ExecuteNonQuery();

                        int novaQtd = produtoAtual.Quantidade - item.Quantidade;

                        _produtoDAO.AtualizarEstoqueECusto(
                            item.ProdutoId,
                            novaQtd,
                            produtoAtual.CustoMedio,    
                            produtoAtual.CustoUltimaCompra ?? 0   
                        );
                    }

                    var condicao = _condicaoPagamentoDAO.BuscarPorId(venda.CondicaoPagamentoId);
                    var parcelasDefinidas = _parcelaCondPgtoDAO.ListarPorCondicaoPagamento(venda.CondicaoPagamentoId);

                    decimal totalVenda = venda.TotalNota;       

                    foreach (var parcelaDef in parcelasDefinidas)
                    {
                        decimal valorParcela = Math.Round(totalVenda * (parcelaDef.ValorPercentual / 100), 2);
                        DateTime dataVencimento = venda.DataEmissao.AddDays(parcelaDef.DiasAposVenda);

                        var contaAReceber = new ContaAReceber
                        {
                            Modelo = venda.Modelo,
                            Serie = venda.Serie,
                            NumeroNota = venda.NumeroNota,
                            ClienteId = venda.ClienteId,
                            NumeroParcela = parcelaDef.NumeroParcela,
                            ValorParcela = valorParcela,
                            DataVencimento = dataVencimento,
                            DataEmissao = venda.DataEmissao,
                            Status = true,
                            Situacao = "A RECEBER",
                            Juros = condicao.Juros,
                            Multa = condicao.Multa,
                            Desconto = condicao.Desconto,
                            IdFormaPgto = parcelaDef.IdFormaPgto
                        };

                        _contaAReceberDAO.Inserir(contaAReceber);
                    }

                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao realizar a venda: " + ex.Message);
                }
            }
        }

        public void Cancelar(string modelo, string serie, int numeroNota, int clienteId, string motivo)
        {
            if (_contaAReceberDAO.VerificarParcelasPagas(modelo, serie, numeroNota, clienteId))
            {
                throw new Exception("Não é possível cancelar: Existem parcelas recebidas.");
            }

            var venda = Listar().FirstOrDefault(v => v.Modelo == modelo && v.Serie == serie && v.NumeroNota == numeroNota && v.ClienteId == clienteId);
            if (venda == null) throw new Exception("Venda não encontrada.");

            foreach (var item in venda.Itens)
            {
                var produto = _produtoDAO.BuscarPorId(item.ProdutoId);
                if (produto != null)
                {
                    int novaQtd = produto.Quantidade + item.Quantidade;

                    _produtoDAO.AtualizarEstoqueECusto(item.ProdutoId, novaQtd, produto.CustoMedio, produto.CustoUltimaCompra ?? 0);
                }
            }

            _contaAReceberDAO.CancelarPorVenda(modelo, serie, numeroNota, clienteId, motivo);

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "UPDATE Venda SET Status = false, MotivoCancelamento = @Motivo WHERE Modelo = @Mod AND Serie = @Ser AND NumeroNota = @Num AND ClienteId = @Cli";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Motivo", motivo);
                cmd.Parameters.AddWithValue("@Mod", modelo);
                cmd.Parameters.AddWithValue("@Ser", serie);
                cmd.Parameters.AddWithValue("@Num", numeroNota);
                cmd.Parameters.AddWithValue("@Cli", clienteId);
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteChaveComposta(string modelo, string serie, int numeroNota, int clienteId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT COUNT(*) FROM Venda WHERE Modelo=@M AND Serie=@S AND NumeroNota=@N AND ClienteId=@C";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@M", modelo);
                cmd.Parameters.AddWithValue("@S", serie);
                cmd.Parameters.AddWithValue("@N", numeroNota);
                cmd.Parameters.AddWithValue("@C", clienteId);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool ExisteVenda(string modelo, string serie, int numeroNota, int clienteId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"SELECT COUNT(*) FROM Venda 
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId";

                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@Modelo", modelo);
                    cmd.Parameters.AddWithValue("@Serie", serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                    cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
