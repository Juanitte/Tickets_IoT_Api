using Common.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dtos
{
    public class GenericFilterDto
    {
        public GenericFilterDto()
        {
            SorteablesFields = new List<string>();
        }

        public int Page { get; set; }

        public string? SearchString { get; set; }

        public string? OrderField { get; set; }

        [DefaultValue(OrderType.up)]
        public OrderType OrderType { get; set; }

        public int TotalCount { get; set; }

        public int ItemNumber { get; set; }

        public IEnumerable<string> SorteablesFields { get; set; }

        public IEnumerable<string> FilterablesFields { get; set; }
    }
}
