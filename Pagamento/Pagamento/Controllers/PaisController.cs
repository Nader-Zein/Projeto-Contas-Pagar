using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class PaisController : Controller
    {
        private readonly PaisDAO _paisDAO = new PaisDAO();

        public IActionResult Index()
        {
            var lista = _paisDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Criar(Pais pais)
        {
            if (ModelState.IsValid)
            {
                _paisDAO.Inserir(pais);
                return RedirectToAction("Index");
            }
            return View(pais);
        }

        public IActionResult Editar(int id)
        {
            var pais = _paisDAO.BuscarPorId(id);
            if (pais == null) return NotFound();
            return View(pais);
        }

        [HttpPost]
        public IActionResult Editar(Pais pais)
        {
            if (ModelState.IsValid)
            {
                _paisDAO.Atualizar(pais);
                return RedirectToAction("Index");
            }
            return View(pais);
        }

        public IActionResult Excluir(int id)
        {
            var pais = _paisDAO.BuscarPorId(id);
            if (pais == null) return NotFound();
            return View(pais);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _paisDAO.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}
