using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class PlanoContaController : Controller
    {
        private readonly ILogger<PlanoContaController> _logger;

        private readonly IPlanoContaService _planoContaService;

        public PlanoContaController(ILogger<PlanoContaController> logger, IPlanoContaService planoContaService)
        {
            _logger = logger;
            _planoContaService = planoContaService;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            var listaPlanoContas = _planoContaService.ListarRegistros();
            List<PlanoContaModel> planoContaModels = new List<PlanoContaModel>();
            foreach(var item in listaPlanoContas)
            {
                var itemPlanoConta = new PlanoContaModel();
                itemPlanoConta.Id = item.Id;
                itemPlanoConta.Descricao = item.Descricao;
                itemPlanoConta.Tipo = item.Tipo;
                planoContaModels.Add(itemPlanoConta);
            }
            ViewBag.ListaPlanoConta = planoContaModels;
            
            return View();
        }

        [HttpGet]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        public IActionResult Cadastrar(int? Id)
        {
            if(Id != null){
                var planoConta = _planoContaService.RetornarRegistro((int) Id);
                if(planoConta == null)
                {
                    return NotFound();
                }

                var planoContaModel = new PlanoContaModel()
                {
                    Id = planoConta.Id,
                    Descricao = planoConta.Descricao,
                    Tipo = planoConta.Tipo
                };
                return View(planoContaModel);
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Cadastrar(PlanoContaModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            PlanoConta planoConta = new PlanoConta();

            planoConta.Descricao = model.Descricao;
            planoConta.Id = model.Id;
            planoConta.Tipo = model.Tipo;

            _planoContaService.Cadastrar(planoConta);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("Excluir/{Id}")]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int Id)
        {
            try
            {
                _planoContaService.Excluir(Id);
            }
            catch(DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Falha ao excluir o plano de conta {Id}", Id);
                TempData["Erro"] = "Nao foi possivel excluir o plano de conta. Verifique se existem transacoes vinculadas a ele.";
            }

            return RedirectToAction("Index");
        }
    }
}
