using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class TransacaoRecorrenteController : Controller
    {
        private readonly ILogger<TransacaoRecorrenteController> _logger;

        private readonly ITransacaoRecorrenteService _transacaoRecorrenteService;

        private readonly IPlanoContaService _planoContaService;


        public TransacaoRecorrenteController(ILogger<TransacaoRecorrenteController> logger, ITransacaoRecorrenteService transacaoRecorrenteService, IPlanoContaService planoContaService)
        {
            _logger = logger;
            _transacaoRecorrenteService = transacaoRecorrenteService;
            _planoContaService = planoContaService;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            var listaTransacaoRecorrente = _transacaoRecorrenteService.ListarRegistros();
            List<TransacaoRecorrenteModel> transacaoRecorrenteModels = new List<TransacaoRecorrenteModel>();
            foreach (var item in listaTransacaoRecorrente)
            {
                var itemTransacaoRecorrente = new TransacaoRecorrenteModel();
                itemTransacaoRecorrente.Id = item.Id;
                itemTransacaoRecorrente.DataInicio = item.DataInicio;
                itemTransacaoRecorrente.DataFim = item.DataFim;
                itemTransacaoRecorrente.Valor = item.Valor;
                itemTransacaoRecorrente.PlanoConta = item.PlanoConta;
                itemTransacaoRecorrente.PlanoContaId = item.PlanoContaId;

                transacaoRecorrenteModels.Add(itemTransacaoRecorrente);
            }

            ViewBag.ListaTransacaoRecorrente = transacaoRecorrenteModels;

            return View();
        }

        [HttpGet]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        public IActionResult Cadastrar(int? Id)
        {
            var transacaoRecorrenteModel = new TransacaoRecorrenteModel()
            {
                ListPlanoConta = ListarPlanoContas(),
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now
            };

            if (Id != null)
            {
                var transacaoRecorrente = _transacaoRecorrenteService.RetornarRegistro((int)Id);
                if (transacaoRecorrente == null)
                {
                    return NotFound();
                }

                transacaoRecorrenteModel.Id = transacaoRecorrente.Id;
                transacaoRecorrenteModel.DataInicio = transacaoRecorrente.DataInicio;
                transacaoRecorrenteModel.DataFim = transacaoRecorrente.DataFim;
                transacaoRecorrenteModel.Valor = transacaoRecorrente.Valor;
                transacaoRecorrenteModel.PlanoConta = transacaoRecorrente.PlanoConta;
                transacaoRecorrenteModel.PlanoContaId = transacaoRecorrente.PlanoContaId;
                
            }

            return View(transacaoRecorrenteModel);

        }

        [HttpPost]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(TransacaoRecorrenteModel model)
        {
            if (model.DataFim < model.DataInicio)
            {
                ModelState.AddModelError(nameof(model.DataFim), "A data de fim deve ser igual ou posterior a data de inicio.");
            }

            if (!ModelState.IsValid)
            {
                model.ListPlanoConta = ListarPlanoContas();
                return View(model);
            }

            TransacaoRecorrente transacaoRecorrente = new TransacaoRecorrente();

            transacaoRecorrente.Id = model.Id;
            transacaoRecorrente.DataInicio = model.DataInicio;
            transacaoRecorrente.DataFim = model.DataFim;
            transacaoRecorrente.Valor = model.Valor;
            transacaoRecorrente.PlanoContaId = model.PlanoContaId;

            _transacaoRecorrenteService.Cadastrar(transacaoRecorrente);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Excluir/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int Id)
        {
            try
            {
                _transacaoRecorrenteService.Excluir(Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Falha ao excluir a transacao recorrente {Id}", Id);
                TempData["Erro"] = "Nao foi possivel excluir a transacao recorrente.";
            }

            return RedirectToAction("Index");
        }

        private SelectList ListarPlanoContas()
        {
            return new SelectList(_planoContaService.ListarRegistros(), "Id", "Descricao");
        }
    }
}
