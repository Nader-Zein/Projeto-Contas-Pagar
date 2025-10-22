using System.ComponentModel.DataAnnotations;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Net.Mime.MediaTypeNames;

namespace Pagamento.Models
{
    public class Produto
    {
        
        public int IdProduto { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O codigo de  barra é obrigatório.")]
        public string Codigo_Barras { get; set; }

        [Required(ErrorMessage = "A referencia é obrigatória.")]
        public string Referencia { get; set; }

        public int MarcaId { get; set; }
        public string? NomeMarca { get; set; }

        public int UnidadeMedidaId { get; set; }
        public string? NomeUnidade { get; set; }

        

        public int CategoriaId { get; set; }
        public string? NomeCategoria { get; set; }

        public decimal ValorCompra { get; set; }

        [Required(ErrorMessage = "O valor de venda é obrigatório.")]
        public decimal ValorVenda { get; set; }

        public int Quantidade { get; set; }

        [Required(ErrorMessage = "A quantidade minima é obrigatória.")]
        public int QuantidadeMinima { get; set; }

        [Required(ErrorMessage = "O percentual de lucro é obrigatório.")]
        public decimal PercentualLucro { get; set; }

        [StringLength(255)]
        public string? Observacoes { get; set; }

        [Required(ErrorMessage = "O status é obrigatório.")]
        public bool Status { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataEdicao { get; set; }
    }
}
