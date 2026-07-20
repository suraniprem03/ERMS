using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERMS.Domain.Common
{
    public abstract class BaseEntity
    {
        public long Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public long CreatedBy { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }

        public long? UpdatedBy { get; set; }
    }
}
