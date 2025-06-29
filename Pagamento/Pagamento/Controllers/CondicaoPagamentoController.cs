using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pagamento.DAO;
using Pagamento.Models;

namespace Pagamento.Controllers
{
    public class CondicaoPagamentoController : Controller
    {
        private readonly CondicaoPagamentoDAO condicaodao = new CondicaoPagamentoDAO();
        private readonly FormaPagamentoDAO formaPgtoDAO = new FormaPagamentoDAO(); 
        private readonly ParcelaCondicaoPagamentoDAO parcelaDAO = new ParcelaCondicaoPagamentoDAO();

        public IActionResult Index()
        {
            List<CondicaoPagamento> lista = condicaodao.Listar();
            return View(lista);
        }


        [HttpGet]
        public IActionResult Criar()
        {
            var model = new CondicaoPagamento
            {
                FormasPagamento = formaPgtoDAO.Listar() ?? new List<FormaPagamento>() 
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Criar(CondicaoPagamento condicaoPagamento, string ParcelasJson)
        {
            if (ModelState.IsValid)
            {
                condicaoPagamento.IdCondPgto = condicaodao.Inserir(condicaoPagamento);

                List<ParcelaCondicaoPagamento> parcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);

                foreach (var parcela in parcelas)
                {
                    parcela.IdCondPgto = condicaoPagamento.IdCondPgto; 
                    parcelaDAO.Inserir(parcela); 
                }

                return RedirectToAction("Index");
            }

            condicaoPagamento.FormasPagamento = formaPgtoDAO.Listar() ?? new List<FormaPagamento>();
            return View(condicaoPagamento);
        }


        


        [HttpGet]
        public IActionResult Editar(int id)
        {
            var condicao = condicaodao.BuscarPorId(id);
            if (condicao == null) return NotFound();

            condicao.Parcelas = parcelaDAO.ListarPorCondicaoPagamento(id);
            condicao.FormasPagamento = formaPgtoDAO.Listar();

            return View(condicao);
        }


        

        [HttpPost]
        public IActionResult Editar(CondicaoPagamento condicao, string ParcelasJson)
        {
            condicaodao.Atualizar(condicao);

            var novasParcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);
            var parcelasAtuais = parcelaDAO.ListarPorCondicaoPagamento(condicao.IdCondPgto);

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
                        parcelaDAO.Atualizar(nova); 
                    }
                }
                else
                {
                    parcelaDAO.Inserir(nova); 
                }
            }

            foreach (var antiga in parcelasAtuais)
            {
                bool aindaExiste = novasParcelas.Any(p =>
                    p.NumeroParcela == antiga.NumeroParcela &&
                    p.IdFormaPgto == antiga.IdFormaPgto);

                if (!aindaExiste)
                {
                    parcelaDAO.Excluir(antiga.IdCondPgto, antiga.NumeroParcela, antiga.IdFormaPgto);
                }
            }

            return RedirectToAction("Index");
        }




        public IActionResult Excluir(int id)
        {
            var condicao = condicaodao.BuscarPorId(id);
            if (condicao == null) return NotFound();

            condicao.Parcelas = parcelaDAO.ListarPorCondicaoPagamento(id); 

            return View(condicao);
        }

        [HttpPost]
        public IActionResult Excluir(CondicaoPagamento condicao)
        {
            condicaodao.Excluir(condicao.IdCondPgto);
            

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult FormModal()
        {
            var condicao = new CondicaoPagamento
            {
                FormasPagamento = formaPgtoDAO.Listar() ?? new List<FormaPagamento>()
            };

            return PartialView("FormCondicaoPagamento", condicao);
        }


        

        [HttpPost]
        public IActionResult FormModal(CondicaoPagamento condicao, string ParcelasJson)
        {
            if (!ModelState.IsValid)
            {
                condicao.FormasPagamento = formaPgtoDAO.Listar() ?? new List<FormaPagamento>();

                ViewBag.ParcelasJson = ParcelasJson;

                return PartialView("FormCondicaoPagamento", condicao);
            }

            condicao.IdCondPgto = condicaodao.Inserir(condicao);

            var parcelas = JsonConvert.DeserializeObject<List<ParcelaCondicaoPagamento>>(ParcelasJson);
            foreach (var parcela in parcelas)
            {
                parcela.IdCondPgto = condicao.IdCondPgto;
                parcelaDAO.Inserir(parcela);
            }

            return Json(new
            {
                sucesso = true,
                condicao = new
                {
                    id = condicao.IdCondPgto,
                    nome = condicao.Descricao
                }
            });
        }






    }
}
