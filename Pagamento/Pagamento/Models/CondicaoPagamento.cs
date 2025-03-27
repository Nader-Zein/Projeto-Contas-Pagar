namespace Pagamento.Models
{
    public class CondicaoPagamento
    {
        public int IdCondPgto { get; set; }
        public string Descricao { get; set; }
        public int QuantidadeParcelas { get; set; }
        public List<FormaPagamento> FormasPagamento { get; set; } = new List<FormaPagamento>();
        public List<ParcelaCondicaoPagamento> Parcelas { get; set; } = new List<ParcelaCondicaoPagamento>();

    }
}
