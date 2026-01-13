using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class TransacaoController : Controller
    {
        private readonly ILogger<TransacaoController> _logger;

        private readonly IPlanoContaService _planoContaService;

        private readonly ITransacaoService _transacaoService;

        public TransacaoController(ILogger<TransacaoController> logger, ITransacaoService transacaoService, IPlanoContaService planoContaService)
        {
            _logger = logger;
            _transacaoService = transacaoService;
            _planoContaService = planoContaService;
        }

        [HttpGet]
        [Route("Index")]
        public IActionResult Index()
        {
            var listaTransacao = _transacaoService.ListarRegistros();
            List<TransacaoModel> transacaoModels = new List<TransacaoModel>();

            foreach(var item in listaTransacao)
            {
                var itemTransacao= new TransacaoModel();
                itemTransacao.Id = item.Id;
                itemTransacao.Historico = item.Historico;
                itemTransacao.Date = item.Date;
                itemTransacao.PlanoConta = item.PlanoConta;
                itemTransacao.PlanoContaId = item.PlanoContaId;
                itemTransacao.Valor = item.Valor;

                transacaoModels.Add(itemTransacao);
            }

            ViewBag.ListaTransacao = transacaoModels;
            
            return View();
        }

        [HttpGet]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        public IActionResult Cadastrar(int? Id)
        {
            var listaPlanoConta = new SelectList(_planoContaService.ListarRegistros(),"Id", "Descricao");

            var transacaoModel = new TransacaoModel()
            {
                ListPlanoConta = listaPlanoConta,
                Date = DateTime.Now
            }; 

             if(Id != null){
                var transacao = _transacaoService.RetornarRegistro((int) Id);
                transacaoModel.Id = transacao.Id;
                transacaoModel.Date = transacao.Date;
                transacaoModel.Historico = transacao.Historico;
                transacaoModel.PlanoConta = transacao.PlanoConta;
                transacaoModel.PlanoContaId = transacao.PlanoContaId;
                transacaoModel.Valor = transacao.Valor;                
            } 

            return View(transacaoModel);

        }

        [HttpPost]
        [Route("Cadastrar")]
        [Route("Cadastrar/{Id}")]
        public IActionResult Cadastrar(TransacaoModel model)
        {
            Transacao transacao = new Transacao();

            transacao.Id = model.Id;
            transacao.Historico = model.Historico;
            transacao.Date = model.Date;
            transacao.PlanoConta = model.PlanoConta;
            transacao.PlanoContaId = model.PlanoContaId;
            transacao.Valor = model.Valor;

            _transacaoService.Cadastrar(transacao);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Excluir/{Id}")]
        public IActionResult Excluir(int? Id)
        {
            _transacaoService.Excluir((int)Id);

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}