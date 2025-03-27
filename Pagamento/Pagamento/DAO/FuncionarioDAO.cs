using MySql.Data.MySqlClient;
using Pagamento.Models;

namespace Pagamento.DAO
{
    public class FuncionarioDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Funcionario> Listar()
        {
            var lista = new List<Funcionario>();

            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Funcionario";
                var cmd = new MySqlCommand(sql, conexao);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Funcionario
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
                    });
                }
            }

            return lista;
        }

        public void Inserir(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Funcionario (TipoPessoa, Nome_RazaoSocial, DataNascimento_Fundacao, CPF_CNPJ, RG_InsMunicipal, Email, Telefone, Endereco, Bairro, Cep, Status, IdCidade)
                               VALUES (@TipoPessoa, @Nome, @Data, @CPF_CNPJ, @RG, @Email, @Telefone, @Endereco, @Bairro, @Cep, @Status, @IdCidade)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", funcionario.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", funcionario.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", funcionario.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", funcionario.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", funcionario.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", funcionario.Email);
                cmd.Parameters.AddWithValue("@Telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", funcionario.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", funcionario.Bairro);
                cmd.Parameters.AddWithValue("@Cep", funcionario.Cep);
                cmd.Parameters.AddWithValue("@Status", funcionario.Status);
                cmd.Parameters.AddWithValue("@IdCidade", funcionario.IdCidade);
                cmd.ExecuteNonQuery();
            }
        }

        public Funcionario BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Funcionario WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Funcionario
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

        public void Atualizar(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Funcionario 
                               SET TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, DataNascimento_Fundacao = @Data, 
                                   CPF_CNPJ = @CPF_CNPJ, RG_InsMunicipal = @RG, Email = @Email, Telefone = @Telefone,
                                   Endereco = @Endereco, Bairro = @Bairro, Cep = @Cep, Status = @Status, IdCidade = @IdCidade
                               WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", funcionario.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", funcionario.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", funcionario.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", funcionario.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", funcionario.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", funcionario.Email);
                cmd.Parameters.AddWithValue("@Telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", funcionario.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", funcionario.Bairro);
                cmd.Parameters.AddWithValue("@Cep", funcionario.Cep);
                cmd.Parameters.AddWithValue("@Status", funcionario.Status);
                cmd.Parameters.AddWithValue("@IdCidade", funcionario.IdCidade);
                cmd.Parameters.AddWithValue("@Id", funcionario.IdPessoa);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Funcionario WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
