using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;
using ERMS.Domain.Enums;

namespace ERMS.Domain.Entities
{
    public class Claim : BaseEntity
    {
        public string ClaimNumber { get; set; } = string.Empty;

        public long EmployeeId { get; set; }

        public Employee Employee { get; set; } = default!;

        public long ExpenseCategoryId { get; set; }

        public ExpenseCategory ExpenseCategory { get; set; } = default!;

        public ClaimStatus Status { get; set; }

        public string? AdminRemarks { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }

        public DateTimeOffset? ApprovedAt { get; set; }

        public long? ApprovedByUserId { get; set; }

        public ICollection<ClaimAttachment> Attachments { get; set; } = new List<ClaimAttachment>();

        public ICollection<ClaimStatusHistory> StatusHistory { get; set; } = new List<ClaimStatusHistory>();

        public TravelClaim? TravelClaim { get; set; }

        public FoodClaim? FoodClaim { get; set; }

        public HotelClaim? HotelClaim { get; set; }

        public RechargeClaim? RechargeClaim { get; set; }
    }
}
