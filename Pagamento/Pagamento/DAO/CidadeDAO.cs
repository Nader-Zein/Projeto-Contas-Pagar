using MySql.Data.MySqlClient;
using Pagamento.Models;
using System;
using System.Collections.Generic;

namespace Pagamento.DAO
{
    public class CidadeDAO
    {
        private string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        public List<Cidade> Listar()
        {
            List<Cidade> lista = new List<Cidade>();

            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                string sql = "SELECT * FROM Cidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new Cidade
                    {
                        IdCidade = reader.GetInt32("IdCidade"),
                        NomeCidade = reader.GetString("NomeCidade"),
                        IdEstado = reader.GetInt32("IdEstado"),
                        Status = reader.GetBoolean("Status"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
                    });
                }

                reader.Close();

                string sqlView = "SELECT IdCidade, NomeEstado FROM vw_CidadeEstadoPais";
                MySqlCommand cmdView = new MySqlCommand(sqlView, conexao);
                MySqlDataReader viewReader = cmdView.ExecuteReader();

                var nomesEstados = new Dictionary<int, string>();

                while (viewReader.Read())
                {
                    int idCidade = viewReader.GetInt32("IdCidade");
                    string nomeEstado = viewReader.GetString("NomeEstado");

                    nomesEstados[idCidade] = nomeEstado;
                }

                viewReader.Close();

                foreach (var cidade in lista)
                {
                    if (nomesEstados.TryGetValue(cidade.IdCidade, out string nomeEstado))
                    {
                        cidade.NomeEstado = nomeEstado;
                    }
                }
            }

            return lista;
        }

        public void Inserir(Cidade cidade)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"INSERT INTO Cidade 
                               (NomeCidade, Status, IdEstado, DataCriacao) 
                               VALUES 
                               (@NomeCidade, @Status, @IdEstado, @DataCriacao)";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeCidade", cidade.NomeCidade.ToUpper());
                cmd.Parameters.AddWithValue("@Status", cidade.Status);
                cmd.Parameters.AddWithValue("@IdEstado", cidade.IdEstado);
                cmd.Parameters.AddWithValue("@DataCriacao", DateTime.Now);
                cmd.ExecuteNonQuery();

                cidade.IdCidade = (int)cmd.LastInsertedId;

            }
        }

        public void Excluir(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "DELETE FROM Cidade WHERE IdCidade = @IdCidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCidade", id);
                cmd.ExecuteNonQuery();
            }
        }

        public void Atualizar(Cidade cidade)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"UPDATE Cidade 
                               SET NomeCidade = @NomeCidade, 
                                   Status = @Status, 
                                   IdEstado = @IdEstado, 
                                   DataEdicao = @DataEdicao 
                               WHERE IdCidade = @IdCidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@NomeCidade", cidade.NomeCidade.ToUpper());
                cmd.Parameters.AddWithValue("@Status", cidade.Status);
                cmd.Parameters.AddWithValue("@IdEstado", cidade.IdEstado);
                cmd.Parameters.AddWithValue("@DataEdicao", DateTime.Now);
                cmd.Parameters.AddWithValue("@IdCidade", cidade.IdCidade);
                cmd.ExecuteNonQuery();
            }
        }

        public Cidade BuscarPorId(int id)
        {
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = "SELECT * FROM Cidade WHERE IdCidade = @IdCidade";
                MySqlCommand cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCidade", id);
                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Cidade
                    {
                        IdCidade = reader.GetInt32("IdCidade"),
                        NomeCidade = reader.GetString("NomeCidade"),
                        Status = reader.GetBoolean("Status"),
                        IdEstado = reader.GetInt32("IdEstado"),
                        DataCriacao = reader.GetDateTime("DataCriacao"),
                        DataEdicao = reader.IsDBNull(reader.GetOrdinal("DataEdicao"))
                                     ? (DateTime?)null
                                     : reader.GetDateTime("DataEdicao")
                    };
                }
            }

            return null;
        }

        public bool CidadeEstrangeira(int idCidade)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"SELECT NomePais 
                       FROM vw_CidadeEstadoPais 
                       WHERE IdCidade = @IdCidade";
                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@IdCidade", idCidade);

                var resultado = cmd.ExecuteScalar();
                if (resultado != null)
                {
                    string nomePais = resultado.ToString().Trim().ToUpper();
                    return nomePais != "BRASIL";
                }

                return false; 
            }
        }


    }
}
