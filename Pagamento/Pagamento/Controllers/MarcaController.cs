using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class MarcaController : Controller
    {
        private readonly MarcaDAO _marcaDAO = new MarcaDAO();

        public IActionResult Index()
        {
            var lista = _marcaDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            return View(new Marca());
        }

        [HttpPost]
        public IActionResult Criar(Marca marca)
        {
            if (_marcaDAO.MarcaDuplicada(marca.Descricao))
            {
                ModelState.AddModelError("Descricao", "Já existe uma marca com essa descrição.");
            }
            if (ModelState.IsValid)
            {
                _marcaDAO.Inserir(marca);
                return RedirectToAction("Index");
            }
            return View(marca);
        }

        public IActionResult Editar(int id)
        {
            var marca = _marcaDAO.BuscarPorId(id);
            if (marca == null) return NotFound();
            return View(marca);
        }

        [HttpPost]
        public IActionResult Editar(Marca marca)
        {
            if (ModelState.IsValid)
            {
                _marcaDAO.Atualizar(marca);
                return RedirectToAction("Index");
            }
            return View(marca);
        }

        public IActionResult Excluir(int id)
        {
            var marca = _marcaDAO.BuscarPorId(id);
            if (marca == null) return NotFound();
            return View(marca);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _marcaDAO.Excluir(id);
            return RedirectToAction("Index");
        }

        public IActionResult FormModal()
        {
            return PartialView("FormMarcaModal", new Marca());
        }

        [HttpPost]
        public IActionResult FormModal(Marca marca)
        {
            if (ModelState.IsValid)
            {
                _marcaDAO.Inserir(marca);
                return Json(new
                {
                    sucesso = true,
                    marca = new { id = marca.IdMarca, nome = marca.Descricao }
                });
            }

            return PartialView("FormMarcaModal", marca);
        }
    }
}
