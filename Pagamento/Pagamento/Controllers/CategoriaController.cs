using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly CategoriaDAO _categoriaDAO = new CategoriaDAO();

        public IActionResult Index()
        {
            var lista = _categoriaDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            return View(new Categoria());
        }

        [HttpPost]
        public IActionResult Criar(Categoria categoria)
        {
            if (_categoriaDAO.ExisteCategoriao(categoria.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta categoria já está cadastrada!");
                return View(categoria);
            }
            if (ModelState.IsValid)
            {
                _categoriaDAO.Inserir(categoria);
                return RedirectToAction("Index");
            }
            return View(categoria);
        }

        public IActionResult Editar(int id)
        {
            var categoria = _categoriaDAO.BuscarPorId(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost]
        public IActionResult Editar(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _categoriaDAO.Atualizar(categoria);
                return RedirectToAction("Index");
            }
            return View(categoria);
        }

        public IActionResult Excluir(int id)
        {
            var categoria = _categoriaDAO.BuscarPorId(id);
            if (categoria == null) return NotFound();
            return View(categoria);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _categoriaDAO.Excluir(id);
            return RedirectToAction("Index");
        }

        public IActionResult FormModal()
        {
            return PartialView("FormCategoriaModal", new Categoria());
        }

        [HttpPost]
        public IActionResult FormModal(Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                _categoriaDAO.Inserir(categoria);
                return Json(new
                {
                    sucesso = true,
                    categoria = new { id = categoria.IdCategoria, nome = categoria.Descricao }
                });
            }

            return PartialView("FormCategoriaModal", categoria);
        }
    }
}
