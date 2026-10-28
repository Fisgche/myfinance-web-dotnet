using myfinance_web_dotnet_domain.Entities.Base;

namespace myfinance_web_dotnet_domain.Entities
{
    public class TransacaoRecorrente : EntityBase
    {
        public decimal Valor { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public int PlanoContaId { get; set; }

        public PlanoConta? PlanoConta { get; set; }
    }
}
