using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.InPatient.Claim.MO;
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
    public class InPatientClaim : ControllerBase
    {
        Guid TenantId;
        string locationId;
        string correlationId;

        [HttpPost]
        [Route("CallInPatientClaim")]
        public async Task<ApiResponse<InPatientMedicalClaimResponseType>> CallInPatientClaim(string TenantId, string locationId, string? correlationId)
        {

            var apiResponse = new ApiResponse<InPatientMedicalClaimResponseType>();

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
                    InPatientMedicalClaimRequestType InPatientMedicalClaimRequestType = JsonConvert.DeserializeObject<InPatientMedicalClaimRequestType>(body);
                    return SubmitInPatientClaim(InPatientMedicalClaimRequestType);

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

        private ApiResponse<InPatientMedicalClaimResponseType> SubmitInPatientClaim(InPatientMedicalClaimRequestType InPatientMedicalClaimRequestType)
        {
            var apiResponse = new ApiResponse<InPatientMedicalClaimResponseType>();

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
            string claimTypeCode = InPatientMedicalClaimRequestType.Claim.ClaimTypeCode;
            string serviceTypeCode = InPatientMedicalClaimRequestType.Claim.ServiceTypeCode;
            string endPoint = "";
            string serviceName = "";
            if (claimTypeCode == "MO")
            {
                if (serviceTypeCode == "O")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mo/general/v1";
                    serviceName = "IMCW-MO-general";
                }
                else if (serviceTypeCode == "S")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mo/specialist/v1";
                    serviceName = "IMCW-MO-specialist";
                }
                else if (serviceTypeCode == "P")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mo/pathology/v1";
                    serviceName = "IMCW-MO-pathology";
                }
                else
                {

                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("ServiceTypeCode is invalid.It should be  S,P or O");
                    return apiResponse;
                }
            }
            else if(claimTypeCode == "MB")
            {
                if (serviceTypeCode == "O")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mb/general/v1";
                    serviceName = "IMCW-MB-general";
                }
                else if (serviceTypeCode == "S")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mb/specialist/v1";
                    serviceName = "IMCW-MB-specialist";
                }
                else if (serviceTypeCode == "P")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/mb/pathology/v1";
                    serviceName = "IMCW-MB-pathology";
                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("ServiceTypeCode is invalid.It should be  S,P or O");
                    return apiResponse;
                }
            }
            else if (claimTypeCode == "AG")
            {
                if (serviceTypeCode == "O")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/ag/general/v1";
                    serviceName = "IMCW-AG-general";
                }
                else if (serviceTypeCode == "S")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/ag/specialist/v1";
                    serviceName = "IMCW-AG-specialist";
                }
                else if (serviceTypeCode == "P")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/ag/pathology/v1";
                    serviceName = "IMCW-AG-pathology";
                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("ServiceTypeCode is invalid.It should be  S,P or O");
                    return apiResponse; 
                }
            }
            else if (claimTypeCode == "SC")
            {
                if (serviceTypeCode == "O")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/sc/general/v1";
                    serviceName = "IMCW-SC-general";
                }
                else if (serviceTypeCode == "S")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/sc/specialist/v1";
                    serviceName = "IMCW-SC-specialist";
                }
                else if (serviceTypeCode == "P")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/sc/pathology/v1";
                    serviceName = "IMCW-SC-pathology";
                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("ServiceTypeCode is invalid.It should be  S,P or O");
                    return apiResponse;
                }
            }
            else if (claimTypeCode == "PC")
            {
                if (serviceTypeCode == "O")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/pc/general/v1";
                    serviceName = "IMCW-PC-general";
                }
                else if (serviceTypeCode == "S")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/pc/specialist/v1";
                    serviceName = "IMCW-PC-specialist";
                }
                else if (serviceTypeCode == "P")
                {
                    endPoint = "/mcp/inpatientmedicalclaim/pc/pathology/v1";
                    serviceName = "IMCW-PC-pathology";
                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("ServiceTypeCode is invalid.It should be  S,P or O");
                    return apiResponse;
                }
            }
            else
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.ErrorCode = 9202;
                apiResponse.Result = null;
                apiResponse.Errors.Add("TypeCode is invalid.It should be  MO,MB,PC,AG or SC");
                return apiResponse;
            }
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            string dhs_subjectId = "N/A"; //Medicare Number
            string dhs_subjectIdType = "N/A";

           if (InPatientMedicalClaimRequestType.Claim.Patient.Medicare is Object)
            { 
             dhs_subjectId = InPatientMedicalClaimRequestType.Claim.Patient.Medicare.MemberNumber; //Medicare Number
             dhs_subjectIdType = "Medicare Card";
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
            string strReq = JsonConvert.SerializeObject(InPatientMedicalClaimRequestType);

            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.ClaimId = InPatientMedicalClaimRequestType.Claim.AccountReferenceId;
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;
            
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);

            try
            {

                HttpClient apiClient = new HttpClient();

                InPatientClaimMO InPatientClaimMO = new InPatientClaimMO(apiClient);

                Task<InPatientMedicalClaimResponseType> taskResp = InPatientClaimMO.McpInPatientMedicalClaimMoGeneral(InPatientMedicalClaimRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, endPoint);
                InPatientMedicalClaimResponseType InPatientMedicalClaimResponseType = taskResp.Result;
                string status = "";
                string response = "";
                string claimId = InPatientMedicalClaimRequestType.Claim.AccountReferenceId;
                if (InPatientMedicalClaimResponseType != null)
                {
                    response = JsonConvert.SerializeObject(InPatientMedicalClaimResponseType);
                    status = InPatientMedicalClaimResponseType.Status;
                    
                   if(InPatientMedicalClaimResponseType.ClaimSummary is object)
                    {
                        claimId = InPatientMedicalClaimResponseType.ClaimSummary.AccountReferenceId;
                    }
                    requestLog.Status = status;
                    requestLog.Response = response;
                    requestLog.ClaimId = claimId;
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
