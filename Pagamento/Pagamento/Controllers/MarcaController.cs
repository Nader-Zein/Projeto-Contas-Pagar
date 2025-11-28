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
            try
            {
                _marcaDAO.Excluir(id);

                TempData["SuccessMessage"] = "Marca excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este marca nao pode ser excluída,pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a marca.";
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
            return PartialView("FormMarcaModal", new Marca());
        }

        [HttpPost]
        public IActionResult FormModal(Marca marca)
        {

            if (_marcaDAO.MarcaDuplicada(marca.Descricao))
            {
                ModelState.AddModelError("Descricao", "Já existe uma marca com essa descrição.");
            }

            if (ModelState.IsValid)
            {
                _marcaDAO.Inserir(marca);
                return Json(new
                {
                    sucesso = true,
                    marca = new { id = marca.IdMarca, nome = marca.Descricao.ToUpper() }
                });
            }

            return PartialView("FormMarcaModal", marca);
        }
    }
}
