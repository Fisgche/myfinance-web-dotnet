using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using myfinance_web_dotnet.Controllers;
using myfinance_web_dotnet.Models;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_service.Interfaces;

namespace myfinance_web_dotnet_tests.Controllers;

public class PlanoContaControllerTests
{
    private readonly Mock<IPlanoContaService> _service = new();
    private readonly PlanoContaController _controller;

    public PlanoContaControllerTests()
    {
        _controller = new PlanoContaController(NullLogger<PlanoContaController>.Instance, _service.Object);
    }

    [Fact]
    public void Index_MapeiaEntidadesParaModelsNaViewBag()
    {
        _service.Setup(s => s.ListarRegistros()).Returns(new List<PlanoConta>
        {
            new() { Id = 1, Descricao = "Salário", Tipo = "R" },
            new() { Id = 2, Descricao = "Aluguel", Tipo = "D" }
        });

        var resultado = _controller.Index();

        Assert.IsType<ViewResult>(resultado);
        List<PlanoContaModel> lista = Assert.IsType<List<PlanoContaModel>>(_controller.ViewBag.ListaPlanoConta);
        Assert.Collection(lista,
            m => { Assert.Equal(1, m.Id); Assert.Equal("Salário", m.Descricao); Assert.Equal("R", m.Tipo); },
            m => { Assert.Equal(2, m.Id); Assert.Equal("Aluguel", m.Descricao); Assert.Equal("D", m.Tipo); });
    }

    [Fact]
    public void CadastrarGet_SemId_RetornaViewSemModel()
    {
        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar((int?)null));

        Assert.Null(resultado.Model);
        _service.Verify(s => s.RetornarRegistro(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void CadastrarGet_ComId_RetornaModelPreenchido()
    {
        _service.Setup(s => s.RetornarRegistro(5)).Returns(new PlanoConta { Id = 5, Descricao = "Luz", Tipo = "D" });

        var resultado = Assert.IsType<ViewResult>(_controller.Cadastrar(5));

        var model = Assert.IsType<PlanoContaModel>(resultado.Model);
        Assert.Equal(5, model.Id);
        Assert.Equal("Luz", model.Descricao);
        Assert.Equal("D", model.Tipo);
    }

    [Fact]
    public void CadastrarGet_IdInexistente_RetornaNotFound()
    {
        Assert.IsType<NotFoundResult>(_controller.Cadastrar(999));
    }

    [Fact]
    public void CadastrarPost_MapeiaModelERedirecionaParaIndex()
    {
        var model = new PlanoContaModel { Id = 9, Descricao = "Internet", Tipo = "D" };

        var resultado = _controller.Cadastrar(model);

        _service.Verify(s => s.Cadastrar(It.Is<PlanoConta>(p =>
            p.Id == 9 && p.Descricao == "Internet" && p.Tipo == "D")), Times.Once);
        var redirect = Assert.IsType<RedirectToActionResult>(resultado);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public void Excluir_ChamaServicoERedirecionaParaIndex()
    {
        var resultado = _controller.Excluir(3);

        _service.Verify(s => s.Excluir(3), Times.Once);
        Assert.Equal("Index", Assert.IsType<RedirectToActionResult>(resultado).ActionName);
    }
}
