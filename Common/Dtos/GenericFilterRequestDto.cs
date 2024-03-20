using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dtos
{
    public class GenericFilterRequestDto
    {
        public int? FilterId { get; set; }
        public string? FilterName { get; set; }
        public string? FilterRole { get; set; }
    }
}
