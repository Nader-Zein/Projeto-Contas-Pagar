using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ContaAPagarDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";


        public List<ContaAPagar> Listar()
        {
            var lista = new List<ContaAPagar>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"
                SELECT 
                    c.*, 
                    f.Nome_RazaoSocial AS NomeFornecedor,
                    fp.Descricao AS NomeFormaPgto
                FROM Conta_a_Pagar c
                JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor
                LEFT JOIN FormaPagamento fp ON c.IdFormaPgto = fp.IdFormaPgto
                ORDER BY 
                    c.DataEmissao DESC,  
                    c.FornecedorId, 
                    c.Modelo, 
                    c.Serie, 
                    c.NumeroNota, 
                    c.NumeroParcela ASC";    

                    var cmd = new MySqlCommand(sql, conexao);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var conta = new ContaAPagar
                        {
                            
                            Modelo = reader.IsDBNull(reader.GetOrdinal("Modelo")) ? null : reader.GetString("Modelo"),
                            Serie = reader.IsDBNull(reader.GetOrdinal("Serie")) ? null : reader.GetString("Serie"),
                            NumeroNota = reader.GetInt32("NumeroNota"),
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            NumeroParcela = reader.GetInt32("NumeroParcela"),
                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataVencimento = reader.GetDateTime("DataVencimento"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago"))
                                        ? (decimal?)null
                                        : reader.GetDecimal("ValorPago"),
                            IdFormaPgto = reader.IsDBNull(reader.GetOrdinal("IdFormaPgto")) ? (int?)null : reader.GetInt32("IdFormaPgto"),
                            Status = reader.GetBoolean("Status"),
                            Situacao = reader.GetString("Situacao"),
                            Motivo_Cancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento")) ? null : reader.GetString("MotivoCancelamento"),

                            NomeFornecedor = reader.GetString("NomeFornecedor"),
                            NomeFormaPgto = reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")) ? null : reader.GetString("NomeFormaPgto"),

                            Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto")
                        };
                        lista.Add(conta);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao listar contas a pagar: {ex.Message}");
                }
            }
            return lista;
        }


        public void Inserir(ContaAPagar conta)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"INSERT INTO Conta_a_Pagar 
                                 (Modelo, Serie, NumeroNota, FornecedorId, NumeroParcela, 
                                  Status, Situacao, ValorParcela, DataVencimento, DataEmissao, 
                                  DataPagamento, Juros, Multa, Desconto, ValorPago, IdFormaPgto) 
                                 VALUES 
                                 (@Modelo, @Serie, @NumeroNota, @FornecedorId, @NumeroParcela, 
                                  @Status, @Situacao, @ValorParcela, @DataVencimento, @DataEmissao, 
                                  NULL, @Juros, @Multa, @Desconto, NULL, @IdFormaPgto)";
                    MySqlCommand cmd = new MySqlCommand(sql, conexao);

                    cmd.Parameters.AddWithValue("@Modelo", conta.Modelo);
                    cmd.Parameters.AddWithValue("@Serie", conta.Serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", conta.NumeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", conta.FornecedorId);
                    cmd.Parameters.AddWithValue("@NumeroParcela", conta.NumeroParcela);

                    cmd.Parameters.AddWithValue("@Status", conta.Status);      
                    cmd.Parameters.AddWithValue("@Situacao", conta.Situacao);     
                    cmd.Parameters.AddWithValue("@ValorParcela", conta.ValorParcela);
                    cmd.Parameters.AddWithValue("@DataVencimento", conta.DataVencimento);
                    cmd.Parameters.AddWithValue("@DataEmissao", conta.DataEmissao);

                    cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? 0.00m);

                    cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto.HasValue ? (object)conta.IdFormaPgto.Value : (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao inserir conta a pagar (Parcela: {conta.NumeroParcela}). Detalhes: {ex.Message}");
                }
            }
        }

        public List<ContaAPagar> ListarPorCompra(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            var lista = new List<ContaAPagar>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"SELECT 
                                     cap.*, 
                                     fp.Descricao AS NomeFormaPgto 
                                 FROM Conta_a_Pagar cap
                                 LEFT JOIN FormaPagamento fp ON cap.IdFormaPgto = fp.IdFormaPgto
                                 WHERE 
                                     cap.Modelo = @Modelo AND
                                     cap.Serie = @Serie AND
                                     cap.NumeroNota = @NumeroNota AND
                                     cap.FornecedorId = @FornecedorId
                                 ORDER BY cap.NumeroParcela";

                    var cmd = new MySqlCommand(sql, conexao);
                    cmd.Parameters.AddWithValue("@Modelo", modelo);
                    cmd.Parameters.AddWithValue("@Serie", serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var conta = new ContaAPagar
                        {
                            Modelo = reader.GetString("Modelo"),
                            Serie = reader.GetString("Serie"),
                            NumeroNota = reader.GetInt32("NumeroNota"),
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            NumeroParcela = reader.GetInt32("NumeroParcela"),
                            Status = reader.GetBoolean("Status"),
                            Situacao = reader.IsDBNull(reader.GetOrdinal("Situacao")) ? null : reader.GetString("Situacao"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            DataVencimento = reader.GetDateTime("DataVencimento"),
                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataPagamento = reader.IsDBNull(reader.GetOrdinal("DataPagamento")) ? (DateTime?)null : reader.GetDateTime("DataPagamento"),
                            Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto"),
                            ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago")) ? (decimal?)null : reader.GetDecimal("ValorPago"),
                            IdFormaPgto = reader.IsDBNull(reader.GetOrdinal("IdFormaPgto")) ? (int?)null : reader.GetInt32("IdFormaPgto"),
                            NomeFormaPgto = reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")) ? "N/D" : reader.GetString("NomeFormaPgto")
                        };
                        lista.Add(conta);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao listar contas a pagar da compra. Detalhes: {ex.Message}");
                }
            }
            return lista;
        }


        public bool VerificarParcelasPagas(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                        SELECT COUNT(*) 
                        FROM Conta_a_Pagar
                        WHERE Modelo = @Modelo 
                          AND Serie = @Serie 
                          AND NumeroNota = @NumeroNota 
                          AND FornecedorId = @FornecedorId
                          AND Situacao = 'PAGO'";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                var count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }


        public void CancelarPorCompra(string modelo, string serie, int numeroNota, int fornecedorId, string motivoCancelamento)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                    UPDATE Conta_a_Pagar
                    SET Status = false,
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

        public ContaAPagar BuscarPorChave(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            ContaAPagar conta = null;
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"
                        SELECT 
                            c.*, 
                            f.Nome_RazaoSocial AS NomeFornecedor,
                            fp.Descricao AS NomeFormaPgto
                        FROM Conta_a_Pagar c
                        JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor
                        LEFT JOIN FormaPagamento fp ON c.IdFormaPgto = fp.IdFormaPgto
                        WHERE 
                            c.Modelo = @Modelo AND
                            c.Serie = @Serie AND
                            c.NumeroNota = @NumeroNota AND
                            c.FornecedorId = @FornecedorId AND
                            c.NumeroParcela = @NumeroParcela";

                    var cmd = new MySqlCommand(sql, conexao);
                    cmd.Parameters.AddWithValue("@Modelo", modelo);
                    cmd.Parameters.AddWithValue("@Serie", serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);
                    cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        conta = new ContaAPagar
                        {
                            Modelo = reader.GetString("Modelo"),
                            Serie = reader.GetString("Serie"),
                            NumeroNota = reader.GetInt32("NumeroNota"),
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            NumeroParcela = reader.GetInt32("NumeroParcela"),

                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataVencimento = reader.GetDateTime("DataVencimento"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            Status = reader.GetBoolean("Status"),
                            Situacao = reader.GetString("Situacao"),
                            Motivo_Cancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento")) ? null : reader.GetString("MotivoCancelamento"),

                            Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto"),

                            DataPagamento = reader.IsDBNull(reader.GetOrdinal("DataPagamento")) ? (DateTime?)null : reader.GetDateTime("DataPagamento"),
                            ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago")) ? (decimal?)null : reader.GetDecimal("ValorPago"),
                            IdFormaPgto = reader.IsDBNull(reader.GetOrdinal("IdFormaPgto")) ? (int?)null : reader.GetInt32("IdFormaPgto"),

                            NomeFornecedor = reader.GetString("NomeFornecedor"),
                            NomeFormaPgto = reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")) ? null : reader.GetString("NomeFormaPgto")
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao buscar conta a pagar. Detalhes: {ex.Message}");
                }
            }
            return conta;
        }

        public void EfetuarBaixa(ContaAPagar conta)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    string sql = @"UPDATE Conta_a_Pagar
                           SET 
                               Situacao = 'PAGO',
                               DataPagamento = @DataPagamento,
                               Juros = @Juros,
                               Multa = @Multa,
                               Desconto = @Desconto,
                               ValorPago = @ValorPago,
                               IdFormaPgto = @IdFormaPgto
                           WHERE 
                               Modelo = @Modelo AND
                               Serie = @Serie AND
                               NumeroNota = @NumeroNota AND
                               FornecedorId = @FornecedorId AND
                               NumeroParcela = @NumeroParcela";

                    var cmd = new MySqlCommand(sql, conexao);

                    cmd.Parameters.AddWithValue("@DataPagamento", conta.DataPagamento);
                    cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? 0.00m);
                    cmd.Parameters.AddWithValue("@ValorPago", conta.ValorPago ?? 0.00m);
                    cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto.HasValue ? (object)conta.IdFormaPgto.Value : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Modelo", conta.Modelo);
                    cmd.Parameters.AddWithValue("@Serie", conta.Serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", conta.NumeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", conta.FornecedorId);
                    cmd.Parameters.AddWithValue("@NumeroParcela", conta.NumeroParcela);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao efetuar baixa da parcela. Detalhes: {ex.Message}");
                }
            }
        }

        public void CancelarParcela(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela, string motivoCancelamento)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                        UPDATE Conta_a_Pagar
                        SET Status = false,
                            MotivoCancelamento = @MotivoCancelamento
                        WHERE 
                            Modelo = @Modelo AND
                            Serie = @Serie AND
                            NumeroNota = @NumeroNota AND
                            FornecedorId = @FornecedorId AND
                            NumeroParcela = @NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);

                cmd.Parameters.AddWithValue("@MotivoCancelamento", motivoCancelamento);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                cmd.ExecuteNonQuery();
            }
        }
    }
}
