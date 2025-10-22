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
                        IdPessoa = reader.GetInt32("IdFuncionario"),
                        TipoPessoa = reader.GetString("TipoPessoa"),
                        Nome_RazaoSocial = reader.GetString("Nome_RazaoSocial"),
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
                        Genero = reader.GetString("Genero"),
                        Status = reader.GetBoolean("Status"),
                        IdCidade = reader.GetInt32("IdCidade"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? null : reader.GetDateTime("DataEdicao"),
                        Matricula = reader.GetString("Matricula"),
                        Cargo = reader.GetString("Cargo"),
                        Salario = reader.GetDecimal("Salario"),
                        Turno = reader.GetString("Turno"),
                        CargaHoraria = reader.GetInt32("CargaHoraria"),
                        DataAdmissao = reader.GetDateTime("DataAdmissao"),
                        DataDemissao = reader.IsDBNull(reader.GetOrdinal("DataDemissao")) ? null : reader.GetDateTime("DataDemissao")
                    });
                }
                reader.Close();

                string sqlView = "SELECT IdFuncionario, NomeCidade FROM vw_funcionario_cidade";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var nomesCidade = new Dictionary<int, string>();

                while (viewReader.Read())
                {
                    int id = viewReader.GetInt32("IdFuncionario");
                    string nomeCidade = viewReader.GetString("NomeCidade");

                    nomesCidade[id] = nomeCidade;
                }

                viewReader.Close();

                foreach (var funcionario in lista)
                {
                    if (nomesCidade.TryGetValue(funcionario.IdPessoa, out string nomeCidade))
                    {
                        funcionario.NomeCidade = nomeCidade;
                    }
                }
            }

            return lista;
        }

        public void Inserir(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Funcionario 
                               (TipoPessoa, Nome_RazaoSocial, DataNascimento_Fundacao, CPF_CNPJ, RG_InsEstadual, Email, Telefone,
                                Endereco, Numero, Complemento, Bairro, Cep, Genero, Status, IdCidade, DataCriacao,
                                Matricula, Cargo, Salario, Turno, CargaHoraria, DataAdmissao, DataDemissao)
                               VALUES 
                               (@TipoPessoa, @Nome, @Data, @CPF_CNPJ, @RG_InsEstadual, @Email, @Telefone,
                                @Endereco, @Numero, @Complemento, @Bairro, @Cep, @Genero, @Status, @IdCidade, @DataCriacao,
                                @Matricula, @Cargo, @Salario, @Turno, @CargaHoraria, @DataAdmissao, @DataDemissao)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", funcionario.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", funcionario.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Data", funcionario.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", funcionario.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", funcionario.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", funcionario.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", funcionario.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", funcionario.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(funcionario.Complemento) ? DBNull.Value : funcionario.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", funcionario.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", funcionario.Cep);
                cmd.Parameters.AddWithValue("@Genero", funcionario.Genero);
                cmd.Parameters.AddWithValue("@Status", funcionario.Status);
                cmd.Parameters.AddWithValue("@IdCidade", funcionario.IdCidade);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Matricula", funcionario.Matricula.ToUpper());
                cmd.Parameters.AddWithValue("@Cargo", funcionario.Cargo.ToUpper());
                cmd.Parameters.AddWithValue("@Salario", funcionario.Salario);
                cmd.Parameters.AddWithValue("@Turno", funcionario.Turno.ToUpper());
                cmd.Parameters.AddWithValue("@CargaHoraria", funcionario.CargaHoraria);
                cmd.Parameters.AddWithValue("@DataAdmissao", funcionario.DataAdmissao);
                cmd.Parameters.AddWithValue("@DataDemissao", (object?)funcionario.DataDemissao ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Funcionario funcionario)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Funcionario 
                               SET TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, DataNascimento_Fundacao = @Data, 
                                   CPF_CNPJ = @CPF_CNPJ, RG_InsEstadual = @RG_InsEstadual, Email = @Email, Telefone = @Telefone,
                                   Endereco = @Endereco, Numero = @Numero, Complemento = @Complemento, Bairro = @Bairro,
                                   Cep = @Cep, Genero = @Genero, Status = @Status, IdCidade = @IdCidade,
                                   DataEdicao = @DataEdicao, Matricula = @Matricula, Cargo = @Cargo, Salario = @Salario,
                                   Turno = @Turno, CargaHoraria = @CargaHoraria,
                                   DataAdmissao = @DataAdmissao, DataDemissao = @DataDemissao
                               WHERE IdFuncionario = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", funcionario.TipoPessoa.ToUpper());
                cmd.Parameters.AddWithValue("@Nome", funcionario.Nome_RazaoSocial.ToUpper());
                cmd.Parameters.AddWithValue("@Data", funcionario.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", funcionario.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG_InsEstadual", funcionario.RG_InsEstadual);
                cmd.Parameters.AddWithValue("@Email", funcionario.Email.ToUpper());
                cmd.Parameters.AddWithValue("@Telefone", funcionario.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", funcionario.Endereco.ToUpper());
                cmd.Parameters.AddWithValue("@Numero", funcionario.Numero);
                cmd.Parameters.AddWithValue("@Complemento", string.IsNullOrWhiteSpace(funcionario.Complemento) ? DBNull.Value : funcionario.Complemento.ToUpper());
                cmd.Parameters.AddWithValue("@Bairro", funcionario.Bairro.ToUpper());
                cmd.Parameters.AddWithValue("@Cep", funcionario.Cep);
                cmd.Parameters.AddWithValue("@Genero", funcionario.Genero);
                cmd.Parameters.AddWithValue("@Status", funcionario.Status);
                cmd.Parameters.AddWithValue("@IdCidade", funcionario.IdCidade);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@Matricula", funcionario.Matricula.ToUpper());
                cmd.Parameters.AddWithValue("@Cargo", funcionario.Cargo.ToUpper());
                cmd.Parameters.AddWithValue("@Salario", funcionario.Salario);
                cmd.Parameters.AddWithValue("@Turno", funcionario.Turno.ToUpper());
                cmd.Parameters.AddWithValue("@CargaHoraria", funcionario.CargaHoraria);
                cmd.Parameters.AddWithValue("@DataAdmissao", funcionario.DataAdmissao);
                cmd.Parameters.AddWithValue("@DataDemissao", (object?)funcionario.DataDemissao ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", funcionario.IdPessoa);
                cmd.ExecuteNonQuery();
            }
        }

        public Funcionario BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Funcionario WHERE IdFuncionario = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Funcionario
                    {
                        IdPessoa = reader.GetInt32("IdFuncionario"),
                        TipoPessoa = reader.GetString("TipoPessoa"),
                        Nome_RazaoSocial = reader.GetString("Nome_RazaoSocial"),
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
                        Genero = reader.GetString("Genero"),
                        Status = reader.GetBoolean("Status"),
                        IdCidade = reader.GetInt32("IdCidade"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao")) ? null : reader.GetDateTime("DataEdicao"),
                        Matricula = reader.GetString("Matricula"),
                        Cargo = reader.GetString("Cargo"),
                        Salario = reader.GetDecimal("Salario"),
                        Turno = reader.GetString("Turno"),
                        CargaHoraria = reader.GetInt32("CargaHoraria"),
                        DataAdmissao = reader.GetDateTime("DataAdmissao"),
                        DataDemissao = reader.IsDBNull(reader.GetOrdinal("DataDemissao")) ? null : reader.GetDateTime("DataDemissao")
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
                var cmd = new MySqlCommand("DELETE FROM Funcionario WHERE IdFuncionario = @Id", conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }

        public bool ExisteCpfCnpj(string cpfCnpj)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT COUNT(*) FROM Funcionario WHERE CPF_CNPJ = @CpfCnpj";
                using (var cmd = new MySqlCommand(sql, conexao))
                {
                    cmd.Parameters.AddWithValue("@CpfCnpj", cpfCnpj.Trim());
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
