namespace Pagamento.Models
{
    public class FormaPagamento
    {
        public int IdFormaPgto { get; set; } 
        public string Descricao { get; set; }
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
