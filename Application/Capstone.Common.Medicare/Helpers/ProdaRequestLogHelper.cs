using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Helpers
{
    public static class ProdaRequestLogHelper
    {

        public static void UpdateProdaRequestLog(ProdaRequestLog requestLog)
        {
            using (TenantDBContext ctx = new TenantDBContext())
            {
                requestLog.Modified = DateTime.Now;
                ctx.Update(requestLog);
                ctx.SaveChanges();
            }

        }
        public static void UpdateProdaRequestLog(Guid messageId, string response)
        {
            using (TenantDBContext ctx = new TenantDBContext())
            {
                ProdaRequestLog requestLog = ctx.ProdaRequestLogs.Find(messageId);
                if (requestLog != null)
                {
                    requestLog.Response = response;
                    requestLog.Modified = DateTime.Now;
                    ctx.Update(requestLog);
                    ctx.SaveChanges();
                }

            }
        }
        public static Guid CreateProdaRequestLog(ProdaRequestLog requestLog)
        {
            try
            {
                using (TenantDBContext ctx = new TenantDBContext())
                {

                    ctx.ProdaRequestLogs.Add(requestLog);
                    ctx.SaveChanges();
                    ctx.Entry<ProdaRequestLog>(requestLog).State = EntityState.Detached;
                    return requestLog.MessageId;
                }

            }
            catch (Exception e)
            {

                return Guid.Empty;
            }
        }
        public static Guid CreateProdaRequestLog(Guid tenantId, string locationId, string request)
        {
            try
            {
                ProdaRequestLog requestLog = new ProdaRequestLog();
                requestLog.TenantId = tenantId;
                requestLog.LocationId = locationId;               
                requestLog.Request = request;
                return CreateProdaRequestLog(requestLog);
            }
            catch (Exception e)
            {

                return Guid.Empty;
            }
        }
    }    

    
}
