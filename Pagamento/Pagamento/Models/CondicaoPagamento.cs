using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class CondicaoPagamento
    {
        public int IdCondPgto { get; set; }

        [Required(ErrorMessage = "Insira a condição de pagamento.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "Insira a quantidade de parcelas.")]
        public int QuantidadeParcelas { get; set; }

        [Required(ErrorMessage = "Insira os juros.")]
        public decimal Juros { get; set; }

        [Required(ErrorMessage = "Insira a multa.")]
        public decimal Multa { get; set; }

        [Required(ErrorMessage = "Insira o desconto.")]
        public decimal Desconto { get; set; }

        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataEdicao { get; set; }

        public List<FormaPagamento> FormasPagamento { get; set; } = new List<FormaPagamento>();

        public List<ParcelaCondicaoPagamento> Parcelas { get; set; } = new List<ParcelaCondicaoPagamento>();
    }
}
