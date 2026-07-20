using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class Area : BaseEntity
    {
        public long StateId { get; set; }

        public State State { get; set; } = default!;

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ICollection<EmployeeArea> EmployeeAreas { get; set; } = new List<EmployeeArea>();
    }
}
