namespace myfinance_web_dotnet_service.Dtos
{
    public class ResumoMensal
    {
        public int Ano { get; set; }

        public int Mes { get; set; }

        public decimal TotalReceitas { get; set; }

        public decimal TotalDespesas { get; set; }

        public decimal TotalInvestimentos { get; set; }

        /// <summary>
        /// O que sobra no mes: receitas menos despesas e investimentos.
        /// </summary>
        public decimal Saldo => TotalReceitas - TotalDespesas - TotalInvestimentos;

        /// <summary>
        /// Transacoes do mes e ocorrencias das transacoes recorrentes, em ordem de data.
        /// </summary>
        public List<LancamentoMensal> Lancamentos { get; set; } = new();

        /// <summary>
        /// Despesas agrupadas por plano de conta, da maior para a menor.
        /// </summary>
        public List<TotalPorCategoria> DespesasPorCategoria { get; set; } = new();

        /// <summary>
        /// Um item por dia do mes, com os valores acumulados ate aquele dia.
        /// </summary>
        public List<AcumuladoDiario> EvolucaoDiaria { get; set; } = new();
    }
}
