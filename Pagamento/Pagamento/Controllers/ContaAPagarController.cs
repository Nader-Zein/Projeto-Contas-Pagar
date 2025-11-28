using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class ContaAPagarController : Controller
    {
        private readonly ContaAPagarDAO _contaAPagarDAO = new ContaAPagarDAO();

        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly FormaPagamentoDAO _formaPagamentoDAO = new FormaPagamentoDAO();

        private readonly CompraDAO _compraDAO = new CompraDAO();

        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();
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

            ViewData["Modo"] = "Criar";

            PreencherViewBagsAvulsa();

            var novaConta = new ContaAPagar
            {
                DataEmissao = DateTime.Today,
                DataVencimento = DateTime.Today,
                NumeroParcela = 0,
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

                if (!string.IsNullOrEmpty(conta.Modelo) &&
                    !string.IsNullOrEmpty(conta.Serie) &&
                    conta.NumeroNota > 0 &&
                    conta.FornecedorId > 0)
                {
                    Compra compraExistente = _compraDAO.BuscarDetalhesPorChaveComposta(
                        conta.Modelo,
                        conta.Serie,
                        conta.NumeroNota.ToString(),          
                        conta.FornecedorId
                    );

                    if (compraExistente != null && compraExistente.Status == true)
                    {
                        ModelState.AddModelError("", "Não é possível criar esta conta avulsa. Os dados de Nota (Modelo, Série, N°, Fornecedor) já existem em uma Compra. Contas avulsas não podem ser anexadas a compras existentes.");
                    }
                }
                if (ModelState.IsValid)
                {
                    var contaDuplicada = _contaAPagarDAO.BuscarPorChave(
                        conta.Modelo,
                        conta.Serie,
                        conta.NumeroNota,
                        conta.FornecedorId,
                        conta.NumeroParcela
                    );

                    if (contaDuplicada != null)
                    {
                        ModelState.AddModelError("", $"Não é possível salvar. Já existe uma conta registrada com esta chave exata (Parcela: {conta.NumeroParcela}), mesmo que ela esteja cancelada.");
                    }
                }
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

            var cidades = _cidadeDAO.Listar() ?? new List<Cidade>();
            var condicoes = _condicaoPagamentoDAO.Listar() ?? new List<CondicaoPagamento>();

            ViewBag.CidadesItens = cidades
                .Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString()))
                .ToList();

            ViewBag.CondicoesPagamentoItens = condicoes
                .Select(c => new SelectListItem(c.Descricao, c.IdCondPgto.ToString()))
                .ToList();
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


            bool ehParcelaDeCompra = _compraDAO.ExisteChaveComposta(modelo, serie, numeroNota.ToString(), fornecedorId);

            if (ehParcelaDeCompra)
            {
                TempData["ErrorMessage"] = "Não é permitido cancelar esta parcela por aqui. Como ela pertence a uma Compra, você deve cancelar a Compra inteira no menu de Compras.";
                return RedirectToAction(nameof(Index));
            }

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

        [HttpGet]
        public IActionResult VerificarPendenciaParcela(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            try
            {
                bool temPendencia = _contaAPagarDAO.TemParcelaAnteriorPendente(modelo, serie, numeroNota, fornecedorId, numeroParcela);

                if (temPendencia)
                {
                    return Json(new
                    {
                        temPendencia = true,
                        mensagem = "Não é possível dar baixar nesta parcela. Parcelas anteriores pendentes."
                    });
                }

                return Json(new { temPendencia = false });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    temPendencia = true,         
                    mensagem = $"Erro ao validar pendências da parcela: {ex.Message}"
                });
            }
        }

        [HttpGet]
        public IActionResult VerificarContaDuplicada(string modelo, string serie, int numeroNota, int fornecedorId, int numeroParcela)
        {
            if (numeroParcela <= 0 || fornecedorId <= 0)
            {
                return Json(new { existe = false });
            }

            try
            {
                var conta = _contaAPagarDAO.BuscarPorChave(modelo, serie, numeroNota, fornecedorId, numeroParcela);

                if (conta != null)
                {
                    return Json(new
                    {
                        existe = true,
                        conta = new
                        {
                            modelo = conta.Modelo,
                            serie = conta.Serie,
                            numeroNota = conta.NumeroNota,
                            fornecedorId = conta.FornecedorId,
                            nomeFornecedor = conta.NomeFornecedor,    
                            numeroParcela = conta.NumeroParcela,
                            dataVencimento = conta.DataVencimento.ToString("yyyy-MM-dd"),
                            valorParcela = conta.ValorParcela,
                            situacao = conta.Situacao,
                            status = conta.Status
                        }
                    });
                }

                return Json(new { existe = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao verificar duplicidade de conta: {ex.Message}");
                return Json(new { existe = false });
            }
        }
    }
}
