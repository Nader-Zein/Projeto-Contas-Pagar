using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class CategoriaController : Controller
    {
        private readonly CategoriaDAO _categoriaDAO;

        public CategoriaController(CategoriaDAO categoriaDAO)
        {
            _categoriaDAO = categoriaDAO;
        }
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
            try
            {
                _categoriaDAO.Excluir(id);

                TempData["SuccessMessage"] = "Categoria excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Esta categoria nao pode ser excluída,pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a categoria.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult FormModal()
        {
            return PartialView("FormCategoriaModal", new Categoria());
        }

        [HttpPost]
        public IActionResult FormModal(Categoria categoria)
        {
            if (_categoriaDAO.ExisteCategoriao(categoria.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta categoria já está cadastrada!");
            }
            if (ModelState.IsValid)
            {
                _categoriaDAO.Inserir(categoria);
                return Json(new
                {
                    sucesso = true,
                    categoria = new { id = categoria.IdCategoria, nome = categoria.Descricao.ToUpper() }
                });
            }

            return PartialView("FormCategoriaModal", categoria);
        }
    }
}
