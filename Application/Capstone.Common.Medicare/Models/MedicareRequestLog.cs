using System;
using System.Collections.Generic;

#nullable disable

namespace Capstone.Common.Medicare.Models
{
    public partial class MedicareRequestLog
    {
        public Guid MessageId { get; set; }
        public Guid TenantId { get; set; }
        public string CorrelationId { get; set; }
        public string ServiceName { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public string ClaimId { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
