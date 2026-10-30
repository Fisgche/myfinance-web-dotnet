using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra;

namespace myfinance_web_dotnet_tests.Helpers;

/// <summary>
/// MyFinanceDbContext apontando para um banco em memória, em vez do SQL Server.
/// Cada nome de banco é isolado; contextos criados com o mesmo nome compartilham os dados.
/// </summary>
public class TestDbContext : MyFinanceDbContext
{
    private readonly string _databaseName;

    public TestDbContext(string databaseName)
        : base(new ConfigurationBuilder().Build())
    {
        _databaseName = databaseName;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase(_databaseName);
    }

    public static string NovoBanco() => Guid.NewGuid().ToString();

    public static PlanoConta CriarPlanoConta(TestDbContext db, string descricao = "Salário", string tipo = "R")
    {
        var planoConta = new PlanoConta { Descricao = descricao, Tipo = tipo };
        db.PlanoConta.Add(planoConta);
        db.SaveChanges();
        return planoConta;
    }
}
