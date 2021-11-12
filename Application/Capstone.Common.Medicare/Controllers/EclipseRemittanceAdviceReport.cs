using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.ERA;
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
    public class EclipseRemittanceAdviceReport : ControllerBase
    {
        Guid tenantId;
        string locationId;
        [HttpPost]
        public async Task<ApiResponse<RetrieveReportResponseType>> CallERA(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<RetrieveReportResponseType>();
            try
            {
                if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(locationId))
                {
                    this.tenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    RetrieveReportRequestType retrieveReportRequestType = JsonConvert.DeserializeObject<RetrieveReportRequestType>(body);
                    return RetrieveERA(retrieveReportRequestType);

                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("TenantId and LocationId required.");
                }
                return apiResponse;
            }
            
            catch (Exception e)
            {
                apiResponse.Result = null;
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors.Add(e.Message);
                return apiResponse;
            }
           
        }

        private ApiResponse<RetrieveReportResponseType> RetrieveERA(RetrieveReportRequestType retrieveReportRequestType)
        {
            var apiResponse = new ApiResponse<RetrieveReportResponseType>();
            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(tenantId.ToString(), locationId); // PRODA
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly.");

            }
            string authorization = "Bearer " + authToken;
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            
           
            //Header values
            string dhs_subjectId = "N/A"; //Medicare Number
            string dhs_auditIdType = "Location Id";
            string dhs_subjectIdType = "N/A";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(retrieveReportRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = tenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "ERA";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                //_logger.LogInformation("in patient claim");
                HttpClient apiClient = new HttpClient();

                EclipseRemittanceAdvice eclipseRemittanceAdvice = new EclipseRemittanceAdvice(apiClient);

                Task<RetrieveReportResponseType> taskResp = eclipseRemittanceAdvice.McpRetrieveReportEra(retrieveReportRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                RetrieveReportResponseType retrieveReportResponseType = taskResp.Result;
                string status = "";
                string response = "";
                string claimId = "";
                if (retrieveReportResponseType != null)
                {
                    response = JsonConvert.SerializeObject(retrieveReportResponseType);
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, status, response, claimId);

                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = retrieveReportResponseType;
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
                apiResponse.Errors.Add(resp);
                apiResponse.TransactionId = dhs_correlationId;
                return apiResponse;
                // return resp;

            }
        }
      
      
    }


}
