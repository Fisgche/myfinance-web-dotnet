using Microsoft.EntityFrameworkCore;
using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_infra.Repositories
{
    public class TransacaoRecorrenteRepository : Repository<TransacaoRecorrente>, ITransacaoRecorrenteRepository
    {
        public TransacaoRecorrenteRepository(MyFinanceDbContext dbContext) : base(dbContext)
        {
        }

        public override List<TransacaoRecorrente> ListarRegistros()
        {
            return DbSetContext.Include(x => x.PlanoConta).ToList();
        }
    }
}
