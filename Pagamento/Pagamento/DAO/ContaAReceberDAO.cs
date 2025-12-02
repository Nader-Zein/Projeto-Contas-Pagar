using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ContaAReceberDAO
    {
        private readonly string connectionString;

        public ContaAReceberDAO(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("PagamentoDB");
        }
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

                cmd.Parameters.AddWithValue("@Juros", conta.Juros ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Multa", conta.Multa ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Desconto", conta.Desconto ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IdFormaPgto", conta.IdFormaPgto);

                cmd.ExecuteNonQuery();
            }
        }

        public List<ContaAReceber> Listar()
        {
            var lista = new List<ContaAReceber>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
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

        public void CancelarPorVenda(string modelo, string serie, int numeroNota, int clienteId, string motivo)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

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

                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                cmd.ExecuteNonQuery();
            }
        }

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

            try { conta.NomeFormaPgto = reader["NomeFormaPgto"].ToString(); } catch { }

            return conta;
        }

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
                    try { conta.NomeCliente = reader["NomeCliente"].ToString(); } catch { }
                    return conta;
                }
            }
            return null;
        }

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
                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@ClienteId", clienteId);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);

                cmd.ExecuteNonQuery();
            }
        }

        public bool TemParcelaAnteriorPendente(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela)
        {
            if (numeroParcela <= 1) return false;

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
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

                return count > 0;
            }
        }
    }
}
