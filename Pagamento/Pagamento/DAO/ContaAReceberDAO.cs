using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ContaAReceberDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public void Inserir(ContaAReceber conta)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Conta_a_Receber 
                               (Modelo, Serie, NumeroNota, ClienteId, NumeroParcela, 
                                Status, Situacao, ValorParcela, DataVencimento, DataEmissao, 
                                Juros, Multa, Desconto, IdFormaPgto) 
                               VALUES 
                               (@Modelo, @Serie, @NumeroNota, @ClienteId, @NumeroParcela, 
                                @Status, @Situacao, @ValorParcela, @DataVencimento, @DataEmissao, 
                                @Juros, @Multa, @Desconto, @IdFormaPgto)";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", conta.Modelo);
                cmd.Parameters.AddWithValue("@Serie", conta.Serie);
                cmd.Parameters.AddWithValue("@NumeroNota", conta.NumeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", conta.ClienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", conta.NumeroParcela);
                cmd.Parameters.AddWithValue("@Status", conta.Status);
                cmd.Parameters.AddWithValue("@Situacao", conta.Situacao);
                cmd.Parameters.AddWithValue("@ValorParcela", conta.ValorParcela);
                cmd.Parameters.AddWithValue("@DataVencimento", conta.DataVencimento);
                cmd.Parameters.AddWithValue("@DataEmissao", conta.DataEmissao);

                // Campos nulos permitidos
                cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto);

                cmd.ExecuteNonQuery();
            }
        }

        // 2. Listar todas as contas (Para o Index Principal)
        public List<ContaAReceber> Listar()
        {
            var lista = new List<ContaAReceber>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // JOIN com Cliente para mostrar o nome na lista principal
                string sql = @"SELECT cr.*, c.Nome_RazaoSocial as NomeCliente, fp.Descricao as NomeFormaPgto
                               FROM Conta_a_Receber cr
                               LEFT JOIN Cliente c ON cr.ClienteId = c.IdCliente
                               LEFT JOIN FormaPagamento fp ON cr.IdFormaPgto = fp.IdFormaPgto
                               ORDER BY cr.Modelo, cr.Serie, cr.NumeroNota, cr.ClienteId, cr.NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var conta = Mapear(reader);
                    // Preenche nomes auxiliares
                    if (!reader.IsDBNull(reader.GetOrdinal("NomeCliente")))
                        conta.NomeCliente = reader.GetString("NomeCliente");

                    if (!reader.IsDBNull(reader.GetOrdinal("NomeFormaPgto")))
                        conta.NomeFormaPgto = reader.GetString("NomeFormaPgto");

                    lista.Add(conta);
                }
            }
            return lista;
        }


        public List<ContaAReceber> ListarPorVenda(string modelo, string serie, int numeroNota, int clienteId)
        {
            var lista = new List<ContaAReceber>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // Traz o nome da Forma de Pagamento através de um JOIN
                string sql = @"SELECT cr.*, fp.Descricao as NomeFormaPgto
                               FROM Conta_a_Receber cr
                               JOIN FormaPagamento fp ON cr.IdFormaPgto = fp.IdFormaPgto
                               WHERE cr.Modelo = @Modelo 
                                 AND cr.Serie = @Serie 
                                 AND cr.NumeroNota = @NumeroNota 
                                 AND cr.ClienteId = @ClienteId
                               ORDER BY cr.NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(Mapear(reader));
                }
            }
            return lista;
        }

        // Verifica se alguma parcela desta venda já foi recebida (baixada)
        public bool VerificarParcelasPagas(string modelo, string serie, int numeroNota, int clienteId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"SELECT COUNT(*) FROM Conta_a_Receber 
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId
                                 AND (DataPagamento IS NOT NULL OR ValorPago > 0)";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);

                long count = Convert.ToInt64(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        // Cancela (exclui) todas as parcelas pendentes de uma venda (usado ao cancelar a venda inteira)
        public void CancelarPorVenda(string modelo, string serie, int numeroNota, int clienteId, string motivo)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                // Opcional: Se você quiser manter o histórico das parcelas canceladas, use UPDATE em vez de DELETE
                // Aqui vamos seguir a lógica de marcar como cancelado na tabela (UPDATE)

                string sql = @"UPDATE Conta_a_Receber 
                               SET Situacao = 'CANCELADA', 
                                   MotivoCancelamento = @Motivo,
                                   Status = false
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@Motivo", motivo);

                cmd.ExecuteNonQuery();
            }
        }

        // Método para realizar o Recebimento (Baixa) de uma parcela específica
        public void ReceberParcela(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela,
                                   DateTime dataPagamento, decimal valorPago, decimal juros, decimal multa, decimal desconto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Conta_a_Receber 
                               SET DataPagamento = @DataPagamento,
                                   ValorPago = @ValorPago,
                                   ValJuros = @Juros,
                                   ValMulta = @Multa,
                                   ValDesconto = @Desconto,
                                   Situacao = 'RECEBIDA',
                                   Status = true -- Define como false pois não está mais pendente
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId
                                 AND NumeroParcela = @NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@DataPagamento", dataPagamento);
                cmd.Parameters.AddWithValue("@ValorPago", valorPago);
                cmd.Parameters.AddWithValue("@Juros", juros);
                cmd.Parameters.AddWithValue("@Multa", multa);
                cmd.Parameters.AddWithValue("@Desconto", desconto);

                // Chaves
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                cmd.ExecuteNonQuery();
            }
        }

        // Método auxiliar para mapear o DataReader para o Objeto
        private ContaAReceber Mapear(MySqlDataReader reader)
        {
            var conta = new ContaAReceber
            {
                Modelo = reader["Modelo"].ToString(),
                Serie = reader["Serie"].ToString(),
                NumeroNota = Convert.ToInt32(reader["NumeroNota"]),
                ClienteId = Convert.ToInt32(reader["ClienteId"]),
                NumeroParcela = Convert.ToInt32(reader["NumeroParcela"]),
                Status = Convert.ToBoolean(reader["Status"]),
                Situacao = reader["Situacao"].ToString(),
                ValorParcela = Convert.ToDecimal(reader["ValorParcela"]),
                DataVencimento = Convert.ToDateTime(reader["DataVencimento"]),
                DataEmissao = Convert.ToDateTime(reader["DataEmissao"]),
                IdFormaPgto = Convert.ToInt32(reader["IdFormaPgto"]),

                // Tratamento de nulos para campos opcionais
                DataPagamento = reader.IsDBNull(reader.GetOrdinal("DataPagamento")) ? (DateTime?)null : reader.GetDateTime("DataPagamento"),
                ValorPago = reader.IsDBNull(reader.GetOrdinal("ValorPago")) ? (decimal?)null : reader.GetDecimal("ValorPago"),
                Juros = reader.IsDBNull(reader.GetOrdinal("Juros")) ? (decimal?)null : reader.GetDecimal("Juros"),
                Multa = reader.IsDBNull(reader.GetOrdinal("Multa")) ? (decimal?)null : reader.GetDecimal("Multa"),
                Desconto = reader.IsDBNull(reader.GetOrdinal("Desconto")) ? (decimal?)null : reader.GetDecimal("Desconto"),
                ValJuros = reader.IsDBNull(reader.GetOrdinal("ValJuros")) ? 0 : reader.GetDecimal("ValJuros"),
                ValMulta = reader.IsDBNull(reader.GetOrdinal("ValMulta")) ? 0 : reader.GetDecimal("ValMulta"),
                ValDesconto = reader.IsDBNull(reader.GetOrdinal("ValDesconto")) ? 0 : reader.GetDecimal("ValDesconto"),
                MotivoCancelamento = reader.IsDBNull(reader.GetOrdinal("MotivoCancelamento")) ? null : reader.GetString("MotivoCancelamento")
            };

            // Tenta mapear o nome da forma de pagamento se veio no SELECT
            try { conta.NomeFormaPgto = reader["NomeFormaPgto"].ToString(); } catch { }

            return conta;
        }

        // Buscar uma conta específica para a tela de Recebimento
        public ContaAReceber BuscarParcela(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"SELECT cr.*, c.Nome_RazaoSocial as NomeCliente 
                               FROM Conta_a_Receber cr
                               JOIN Cliente c ON cr.ClienteId = c.IdCliente
                               WHERE cr.Modelo = @Modelo 
                                 AND cr.Serie = @Serie 
                                 AND cr.NumeroNota = @NumeroNota 
                                 AND cr.ClienteId = @ClienteId
                                 AND cr.NumeroParcela = @NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var conta = Mapear(reader);
                    // Mapeia o nome do cliente para exibir na tela de baixa
                    try { conta.NomeCliente = reader["NomeCliente"].ToString(); } catch { }
                    return conta;
                }
            }
            return null;
        }

        // 1. Cancelar uma Parcela Específica (Faltando)
        public void CancelarParcela(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela, string motivo)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Conta_a_Receber 
                               SET Situacao = 'CANCELADO', 
                                   Status = false, 
                                   MotivoCancelamento = @Motivo
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId
                                 AND NumeroParcela = @NumeroParcela";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Motivo", motivo);
                // Chaves para identificar a parcela única
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                cmd.ExecuteNonQuery();
            }
        }

        // 2. Verificar Parcelas Anteriores Pendentes (Faltando)
        // Usado pelo JavaScript/Controller antes de abrir a tela de baixa
        public bool TemParcelaAnteriorPendente(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela)
        {
            // Se for a primeira parcela, não tem anterior, retorna false
            if (numeroParcela <= 1) return false;

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // Verifica se existe alguma parcela com número MENOR (<) 
                // que NÃO esteja "RECEBIDO" e NÃO esteja Cancelada (Status = true)
                string sql = @"SELECT COUNT(*) FROM Conta_a_Receber 
                               WHERE Modelo = @Modelo 
                                 AND Serie = @Serie 
                                 AND NumeroNota = @NumeroNota 
                                 AND ClienteId = @ClienteId
                                 AND NumeroParcela < @NumeroParcela
                                 AND Situacao != 'RECEBIDA' 
                                 AND Status = true";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                long count = Convert.ToInt64(cmd.ExecuteScalar());

                // Se count > 0, significa que existem parcelas anteriores não pagas
                return count > 0;
            }
        }
    }
}
