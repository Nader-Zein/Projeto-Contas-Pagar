using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class CidadeController : Controller
    {
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly EstadoDAO _estadoDAO = new EstadoDAO();

        public IActionResult Index()
        {
            var lista = _cidadeDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            var estados = _estadoDAO.Listar();

            ViewBag.Estados = estados.Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),  
                Text = e.NomeEstado  
            }).ToList();

            return View(new Cidade());
        }

        [HttpPost]
        public IActionResult Criar(Cidade cidade)
        {

            if (cidade.IdEstado == 0)
            {
                ModelState.AddModelError("IdEstado", "Selecione um estado.");
            }

            if (_cidadeDAO.ExisteCidadePorNome(cidade.NomeCidade, cidade.IdEstado))
            {
                ModelState.AddModelError("NomeCidade", "Esta cidade já está cadastrada!");

                ViewBag.Estados = _estadoDAO.Listar().Select(e => new SelectListItem
                {
                    Value = e.IdEstado.ToString(),
                    Text = e.NomeEstado
                }).ToList();

                return View(cidade);
            }

            if (ModelState.IsValid)
            {
                _cidadeDAO.Inserir(cidade);
                return RedirectToAction("Index");
            }

            ViewBag.Estados = _estadoDAO.Listar().Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),
                Text = e.NomeEstado
            }).ToList();

            return View(cidade);
        }


        public IActionResult Excluir(int id)
        {
            var cidade = _cidadeDAO.Listar().FirstOrDefault(c => c.IdCidade == id);
            if (cidade == null) return NotFound();

            ViewBag.NomeEstado = _estadoDAO.BuscarPorId(cidade.IdEstado)?.NomeEstado ?? "";

            return View(cidade);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            
            try
            {
                _cidadeDAO.Excluir(id);

                TempData["SuccessMessage"] = "Cidade excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Esta cidade nao pode ser excluida, pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a cidade.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("Index");
        }

        public IActionResult Editar(int id)
        {
            var cidade = _cidadeDAO.BuscarPorId(id);
            if (cidade == null) return NotFound();

            var estados = _estadoDAO.Listar();
            ViewBag.Estados = estados.Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),
                Text = e.NomeEstado
            }).ToList();

            ViewBag.NomeEstado = _estadoDAO.BuscarPorId(cidade.IdEstado)?.NomeEstado ?? "";

            return View(cidade);
        }


        [HttpPost]
        public IActionResult Editar(Cidade cidade)
        {

            if (ModelState.IsValid)
            {
                
                _cidadeDAO.Atualizar(cidade);
                return RedirectToAction("Index");
            }

            var estados = _estadoDAO.Listar();
            ViewBag.Estados = estados.Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),
                Text = e.NomeEstado
            }).ToList();

            ViewBag.NomeEstado = _estadoDAO.BuscarPorId(cidade.IdEstado)?.NomeEstado ?? "";


            return View(cidade);
        }

        public IActionResult FormModal()
        {
            ViewBag.Estados = _estadoDAO.Listar().Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),
                Text = e.NomeEstado
            }).ToList();

            return PartialView("FormCidadeModal", new Cidade());
        }

        [HttpPost]
        public IActionResult FormModal(Cidade cidade)
        {

            if (_cidadeDAO.ExisteCidadePorNome(cidade.NomeCidade, cidade.IdEstado))
            {
                ModelState.AddModelError("NomeCidade", "Esta cidade já está cadastrada!");
            }
            if (ModelState.IsValid)
            {
                _cidadeDAO.Inserir(cidade);
                return Json(new
                {
                    sucesso = true,
                    cidade = new { id = cidade.IdCidade, nome = cidade.NomeCidade.ToUpper() }
                });
            }

            ViewBag.Estados = _estadoDAO.Listar().Select(e => new SelectListItem
            {
                Value = e.IdEstado.ToString(),
                Text = e.NomeEstado
            }).ToList();

            return PartialView("FormCidadeModal", cidade);
        }



    }
}
