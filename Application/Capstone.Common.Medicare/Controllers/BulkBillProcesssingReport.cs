using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.BulkBill.ProcessingReport;
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
    public class BulkBillProcesssingReport : ControllerBase
    {
        Guid tenantId;
        string locationId;

        [HttpPost]
        [Route("GetBulkBillProcessingReport")]
        public async Task<ApiResponse<BBSProcessingReportResponseType>> CallBulkBillProcessingReport(string tenantId,string locationId, string correlationId)
        {
            var apiResponse = new ApiResponse<BBSProcessingReportResponseType>();
            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.tenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    BBSReportRequestType bBSReportRequestType = JsonConvert.DeserializeObject<BBSReportRequestType>(body);
                    return SubmitBulkBillProcessignReport(bBSReportRequestType, correlationId);

                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add( "TenantId and LocationId required." );

                }
                return apiResponse;
            }
           
            catch (Exception e)
            {
                apiResponse.Result = null;
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors .Add( e.Message );
                return apiResponse;
            }

        }

        private ApiResponse<BBSProcessingReportResponseType> SubmitBulkBillProcessignReport(BBSReportRequestType bBSReportRequestType, string dhs_correlationId)
        {
            var apiResponse = new ApiResponse<BBSProcessingReportResponseType>();

            Authentication authController = new Authentication();

            string authToken =authController.GetAuthorisationCodeUsingAzure(tenantId.ToString(), locationId); // PRODA
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly.");

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
            string MedicareNo = string.Empty;
            //Header values
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey; 
            string dhs_subjectId = "N/A"; //Medicare Number
            string dhs_subjectIdType = "N/A";

            string strReq = JsonConvert.SerializeObject(bBSReportRequestType);
            string claimId = bBSReportRequestType.ClaimId;
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.ClaimId = claimId;
            requestLog.TenantId = tenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "BPRW";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {
                HttpClient apiClient = new HttpClient();
                BBSProcessingReport processingReport = new BBSProcessingReport(apiClient);
                Task<BBSProcessingReportResponseType> taskResp = processingReport.McpBulkBillProcessingReport(bBSReportRequestType, authorization,dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:"+dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                taskResp.Wait();
                BBSProcessingReportResponseType bBSProcessingReportResponseType = taskResp.Result;
                string response = "";
                string status = "";
                if (bBSProcessingReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(bBSProcessingReportResponseType);
                    status = bBSProcessingReportResponseType.Status;
                    requestLog.Status = status;
                    requestLog.Response = response;
                    MedicareRequestLogHelper.updateResponse(requestLog);
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = bBSProcessingReportResponseType;
                return apiResponse;
                // return response;

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
                apiResponse.Errors.Add(resp );
                apiResponse.TransactionId = dhs_correlationId;
                return apiResponse;
                //return resp;
            }
        }
      
    }
}
