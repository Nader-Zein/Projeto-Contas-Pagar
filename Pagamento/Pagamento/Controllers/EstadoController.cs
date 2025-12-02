using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class EstadoController : Controller
    {
        private readonly EstadoDAO _estadoDAO;
        private readonly PaisDAO _paisDAO;

        public EstadoController(EstadoDAO estadoDAO, PaisDAO paisDAO)
        {
            _estadoDAO = estadoDAO;
            _paisDAO = paisDAO;
        }
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
                TempData["SuccessMessage"] = "Estado cadastrado com sucesso!";

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
                TempData["SuccessMessage"] = "Estado editado com sucesso!";

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
            try
            {
                _estadoDAO.Excluir(id);

                TempData["SuccessMessage"] = "Estado  excluído com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este estado nao pode ser excluído,pois esta sendo utilizado em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir o estado.";
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
            if (_estadoDAO.ExisteEstadoPorNome(estado.NomeEstado))
            {
                ModelState.AddModelError("NomeEstado", "Este estado já está cadastrado!");
            }

            if (ModelState.IsValid)
            {
                _estadoDAO.Inserir(estado);
                return Json(new
                {
                    sucesso = true,
                    estado = new { id = estado.IdEstado, nome = estado.NomeEstado.ToUpper() }
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
