using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Estado
    {
        public int IdEstado{ get; set; }
        public string NomeEstado { get; set; }
        public bool Status { get; set; }
        public string Uf {  get; set; }
        public int IdPais { get; set; }
        public string? NomePais { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime? DataEdicao { get; set; }
    }
}
