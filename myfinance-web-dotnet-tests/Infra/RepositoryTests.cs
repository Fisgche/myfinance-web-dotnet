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

    [Fact]
    public void Transacao_ListarPorPeriodo_FiltraOIntervaloEmOrdemDeDataComPlanoConta()
    {
        var banco = TestDbContext.NovoBanco();
        using (var db = new TestDbContext(banco))
        {
            var planoConta = TestDbContext.CriarPlanoConta(db, "Mercado", "D");
            foreach (var data in new[] { new DateTime(2026, 10, 31), new DateTime(2026, 9, 30), new DateTime(2026, 11, 1), new DateTime(2026, 10, 1) })
            {
                db.Transacao.Add(new Transacao { Historico = data.ToString("dd/MM"), Date = data, Valor = 10m, PlanoContaId = planoConta.Id!.Value });
            }
            db.SaveChanges();
        }

        using var leitura = new TestDbContext(banco);
        var registros = new TransacaoRepository(leitura).ListarPorPeriodo(new DateTime(2026, 10, 1), new DateTime(2026, 11, 1));

        Assert.Equal(new[] { "01/10", "31/10" }, registros.Select(x => x.Historico));
        Assert.All(registros, x => Assert.Equal("Mercado", x.PlanoConta?.Descricao));
    }

    [Fact]
    public void TransacaoRecorrente_ListarVigentesNoPeriodo_TrazAsQueCruzamOIntervalo()
    {
        var banco = TestDbContext.NovoBanco();
        using (var db = new TestDbContext(banco))
        {
            var planoConta = TestDbContext.CriarPlanoConta(db, "Salario", "R");
            void Adicionar(decimal valor, DateTime inicio, DateTime fim) =>
                db.TransacaoRecorrente.Add(new TransacaoRecorrente { Valor = valor, DataInicio = inicio, DataFim = fim, PlanoContaId = planoConta.Id!.Value });

            Adicionar(1m, new DateTime(2026, 1, 1), new DateTime(2026, 9, 30));    // terminou antes
            Adicionar(2m, new DateTime(2025, 1, 1), new DateTime(2026, 10, 1));    // termina no primeiro dia
            Adicionar(3m, new DateTime(2026, 10, 31), new DateTime(2027, 12, 31)); // comeca no ultimo dia
            Adicionar(4m, new DateTime(2026, 11, 1), new DateTime(2027, 12, 31));  // comeca depois
            Adicionar(5m, new DateTime(2025, 1, 1), new DateTime(2027, 12, 31));   // cobre o mes inteiro
            db.SaveChanges();
        }

        using var leitura = new TestDbContext(banco);
        var registros = new TransacaoRecorrenteRepository(leitura).ListarVigentesNoPeriodo(new DateTime(2026, 10, 1), new DateTime(2026, 11, 1));

        Assert.Equal(new[] { 2m, 3m, 5m }, registros.Select(x => x.Valor).OrderBy(x => x));
        Assert.All(registros, x => Assert.Equal("Salario", x.PlanoConta?.Descricao));
    }
}
