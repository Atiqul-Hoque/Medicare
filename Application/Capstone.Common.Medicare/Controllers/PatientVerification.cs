using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Capstone.Common.ThirdParty.MedicareOnline;
using Newtonsoft.Json;
using System.IO;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Validation;

namespace Capstone.Common.Controllers.MedicareOnline
{

    [Route("api/[controller]")]
    [ApiController]
    
    public class PatientVerification : ControllerBase
    {
        Guid TenantId;
        string locationId;
        [HttpPost]
        public async Task<string> CallPatientVerification(string TenantId, string locationId)
        {
            if (String.IsNullOrEmpty(TenantId) || String.IsNullOrEmpty(locationId))
            {
                return "{ \"statusCode\": 400,\"errorCode\": 100,\"errors\": \"TenantId and LocationId required\"}";
            }
            else
            {
                try
                {
                    this.TenantId = Guid.Parse(TenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    PatientVerificationRequestType patientVerificationRequestType = JsonConvert.DeserializeObject<PatientVerificationRequestType>(body);
                    return IsMedicareValid(patientVerificationRequestType);
                }
                catch (Exception e)
                {
                    return "{ \"statusCode\": 400,\"errorCode\": 99,\"errors\": \""+e.Message+"\"}"; 
                }
            }
 
        }

        private string IsMedicareValid(PatientVerificationRequestType patientVerificationRequestType)
        {
            Authentication authController = new Authentication();
            string token = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId);

            if (String.IsNullOrEmpty(token))
            {
             return "{ \"statusCode\": 400,\"errorCode\": 7,\"errors\": \"Token Errors\"}";
            }

            string authorization = "Bearer " + token; // PRODA

            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            string typeCode = patientVerificationRequestType.TypeCode;
            
            string endpoint = "";
            string serviceName = "";

            if (typeCode == "OPV")
            {
                endpoint = "/mcp/patientverification/v1";
                serviceName = "OPV";
            }

            else if (typeCode == "PVM")
            {
                endpoint = "/mcp/patientverification/medicare/v1";
                serviceName = "PVM";
            }

           else if (typeCode == "PVF")
            {
                endpoint = "/mcp/patientverification/hf/v1";
                serviceName = "PVF";
            }
            else
            {
            
                return "{ \"statusCode\": 400,\"errorCode\": 9202,\"errors\": \"Data Errors\"}";
            }

            var httpClient = new System.Net.Http.HttpClient();
            var apiClient = new Verification(httpClient);
            string dhs_subjectId = "N/A";
            string dhs_subjectIdType = "N/A";
            if (patientVerificationRequestType.Patient.Medicare is Object)
            {
                bool valid = MedicareCommonValidations.MedicareValidityCheck(patientVerificationRequestType.Patient.Medicare.MemberNumber);
                if (valid)
                {
                    dhs_subjectId = patientVerificationRequestType.Patient.Medicare.MemberNumber;
                    dhs_subjectIdType = "Medicare Card";
                }
                else
                {
                    return "{ \"statusCode\": 400,\"errorCode\": 9202,\"errors\": \"Invalid Medicare Number\"}";
                }
            }
            else if (patientVerificationRequestType.Patient.HealthFund is Object)
            {
                dhs_subjectId = patientVerificationRequestType.Patient.HealthFund.MemberNumber;
                dhs_subjectIdType = "Health Fund Member Number";
            }
            else
            {
                return "{ \"statusCode\": 400,\"errorCode\": 9202,\"errors\": \"Data Errors\"}";
            }
            string dhs_auditId = locationId;
            string dhs_auditIdType = "Location Id";

            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string strReq = JsonConvert.SerializeObject(patientVerificationRequestType);
           
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;
           
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);

            try
            {
                string status = "";
                string response = "";
                PatientVerificationResponseType patientVerificationResponseType = apiClient.McpPatientVerification(patientVerificationRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, endpoint).Result;
                if (patientVerificationResponseType != null)
                {
                    response = JsonConvert.SerializeObject(patientVerificationResponseType);
                    if (patientVerificationResponseType.MedicareStatus is Object && patientVerificationResponseType.HealthFundStatus is Object)
                    {
                        status = patientVerificationResponseType.MedicareStatus.Status.Code.Value.ToString() + ',' + patientVerificationResponseType.HealthFundStatus.Status.Code.Value.ToString();
                    }
                    else if (patientVerificationResponseType.MedicareStatus is Object)
                    {
                        status = patientVerificationResponseType.MedicareStatus.Status.Code.Value.ToString();
                    }
                    else if (patientVerificationResponseType.HealthFundStatus is Object)
                    {
                        status = patientVerificationResponseType.HealthFundStatus.Status.Code.Value.ToString();
                    }
                    requestLog.Response = response;
                    requestLog.Status = status;
                    MedicareRequestLogHelper.updateResponse(requestLog);
                }
                return response;
            }

            catch (Exception e)
            {
                string resp = e.Message;
                if (typeof(ApiException<ServiceMessagesType>) == e.InnerException.GetType())
                {
                    ApiException<ServiceMessagesType> exp = (ApiException<ServiceMessagesType>)e.InnerException;
                    resp = JsonConvert.SerializeObject(exp.Result);

                }
                else if (typeof(ApiException) == e.InnerException.GetType())
                {
                    ApiException exp = (ApiException)e.InnerException;
                    resp = exp.Response;

                }

                else
                    resp = JsonConvert.SerializeObject(e.Message);

                MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                return "{ \"statusCode\": 400,\"errorCode\": 98,\"errors\":" + resp + "}";

            }

        }


    }
}
