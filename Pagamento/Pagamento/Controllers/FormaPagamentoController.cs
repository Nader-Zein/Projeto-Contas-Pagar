using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;
using static Org.BouncyCastle.Bcpg.Attr.ImageAttrib;

namespace Pagamento.Controllers
{
    public class FormaPagamentoController : Controller
    {
        private readonly FormaPagamentoDAO _formaPagamentoDAO;

        public FormaPagamentoController(FormaPagamentoDAO formaPagamentoDAO)
        {
            _formaPagamentoDAO = formaPagamentoDAO;
        }

        public IActionResult FormaPagamento()
        {
            List<FormaPagamento> lista = _formaPagamentoDAO.Listar();
            return View(lista);
        }

        [HttpGet]
        public IActionResult CriarForma()
        {
            return View(new FormaPagamento());
        }

        [HttpPost]
        public IActionResult CriarForma(FormaPagamento forma)
        {
            if (_formaPagamentoDAO.ExisteForma(forma.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta forma de pagamento já está cadastrada!");
                return View(forma);
            }
            if (!string.IsNullOrEmpty(forma.Descricao))
            {
                _formaPagamentoDAO.Inserir(forma);
                TempData["SuccessMessage"] = "Forma de pagamento cadastrada com sucesso!";
                return RedirectToAction("FormaPagamento");
            }
            return View(forma);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var forma = _formaPagamentoDAO.BuscarPorId(id);
            if (forma == null) return NotFound();
            return View(forma);
        }

        [HttpPost]
        public IActionResult Editar(FormaPagamento forma)
        {
            if (!string.IsNullOrEmpty(forma.Descricao))
            {
                _formaPagamentoDAO.Atualizar(forma);
                TempData["SuccessMessage"] = "Forma de pagamento atualizada com sucesso!";
                return RedirectToAction("FormaPagamento");
            }
            return View(forma);
        }

        public IActionResult Excluir(int id)
        {
            var forma = _formaPagamentoDAO.BuscarPorId(id);
            if (forma == null) return NotFound();
            return View(forma);
        }

        [HttpPost]
        public IActionResult Excluir(FormaPagamento forma)
        {
            try
            {
                _formaPagamentoDAO.Excluir(forma.IdFormaPgto);

                TempData["SuccessMessage"] = "Forma de pagamento excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Esta forma de pagamento nao pode ser excluída,pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a forma de pagamento.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("FormaPagamento");
        }
        

        public IActionResult FormModal()
        {
            return PartialView("FormFormaPagamentoModal", new FormaPagamento());
        }

        [HttpPost]
        public IActionResult FormModal(FormaPagamento forma)
        {

            if (_formaPagamentoDAO.ExisteForma(forma.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta forma de pagamento já está cadastrada!");
            }

            if (ModelState.IsValid)
            {
                _formaPagamentoDAO.Inserir(forma);
                return Json(new
                {
                    sucesso = true,
                    forma = new { id = forma.IdFormaPgto, nome = forma.Descricao.ToUpper() }
                });
            }

            return PartialView("FormFormaPagamentoModal", forma);
        }


        [HttpGet]
        public IActionResult BuscarPorIdJSON(int id)
        {
            var forma = _formaPagamentoDAO.BuscarPorId(id);       
            if (forma == null)
            {
                return NotFound("Forma de Pagamento não encontrada.");
            }
            return Json(new
            {
                idFormaPgto = forma.IdFormaPgto,
                descricao = forma.Descricao
            });
        }
    }
}
