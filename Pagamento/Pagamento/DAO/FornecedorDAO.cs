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
                        IdCidade = reader.GetInt32("IdCidade"),
                        InscricaoEstadual = reader.GetString("InscricaoEstadual"),
                        InscricaoSubstitutoTributario = reader.GetString("InscricaoSubstitutoTributario")
                    });
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
                                (TipoPessoa, Nome_RazaoSocial, DataNascimento_Fundacao, CPF_CNPJ, RG_InsMunicipal, Email, Telefone, Endereco, Bairro, Cep, Status, IdCidade, InscricaoEstadual, InscricaoSubstitutoTributario)
                               VALUES 
                                (@TipoPessoa, @Nome, @Data, @CPF_CNPJ, @RG, @Email, @Telefone, @Endereco, @Bairro, @Cep, @Status, @IdCidade, @IE, @IST)";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", fornecedor.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", fornecedor.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", fornecedor.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", fornecedor.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", fornecedor.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", fornecedor.Email);
                cmd.Parameters.AddWithValue("@Telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", fornecedor.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", fornecedor.Bairro);
                cmd.Parameters.AddWithValue("@Cep", fornecedor.Cep);
                cmd.Parameters.AddWithValue("@Status", fornecedor.Status);
                cmd.Parameters.AddWithValue("@IdCidade", fornecedor.IdCidade);
                cmd.Parameters.AddWithValue("@IE", fornecedor.InscricaoEstadual);
                cmd.Parameters.AddWithValue("@IST", fornecedor.InscricaoSubstitutoTributario);
                cmd.ExecuteNonQuery();
            }
        }

        public Fornecedor BuscarPorId(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Fornecedor WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Fornecedor
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
                        IdCidade = reader.GetInt32("IdCidade"),
                        InscricaoEstadual = reader.GetString("InscricaoEstadual"),
                        InscricaoSubstitutoTributario = reader.GetString("InscricaoSubstitutoTributario")
                    };
                }
            }

            return null;
        }

        public void Atualizar(Fornecedor fornecedor)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Fornecedor 
                               SET TipoPessoa = @TipoPessoa, Nome_RazaoSocial = @Nome, DataNascimento_Fundacao = @Data, 
                                   CPF_CNPJ = @CPF_CNPJ, RG_InsMunicipal = @RG, Email = @Email, Telefone = @Telefone,
                                   Endereco = @Endereco, Bairro = @Bairro, Cep = @Cep, Status = @Status, IdCidade = @IdCidade,
                                   InscricaoEstadual = @IE, InscricaoSubstitutoTributario = @IST
                               WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@TipoPessoa", fornecedor.TipoPessoa);
                cmd.Parameters.AddWithValue("@Nome", fornecedor.Nome_RazaoSocial);
                cmd.Parameters.AddWithValue("@Data", fornecedor.DataNascimento_Fundacao);
                cmd.Parameters.AddWithValue("@CPF_CNPJ", fornecedor.CPF_CNPJ);
                cmd.Parameters.AddWithValue("@RG", fornecedor.RG_InsMunicipal);
                cmd.Parameters.AddWithValue("@Email", fornecedor.Email);
                cmd.Parameters.AddWithValue("@Telefone", fornecedor.Telefone);
                cmd.Parameters.AddWithValue("@Endereco", fornecedor.Endereco);
                cmd.Parameters.AddWithValue("@Bairro", fornecedor.Bairro);
                cmd.Parameters.AddWithValue("@Cep", fornecedor.Cep);
                cmd.Parameters.AddWithValue("@Status", fornecedor.Status);
                cmd.Parameters.AddWithValue("@IdCidade", fornecedor.IdCidade);
                cmd.Parameters.AddWithValue("@IE", fornecedor.InscricaoEstadual);
                cmd.Parameters.AddWithValue("@IST", fornecedor.InscricaoSubstitutoTributario);
                cmd.Parameters.AddWithValue("@Id", fornecedor.IdPessoa);
                cmd.ExecuteNonQuery();
            }
        }

        public void Excluir(int id)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Fornecedor WHERE IdPessoa = @Id";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
