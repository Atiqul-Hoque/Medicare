using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Models
{
    public partial class MedicareTokenProperty
    {
        public Guid TenantId { get; set; }
        public string LocationId { get; set; }
        public string AccessToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
