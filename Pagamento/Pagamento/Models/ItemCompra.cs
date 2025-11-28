namespace Pagamento.Models
{
    public class ItemCompra
    {
        public string CompraModelo { get; set; }
        public string CompraSerie { get; set; }
        public int CompraNumeroNota { get; set; }
        public int CompraFornecedorId { get; set; }
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

        public decimal CustoUnitarioReal { get; set; }

        public decimal Total => Quantidade * ValorUnitario;

        public string? NomeProduto { get; set; }

        public string? NomeUnidade { get; set; }
    }
}