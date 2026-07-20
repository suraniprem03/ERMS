using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class RechargeClaim : BaseEntity
    {
        public long ClaimId { get; set; }

        public Claim Claim { get; set; } = default!;

        public decimal Amount { get; set; }
    }
}
