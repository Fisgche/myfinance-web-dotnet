using myfinance_web_dotnet_domain.Entities.Base;
using myfinance_web_dotnet_domain.Enums;

namespace myfinance_web_dotnet_domain.Entities
{
    public class TransacaoRecorrente : EntityBase
    {
        public decimal Valor { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public Frequencia Frequencia { get; set; } = Frequencia.Mensal;

        public int PlanoContaId { get; set; }

        public PlanoConta? PlanoConta { get; set; }
    }
}
