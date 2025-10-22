using System;
using System.Collections.Generic;

namespace Pagamento.Models
{
    public class Compra
    {
        public string Modelo { get; set; }
        public string Serie { get; set; }
        public int NumeroNota { get; set; }
        public int FornecedorId { get; set; }

        public bool Status { get; set; }

        public DateTime DataEmissao { get; set; }
        public DateTime DataChegada { get; set; }

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
    }
}