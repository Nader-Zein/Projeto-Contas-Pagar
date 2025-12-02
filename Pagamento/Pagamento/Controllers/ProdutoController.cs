using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;
using System.Linq;
using System;      
using System.Collections.Generic;      
namespace Pagamento.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ProdutoDAO _produtoDAO;
        private readonly ProdutoFornecedorDAO _produtoFornecedorDAO;
        private readonly MarcaDAO _marcaDAO;
        private readonly UnidadeMedidaDAO _unidadeMedidaDAO;
        private readonly FornecedorDAO _fornecedorDAO;
        private readonly CategoriaDAO _categoriaDAO;

        private readonly CidadeDAO _cidadeDAO;
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO;
        private readonly EstadoDAO _estadoDAO;
        private readonly PaisDAO _paisDAO;
        private readonly FormaPagamentoDAO _formaPagamentoDAO;

        public ProdutoController(
            ProdutoDAO produtoDAO,
            ProdutoFornecedorDAO produtoFornecedorDAO,
            MarcaDAO marcaDAO,
            UnidadeMedidaDAO unidadeMedidaDAO,
            FornecedorDAO fornecedorDAO,
            CategoriaDAO categoriaDAO,
            CidadeDAO cidadeDAO,
            CondicaoPagamentoDAO condicaoPagamentoDAO,
            EstadoDAO estadoDAO,
            PaisDAO paisDAO,
            FormaPagamentoDAO formaPagamentoDAO)
        {
            _produtoDAO = produtoDAO;
            _produtoFornecedorDAO = produtoFornecedorDAO;
            _marcaDAO = marcaDAO;
            _unidadeMedidaDAO = unidadeMedidaDAO;
            _fornecedorDAO = fornecedorDAO;
            _categoriaDAO = categoriaDAO;
            _cidadeDAO = cidadeDAO;
            _condicaoPagamentoDAO = condicaoPagamentoDAO;
            _estadoDAO = estadoDAO;
            _paisDAO = paisDAO;
            _formaPagamentoDAO = formaPagamentoDAO;
        }

        public IActionResult Index()
        {
            var lista = _produtoDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            CarregarSelectLists();
            return View(new Produto());
        }

        [HttpPost]
        public IActionResult Criar(Produto produto, string FornecedoresSelecionados)
        {
           
            if (_produtoDAO.ProdutoDuplicado(produto.Descricao, produto.Codigo_Barras))
            {
                ModelState.AddModelError("Descricao", "Já existe um produto com esta descrição ou código de barras.");
            }

            if (string.IsNullOrWhiteSpace(FornecedoresSelecionados))
            {
                ModelState.AddModelError("FornecedoresSelecionados", "Selecione ao menos um fornecedor.");
            }
            if (produto.MarcaId == 0)
            {
                ModelState.AddModelError("MarcaId", "Selecione uma marca.");
            }
            if (produto.UnidadeMedidaId == 0)
            {
                ModelState.AddModelError("UnidadeMedidaId", "Selecione uma Unidade Medida.");
            }
            if (produto.CategoriaId == 0)
            {
                ModelState.AddModelError("CategoriaId", "Selecione uma Categoria.");
            }
            if (ModelState.IsValid)
            {
                _produtoDAO.Inserir(produto); 

                if (!string.IsNullOrEmpty(FornecedoresSelecionados))
                {
                    var fornecedores = FornecedoresSelecionados
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();


                    foreach (var idFornecedor in fornecedores)
                    {
                        _produtoFornecedorDAO.InserirOuAtualizarAssociacao(produto.IdProduto, idFornecedor,produto.Observacoes);
                    }
                }
                TempData["SuccessMessage"] = "Produto cadastrado com sucesso!";

                return RedirectToAction("Index");
            }

            CarregarSelectLists();
            return View(produto);
        }



        public IActionResult Editar(int id)
        {
            var produto = _produtoDAO.BuscarPorId(id);
            if (produto == null) return NotFound();

            CarregarSelectLists();

           
            ViewBag.FornecedoresSelecionadosIds = _produtoFornecedorDAO.ListarFornecedoresIds(id);

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeCategoria = _categoriaDAO.BuscarPorId(produto.CategoriaId)?.Descricao ?? "Não encontrado";


            var fornecedoresSelecionados = _fornecedorDAO.Listar()
                                        .Where(f => ViewBag.FornecedoresSelecionadosIds.Contains(f.IdPessoa))
                                        .Select(f => f.Nome_RazaoSocial);
            ViewBag.NomesFornecedoresSelecionados = string.Join(", ", fornecedoresSelecionados);
            return View(produto);
        }


        [HttpPost]
        public IActionResult Editar(Produto produto, string FornecedoresSelecionados)
        {
            if (!ModelState.IsValid)
            {
                CarregarSelectLists();

                ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
                ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
                ViewBag.NomeCategoria = _categoriaDAO.BuscarPorId(produto.CategoriaId)?.Descricao ?? "Não encontrado";

                var nomesFornecedores = _fornecedorDAO.Listar()
                    .Where(f => (FornecedoresSelecionados ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => int.TryParse(s, out var i) ? i : 0)
                        .Contains(f.IdPessoa))
                    .Select(f => f.Nome_RazaoSocial);

                ViewBag.NomesFornecedoresSelecionados = string.Join(", ", nomesFornecedores);
                return View(produto);
            }

            var fornecedores = (FornecedoresSelecionados ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var x) ? x : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            _produtoDAO.Atualizar(produto);

            if (fornecedores.Count == 0)
            {
                _produtoFornecedorDAO.RemoverTodos(produto.IdProduto);
            }
            else
            {
                _produtoFornecedorDAO.RemoverNaoSelecionados(produto.IdProduto, fornecedores);

                foreach (var idFornecedor in fornecedores)
                {
                    _produtoFornecedorDAO.InserirOuAtualizarAssociacao(produto.IdProduto, idFornecedor, produto.Observacoes);
                }
            }
            TempData["SuccessMessage"] = "Produto atualizado com sucesso!";

            return RedirectToAction("Index");
        }

        public IActionResult Excluir(int id)
        {
            var produto = _produtoDAO.BuscarPorId(id);
            if (produto == null) return NotFound();

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "";
            ViewBag.NomeCategoria = _categoriaDAO.BuscarPorId(produto.CategoriaId)?.Descricao ?? "Não encontrado";

            var fornecedores = _produtoDAO.BuscarFornecedoresPorProduto(id); 
            ViewBag.NomesFornecedores = string.Join(", ", fornecedores.Select(f => f.Nome_RazaoSocial));

            return View(produto);
        }


        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            try
            {
                _produtoDAO.Excluir(id);

                TempData["SuccessMessage"] = "Produto excluído com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este produto nao pode ser excluído,pois esta sendo utilizado em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir o produto.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("Index");
        }

        private void CarregarSelectLists()
        {
            var marcas = _marcaDAO.Listar() ?? new List<Marca>();
            var unidades = _unidadeMedidaDAO.Listar() ?? new List<UnidadeMedida>();
            var fornecedores = _fornecedorDAO.Listar() ?? new List<Fornecedor>();
            var categorias = _categoriaDAO.Listar() ?? new List<Categoria>();

            ViewBag.Marcas = marcas.Select(m => new SelectListItem(m.Descricao, m.IdMarca.ToString())).ToList();
            ViewBag.Unidades = unidades.Select(u => new SelectListItem(u.Descricao, u.IdUnidadeMedida.ToString())).ToList();
            ViewBag.Fornecedores = fornecedores.Select(f => new SelectListItem(f.Nome_RazaoSocial, f.IdPessoa.ToString())).ToList();
            ViewBag.Categorias = categorias.Select(c => new SelectListItem(c.Descricao, c.IdCategoria.ToString())).ToList();

            var cidades = _cidadeDAO.Listar() ?? new List<Cidade>();
            var condicoes = _condicaoPagamentoDAO.Listar() ?? new List<CondicaoPagamento>();
            var estados = _estadoDAO.Listar() ?? new List<Estado>();      
            var paises = _paisDAO.Listar() ?? new List<Pais>();            
            var formasPgto = _formaPagamentoDAO.Listar() ?? new List<FormaPagamento>();      

            ViewBag.Cidades = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();
            ViewBag.CondicoesPagamento = condicoes.Select(c => new SelectListItem(c.Descricao, c.IdCondPgto.ToString())).ToList();
            ViewBag.EstadosItens = estados.Select(e => new SelectListItem(e.NomeEstado, e.IdEstado.ToString())).ToList();
            ViewBag.PaisesItens = paises.Select(p => new SelectListItem(p.NomePais, p.IdPais.ToString())).ToList();
            ViewBag.FormasPagamentoItens = formasPgto.Select(f => new SelectListItem(f.Descricao, f.IdFormaPgto.ToString())).ToList();
        }


        public IActionResult FormModal()
        {
            CarregarSelectLists();
            return PartialView("FormProdutoModal", new Produto());
        }

        [HttpPost]
        public IActionResult FormModal(Produto produto, string fornecedoresSelecionados)
        {
            if (_produtoDAO.ProdutoDuplicado(produto.Descricao, produto.Codigo_Barras))
                ModelState.AddModelError("Descricao", "Produto já cadastrado.");
            if (string.IsNullOrWhiteSpace(fornecedoresSelecionados))
                ModelState.AddModelError("FornecedoresSelecionados", "Selecione ao menos um fornecedor.");
            if (produto.MarcaId == 0)
                ModelState.AddModelError("MarcaId", "Selecione uma marca.");
            if (produto.UnidadeMedidaId == 0)
                ModelState.AddModelError("UnidadeMedidaId", "Selecione uma unidade.");
            if (produto.CategoriaId == 0)
                ModelState.AddModelError("CategoriaId", "Selecione uma categoria.");

            if (ModelState.IsValid)
            {
                _produtoDAO.Inserir(produto);

                if (!string.IsNullOrEmpty(fornecedoresSelecionados))
                {
                    var fornecedorIds = fornecedoresSelecionados.Split(',').Select(int.Parse);
                    foreach (var fornecedorId in fornecedorIds)
                    {
                        _produtoFornecedorDAO.InserirOuAtualizarAssociacao(produto.IdProduto, fornecedorId,produto.Observacoes);
                    }
                }

                return Json(new
                {
                    sucesso = true,
                    produto = new
                    {
                        id = produto.IdProduto,
                        nome = produto.Descricao.ToUpper(),
                        unidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao,
                        valorVenda = produto.ValorVenda
                    }
                });
            }

            CarregarSelectLists();
            return PartialView("FormProdutoModal", produto);
        }


        [HttpGet]
        public IActionResult BuscarPorIdJSON(int id)
        {
            var produto = _produtoDAO.BuscarPorIdComNomes(id);        

            if (produto == null)
            {
                return NotFound(new { mensagem = "Produto não encontrado com o ID informado." });
            }

            return Json(new
            {
                idProduto = produto.IdProduto,
                descricao = produto.Descricao,
                nomeUnidade = produto.NomeUnidade,     
                valorVenda = produto.ValorVenda,
                estoque = produto.Quantidade
            });
        }
    }
}
