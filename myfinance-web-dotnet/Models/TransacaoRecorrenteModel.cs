using Microsoft.AspNetCore.Mvc.Rendering;
using myfinance_web_dotnet_domain.Entities;

namespace myfinance_web_dotnet.Models
{
    public class TransacaoRecorrenteModel
    {
        public int? Id { get; set; }

        public DateTime DataInicio { get; set; }

        public DateTime DataFim { get; set; }

        public decimal Valor { get; set; }

        public int PlanoContaId { get; set; }

        public PlanoConta PlanoConta { get; set; }
        public IEnumerable<SelectListItem>? ListPlanoConta { get; set; }
    }
}
