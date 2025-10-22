using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = " O descrição da categoria é obrigatória.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
