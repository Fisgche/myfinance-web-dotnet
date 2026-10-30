using myfinance_web_dotnet.Helpers;
using myfinance_web_dotnet_service.Dtos;

namespace myfinance_web_dotnet_tests.Web;

public class FormatacaoTests
{
    // A cultura pt-BR separa "R$" do numero com espaco nao separavel (U+00A0)
    private static string Normalizar(string texto) => texto.Replace(' ', ' ');

    [Theory]
    [InlineData("1234.5", "R$ 1.234,50")]
    [InlineData("0", "R$ 0,00")]
    [InlineData("-10.5", "-R$ 10,50")]
    public void Moeda_FormataEmReais(string valor, string esperado)
    {
        Assert.Equal(esperado, Normalizar(Formatacao.Moeda(decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture))));
    }

    [Theory]
    [InlineData("10", "+R$ 10,00")]
    [InlineData("-10", "-R$ 10,00")]
    [InlineData("0", "R$ 0,00")]
    public void MoedaComSinal_MostraOSinal(string valor, string esperado)
    {
        Assert.Equal(esperado, Normalizar(Formatacao.MoedaComSinal(decimal.Parse(valor, System.Globalization.CultureInfo.InvariantCulture))));
    }

    [Theory]
    [InlineData(TipoLancamento.Receita, "+R$ 50,00")]
    [InlineData(TipoLancamento.Despesa, "-R$ 50,00")]
    [InlineData(TipoLancamento.Investimento, "-R$ 50,00")]
    [InlineData(TipoLancamento.Indefinido, "R$ 50,00")]
    public void ValorLancamento_UsaOSinalDoEfeitoNoSaldo(TipoLancamento tipo, string esperado)
    {
        var lancamento = new LancamentoMensal { Tipo = tipo, Valor = 50m };

        Assert.Equal(esperado, Normalizar(Formatacao.ValorLancamento(lancamento)));
    }

    [Fact]
    public void Datas_FormatadasEmPortugues()
    {
        var data = new DateTime(2026, 9, 5);

        Assert.Equal("Setembro de 2026", Formatacao.MesAno(data));
        Assert.Equal("setembro de 2026", Formatacao.MesAnoNaFrase(data));
        Assert.Equal("Setembro", Formatacao.NomeMes(data));
        Assert.Equal("set.", Formatacao.MesAbreviado(data));
        Assert.Equal("05/09", Formatacao.DiaMes(data));
        Assert.Equal("2026-09", Formatacao.ParametroMes(data));
    }

    [Fact]
    public void Percentual_UsaVirgulaDecimal()
    {
        Assert.Equal("38,5%", Normalizar(Formatacao.Percentual(38.5m)));
    }
}
