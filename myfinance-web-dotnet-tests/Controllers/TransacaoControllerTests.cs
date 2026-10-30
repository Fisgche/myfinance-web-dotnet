using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using myfinance_web_dotnet.Controllers;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_tests.Controllers;

public class TransacaoControllerTests
{
    private readonly Mock<ITransacaoService> _transacaoService = new();
    private readonly Mock<ITransacaoRecorrenteService> _transacaoRecorrenteService = new();
    private readonly Mock<IPlanoContaService> _planoContaService = new();
    private readonly TransacaoController _controller;

    public TransacaoControllerTests()
    {
        _planoContaService.Setup(s => s.ListarRegistros()).Returns(new List<PlanoConta>
        {
            new() { Id = 1, Descricao = "Mercado", Tipo = "D" },
            new() { Id = 2, Descricao = "Salário", Tipo = "R" }
        });
        _transacaoService.Setup(s => s.ListarRegistros()).Returns(new List<Transacao>());
        _transacaoRecorrenteService.Setup(s => s.ListarRegistros()).Returns(new List<TransacaoRecorrente>());
        _controller = new TransacaoController(
            NullLogger<TransacaoController>.Instance,
            _transacaoService.Object,
            _transacaoRecorrenteService.Object,
            _planoContaService.Object);
    }

