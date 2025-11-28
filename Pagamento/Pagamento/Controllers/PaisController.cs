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
            return View(new Pais());
        }

        [HttpPost]
        public IActionResult Criar(Pais pais)
        {
            if (_paisDAO.NomeExiste(pais.NomePais))
            {
                ModelState.AddModelError("NomePais", "Este país já está cadastrado!");
            }

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
            try
            {
                _paisDAO.Excluir(id);

                TempData["SuccessMessage"] = "Pais excluído com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este pais nao pode ser excluído,pois esta sendo utilizado em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir o pais.";
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
            return PartialView("FormPaisModal", new Pais());
        }

        [HttpPost]
        public IActionResult FormModal(Pais pais)
        {

            if (_paisDAO.NomeExiste(pais.NomePais))
            {
                ModelState.AddModelError("NomePais", "Este país já está cadastrado!");
            }

            if (ModelState.IsValid)
            {
                _paisDAO.Inserir(pais);
                return Json(new
                {
                    sucesso = true,
                    pais = new { id = pais.IdPais, nome = pais.NomePais.ToUpper() }
                });
            }

            return PartialView("FormPaisModal", pais);
        }

    }
}
