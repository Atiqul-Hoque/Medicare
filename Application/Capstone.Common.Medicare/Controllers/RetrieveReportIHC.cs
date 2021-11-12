using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.RTVW.IHCW;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RetrieveReportIHCW : ControllerBase
    {
        Guid TenantId;
        string locationId;
       [HttpPost]
        public async Task<string> CallRetrieveReportIHC(string TenantId, string locationId)
        {
            try
            {
                if (!String.IsNullOrEmpty(TenantId) || !String.IsNullOrEmpty(locationId))
                { 
                    this.TenantId = Guid.Parse(TenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    RetrieveReportRequestType RetrieveReportRequestType = JsonConvert.DeserializeObject<RetrieveReportRequestType>(body);
                    return submitRetrieveReportIHC(RetrieveReportRequestType);

                }
                else
                {
                    return "{ \"statusCode\": 400,\"errorCode\": 100,\"errors\": \"TenantId and LocationId required\"}";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        private string submitRetrieveReportIHC(RetrieveReportRequestType RetrieveReportRequestType)
        {
            Authentication authController = new Authentication();
            string token = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId);

            if (String.IsNullOrEmpty(token))
            {
                return "{ \"statusCode\": 400,\"errorCode\": 7,\"errors\": \"Token Errors\"}";
            }

            string authorization = "Bearer " + token; // PRODA

            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
           
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            string dhs_subjectId = "N/A"; //Medicare Number
            string dhs_subjectIdType = "N/A";
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(RetrieveReportRequestType);
          
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.ServiceName = "RTVW-IHCW";
            requestLog.CorrelationId = dhs_correlationId;

            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {
               
                HttpClient apiClient = new HttpClient();
                RetrieveReportIHC retrieveReport = new RetrieveReportIHC(apiClient);
                Task<RetrieveReportResponseType> taskResp = retrieveReport.McpRetrieveReportIhc(RetrieveReportRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId,dhs_auditIdType, "urn:uuid:"+dhs_correlationId, dhs_productId, dhs_subjectIdType,apiKey);
                taskResp.Wait();
                RetrieveReportResponseType retrieveReportResponseType = taskResp.Result;
                string response = "";
                string status = "SUCCESS";
                if (retrieveReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(retrieveReportResponseType);
                  
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

                else
                    resp = JsonConvert.SerializeObject(e.Message);

                MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                return resp;
            }
        }
      
    }
}
