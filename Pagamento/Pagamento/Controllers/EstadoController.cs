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

            return View(new Estado());
        }

        [HttpPost]
        public IActionResult Criar(Estado estado)
        {
            if (estado.IdPais == 0)
            {
                ModelState.AddModelError("IdPais", "Selecione um Pais.");
            }

            if (_estadoDAO.ExisteEstadoPorNome(estado.NomeEstado))
            {
                ModelState.AddModelError("NomeEstado", "Este estado já está cadastrado!");
            }

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

            ViewBag.NomePais = _paisDAO.BuscarPorId(estado.IdPais)?.NomePais ?? "";


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

            ViewBag.NomePais = _paisDAO.BuscarPorId(estado.IdPais)?.NomePais ?? "";

            return View(estado);
        }

        public IActionResult Excluir(int id)
        {
            var estado = _estadoDAO.BuscarPorId(id);
            if (estado == null) return NotFound();

            ViewBag.NomePais = _paisDAO.BuscarPorId(estado.IdPais)?.NomePais ?? "";

            return View(estado);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _estadoDAO.Excluir(id);
            return RedirectToAction("Index");
        }

        public IActionResult FormModal()
        {
            ViewBag.Paises = _paisDAO.Listar().Select(p => new SelectListItem
            {
                Value = p.IdPais.ToString(),
                Text = p.NomePais
            }).ToList();

            return PartialView("FormEstadoModal", new Estado());
        }

        [HttpPost]
        public IActionResult FormModal(Estado estado)
        {

            if (ModelState.IsValid)
            {
                _estadoDAO.Inserir(estado);
                return Json(new
                {
                    sucesso = true,
                    estado = new { id = estado.IdEstado, nome = estado.NomeEstado }
                });
            }

            ViewBag.Paises = _paisDAO.Listar().Select(p => new SelectListItem
            {
                Value = p.IdPais.ToString(),
                Text = p.NomePais
            }).ToList();


            return PartialView("FormEstadoModal", estado);
        }

    }
}
