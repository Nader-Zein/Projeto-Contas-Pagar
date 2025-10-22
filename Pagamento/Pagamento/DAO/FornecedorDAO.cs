using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class FornecedorDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Fornecedor> Listar()
        {
            var lista = new List<Fornecedor>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Fornecedor";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Fornecedor
                    {
                        IdPessoa = reader.GetInt32("IdFornecedor"),
                        TipoPessoa = reader.GetString("TipoPessoa"),
                        Nome_RazaoSocial = reader.GetString("Nome_RazaoSocial"),
                        Apelido_NomeFantasia = reader.IsDBNull(reader.GetOrdinal("Apelido_NomeFantasia")) ? null : reader.GetString("Apelido_NomeFantasia"),
                        DataNascimento_Fundacao = reader.GetDateTime("DataNascimento_Fundacao"),
                        CPF_CNPJ = reader.IsDBNull(reader.GetOrdinal("CPF_CNPJ")) ? null : reader.GetString("CPF_CNPJ"),
                        RG_InsEstadual = reader.GetString("RG_InsEstadual"),
                        Email = reader.GetString("Email"),
                        Telefone = reader.GetString("Telefone"),
                        Endereco = reader.GetString("Endereco"),
                        Numero = reader.GetString("Numero"),
                        Complemento = reader.IsDBNull(reader.GetOrdinal("Complemento")) ? null : reader.GetString("Complemento"),
                        Bairro = reader.GetString("Bairro"),
                        Cep = reader.GetString("Cep"),
                        Status = reader.GetBoolean("Status"),
                        IdCidade = reader.GetInt32("IdCidade"),
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        LimiteCredito = reader.IsDBNull(reader.GetOrdinal("LimiteCredito"))? (decimal?)null: reader.GetDecimal("LimiteCredito"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? null : reader.GetDateTime("DataEdicao")
                    });
                }
                reader.Close();

                string sqlView = "SELECT IdFornecedor, NomeCidade FROM vw_fornecedor_cidade";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var nomesCidade = new Dictionary<int, string>();

                while (viewReader.Read())
                {
                    int idFornecedor = viewReader.GetInt32("IdFornecedor");
                    string nomeCidade = viewReader.GetString("NomeCidade");

                    nomesCidade[idFornecedor] = nomeCidade;
                }

                viewReader.Close();

                foreach (var fornecedor in lista)
                {
                    if (nomesCidade.TryGetValue(fornecedor.IdPessoa, out string nomeCidade))
                    {
                        fornecedor.NomeCidade = nomeCidade;
                    }
                }
            }

            return lista;
        }

        public void Inserir(Fornecedor fornecedor)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Fornecedor 
                               (TipoPessoa, Nome_RazaoSocial, Apelido_NomeFantasia, DataNascimento_Fundacao, CPF_CNPJ, RG_InsEstadual, Email, Telefone, 
                                Endereco, Numero, Complemento, Bairro, Cep, Status, IdCidade, IdCondPgto, LimiteCredito, DataCriacao)
                               VALUES 
                               (@TipoPessoa, @Nome, @Apelido, @Data, @CPF_CNPJ, @RG_InsEstadual, @Email, @Telefone, 
                                @Endereco, @Numero, @Complemento, @Bairro, @Cep, @Status, @IdCidade, @IdCondPgto, @LimiteCredito, @DataCriacao)";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", fornecedor.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", fornecedor.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Apelido",string.IsNullOrWhiteSpace(fornecedor.Apelido_NomeFantasia)? DBNull.Value: fornecedor.Apelido_NomeFantasia.ToUpper());
                cmd.Parameters.AddWithValue("@Data", fornecedor.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", fornecedor.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", fornecedor.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", fornecedor.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", fornecedor.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", fornecedor.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(fornecedor.Complemento) ? DBNull.Value : fornecedor.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", fornecedor.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", fornecedor.Cep);
                cmd.Parameters.AddWithValue("@Status", fornecedor.Status);
                cmd.Parameters.AddWithValue("@IdCidade", fornecedor.IdCidade);
                cmd.Parameters.AddWithValue("@IdCondPgto", fornecedor.IdCondPgto);
                cmd.Parameters.AddWithValue("@LimiteCredito", fornecedor.LimiteCredito ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);

                cmd.ExecuteNonQuery();

                fornecedor.IdPessoa = (int)cmd.LastInsertedId;

            }
        }

        public void Atualizar(Fornecedor fornecedor)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Fornecedor 
                               SET TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, Apelido_NomeFantasia = @Apelido,
                                   DataNascimento_Fundacao = @Data, CPF_CNPJ = @CPF_CNPJ, RG_InsEstadual = @RG_InsEstadual,
                                   Email = @Email, Telefone = @Telefone, Endereco = @Endereco, Numero = @Numero,
                                   Complemento = @Complemento, Bairro = @Bairro, Cep = @Cep, Status = @Status, 
                                   IdCidade = @IdCidade, IdCondPgto = @IdCondPgto, LimiteCredito = @LimiteCredito, 
                                   DataEdicao = @DataEdicao
                               WHERE IdFornecedor = @Id";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", fornecedor.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", fornecedor.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Apelido", string.IsNullOrWhiteSpace(fornecedor.Apelido_NomeFantasia) ? DBNull.Value : fornecedor.Apelido_NomeFantasia.ToUpper());
                cmd.Parameters.AddWithValue("@Data", fornecedor.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", fornecedor.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", fornecedor.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", fornecedor.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", fornecedor.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", fornecedor.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(fornecedor.Complemento) ? DBNull.Value : fornecedor.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", fornecedor.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", fornecedor.Cep);
                cmd.Parameters.AddWithValue("@Status", fornecedor.Status);
                cmd.Parameters.AddWithValue("@IdCidade", fornecedor.IdCidade);
                cmd.Parameters.AddWithValue("@IdCondPgto", fornecedor.IdCondPgto);
                cmd.Parameters.AddWithValue("@LimiteCredito", fornecedor.LimiteCredito ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", fornecedor.IdPessoa);

                cmd.ExecuteNonQuery();
            }
        }

        public Fornecedor BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                var cmd = new MySqlCommand("SELECT * FROM Fornecedor WHERE IdFornecedor = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Fornecedor
                    {
                        IdPessoa = reader.GetInt32("IdFornecedor"),
                        TipoPessoa = reader.GetString("TipoPessoa"),
                        Nome_RazaoSocial = reader.GetString("Nome_RazaoSocial"),
                        Apelido_NomeFantasia = reader.IsDBNull(reader.GetOrdinal("Apelido_NomeFantasia")) ? null : reader.GetString("Apelido_NomeFantasia"),
                        DataNascimento_Fundacao = reader.GetDateTime("DataNascimento_Fundacao"),
                        CPF_CNPJ = reader.IsDBNull(reader.GetOrdinal("CPF_CNPJ")) ? null : reader.GetString("CPF_CNPJ"),
                        RG_InsEstadual = reader.GetString("RG_InsEstadual"),
                        Email = reader.GetString("Email"),
                        Telefone = reader.GetString("Telefone"),
                        Endereco = reader.GetString("Endereco"),
                        Numero = reader.GetString("Numero"),
                        Complemento = reader.IsDBNull(reader.GetOrdinal("Complemento")) ? null : reader.GetString("Complemento"),
                        Bairro = reader.GetString("Bairro"),
                        Cep = reader.GetString("Cep"),
                        Status = reader.GetBoolean("Status"),
                        IdCidade = reader.GetInt32("IdCidade"),
                        IdCondPgto = reader.GetInt32("IdCondPgto"),
                        LimiteCredito = reader.IsDBNull(reader.GetOrdinal("LimiteCredito")) ? (decimal?)null : reader.GetDecimal("LimiteCredito"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? null : reader.GetDateTime("DataEdicao")
                    };
                }
            }

            return null;
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                var cmd = new MySqlCommand("DELETE FROM Fornecedor WHERE IdFornecedor = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteCpfCnpj(string cpfCnpj)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT COUNT(*) FROM Fornecedor WHERE CPF_CNPJ = @CPF_CNPJ";
                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@CPF_CNPJ", cpfCnpj.Trim());
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
