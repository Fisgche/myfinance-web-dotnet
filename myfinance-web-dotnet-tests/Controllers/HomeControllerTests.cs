using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using myfinance_web_dotnet.Controllers;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_service.Dtos;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IDashboardService> _dashboardService = new();
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _dashboardService
            .Setup(s => s.ObterResumoMensal(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int ano, int mes) => new ResumoMensal { Ano = ano, Mes = mes });
        _controller = new HomeController(NullLogger<HomeController>.Instance, _dashboardService.Object);
    }

    private DashboardModel ExecutarIndex(string? mes)
    {
        var resultado = Assert.IsType<ViewResult>(_controller.Index(mes));
        return Assert.IsType<DashboardModel>(resultado.Model);
    }

    private void ConfigurarResumo(int ano, int mes, ResumoMensal resumo)
    {
        _dashboardService.Setup(s => s.ObterResumoMensal(ano, mes)).Returns(resumo);
    }

    // ---------- Mes exibido ----------

    [Fact]
    public void Index_SemMes_ExibeOMesAtual()
    {
        var hoje = DateTime.Today;

        var model = ExecutarIndex(null);

        Assert.Equal(new DateTime(hoje.Year, hoje.Month, 1), model.Mes);
        Assert.Equal(hoje.Day, model.DiaAtual);
        Assert.True(model.EhMesAtual);
        _dashboardService.Verify(s => s.ObterResumoMensal(hoje.Year, hoje.Month), Times.Once);
    }

    [Fact]
    public void Index_ComMes_ExibeOMesInformadoEComparaComOAnterior()
    {
        var model = ExecutarIndex("2020-03");

        Assert.Equal(new DateTime(2020, 3, 1), model.Mes);
        Assert.Null(model.DiaAtual);
        Assert.False(model.EhMesAtual);
        Assert.Equal(2020, model.Resumo.Ano);
        Assert.Equal(3, model.Resumo.Mes);
        _dashboardService.Verify(s => s.ObterResumoMensal(2020, 3), Times.Once);
        _dashboardService.Verify(s => s.ObterResumoMensal(2020, 2), Times.Once);
    }

    [Fact]
    public void Index_Janeiro_ComparaComDezembroDoAnoAnterior()
    {
        var model = ExecutarIndex("2021-01");

        Assert.Equal(new DateTime(2020, 12, 1), model.MesAnterior);
        Assert.Equal(new DateTime(2021, 2, 1), model.ProximoMes);
        _dashboardService.Verify(s => s.ObterResumoMensal(2020, 12), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("2026-13")]
    [InlineData("10/2026")]
    [InlineData("1800-01")]
    [InlineData("9999-12")]
    public void Index_MesInvalido_ExibeOMesAtual(string mes)
    {
        var hoje = DateTime.Today;

        var model = ExecutarIndex(mes);

        Assert.Equal(new DateTime(hoje.Year, hoje.Month, 1), model.Mes);
    }

    // ---------- Indicadores ----------

    [Fact]
    public void Index_MontaOsIndicadoresComAComparacaoDoMesAnterior()
    {
        ConfigurarResumo(2020, 3, new ResumoMensal { TotalReceitas = 5000m, TotalDespesas = 1500m, TotalInvestimentos = 500m });
        ConfigurarResumo(2020, 2, new ResumoMensal { TotalReceitas = 4000m, TotalDespesas = 2000m, TotalInvestimentos = 500m });

        var indicadores = ExecutarIndex("2020-03").Indicadores;

        Assert.Collection(indicadores,
            i => { Assert.Equal("Receitas", i.Titulo); Assert.Equal(SeriesDashboard.Receitas, i.Serie); Assert.Equal(5000m, i.Valor); Assert.Equal(1000m, i.Variacao); Assert.Equal(SituacaoVariacao.Boa, i.Situacao); },
            i => { Assert.Equal("Despesas", i.Titulo); Assert.Equal(SeriesDashboard.Despesas, i.Serie); Assert.Equal(1500m, i.Valor); Assert.Equal(-500m, i.Variacao); Assert.Equal(SituacaoVariacao.Boa, i.Situacao); },
            i => { Assert.Equal("Investimentos", i.Titulo); Assert.Equal(SeriesDashboard.Investimentos, i.Serie); Assert.Equal(500m, i.Valor); Assert.Equal(SituacaoVariacao.Estavel, i.Situacao); },
            i => { Assert.Equal("Saldo do mês", i.Titulo); Assert.Null(i.Serie); Assert.Equal(3000m, i.Valor); Assert.Equal(1500m, i.ValorAnterior); Assert.Equal(SituacaoVariacao.Boa, i.Situacao); });
    }

    [Fact]
    public void Index_AumentoDeDespesasEQuedaDeReceitas_SaoRuins()
    {
        ConfigurarResumo(2020, 3, new ResumoMensal { TotalReceitas = 3000m, TotalDespesas = 2500m });
        ConfigurarResumo(2020, 2, new ResumoMensal { TotalReceitas = 4000m, TotalDespesas = 2000m });

        var indicadores = ExecutarIndex("2020-03").Indicadores;

        Assert.Equal(SituacaoVariacao.Ruim, indicadores[0].Situacao); // receitas caíram
        Assert.Equal(SituacaoVariacao.Ruim, indicadores[1].Situacao); // despesas subiram
        Assert.Equal(SituacaoVariacao.Ruim, indicadores[3].Situacao); // saldo caiu
    }

    // ---------- Grafico de evolucao ----------

    [Fact]
    public void Index_SeriesDaEvolucao_SoTiposComMovimentoNoMes()
    {
        var evolucao = new List<AcumuladoDiario>
        {
            new() { Data = new DateTime(2020, 3, 1), Receitas = 0m, Despesas = 100m },
            new() { Data = new DateTime(2020, 3, 2), Receitas = 5000m, Despesas = 100m }
        };
        ConfigurarResumo(2020, 3, new ResumoMensal { TotalReceitas = 5000m, TotalDespesas = 100m, EvolucaoDiaria = evolucao });

        var series = ExecutarIndex("2020-03").SeriesEvolucao;

        Assert.Collection(series,
            s => { Assert.Equal(SeriesDashboard.Receitas, s.Chave); Assert.Equal("Receitas", s.Nome); Assert.Equal(new[] { 0m, 5000m }, s.Valores); },
            s => { Assert.Equal(SeriesDashboard.Despesas, s.Chave); Assert.Equal("Despesas", s.Nome); Assert.Equal(new[] { 100m, 100m }, s.Valores); });
    }

    [Fact]
    public void Index_MesSemMovimento_NaoTemSeries()
    {
        Assert.Empty(ExecutarIndex("2020-03").SeriesEvolucao);
    }
}
