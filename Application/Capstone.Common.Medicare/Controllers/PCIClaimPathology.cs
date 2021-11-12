using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.PCI.Pathology;
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
    [ApiController]
    [Route("api/[controller]")]
    public class PCIClaimPathology : Controller
    {
        private Guid TenantId;
        private string locationId;

       
        [HttpPost]
        [Route("SubmitClaim")]
        public async Task<ApiResponse<PatientClaimInteractiveResponseType>> CallPCIPathologyAsync(string tenantId, string locationId,string correlationId)
        {
            var apiResponse = new ApiResponse<PatientClaimInteractiveResponseType>();

            try
            {
                if (!String.IsNullOrEmpty(tenantId) && !String.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();

                    PatientClaimInteractiveRequestType request = JsonConvert.DeserializeObject<PatientClaimInteractiveRequestType>(body);
                    if (request is null)
                    {
                        apiResponse.Result = null;
                        apiResponse.Errors.Add("invalid Request body");
                        return apiResponse;
                    }
                    return SubmitPCIPathology(request, correlationId);
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
        private ApiResponse<PatientClaimInteractiveResponseType> SubmitPCIPathology(PatientClaimInteractiveRequestType patientClaimInteractiveRequestType,string correlationId)
        {
            var apiResponse = new ApiResponse<PatientClaimInteractiveResponseType>();

            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId); // PRODA
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly.");
                return apiResponse;
            }
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            string authorization = "Bearer " + authToken;

            string MedicareNo = string.Empty;
            try
            {
                MedicareNo = patientClaimInteractiveRequestType.PatientClaimInteractive.Patient.Medicare.MemberNumber;
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
            string dhs_correlationId = string.Empty;
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                string corrID = MedicareUtilityHelper.GenerateCorrelationId();
                dhs_correlationId = locationId + corrID;
                dhs_correlationId = dhs_correlationId.Substring(0, 24);
            }
            else
            {
                //Claim resend
                dhs_correlationId = correlationId;
            }
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(patientClaimInteractiveRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.Request = strReq;
            requestLog.TenantId = TenantId;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "PCI - Pathology";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {
                HttpClient apiClient = new HttpClient();
                    PCIPathology patientClaimSpecialist = new PCIPathology(apiClient);
                Task<PatientClaimInteractiveResponseType> taskResponse = patientClaimSpecialist.McpPatientClaimInteractivePathology
                        (patientClaimInteractiveRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);
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
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, status, response, claimId);
                   
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = patientClaimInteractiveResponseType;
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
                apiResponse.Errors.Add(resp );
                apiResponse.TransactionId = dhs_correlationId;
                // return resp;
                return apiResponse;
            }

        }

    }
}
