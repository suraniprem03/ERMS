using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public string EmployeeCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public User? User { get; set; }

        public ICollection<EmployeeArea> EmployeeAreas { get; set; } = new List<EmployeeArea>();

        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
