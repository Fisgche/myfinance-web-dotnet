namespace myfinance_web_dotnet_service.Dtos
{
    /// <summary>
    /// Totais acumulados do primeiro dia do mes ate <see cref="Data"/> (inclusive).
    /// </summary>
    public class AcumuladoDiario
    {
        public DateTime Data { get; set; }

        public decimal Receitas { get; set; }

        public decimal Despesas { get; set; }

        public decimal Investimentos { get; set; }
    }
}
