using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Venda
    {
        [Required(ErrorMessage = "O campo Modelo é obrigatório.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O campo Serie é obrigatório.")]
        public string Serie { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O Número da Nota deve ser válido.")]
        public int NumeroNota { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar um Cliente.")]
        public int ClienteId { get; set; }

        public string? NomeCliente { get; set; }   
        public bool Status { get; set; }

        [Required(ErrorMessage = "A Data de Emissão é obrigatória.")]
        public DateTime DataEmissao { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar uma Condição de Pagamento.")]
        public int CondicaoPagamentoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar um Funcionário.")]
        public int FuncionarioId { get; set; }
        public string? NomeFuncionario { get; set; }   

        public string? Observacoes { get; set; }
        public string? Motivo_Cancelamento { get; set; }

        public DateTime DataCriacao { get; set; }

        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();

        public List<ContaAReceber> ParcelasGeradas { get; set; } = new List<ContaAReceber>();

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

        public decimal TotalNota => TotalProdutos;
    }
}
