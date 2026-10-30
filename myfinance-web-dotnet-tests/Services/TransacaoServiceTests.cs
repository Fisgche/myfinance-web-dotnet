using Moq;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service;

namespace myfinance_web_dotnet_tests.Services;

public class TransacaoServiceTests
{
    private readonly Mock<ITransacaoRepository> _repository = new();
    private readonly TransacaoService _service;

    public TransacaoServiceTests()
    {
        _service = new TransacaoService(_repository.Object);
    }

    [Fact]
    public void Cadastrar_DelegaParaRepositorio()
    {
        var transacao = new Transacao { Historico = "Feira", Valor = 45.5m, PlanoContaId = 1 };

        _service.Cadastrar(transacao);

        _repository.Verify(r => r.Cadastrar(transacao), Times.Once);
    }

    [Fact]
    public void Excluir_DelegaParaRepositorio()
    {
        _service.Excluir(10);

        _repository.Verify(r => r.Excluir(10), Times.Once);
    }

    [Fact]
    public void ListarRegistros_RetornaListaDoRepositorio()
    {
        var lista = new List<Transacao> { new() { Id = 1, Historico = "Feira" } };
        _repository.Setup(r => r.ListarRegistros()).Returns(lista);

        Assert.Same(lista, _service.ListarRegistros());
    }

    [Fact]
    public void RetornarRegistro_RetornaEntidadeDoRepositorio()
    {
        var transacao = new Transacao { Id = 10, Historico = "Feira" };
        _repository.Setup(r => r.RetornarRegistro(10)).Returns(transacao);

        Assert.Same(transacao, _service.RetornarRegistro(10));
    }

    [Fact]
    public void ListarPorPeriodo_RetornaListaDoRepositorio()
    {
        var inicio = new DateTime(2026, 10, 1);
        var fim = new DateTime(2026, 11, 1);
        var lista = new List<Transacao> { new() { Id = 1, Historico = "Feira" } };
        _repository.Setup(r => r.ListarPorPeriodo(inicio, fim)).Returns(lista);

        Assert.Same(lista, _service.ListarPorPeriodo(inicio, fim));
    }
}
