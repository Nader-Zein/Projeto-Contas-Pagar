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
        public IActionResult Criar(Produto produto)
        {
            if (ModelState.IsValid)
            {
                _produtoDAO.Inserir(produto);
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

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeFornecedor = _fornecedorDAO.BuscarPorId(produto.FornecedorId)?.Nome_RazaoSocial ?? "Não encontrado";

            return View(produto);
        }

        [HttpPost]
        public IActionResult Editar(Produto produto)
        {
            if (ModelState.IsValid)
            {
                _produtoDAO.Atualizar(produto);
                return RedirectToAction("Index");
            }

            CarregarSelectLists();

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "Não encontrado";
            ViewBag.NomeFornecedor = _fornecedorDAO.BuscarPorId(produto.FornecedorId)?.Nome_RazaoSocial ?? "Não encontrado";

            return View(produto);
        }

        public IActionResult Excluir(int id)
        {
            var produto = _produtoDAO.BuscarPorId(id);
            if (produto == null) return NotFound();

            ViewBag.NomeMarca = _marcaDAO.BuscarPorId(produto.MarcaId)?.Descricao ?? "";
            ViewBag.NomeUnidade = _unidadeMedidaDAO.BuscarPorId(produto.UnidadeMedidaId)?.Descricao ?? "";
            ViewBag.NomeFornecedor = _fornecedorDAO.BuscarPorId(produto.FornecedorId)?.Nome_RazaoSocial ?? "Não encontrado";

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
        }
    }
}
