using System;
using System.Collections.Generic;

#nullable disable

namespace Capstone.Common.Medicare.Models
{
    public partial class ProdaTokenProperty
    {
        public Guid TenantId { get; set; }
        public string LocationId { get; set; }
        public string DeviceName { get; set; }
        public DateTime? DeviceExpiry { get; set; }
        public DateTime? KeyExpiry { get; set; }
        public string AccessToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
