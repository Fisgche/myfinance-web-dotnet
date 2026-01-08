using System.Data.Common;

namespace myfinance_web_dotnet_domain.Entities;

public class Transacao
{
    public int? Id {get; set;}

    public string Historico  {get; set;}

    public DateTime Date  {get; set;}

    public decimal Valor {get; set;}

    public int PlanoContaId {get; set;}

    public PlanoConta PlanoConta {get; set;}
}
