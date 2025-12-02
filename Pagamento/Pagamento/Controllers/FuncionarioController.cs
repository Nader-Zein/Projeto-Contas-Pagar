using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySqlX.XDevAPI;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FuncionarioController : Controller
    {
        private readonly FuncionarioDAO _funcionarioDAO;
        private readonly CidadeDAO _cidadeDAO;

        public FuncionarioController(FuncionarioDAO funcionarioDAO, CidadeDAO cidadeDAO)
        {
            _funcionarioDAO = funcionarioDAO;
            _cidadeDAO = cidadeDAO;
        }
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

            var funcionario = new Funcionario
            {
                Status = true,               
                TipoPessoa = "Física"       
            };

            return View(funcionario);
        }

        [HttpPost]
        public IActionResult Criar(Funcionario funcionario)
        {

            if (funcionario.IdCidade == 0)
            {
                ModelState.AddModelError("IdCidade", "Selecione uma cidade.");
            }
            else
            {
                bool estrangeiro = _cidadeDAO.CidadeEstrangeira(funcionario.IdCidade);

                if (!estrangeiro)
                {
                    if (string.IsNullOrWhiteSpace(funcionario.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "O CPF é obrigatório para funcionarios brasileiros.");
                    }
                    else if (_funcionarioDAO.ExisteCpfCnpj(funcionario.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "Já existe um funcionario com este CPF.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _funcionarioDAO.Inserir(funcionario);
                TempData["SuccessMessage"] = "Funcionario cadastrado com sucesso!";

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
                TempData["SuccessMessage"] = "Funcionario atualizado com sucesso!";

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
            try
            {
                _funcionarioDAO.Excluir(id);

                TempData["SuccessMessage"] = "Funcionario excluído com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este funcionario nao pode ser excluído,pois esta sendo utilizado em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir o funcionario.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult BuscarPorIdJSON(int id)
        {
            var funcionario = _funcionarioDAO.BuscarPorId(id);
            if (funcionario == null)
            {
                return NotFound(new { mensagem = "Funcionário não encontrado." });
            }

            return Json(new
            {
                idFuncionario = funcionario.IdPessoa,
                nome_RazaoSocial = funcionario.Nome_RazaoSocial
            });
        }

        public IActionResult FormModal()
        {
            var cidades = _cidadeDAO.Listar();
            ViewBag.Cidades = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();

            return PartialView("FormFuncionarioModal", new Funcionario { Status = true });
        }

        [HttpPost]
        public IActionResult FormModal(Funcionario funcionario)
        {
            if (string.IsNullOrEmpty(funcionario.Nome_RazaoSocial))
                ModelState.AddModelError("Nome_RazaoSocial", "O nome é obrigatório.");

            if (funcionario.IdCidade == 0)
                ModelState.AddModelError("IdCidade", "A cidade é obrigatória.");

            if (ModelState.IsValid)
            {
                _funcionarioDAO.Inserir(funcionario);

                return Json(new
                {
                    sucesso = true,
                    funcionario = new
                    {
                        idFuncionario = funcionario.IdPessoa,       
                        nome_RazaoSocial = funcionario.Nome_RazaoSocial.ToUpper()
                    }
                });
            }

            var cidades = _cidadeDAO.Listar();
            ViewBag.Cidades = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();

            return PartialView("FormFuncionarioModal", funcionario);
        }
    }
}
