using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.STSW;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetrieveStatusReport : ControllerBase
    {
        Guid TenantId;
        string locationId;
        [HttpPost]
        [Route("GetStatusReport")]
        public async Task<string> CallStatusReport(string tenantId, string locationId,[FromBody] StatusReportRequestType statusReportRequestType)
        {
            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    return  submitStatusProcessignReport(statusReportRequestType);

                }
                else
                {
                    return "TenantId and LocationId required.";
                }
            }
            catch (ArgumentNullException arg)
            {
                return arg.Message;
            }
            catch (FormatException f)
            {
                return f.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        private string submitStatusProcessignReport(StatusReportRequestType statusReportRequestType)
        {
            Authentication authController = new Authentication();
            string authorization = "Bearer " + authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId); // PRODA
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            //Header values
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string dhs_subjectId = "N/A";
            string dhs_subjectIdType = "N/A";
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);


            string strReq = JsonConvert.SerializeObject(statusReportRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "STSW";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                //_logger.LogInformation("status report");
                HttpClient apiClient = new HttpClient();
                StatusReport statusReport = new StatusReport(apiClient);
                Task<StatusReportResponseType> taskResp = statusReport.McpStatusReport(statusReportRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                taskResp.Wait();
                StatusReportResponseType statusReportResponseType = taskResp.Result;
                string response = "";
                string status = "";
                if (statusReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(statusReportResponseType);
                    status = statusReportResponseType.Status;
                    requestLog.Status = status;
                    requestLog.Response = response;
                    MedicareRequestLogHelper.updateResponse(requestLog);
                }
                return response;

            }
            catch (Exception e)
            {
                string resp = e.Message;
                if (typeof(ApiException<ServiceMessagesType>) == e.InnerException.GetType())
                {
                    ApiException<ServiceMessagesType> exp = (ApiException<ServiceMessagesType>)e.InnerException;
                    resp = JsonConvert.SerializeObject(exp.Result);

                }
                else if (typeof(ApiException) == e.InnerException.GetType())
                {
                    ApiException exp = (ApiException)e.InnerException;
                    resp = exp.Response;

                }
                else if (typeof(SqlException) == e.InnerException.GetType())
                {
                    SqlException sqlExp = (SqlException)e.InnerException;
                    resp = sqlExp.Message;

                }
                else
                    resp = JsonConvert.SerializeObject(e.Message);

                MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                return resp;
            }
        }
      
    }
}
