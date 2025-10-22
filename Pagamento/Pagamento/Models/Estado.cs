using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Estado
    {
        public int IdEstado{ get; set; }
        [Required(ErrorMessage = "Insira o nome do estado.")]
        public string NomeEstado { get; set; }
        public bool Status { get; set; }
        [Required(ErrorMessage = "Insira a unidade federativa.")]
        public string Uf {  get; set; }
        public int IdPais { get; set; }
        public string? NomePais { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
