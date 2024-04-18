using Common.Utilities;
using System.ComponentModel;

namespace Common.Dtos
{
    public class GenericFilterRequestDto
    {
        public object? Value { get; set; }

        [DefaultValue(FilterType.equals)]
        public FilterType FilterType { get; set; }

        public string? PropertyName { get; set; }
    }
}
