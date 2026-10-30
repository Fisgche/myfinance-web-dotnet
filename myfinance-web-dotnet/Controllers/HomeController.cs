using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_service.Dtos;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IDashboardService _dashboardService;

    public HomeController(ILogger<HomeController> logger, IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Dashboard do mes. <paramref name="mes"/> no formato "yyyy-MM"; ausente ou invalido usa o mes atual.
    /// </summary>
    public IActionResult Index(string? mes)
    {
        var hoje = DateTime.Today;
        var mesAtual = new DateTime(hoje.Year, hoje.Month, 1);
        var mesExibido = InterpretarMes(mes) ?? mesAtual;
        var mesAnterior = mesExibido.AddMonths(-1);

        var resumo = _dashboardService.ObterResumoMensal(mesExibido.Year, mesExibido.Month);
        var resumoAnterior = _dashboardService.ObterResumoMensal(mesAnterior.Year, mesAnterior.Month);

        var model = new DashboardModel()
        {
            Mes = mesExibido,
            DiaAtual = mesExibido == mesAtual ? hoje.Day : null,
            Resumo = resumo,
            Indicadores = new List<IndicadorModel>()
            {
                new() { Titulo = "Receitas", Serie = SeriesDashboard.Receitas, Valor = resumo.TotalReceitas, ValorAnterior = resumoAnterior.TotalReceitas },
                new() { Titulo = "Despesas", Serie = SeriesDashboard.Despesas, Valor = resumo.TotalDespesas, ValorAnterior = resumoAnterior.TotalDespesas, AumentoEhBom = false },
                new() { Titulo = "Investimentos", Serie = SeriesDashboard.Investimentos, Valor = resumo.TotalInvestimentos, ValorAnterior = resumoAnterior.TotalInvestimentos },
                new() { Titulo = "Saldo do mês", Valor = resumo.Saldo, ValorAnterior = resumoAnterior.Saldo, Descricao = "Receitas − despesas − investimentos" }
            },
            SeriesEvolucao = MontarSeriesEvolucao(resumo)
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static DateTime? InterpretarMes(string? mes)
    {
        if(DateTime.TryParseExact(mes, "yyyy-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
            && data.Year >= 1900 && data.Year <= 9998)
        {
            return data;
        }

        return null;
    }

    private static List<SerieModel> MontarSeriesEvolucao(ResumoMensal resumo)
    {
        var series = new List<SerieModel>();

        if(resumo.TotalReceitas != 0)
        {
            series.Add(new SerieModel() { Chave = SeriesDashboard.Receitas, Nome = "Receitas", Valores = resumo.EvolucaoDiaria.Select(x => x.Receitas).ToList() });
        }

        if(resumo.TotalDespesas != 0)
        {
            series.Add(new SerieModel() { Chave = SeriesDashboard.Despesas, Nome = "Despesas", Valores = resumo.EvolucaoDiaria.Select(x => x.Despesas).ToList() });
        }

        if(resumo.TotalInvestimentos != 0)
        {
            series.Add(new SerieModel() { Chave = SeriesDashboard.Investimentos, Nome = "Investimentos", Valores = resumo.EvolucaoDiaria.Select(x => x.Investimentos).ToList() });
        }

        return series;
    }
}
