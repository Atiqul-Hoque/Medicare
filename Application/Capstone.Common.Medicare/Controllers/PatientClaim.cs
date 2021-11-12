
using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.PCI.General;
using Capstone.Common.Medicare.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientClaim : ControllerBase
    {

        private Guid TenantId;
        private string locationId;
       
        [HttpPost]
        [Route("SubmitClaim")]
        public async Task<ApiResponse<PatientClaimInteractiveResponseType>> CallPCIGeneralAsync(string tenantId,string locationId, string correlationId)
        {
            var apiResponse = new ApiResponse<PatientClaimInteractiveResponseType>();

            try
            {
                if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                   
                    PatientClaimInteractiveRequestType request = JsonConvert.DeserializeObject<PatientClaimInteractiveRequestType>(body);
                    if (request is null)
                    {
                        apiResponse.Result = null;
                        apiResponse.ErrorCode = 99;
                        apiResponse.Errors.Add( "invalid Request body" );
                        return apiResponse;
                    }
                    return SubmitPCIGeneral(request,  correlationId);
                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.ErrorCode = 100;
                    apiResponse.Errors.Add("TenantId and LocationId required." );
                    
                }
                return apiResponse;
            }
         
            catch (Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.ErrorCode = 99;
                apiResponse.Errors.Add(e.Message );
                return apiResponse;

            }

        }

        
        private ApiResponse<PatientClaimInteractiveResponseType> SubmitPCIGeneral(PatientClaimInteractiveRequestType patientClaimInteractiveRequestType, string correlationId)
        {
            var apiResponse = new ApiResponse<PatientClaimInteractiveResponseType>();
            Authentication authController = new Authentication();
            string authToken =  authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId); // PRODA
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.ErrorCode = 7;
                apiResponse.Errors.Add("Authentication Token not generated properly." );
                return apiResponse;
            }
            string authorization = "Bearer " + authToken;
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;

            string MedicareNo = string.Empty;
            string endPoint = string.Empty;
            string serviceName = string.Empty;
            string typecode = string.Empty;
            try
            {
                MedicareNo = patientClaimInteractiveRequestType.PatientClaimInteractive.Patient.Medicare.MemberNumber;
                if (patientClaimInteractiveRequestType.PatientClaimInteractive.Referral is Object) 
                {
                typecode = patientClaimInteractiveRequestType.PatientClaimInteractive.Referral.TypeCode;
                }
                if (typecode == "P")
                {
                    endPoint = "/mcp/patientclaiminteractive/pathology/v1";
                    serviceName = "PCI-pathology";
                }
                else if (typecode == "S" || typecode == "D")
                {
                    endPoint = "/mcp/patientclaiminteractive/specialist/v1";
                    serviceName = "PCI-specialist";
                }
                else 
                {
                    endPoint = "/mcp/patientclaiminteractive/general/v1";
                    serviceName = "PCI-general";
                }

                bool isValid = MedicareCommonValidations.MedicareValidityCheck(MedicareNo);
                if (!isValid)
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Result = null;
                    apiResponse.ErrorCode = 9202;
                    apiResponse.Errors.Add( "Invalid Medicare Number." );
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.ErrorCode = 9202;
                apiResponse.Errors.Add("Invalid Medicare Number."+e.Message );
                return apiResponse;
            }
            //Header values
            string dhs_subjectId = MedicareNo; //Medicare Number
            string dhs_auditIdType = "Location Id";
            string dhs_subjectIdType = "Medicare Card";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
			string dhs_correlationId = string.Empty;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                string corrID = MedicareUtilityHelper.GenerateCorrelationId();
                dhs_correlationId = locationId + corrID;
                dhs_correlationId = dhs_correlationId.Substring(0, 24);
            }
            else
            {
                dhs_correlationId = correlationId;
            }
            
            string strReq = JsonConvert.SerializeObject(patientClaimInteractiveRequestType);

            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.Request = strReq;
            requestLog.TenantId = TenantId;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;

            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                //_logger.LogInformation("in patient claim");
                HttpClient apiClient = new HttpClient();

                PCIGeneral patientClaim = new PCIGeneral(apiClient);
               
                Task<PatientClaimInteractiveResponseType> taskResponse = patientClaim.McpPatientClaimInteractiveGeneral
                    (patientClaimInteractiveRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, endPoint);
                taskResponse.Wait();
                 PatientClaimInteractiveResponseType patientClaimInteractiveResponseType = taskResponse.Result;
                string status = "";
                string response = "";
                string claimId = "";
                if (patientClaimInteractiveResponseType != null)
                {
                    response = JsonConvert.SerializeObject(patientClaimInteractiveResponseType);
                    status = patientClaimInteractiveResponseType.Status;

                    if (patientClaimInteractiveResponseType.ClaimAssessment != null)
                        claimId = patientClaimInteractiveResponseType.ClaimAssessment.ClaimId;
                    int iRows = MedicareRequestLogHelper.updateResponse(dhs_messageId, "COMPLETED", response, claimId);
                    
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = patientClaimInteractiveResponseType;
                return apiResponse;
                //return response;
            }            
            catch(Exception e)
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
                    apiResponse.ErrorCode = 98;
                }
                else if (typeof(SqlException) == e.InnerException.GetType())
                {
                    SqlException sqlExp = (SqlException)e.InnerException;
                    resp = sqlExp.Message;
                    apiResponse.StatusCode = StatusCodes.Status500InternalServerError;
                    apiResponse.ErrorCode = 98;

                }
                else
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.ErrorCode = 98;
                    resp = JsonConvert.SerializeObject(e.Message);
                }

                if (!Guid.Empty.Equals(dhs_messageId))
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                apiResponse.Result = null;
                apiResponse.ErrorCode = 98;
                apiResponse.Errors.Add(resp );
                apiResponse.TransactionId = dhs_correlationId;
                // return resp;
                return apiResponse;

            }

        }   

      
    }
}
