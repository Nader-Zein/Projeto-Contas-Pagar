using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Funcionario : Pessoa
    {
        [Required(ErrorMessage = "A matrícula é obrigatória.")]
        public string Matricula { get; set; }

        [Required(ErrorMessage = "O cargo é obrigatório.")]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "O salário é obrigatório.")]
        public decimal Salario { get; set; }

        [Required(ErrorMessage = "O gênero é obrigatório.")]
        public string Genero { get; set; }

        [Required(ErrorMessage = "O turno é obrigatório.")]
        public string Turno { get; set; }

        [Required(ErrorMessage = "A carga horária é obrigatória.")]
        public int CargaHoraria { get; set; }

        [Required(ErrorMessage = "A data de admissão é obrigatória.")]
        public DateTime DataAdmissao { get; set; }

        
        public DateTime? DataDemissao { get; set; }
    }
}
