using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.GPRW;
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
    public class GetParticipantFunds : ControllerBase
    {
        Guid tenantId;
        string locationId;

        [HttpGet]
        public async Task<ApiResponse<GetParticipantsResponseType>> GetParticipants([FromBody] GetParticipantsRequestType getParticipantsRequestType ,string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<GetParticipantsResponseType>();

            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.tenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    if (getParticipantsRequestType is null)
                    {
                        apiResponse.Result = null;
                        apiResponse.Errors.Add("Invalid Request body");
                        return apiResponse;
                    }

                    return SubmitGetParticipants(getParticipantsRequestType);

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
                 
           

        private ApiResponse<GetParticipantsResponseType> SubmitGetParticipants(GetParticipantsRequestType getParticipantsRequestType)
        {
            var apiResponse = new ApiResponse<GetParticipantsResponseType>();

            Guid dhs_messageId = Guid.Empty;
            string dhs_correlationId = string.Empty;

            try
            {
                bool isValid = ValidateParticipantType(getParticipantsRequestType);
                if (!isValid)
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("Invalid value supplied for Participant Type.");
                    return apiResponse;
                   
                }
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
                //Header values
                string dhs_auditIdType = "Location Id";
                string dhs_auditId = locationId;
                string dhs_productId = tenantMedicareProperty.ApplicationName;
                string apiKey = tenantMedicareProperty.Apikey; 
                string dhs_subjectId = "N/A";
                string dhs_subjectIdType = "N/A";
                string corrID = MedicareUtilityHelper.GenerateCorrelationId();
                dhs_correlationId = dhs_auditId + corrID;
                dhs_correlationId = dhs_correlationId.Substring(0, 24);
                string strReq = JsonConvert.SerializeObject(getParticipantsRequestType);
            
                MedicareRequestLog requestLog = new MedicareRequestLog();
                requestLog.TenantId = tenantId;
                requestLog.Request = strReq;
                requestLog.CorrelationId = dhs_correlationId;
                requestLog.ServiceName = "GPRW";
                dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            
                //_logger.LogInformation("in patient claim");
                HttpClient apiClient = new HttpClient();
                GetParticipants getParticipants = new GetParticipants(apiClient);
                Task<GetParticipantsResponseType> taskResp = getParticipants.McpGetParticipants(getParticipantsRequestType, authorization,dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:"+dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
                taskResp.Wait();
                GetParticipantsResponseType getParticipantsResponseType = taskResp.Result;
                string response = "";
                string status = "";
                if (getParticipantsResponseType != null)
                {
                    response = JsonConvert.SerializeObject(getParticipantsResponseType);
                    if(getParticipantsResponseType.Participant.Count > 0)
                    {
                        status = "COMPLETE";
                    }
                    requestLog.Status = status;
                    requestLog.Response = response;
                    MedicareRequestLogHelper.updateResponse(requestLog);
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = getParticipantsResponseType;
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

        private bool ValidateParticipantType(GetParticipantsRequestType getParticipantsRequestType)
        {
            if(getParticipantsRequestType is null ||(string.IsNullOrWhiteSpace(getParticipantsRequestType.ParticipantType)) || !("F").Equals(getParticipantsRequestType.ParticipantType))
            {
                return false;
            }
            return true;
        }

    }
}
