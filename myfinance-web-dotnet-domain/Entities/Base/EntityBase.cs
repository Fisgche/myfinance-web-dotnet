using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace myfinance_web_dotnet_domain.Entities.Base
{
    public class EntityBase
    {
        [Key]
        public int? Id {get; set;}
    }
}