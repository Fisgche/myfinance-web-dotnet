using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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

        [Range(1, int.MaxValue, ErrorMessage = "Selecione um plano de conta.")]
        public int PlanoContaId { get; set; }

        [ValidateNever]
        public PlanoConta? PlanoConta { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem>? ListPlanoConta { get; set; }
    }
}
