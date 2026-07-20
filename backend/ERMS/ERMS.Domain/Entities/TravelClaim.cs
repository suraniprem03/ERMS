using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class TravelClaim : BaseEntity
    {
        public long ClaimId { get; set; }

        public Claim Claim { get; set; } = default!;

        public DateOnly TravelDate { get; set; }

        public string Day { get; set; } = string.Empty;

        public string FromLocation { get; set; } = string.Empty;

        public string ToLocation { get; set; } = string.Empty;

        public decimal TotalKm { get; set; }

        public decimal RatePerKm { get; set; }

        public decimal CalculatedAmount { get; set; }
    }
}
