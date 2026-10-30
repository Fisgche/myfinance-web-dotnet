using myfinance_web_dotnet_domain.Entities;
using myfinance_web_dotnet_infra.Interfaces.Base;

namespace myfinance_web_dotnet_infra.Interfaces
{
    public interface ITransacaoRepository : IRepository<Transacao>
    {
        /// <summary>
        /// Transacoes com data no intervalo [inicio, fim), com o PlanoConta carregado.
        /// </summary>
        List<Transacao> ListarPorPeriodo(DateTime inicio, DateTime fim);
    }
}
