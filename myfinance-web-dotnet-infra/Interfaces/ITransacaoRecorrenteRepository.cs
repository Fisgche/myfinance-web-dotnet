using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_infra.Interfaces
{
    public interface ITransacaoRecorrenteRepository : IRepository<TransacaoRecorrente>
    {
        /// <summary>
        /// Transacoes recorrentes cuja vigencia (DataInicio a DataFim) cruza o intervalo [inicio, fim),
        /// com o PlanoConta carregado.
        /// </summary>
        List<TransacaoRecorrente> ListarVigentesNoPeriodo(DateTime inicio, DateTime fim);
    }
}
