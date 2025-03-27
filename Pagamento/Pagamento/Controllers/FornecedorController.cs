using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();

        public IActionResult Index()
        {
            var lista = _fornecedorDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Criar(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _fornecedorDAO.Inserir(fornecedor);
                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(fornecedor);
        }

        public IActionResult Editar(int id)
        {
            var fornecedor = _fornecedorDAO.BuscarPorId(id);
            if (fornecedor == null) return NotFound();

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(fornecedor);
        }

        [HttpPost]
        public IActionResult Editar(Fornecedor fornecedor)
        {
            if (ModelState.IsValid)
            {
                _fornecedorDAO.Atualizar(fornecedor);
                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(fornecedor);
        }

        public IActionResult Excluir(int id)
        {
            var fornecedor = _fornecedorDAO.BuscarPorId(id);
            if (fornecedor == null) return NotFound();

            return View(fornecedor);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _fornecedorDAO.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}
