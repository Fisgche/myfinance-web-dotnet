using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Repositories;
using myfinance_web_dotnet_tests.Helpers;

namespace myfinance_web_dotnet_tests.Infra;

public class RepositoryTests
{
    [Fact]
    public void Cadastrar_SemId_InsereNovoRegistro()
    {
        var banco = TestDbContext.NovoBanco();
        using (var db = new TestDbContext(banco))
        {
            new PlanoContaRepository(db).Cadastrar(new PlanoConta { Descricao = "Aluguel", Tipo = "D" });
        }

        using var leitura = new TestDbContext(banco);
        var registro = Assert.Single(leitura.PlanoConta);
        Assert.NotNull(registro.Id);
        Assert.Equal("Aluguel", registro.Descricao);
        Assert.Equal("D", registro.Tipo);
    }

    [Fact]
    public void Cadastrar_ComId_AtualizaRegistroExistente()
    {
        var banco = TestDbContext.NovoBanco();
        int id;
        using (var db = new TestDbContext(banco))
        {
            id = TestDbContext.CriarPlanoConta(db, "Aluguel", "D").Id!.Value;
        }

        using (var db = new TestDbContext(banco))
        {
            new PlanoContaRepository(db).Cadastrar(new PlanoConta { Id = id, Descricao = "Aluguel apto", Tipo = "D" });
        }

        using var leitura = new TestDbContext(banco);
        var registro = Assert.Single(leitura.PlanoConta);
        Assert.Equal(id, registro.Id);
        Assert.Equal("Aluguel apto", registro.Descricao);
    }

    [Fact]
    public void Excluir_RemoveRegistro()
    {
        var banco = TestDbContext.NovoBanco();
        int id;
        using (var db = new TestDbContext(banco))
        {
            id = TestDbContext.CriarPlanoConta(db).Id!.Value;
        }

        using (var db = new TestDbContext(banco))
        {
            new PlanoContaRepository(db).Excluir(id);
        }

        using var leitura = new TestDbContext(banco);
        Assert.Empty(leitura.PlanoConta);
    }

    [Fact]
    public void RetornarRegistro_IdExistente_RetornaEntidade()
    {
        var banco = TestDbContext.NovoBanco();
        using var db = new TestDbContext(banco);
        TestDbContext.CriarPlanoConta(db, "Mercado", "D");
        var esperado = TestDbContext.CriarPlanoConta(db, "Salário", "R");

        var registro = new PlanoContaRepository(db).RetornarRegistro(esperado.Id!.Value);

        Assert.NotNull(registro);
        Assert.Equal("Salário", registro.Descricao);
    }

    [Fact]
    public void RetornarRegistro_IdInexistente_RetornaNull()
    {
        using var db = new TestDbContext(TestDbContext.NovoBanco());

        Assert.Null(new PlanoContaRepository(db).RetornarRegistro(999));
    }

    [Fact]
    public void Excluir_IdInexistente_NaoLancaExcecao()
    {
        using var db = new TestDbContext(TestDbContext.NovoBanco());
        TestDbContext.CriarPlanoConta(db);

        new PlanoContaRepository(db).Excluir(999);

        Assert.Single(db.PlanoConta);
    }

    [Fact]
    public void ListarRegistros_RetornaTodos()
    {
        using var db = new TestDbContext(TestDbContext.NovoBanco());
        TestDbContext.CriarPlanoConta(db, "Mercado", "D");
        TestDbContext.CriarPlanoConta(db, "Salário", "R");

        var registros = new PlanoContaRepository(db).ListarRegistros();

        Assert.Equal(2, registros.Count);
    }

    [Fact]
    public void TransacaoRecorrente_ListarRegistros_IncluiPlanoConta()
    {
        var banco = TestDbContext.NovoBanco();
        using (var db = new TestDbContext(banco))
        {
            var planoConta = TestDbContext.CriarPlanoConta(db, "Internet", "D");
            db.TransacaoRecorrente.Add(new TransacaoRecorrente
            {
                Valor = 120m,
                DataInicio = new DateTime(2026, 1, 1),
                DataFim = new DateTime(2026, 12, 31),
                PlanoContaId = planoConta.Id!.Value
            });
            db.SaveChanges();
        }

        // Contexto novo: sem o Include, PlanoConta viria nulo
        using var leitura = new TestDbContext(banco);
        var registros = new TransacaoRecorrenteRepository(leitura).ListarRegistros();

        var registro = Assert.Single(registros);
        Assert.NotNull(registro.PlanoConta);
        Assert.Equal("Internet", registro.PlanoConta.Descricao);
    }

    [Fact]
    public void Transacao_ListarRegistros_IncluiPlanoConta()
    {
        var banco = TestDbContext.NovoBanco();
        using (var db = new TestDbContext(banco))
        {
            var planoConta = TestDbContext.CriarPlanoConta(db, "Mercado", "D");
            db.Transacao.Add(new Transacao
            {
                Historico = "Feira",
                Date = new DateTime(2026, 10, 3),
                Valor = 45.5m,
                PlanoContaId = planoConta.Id!.Value
            });
            db.SaveChanges();
        }

        using var leitura = new TestDbContext(banco);
        var registros = new TransacaoRepository(leitura).ListarRegistros();

        var registro = Assert.Single(registros);
        Assert.NotNull(registro.PlanoConta);
        Assert.Equal("Mercado", registro.PlanoConta.Descricao);
    }
}
