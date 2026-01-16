using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyLoan.Dtos.Common
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; } = [];
    }
}
