using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pagamento.DAO;
using Pagamento.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;     

namespace Pagamento.Controllers
{
    public class CompraController : Controller
    {
        private readonly FornecedorDAO _fornecedorDAO = new FornecedorDAO();
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();
        private readonly CompraDAO _compraDAO = new CompraDAO();        
        private readonly ContaAPagarDAO _contaAPagarDAO = new ContaAPagarDAO();


        private readonly CidadeDAO _cidadeDAO = new CidadeDAO();
        private readonly MarcaDAO _marcaDAO = new MarcaDAO();
        private readonly UnidadeMedidaDAO _unidadeMedidaDAO = new UnidadeMedidaDAO();
        private readonly CategoriaDAO _categoriaDAO = new CategoriaDAO();
        public IActionResult Index()
        {
            try
            {
                var listaDeCompras = _compraDAO.Listar();
                return View(listaDeCompras);       
            }
            catch (Exception erro)
            {
                TempData["ErrorMessage"] = $"Não foi possível carregar a lista de compras. Erro: {erro.Message}";
                return View(new List<Compra>());
            }
        }

        public IActionResult Criar()
        {

            ViewData["Title"] = "Cadastro de Compra";
            ViewData["Modo"] = "Criar";

            PreencherViewBags();


            var novaCompra = new Compra
            {
                DataEmissao = DateTime.Today,
                DataChegada = DateTime.Today,
                Status = true
            };
            return View(novaCompra);
        }

       

        private void PreencherViewBags()
        {
            var fornecedores = _fornecedorDAO.Listar();
            var produtos = _produtoDAO.Listar();
            var condicoes = _condicaoPagamentoDAO.Listar();

            ViewBag.Fornecedores = fornecedores ?? new List<Fornecedor>();
            ViewBag.Produtos = produtos ?? new List<Produto>();
            ViewBag.CondicoesPagamento = condicoes ?? new List<CondicaoPagamento>();

            var cidades = _cidadeDAO.Listar() ?? new List<Cidade>();
            var marcas = _marcaDAO.Listar() ?? new List<Marca>();
            var unidades = _unidadeMedidaDAO.Listar() ?? new List<UnidadeMedida>();
            var categorias = _categoriaDAO.Listar() ?? new List<Categoria>();

            ViewBag.CidadesItens = cidades.Select(c => new SelectListItem(c.NomeCidade, c.IdCidade.ToString())).ToList();
            ViewBag.MarcasItens = marcas.Select(m => new SelectListItem(m.Descricao, m.IdMarca.ToString())).ToList();
            ViewBag.UnidadesItens = unidades.Select(u => new SelectListItem(u.Descricao, u.IdUnidadeMedida.ToString())).ToList();
            ViewBag.CategoriasItens = categorias.Select(cat => new SelectListItem(cat.Descricao, cat.IdCategoria.ToString())).ToList();

            ViewBag.FornecedoresItens = fornecedores.Select(f => new SelectListItem(f.Nome_RazaoSocial, f.IdPessoa.ToString())).ToList();
            ViewBag.CondicoesPagamentoItens = condicoes.Select(c => new SelectListItem(c.Descricao, c.IdCondPgto.ToString())).ToList();
        }



        [HttpPost]
        public IActionResult Criar(Compra compra, string itensJson)
        {
            try
            {
                if (!string.IsNullOrEmpty(itensJson))
                {
                    compra.Itens = JsonSerializer.Deserialize<List<ItemCompra>>(itensJson);
                }

                if (compra.Itens == null || !compra.Itens.Any())
                {
                    ModelState.AddModelError("", "A compra deve ter pelo menos um item.");
                }

                
                if (ModelState.IsValid)
                {
                    _compraDAO.Inserir(compra);

                    return RedirectToAction("Index");
                }
            }
            catch (Exception erro)
            {
                ModelState.AddModelError("", "Ocorreu um erro ao salvar a compra: " + erro.Message);
            }

            ViewData["Title"] = "Cadastro de Compra";
            ViewData["Modo"] = "Criar";
            PreencherViewBags();
            return View(compra);
        }

