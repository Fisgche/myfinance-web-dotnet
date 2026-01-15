using myfinance_web_dotnet_domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_infra.Interfaces.Base
{
    public interface IRepository<T> where T : class
    {
        void Cadastrar(T entidade);

        void Excluir(int id);

        List<T> ListarRegistros();

        T RetornarRegistro(int id);
    }
}
