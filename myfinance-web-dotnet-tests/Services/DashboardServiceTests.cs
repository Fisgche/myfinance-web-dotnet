using Moq;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_domain.Enums;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service;
using myfinance_web_dotnet_service.Dtos;

namespace myfinance_web_dotnet_tests.Services;

public class DashboardServiceTests
{
    private static readonly PlanoConta Salario = new() { Id = 1, Descricao = "Salario", Tipo = "R" };
    private static readonly PlanoConta Mercado = new() { Id = 2, Descricao = "Mercado", Tipo = "D" };
    private static readonly PlanoConta Aluguel = new() { Id = 3, Descricao = "Aluguel", Tipo = "D" };
    private static readonly PlanoConta Cofrinho = new() { Id = 4, Descricao = "Cofrinho", Tipo = "I" };

    private readonly Mock<ITransacaoRepository> _transacaoRepository = new();
    private readonly Mock<ITransacaoRecorrenteRepository> _transacaoRecorrenteRepository = new();
    private readonly List<Transacao> _transacoes = new();
    private readonly List<TransacaoRecorrente> _recorrentes = new();
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _transacaoRepository.Setup(r => r.ListarPorPeriodo(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(_transacoes);
        _transacaoRecorrenteRepository.Setup(r => r.ListarVigentesNoPeriodo(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(_recorrentes);
        _service = new DashboardService(_transacaoRepository.Object, _transacaoRecorrenteRepository.Object);
    }

    private static Transacao NovaTransacao(int id, DateTime data, decimal valor, PlanoConta planoConta, string historico = "Teste")
    {
        return new Transacao { Id = id, Date = data, Valor = valor, Historico = historico, PlanoContaId = planoConta.Id!.Value, PlanoConta = planoConta };
    }

    private static TransacaoRecorrente NovaRecorrente(int id, DateTime inicio, DateTime fim, decimal valor, PlanoConta planoConta, Frequencia frequencia = Frequencia.Mensal)
    {
        return new TransacaoRecorrente { Id = id, DataInicio = inicio, DataFim = fim, Valor = valor, Frequencia = frequencia, PlanoContaId = planoConta.Id!.Value, PlanoConta = planoConta };
    }

    // ---------- Consulta ----------

    [Fact]
    public void ObterResumoMensal_ConsultaOsRepositoriosComOPeriodoDoMes()
    {
        _service.ObterResumoMensal(2026, 12);

        var inicio = new DateTime(2026, 12, 1);
        var fim = new DateTime(2027, 1, 1);
        _transacaoRepository.Verify(r => r.ListarPorPeriodo(inicio, fim), Times.Once);
        _transacaoRecorrenteRepository.Verify(r => r.ListarVigentesNoPeriodo(inicio, fim), Times.Once);
    }

    // ---------- Totais ----------

    [Fact]
    public void ObterResumoMensal_SomaOsTotaisPorTipoECalculaOSaldo()
    {
        _transacoes.Add(NovaTransacao(1, new DateTime(2026, 10, 5), 5000m, Salario));
        _transacoes.Add(NovaTransacao(2, new DateTime(2026, 10, 10), 300m, Mercado));
        _transacoes.Add(NovaTransacao(3, new DateTime(2026, 10, 20), 1200m, Aluguel));
        _transacoes.Add(NovaTransacao(4, new DateTime(2026, 10, 25), 500m, Cofrinho));

        var resumo = _service.ObterResumoMensal(2026, 10);

        Assert.Equal(5000m, resumo.TotalReceitas);
        Assert.Equal(1500m, resumo.TotalDespesas);
        Assert.Equal(500m, resumo.TotalInvestimentos);
        Assert.Equal(3000m, resumo.Saldo);
        Assert.Equal(2026, resumo.Ano);
        Assert.Equal(10, resumo.Mes);
    }

    [Fact]
    public void ObterResumoMensal_TipoDesconhecido_ApareceNaListaMasNaoEntraNosTotais()
    {
        var semTipo = new PlanoConta { Id = 9, Descricao = "Outros", Tipo = "X" };
        _transacoes.Add(NovaTransacao(1, new DateTime(2026, 10, 5), 70m, semTipo));
        _transacoes.Add(NovaTransacao(2, new DateTime(2026, 10, 6), 30m, new PlanoConta { Id = 10, Descricao = "Farmacia", Tipo = " d " }));

        var resumo = _service.ObterResumoMensal(2026, 10);

        Assert.Equal(2, resumo.Lancamentos.Count);
        Assert.Equal(TipoLancamento.Indefinido, resumo.Lancamentos[0].Tipo);
        Assert.Equal(TipoLancamento.Despesa, resumo.Lancamentos[1].Tipo);
        Assert.Equal(30m, resumo.TotalDespesas);
        Assert.Equal(0m, resumo.TotalReceitas);
    }

    [Fact]
    public void ObterResumoMensal_MesSemLancamentos_RetornaTudoZerado()
    {
        var resumo = _service.ObterResumoMensal(2026, 11);

        Assert.Empty(resumo.Lancamentos);
        Assert.Empty(resumo.DespesasPorCategoria);
        Assert.Equal(0m, resumo.Saldo);
        Assert.Equal(30, resumo.EvolucaoDiaria.Count);
        Assert.All(resumo.EvolucaoDiaria, dia => Assert.Equal(0m, dia.Receitas + dia.Despesas + dia.Investimentos));
    }

    // ---------- Lancamentos ----------

    [Fact]
    public void ObterResumoMensal_MapeiaTransacaoUnica()
    {
        _transacoes.Add(NovaTransacao(7, new DateTime(2026, 10, 3, 14, 30, 0), 45.5m, Mercado, "Feira"));

        var lancamento = Assert.Single(_service.ObterResumoMensal(2026, 10).Lancamentos);

        Assert.Equal(7, lancamento.Id);
        Assert.False(lancamento.Recorrente);
        Assert.Equal(new DateTime(2026, 10, 3), lancamento.Data);
        Assert.Equal("Feira", lancamento.Descricao);
        Assert.Equal(2, lancamento.PlanoContaId);
        Assert.Equal("Mercado", lancamento.Categoria);
        Assert.Equal(TipoLancamento.Despesa, lancamento.Tipo);
        Assert.Equal(45.5m, lancamento.Valor);
    }

    [Fact]
    public void ObterResumoMensal_IncluiAOcorrenciaDaRecorrenteNoMes()
    {
        _recorrentes.Add(NovaRecorrente(5, new DateTime(2026, 2, 11), new DateTime(2026, 12, 31), 5700m, Salario));

        var resumo = _service.ObterResumoMensal(2026, 10);

        var lancamento = Assert.Single(resumo.Lancamentos);
        Assert.Equal(5, lancamento.Id);
        Assert.True(lancamento.Recorrente);
        Assert.Equal(new DateTime(2026, 10, 11), lancamento.Data);
        Assert.Equal("Recorrente (mensal)", lancamento.Descricao);
        Assert.Equal("Salario", lancamento.Categoria);
        Assert.Equal(TipoLancamento.Receita, lancamento.Tipo);
        Assert.Equal(5700m, resumo.TotalReceitas);
    }

    [Fact]
    public void ObterResumoMensal_RecorrenteQueNaoCaiNoMes_NaoEntra()
    {
        // Bimestral a partir de janeiro: ocorre em marco, nao em fevereiro
        _recorrentes.Add(NovaRecorrente(5, new DateTime(2026, 1, 15), new DateTime(2026, 12, 31), 99m, Mercado, Frequencia.Bimestral));

        var resumo = _service.ObterResumoMensal(2026, 2);

        Assert.Empty(resumo.Lancamentos);
        Assert.Equal(0m, resumo.TotalDespesas);
    }

    [Fact]
    public void ObterResumoMensal_OrdenaPorDataComAsUnicasAntesDasRecorrentesNoMesmoDia()
    {
        _transacoes.Add(NovaTransacao(2, new DateTime(2026, 10, 20), 10m, Mercado, "Depois"));
        _transacoes.Add(NovaTransacao(1, new DateTime(2026, 10, 11), 10m, Mercado, "Mesmo dia"));
        _recorrentes.Add(NovaRecorrente(9, new DateTime(2026, 1, 11), new DateTime(2026, 12, 31), 5700m, Salario));
        _transacoes.Add(NovaTransacao(3, new DateTime(2026, 10, 1), 10m, Mercado, "Primeira"));

        var resumo = _service.ObterResumoMensal(2026, 10);

        Assert.Equal(new[] { "Primeira", "Mesmo dia", "Recorrente (mensal)", "Depois" }, resumo.Lancamentos.Select(x => x.Descricao));
    }

    // ---------- Ocorrencias das recorrentes ----------

    [Theory]
    [InlineData(Frequencia.Mensal, 2026, 1, 15)]      // o proprio mes de inicio
    [InlineData(Frequencia.Mensal, 2026, 6, 15)]
    [InlineData(Frequencia.Bimestral, 2026, 3, 15)]
    [InlineData(Frequencia.Bimestral, 2026, 2, null)]
    [InlineData(Frequencia.Trimestral, 2026, 4, 15)]
    [InlineData(Frequencia.Trimestral, 2026, 5, null)]
    [InlineData(Frequencia.Semestral, 2026, 7, 15)]
    [InlineData(Frequencia.Anual, 2027, 1, 15)]
    [InlineData(Frequencia.Anual, 2026, 12, null)]
    [InlineData(Frequencia.Mensal, 2025, 12, null)]   // antes do inicio
    public void CalcularOcorrencia_RespeitaAFrequencia(Frequencia frequencia, int ano, int mes, int? diaEsperado)
    {
        var recorrente = NovaRecorrente(1, new DateTime(2026, 1, 15), new DateTime(2030, 12, 31), 10m, Mercado, frequencia);

        var ocorrencia = DashboardService.CalcularOcorrencia(recorrente, ano, mes);

        Assert.Equal(diaEsperado == null ? null : new DateTime(ano, mes, diaEsperado.Value), ocorrencia);
    }

    [Theory]
    [InlineData(9, 25)]    // ultimo mes inteiro antes do fim
    [InlineData(10, null)] // ocorreria em 25/10, depois do fim (20/10)
    [InlineData(11, null)]
    public void CalcularOcorrencia_NaoPassaDaDataFim(int mes, int? diaEsperado)
    {
        var recorrente = NovaRecorrente(1, new DateTime(2026, 1, 25), new DateTime(2026, 10, 20), 10m, Mercado);

        var ocorrencia = DashboardService.CalcularOcorrencia(recorrente, 2026, mes);

        Assert.Equal(diaEsperado == null ? null : new DateTime(2026, mes, diaEsperado.Value), ocorrencia);
    }

    [Fact]
    public void CalcularOcorrencia_OcorrenciaNoDiaDaDataFim_Entra()
    {
        var recorrente = NovaRecorrente(1, new DateTime(2026, 1, 20), new DateTime(2026, 10, 20), 10m, Mercado);

        Assert.Equal(new DateTime(2026, 10, 20), DashboardService.CalcularOcorrencia(recorrente, 2026, 10));
    }

    [Theory]
    [InlineData(2026, 2, 28)]
    [InlineData(2028, 2, 29)]
    [InlineData(2026, 3, 31)] // nao "encolhe" depois de fevereiro
    [InlineData(2026, 4, 30)]
    public void CalcularOcorrencia_DiaQueNaoExisteNoMes_UsaOUltimoDia(int ano, int mes, int diaEsperado)
    {
        var recorrente = NovaRecorrente(1, new DateTime(2026, 1, 31), new DateTime(2030, 12, 31), 10m, Mercado);

        Assert.Equal(new DateTime(ano, mes, diaEsperado), DashboardService.CalcularOcorrencia(recorrente, ano, mes));
    }

    // ---------- Despesas por categoria ----------

    [Fact]
    public void ObterResumoMensal_AgrupaDespesasPorCategoriaDaMaiorParaAMenor()
    {
        _transacoes.Add(NovaTransacao(1, new DateTime(2026, 10, 2), 300m, Mercado));
        _transacoes.Add(NovaTransacao(2, new DateTime(2026, 10, 9), 100m, Mercado));
        _transacoes.Add(NovaTransacao(3, new DateTime(2026, 10, 10), 1200m, Aluguel));
        _transacoes.Add(NovaTransacao(4, new DateTime(2026, 10, 10), 5000m, Salario));
        _recorrentes.Add(NovaRecorrente(5, new DateTime(2026, 1, 15), new DateTime(2026, 12, 31), 100m, Mercado));

        var categorias = _service.ObterResumoMensal(2026, 10).DespesasPorCategoria;

        Assert.Collection(categorias,
            c => { Assert.Equal("Aluguel", c.Categoria); Assert.Equal(3, c.PlanoContaId); Assert.Equal(1200m, c.Valor); Assert.Equal(70.6m, c.Percentual); },
            c => { Assert.Equal("Mercado", c.Categoria); Assert.Equal(2, c.PlanoContaId); Assert.Equal(500m, c.Valor); Assert.Equal(29.4m, c.Percentual); });
    }

    // ---------- Evolucao diaria ----------

    [Theory]
    [InlineData(2026, 2, 28)]
    [InlineData(2028, 2, 29)]
    [InlineData(2026, 10, 31)]
    [InlineData(2026, 11, 30)]
    public void ObterResumoMensal_EvolucaoTemUmItemPorDia(int ano, int mes, int dias)
    {
        var evolucao = _service.ObterResumoMensal(ano, mes).EvolucaoDiaria;

        Assert.Equal(dias, evolucao.Count);
        Assert.Equal(new DateTime(ano, mes, 1), evolucao[0].Data);
        Assert.Equal(new DateTime(ano, mes, dias), evolucao[^1].Data);
    }

    [Fact]
    public void ObterResumoMensal_EvolucaoAcumulaOsValoresDiaADia()
    {
        _transacoes.Add(NovaTransacao(1, new DateTime(2026, 10, 10), 300m, Mercado));
        _transacoes.Add(NovaTransacao(2, new DateTime(2026, 10, 20), 1200m, Aluguel));
        _transacoes.Add(NovaTransacao(3, new DateTime(2026, 10, 25), 500m, Cofrinho));
        _recorrentes.Add(NovaRecorrente(4, new DateTime(2026, 1, 5), new DateTime(2026, 12, 31), 5000m, Salario));

        var evolucao = _service.ObterResumoMensal(2026, 10).EvolucaoDiaria;

        Assert.Equal(0m, evolucao[3].Receitas);      // 04/10
        Assert.Equal(5000m, evolucao[4].Receitas);   // 05/10
        Assert.Equal(0m, evolucao[8].Despesas);      // 09/10
        Assert.Equal(300m, evolucao[9].Despesas);    // 10/10
        Assert.Equal(300m, evolucao[18].Despesas);   // 19/10
        Assert.Equal(1500m, evolucao[19].Despesas);  // 20/10
        Assert.Equal(500m, evolucao[24].Investimentos);
        Assert.Equal(5000m, evolucao[^1].Receitas);
        Assert.Equal(1500m, evolucao[^1].Despesas);
        Assert.Equal(500m, evolucao[^1].Investimentos);
    }
}
