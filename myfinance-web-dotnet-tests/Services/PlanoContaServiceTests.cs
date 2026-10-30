using Moq;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using myfinance_web_dotnet_service;

namespace myfinance_web_dotnet_tests.Services;

public class PlanoContaServiceTests
{
    private readonly Mock<IPlanoContaRepository> _repository = new();
    private readonly PlanoContaService _service;

    public PlanoContaServiceTests()
    {
        _service = new PlanoContaService(_repository.Object);
    }

    [Fact]
    public void Cadastrar_DelegaParaRepositorio()
    {
        var planoConta = new PlanoConta { Descricao = "Salário", Tipo = "R" };

        _service.Cadastrar(planoConta);

        _repository.Verify(r => r.Cadastrar(planoConta), Times.Once);
    }

    [Fact]
    public void Excluir_DelegaParaRepositorio()
    {
        _service.Excluir(7);

        _repository.Verify(r => r.Excluir(7), Times.Once);
    }

    [Fact]
    public void ListarRegistros_RetornaListaDoRepositorio()
    {
        var lista = new List<PlanoConta> { new() { Id = 1, Descricao = "Salário", Tipo = "R" } };
        _repository.Setup(r => r.ListarRegistros()).Returns(lista);

        Assert.Same(lista, _service.ListarRegistros());
    }

    [Fact]
    public void RetornarRegistro_RetornaEntidadeDoRepositorio()
    {
        var planoConta = new PlanoConta { Id = 3, Descricao = "Aluguel", Tipo = "D" };
        _repository.Setup(r => r.RetornarRegistro(3)).Returns(planoConta);

        Assert.Same(planoConta, _service.RetornarRegistro(3));
    }
}
