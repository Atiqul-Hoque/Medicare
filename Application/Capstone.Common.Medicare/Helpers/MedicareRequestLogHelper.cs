using Capstone.Common.Medicare.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Helpers
{
    public static class MedicareRequestLogHelper
    {
        public static void updateResponse(int id, string status)
        {
            using (ApplicationDBContext ctx = new ApplicationDBContext())
            {
                MedicareRequestLog medicareRequest = ctx.MedicareRequestLogs.Find(id);
                medicareRequest.Status = status;
                medicareRequest.Modified = DateTime.UtcNow;
                ctx.Update(medicareRequest);
                ctx.SaveChanges();
            }
        }

        public static int updateResponse(Guid messageId, string status, string response, string claimId)
        {
            using (ApplicationDBContext ctx = new ApplicationDBContext())
            {
                MedicareRequestLog medicareRequest = ctx.MedicareRequestLogs.Find(messageId);
                medicareRequest.Response = response;
                medicareRequest.ClaimId = claimId;
                medicareRequest.Status = status;
                medicareRequest.Modified = DateTime.UtcNow;
                ctx.Update(medicareRequest);
                return ctx.SaveChanges();
            }

        }
        public static void updateResponse(MedicareRequestLog medicareRequest)
        {
            using (ApplicationDBContext ctx = new ApplicationDBContext())
            {
                medicareRequest.Modified = DateTime.UtcNow;
                ctx.Update(medicareRequest);
                ctx.SaveChanges();
            }

        }
        public static Guid LogRequest(Guid TenantId, string strReq, string dhs_correlationId)
        {
            MedicareRequestLog medicareRequest = new MedicareRequestLog();

            using (ApplicationDBContext ctx = new ApplicationDBContext())
            {
                medicareRequest.Request = strReq;
                medicareRequest.CorrelationId = dhs_correlationId;
                medicareRequest.TenantId = TenantId;
                ctx.MedicareRequestLogs.Add(medicareRequest);
                ctx.SaveChanges();
                ctx.Entry<MedicareRequestLog>(medicareRequest).State = EntityState.Detached;
            }
            return medicareRequest.MessageId;
        }
        public static Guid LogRequest(MedicareRequestLog medicareRequest)
        {
           
            using (ApplicationDBContext ctx = new ApplicationDBContext())
            {
               
                ctx.MedicareRequestLogs.Add(medicareRequest);
                ctx.SaveChanges();
                ctx.Entry<MedicareRequestLog>(medicareRequest).State = EntityState.Detached;
            }
            return medicareRequest.MessageId;
        }
       
        internal static int updateResponse(Guid messageId, string status, string response)
        {
            return updateResponse(messageId, status, response, "");
        }
    }
}
