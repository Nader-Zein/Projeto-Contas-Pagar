using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class VendaController : Controller
    {
        private readonly VendaDAO _vendaDAO = new VendaDAO();
        private readonly ClienteDAO _clienteDAO = new ClienteDAO();
        private readonly FuncionarioDAO _funcionarioDAO = new FuncionarioDAO();
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();
        private readonly ContaAReceberDAO _contaAReceberDAO = new ContaAReceberDAO();

        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly MarcaDAO _marcaDAO = new MarcaDAO();
        private readonly UnidadeMedidaDAO _unidadeMedidaDAO = new UnidadeMedidaDAO();
        private readonly CategoriaDAO _categoriaDAO = new CategoriaDAO();
        private readonly FormaPagamentoDAO _formaPagamentoDAO = new FormaPagamentoDAO();

        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        public IActionResult Index()
        {
            try
            {
                var lista = _vendaDAO.Listar();
                return View(lista);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar vendas: " + ex.Message;
                return View(new List<Venda>());
            }
        }

        public IActionResult Criar()
        {
            ViewData["Title"] = "Nova Venda";
            ViewData["Modo"] = "Criar";

            PreencherViewBags();

            return View(new Venda
            {
                DataEmissao = DateTime.Today,
                Status = true
            });
        }

        

        [HttpPost]
        public IActionResult Criar(Venda venda, string itensJson)
        {
            try
            {
                if (!string.IsNullOrEmpty(itensJson))
                {
                    venda.Itens = JsonSerializer.Deserialize<List<ItemVenda>>(itensJson);
                }

                if (venda.Itens == null || !venda.Itens.Any())
                {
                    ModelState.AddModelError("", "A venda deve ter pelo menos um item.");
                }


                if (ModelState.IsValid)
                {
                    _vendaDAO.Inserir(venda);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception erro)
            {
                ModelState.AddModelError("", "Ocorreu um erro ao salvar a venda: " + erro.Message);
            }

            ViewData["Title"] = "Cadastro de Venda";
            ViewData["Modo"] = "Criar";
            PreencherViewBags();
            return View(venda);
        }


        [HttpGet]
        public IActionResult Detalhes(string modelo, string serie, int numeroNota, int clienteId)
        {
            var venda = _vendaDAO.Listar().FirstOrDefault(v => v.Modelo == modelo && v.Serie == serie && v.NumeroNota == numeroNota && v.ClienteId == clienteId);
            if (venda == null) return NotFound();

            venda.ParcelasGeradas = _contaAReceberDAO.ListarPorVenda(modelo, serie, numeroNota, clienteId);

            var condicao = _condicaoPagamentoDAO.BuscarPorId(venda.CondicaoPagamentoId);
            ViewBag.NomeCondicaoPagamento = condicao?.Descricao ?? "N/D";

            ViewData["Title"] = "Detalhes da Venda";
            ViewData["Modo"] = "Detalhes";

            PreencherViewBags();
            return View("Criar", venda);
        }

        [HttpGet]
        public IActionResult Cancelar(string modelo, string serie, int numeroNota, int clienteId)
        {
            if (_contaAReceberDAO.VerificarParcelasPagas(modelo, serie, numeroNota, clienteId))
            {
                TempData["ErrorMessage"] = "Não é possível cancelar esta venda pois existem parcelas que já foram recebidas (baixadas).";
                return RedirectToAction("Index");
            }

            var venda = _vendaDAO.Listar().FirstOrDefault(v => v.Modelo == modelo && v.Serie == serie && v.NumeroNota == numeroNota && v.ClienteId == clienteId);
            if (venda == null) return NotFound();

            venda.ParcelasGeradas = _contaAReceberDAO.ListarPorVenda(modelo, serie, numeroNota, clienteId);

            
            var condicao = _condicaoPagamentoDAO.BuscarPorId(venda.CondicaoPagamentoId);
            ViewBag.NomeCondicaoPagamento = condicao?.Descricao ?? "N/D";

            ViewData["Title"] = "Cancelar Venda";
            ViewData["Modo"] = "Cancelar";
            PreencherViewBags();
            return View("Criar", venda);
        }

        [HttpPost]
        public IActionResult ConfirmarCancelamento(string modelo, string serie, int numeroNota, int clienteId, string motivoCancelamento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivoCancelamento))
                {
                    TempData["ErrorMessage"] = "Informe o motivo.";
                    return RedirectToAction("Cancelar", new { modelo, serie, numeroNota, clienteId });
                }

                _vendaDAO.Cancelar(modelo, serie, numeroNota, clienteId, motivoCancelamento);
                TempData["SuccessMessage"] = "Venda cancelada.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Cancelar", new { modelo, serie, numeroNota, clienteId });
            }
        }

        private void PreencherViewBags()
        {
            var clientes = _clienteDAO.Listar();
            var funcionarios = _funcionarioDAO.Listar();
            var produtos = _produtoDAO.Listar();
            var condicoes = _condicaoPagamentoDAO.Listar();
            var cidades = _cidadeDAO.Listar();
            var fornecedores = _fornecedorDAO.Listar();
            var marcas = _marcaDAO.Listar();
            var unidades = _unidadeMedidaDAO.Listar();
            var categorias = _categoriaDAO.Listar();
            var formas = _formaPagamentoDAO.Listar();      

            ViewBag.Clientes = clientes;
            ViewBag.Funcionarios = funcionarios;
            ViewBag.Produtos = produtos;
            ViewBag.CondicoesPagamento = condicoes;

            ViewBag.CidadesItens = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();

            ViewBag.ClientesItens = clientes.Select(c => new SelectListItem(c.Nome_RazaoSocial, c.IdPessoa.ToString())).ToList();
            ViewBag.FuncionariosItens = funcionarios.Select(f => new SelectListItem(f.Nome_RazaoSocial, f.IdPessoa.ToString())).ToList();
            ViewBag.CondicoesPagamentoItens = condicoes.Select(c => new SelectListItem(c.Descricao, c.IdCondPgto.ToString())).ToList();
            ViewBag.FornecedoresItens = fornecedores.Select(f => new SelectListItem(f.Nome_RazaoSocial, f.IdPessoa.ToString())).ToList();
            ViewBag.MarcasItens = marcas.Select(m => new SelectListItem(m.Descricao, m.IdMarca.ToString())).ToList();
            ViewBag.UnidadesItens = unidades.Select(u => new SelectListItem(u.Descricao, u.IdUnidadeMedida.ToString())).ToList();
            ViewBag.CategoriasItens = categorias.Select(c => new SelectListItem(c.Descricao, c.IdCategoria.ToString())).ToList();
            ViewBag.FormasPagamentoItens = formas.Select(f => new SelectListItem(f.Descricao, f.IdFormaPgto.ToString())).ToList();
        }

        [HttpGet]
        public IActionResult VerificarDuplicidade(string modelo, string serie, int numeroNota, int clienteId)
        {
            var venda = _vendaDAO.Listar()
                .FirstOrDefault(v => v.Modelo == modelo &&
                                     v.Serie == serie &&
                                     v.NumeroNota == numeroNota &&
                                     v.ClienteId == clienteId);

            if (venda != null)
            {
                return Json(new
                {
                    existe = true,
                    venda = new
                    {
                        modelo = venda.Modelo,
                        serie = venda.Serie,
                        numeroNota = venda.NumeroNota,
                        clienteId = venda.ClienteId,
                        nomeCliente = venda.NomeCliente,
                        dataEmissao = venda.DataEmissao.ToString("yyyy-MM-dd"),
                        total = venda.TotalNota.ToString("C2"),
                        observacoes = venda.Observacoes,
                        funcionario = venda.NomeFuncionario
                    }
                });
            }

            return Json(new { existe = false });
        }
    }
}
