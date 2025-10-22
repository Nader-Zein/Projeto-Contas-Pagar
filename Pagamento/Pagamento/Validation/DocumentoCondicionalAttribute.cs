using System;
using System.ComponentModel.DataAnnotations;

namespace Pagamento.Validation
{
    public class DocumentoCondicionalAttribute : ValidationAttribute
    {
        private readonly string _tipoPessoaProperty;

        public DocumentoCondicionalAttribute(string tipoPessoaProperty)
        {
            _tipoPessoaProperty = tipoPessoaProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            var tipoProp = context.ObjectType.GetProperty(_tipoPessoaProperty);
            if (tipoProp == null)
                return new ValidationResult($"Propriedade {_tipoPessoaProperty} não encontrada.");

            var tipoPessoa = tipoProp.GetValue(context.ObjectInstance)?.ToString();
            var documento = value?.ToString();

            if (string.IsNullOrWhiteSpace(documento))
                return ValidationResult.Success;

            if (tipoPessoa == "Física")
                return new CPFAttribute().GetValidationResult(value, context);

            if (tipoPessoa == "Jurídica")
                return new CNPJAttribute().GetValidationResult(value, context);

            return ValidationResult.Success;
        }
    }
}
