using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pagamento.Models
{
    public class Compra
    {
        
        [Required(ErrorMessage = "O campo Modelo é obrigatório.")]
        public string Modelo { get; set; }

        [Required(ErrorMessage = "O campo Serie é obrigatório.")]
        public string Serie { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O Número da Nota deve ser válido.")]
        public int NumeroNota { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar um Fornecedor.")]
        public int FornecedorId { get; set; }

        public string? NomeFornecedor { get; set; }
        

        public bool Status { get; set; }

        [Required(ErrorMessage = "A Data de Emissão é obrigatória.")]
        public DateTime DataEmissao { get; set; }

        [Required(ErrorMessage = "A Data de Chegada é obrigatória.")]
        public DateTime DataChegada { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "É obrigatório selecionar uma Condição de Pagamento.")]
        public int CondicaoPagamentoId { get; set; }

        
        public decimal Frete { get; set; }
        public decimal Seguro { get; set; }
        public decimal Despesas { get; set; }

        public List<ItemCompra> Itens { get; set; } = new List<ItemCompra>();

        
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
        public decimal TotalNota => TotalProdutos + Frete + Seguro + Despesas;

        

        
        public string? Observacoes { get; set; } 

    }
}