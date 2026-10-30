using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using myfinance_web_dotnet_domain.Entities;

namespace myfinance_web_dotnet_infra;

public class MyFinanceDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<PlanoConta> PlanoConta {get; set;} = null!;

    public DbSet<Transacao> Transacao {get; set;} = null!;

    public DbSet<TransacaoRecorrente> TransacaoRecorrente {get; set;} = null!;

    public MyFinanceDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("Database"));
        
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transacao>().Property(x => x.Valor).HasPrecision(18, 2);
        modelBuilder.Entity<TransacaoRecorrente>().Property(x => x.Valor).HasPrecision(18, 2);

        // Impede excluir um plano de conta que possui transacoes vinculadas
        modelBuilder.Entity<Transacao>().HasOne(x => x.PlanoConta).WithMany().HasForeignKey(x => x.PlanoContaId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<TransacaoRecorrente>().HasOne(x => x.PlanoConta).WithMany().HasForeignKey(x => x.PlanoContaId).OnDelete(DeleteBehavior.Restrict);

        base.OnModelCreating(modelBuilder);
    }
}