        [HttpGet]       
        public IActionResult VerificarChaveExistente(string modelo, string serie, string numeroNota, int fornecedorId)
        {
            if (string.IsNullOrWhiteSpace(modelo) ||
                string.IsNullOrWhiteSpace(serie) ||
                string.IsNullOrWhiteSpace(numeroNota) ||
                fornecedorId <= 0)
            {
                return BadRequest(new { existe = false, mensagem = "Dados incompletos para verificação." });
            }

            try
            {
                Compra compraExistente = _compraDAO.BuscarDetalhesPorChaveComposta(modelo, serie, numeroNota, fornecedorId);

                if (compraExistente != null)
                {
                    return Json(new
                    {
                        existe = true,
                        compra = new
                        {      
                            modelo = compraExistente.Modelo,
                            serie = compraExistente.Serie,
                            numeroNota = compraExistente.NumeroNota,
                            dataEmissao = compraExistente.DataEmissao.ToString("yyyy-MM-dd"),   
                            dataChegada = compraExistente.DataChegada.ToString("yyyy-MM-dd"),   
                            fornecedorId = compraExistente.FornecedorId,
                            nomeFornecedor = compraExistente.NomeFornecedor,
                            observacoes = compraExistente.Observacoes ?? ""      
                        }
                    });
                }
                else
                {
                    return Json(new { existe = false });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { existe = false, mensagem = "Erro ao verificar a chave no banco de dados." });
            }
        }


        [HttpGet]
        public IActionResult Detalhes(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            if (string.IsNullOrEmpty(modelo) || string.IsNullOrEmpty(serie) || numeroNota <= 0 || fornecedorId <= 0)
            {
                return BadRequest("Chave da compra inválida.");
            }

            try
            {
                var compra = _compraDAO.Listar()
                    .FirstOrDefault(c =>
                        c.Modelo == modelo &&
                        c.Serie == serie &&
                        c.NumeroNota == numeroNota &&
                        c.FornecedorId == fornecedorId);

                if (compra == null)
                {
                    return NotFound("Compra não encontrada.");
                }

                compra.ParcelasGeradas = _contaAPagarDAO.ListarPorCompra(modelo, serie, numeroNota, fornecedorId);

                var condicao = _condicaoPagamentoDAO.BuscarPorId(compra.CondicaoPagamentoId);
                ViewBag.NomeCondicaoPagamento = condicao?.Descricao ?? "N/D";

                ViewData["Title"] = "Detalhes da Compra";
                ViewData["Modo"] = "Detalhes";

                ViewBag.TotalProdutosFormatado = compra.TotalProdutos.ToString("C2");
                ViewBag.TotalNotaFormatado = compra.TotalNota.ToString("C2");

                return View("Criar", compra);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao carregar detalhes: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult Cancelar(string modelo, string serie, int numeroNota, int fornecedorId)
        {
            var compra = _compraDAO.Listar()
                .FirstOrDefault(c =>
                    c.Modelo == modelo &&
                    c.Serie == serie &&
                    c.NumeroNota == numeroNota &&
                    c.FornecedorId == fornecedorId);

            if (compra == null) return NotFound();

            compra.ParcelasGeradas = _contaAPagarDAO.ListarPorCompra(modelo, serie, numeroNota, fornecedorId);

            var condicao = _condicaoPagamentoDAO.BuscarPorId(compra.CondicaoPagamentoId);
            ViewBag.NomeCondicaoPagamento = condicao?.Descricao ?? "N/D";

            ViewData["Title"] = "Cancelar Compra";
            ViewData["Modo"] = "Cancelar";

            PreencherViewBags();

            return View("Criar", compra);
        }

        [HttpPost]
        public IActionResult ConfirmarCancelamento(string modelo, string serie, int numeroNota, int fornecedorId, [FromForm] string motivoCancelamento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(motivoCancelamento))
                {
                    TempData["ErrorMessage"] = "O motivo do cancelamento é obrigatório.";
                    return RedirectToAction("Cancelar", new { modelo, serie, numeroNota, fornecedorId });
                }

                _compraDAO.Cancelar(modelo, serie, numeroNota, fornecedorId, motivoCancelamento);

                TempData["SuccessMessage"] = "Compra cancelada com sucesso.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erro ao cancelar: {ex.Message}";
                return RedirectToAction("Cancelar", new { modelo, serie, numeroNota, fornecedorId });
            }
        }
    }
}
