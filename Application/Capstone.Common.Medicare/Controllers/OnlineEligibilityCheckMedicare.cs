using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.ECM;
using Capstone.Common.Medicare.Validation;
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
    public class OnlineEligibilityCheckMedicare : ControllerBase
    {
        Guid tenantId;
        string locationId;
        [HttpPost]
        public async Task<ApiResponse<OnlineEligibilityCheckResponseType>> CallOnlineEligibilityCheckMedicare(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<OnlineEligibilityCheckResponseType>();

            try
            {
                if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(locationId))
                {
                    this.tenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    OnlineEligibilityCheckRequestType onlineEligibilityCheckRequestType = JsonConvert.DeserializeObject<OnlineEligibilityCheckRequestType>(body);
                    if(onlineEligibilityCheckRequestType is null)
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
            string authorization = "Bearer " + authToken;
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            
            string MedicareNo = string.Empty;
            try
            {
                MedicareNo = onlineEligibilityCheckRequestType.Claim.Patient.Medicare.MemberNumber;
                bool isValid = MedicareCommonValidations.MedicareValidityCheck(MedicareNo);
                if (!isValid)
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("Invalid Medicare Number.");
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Invalid Medicare Number." + e.Message);
                return apiResponse;
            }
            //Header values
            string dhs_subjectId = MedicareNo; //Medicare Number
            string dhs_auditIdType = "Location Id";
            string dhs_subjectIdType = "Medicare Card";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = locationId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(onlineEligibilityCheckRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = tenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "ECM";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                //_logger.LogInformation("in patient claim");
                HttpClient apiClient = new HttpClient();

                MedicareEligibilityCheck medicareEligibilityCheck = new MedicareEligibilityCheck(apiClient);

                Task<OnlineEligibilityCheckResponseType> taskResp = medicareEligibilityCheck.McpEligibilityCheckMedicare(onlineEligibilityCheckRequestType, authorization, locationId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
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
