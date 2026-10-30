namespace myfinance_web_dotnet_service.Dtos
{
    /// <summary>
    /// Um lancamento do mes: uma Transacao ou uma ocorrencia de TransacaoRecorrente.
    /// </summary>
    public class LancamentoMensal
    {
        /// <summary>
        /// Id da Transacao ou, quando <see cref="Recorrente"/>, da TransacaoRecorrente.
        /// </summary>
        public int? Id { get; set; }

        public bool Recorrente { get; set; }

        public DateTime Data { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public int PlanoContaId { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public TipoLancamento Tipo { get; set; }

        public decimal Valor { get; set; }
    }
}
