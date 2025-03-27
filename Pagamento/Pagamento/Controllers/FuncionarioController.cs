using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FuncionarioController : Controller
    {
        private readonly FuncionarioDAO _funcionarioDAO = new FuncionarioDAO();
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();

        public IActionResult Index()
        {
            var lista = _funcionarioDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Criar(Funcionario funcionario)
        {
            if (ModelState.IsValid)
            {
                _funcionarioDAO.Inserir(funcionario);
                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(funcionario);
        }

        public IActionResult Editar(int id)
        {
            var funcionario = _funcionarioDAO.BuscarPorId(id);
            if (funcionario == null) return NotFound();

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(funcionario);
        }

        [HttpPost]
        public IActionResult Editar(Funcionario funcionario)
        {
            if (ModelState.IsValid)
            {
                _funcionarioDAO.Atualizar(funcionario);
                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            return View(funcionario);
        }

        public IActionResult Excluir(int id)
        {
            var funcionario = _funcionarioDAO.BuscarPorId(id);
            if (funcionario == null) return NotFound();

            return View(funcionario);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _funcionarioDAO.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}
