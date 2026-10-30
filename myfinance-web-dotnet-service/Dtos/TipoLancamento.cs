namespace myfinance_web_dotnet_service.Dtos
{
    /// <summary>
    /// Tipo do lancamento, derivado de PlanoConta.Tipo ("R", "D" ou "I").
    /// Lancamentos de tipo Indefinido aparecem na lista, mas nao entram nos totais.
    /// </summary>
    public enum TipoLancamento
    {
        Indefinido = 0,
        Receita,
        Despesa,
        Investimento
    }
}
