using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ClienteDAO _clienteDAO = new ClienteDAO();
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO(); 


        public IActionResult Index()
        {
            var lista = _clienteDAO.Listar();
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

            return View(new Cliente());
        }

        [HttpPost]
        public IActionResult Criar(Cliente cliente)
        {
            if (cliente.IdCondPgto == 0)
            {
                ModelState.AddModelError("IdCondPgto", "Selecione uma condição de pagamento.");
            }

            if (cliente.IdCidade == 0)
            {
                ModelState.AddModelError("IdCidade", "Selecione uma cidade.");
            }
            else
            {
                bool estrangeiro = _cidadeDAO.CidadeEstrangeira(cliente.IdCidade);

                if (!estrangeiro)
                {
                    if (string.IsNullOrWhiteSpace(cliente.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "O CPF/CNPJ é obrigatório para clientes brasileiros.");
                    }
                    else if (_clienteDAO.ExisteCpfCnpj(cliente.CPF_CNPJ))
                    {
                        ModelState.AddModelError("CPF_CNPJ", "Já existe um cliente com este CPF ou CNPJ.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                _clienteDAO.Inserir(cliente);
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

            return View(cliente);
        }

        public IActionResult Editar(int id)
        {
            var cliente = _clienteDAO.BuscarPorId(id);
            if (cliente == null) return NotFound();

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

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(cliente.IdCidade)?.NomeCidade ?? "Não encontrado";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(cliente.IdCondPgto)?.Descricao ?? "Não encontrado";


            return View(cliente);
        }

        [HttpPost]
        public IActionResult Editar(Cliente cliente)
        {
            bool estrangeiro = _cidadeDAO.CidadeEstrangeira(cliente.IdCidade);


            

            if (!estrangeiro)
            {
                if (string.IsNullOrWhiteSpace(cliente.CPF_CNPJ))
                {
                    ModelState.AddModelError("CPF_CNPJ", "O CPF/CNPJ é obrigatório para clientes brasileiros.");
                }
            }

            if (ModelState.IsValid)
            {
                _clienteDAO.Atualizar(cliente);
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

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(cliente.IdCidade)?.NomeCidade ?? "Não encontrado";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(cliente.IdCondPgto)?.Descricao ?? "Não encontrado";

            return View(cliente);
        }

        public IActionResult Excluir(int id)
        {
            var cliente = _clienteDAO.BuscarPorId(id);
            if (cliente == null) return NotFound();

            ViewBag.NomeCidade = _cidadeDAO.BuscarPorId(cliente.IdCidade)?.NomeCidade ?? "Não encontrado";
            ViewBag.NomeCondicao = _condicaoPagamentoDAO.BuscarPorId(cliente.IdCondPgto)?.Descricao;

            return View(cliente);
        }

        [HttpPost, ActionName("Excluir")]
        public IActionResult ConfirmarExclusao(int id)
        {
            _clienteDAO.Excluir(id);
            return RedirectToAction("Index");
        }
    }
}
