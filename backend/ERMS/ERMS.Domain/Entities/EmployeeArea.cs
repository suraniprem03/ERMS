using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class EmployeeArea : BaseEntity
    {
        public long EmployeeId { get; set; }

        public Employee Employee { get; set; } = default!;

        public long AreaId { get; set; }

        public Area Area { get; set; } = default!;
    }
}
