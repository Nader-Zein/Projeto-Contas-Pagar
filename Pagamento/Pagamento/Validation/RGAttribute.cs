using System.ComponentModel.DataAnnotations;

namespace Pagamento.Validation
{
    public class RGAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string rg = value.ToString().Trim();

            rg = rg.Replace(".", "").Replace("-", "");

            if (rg.Length < 5 || rg.Length > 14)
                return new ValidationResult("O RG informado é inválido.");

            if (!System.Text.RegularExpressions.Regex.IsMatch(rg, @"^[0-9]{1,13}[0-9Xx]?$"))
                return new ValidationResult("O RG informado possui caracteres inválidos.");

            return ValidationResult.Success;
        }
    }
}
