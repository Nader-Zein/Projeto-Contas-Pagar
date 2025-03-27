using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class EstadoController : Controller
    {
        private readonly EstadoDAO _estadoDAO = new EstadoDAO();
        private readonly PaisDAO _paisDAO = new PaisDAO();

        public IActionResult Index()
        {
            var lista = _estadoDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            var paises = _paisDAO.Listar();

            ViewBag.Paises = paises.Select(p => new SelectListItem
            {
                Value = p.IdPais.ToString(),  
                Text = p.NomePais 
            }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Criar(Estado estado)
        {
            if (ModelState.IsValid)
            {
                _estadoDAO.Inserir(estado);
                return RedirectToAction("Index");
            }

            ViewBag.Paises = _paisDAO.Listar().Select(p => new SelectListItem
            {
                Value = p.IdPais.ToString(),
                Text = p.NomePais
            }).ToList();

            return View(estado);
        }

        public IActionResult Editar(int id)
        {
            var estado = _estadoDAO.BuscarPorId(id);
            if (estado == null) return NotFound();

            ViewBag.Paises = _paisDAO.Listar().Select(p => new SelectListItem
            {
                Value = p.IdPais.ToString(),
                Text = p.NomePais
            }).ToList();

            return View(estado);
        }

        [HttpPost]
        public IActionResult Editar(Estado estado)
        {
            if (ModelState.IsValid)
            {
                _estadoDAO.Atualizar(estado);
                return RedirectToAction("Index");
            }
            ViewBag.Paises = _paisDAO.Listar();
            return View(estado);
        }

        public IActionResult Excluir(int id)
        {
            var estado = _estadoDAO.BuscarPorId(id);
            if (estado == null) return NotFound();
            return View(estado);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _estadoDAO.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}
