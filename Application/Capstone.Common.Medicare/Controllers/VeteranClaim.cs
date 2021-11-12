using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.Veteran.Claim.General;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Capstone.Common.Controllers.MedicareOnline
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeteranClaim : ControllerBase
    {
        Guid TenantId;
        string locationId;
        [HttpPost]
        [Route("CallVeteranClaimGeneral")]
        public async Task<string> CallVeteranClaimGeneral(string TenantId, string locationId)
        {
            try
            {
                if (!String.IsNullOrEmpty(TenantId) || !String.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(TenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    DVAClaimRequestType DVAClaimRequestType = JsonConvert.DeserializeObject<DVAClaimRequestType>(body);

                    return SubmitVeteranClaimGeneral(DVAClaimRequestType); ;

                }
                else
                {
                    return "{ \"statusCode\": 400,\"errorCode\": 100,\"errors\": \"TenantId and LocationId required\"}";
                }
            }
            catch(Exception e)
            {
                return e.Message;
            }
           
        }

        private string SubmitVeteranClaimGeneral(DVAClaimRequestType DVAClaimRequestType)
        {
            Authentication authController = new Authentication();
            string token = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId);

            if (String.IsNullOrEmpty(token))
            {
                return "{ \"statusCode\": 400,\"errorCode\": 7,\"errors\": \"Token Errors\"}";
            }

            string authorization = "Bearer " + token; // PRODA

            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            string serviceTypeCode = DVAClaimRequestType.Claim.ServiceTypeCode;
            string endPoint = "";
            string serviceName = "";
            if (serviceTypeCode == "O")
            {
                endPoint = "/mcp/dvaclaim/general/v1";
                serviceName = "DVAW-general";
            }
            else if (serviceTypeCode == "S")
            {
                endPoint = "/mcp/dvaclaim/specialist/v1";
                serviceName = "DVAW-specialist";
            }
            else if (serviceTypeCode == "P")
            {
                endPoint = "/mcp/dvaclaim/pathology/v1";
                serviceName = "DVAW-pathology";
            }
            else
            {
                return "{ \"statusCode\": 400,\"errorCode\": 9202,\"errors\": \"Data Errors\"}";
            }
            string dhs_auditIdType = "Location Id";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;

            //TODO : Need to sure whether Veteran number require or not for dhs_subjectId
            string VeteranNo = DVAClaimRequestType.Claim.MedicalEvent.ElementAt(0).Patient.VeteranMembership.VeteranNumber;

            string dhs_subjectId = "N/A"; //Veteran Number
            string dhs_subjectIdType = "N/A";

            if (!String.IsNullOrEmpty(VeteranNo))
            {
                dhs_subjectId = VeteranNo;
                dhs_subjectIdType = "Veteran File Number";
            }
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(DVAClaimRequestType);

            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;

            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);

            try
            {
                HttpClient apiClient = new HttpClient();

                VeteranClaimGeneral veteranClaim = new VeteranClaimGeneral(apiClient);

                Task<DVAClaimResponseType> taskResp = veteranClaim.McpDvaClaimGeneral(DVAClaimRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, endPoint);
                DVAClaimResponseType DVAClaimResponseType = taskResp.Result;
                string status = "";
                string response = "";
                string claimId = "";
                if (DVAClaimResponseType != null)
                {
                    response = JsonConvert.SerializeObject(DVAClaimResponseType);
                    status = DVAClaimResponseType.Status;
                    claimId = DVAClaimResponseType.ClaimId;

                    requestLog.Response = response;
                    requestLog.Status = status;
                    requestLog.ClaimId = claimId;
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
                return resp;

            }
        }
    
   
   
    }


}
