using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class ItemVenda
    {
        public string VendaModelo { get; set; }
        public string VendaSerie { get; set; }
        public int VendaNumeroNota { get; set; }
        public int VendaClienteId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Selecione um produto.")]
        public int ProdutoId { get; set; }

        public string? NomeProduto { get; set; }     
        public string? NomeUnidade { get; set; }     

        [Required(ErrorMessage = "A quantidade é obrigatória.")]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }

        [Required(ErrorMessage = "O valor unitário é obrigatório.")]
        public decimal ValorUnitario { get; set; }

        public decimal CustoUnitario { get; set; }

        public decimal Total => Quantidade * ValorUnitario;
    }
}
