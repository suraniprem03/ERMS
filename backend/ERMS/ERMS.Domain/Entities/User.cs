using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class User : BaseEntity
    {
        public long EmployeeId { get; set; }

        public Employee Employee { get; set; } = default!;

        public long RoleId { get; set; }

        public Role Role { get; set; } = default!;

        public string PasswordHash { get; set; } = string.Empty;

        public bool IsPasswordChangeRequired { get; set; } = true;

        public DateTimeOffset? PasswordChangedAt { get; set; }

        public DateTimeOffset? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
