using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Venda
    {
        // --- Chave Primária Composta ---
        [Required(ErrorMessage = "O campo Modelo é obrigatório.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O campo Serie é obrigatório.")]
        public string Serie { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O Número da Nota deve ser válido.")]
        public int NumeroNota { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar um Cliente.")]
        public int ClienteId { get; set; }

        public string? NomeCliente { get; set; } // Para exibição
        // ---------------------------------

        public bool Status { get; set; }

        [Required(ErrorMessage = "A Data de Emissão é obrigatória.")]
        public DateTime DataEmissao { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar uma Condição de Pagamento.")]
        public int CondicaoPagamentoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar um Funcionário.")]
        public int FuncionarioId { get; set; }
        public string? NomeFuncionario { get; set; } // Para exibição

        public string? Observacoes { get; set; }
        public string? Motivo_Cancelamento { get; set; }

        public DateTime DataCriacao { get; set; }

        // --- Listas para manipulação na tela ---
        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();

        // Lista para carregar os detalhes das parcelas geradas automaticamente
        public List<ContaAReceber> ParcelasGeradas { get; set; } = new List<ContaAReceber>();

        // --- Propriedades Calculadas ---
        public decimal TotalProdutos
        {
            get
            {
                decimal total = 0;
                if (Itens != null)
                {
                    foreach (var item in Itens)
                    {
                        total += item.Total;
                    }
                }
                return total;
            }
        }

        // Na venda, se não houver frete/seguro, o Total da Nota é igual ao dos produtos
        public decimal TotalNota => TotalProdutos;
    }
}
