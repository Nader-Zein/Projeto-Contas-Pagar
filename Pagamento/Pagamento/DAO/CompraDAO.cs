using MySql.Data.MySqlClient;
using Pagamento.Models;
using System;
using System.Collections.Generic;

namespace Pagamento.DAO
{
    public class CompraDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";



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
                            NomeFornecedor = readerPrincipal["NomeFornecedor"].ToString()
                        };
                        listaDeCompras.Add(compra);
                    }
                    readerPrincipal.Close(); 

                    foreach (var compra in listaDeCompras)
                    {
                        string sqlItens = @"SELECT 
                                        ic.*, 
                                        p.Descricao AS NomeProduto 
                                    FROM ItemCompra ic
                                    JOIN Produto p ON ic.ProdutoId = p.IdProduto
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
                                NomeProduto = readerItens["NomeProduto"].ToString()
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
                        string sqlItem = @"INSERT INTO ItemCompra 
                                           (CompraModelo, CompraSerie, CompraNumeroNota, CompraFornecedorId, ProdutoId, Quantidade, ValorUnitario) 
                                           VALUES 
                                           (@CompraModelo, @CompraSerie, @CompraNumeroNota, @CompraFornecedorId, @ProdutoId, @Quantidade, @ValorUnitario)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conexao);
                        cmdItem.Parameters.AddWithValue("@CompraModelo", compra.Modelo);
                        cmdItem.Parameters.AddWithValue("@CompraSerie", compra.Serie);
                        cmdItem.Parameters.AddWithValue("@CompraNumeroNota", compra.NumeroNota);
                        cmdItem.Parameters.AddWithValue("@CompraFornecedorId", compra.FornecedorId);
                        cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@ValorUnitario", item.ValorUnitario);
                        cmdItem.ExecuteNonQuery(); 
                    }

                    var custosRateados = RatearCustosAdicionais(compra);
                    var produtoDAO = new ProdutoDAO(); 
                    var produtoFornecedorDAO = new ProdutoFornecedorDAO();

                    foreach (var item in compra.Itens)
                    {
                        if (custosRateados.TryGetValue(item.ProdutoId, out decimal custoRealRateado))
                        {



                            
                            produtoFornecedorDAO.GarantirAssociacao(item.ProdutoId, compra.FornecedorId);


                            produtoFornecedorDAO.AtualizarDadosCompra(item.ProdutoId, compra.FornecedorId, custoRealRateado, compra.DataEmissao, compra.Observacoes);

                            Produto produtoAtual = produtoDAO.BuscarPorId(item.ProdutoId);
                            if (produtoAtual != null)
                            {
                                int qtdAntiga = produtoAtual.Quantidade;
                                decimal custoMedioAntigo = produtoAtual.PrecoMedioCusto;
                                int qtdNova = item.Quantidade;
                                int novaQtdTotal = qtdAntiga + qtdNova;
                                decimal novoCustoMedio = custoRealRateado;
                                if (novaQtdTotal > 0 && qtdAntiga > 0)
                                {
                                    novoCustoMedio = ((qtdAntiga * custoMedioAntigo) + (qtdNova * custoRealRateado)) / novaQtdTotal;
                                }
                                produtoDAO.AtualizarEstoqueECusto(item.ProdutoId, novaQtdTotal, novoCustoMedio);
                            }
                        }
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
    }
}