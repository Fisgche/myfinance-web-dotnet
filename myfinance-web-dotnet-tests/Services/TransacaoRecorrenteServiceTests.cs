using Moq;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service;

namespace myfinance_web_dotnet_tests.Services;

public class TransacaoRecorrenteServiceTests
{
    private readonly Mock<ITransacaoRecorrenteRepository> _repository = new();
    private readonly TransacaoRecorrenteService _service;

    public TransacaoRecorrenteServiceTests()
    {
        _service = new TransacaoRecorrenteService(_repository.Object);
    }

    [Fact]
    public void Cadastrar_DelegaParaRepositorio()
    {
        var transacaoRecorrente = new TransacaoRecorrente { Valor = 99.90m, PlanoContaId = 1 };

        _service.Cadastrar(transacaoRecorrente);

        _repository.Verify(r => r.Cadastrar(transacaoRecorrente), Times.Once);
    }

    [Fact]
    public void Excluir_DelegaParaRepositorio()
    {
        _service.Excluir(4);

        _repository.Verify(r => r.Excluir(4), Times.Once);
    }

    [Fact]
    public void ListarRegistros_RetornaListaDoRepositorio()
    {
        var lista = new List<TransacaoRecorrente> { new() { Id = 1, Valor = 50m } };
        _repository.Setup(r => r.ListarRegistros()).Returns(lista);

        Assert.Same(lista, _service.ListarRegistros());
    }

    [Fact]
    public void RetornarRegistro_RetornaEntidadeDoRepositorio()
    {
        var transacaoRecorrente = new TransacaoRecorrente { Id = 2, Valor = 50m };
        _repository.Setup(r => r.RetornarRegistro(2)).Returns(transacaoRecorrente);

        Assert.Same(transacaoRecorrente, _service.RetornarRegistro(2));
    }

    [Fact]
    public void ListarVigentesNoPeriodo_RetornaListaDoRepositorio()
    {
        var inicio = new DateTime(2026, 10, 1);
        var fim = new DateTime(2026, 11, 1);
        var lista = new List<TransacaoRecorrente> { new() { Id = 1, Valor = 50m } };
        _repository.Setup(r => r.ListarVigentesNoPeriodo(inicio, fim)).Returns(lista);

        Assert.Same(lista, _service.ListarVigentesNoPeriodo(inicio, fim));
    }
}
