using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Helpers
{
    public static class MedicareUtilityHelper
    {

        public static string GenerateCorrelationId()
        {
            DateTime  currTime = DateTime.UtcNow;
            string corrId = currTime.ToString("yyMMddHHmmssffff");
            return corrId;
        }
    }
}
 