using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;
using ERMS.Domain.Enums;

namespace ERMS.Domain.Entities
{
    public class ClaimStatusHistory : BaseEntity
    {
        public long ClaimId { get; set; }

        public Claim Claim { get; set; } = default!;

        public ClaimStatus Status { get; set; }

        public string? Comment { get; set; }
    }
}
