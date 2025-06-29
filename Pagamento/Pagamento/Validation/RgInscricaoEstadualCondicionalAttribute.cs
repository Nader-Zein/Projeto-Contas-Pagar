using System;
using System.ComponentModel.DataAnnotations;

namespace Pagamento.Validation
{
    public class RgInscricaoEstadualCondicionalAttribute : ValidationAttribute
    {
        private readonly string _tipoPessoaProperty;

        public RgInscricaoEstadualCondicionalAttribute(string tipoPessoaProperty)
        {
            _tipoPessoaProperty = tipoPessoaProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var tipoProp = context.ObjectType.GetProperty(_tipoPessoaProperty);
            if (tipoProp == null)
                return new ValidationResult($"Propriedade {_tipoPessoaProperty} não encontrada.");

            var tipoPessoa = tipoProp.GetValue(context.ObjectInstance)?.ToString();
            var doc = value?.ToString();

            if (string.IsNullOrWhiteSpace(doc))
                return ValidationResult.Success;

            if (tipoPessoa == "Física")
                return new RGAttribute().GetValidationResult(value, context);

            if (tipoPessoa == "Jurídica")
                return new InscricaoEstadualAttribute().GetValidationResult(value, context);

            return ValidationResult.Success;
        }
    }
}
