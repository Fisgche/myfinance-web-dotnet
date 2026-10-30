using System.Globalization;
using myfinance_web_dotnet_service.Dtos;

namespace myfinance_web_dotnet.Helpers
{
    /// <summary>
    /// Formatacao sempre em pt-BR, independente da cultura do servidor (no Azure ela pode ser outra).
    /// </summary>
    public static class Formatacao
    {
        public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

        /// <summary>"R$ 1.234,56"</summary>
        public static string Moeda(decimal valor)
        {
            return valor.ToString("C", Cultura);
        }

        /// <summary>"+R$ 1.234,56", "-R$ 1.234,56" ou "R$ 0,00"</summary>
        public static string MoedaComSinal(decimal valor)
        {
            return valor > 0 ? "+" + Moeda(valor) : Moeda(valor);
        }

        /// <summary>
        /// Valor com o sinal do efeito no saldo: receitas positivas; despesas e investimentos negativos.
        /// </summary>
        public static string ValorLancamento(LancamentoMensal lancamento)
        {
            return lancamento.Tipo switch
            {
                TipoLancamento.Receita => MoedaComSinal(lancamento.Valor),
                TipoLancamento.Despesa or TipoLancamento.Investimento => MoedaComSinal(-lancamento.Valor),
                _ => Moeda(lancamento.Valor)
            };
        }

        /// <summary>"38,5%" a partir de 38.5</summary>
        public static string Percentual(decimal valor)
        {
            return (valor / 100).ToString("P1", Cultura);
        }

        /// <summary>"Outubro"</summary>
        public static string NomeMes(DateTime data)
        {
            return Capitalizar(data.ToString("MMMM", Cultura));
        }

        /// <summary>"Outubro de 2026"</summary>
        public static string MesAno(DateTime data)
        {
            return Capitalizar(MesAnoNaFrase(data));
        }

        /// <summary>"outubro de 2026", para usar no meio de uma frase</summary>
        public static string MesAnoNaFrase(DateTime data)
        {
            return data.ToString("MMMM 'de' yyyy", Cultura);
        }

        /// <summary>"set."</summary>
        public static string MesAbreviado(DateTime data)
        {
            return data.ToString("MMM", Cultura);
        }

        /// <summary>"05/10"</summary>
        public static string DiaMes(DateTime data)
        {
            return data.ToString("dd/MM", Cultura);
        }

        /// <summary>"2026-10": formato do parametro ?mes= e do input type="month"</summary>
        public static string ParametroMes(DateTime data)
        {
            return data.ToString("yyyy-MM", CultureInfo.InvariantCulture);
        }

        private static string Capitalizar(string texto)
        {
            return string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0], Cultura) + texto[1..];
        }
    }
}
