using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Cidade
    {
        public int IdCidade { get; set; }

        [Required(ErrorMessage = "Insira o nome da cidade.")]
        public string NomeCidade { get; set; }
        public bool Status { get; set; }

        public int IdEstado { get; set; }
        public string? NomeEstado { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
