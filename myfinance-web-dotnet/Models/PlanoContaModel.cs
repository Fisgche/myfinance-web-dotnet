using System.ComponentModel.DataAnnotations;

namespace myfinance_web_dotnet.Models
{
    public class PlanoContaModel
    {
        public int? Id {get; set;}

        [Required(ErrorMessage = "Informe a descricao.")]
        public string Descricao  {get; set;} = string.Empty;

        [Required(ErrorMessage = "Informe o tipo.")]
        [RegularExpression("^[DRI]$", ErrorMessage = "Tipo invalido.")]
        public string Tipo  {get; set;} = string.Empty;
    }
}
