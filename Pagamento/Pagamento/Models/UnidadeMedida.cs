using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class UnidadeMedida
    {
        public int IdUnidadeMedida { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }

    }
}
