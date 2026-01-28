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
    internal class TransacaoRecorrenteRepository : Repository<TransacaoRecorrente>, ITransacaoRecorrenteRepository
    {
        public TransacaoRecorrenteRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}
