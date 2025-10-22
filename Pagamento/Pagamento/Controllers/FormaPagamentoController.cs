using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FormaPagamentoController : Controller
    {
        private readonly FormaPagamentoDAO _dao = new FormaPagamentoDAO();

        public IActionResult FormaPagamento()
        {
            List<FormaPagamento> lista = _dao.Listar();
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
            if (_dao.ExisteForma(forma.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta forma de pagamento já está cadastrada!");
                return View(forma);
            }
            if (!string.IsNullOrEmpty(forma.Descricao))
            {
                _dao.Inserir(forma);
                return RedirectToAction("FormaPagamento");
            }
            return View(forma);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var forma = _dao.BuscarPorId(id);
            if (forma == null) return NotFound();
            return View(forma);
        }

        [HttpPost]
        public IActionResult Editar(FormaPagamento forma)
        {
            if (!string.IsNullOrEmpty(forma.Descricao))
            {
                _dao.Atualizar(forma);
                return RedirectToAction("FormaPagamento");
            }
            return View(forma);
        }

        public IActionResult Excluir(int id)
        {
            var forma = _dao.BuscarPorId(id);
            if (forma == null) return NotFound();
            return View(forma);
        }

        [HttpPost]
        public IActionResult Excluir(FormaPagamento forma)
        {
            _dao.Excluir(forma.IdFormaPgto);
            return RedirectToAction("FormaPagamento");
        }

        public IActionResult FormModal()
        {
            return PartialView("FormFormaPagamentoModal", new FormaPagamento());
        }

        [HttpPost]
        public IActionResult FormModal(FormaPagamento forma)
        {
            if (ModelState.IsValid)
            {
                _dao.Inserir(forma);
                return Json(new
                {
                    sucesso = true,
                    forma = new { id = forma.IdFormaPgto, nome = forma.Descricao }
                });
            }

            return PartialView("FormFormaPagamentoModal", forma);
        }

    }
}
