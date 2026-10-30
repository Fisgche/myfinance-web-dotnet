using myfinance_web_dotnet_service.Dtos;

namespace myfinance_web_dotnet_service.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Resumo de receitas, despesas e investimentos do mes, incluindo as ocorrencias
        /// das transacoes recorrentes vigentes.
        /// </summary>
        ResumoMensal ObterResumoMensal(int ano, int mes);
    }
}
