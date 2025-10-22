using System.ComponentModel.DataAnnotations;
namespace Pagamento.Models

{
    public class Pais
    {
        public int IdPais { get; set; }

        [Required(ErrorMessage = "Insira o nome do pais.")]
        public string NomePais { get; set; }
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }

    }
}
