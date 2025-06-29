using System.ComponentModel.DataAnnotations;
namespace Pagamento.Models

{
    public class Pais
    {
        [Required(ErrorMessage = "O país é obrigatório.")]
        public int IdPais { get; set; }
        public string NomePais { get; set; }
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }

    }
}
