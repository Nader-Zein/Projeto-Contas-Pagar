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
            // 'connectionString' deve ser um campo privado da sua classe DAO
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    // 1. O SQL junta ContaAPagar com Fornecedor (obrigatório)
                    // 2. e usa LEFT JOIN com FormaPagamento (pois pode ser nulo)
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
                    c.NumeroParcela ASC"; // Ordena por vencimento

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
                            // ✅ ADICIONE ISSO
                            DataPagamento = reader.IsDBNull(reader.GetOrdinal("DataPagamento"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime("DataPagamento"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago"))
                                        ? (decimal?)null
                                        : reader.GetDecimal("ValorPago"),
                            IdFormaPgto = reader.IsDBNull(reader.GetOrdinal("IdFormaPgto")) ? (int?)null : reader.GetInt32("IdFormaPgto"),
                            Status = reader.GetBoolean("Status"),
                            Situacao = reader.GetString("Situacao"),
                            Motivo_Cancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento")) ? null : reader.GetString("MotivoCancelamento"),

                            ValJuros = reader.IsDBNull(reader.GetOrdinal("ValJuros")) ? 0 : reader.GetDecimal("ValJuros"),
                            ValMulta = reader.IsDBNull(reader.GetOrdinal("ValMulta")) ? 0 : reader.GetDecimal("ValMulta"),
                            ValDesconto = reader.IsDBNull(reader.GetOrdinal("ValDesconto")) ? 0 : reader.GetDecimal("ValDesconto"),
                            // Campos dos JOINs
                            NomeFornecedor = reader.GetString("NomeFornecedor"),
                            NomeFormaPgto = reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")) ? null : reader.GetString("NomeFormaPgto"),

                            // Campos de Termos (Juros, Multa, etc.)
                            Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto")
                        };
                        lista.Add(conta);
                    }
                }
                catch (Exception ex)
                {
                    // Lança o erro para o Controller tratar
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
                    // Comando SQL baseado na sua tabela 'Conta_a_Pagar'
                    string sql = @"INSERT INTO Conta_a_Pagar 
                                 (Modelo, Serie, NumeroNota, FornecedorId, NumeroParcela, 
                                  Status, Situacao, ValorParcela, DataVencimento, DataEmissao, 
                                  DataPagamento, Juros, Multa, Desconto, ValorPago, IdFormaPgto) 
                                 VALUES 
                                 (@Modelo, @Serie, @NumeroNota, @FornecedorId, @NumeroParcela, 
                                  @Status, @Situacao, @ValorParcela, @DataVencimento, @DataEmissao, 
                                  NULL, @Juros, @Multa, @Desconto, NULL, @IdFormaPgto)";
                    // Note: Campos de pagamento iniciam nulos ou com zero

                    MySqlCommand cmd = new MySqlCommand(sql, conexao);

                    // Mapeamento da Chave
                    cmd.Parameters.AddWithValue("@Modelo", conta.Modelo);
                    cmd.Parameters.AddWithValue("@Serie", conta.Serie);
                    cmd.Parameters.AddWithValue("@NumeroNota", conta.NumeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", conta.FornecedorId);
                    cmd.Parameters.AddWithValue("@NumeroParcela", conta.NumeroParcela);

                    // Dados da Parcela
                    cmd.Parameters.AddWithValue("@Status", conta.Status); // Status (ex: true = pendente)
                    cmd.Parameters.AddWithValue("@Situacao", conta.Situacao); // Situação (ex: 'A PAGAR')
                    cmd.Parameters.AddWithValue("@ValorParcela", conta.ValorParcela);
                    cmd.Parameters.AddWithValue("@DataVencimento", conta.DataVencimento);
                    cmd.Parameters.AddWithValue("@DataEmissao", conta.DataEmissao);

                    cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? 0.00m);

                    // IdFormaPgto pode ser nulo
                    cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto.HasValue ? (object)conta.IdFormaPgto.Value : (object)DBNull.Value);
                    // --- FIM DA MODIFICAÇÃO ---

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao inserir conta a pagar (Parcela: {conta.NumeroParcela}). Detalhes: {ex.Message}");
                }
            }
        }

        // --- INÍCIO DA MODIFICAÇÃO ---
        // Adicione este método
        public List<ContaAPagar> ListarPorCompra(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            var lista = new List<ContaAPagar>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    // Busca as contas a pagar e junta com FormaPagamento para pegar o nome
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
                // Esta lógica está correta, pois verifica a SITUAÇÃO (PAGO), 
                // e não o STATUS (Ativo/Cancelado).
                string sql = @"
                        SELECT COUNT(*) 
                        FROM Conta_a_Pagar
                        WHERE Modelo = @Modelo 
                          AND Serie = @Serie 
                          AND NumeroNota = @NumeroNota 
                          AND FornecedorId = @FornecedorId
                          AND Situacao = 'PAGO'";
                // Ajuste 'PAGO' se o seu texto for diferente

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                var count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }


        // --- INÍCIO DA MODIFICAÇÃO (Substituir ExcluirPorCompra) ---

        /// <summary>
        /// (NOVO MÉTODO)
        /// Cancela (define Status = false) TODAS as parcelas associadas a uma nota de compra.
        /// </summary>
        public void CancelarPorCompra(string modelo, string serie, int numeroNota, int fornecedorId, string motivoCancelamento)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // Altera o STATUS para 'false' (Cancelado) em vez de DELETAR
                string sql = @"
                    UPDATE Conta_a_Pagar
                    SET Status = false,
                        MotivoCancelamento = @MotivoCancelamento
                    WHERE Modelo = @Modelo 
                      AND Serie = @Serie 
                      AND NumeroNota = @NumeroNota 
                      AND FornecedorId = @FornecedorId";
                // Não precisamos verificar a Situação aqui, pois o
                // 'VerificarParcelasPagas' já terá barrado a execução
                // se alguma parcela estivesse 'PAGA'.

                var cmd = new MySqlCommand(sql, conexao);

                cmd.Parameters.AddWithValue("@MotivoCancelamento", motivoCancelamento);

                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Busca uma única Conta a Pagar pela sua chave composta.
        /// </summary>
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
                            // Chave
                            Modelo = reader.GetString("Modelo"),
                            Serie = reader.GetString("Serie"),
                            NumeroNota = reader.GetInt32("NumeroNota"),
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            NumeroParcela = reader.GetInt32("NumeroParcela"),

                            // Dados da Parcela
                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataVencimento = reader.GetDateTime("DataVencimento"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            Status = reader.GetBoolean("Status"),
                            Situacao = reader.GetString("Situacao"),
                            Motivo_Cancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento")) ? null : reader.GetString("MotivoCancelamento"),

                            // Campos de Termos (Juros, Multa, etc. vindo do BD)
                            Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                            Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                            Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto"),


                            // ---------------------------------------------------------
                            // 2. Campos de VALOR EM R$ (O que faltava!)
                            // ---------------------------------------------------------
                            // Se o banco tiver NULL, assume 0. Se tiver valor, lê o valor.
                            ValJuros = reader.IsDBNull(reader.GetOrdinal("ValJuros")) ? 0m : reader.GetDecimal("ValJuros"),
                            ValMulta = reader.IsDBNull(reader.GetOrdinal("ValMulta")) ? 0m : reader.GetDecimal("ValMulta"),
                            ValDesconto = reader.IsDBNull(reader.GetOrdinal("ValDesconto")) ? 0m : reader.GetDecimal("ValDesconto"),
                            // ---------------------------------------------------------


                            // Dados de Pagamento (se já existirem)
                            DataPagamento = reader.IsDBNull(reader.GetOrdinal("DataPagamento")) ? (DateTime?)null : reader.GetDateTime("DataPagamento"),
                            ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago")) ? (decimal?)null : reader.GetDecimal("ValorPago"),
                            IdFormaPgto = reader.IsDBNull(reader.GetOrdinal("IdFormaPgto")) ? (int?)null : reader.GetInt32("IdFormaPgto"),

                            // Campos dos JOINs
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

        /// <summary>
        /// Atualiza uma parcela para o status 'PAGO'.
        /// </summary>
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
                               ValJuros = @ValJuros,
                               ValMulta = @ValMulta,
                              ValDesconto = @ValDesconto,
                               ValorPago = @ValorPago,
                               IdFormaPgto = @IdFormaPgto
                           WHERE 
                               Modelo = @Modelo AND
                               Serie = @Serie AND
                               NumeroNota = @NumeroNota AND
                               FornecedorId = @FornecedorId AND
                               NumeroParcela = @NumeroParcela";

                    var cmd = new MySqlCommand(sql, conexao);

                    // Dados do Pagamento
                    cmd.Parameters.AddWithValue("@DataPagamento", conta.DataPagamento);
                    cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? 0.00m);
                    cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? 0.00m);
                    cmd.Parameters.AddWithValue("@ValJuros", conta.ValJuros);
                    cmd.Parameters.AddWithValue("@ValMulta", conta.ValMulta);
                    cmd.Parameters.AddWithValue("@ValDesconto", conta.ValDesconto);

                    cmd.Parameters.AddWithValue("@ValorPago", conta.ValorPago ?? 0.00m);
                    cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto.HasValue ? (object)conta.IdFormaPgto.Value : (object)DBNull.Value);

                    // Chave Primária
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

        /// <summary>
        /// Cancela (define Status = false) uma única parcela.
        /// </summary>
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


        // --- ADICIONE ESTE NOVO MÉTODO ---

        /// <summary>
        /// Verifica se existe alguma parcela anterior a esta (da mesma nota)
        /// que ainda esteja pendente de pagamento.
        /// </summary>
        /// <returns>True se houver pendência, False caso contrário.</returns>
        public bool TemParcelaAnteriorPendente(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            // 1. Se for a parcela 1, não há parcelas anteriores.
            if (numeroParcela <= 1)
            {
                return false;
            }

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                // --- INÍCIO DA MODIFICAÇÃO ---
                // 2. Verifica se a chave (Modelo, Serie, NNota, FornecedorId)
                //    corresponde a uma COMPRA existente.
                string sqlVerificaCompra = @"
                SELECT COUNT(*) 
                FROM Compra 
                WHERE Modelo = @Modelo 
                  AND Serie = @Serie 
                  AND NumeroNota = @NumeroNota 
                  AND FornecedorId = @FornecedorId";

                // Usamos (object)DBNull.Value para o caso de o usuário não ter preenchido
                // o modelo ou serie na tela de conta avulsa.
                var cmdVerifica = new MySqlCommand(sqlVerificaCompra, conexao);
                cmdVerifica.Parameters.AddWithValue("@Modelo", (object)modelo ?? DBNull.Value);
                cmdVerifica.Parameters.AddWithValue("@Serie", (object)serie ?? DBNull.Value);
                cmdVerifica.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmdVerifica.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                var compraCount = Convert.ToInt64(cmdVerifica.ExecuteScalar());

                // 3. Se não houver NENHUMA compra (compraCount == 0),
                //    significa que é uma "Conta Avulsa" (mesmo com os campos preenchidos).
                //    Neste caso, a regra de ordem NÃO SE APLICA.
                if (compraCount == 0)
                {
                    return false; // Retorna 'false' (sem pendência)
                }
                // --- FIM DA MODIFICAÇÃO ---



                // 3. Conta quantas parcelas ANTERIORES (NumeroParcela < @NumeroParcela)
                //    desta MESMA NOTA ainda NÃO ESTÃO 'PAGO'.
                string sql = @"
                SELECT COUNT(*) 
                FROM Conta_a_Pagar
                WHERE Modelo = @Modelo 
                  AND Serie = @Serie 
                  AND NumeroNota = @NumeroNota 
                  AND FornecedorId = @FornecedorId
                  AND NumeroParcela < @NumeroParcela
                  AND Situacao != 'PAGO'
                  AND Status = true"; // Garante que não estamos contando parcelas canceladas

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                var count = Convert.ToInt64(cmd.ExecuteScalar());

                // 4. Se count > 0, significa que há pendências.
                return count > 0;
            }
        }

        /// <summary>
        /// Verifica se já existe QUALQUER parcela (avulsa ou não)
        /// com a chave da nota (Modelo, Serie, NNota, FornecedorId).
        /// </summary>
        /// <returns>True se encontrar qualquer registro.</returns>
        public bool ExisteChaveDaNota(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // Usamos 'SELECT 1 ... LIMIT 1' por ser a forma mais rápida
                // de apenas verificar a existência.
                string sql = @"
                        SELECT 1 
                        FROM Conta_a_Pagar
                        WHERE Modelo = @Modelo 
                          AND Serie = @Serie 
                          AND NumeroNota = @NumeroNota 
                          AND FornecedorId = @FornecedorId
                        LIMIT 1";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", (object)modelo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Serie", (object)serie ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                var result = cmd.ExecuteScalar(); // Retorna 1 (se achou) ou null

                // Retorna true se o resultado não for nulo
                return result != null && result != DBNull.Value;
            }
        }

        // <summary>
        /// Busca a primeira parcela (avulsa ou não) encontrada
        /// pela chave da nota, trazendo o nome do fornecedor.
        /// </summary>
        public ContaAPagar BuscarPrimeiraParcelaDaNota(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            ContaAPagar conta = null;
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();
                    // Query similar a BuscarPorChave, mas sem NumeroParcela
                    // e com LIMIT 1
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
                            c.Status = true AND          -- <-- LINHA ADICIONADA
                            c.Situacao = 'A PAGAR'       -- <-- LINHA ADICIONADA
                        ORDER BY c.NumeroParcela ASC
                        LIMIT 1";

                    var cmd = new MySqlCommand(sql, conexao);
                    cmd.Parameters.AddWithValue("@Modelo", (object)modelo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Serie", (object)serie ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                    cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

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
                            DataVencimento = reader.GetDateTime("DataVencimento"),
                            ValorParcela = reader.GetDecimal("ValorParcela"),
                            Situacao = reader.GetString("Situacao"),
                            NomeFornecedor = reader.GetString("NomeFornecedor"),
                            NomeFormaPgto = reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")) ? null : reader.GetString("NomeFormaPgto")
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Lança o erro para o Controller tratar
                    throw new Exception($"Erro ao buscar primeira parcela da nota. Detalhes: {ex.Message}");
                }
            }
            return conta;
        }
    }
}
