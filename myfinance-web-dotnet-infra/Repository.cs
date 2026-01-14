using Microsoft.Extensions.Configuration;
using myfinance_web_dotnet_infra.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_infra
{
    public abstract class Repository<T> : MyFinanceDbContext, IRepository<T> where T : class
    {
        protected Repository(IConfiguration configuration) : base(configuration)
        {
        }

        public void Cadastrar(T entidade)
        {
            throw new NotImplementedException();
        }

        public void Excluir(int id)
        {
            throw new NotImplementedException();
        }

        public List<T> ListarRegistros()
        {
            throw new NotImplementedException();
        }

        public T RetornarRegistro(int id)
        {
            throw new NotImplementedException();
        }
    }
}
