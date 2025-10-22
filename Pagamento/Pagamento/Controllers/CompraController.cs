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
        private readonly CompraDAO _compraDAO = new CompraDAO(); 



        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly MarcaDAO _marcaDAO = new MarcaDAO();
        private readonly UnidadeMedidaDAO _unidadeMedidaDAO = new UnidadeMedidaDAO();
        private readonly CategoriaDAO _categoriaDAO = new CategoriaDAO();
       

        public IActionResult Index()
        {
            try
            {
                var listaDeCompras = _compraDAO.Listar();
                return View(listaDeCompras); 
            }
            catch (Exception erro)
            {
                TempData["ErrorMessage"] = $"Não foi possível carregar a lista de compras. Erro: {erro.Message}";
                return View(new List<Compra>());
            }
        }

        public IActionResult Criar()
        {
            PreencherViewBags();


            var novaCompra = new Compra
            {
                DataEmissao = DateTime.Today,
                DataChegada = DateTime.Today,
                Status = true
                
            };
            return View(novaCompra);
        }

       

        private void PreencherViewBags()
        {
            var fornecedores = _fornecedorDAO.Listar();
            var produtos = _produtoDAO.Listar();
            var condicoes = _condicaoPagamentoDAO.Listar();

            
            ViewBag.Fornecedores = fornecedores ?? new List<Fornecedor>();
            ViewBag.Produtos = produtos ?? new List<Produto>();
            ViewBag.CondicoesPagamento = condicoes ?? new List<CondicaoPagamento>();

           
            var cidades = _cidadeDAO.Listar() ?? new List<Cidade>();
            var marcas = _marcaDAO.Listar() ?? new List<Marca>();
            var unidades = _unidadeMedidaDAO.Listar() ?? new List<UnidadeMedida>();
            var categorias = _categoriaDAO.Listar() ?? new List<Categoria>();

           
            ViewBag.CidadesItens = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();
            ViewBag.MarcasItens = marcas.Select(m => new SelectListItem(m.Descricao, m.IdMarca.ToString())).ToList();
            ViewBag.UnidadesItens = unidades.Select(u => new SelectListItem(u.Descricao, u.IdUnidadeMedida.ToString())).ToList();
            ViewBag.CategoriasItens = categorias.Select(cat => new SelectListItem(cat.Descricao, cat.IdCategoria.ToString())).ToList();

           
            ViewBag.FornecedoresItens = fornecedores.Select(f => new SelectListItem(f.Nome_RazaoSocial, f.IdPessoa.ToString())).ToList();
            ViewBag.CondicoesPagamentoItens = condicoes.Select(c => new SelectListItem(c.Descricao, c.IdCondPgto.ToString())).ToList();
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
                    
                    _compraDAO.Inserir(compra);

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
