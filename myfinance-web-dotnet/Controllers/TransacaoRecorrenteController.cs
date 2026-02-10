using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service;
using myfinance_web_dotnet_service.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace myfinance_web_dotnet.Controllers
{
    [Route("[controller]")]
    public class TransacaoRecorrenteController : Controller
    {
        private readonly ILogger<PlanoContaController> _logger;

        private readonly ITransacaoRecorrenteService _transacaoRecorrenteService;

        public TransacaoRecorrenteController(ILogger<PlanoContaController> logger, ITransacaoRecorrenteService transacaoRecorrenteService)
        {
            _logger = logger;
            _transacaoRecorrenteService = transacaoRecorrenteService;
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


    }
}
