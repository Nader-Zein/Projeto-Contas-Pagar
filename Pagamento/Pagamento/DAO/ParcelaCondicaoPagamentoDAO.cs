using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ParcelaCondicaoPagamentoDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234"; 

        public List<ParcelaCondicaoPagamento> ListarPorCondicaoPagamento(int idCondPgto)
        {
            var lista = new List<ParcelaCondicaoPagamento>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM ParcelaCondicaoPagamento WHERE IdCondPgto = @IdCondPgto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", idCondPgto);

                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new ParcelaCondicaoPagamento
                    {
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        NumeroParcela = reader.GetInt32("NumeroParcela"),
                        IdFormaPgto = reader.GetInt32("IdFormaPgto"),
                        ValorPercentual = reader.GetDecimal("ValorPercentual"),
                        DiasAposVenda = reader.GetInt32("DiasAposVenda")
                    });
                }
                reader.Close();

                string sqlView = "SELECT IdCondPgto, NumeroParcela, IdFormaPgto, NomeFormaPgto FROM vw_ParcelasCondicaoForma WHERE IdCondPgto = @IdCondPgto";
                var cmdView = new MySqlCommand(sqlView, conexao);
                cmdView.Parameters.AddWithValue("@IdCondPgto", idCondPgto);

                var readerView = cmdView.ExecuteReader();
                var nomesFormas = new Dictionary<(int, int, int), string>();

                while (readerView.Read())
                {
                    var chave = (
                        readerView.GetInt32("IdCondPgto"),
                        readerView.GetInt32("NumeroParcela"),
                        readerView.GetInt32("IdFormaPgto")
                    );
                    nomesFormas[chave] = readerView.GetString("NomeFormaPgto");
                }
                readerView.Close();

                foreach (var parcela in lista)
                {
                    var chave = (parcela.IdCondPgto, parcela.NumeroParcela, parcela.IdFormaPgto);
                    if (nomesFormas.TryGetValue(chave, out var nome))
                    {
                        parcela.NomeFormaPagamento = nome;
                    }
                }
            }

            return lista;
        }

        public void Inserir(ParcelaCondicaoPagamento parcela)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO ParcelaCondicaoPagamento 
                               (IdCondPgto, NumeroParcela, IdFormaPgto, ValorPercentual, DiasAposVenda) 
                               VALUES (@IdCondPgto, @NumeroParcela, @IdFormaPgto, @ValorPercentual, @DiasAposVenda)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", parcela.IdCondPgto);
                cmd.Parameters.AddWithValue("@NumeroParcela", parcela.NumeroParcela);
                cmd.Parameters.AddWithValue("@IdFormaPgto", parcela.IdFormaPgto);
                cmd.Parameters.AddWithValue("@ValorPercentual", parcela.ValorPercentual);
                cmd.Parameters.AddWithValue("@DiasAposVenda", parcela.DiasAposVenda);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(ParcelaCondicaoPagamento parcela)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE ParcelaCondicaoPagamento 
                               SET ValorPercentual = @ValorPercentual, DiasAposVenda = @DiasAposVenda
                               WHERE IdCondPgto = @IdCondPgto AND NumeroParcela = @NumeroParcela AND IdFormaPgto = @IdFormaPgto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", parcela.IdCondPgto);
                cmd.Parameters.AddWithValue("@NumeroParcela", parcela.NumeroParcela);
                cmd.Parameters.AddWithValue("@IdFormaPgto", parcela.IdFormaPgto);
                cmd.Parameters.AddWithValue("@ValorPercentual", parcela.ValorPercentual);
                cmd.Parameters.AddWithValue("@DiasAposVenda", parcela.DiasAposVenda);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int idCondPgto, int numeroParcela, int idFormaPgto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"DELETE FROM ParcelaCondicaoPagamento 
                               WHERE IdCondPgto = @IdCondPgto AND NumeroParcela = @NumeroParcela AND IdFormaPgto = @IdFormaPgto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", idCondPgto);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);
                cmd.Parameters.AddWithValue("@IdFormaPgto", idFormaPgto);
                cmd.ExecuteNonQuery();
            }
        }

        public ParcelaCondicaoPagamento BuscarPorId(int idCondPgto, int numeroParcela, int idFormaPgto)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"SELECT * FROM ParcelaCondicaoPagamento 
                               WHERE IdCondPgto = @IdCondPgto AND NumeroParcela = @NumeroParcela AND IdFormaPgto = @IdFormaPgto";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCondPgto", idCondPgto);
                cmd.Parameters.AddWithValue("@NumeroParcela", numeroParcela);
                cmd.Parameters.AddWithValue("@IdFormaPgto", idFormaPgto);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new ParcelaCondicaoPagamento
                    {
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        NumeroParcela = reader.GetInt32("NumeroParcela"),
                        IdFormaPgto = reader.GetInt32("IdFormaPgto"),
                        ValorPercentual = reader.GetDecimal("ValorPercentual"),
                        DiasAposVenda = reader.GetInt32("DiasAposVenda")
                    };
                }
            }

            return null;
        }
    }
}
