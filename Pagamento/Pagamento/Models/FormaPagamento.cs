using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class FormaPagamento
    {
        public int IdFormaPgto { get; set; }
        [Required(ErrorMessage = "Insira a forma de pagamento.")]
        public string Descricao { get; set; }
        public bool Status { get; set; } = true;

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
