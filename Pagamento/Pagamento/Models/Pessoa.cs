using System;
using System.ComponentModel.DataAnnotations;
using Pagamento.Validation;

namespace Pagamento.Models
{
    public class Pessoa
    {
        public int IdPessoa { get; set; }

        [Required(ErrorMessage = "O tipo de pessoa é obrigatório.")]
        public string TipoPessoa { get; set; }  

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string Nome_RazaoSocial { get; set; }

        [Required(ErrorMessage = "Campo obrigatório.")]
        public DateTime DataNascimento_Fundacao { get; set; }

        [DocumentoCondicional("TipoPessoa", ErrorMessage = "CPF ou CNPJ inválido.")]
        public string? CPF_CNPJ { get; set; }

        [Required(ErrorMessage = "Campo obrigatório.")]
        public string RG_InsEstadual { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O endereço é obrigatório.")]
        public string Endereco { get; set; }

        [Required(ErrorMessage = "O bairro é obrigatório.")]
        public string Bairro { get; set; }

        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "O CEP deve conter 8 digitos.")]
        public string Cep { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public bool Status { get; set; }

        public string? Complemento { get; set; }

        [Required(ErrorMessage = "O número é obrigatório.")]
        public string Numero { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataEdicao { get; set; }

        [Required(ErrorMessage = "A cidade é obrigatória.")]
        public int IdCidade { get; set; }

        public string? NomeCidade { get; set; }

    }
}
