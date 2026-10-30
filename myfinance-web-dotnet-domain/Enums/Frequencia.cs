using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace myfinance_web_dotnet_domain.Enums
{
    /// <summary>
    /// Frequencia de uma transacao recorrente. O valor de cada item e o intervalo em meses.
    /// </summary>
    public enum Frequencia
    {
        [Display(Name = "Mensal")]
        Mensal = 1,

        [Display(Name = "A cada 2 meses")]
        Bimestral = 2,

        [Display(Name = "A cada 3 meses")]
        Trimestral = 3,

        [Display(Name = "A cada 4 meses")]
        Quadrimestral = 4,

        [Display(Name = "A cada 6 meses")]
        Semestral = 6,

        [Display(Name = "Anual")]
        Anual = 12
    }

    public static class FrequenciaExtensions
    {
        public static string ObterDescricao(this Frequencia frequencia)
        {
            var membro = typeof(Frequencia).GetField(frequencia.ToString());
            return membro?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? frequencia.ToString();
        }
    }
}
