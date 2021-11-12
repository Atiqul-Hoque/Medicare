using System;
using System.Collections.Generic;

#nullable disable

namespace Capstone.Common.Medicare.Models
{
    public partial class TenantAzureProperty
    {
        public Guid TenantId { get; set; }
        public string LocationId { get; set; }
        public string ClientId { get; set; }
        public string SecretKey { get; set; }
        public string AzureTenantId { get; set; }
        public string CertificateUri { get; set; }
        public string CertificateName { get; set; }
    }
}
