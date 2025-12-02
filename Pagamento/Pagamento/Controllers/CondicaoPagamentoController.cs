using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class CondicaoPagamentoController : Controller
    {
        private readonly CondicaoPagamentoDAO _condicaoDAO;
        private readonly FormaPagamentoDAO _formaPgtoDAO;
        private readonly ParcelaCondicaoPagamentoDAO _parcelaDAO;

        public CondicaoPagamentoController(
            CondicaoPagamentoDAO condicaoDAO,
            FormaPagamentoDAO formaPgtoDAO,
            ParcelaCondicaoPagamentoDAO parcelaDAO)
        {
            _condicaoDAO = condicaoDAO;
            _formaPgtoDAO = formaPgtoDAO;
            _parcelaDAO = parcelaDAO;
        }
        public IActionResult Index()
        {
            List<CondicaoPagamento> lista = _condicaoDAO.Listar();
            return View(lista);
        }


        [HttpGet]
        public IActionResult Criar()
        {
            var model = new CondicaoPagamento
            {
                FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>() 
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Criar(CondicaoPagamento condicaoPagamento, string ParcelasJson)
        {

            if (_condicaoDAO.ExisteCondicao(condicaoPagamento.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta condição de pagamento já está cadastrada!");
                condicaoPagamento.FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>();
                return View(condicaoPagamento);
            }


            if (string.IsNullOrEmpty(ParcelasJson))
            {
                ModelState.AddModelError("", "É necessário adicionar pelo menos uma parcela.");
                condicaoPagamento.FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>();
                return View(condicaoPagamento);
            }
            if (ModelState.IsValid)
            {
                condicaoPagamento.IdCondPgto = _condicaoDAO.Inserir(condicaoPagamento);

                List<ParcelaCondicaoPagamento> parcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);

                foreach (var parcela in parcelas)
                {
                    parcela.IdCondPgto = condicaoPagamento.IdCondPgto;
                    _parcelaDAO.Inserir(parcela); 
                }
                TempData["SuccessMessage"] = "Condição de pagamento cadastrada com sucesso!";
                return RedirectToAction("Index");
            }

            condicaoPagamento.FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>();
            return View(condicaoPagamento);
        }


        


        [HttpGet]
        public IActionResult Editar(int id)
        {
            var condicao = _condicaoDAO.BuscarPorId(id);
            if (condicao == null) return NotFound();

            condicao.Parcelas = _parcelaDAO.ListarPorCondicaoPagamento(id);
            condicao.FormasPagamento = _formaPgtoDAO.Listar();

            return View(condicao);
        }


        

        [HttpPost]
        public IActionResult Editar(CondicaoPagamento condicao, string ParcelasJson)
        {

            if (string.IsNullOrEmpty(ParcelasJson))
            {
                ModelState.AddModelError("", "As parcelas não foram enviadas corretamente. Verifique se o JavaScript está habilitado.");

                condicao.FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>();
                condicao.Parcelas = _parcelaDAO.ListarPorCondicaoPagamento(condicao.IdCondPgto);

                return View(condicao);
            }
            _condicaoDAO.Atualizar(condicao);

            var novasParcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);
            var parcelasAtuais = _parcelaDAO.ListarPorCondicaoPagamento(condicao.IdCondPgto);

            foreach (var nova in novasParcelas)
            {
                nova.IdCondPgto = condicao.IdCondPgto;
                var existente = parcelasAtuais.FirstOrDefault(p =>
                    p.NumeroParcela == nova.NumeroParcela &&
                    p.IdFormaPgto == nova.IdFormaPgto);

                if (existente != null)
                {
                    if (existente.ValorPercentual != nova.ValorPercentual ||
                        existente.DiasAposVenda != nova.DiasAposVenda)
                    {
                        _parcelaDAO.Atualizar(nova); 
                    }
                }
                else
                {
                    _parcelaDAO.Inserir(nova); 
                }
            }

            foreach (var antiga in parcelasAtuais)
            {
                bool aindaExiste = novasParcelas.Any(p =>
                    p.NumeroParcela == antiga.NumeroParcela &&
                    p.IdFormaPgto == antiga.IdFormaPgto);

                if (!aindaExiste)
                {
                    _parcelaDAO.Excluir(antiga.IdCondPgto, antiga.NumeroParcela, antiga.IdFormaPgto);
                }
            }

            TempData["SuccessMessage"] = "Condição de pagamento atualizada com sucesso!";

            return RedirectToAction("Index");
        }




        public IActionResult Excluir(int id)
        {
            var condicao = _condicaoDAO.BuscarPorId(id);
            if (condicao == null) return NotFound();

            condicao.Parcelas = _parcelaDAO.ListarPorCondicaoPagamento(id); 

            return View(condicao);
        }

        [HttpPost]
        public IActionResult Excluir(CondicaoPagamento condicao)
        {
             try
            {
                _condicaoDAO.Excluir(condicao.IdCondPgto);

                TempData["SuccessMessage"] = "Condicao de pagamento excluída com sucesso!";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                if (ex.Number == 1451)
                {
                    TempData["ErrorMessage"] = "Esta condicao de pagamento nao pode ser excluída,pois esta sendo utilizada em outro cadastro.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Ocorreu um erro de banco de dados ao tentar excluir a condicao de pagamento.";
                }
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Ocorreu um erro inesperado no sistema.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult FormModal()
        {
            var condicao = new CondicaoPagamento
            {
                FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>()
            };

            return PartialView("FormCondicaoPagamento", condicao);
        }


        

        [HttpPost]
        public IActionResult FormModal(CondicaoPagamento condicao, string ParcelasJson)
        {

            if (_condicaoDAO.ExisteCondicao(condicao.Descricao))
            {
                ModelState.AddModelError("Descricao", "Esta condição de pagamento já está cadastrada!");
                
            }

            if (!ModelState.IsValid)
            {
                condicao.FormasPagamento = _formaPgtoDAO.Listar() ?? new List<FormaPagamento>();

                ViewBag.ParcelasJson = ParcelasJson;

                return PartialView("FormCondicaoPagamento", condicao);
            }

            condicao.IdCondPgto = _condicaoDAO.Inserir(condicao);

            var parcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);
            foreach (var parcela in parcelas)
            {
                parcela.IdCondPgto = condicao.IdCondPgto;
                _parcelaDAO.Inserir(parcela);
            }

            return Json(new
            {
                sucesso = true,
                condicao = new
                {
                    id = condicao.IdCondPgto,
                    nome = condicao.Descricao.ToUpper()
                }
            });
        }


        [HttpGet]
        public IActionResult ObterDetalhes(int id)
        {
            try
            {
                var condicao = _condicaoDAO.BuscarPorId(id);
                if (condicao == null)
                {
                    return NotFound();        
                }

                condicao.Parcelas = _parcelaDAO.ListarPorCondicaoPagamento(id);

                return Json(condicao);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro interno ao buscar detalhes da condição: " + ex.Message });
            }
        }

        [HttpGet]
        public IActionResult BuscarPorIdJSON(int id)
        {
            var cond = _condicaoDAO.BuscarPorId(id);     

            if (cond == null)
                return NotFound(new { mensagem = "Condição não encontrada." });

            return Json(new
            {
                idCondPgto = cond.IdCondPgto,
                descricao = cond.Descricao
            });
        }

    }
}
