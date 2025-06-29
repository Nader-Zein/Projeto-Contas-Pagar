using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class ClienteDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Cliente> Listar()
        {
            var lista = new List<Cliente>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Cliente";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Cliente
                    {
                        IdPessoa = reader.GetInt32("IdPessoa"),
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
                    });
                }

                reader.Close();

                string sqlView = "SELECT IdPessoa, NomeCidade FROM vw_cliente_cidade";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var nomesCidade = new Dictionary<int, string>();

                while (viewReader.Read())
                {
                    int idCliente = viewReader.GetInt32("IdPessoa");
                    string nomeCidade = viewReader.GetString("NomeCidade");

                    nomesCidade[idCliente] = nomeCidade;
                }

                viewReader.Close();

                foreach (var cliente in lista)
                {
                    if (nomesCidade.TryGetValue(cliente.IdPessoa, out string nomeCidade))
                    {
                        cliente.NomeCidade = nomeCidade;
                    }
                }
            }

            return lista;
        }

        public void Inserir(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Cliente 
                           (TipoPessoa, Nome_RazaoSocial, Apelido_NomeFantasia, DataNascimento_Fundacao, CPF_CNPJ, RG_InsEstadual, Email, Telefone, Endereco, Numero, Complemento, Bairro, Cep, Status, IdCidade, IdCondPgto, LimiteCredito, DataCriacao)
                           VALUES 
                           (@TipoPessoa, @Nome, @Apelido, @Data, @CPF_CNPJ, @RG_InsEstadual, @Email, @Telefone, @Endereco, @Numero, @Complemento, @Bairro, @Cep, @Status, @IdCidade, @IdCondPgto, @LimiteCredito, @DataCriacao)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", cliente.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Apelido", string.IsNullOrWhiteSpace(cliente.Apelido_NomeFantasia) ? DBNull.Value : cliente.Apelido_NomeFantasia.ToUpper());
                cmd.Parameters.AddWithValue("@Data", cliente.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", cliente.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", cliente.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", cliente.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", cliente.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", cliente.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(cliente.Complemento) ? DBNull.Value : cliente.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", cliente.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", cliente.Cep);
                cmd.Parameters.AddWithValue("@Status", cliente.Status);
                cmd.Parameters.AddWithValue("@IdCidade", cliente.IdCidade);
                cmd.Parameters.AddWithValue("@IdCondPgto", cliente.IdCondPgto);
                cmd.Parameters.AddWithValue("@LimiteCredito", cliente.LimiteCredito ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Cliente SET 
                           TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, Apelido_NomeFantasia = @Apelido, DataNascimento_Fundacao = @Data, CPF_CNPJ = @CPF_CNPJ,
                           RG_InsEstadual = @RG_InsEstadual, Email = @Email, Telefone = @Telefone, Endereco = @Endereco,
                           Numero = @Numero, Complemento = @Complemento, Bairro = @Bairro, Cep = @Cep, Status = @Status,
                           IdCidade = @IdCidade, IdCondPgto = @IdCondPgto, LimiteCredito = @LimiteCredito, DataEdicao = @DataEdicao
                           WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", cliente.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Apelido", string.IsNullOrWhiteSpace(cliente.Apelido_NomeFantasia) ? DBNull.Value : cliente.Apelido_NomeFantasia.ToUpper());
                cmd.Parameters.AddWithValue("@Data", cliente.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", cliente.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", cliente.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", cliente.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", cliente.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", cliente.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(cliente.Complemento) ? DBNull.Value : cliente.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", cliente.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", cliente.Cep);
                cmd.Parameters.AddWithValue("@Status", cliente.Status);
                cmd.Parameters.AddWithValue("@IdCidade", cliente.IdCidade);
                cmd.Parameters.AddWithValue("@IdCondPgto", cliente.IdCondPgto);
                cmd.Parameters.AddWithValue("@LimiteCredito", cliente.LimiteCredito ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Id", cliente.IdPessoa);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                var cmd = new MySqlCommand("DELETE FROM Cliente WHERE IdPessoa = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public Cliente BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Cliente WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Cliente
                    {
                        IdPessoa = reader.GetInt32("IdPessoa"),
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
    }
}

