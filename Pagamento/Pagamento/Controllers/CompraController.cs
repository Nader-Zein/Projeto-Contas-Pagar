using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;
using System;
using System.Collections.Generic;
using System.Text.Json; 

namespace Pagamento.Controllers
{
    public class CompraController : Controller
    {
        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();

        public IActionResult Index()
        {
            
            return View(new List<Compra>()); 
        }

        public IActionResult Criar()
        {
            PreencherViewBags();

            return View(new Compra());
        }

        

        private void PreencherViewBags()
        {
            var fornecedores = _fornecedorDAO.Listar();
            var produtos = _produtoDAO.Listar();
            var condicoes = _condicaoPagamentoDAO.Listar();

            ViewBag.Fornecedores = fornecedores ?? new List<Fornecedor>();
            ViewBag.Produtos = produtos ?? new List<Produto>();
            ViewBag.CondicoesPagamento = condicoes ?? new List<CondicaoPagamento>();
        }



        [HttpPost]
        public IActionResult Criar(Compra compra, string itensJson)
        {
            try
            {
                if (!string.IsNullOrEmpty(itensJson))
                {
                    compra.Itens = JsonSerializer.Deserialize<List<ItemCompra>>(itensJson);
                }

                if (compra.Itens == null || !compra.Itens.Any())
                {
                    ModelState.AddModelError("", "A compra deve ter pelo menos um item.");
                }

                if (ModelState.IsValid)
                {
                   

                    return RedirectToAction("Index"); 
                }
            }
            catch (Exception erro)
            {
                ModelState.AddModelError("", "Ocorreu um erro ao salvar a compra: " + erro.Message);
            }

            PreencherViewBags();
            return View(compra);
        }



    }
}
