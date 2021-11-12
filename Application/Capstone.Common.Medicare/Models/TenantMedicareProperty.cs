using System;
using System.Collections.Generic;

#nullable disable

namespace Capstone.Common.Medicare.Models
{
    public partial class TenantMedicareProperty
    {
        public Guid TenantId { get; set; }
        public string LocationId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceActivationCode { get; set; }
        public string ProdaOrgRa { get; set; }
        public string ClientId { get; set; }
        public string ApplicationName { get; set; }
        public string Apikey { get; set; }
        public string Apisecret { get; set; }
        public DateTime Created { get; set; }
    }
}
