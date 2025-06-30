using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;
using System.Linq;

namespace Pagamento.Controllers
{
    public class ProdutoController : Controller
    {
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly MarcaDAO _marcaDAO = new MarcaDAO();
        private readonly UnidadeMedidaDAO _unidadeMedidaDAO = new UnidadeMedidaDAO();
        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO(); 
        private readonly CategoriaDAO _categoriaDAO = new CategoriaDAO(); 

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

                    var produtoFornecedorDAO = new ProdutoFornecedorDAO();

                    foreach (var idFornecedor in fornecedores)
                    {
                        produtoFornecedorDAO.Inserir(produto.IdProduto, idFornecedor);
                    }
                }

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

            var produtoFornecedorDAO = new ProdutoFornecedorDAO();
            ViewBag.FornecedoresSelecionadosIds = produtoFornecedorDAO.ListarFornecedoresIds(id);

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeCategoria = _categoriaDAO.BuscarPorId(produto.CategoriaId)?.Descricao ?? "Não encontrado";

            return View(produto);
        }


        [HttpPost]
        public IActionResult Editar(Produto produto, string FornecedoresSelecionados)
        {
            if (ModelState.IsValid)
            {
                _produtoDAO.Atualizar(produto);

                var produtoFornecedorDAO = new ProdutoFornecedorDAO();

                produtoFornecedorDAO.RemoverTodos(produto.IdProduto);

                if (!string.IsNullOrEmpty(FornecedoresSelecionados))
                {
                    var fornecedores = FornecedoresSelecionados
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    foreach (var idFornecedor in fornecedores)
                    {
                        produtoFornecedorDAO.Inserir(produto.IdProduto, idFornecedor);
                    }
                }

                return RedirectToAction("Index");
            }

            CarregarSelectLists();

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeCategoria = _categoriaDAO.BuscarPorId(produto.CategoriaId)?.Descricao ?? "Não encontrado";

            return View(produto);
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
            _produtoDAO.Excluir(id);
            return RedirectToAction("Index");
        }

        private void CarregarSelectLists()
        {
            ViewBag.Marcas = _marcaDAO.Listar().Select(m => new SelectListItem
            {
                Value = m.IdMarca.ToString(),
                Text = m.Descricao
            }).ToList();

            ViewBag.Unidades = _unidadeMedidaDAO.Listar().Select(u => new SelectListItem
            {
                Value = u.IdUnidadeMedida.ToString(),
                Text = u.Descricao
            }).ToList();

            ViewBag.Fornecedores = _fornecedorDAO.Listar().Select(t => new SelectListItem
            {
                Value = t.IdPessoa.ToString(),
                Text = t.Nome_RazaoSocial
            }).ToList();

            ViewBag.Categorias = _categoriaDAO.Listar().Select(t => new SelectListItem
            {
                Value = t.IdCategoria.ToString(),
                Text = t.Descricao
            }).ToList();
        }
    }
}
