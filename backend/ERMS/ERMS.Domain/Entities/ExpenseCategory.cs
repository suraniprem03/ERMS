using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class ExpenseCategory : BaseEntity
    {
        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public decimal? MaxLimit { get; set; }

        public bool IsDynamic { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
