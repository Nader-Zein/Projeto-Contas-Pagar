using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class UnidadeMedidaController : Controller
    {
        private readonly UnidadeMedidaDAO _unidadeDAO = new UnidadeMedidaDAO();

        public IActionResult Index()
        {
            var lista = _unidadeDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            return View(new UnidadeMedida());
        }

        [HttpPost]
        public IActionResult Criar(UnidadeMedida unidade)
        {

            if (_unidadeDAO.UnidadeDuplicada(unidade.Descricao))
            {
                ModelState.AddModelError("Descricao", "Já existe uma unidade de medida com essa descrição.");
            }
            if (ModelState.IsValid)
            {
                _unidadeDAO.Inserir(unidade);
                return RedirectToAction("Index");
            }
            return View(unidade);
        }

        public IActionResult Editar(int id)
        {
            var unidade = _unidadeDAO.BuscarPorId(id);
            if (unidade == null) return NotFound();
            return View(unidade);
        }

        [HttpPost]
        public IActionResult Editar(UnidadeMedida unidade)
        {
            if (ModelState.IsValid)
            {
                _unidadeDAO.Atualizar(unidade);
                return RedirectToAction("Index");
            }
            return View(unidade);
        }

        public IActionResult Excluir(int id)
        {
            var unidade = _unidadeDAO.BuscarPorId(id);
            if (unidade == null) return NotFound();
            return View(unidade);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            try
            {
                _unidadeDAO.Excluir(id);

                TempData["SuccessMessage"] = "Unidade de medida excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este unidade de medida nao pode ser excluída,pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a unidade de medida.";
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
            return PartialView("FormUnidadeModal", new UnidadeMedida());
        }

        [HttpPost]
        public IActionResult FormModal(UnidadeMedida unidade)
        {


            if (_unidadeDAO.UnidadeDuplicada(unidade.Descricao))
            {
                ModelState.AddModelError("Descricao", "Já existe uma unidade de medida com essa descrição.");
            }
            if (ModelState.IsValid)
            {
                _unidadeDAO.Inserir(unidade);
                return Json(new
                {
                    sucesso = true,
                    unidade = new { id = unidade.IdUnidadeMedida, nome = unidade.Descricao.ToUpper() }
                });
            }

            return PartialView("FormUnidadeModal", unidade);
        }
    }
}
