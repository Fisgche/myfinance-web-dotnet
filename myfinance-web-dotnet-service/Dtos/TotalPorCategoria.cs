namespace myfinance_web_dotnet_service.Dtos
{
    public class TotalPorCategoria
    {
        public int PlanoContaId { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        /// <summary>
        /// Participacao no total do tipo, de 0 a 100 (uma casa decimal).
        /// </summary>
        public decimal Percentual { get; set; }
    }
}
