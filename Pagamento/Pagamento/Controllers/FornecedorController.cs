using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO(); 

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
            _fornecedorDAO.Excluir(id);
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
                        nome = fornecedor.Nome_RazaoSocial
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
    }
}
