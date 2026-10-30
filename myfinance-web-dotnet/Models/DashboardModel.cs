using myfinance_web_dotnet_service.Dtos;

namespace myfinance_web_dotnet.Models
{
    public class DashboardModel
    {
        /// <summary>
        /// Primeiro dia do mes exibido.
        /// </summary>
        public DateTime Mes {get; set;}

        public DateTime MesAnterior => Mes.AddMonths(-1);

        public DateTime ProximoMes => Mes.AddMonths(1);

        /// <summary>
        /// Dia de hoje quando o mes exibido e o mes atual (marca "Hoje" no grafico); null nos demais meses.
        /// </summary>
        public int? DiaAtual {get; set;}

        public bool EhMesAtual => DiaAtual != null;

        public ResumoMensal Resumo {get; set;} = new();

        public List<IndicadorModel> Indicadores {get; set;} = new();

        /// <summary>
        /// Series do grafico de evolucao: apenas os tipos com movimento no mes.
        /// </summary>
        public List<SerieModel> SeriesEvolucao {get; set;} = new();
    }

    /// <summary>
    /// Chaves e nomes das series do dashboard. A chave define a cor (classes chave-* e --serie-* no dashboard.css),
    /// que e a mesma em todos os graficos.
    /// </summary>
    public static class SeriesDashboard
    {
        public const string Receitas = "receitas";
        public const string Despesas = "despesas";
        public const string Investimentos = "investimentos";

        public static string Chave(TipoLancamento tipo)
        {
            return tipo switch
            {
                TipoLancamento.Receita => Receitas,
                TipoLancamento.Despesa => Despesas,
                TipoLancamento.Investimento => Investimentos,
                _ => "indefinido"
            };
        }

        public static string NomeTipo(TipoLancamento tipo)
        {
            return tipo switch
            {
                TipoLancamento.Receita => "Receita",
                TipoLancamento.Despesa => "Despesa",
                TipoLancamento.Investimento => "Investimento",
                _ => "Sem tipo"
            };
        }
    }

    public class SerieModel
    {
        public string Chave {get; set;} = string.Empty;

        public string Nome {get; set;} = string.Empty;

        public List<decimal> Valores {get; set;} = new();
    }

    public enum SituacaoVariacao
    {
        Estavel,
        Boa,
        Ruim
    }

    /// <summary>
    /// Um total do mes com a comparacao com o mes anterior.
    /// </summary>
    public class IndicadorModel
    {
        public string Titulo {get; set;} = string.Empty;

        /// <summary>
        /// Chave da serie (cor da identificacao); null quando o indicador nao e uma serie (saldo).
        /// </summary>
        public string? Serie {get; set;}

        public decimal Valor {get; set;}

        public decimal ValorAnterior {get; set;}

        /// <summary>
        /// Se um aumento em relacao ao mes anterior e bom (receitas, investimentos, saldo) ou ruim (despesas).
        /// </summary>
        public bool AumentoEhBom {get; set;} = true;

        public string? Descricao {get; set;}

        public decimal Variacao => Valor - ValorAnterior;

        public SituacaoVariacao Situacao => Variacao == 0
            ? SituacaoVariacao.Estavel
            : (Variacao > 0) == AumentoEhBom ? SituacaoVariacao.Boa : SituacaoVariacao.Ruim;
    }
}
