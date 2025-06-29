using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Pagamento.Validation
{
    public class InscricaoEstadualAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var inscricao = value?.ToString();

            if (string.IsNullOrWhiteSpace(inscricao))
                return ValidationResult.Success; 

            inscricao = Regex.Replace(inscricao, @"[^a-zA-Z0-9]", "");

            if (inscricao.Length < 5 || inscricao.Length > 20)
                return new ValidationResult("Inscrição Estadual deve ter entre 5 e 20 caracteres.");

            if (!Regex.IsMatch(inscricao, @"^[a-zA-Z0-9]+$"))
                return new ValidationResult("Inscrição Estadual inválida. Use apenas letras e números.");

            return ValidationResult.Success;
        }
    }
}
