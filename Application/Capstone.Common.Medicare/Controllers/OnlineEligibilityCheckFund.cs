using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.ECF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnlineEligibilityCheckFund : ControllerBase
    {
        Guid tenantId;
        string locationId;
        [HttpPost]
        public async Task<ApiResponse<OnlineEligibilityCheckResponseType>> CallOnlineEligibilityCheckFund(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<OnlineEligibilityCheckResponseType>();

            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.locationId = locationId;
                    this.tenantId = Guid.Parse(tenantId);
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    OnlineEligibilityCheckRequestType onlineEligibilityCheckRequestType = JsonConvert.DeserializeObject<OnlineEligibilityCheckRequestType>(body);
                    if (onlineEligibilityCheckRequestType is null)
                    {
                        apiResponse.Result = null;
                        apiResponse.Errors.Add("Invalid Request body.");
                    }
                    return SubmitOEC(onlineEligibilityCheckRequestType);

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
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add(e.Message);
                return apiResponse;
            }

        }

       
        private ApiResponse<OnlineEligibilityCheckResponseType> SubmitOEC(OnlineEligibilityCheckRequestType onlineEligibilityCheckRequestType)
        {
            var apiResponse = new ApiResponse<OnlineEligibilityCheckResponseType>();

            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(tenantId.ToString(), locationId); // PRODA
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token not generated properly.");
                return apiResponse;
            }
            string authorization = "Bearer " + authToken; string healthFundNo = string.Empty;
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            
            try
            {
                 healthFundNo = onlineEligibilityCheckRequestType.Claim.Patient.HealthFund.MemberNumber;
                if (string.IsNullOrWhiteSpace(healthFundNo))
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("Invalid Health Fund Details.");
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Invalid Health Fund Details."+e.Message);
                return apiResponse;
            }
            
            
            //Header values
            string dhs_subjectId = healthFundNo; //Medicare Number
            string dhs_auditIdType = "Location Id";
            string dhs_subjectIdType = "Health Fund Member Number";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(onlineEligibilityCheckRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = tenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "ECF";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                //_logger.LogInformation("in patient claim");
                HttpClient apiClient = new HttpClient();

                FundEligibilityCheck fundEligibilityCheck = new FundEligibilityCheck(apiClient);

                Task<OnlineEligibilityCheckResponseType> taskResp = fundEligibilityCheck.McpEligibilityCheckHf(onlineEligibilityCheckRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                OnlineEligibilityCheckResponseType onlineEligibilityCheckResponseType = taskResp.Result;
                string status = "";
                string response = "";
                string claimId = "";
                if (onlineEligibilityCheckResponseType != null)
                {
                    response = JsonConvert.SerializeObject(onlineEligibilityCheckResponseType);
                    status = onlineEligibilityCheckResponseType.Status;
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, status, response, claimId);

                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = onlineEligibilityCheckResponseType;
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
                else if (typeof(SqlException) == e.InnerException.GetType())
                {
                    SqlException sqlExp = (SqlException)e.InnerException;
                    resp = sqlExp.Message;
                    apiResponse.StatusCode = StatusCodes.Status500InternalServerError;

                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    resp = JsonConvert.SerializeObject(e.Message);
                }

                if (!Guid.Empty.Equals(dhs_messageId))
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                apiResponse.Result = null;
                apiResponse.Errors.Add(resp);
                apiResponse.TransactionId = dhs_correlationId;
                // return resp;
                return apiResponse;
            }
        }
    
    }
}
