using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class ContaAReceber
    {
        public string? Modelo { get; set; }
        public string? Serie { get; set; }
        public int NumeroNota { get; set; }
        public int ClienteId { get; set; }
        public int NumeroParcela { get; set; }

        public string? NomeCliente { get; set; }
        public string? NomeFormaPgto { get; set; }

        public bool Status { get; set; }       
        public string? Situacao { get; set; }    

        [Required(ErrorMessage = "O valor da parcela é obrigatório.")]
        public decimal ValorParcela { get; set; }

        [Required(ErrorMessage = "A data de vencimento é obrigatória.")]
        public DateTime DataVencimento { get; set; }
        public DateTime DataEmissao { get; set; }

        public DateTime? DataPagamento { get; set; }
        public decimal? ValorPago { get; set; }

        public decimal? Juros { get; set; }
        public decimal? Multa { get; set; }
        public decimal? Desconto { get; set; }

        public decimal ValJuros { get; set; }
        public decimal ValMulta { get; set; }
        public decimal ValDesconto { get; set; }

        public int? IdFormaPgto { get; set; }

        public string? MotivoCancelamento { get; set; }
    }
}
