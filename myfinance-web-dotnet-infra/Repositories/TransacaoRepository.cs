using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;

namespace myfinance_web_dotnet_infra.Repositories
{
    public class TransacaoRepository : Repository<Transacao>, ITransacaoRepository
    {
        public TransacaoRepository(MyFinanceDbContext dbContext) : base(dbContext)
        {
        }

        public override List<Transacao> ListarRegistros()
        {
            return DbSetContext.Include(x => x.PlanoConta).ToList();
        }

        public List<Transacao> ListarPorPeriodo(DateTime inicio, DateTime fim)
        {
            return DbSetContext
                .Include(x => x.PlanoConta)
                .Where(x => x.Date >= inicio && x.Date < fim)
                .OrderBy(x => x.Date)
                .ToList();
        }
    }
}
