using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class TransacaoController : Controller
    {
        public const string AbaUnicas = "unicas";
        public const string AbaRecorrentes = "recorrentes";

        private readonly ILogger<TransacaoController> _logger;

        private readonly IPlanoContaService _planoContaService;

        private readonly ITransacaoService _transacaoService;

        private readonly ITransacaoRecorrenteService _transacaoRecorrenteService;

        public TransacaoController(ILogger<TransacaoController> logger, ITransacaoService transacaoService, ITransacaoRecorrenteService transacaoRecorrenteService, IPlanoContaService planoContaService)
        {
            _logger = logger;
            _transacaoService = transacaoService;
            _transacaoRecorrenteService = transacaoRecorrenteService;
            _planoContaService = planoContaService;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index(string? aba)
        {
            List<TransacaoModel> transacaoModels = new List<TransacaoModel>();
            foreach(var item in _transacaoService.ListarRegistros())
            {
                transacaoModels.Add(new TransacaoModel()
                {
                    Id = item.Id,
                    Historico = item.Historico,
                    Date = item.Date,
                    PlanoConta = item.PlanoConta,
                    PlanoContaId = item.PlanoContaId,
                    Valor = item.Valor
                });
            }

            List<TransacaoModel> transacaoRecorrenteModels = new List<TransacaoModel>();
            foreach(var item in _transacaoRecorrenteService.ListarRegistros())
            {
                transacaoRecorrenteModels.Add(new TransacaoModel()
                {
                    Id = item.Id,
                    Recorrente = true,
                    DataInicio = item.DataInicio,
                    DataFim = item.DataFim,
                    PlanoConta = item.PlanoConta,
                    PlanoContaId = item.PlanoContaId,
                    Valor = item.Valor
                });
            }

            ViewBag.ListaTransacao = transacaoModels;
            ViewBag.ListaTransacaoRecorrente = transacaoRecorrenteModels;
            ViewBag.Aba = aba == AbaRecorrentes ? AbaRecorrentes : AbaUnicas;

            return View();
        }

        [HttpGet]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        public IActionResult Cadastrar(int? Id, bool recorrente = false)
        {
            var transacaoModel = NovoModel(recorrente && Id == null);

            if(Id != null){
                var transacao = _transacaoService.RetornarRegistro((int) Id);
                if(transacao == null)
                {
                    return NotFound();
                }

                transacaoModel.Id = transacao.Id;
                transacaoModel.Date = transacao.Date;
                transacaoModel.Historico = transacao.Historico;
                transacaoModel.PlanoConta = transacao.PlanoConta;
                transacaoModel.PlanoContaId = transacao.PlanoContaId;
                transacaoModel.Valor = transacao.Valor;
            }

            return View(transacaoModel);
        }

        [HttpGet]
        [Route("CadastrarRecorrente/{Id}")]
        public IActionResult CadastrarRecorrente(int Id)
        {
            var transacaoRecorrente = _transacaoRecorrenteService.RetornarRegistro(Id);
            if(transacaoRecorrente == null)
            {
                return NotFound();
            }

            var transacaoModel = NovoModel(true);
            transacaoModel.Id = transacaoRecorrente.Id;
            transacaoModel.DataInicio = transacaoRecorrente.DataInicio;
            transacaoModel.DataFim = transacaoRecorrente.DataFim;
            transacaoModel.Valor = transacaoRecorrente.Valor;
            transacaoModel.PlanoConta = transacaoRecorrente.PlanoConta;
            transacaoModel.PlanoContaId = transacaoRecorrente.PlanoContaId;

            return View("Cadastrar", transacaoModel);
        }

        [HttpPost]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(TransacaoModel model)
        {
            ValidarPorTipo(model);

            if(!ModelState.IsValid)
            {
                model.ListPlanoConta = ListarPlanoContas();
                return View("Cadastrar", model);
            }

            if(model.Recorrente)
            {
                _transacaoRecorrenteService.Cadastrar(new TransacaoRecorrente()
                {
                    Id = model.Id,
                    DataInicio = model.DataInicio,
                    DataFim = model.DataFim,
                    Valor = model.Valor,
                    PlanoContaId = model.PlanoContaId
                });

                return RedirectToAction("Index", new { aba = AbaRecorrentes });
            }

            _transacaoService.Cadastrar(new Transacao()
            {
                Id = model.Id,
                Historico = model.Historico,
                Date = model.Date,
                PlanoContaId = model.PlanoContaId,
                Valor = model.Valor
            });

            return RedirectToAction("Index", new { aba = AbaUnicas });
        }

        [HttpPost]
        [Route("Excluir/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int Id)
        {
            try
            {
                _transacaoService.Excluir(Id);
            }
            catch(DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Falha ao excluir a transacao {Id}", Id);
                TempData["Erro"] = "Nao foi possivel excluir a transacao.";
            }

            return RedirectToAction("Index", new { aba = AbaUnicas });
        }

        [HttpPost]
        [Route("ExcluirRecorrente/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirRecorrente(int Id)
        {
            try
            {
                _transacaoRecorrenteService.Excluir(Id);
            }
            catch(DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Falha ao excluir a transacao recorrente {Id}", Id);
                TempData["Erro"] = "Nao foi possivel excluir a transacao recorrente.";
            }

            return RedirectToAction("Index", new { aba = AbaRecorrentes });
        }

        private void ValidarPorTipo(TransacaoModel model)
        {
            if(model.Recorrente)
            {
                if(model.DataFim < model.DataInicio)
                {
                    ModelState.AddModelError(nameof(model.DataFim), "A data de fim deve ser igual ou posterior a data de inicio.");
                }
            }
            else if(string.IsNullOrWhiteSpace(model.Historico))
            {
                ModelState.AddModelError(nameof(model.Historico), "Informe o historico.");
            }
        }

        private TransacaoModel NovoModel(bool recorrente)
        {
            return new TransacaoModel()
            {
                Recorrente = recorrente,
                ListPlanoConta = ListarPlanoContas(),
                Date = DateTime.Now,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now
            };
        }

        private SelectList ListarPlanoContas()
        {
            return new SelectList(_planoContaService.ListarRegistros(), "Id", "Descricao");
        }
    }
}
