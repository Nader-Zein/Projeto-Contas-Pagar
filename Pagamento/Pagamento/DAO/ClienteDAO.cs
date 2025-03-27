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
                        DataNascimento_Fundacao = reader.GetDateTime("DataNascimento_Fundacao"),
                        CPF_CNPJ = reader.GetString("CPF_CNPJ"),
                        RG_InsMunicipal =  reader.GetString("RG_InsMunicipal"),
                        Email = reader.GetString("Email"),
                        Telefone = reader.GetString("Telefone"),
                        Endereco = reader.GetString("Endereco"),
                        Bairro = reader.GetString("Bairro"),
                        Cep = reader.GetString("Cep"),
                        Status = reader.GetString("Status"),
                        IdCidade = reader.GetInt32("IdCidade")
                    });
                }
            }

            return lista;
        }

        public void Inserir(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Cliente (TipoPessoa, Nome_RazaoSocial, DataNascimento_Fundacao, CPF_CNPJ, RG_InsMunicipal, Email, Telefone, Endereco, Bairro, Cep, Status, IdCidade)
                               VALUES (@TipoPessoa, @Nome, @Data, @CPF_CNPJ, @RG, @Email, @Telefone, @Endereco, @Bairro, @Cep, @Status, @IdCidade)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", cliente.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", cliente.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", cliente.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", cliente.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", cliente.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", cliente.Bairro);
                cmd.Parameters.AddWithValue("@Cep", cliente.Cep);
                cmd.Parameters.AddWithValue("@Status", cliente.Status);
                cmd.Parameters.AddWithValue("@IdCidade", cliente.IdCidade);
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
                        DataNascimento_Fundacao = reader.GetDateTime("DataNascimento_Fundacao"),
                        CPF_CNPJ = reader.GetString("CPF_CNPJ"),
                        RG_InsMunicipal = reader.GetString("RG_InsMunicipal"),
                        Email = reader.GetString("Email"),
                        Telefone = reader.GetString("Telefone"),
                        Endereco = reader.GetString("Endereco"),
                        Bairro = reader.GetString("Bairro"),
                        Cep = reader.GetString("Cep"),
                        Status = reader.GetString("Status"),
                        IdCidade = reader.GetInt32("IdCidade")
                    };
                }
            }

            return null;
        }

        public void Atualizar(Cliente cliente)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Cliente 
                               SET TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, DataNascimento_Fundacao = @Data, 
                                   CPF_CNPJ = @CPF_CNPJ, RG_InsMunicipal = @RG, Email = @Email, Telefone = @Telefone,
                                   Endereco = @Endereco, Bairro = @Bairro, Cep = @Cep, Status = @Status, IdCidade = @IdCidade
                               WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", cliente.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", cliente.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", cliente.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", cliente.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", cliente.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", cliente.Email);
                cmd.Parameters.AddWithValue("@Telefone", cliente.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", cliente.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", cliente.Bairro);
                cmd.Parameters.AddWithValue("@Cep", cliente.Cep);
                cmd.Parameters.AddWithValue("@Status", cliente.Status);
                cmd.Parameters.AddWithValue("@IdCidade", cliente.IdCidade);
                cmd.Parameters.AddWithValue("@Id", cliente.IdPessoa);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Cliente WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
