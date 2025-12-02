using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly FornecedorDAO _fornecedorDAO;
        private readonly CidadeDAO _cidadeDAO;
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO;

        public FornecedorController(
            FornecedorDAO fornecedorDAO,
            CidadeDAO cidadeDAO,
            CondicaoPagamentoDAO condicaoPagamentoDAO)
        {
            _fornecedorDAO = fornecedorDAO;
            _cidadeDAO = cidadeDAO;
            _condicaoPagamentoDAO = condicaoPagamentoDAO;
        }
        public IActionResult Index()
        {
            var lista = _fornecedorDAO.Listar();
            return View(lista);
        }

        public IActionResult Criar()
        {
            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            return View(new Fornecedor());
        }

        [HttpPost]
        public IActionResult Criar(Fornecedor fornecedor)
        {


            if (fornecedor.IdCondPgto == 0)
            {
                ModelState.AddModelError("IdCondPgto", "Selecione uma condição de pagamento.");
            }

            if (fornecedor.IdCidade == 0)
            {
                ModelState.AddModelError("IdCidade", "Selecione uma cidade.");
            }
            else
            {
                bool estrangeiro = _cidadeDAO.CidadeEstrangeira(fornecedor.IdCidade);

                if (!estrangeiro)
                {
                    if (string.IsNullOrWhiteSpace(fornecedor.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "O CPF/CNPJ é obrigatório para fornecedores brasileiros.");
                    }
                    else if (_fornecedorDAO.ExisteCpfCnpj(fornecedor.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "Já existe um fornecedor com este CPF ou CNPJ.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _fornecedorDAO.Inserir(fornecedor);
                TempData["SuccessMessage"] = "Fornecedor cadastrado com sucesso!";

                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            return View(fornecedor);
        }

        public IActionResult Editar(int id)
        {
            var fornecedor = _fornecedorDAO.BuscarPorId(id);
            if (fornecedor == null) return NotFound();

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(fornecedor.IdCidade)?.NomeCidade ?? "";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(fornecedor.IdCondPgto)?.Descricao ?? "Não encontrado";

            return View(fornecedor);
        }

        [HttpPost]
        public IActionResult Editar(Fornecedor fornecedor)
        {

            bool estrangeiro = _cidadeDAO.CidadeEstrangeira(fornecedor.IdCidade);


            

            if (!estrangeiro)
            {
                if (string.IsNullOrWhiteSpace(fornecedor.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "O CPF/CNPJ é obrigatório para fornecedores brasileiros.");
                }
            }
            if (ModelState.IsValid)
            {
                _fornecedorDAO.Atualizar(fornecedor);
                TempData["SuccessMessage"] = "Fornecedor atualizado com sucesso!";

                return RedirectToAction("Index");
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(fornecedor.IdCidade)?.NomeCidade ?? "Não encontrado";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(fornecedor.IdCondPgto)?.Descricao ?? "Não encontrado";
            return View(fornecedor);
        }


        public IActionResult Excluir(int id)
        {
            var fornecedor = _fornecedorDAO.BuscarPorId(id);
            if (fornecedor == null) return NotFound();

            var cidade = _cidadeDAO.BuscarPorId(fornecedor.IdCidade);
            ViewBag.NomeCidade = cidade?.NomeCidade ?? "";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(fornecedor.IdCondPgto)?.Descricao;
            return View(fornecedor);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            try
            {
                _fornecedorDAO.Excluir(id);

                TempData["SuccessMessage"] = "Fornecedor excluído com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Este fornecedor nao pode ser excluído,pois esta sendo utilizado em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir o fornecedor.";
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
            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            return PartialView("FormFornecedorModal", new Fornecedor());
        }


        [HttpPost]
        public IActionResult FormModal(Fornecedor fornecedor)
        {

            bool estrangeiro = _cidadeDAO.CidadeEstrangeira(fornecedor.IdCidade);

            if (!estrangeiro)
            {
                if (string.IsNullOrWhiteSpace(fornecedor.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "O CPF/CNPJ é obrigatório para fornecedores brasileiros.");
                }


                else if (_fornecedorDAO.ExisteCpfCnpj(fornecedor.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "Já existe um fornecedor com este CPF ou CNPJ.");
                }
            }



            if (ModelState.IsValid)
            {
                _fornecedorDAO.Inserir(fornecedor);
                return Json(new
                {
                    sucesso = true,
                    fornecedor = new
                    {
                        id = fornecedor.IdPessoa,
                        nome = fornecedor.Nome_RazaoSocial.ToUpper(),
                        idCondPgto = fornecedor.IdCondPgto
                    }
                });
            }

            ViewBag.Cidades = _cidadeDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCidade.ToString(),
                Text = c.NomeCidade
            }).ToList();

            ViewBag.CondicoesPagamento = _condicaoPagamentoDAO.Listar().Select(c => new SelectListItem
            {
                Value = c.IdCondPgto.ToString(),
                Text = c.Descricao
            }).ToList();

            return PartialView("FormFornecedorModal", fornecedor);
        }

        [HttpGet]
        public IActionResult BuscarPorIdJSON(int id)
        {
            var fornecedor = _fornecedorDAO.BuscarPorId(id);

            if (fornecedor == null)
            {
                return NotFound(new { mensagem = "Fornecedor não encontrado com o ID informado." });
            }

            return Json(new
            {
                idPessoa = fornecedor.IdPessoa,
                nome = fornecedor.Nome_RazaoSocial,
                idCondPgto = fornecedor.IdCondPgto
            });
        }
    }
}
