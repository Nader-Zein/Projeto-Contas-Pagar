using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class CondicaoPagamento
    {
        public int IdCondPgto { get; set; }

        public string Descricao { get; set; }

        public int QuantidadeParcelas { get; set; }

        public decimal Juros { get; set; }

        public decimal Multa { get; set; }

        public decimal Desconto { get; set; }

        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataEdicao { get; set; }

        public List<FormaPagamento> FormasPagamento { get; set; } = new List<FormaPagamento>();

        public List<ParcelaCondicaoPagamento> Parcelas { get; set; } = new List<ParcelaCondicaoPagamento>();
    }
}
