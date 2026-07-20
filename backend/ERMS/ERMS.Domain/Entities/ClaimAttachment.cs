using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ERMS.Domain.Common;

namespace ERMS.Domain.Entities
{
    public class ClaimAttachment : BaseEntity
    {
        public long ClaimId { get; set; }

        public Claim Claim { get; set; } = default!;

        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FileExtension { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }
    }
}
