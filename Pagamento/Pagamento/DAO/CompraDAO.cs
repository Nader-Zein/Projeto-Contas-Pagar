using MySql.Data.MySqlClient;
using Pagamento.Models;
using System;
using System.Collections.Generic;

namespace Pagamento.DAO
{
    public class CompraDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        private readonly ParcelaCondicaoPagamentoDAO _parcelaCondPgtoDAO = new ParcelaCondicaoPagamentoDAO();
        private readonly ContaAPagarDAO _contaAPagarDAO = new ContaAPagarDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly ProdutoFornecedorDAO _produtoFornecedorDAO = new ProdutoFornecedorDAO();
        public List<Compra> Listar()
        {
            var listaDeCompras = new List<Compra>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();

                    string sqlPrincipal = @"SELECT 
                                        c.*, 
                                        f.Nome_RazaoSocial AS NomeFornecedor 
                                    FROM Compra c
                                    JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor
                                    ORDER BY c.DataEmissao DESC";

                    var cmdPrincipal = new MySqlCommand(sqlPrincipal, conexao);
                    var readerPrincipal = cmdPrincipal.ExecuteReader();

                    while (readerPrincipal.Read())
                    {
                        var compra = new Compra
                        {
                            Modelo = readerPrincipal["Modelo"].ToString(),
                            Serie = readerPrincipal["Serie"].ToString(),
                            NumeroNota = Convert.ToInt32(readerPrincipal["NumeroNota"]),
                            FornecedorId = Convert.ToInt32(readerPrincipal["FornecedorId"]),
                            DataEmissao = Convert.ToDateTime(readerPrincipal["DataEmissao"]),
                            DataChegada = Convert.ToDateTime(readerPrincipal["DataChegada"]),
                            CondicaoPagamentoId = Convert.ToInt32(readerPrincipal["CondicaoPagamentoId"]),
                            Frete = Convert.ToDecimal(readerPrincipal["Frete"]),
                            Seguro = Convert.ToDecimal(readerPrincipal["Seguro"]),
                            Despesas = Convert.ToDecimal(readerPrincipal["Despesas"]),
                            Status = Convert.ToBoolean(readerPrincipal["Status"]),
                            NomeFornecedor = readerPrincipal["NomeFornecedor"].ToString(),
                            Motivo_Cancelamento = readerPrincipal.IsDBNull(readerPrincipal.GetOrdinal("MotivoCancelamento"))
                                            ? null
                                            : readerPrincipal["MotivoCancelamento"].ToString()
                        };
                        listaDeCompras.Add(compra);
                    }
                    readerPrincipal.Close();         

                    foreach (var compra in listaDeCompras)
                    {
                        string sqlItens = @"SELECT 
                                        ic.*, 
                                        p.Descricao AS NomeProduto, 
                                        um.Descricao AS NomeUnidade,
                                        ic.CustoUnitarioReal
                                    FROM ItemCompra ic
                                    JOIN Produto p ON ic.ProdutoId = p.IdProduto
                                    JOIN UnidadeMedida um ON p.UnidadeMedidaId = um.IdUnidadeMedida
                                    WHERE 
                                        ic.CompraModelo = @Modelo AND
                                        ic.CompraSerie = @Serie AND
                                        ic.CompraNumeroNota = @NumeroNota AND
                                        ic.CompraFornecedorId = @FornecedorId";

                        var cmdItens = new MySqlCommand(sqlItens, conexao);
                        cmdItens.Parameters.AddWithValue("@Modelo", compra.Modelo);
                        cmdItens.Parameters.AddWithValue("@Serie", compra.Serie);
                        cmdItens.Parameters.AddWithValue("@NumeroNota", compra.NumeroNota);
                        cmdItens.Parameters.AddWithValue("@FornecedorId", compra.FornecedorId);

                        var readerItens = cmdItens.ExecuteReader();
                        while (readerItens.Read())
                        {
                            var item = new ItemCompra
                            {
                                ProdutoId = Convert.ToInt32(readerItens["ProdutoId"]),
                                Quantidade = Convert.ToInt32(readerItens["Quantidade"]),
                                ValorUnitario = Convert.ToDecimal(readerItens["ValorUnitario"]),
                                NomeProduto = readerItens["NomeProduto"].ToString(),
                                NomeUnidade = readerItens["NomeUnidade"].ToString(),
                                CustoUnitarioReal = Convert.ToDecimal(readerItens["CustoUnitarioReal"])
                            };
                            compra.Itens.Add(item);
                        }
                        readerItens.Close();      
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao listar as compras. Detalhes: {ex.Message}");
                }
            }
            return listaDeCompras;
        }

        public void Inserir(Compra compra)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();

                    var custosRateados = RatearCustosAdicionais(compra);

                    string sqlCompra = @"INSERT INTO Compra 
                                         (Modelo, Serie, NumeroNota, FornecedorId, DataEmissao, DataChegada, CondicaoPagamentoId, Frete, Seguro, Despesas, Status,DataCriacao) 
                                         VALUES 
                                         (@Modelo, @Serie, @NumeroNota, @FornecedorId, @DataEmissao, @DataChegada, @CondicaoPagamentoId, @Frete, @Seguro, @Despesas, @Status,@DataCriacao)";

                    MySqlCommand cmdCompra = new MySqlCommand(sqlCompra, conexao);
                    cmdCompra.Parameters.AddWithValue("@Modelo", compra.Modelo);
                    cmdCompra.Parameters.AddWithValue("@Serie", compra.Serie);
                    cmdCompra.Parameters.AddWithValue("@NumeroNota", compra.NumeroNota);
                    cmdCompra.Parameters.AddWithValue("@FornecedorId", compra.FornecedorId);
                    cmdCompra.Parameters.AddWithValue("@DataEmissao", compra.DataEmissao);
                    cmdCompra.Parameters.AddWithValue("@DataChegada", compra.DataChegada);
                    cmdCompra.Parameters.AddWithValue("@CondicaoPagamentoId", compra.CondicaoPagamentoId);
                    cmdCompra.Parameters.AddWithValue("@Frete", compra.Frete);
                    cmdCompra.Parameters.AddWithValue("@Seguro", compra.Seguro);
                    cmdCompra.Parameters.AddWithValue("@Despesas", compra.Despesas);
                    cmdCompra.Parameters.AddWithValue("@Status", compra.Status);
                    cmdCompra.Parameters.AddWithValue("@DataCriacao", DateTime.Now);

                    cmdCompra.ExecuteNonQuery();

                    foreach (var item in compra.Itens)
                    {

                        decimal custoRealRateado = custosRateados.TryGetValue(item.ProdutoId, out var v)
                                                                                ? v : item.ValorUnitario;

                        decimal custoAdicUnit = Math.Round(
                            custoRealRateado - item.ValorUnitario, 2, MidpointRounding.AwayFromZero);
                        decimal custoRealUnit = Math.Round(
                            custoRealRateado, 2, MidpointRounding.AwayFromZero);

                        string sqlItem = @"INSERT INTO ItemCompra 
                                           (CompraModelo, CompraSerie, CompraNumeroNota, CompraFornecedorId, ProdutoId, Quantidade, ValorUnitario, CustoAdicionalUnitario, CustoUnitarioReal) 
                                           VALUES 
                                           (@CompraModelo, @CompraSerie, @CompraNumeroNota, @CompraFornecedorId, @ProdutoId, @Quantidade, @ValorUnitario, @CustoAdicionalUnitario, @CustoUnitarioReal)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conexao);
                        cmdItem.Parameters.AddWithValue("@CompraModelo", compra.Modelo);
                        cmdItem.Parameters.AddWithValue("@CompraSerie", compra.Serie);
                        cmdItem.Parameters.AddWithValue("@CompraNumeroNota", compra.NumeroNota);
                        cmdItem.Parameters.AddWithValue("@CompraFornecedorId", compra.FornecedorId);
                        cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@ValorUnitario", item.ValorUnitario);
                        cmdItem.Parameters.AddWithValue("@CustoAdicionalUnitario", Math.Round(custoRealRateado - item.ValorUnitario, 2));
                        cmdItem.Parameters.AddWithValue("@CustoUnitarioReal", Math.Round(custoRealRateado, 2));
                        cmdItem.ExecuteNonQuery();     
                    }

                    var produtoDAO = new ProdutoDAO();           
                    var produtoFornecedorDAO = new ProdutoFornecedorDAO();

                    foreach (var item in compra.Itens)
                    {
                        if (custosRateados.TryGetValue(item.ProdutoId, out decimal custoRealRateado))
                        {



                            produtoFornecedorDAO.GarantirAssociacao(item.ProdutoId, compra.FornecedorId);


                            produtoFornecedorDAO.AtualizarDadosCompra(item.ProdutoId, compra.FornecedorId, custoRealRateado, compra.DataEmissao);

                            Produto produtoAtual = produtoDAO.BuscarPorId(item.ProdutoId);
                            if (produtoAtual != null)
                            {
                                int qtdAntiga = produtoAtual.Quantidade;
                                decimal custoMedioAntigo = produtoAtual.CustoMedio;
                                int qtdNova = item.Quantidade;
                                int novaQtdTotal = qtdAntiga + qtdNova;
                                decimal novoCustoMedio = (qtdAntiga > 0)
                                                        ? ((qtdAntiga * custoMedioAntigo) + (qtdNova * custoRealRateado)) / (qtdAntiga + qtdNova)
                                                        : custoRealRateado;
                                if (novaQtdTotal > 0 && qtdAntiga > 0)
                                {
                                    novoCustoMedio = ((qtdAntiga * custoMedioAntigo) + (qtdNova * custoRealRateado)) / novaQtdTotal;
                                }
                                produtoDAO.AtualizarEstoqueECusto(item.ProdutoId, novaQtdTotal, novoCustoMedio, custoRealRateado);
                            }
                        }
                    }



                    var condicao = _condicaoPagamentoDAO.BuscarPorId(compra.CondicaoPagamentoId);
                    if (condicao == null)
                    {
                        throw new Exception($"A Condição de Pagamento com ID {compra.CondicaoPagamentoId} não foi encontrada.");
                    }


                    var parcelasDefinidas = _parcelaCondPgtoDAO.ListarPorCondicaoPagamento(compra.CondicaoPagamentoId);

                    decimal totalNota = compra.TotalNota;

                    foreach (var parcelaDef in parcelasDefinidas)
                    {
                        decimal valorParcela = totalNota * (parcelaDef.ValorPercentual / 100);

                        DateTime dataVencimento = compra.DataEmissao.AddDays(parcelaDef.DiasAposVenda);

                        var contaAPagar = new ContaAPagar
                        {
                            Modelo = compra.Modelo,
                            Serie = compra.Serie,
                            NumeroNota = compra.NumeroNota,
                            FornecedorId = compra.FornecedorId,

                            NumeroParcela = parcelaDef.NumeroParcela,
                            ValorParcela = Math.Round(valorParcela, 2),      
                            DataVencimento = dataVencimento,
                            DataEmissao = compra.DataEmissao,       

                            Status = true,          
                            Situacao = "A PAGAR",     


                            Juros = condicao.Juros,
                            Multa = condicao.Multa,
                            Desconto = condicao.Desconto,
                            IdFormaPgto = parcelaDef.IdFormaPgto
                        };

                        _contaAPagarDAO.Inserir(contaAPagar);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Ocorreu um erro durante a inserção da compra. O processo foi interrompido, mas alguns dados podem ter sido salvos. Detalhes: " + ex.Message);
                }
            }
        }

        private Dictionary<int, decimal> RatearCustosAdicionais(Compra compra)
        {
            decimal valorTotalItens = compra.Itens.Sum(item => item.ValorUnitario * item.Quantidade);

            if (valorTotalItens == 0) return new Dictionary<int, decimal>();

            decimal totalCustosAdicionais = compra.Frete + compra.Seguro + compra.Despesas;
            var custosReais = new Dictionary<int, decimal>();

            foreach (var item in compra.Itens)
            {
                decimal valorTotalDoItem = item.ValorUnitario * item.Quantidade;
                decimal proporcao = valorTotalDoItem / valorTotalItens;
                decimal custoAdicionalDoItem = totalCustosAdicionais * proporcao;
                decimal custoAdicionalUnitario = (item.Quantidade > 0) ? custoAdicionalDoItem / item.Quantidade : 0;
                decimal custoRealUnitario = item.ValorUnitario + custoAdicionalUnitario;
                custosReais.Add(item.ProdutoId, custoRealUnitario);
            }

            return custosReais;
        }

        public bool ExisteChaveComposta(string modelo, string serie, string numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                                SELECT COUNT(*) 
                                FROM Compra 
                                WHERE Modelo = @Modelo 
                                  AND Serie = @Serie 
                                  AND NumeroNota = @NumeroNota 
                                  AND FornecedorId = @FornecedorId";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo ?? (object)DBNull.Value);      
                cmd.Parameters.AddWithValue("@Serie", serie ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                var count = Convert.ToInt64(cmd.ExecuteScalar());

                return count > 0;         
            }
        }


        public Compra BuscarDetalhesPorChaveComposta(string modelo, string serie, string numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                string sql = @"
                            SELECT 
                                c.DataEmissao, c.DataChegada, c.Modelo, c.Serie, c.NumeroNota, 
                                c.FornecedorId, 
                                f.Nome_RazaoSocial AS FornecedorNome, -- Pega o nome da tabela Fornecedor
                                c.Observacao,
                                c.Status
                                -- Adicione outras colunas da tabela Compra se precisar exibi-las
                                -- Ex: c.Status, c.DataCriacao
                            FROM Compra c
                            JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor -- JOIN CORRETO com Fornecedor
                            WHERE c.Modelo = @Modelo 
                              AND c.Serie = @Serie 
                              AND c.NumeroNota = @NumeroNota 
                              AND c.FornecedorId = @FornecedorId
                            LIMIT 1";        
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Serie", serie ?? (object)DBNull.Value);

                if (int.TryParse(numeroNota, out int numeroNotaInt))
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNotaInt);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", DBNull.Value);
                }
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                try         
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int numeroNotaString = reader.GetInt32("NumeroNota");            

                        string observacaoLida = reader.IsDBNull(reader.GetOrdinal("Observacao")) ? null : reader.GetString("Observacao");

                        return new Compra
                        {
                            Modelo = reader.GetString("Modelo"),
                            Serie = reader.GetString("Serie"),
                            NumeroNota = numeroNotaString,           
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataChegada = reader.GetDateTime("DataChegada"),
                            NomeFornecedor = reader.GetString("FornecedorNome"),         
                            Observacoes = observacaoLida,         
                            Status = reader.GetBoolean("Status")              
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro em BuscarDetalhesPorChaveComposta - DB Query/Read: {ex.ToString()}");   
                    throw;             
                }
            }
            return null;             
        }

        public void Cancelar(string modelo, string serie, int numeroNota, int fornecedorId, string motivoCancelamento)
        {
            if (_contaAPagarDAO.VerificarParcelasPagas(modelo, serie, numeroNota, fornecedorId))
            {
                throw new Exception("Não é possível cancelar a compra. Existem parcelas que já foram pagas.");
            }

            var compra = this.Listar()
                .FirstOrDefault(c =>
                    c.Modelo == modelo &&
                    c.Serie == serie &&
                    c.NumeroNota == numeroNota &&
                    c.FornecedorId == fornecedorId);

            if (compra == null || compra.Itens == null || !compra.Itens.Any())
            {
                throw new Exception("Compra não encontrada ou sem itens para reverter.");
            }

            foreach (var itemCancelado in compra.Itens)
            {
                Produto produtoAtual = _produtoDAO.BuscarPorId(itemCancelado.ProdutoId);
                if (produtoAtual == null) continue;

                int qtdAtual = produtoAtual.Quantidade;
                decimal custoMedioAtual = produtoAtual.CustoMedio;

                int qtdCancelada = itemCancelado.Quantidade;
                decimal custoRealCancelado = itemCancelado.CustoUnitarioReal;      

                int novaQtdTotal = qtdAtual - qtdCancelada;
                if (novaQtdTotal < 0)
                {
                    throw new Exception($"Não é possível cancelar: O produto '{produtoAtual.Descricao}' ficaria com estoque negativo ({novaQtdTotal}).");
                }

                decimal novoCustoMedio = 0;
                if (novaQtdTotal > 0)
                {
                    decimal valorTotalEstoqueAtual = qtdAtual * custoMedioAtual;
                    decimal valorTotalEstoqueCancelado = qtdCancelada * custoRealCancelado;

                    decimal novoValorTotalEstoque = valorTotalEstoqueAtual - valorTotalEstoqueCancelado;

                    novoCustoMedio = (novoValorTotalEstoque > 0) ? (novoValorTotalEstoque / novaQtdTotal) : 0;
                }
                decimal novoCustoUltimaCompra = (novaQtdTotal > 0) ? novoCustoMedio : 0;

                _produtoDAO.AtualizarEstoqueECusto(
                    itemCancelado.ProdutoId,
                    novaQtdTotal,
                    Math.Round(novoCustoMedio, 2),     
                    Math.Round(novoCustoUltimaCompra, 2)     
                );

            }

            _contaAPagarDAO.CancelarPorCompra(modelo, serie, numeroNota, fornecedorId, motivoCancelamento);

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                    UPDATE Compra 
                    SET Status = false,  -- Define como Inativo/Cancelado
                    MotivoCancelamento = @MotivoCancelamento
                      WHERE Modelo = @Modelo 
                      AND Serie = @Serie 
                      AND NumeroNota = @NumeroNota 
                      AND FornecedorId = @FornecedorId";

                var cmd = new MySqlCommand(sql, conexao);

                cmd.Parameters.AddWithValue("@MotivoCancelamento", motivoCancelamento);

                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}