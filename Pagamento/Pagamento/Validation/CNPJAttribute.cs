using System.ComponentModel.DataAnnotations;

namespace Pagamento.Validation
{
    public class CNPJAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var cnpj = value as string;

            if (string.IsNullOrWhiteSpace(cnpj) || !ValidacoesDocumento.ValidarCNPJ(cnpj))
            {
                return new ValidationResult(ErrorMessage ?? "CNPJ inválido.");
            }

            return ValidationResult.Success;
        }
    }
}
