using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet_domain.Entities;

namespace myfinance_web_dotnet_infra;

public class MyFinanceDbContext : DbContext
{
    public DbSet<PlanoConta> PlanoConta {get; set;}

    public DbSet<Transacao> Transacao {get; set;}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=DESKTOP-8M751P2\SQLEXPRESS;Database=myfinance;Trusted_Connection=True;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=False;");
        //optionsBuilder.UseSqlServer(@"Server=meuservidorsqlserver.database.windows.net;Database=myfinance;User Id=user;Password=*********;Connect Timeout=60;");
        base.OnConfiguring(optionsBuilder);
    }
}
