using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class ContaAReceberController : Controller
    {
        private readonly ContaAReceberDAO _contaAReceberDAO = new ContaAReceberDAO();
        private readonly ClienteDAO _clienteDAO = new ClienteDAO();
        private readonly FormaPagamentoDAO _formaPagamentoDAO = new FormaPagamentoDAO();
        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();

        public IActionResult Index()
        {
            try
            {
                var lista = _contaAReceberDAO.Listar();
                return View(lista);
            }
            catch (Exception erro)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar contas a receber: {erro.Message}";
                return View(new List<ContaAReceber>());
            }
        }

        [HttpGet]
        public IActionResult Criar(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela, bool apenasDetalhes = false)
        {
            ViewData["Title"] = "Detalhes / Baixa de Conta";

            if (string.IsNullOrEmpty(modelo) && numeroNota == 0)
            {
                TempData["ErrorMessage"] = "A criação manual de contas a receber está desativada. As contas são geradas automaticamente pelas Vendas.";
                return RedirectToAction(nameof(Index));
            }

            var conta = _contaAReceberDAO.BuscarParcela(modelo, serie, numeroNota, clienteId, numeroParcela);

            if (conta == null)
            {
                return NotFound();
            }

            if (conta.Situacao == "RECEBIDO" || conta.Status == false || apenasDetalhes)
            {
                ViewData["Modo"] = "Detalhes";   
            }
            else
            {
                ViewData["Modo"] = "Baixar";     
                conta.DataPagamento = DateTime.Today;      
            }

            if (string.IsNullOrEmpty(conta.NomeCliente) && conta.ClienteId > 0)
            {
                var cliente = _clienteDAO.BuscarPorId(conta.ClienteId);
                if (cliente != null) conta.NomeCliente = cliente.Nome_RazaoSocial;
            }
            if (conta.IdFormaPgto.HasValue)
            {
                var formaPgto = _formaPagamentoDAO.BuscarPorId(conta.IdFormaPgto.Value);
                if (formaPgto != null) conta.NomeFormaPgto = formaPgto.Descricao;
            }

            PreencherViewBags();
            return View(conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Criar(ContaAReceber conta)
        {
            try
            {
                if (conta.DataPagamento == null)
                    ModelState.AddModelError("DataPagamento", "A Data do Recebimento é obrigatória.");

                if (conta.ValorPago == null || conta.ValorPago <= 0)
                    ModelState.AddModelError("ValorPago", "O Valor Recebido é inválido.");

                if (ModelState.IsValid)
                {
                    _contaAReceberDAO.ReceberParcela(
                        conta.Modelo,
                        conta.Serie,
                        conta.NumeroNota,
                        conta.ClienteId,
                        conta.NumeroParcela,
                        conta.DataPagamento.Value,
                        conta.ValorPago.Value,
                        conta.ValJuros,
                        conta.ValMulta,
                        conta.ValDesconto
                    );

                    TempData["SuccessMessage"] = "Conta recebida com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao realizar o recebimento: {ex.Message}");
            }

            ViewData["Modo"] = "Baixar";
            ViewData["Title"] = "Detalhes / Baixa de Conta";
            PreencherViewBags();
            return View(conta);
        }

        [HttpGet]
        public IActionResult Cancelar()
        {
            TempData["ErrorMessage"] = "Não é possível cancelar contas a receber por aqui. Cancele a Venda correspondente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Cancelar(ContaAReceber conta)
        {
            TempData["ErrorMessage"] = "Ação não permitida.";
            return RedirectToAction(nameof(Index));
        }

        private void PreencherViewBags()
        {
            ViewBag.Clientes = _clienteDAO.Listar() ?? new List<Cliente>();
            ViewBag.FormasPagamento = _formaPagamentoDAO.Listar() ?? new List<FormaPagamento>();

            ViewBag.CidadesItens = new List<SelectListItem>();
            ViewBag.CondicoesPagamentoItens = new List<SelectListItem>();
        }

        [HttpGet]
        public IActionResult VerificarPendenciaParcela(string modelo, string serie, int numeroNota, int clienteId, int numeroParcela)
        {
            try
            {
                bool temPendencia = _contaAReceberDAO.TemParcelaAnteriorPendente(modelo, serie, numeroNota, clienteId, numeroParcela);

                if (temPendencia)
                {
                    return Json(new
                    {
                        temPendencia = true,
                        mensagem = "Não é possível dar baixa nesta parcela. Existem parcelas anteriores pendentes para este cliente nesta nota."
                    });
                }

                return Json(new { temPendencia = false });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    temPendencia = true,
                    mensagem = $"Erro ao validar pendências: {ex.Message}"
                });
            }
        }
    }
}