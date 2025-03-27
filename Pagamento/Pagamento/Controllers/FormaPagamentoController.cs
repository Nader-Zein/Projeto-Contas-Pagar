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
            return View();
        }

        [HttpPost]
        public IActionResult CriarForma(FormaPagamento forma)
        {
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
    }
}
