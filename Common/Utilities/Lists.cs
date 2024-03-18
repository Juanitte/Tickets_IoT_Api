using Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public static class Lists
    {
        public static List<LanguageDto> Languages = new List<LanguageDto>()
        {
            new LanguageDto() { Id = Language.English, Culture = "en-US", Name = "English" },
            new LanguageDto() { Id = Language.Spanish, Culture = "es-ES", Name = "Spanish" }
        };
    }
}
