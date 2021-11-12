using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.BulkBill.PaymentReport;
using Microsoft.AspNetCore.Http;
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
    public class BulkBillPaymentReport : ControllerBase
    {
        Guid TenantId;
        string locationId;
        [HttpPost]
        public async Task<ApiResponse<BBSPaymentReportResponseType>> CallBulkBillPaymentReport(string tenantId,string locationId, string correlationId)
        {
            var apiResponse = new ApiResponse<BBSPaymentReportResponseType>();

            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    BBSReportRequestType bBSReportRequestType = JsonConvert.DeserializeObject<BBSReportRequestType>(body);
                    return SubmitBulkBillPaymentReport(bBSReportRequestType, correlationId);

                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("TenantId and LocationId required." );
                    return apiResponse;
                }
            }
         
            catch (Exception e)
            {
                apiResponse.Result = null;
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors.Add(e.Message );
                return apiResponse;
            }

        }

        private ApiResponse<BBSPaymentReportResponseType> SubmitBulkBillPaymentReport(BBSReportRequestType bBSReportRequestType, string dhs_correlationId)
        {
            var apiResponse = new ApiResponse<BBSPaymentReportResponseType>();

            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId); // PRODA
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly." );
                return apiResponse;
            }
            if (tenantMedicareProperty is null)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Tenant Properties not loaded properly.");

            }

            if (apiResponse.Errors.Count > 0)
            {
                return apiResponse;
            }
            string authorization = "Bearer " + authToken;

            //Header values
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string dhs_subjectId = "N/A"; 
            string dhs_subjectIdType = "N/A";

            string strReq = JsonConvert.SerializeObject(bBSReportRequestType);
            string claimId = bBSReportRequestType.ClaimId;
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.ClaimId = claimId;
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "BPYW";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                HttpClient apiClient = new HttpClient();
                BBSPaymentReport paymentReport = new BBSPaymentReport(apiClient);
                Task<BBSPaymentReportResponseType> taskResp = paymentReport.McpBulkBillPaymentReport(bBSReportRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                taskResp.Wait();
                BBSPaymentReportResponseType bBSPaymentReportResponseType = taskResp.Result;
                string response = "";
                string status = "";
                if (bBSPaymentReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(bBSPaymentReportResponseType);
                    status = bBSPaymentReportResponseType.Status;
                    requestLog.Status = status;
                    requestLog.Response = response;
                    MedicareRequestLogHelper.updateResponse(requestLog);
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = bBSPaymentReportResponseType;
                return apiResponse;
            }
            catch (Exception e)
            {
                string resp = e.Message;
                if (typeof(ApiException<ServiceMessagesType>) == e.InnerException.GetType())
                {
                    ApiException<ServiceMessagesType> exp = (ApiException<ServiceMessagesType>)e.InnerException;
                    resp = JsonConvert.SerializeObject(exp.Result);
                    apiResponse.StatusCode = exp.StatusCode;
                }
                else if (typeof(ApiException) == e.InnerException.GetType())
                {
                    ApiException exp = (ApiException)e.InnerException;
                    resp = exp.Response;
                    apiResponse.StatusCode = exp.StatusCode;
                }

                else
                {
                    resp = JsonConvert.SerializeObject(e.Message);
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                }

                if (!Guid.Empty.Equals(dhs_messageId))
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                apiResponse.Result = null;
                apiResponse.Errors.Add( resp );
                apiResponse.TransactionId = dhs_correlationId;
                return apiResponse;
               // return resp;
            }
        }
    
    }
}
