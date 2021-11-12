using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.Veteran.Payment.Report;
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
    public class DVAPaymentReport : ControllerBase
    {
        Guid TenantId;
        string locationId;
        [HttpPost]
        [Route("GetVeteranPaymentReport")]
        public async Task<string> CallVeteranPaymentReport(string TenantId, string locationId, string correlationId)
        {
            try
            {
                if (!String.IsNullOrEmpty(TenantId) || !String.IsNullOrEmpty(locationId))
                { 
                    this.TenantId = Guid.Parse(TenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    DVAReportRequestType DVAReportRequestType = JsonConvert.DeserializeObject<DVAReportRequestType>(body);
                    return submitVeteranPaymentReport(DVAReportRequestType, correlationId);

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

        private string submitVeteranPaymentReport(DVAReportRequestType DVAReportRequestType, string dhs_correlationId)
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
            string strReq = JsonConvert.SerializeObject(DVAReportRequestType);
            string claimId = DVAReportRequestType.ClaimId;
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.ClaimId = claimId;
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.ServiceName = "DVYW";
            requestLog.CorrelationId = dhs_correlationId;

            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {
               
                HttpClient apiClient = new HttpClient();
                VeteranPaymentReport PaymentReport = new VeteranPaymentReport(apiClient);
                Task<DVAPaymentReportResponseType> taskResp = PaymentReport.McpDvaPaymentReport(DVAReportRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId,dhs_auditIdType, "urn:uuid:"+dhs_correlationId, dhs_productId, dhs_subjectIdType,apiKey);
                taskResp.Wait();
                DVAPaymentReportResponseType DVAPaymentReportResponseType = taskResp.Result;
                string response = "";
                string status = "";
                if (DVAPaymentReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(DVAPaymentReportResponseType);
                    status = DVAPaymentReportResponseType.Status;
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
