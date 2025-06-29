using System.ComponentModel.DataAnnotations;

namespace Pagamento.Validation
{
    public class CPFAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var cpf = value as string;

            if (string.IsNullOrWhiteSpace(cpf) || !ValidacoesDocumento.ValidarCPF(cpf))
            {
                return new ValidationResult(ErrorMessage ?? "CPF inválido.");
            }

            return ValidationResult.Success;
        }
    }
}
