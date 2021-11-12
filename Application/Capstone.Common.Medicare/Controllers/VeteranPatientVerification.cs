using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Capstone.Common.Controllers.MedicareOnline;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.Veteran.Verification;
using System.IO;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Helpers;

namespace Capstone.Common.Controllers.MedicareOnline
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class VeteranPatientVerification : ControllerBase
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
                    VeteranVerificationRequestType veteranVerificationRequestType = JsonConvert.DeserializeObject<VeteranVerificationRequestType>(body);
                    return IsVeteranValid(veteranVerificationRequestType);

                }
                catch (Exception e)
                {
                    return "{ \"statusCode\": 400,\"errorCode\": 99,\"errors\": \"" + e.Message + "\"}";
                }

            }
        }

        private string IsVeteranValid(VeteranVerificationRequestType veteranVerificationRequestType)
        {
            Authentication authController = new Authentication();
            string token = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId);

            if (String.IsNullOrEmpty(token))
            {
                return "{ \"statusCode\": 400,\"errorCode\": 7,\"errors\": \"Token Errors\"}";
            }

            string authorization = "Bearer " + token; // PRODA

            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            
            var httpClient = new System.Net.Http.HttpClient();
            VeteranVerification apiClient = new VeteranVerification(httpClient);
            string dhs_subjectId = "N/A";
            string dhs_subjectIdType = "N/A";
            if (veteranVerificationRequestType.Patient.VeteranMembership is Object)
            {
                dhs_subjectId = veteranVerificationRequestType.Patient.VeteranMembership.VeteranNumber;
                dhs_subjectIdType = "Veteran File Number";
            }

            string dhs_auditId = locationId;
            string dhs_auditIdType = "Location Id";
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string strReq = JsonConvert.SerializeObject(veteranVerificationRequestType);
           
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "OVVW";
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);

            try
            {
                string status = "";
                string response = "";
              VeteranVerificationResponseType veteranVerificationResponseType = apiClient.McpVeteranVerification(veteranVerificationRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey).Result;
                if (veteranVerificationResponseType != null)
                {
                    response = JsonConvert.SerializeObject(veteranVerificationResponseType);
                    if (veteranVerificationResponseType.VeteranStatus is Object)
                    {
                        status = veteranVerificationResponseType.VeteranStatus.Status.Code.Value.ToString();
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