    private static string? AbaDoRedirect(IActionResult resultado)
    {
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirect.ActionName);
        return redirect.RouteValues?["aba"] as string;
    }

    // ---------- Index ----------

    [Fact]
    public void Index_MapeiaTransacoesUnicasERecorrentesNaViewBag()
    {
        var planoConta = new PlanoConta { Id = 1, Descricao = "Mercado", Tipo = "D" };
        _transacaoService.Setup(s => s.ListarRegistros()).Returns(new List<Transacao>
        {
            new() { Id = 10, Historico = "Feira", Date = new DateTime(2026, 10, 3), Valor = 45.5m, PlanoContaId = 1, PlanoConta = planoConta }
        });
        _transacaoRecorrenteService.Setup(s => s.ListarRegistros()).Returns(new List<TransacaoRecorrente>
        {
            new() { Id = 4, Valor = 120m, DataInicio = new DateTime(2026, 1, 1), DataFim = new DateTime(2026, 12, 31), PlanoContaId = 1, PlanoConta = planoConta }
        });

        Assert.IsType<ViewResult>(_controller.Index(null));

        List<TransacaoModel> unicas = Assert.IsType<List<TransacaoModel>>(_controller.ViewBag.ListaTransacao);
        var unica = Assert.Single(unicas);
        Assert.False(unica.Recorrente);
        Assert.Equal(10, unica.Id);
        Assert.Equal("Feira", unica.Historico);
        Assert.Equal(new DateTime(2026, 10, 3), unica.Date);
        Assert.Equal(45.5m, unica.Valor);
        Assert.Same(planoConta, unica.PlanoConta);

        List<TransacaoModel> recorrentes = Assert.IsType<List<TransacaoModel>>(_controller.ViewBag.ListaTransacaoRecorrente);
        var recorrente = Assert.Single(recorrentes);
        Assert.True(recorrente.Recorrente);
        Assert.Equal(4, recorrente.Id);
        Assert.Equal(new DateTime(2026, 1, 1), recorrente.DataInicio);
        Assert.Equal(new DateTime(2026, 12, 31), recorrente.DataFim);
        Assert.Equal(120m, recorrente.Valor);
        Assert.Same(planoConta, recorrente.PlanoConta);
    }

    [Theory]
    [InlineData(null, TransacaoController.AbaUnicas)]
    [InlineData("unicas", TransacaoController.AbaUnicas)]
    [InlineData("recorrentes", TransacaoController.AbaRecorrentes)]
    [InlineData("qualquer", TransacaoController.AbaUnicas)]
    public void Index_DefineAbaAtiva(string? aba, string esperada)
    {
        _controller.Index(aba);

        Assert.Equal(esperada, (string)_controller.ViewBag.Aba);
    }

    // ---------- Cadastrar (GET) ----------

    [Fact]
    public void CadastrarGet_SemId_RetornaModelUnicaComListaDePlanos()
    {
        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar((int?)null));

        var model = Assert.IsType<TransacaoModel>(resultado.Model);
        Assert.Null(model.Id);
        Assert.False(model.Recorrente);
        Assert.Equal(DateTime.Today, model.Date.Date);
        Assert.Equal(DateTime.Today, model.DataInicio.Date);
        Assert.Equal(DateTime.Today, model.DataFim.Date);
        Assert.Equal(new[] { "Mercado", "Salário" }, model.ListPlanoConta!.Select(i => i.Text));
        Assert.Equal(new[] { "1", "2" }, model.ListPlanoConta!.Select(i => i.Value));
    }

    [Fact]
    public void CadastrarGet_SemIdComRecorrente_PreSelecionaRecorrente()
    {
        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar(null, recorrente: true));

        Assert.True(Assert.IsType<TransacaoModel>(resultado.Model).Recorrente);
    }

    [Fact]
    public void CadastrarGet_ComId_PreencheModelComTransacaoUnica()
    {
        _transacaoService.Setup(s => s.RetornarRegistro(10)).Returns(new Transacao
        {
            Id = 10, Historico = "Feira", Date = new DateTime(2026, 10, 3), Valor = 45.5m, PlanoContaId = 1
        });

        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar(10));

        var model = Assert.IsType<TransacaoModel>(resultado.Model);
        Assert.False(model.Recorrente);
        Assert.Equal(10, model.Id);
        Assert.Equal("Feira", model.Historico);
        Assert.Equal(new DateTime(2026, 10, 3), model.Date);
        Assert.Equal(45.5m, model.Valor);
        Assert.Equal(1, model.PlanoContaId);
        Assert.NotNull(model.ListPlanoConta);
    }

    [Fact]
    public void CadastrarGet_IdInexistente_RetornaNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.Cadastrar(999));
    }

    [Fact]
    public void CadastrarRecorrente_ComId_PreencheModelRecorrenteNaViewCadastrar()
    {
        _transacaoRecorrenteService.Setup(s => s.RetornarRegistro(4)).Returns(new TransacaoRecorrente
        {
            Id = 4, Valor = 120m, DataInicio = new DateTime(2026, 1, 1), DataFim = new DateTime(2026, 12, 31), PlanoContaId = 1
        });

        var resultado = Assert.IsType<ViewResult>(_controller.CadastrarRecorrente(4));

        Assert.Equal("Cadastrar", resultado.ViewName);
        var model = Assert.IsType<TransacaoModel>(resultado.Model);
        Assert.True(model.Recorrente);
        Assert.Equal(4, model.Id);
        Assert.Equal(120m, model.Valor);
        Assert.Equal(new DateTime(2026, 1, 1), model.DataInicio);
        Assert.Equal(new DateTime(2026, 12, 31), model.DataFim);
        Assert.Equal(1, model.PlanoContaId);
        Assert.NotNull(model.ListPlanoConta);
    }

    [Fact]
    public void CadastrarRecorrente_IdInexistente_RetornaNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.CadastrarRecorrente(999));
    }

    // ---------- Cadastrar (POST) ----------

    [Fact]
    public void CadastrarPost_Unica_SalvaTransacaoERedirecionaParaAbaUnicas()
    {
        var model = new TransacaoModel
        {
            Id = 10, Historico = "Feira", Date = new DateTime(2026, 10, 3), Valor = 45.5m, PlanoContaId = 1
        };

        var resultado = _controller.Cadastrar(model);

        _transacaoService.Verify(s => s.Cadastrar(It.Is<Transacao>(t =>
            t.Id == 10 && t.Historico == "Feira" && t.Date == new DateTime(2026, 10, 3) &&
            t.Valor == 45.5m && t.PlanoContaId == 1)), Times.Once);
        _transacaoRecorrenteService.Verify(s => s.Cadastrar(It.IsAny<TransacaoRecorrente>()), Times.Never);
        Assert.Equal(TransacaoController.AbaUnicas, AbaDoRedirect(resultado));
    }

    [Fact]
    public void CadastrarPost_Recorrente_SalvaTransacaoRecorrenteERedirecionaParaAbaRecorrentes()
    {
        var model = new TransacaoModel
        {
            Id = 4, Recorrente = true, Valor = 120m,
            DataInicio = new DateTime(2026, 1, 1), DataFim = new DateTime(2026, 12, 31), PlanoContaId = 1
        };

        var resultado = _controller.Cadastrar(model);

        _transacaoRecorrenteService.Verify(s => s.Cadastrar(It.Is<TransacaoRecorrente>(t =>
            t.Id == 4 && t.Valor == 120m && t.DataInicio == new DateTime(2026, 1, 1) &&
            t.DataFim == new DateTime(2026, 12, 31) && t.PlanoContaId == 1)), Times.Once);
        _transacaoService.Verify(s => s.Cadastrar(It.IsAny<Transacao>()), Times.Never);
        Assert.Equal(TransacaoController.AbaRecorrentes, AbaDoRedirect(resultado));
    }

    [Fact]
    public void CadastrarPost_UnicaSemHistorico_ReexibeFormularioComErro()
    {
        var model = new TransacaoModel { Historico = " ", Valor = 10m, PlanoContaId = 1 };

        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar(model));

        Assert.True(_controller.ModelState.ContainsKey(nameof(TransacaoModel.Historico)));
        Assert.Same(model, resultado.Model);
        Assert.NotNull(model.ListPlanoConta);
        _transacaoService.Verify(s => s.Cadastrar(It.IsAny<Transacao>()), Times.Never);
    }

    [Fact]
    public void CadastrarPost_RecorrenteSemHistorico_EhValido()
    {
        var model = new TransacaoModel
        {
            Recorrente = true, Valor = 10m, PlanoContaId = 1,
            DataInicio = new DateTime(2026, 1, 1), DataFim = new DateTime(2026, 1, 1)
        };

        Assert.IsType<RedirectToActionResult>(_controller.Cadastrar(model));
        _transacaoRecorrenteService.Verify(s => s.Cadastrar(It.IsAny<TransacaoRecorrente>()), Times.Once);
    }

    [Fact]
    public void CadastrarPost_RecorrenteComDataFimAntesDoInicio_ReexibeFormularioComErro()
    {
        var model = new TransacaoModel
        {
            Recorrente = true, Valor = 10m, PlanoContaId = 1,
            DataInicio = new DateTime(2026, 5, 1), DataFim = new DateTime(2026, 4, 30)
        };

        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar(model));

        Assert.True(_controller.ModelState.ContainsKey(nameof(TransacaoModel.DataFim)));
        Assert.Equal("Cadastrar", resultado.ViewName);
        _transacaoRecorrenteService.Verify(s => s.Cadastrar(It.IsAny<TransacaoRecorrente>()), Times.Never);
    }

    // ---------- Excluir ----------

    [Fact]
    public void Excluir_ExcluiTransacaoUnicaERedirecionaParaAbaUnicas()
    {
        var resultado = _controller.Excluir(10);

        _transacaoService.Verify(s => s.Excluir(10), Times.Once);
        _transacaoRecorrenteService.Verify(s => s.Excluir(It.IsAny<int>()), Times.Never);
        Assert.Equal(TransacaoController.AbaUnicas, AbaDoRedirect(resultado));
    }

    [Fact]
    public void ExcluirRecorrente_ExcluiTransacaoRecorrenteERedirecionaParaAbaRecorrentes()
    {
        var resultado = _controller.ExcluirRecorrente(4);

        _transacaoRecorrenteService.Verify(s => s.Excluir(4), Times.Once);
        _transacaoService.Verify(s => s.Excluir(It.IsAny<int>()), Times.Never);
        Assert.Equal(TransacaoController.AbaRecorrentes, AbaDoRedirect(resultado));
    }
}
