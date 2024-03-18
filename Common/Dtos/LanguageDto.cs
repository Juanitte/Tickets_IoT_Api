using Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dtos
{
    public class LanguageDto
    {
        public Language Id {  get; set; }

        public string Culture { get; set; }

        public string Name { get; set; }
    }
}
