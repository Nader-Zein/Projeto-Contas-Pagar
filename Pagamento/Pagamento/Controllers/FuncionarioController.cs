using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlX.XDevAPI;
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

            bool estrangeiro = _cidadeDAO.CidadeEstrangeira(funcionario.IdCidade);


            

            if (!estrangeiro)
            {
                if (string.IsNullOrWhiteSpace(funcionario.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "O CPF é obrigatório para funcionarios brasileiros.");
                }
            }

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

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(funcionario.IdCidade)?.NomeCidade ?? "Não encontrado";

            return View(funcionario);
        }

        [HttpPost]
        public IActionResult Editar(Funcionario funcionario)
        {

            bool estrangeiro = _cidadeDAO.CidadeEstrangeira(funcionario.IdCidade);


            

            if (!estrangeiro)
            {
                if (string.IsNullOrWhiteSpace(funcionario.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "O CPF é obrigatório para funcionarios brasileiros.");
                }
            }

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

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(funcionario.IdCidade)?.NomeCidade ?? "Não encontrado";

            return View(funcionario);
        }

        public IActionResult Excluir(int id)
        {
            var funcionario = _funcionarioDAO.BuscarPorId(id);
            if (funcionario == null) return NotFound();

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(funcionario.IdCidade)?.NomeCidade ?? "Não encontrado";

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
