using MySql.Data.MySqlClient;
using Pagamento.Models;
using System;
using System.Collections.Generic;

namespace Pagamento.DAO
{
    public class CompraDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public void Inserir(Compra compra)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                MySqlTransaction transaction = conexao.BeginTransaction();

                try
                {
                    string sqlCompra = @"INSERT INTO Compra 
                                         (Modelo, Serie, NumeroNota, FornecedorId, DataEmissao, DataChegada, CondicaoPagamentoId, Frete, Seguro, Despesas, Status) 
                                         VALUES 
                                         (@Modelo, @Serie, @NumeroNota, @FornecedorId, @DataEmissao, @DataChegada, @CondicaoPagamentoId, @Frete, @Seguro, @Despesas, @Status)";

                    MySqlCommand cmdCompra = new MySqlCommand(sqlCompra, conexao, transaction);

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

                    cmdCompra.ExecuteNonQuery();

                    foreach (var item in compra.Itens)
                    {
                        string sqlItem = @"INSERT INTO ItemCompra 
                                           (CompraModelo, CompraSerie, CompraNumeroNota, CompraFornecedorId, ProdutoId, Quantidade, ValorUnitario) 
                                           VALUES 
                                           (@CompraModelo, @CompraSerie, @CompraNumeroNota, @CompraFornecedorId, @ProdutoId, @Quantidade, @ValorUnitario)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conexao, transaction);

                        cmdItem.Parameters.AddWithValue("@CompraModelo", compra.Modelo);
                        cmdItem.Parameters.AddWithValue("@CompraSerie", compra.Serie);
                        cmdItem.Parameters.AddWithValue("@CompraNumeroNota", compra.NumeroNota);
                        cmdItem.Parameters.AddWithValue("@CompraFornecedorId", compra.FornecedorId);

                        cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@ValorUnitario", item.ValorUnitario);

                        cmdItem.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    throw new Exception("Erro ao inserir a compra no banco de dados. Detalhes: " + ex.Message);
                }
            }
        }


    }
}