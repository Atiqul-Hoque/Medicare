using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.InHospital.Claim;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Capstone.Common.Controllers.MedicareOnline
{
    [Route("api/[controller]")]
    [ApiController]
    public class InHospitalClaimPrivate : ControllerBase
    {
        Guid TenantId;
        string locationId;
        string correlationId;

        [HttpPost]
        //[Route("CallInPatientClaim")]
        public async Task<ApiResponse<InHospitalClaimResponseType>> CallInPatientClaim(string TenantId, string locationId, string? correlationId)
        {

            var apiResponse = new ApiResponse<InHospitalClaimResponseType>();

            try
            {
                if (!String.IsNullOrEmpty(TenantId) || !String.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(TenantId);
                    this.locationId = locationId;
                    if (!string.IsNullOrWhiteSpace(correlationId))
                    this.correlationId = correlationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    InHospitalClaimRequestType InHospitalClaimRequestType = JsonConvert.DeserializeObject<InHospitalClaimRequestType>(body);
                    return SubmitInHospitalClaim(InHospitalClaimRequestType);
                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Errors.Add("TenantId and LocationId required.");
                    apiResponse.ErrorCode = 100;
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.Result = null;
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors.Add(e.Message);
                return apiResponse;
            }
           
        }

        private ApiResponse<InHospitalClaimResponseType> SubmitInHospitalClaim(InHospitalClaimRequestType InHospitalClaimRequestType)
        {
            var apiResponse = new ApiResponse<InHospitalClaimResponseType>();

            Authentication authController = new Authentication();
            string token = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId);

            if (String.IsNullOrEmpty(token))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.ErrorCode = 7;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly.");
                return apiResponse;
            }

            string authorization = "Bearer " + token; // PRODA

            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            string claimTypeCode = InHospitalClaimRequestType.Claim.Summary.TypeCode;
            string endPoint = "";
            string serviceName = "";
            if (claimTypeCode == "PR")
            {
                    endPoint = "/mcp/inhospitalclaim/private/v1";
                    serviceName = "IHCW-private";

            }
            else if(claimTypeCode == "PU")
            {
                endPoint = "/mcp/inhospitalclaim/public/v1";
                serviceName = "IHCW-public";
            }
           
            else
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.ErrorCode = 9202;
                apiResponse.Result = null;
                apiResponse.Errors.Add("TypeCode is invalid.It should be  PR or PU");
                return apiResponse;
            }
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            string dhs_subjectId = "N/A"; //Medicare Number
            string dhs_subjectIdType = "N/A";

           if (InHospitalClaimRequestType.Claim.PatientSummary.FundMembership is Object)
            { 
             dhs_subjectId = InHospitalClaimRequestType.Claim.PatientSummary.FundMembership.MemberNumber; //Medicare Number
             dhs_subjectIdType = "Health Fund Member Number";
            }
            string dhs_correlationId = string.Empty;
            if (string.IsNullOrEmpty(correlationId))
            {
                string corrID = MedicareUtilityHelper.GenerateCorrelationId();
                dhs_correlationId = dhs_auditId + corrID;
                dhs_correlationId = dhs_correlationId.Substring(0, 24);
            }
            else
            {
                dhs_correlationId = correlationId;
            }
            string strReq = JsonConvert.SerializeObject(InHospitalClaimRequestType);

            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.ClaimId = InHospitalClaimRequestType.Claim.Summary.AccountReferenceId;
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;
            
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);

            try
            {

                HttpClient apiClient = new HttpClient();

                InHospitalClaim InHospitalClaim = new InHospitalClaim(apiClient);

                Task<InHospitalClaimResponseType> taskResp = InHospitalClaim.McpInHospitalClaimPrivate(InHospitalClaimRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, endPoint);
                InHospitalClaimResponseType InPatientMedicalClaimResponseType = taskResp.Result;
                string status = "";
                string response = "";
        
                if (InPatientMedicalClaimResponseType != null)
                {
                    response = JsonConvert.SerializeObject(InPatientMedicalClaimResponseType);
                    status = InPatientMedicalClaimResponseType.Status;
                    
                 
                    requestLog.Status = status;
                    requestLog.Response = response;
                   
                    MedicareRequestLogHelper.updateResponse(requestLog);

                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = InPatientMedicalClaimResponseType;
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
                    resp = JsonConvert.SerializeObject(e.Message);
                if (!Guid.Empty.Equals(dhs_messageId))
                {
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);

                }
                apiResponse.Result = null;
                apiResponse.Errors.Add(resp);
                apiResponse.TransactionId = dhs_correlationId;
                return apiResponse;
                // return resp;

            }
        }
        
   
   
    }


}
