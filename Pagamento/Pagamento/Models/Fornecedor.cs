namespace Pagamento.Models
{
    public class Fornecedor : Pessoa
    {
        public int IdCondPgto { get; set; }

        public string? Apelido_NomeFantasia { get; set; }

        public decimal? LimiteCredito { get; set; }
    }
}
