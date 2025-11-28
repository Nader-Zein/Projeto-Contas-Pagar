using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pagamento.Models
{
    public class ContaAPagar
    {
        
        [Required(ErrorMessage = "O Modelo da nota é obrigatório.")]
        [StringLength(10)]
        public string Modelo { get; set; }

        
        [Required(ErrorMessage = "A Série da nota é obrigatória.")]
        [StringLength(10)]
        public string Serie { get; set; }

        
        [Required(ErrorMessage = "O Número da Nota é obrigatório.")]
        public int NumeroNota { get; set; }

        
        [Required(ErrorMessage = "O ID do Fornecedor é obrigatório.")]
        public int FornecedorId { get; set; }

        public string? NomeFornecedor { get; set; }

        
        [Required(ErrorMessage = "O Número da Parcela é obrigatório.")]
        [Range(1, int.MaxValue, ErrorMessage = "Número da parcela inválido.")]
        public int NumeroParcela { get; set; }

        [Required(ErrorMessage = "O Status é obrigatório.")]
        
        public Boolean Status { get; set; } 

        public string? Situacao { get; set; }

        [Required(ErrorMessage = "O Valor da Parcela é obrigatório.")]
        [Column(TypeName = "decimal(10, 2)")]      
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor da parcela deve ser positivo.")]
        public decimal ValorParcela { get; set; }

        [Required(ErrorMessage = "A Data de Vencimento é obrigatória.")]
        [DataType(DataType.Date)]      
        public DateTime DataVencimento { get; set; }

        [Required(ErrorMessage = "A Data de Emissão (referência) é obrigatória.")]
        [DataType(DataType.Date)]
        public DateTime DataEmissao { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DataPagamento { get; set; }   

        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Juros não podem ser negativos.")]
        public decimal? Juros { get; set; }   

        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Multa não pode ser negativa.")]
        public decimal? Multa { get; set; }   

        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Desconto não pode ser negativo.")]
        public decimal? Desconto { get; set; }   

        [Column(TypeName = "decimal(10, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor pago deve ser positivo (se informado).")]
        public decimal? ValorPago { get; set; }   

        public decimal ValJuros { get; set; }
        public decimal ValMulta { get; set; }
        public decimal ValDesconto { get; set; }

        public int? IdFormaPgto { get; set; }   

        public string? NomeFormaPgto { get; set; }

        public string? Motivo_Cancelamento { get; set; }
    }
}
