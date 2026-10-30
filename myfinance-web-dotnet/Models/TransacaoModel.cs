using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using myfinance_web_dotnet_domain.Entities;

namespace myfinance_web_dotnet.Models
{
    /// <summary>
    /// Model unico para transacoes singelas (Transacao) e recorrentes (TransacaoRecorrente).
    /// Os campos usados dependem de <see cref="Recorrente"/>.
    /// </summary>
    public class TransacaoModel
    {
        public int? Id {get; set;}

        public bool Recorrente {get; set;}

        // Transacao singela
        public string Historico  {get; set;} = string.Empty;

        public DateTime Date  {get; set;}

        // Transacao recorrente
        public DateTime DataInicio {get; set;}

        public DateTime DataFim {get; set;}

        // Comuns
        public decimal Valor {get; set;}

        [Range(1, int.MaxValue, ErrorMessage = "Selecione um plano de conta.")]
        public int PlanoContaId {get; set;}

        [ValidateNever]
        public PlanoConta? PlanoConta {get; set;}

        [ValidateNever]
        public IEnumerable<SelectListItem>? ListPlanoConta {get; set;}
    }
}
