using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Marca
    {
        public int IdMarca { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; }

        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataEdicao { get; set; }
    }
}
