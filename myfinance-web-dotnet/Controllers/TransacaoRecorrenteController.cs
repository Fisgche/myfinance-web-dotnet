using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service;
using myfinance_web_dotnet_service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class TransacaoRecorrenteController : Controller
    {
        private readonly ILogger<PlanoContaController> _logger;

        private readonly ITransacaoRecorrenteService _transacaoRecorrenteService;

        private readonly IPlanoContaService _planoContaService;


        public TransacaoRecorrenteController(ILogger<PlanoContaController> logger, ITransacaoRecorrenteService transacaoRecorrenteService, IPlanoContaService planoContaService)
        {
            _logger = logger;
            _transacaoRecorrenteService = transacaoRecorrenteService;
            _planoContaService = planoContaService;
        }

        [HttpGet]
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
            var listaPlanoConta = new SelectList(_planoContaService.ListarRegistros(), "Id", "Descricao");

            var transacaoRecorrenteModel = new TransacaoRecorrenteModel()
            {
                ListPlanoConta = listaPlanoConta,
                DataInicio = DateTime.Now,
                DataFim = DateTime.Now
            };

            if (Id != null)
            {
                var transacaoRecorrente = _transacaoRecorrenteService.RetornarRegistro((int)Id);

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
        public IActionResult Cadastrar(TransacaoRecorrenteModel model)
        {
            TransacaoRecorrente transacaoRecorrente = new TransacaoRecorrente();

            transacaoRecorrente.Id = model.Id;
            transacaoRecorrente.DataInicio = model.DataInicio;
            transacaoRecorrente.DataFim = model.DataFim;
            transacaoRecorrente.Valor = model.Valor;
            transacaoRecorrente.PlanoConta = model.PlanoConta;
            transacaoRecorrente.PlanoContaId = model.PlanoContaId;

            _transacaoRecorrenteService.Cadastrar(transacaoRecorrente);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("Excluir/{Id}")]
        public IActionResult Excluir(int? Id)
        {
            _transacaoRecorrenteService.Excluir((int)Id);

            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }


    }
}
