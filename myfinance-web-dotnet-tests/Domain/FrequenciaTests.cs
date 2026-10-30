using myfinance_web_dotnet_domain.Enums;

namespace myfinance_web_dotnet_tests.Domain;

public class FrequenciaTests
{
    [Theory]
    [InlineData(Frequencia.Mensal, 1, "Mensal")]
    [InlineData(Frequencia.Bimestral, 2, "A cada 2 meses")]
    [InlineData(Frequencia.Trimestral, 3, "A cada 3 meses")]
    [InlineData(Frequencia.Quadrimestral, 4, "A cada 4 meses")]
    [InlineData(Frequencia.Semestral, 6, "A cada 6 meses")]
    [InlineData(Frequencia.Anual, 12, "Anual")]
    public void ValorEhIntervaloEmMesesEDescricaoVemDoDisplay(Frequencia frequencia, int meses, string descricao)
    {
        Assert.Equal(meses, (int)frequencia);
        Assert.Equal(descricao, frequencia.ObterDescricao());
    }

    [Fact]
    public void ObterDescricao_ValorNaoDefinido_RetornaONumero()
    {
        Assert.Equal("5", ((Frequencia)5).ObterDescricao());
    }
}
