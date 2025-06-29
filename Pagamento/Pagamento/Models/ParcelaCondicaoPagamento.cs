namespace Pagamento.Models
{
    public class ParcelaCondicaoPagamento
    {
        public int IdCondPgto { get; set; } 
        public int NumeroParcela { get; set; } 
        public int IdFormaPgto { get; set; }
        public string? NomeFormaPagamento { get; set; }

        public decimal ValorPercentual { get; set; }
        public int DiasAposVenda { get; set; }
    }
}
