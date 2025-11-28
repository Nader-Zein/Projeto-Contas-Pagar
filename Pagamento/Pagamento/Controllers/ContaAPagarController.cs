using Microsoft.AspNetCore.Mvc;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class ContaAPagarController : Controller
    {
        private readonly ContaAPagarDAO _contaAPagarDAO = new ContaAPagarDAO();

        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly FormaPagamentoDAO _formaPagamentoDAO = new FormaPagamentoDAO();

        public IActionResult Index()
        {
            try
            {
                var lista = _contaAPagarDAO.Listar();
                return View(lista);       
            }
            catch (Exception erro)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar contas: {erro.Message}";
                return View(new List<ContaAPagar>());     
            }
        }

        [HttpGet]
        public IActionResult CriarAvulsa()
        {
            ViewData["Title"] = "Cadastro de Conta a Pagar";

            PreencherViewBagsAvulsa();

            var novaConta = new ContaAPagar
            {
                DataEmissao = DateTime.Today,
                DataVencimento = DateTime.Today,
                NumeroParcela = 1,
                Status = true
            };

            return View(novaConta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CriarAvulsa(ContaAPagar conta)
        {
            try
            {
                conta.Situacao = "A PAGAR";      
                if (ModelState.IsValid)
                {
                    _contaAPagarDAO.Inserir(conta);     
                    TempData["SuccessMessage"] = "Conta a pagar salva com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar a conta: {ex.Message}");
            }

            ViewData["Title"] = "Cadastro de Conta a Pagar";
            PreencherViewBagsAvulsa();
            return View(conta);
        }

        private void PreencherViewBagsAvulsa()
        {
            var fornecedores = _fornecedorDAO.Listar() ?? new List<Fornecedor>();
            var formasPgto = _formaPagamentoDAO.Listar() ?? new List<FormaPagamento>();

            ViewBag.Fornecedores = fornecedores;
            ViewBag.FormasPagamento = formasPgto;
        }



        [HttpGet]
        public IActionResult Baixar(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            ViewData["Modo"] = "Baixar";
            ViewData["Title"] = "Baixa de Conta a Pagar";

            var conta = _contaAPagarDAO.BuscarPorChave(modelo, serie, numeroNota, fornecedorId, numeroParcela);

            if (conta == null)
            {
                return NotFound();
            }

            if (conta.Situacao == "PAGO" || conta.Status == false)
            {
                TempData["ErrorMessage"] = "Esta conta já foi baixada ou está cancelada.";
                return RedirectToAction(nameof(Index));
            }

            conta.DataPagamento = DateTime.Today;

            if (conta.FornecedorId > 0)
            {
                var fornecedor = _fornecedorDAO.BuscarPorId(conta.FornecedorId);
                if (fornecedor != null) conta.NomeFornecedor = fornecedor.Nome_RazaoSocial;
            }
            if (conta.IdFormaPgto.HasValue)
            {
                var formaPgto = _formaPagamentoDAO.BuscarPorId(conta.IdFormaPgto.Value);
                if (formaPgto != null) conta.NomeFormaPgto = formaPgto.Descricao;
            }

            PreencherViewBagsAvulsa();

            return View("CriarAvulsa", conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Baixar(ContaAPagar conta)
        {
            try
            {
                if (conta.DataPagamento == null)
                    ModelState.AddModelError("DataPagamento", "A Data de Pagamento é obrigatória.");

                if (conta.ValorPago == null || conta.ValorPago <= 0)
                    ModelState.AddModelError("ValorPago", "O Valor Pago calculado é inválido.");

                if (conta.IdFormaPgto == null || conta.IdFormaPgto <= 0)
                    ModelState.AddModelError("IdFormaPgto", "A Forma de Pagamento é obrigatória para a baixa.");


                if (ModelState.IsValid)
                {
                    conta.Situacao = "PAGO";
                    conta.Status = true;       

                    _contaAPagarDAO.EfetuarBaixa(conta);

                    TempData["SuccessMessage"] = "Conta baixada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar a baixa: {ex.Message}");
            }

            ViewData["Modo"] = "Baixar";
            ViewData["Title"] = "Baixa de Conta a Pagar";
            PreencherViewBagsAvulsa();
            return View("CriarAvulsa", conta);
        }

        [HttpGet]
        public IActionResult Cancelar(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            ViewData["Modo"] = "Cancelar";
            ViewData["Title"] = "Cancelamento de Conta a Pagar";

            var conta = _contaAPagarDAO.BuscarPorChave(modelo, serie, numeroNota, fornecedorId, numeroParcela);

            if (conta == null)
            {
                return NotFound();
            }

            if (conta.Situacao == "PAGO" || conta.Status == false)
            {
                TempData["ErrorMessage"] = "Esta conta já foi paga ou já está cancelada.";
                return RedirectToAction(nameof(Index));
            }

            if (conta.FornecedorId > 0)
            {
                var fornecedor = _fornecedorDAO.BuscarPorId(conta.FornecedorId);
                if (fornecedor != null) conta.NomeFornecedor = fornecedor.Nome_RazaoSocial;
            }
            if (conta.IdFormaPgto.HasValue)
            {
                var formaPgto = _formaPagamentoDAO.BuscarPorId(conta.IdFormaPgto.Value);
                if (formaPgto != null) conta.NomeFormaPgto = formaPgto.Descricao;
            }

            PreencherViewBagsAvulsa();

            return View("CriarAvulsa", conta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(ContaAPagar conta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(conta.Motivo_Cancelamento))
                {
                    ModelState.AddModelError("Motivo_Cancelamento", "O Motivo do Cancelamento é obrigatório.");
                }

                if (ModelState.IsValid)
                {
                    _contaAPagarDAO.CancelarParcela(
                        conta.Modelo,
                        conta.Serie,
                        conta.NumeroNota,
                        conta.FornecedorId,
                        conta.NumeroParcela,
                        conta.Motivo_Cancelamento
                    );

                    TempData["SuccessMessage"] = "Conta cancelada com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar o cancelamento: {ex.Message}");
            }

            ViewData["Modo"] = "Cancelar";
            ViewData["Title"] = "Cancelamento de Conta a Pagar";
            PreencherViewBagsAvulsa();
            return View("CriarAvulsa", conta);
        }


        [HttpGet]
        public IActionResult Detalhes(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            ViewData["Modo"] = "Detalhes";
            ViewData["Title"] = "Detalhes da Conta a Pagar";

            var conta = _contaAPagarDAO.BuscarPorChave(modelo, serie, numeroNota, fornecedorId, numeroParcela);

            if (conta == null)
            {
                return NotFound();
            }

            if (conta.FornecedorId > 0)
            {
                var fornecedor = _fornecedorDAO.BuscarPorId(conta.FornecedorId);
                if (fornecedor != null) conta.NomeFornecedor = fornecedor.Nome_RazaoSocial;
            }
            if (conta.IdFormaPgto.HasValue)
            {
                var formaPgto = _formaPagamentoDAO.BuscarPorId(conta.IdFormaPgto.Value);
                if (formaPgto != null) conta.NomeFormaPgto = formaPgto.Descricao;
            }

            PreencherViewBagsAvulsa();

            return View("CriarAvulsa", conta);
        }
    }
}
