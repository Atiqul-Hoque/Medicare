using System;
using System.Collections.Generic;

#nullable disable

namespace Capstone.Common.Medicare.Models
{
    public partial class ProdaRequestLog
    {
        public Guid TenantId { get; set; }
        public string LocationId { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public Guid MessageId { get; set; }
        public Guid? Correlationid { get; set; }
        public string Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
