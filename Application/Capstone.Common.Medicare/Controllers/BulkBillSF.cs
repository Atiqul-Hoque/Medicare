using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.BulkBill.General;
using Capstone.Common.Medicare.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Capstone.Common.Medicare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BulkBillSF : ControllerBase
    {
        Guid tenantId;
        string locationId;
        string endPoint;
        string serviceName;
        private readonly ILogger<BulkBillSF> _logger;

        public BulkBillSF(ILogger<BulkBillSF> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("CallBulkBill")]
        public async Task<ApiResponse<BulkBillStoreForwardResponseType>> CallBulkBill(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<BulkBillStoreForwardResponseType>();

            try
            {
                if (!string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(locationId))
                {
                    this.tenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    BulkBillStoreForwardRequestType bulkBillStoreForwardRequestType = JsonConvert.DeserializeObject< BulkBillStoreForwardRequestType>(body);
                   
                    return submitBulkBillGeneral(bulkBillStoreForwardRequestType);

                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("TenantId and LocationId required." );
                    
                }
                return apiResponse;
            }
            
            catch (Exception e)
            {
                apiResponse.Result = null;
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Errors.Add(e.Message );
                _logger.LogError(e, "Error in submitting BulkBill Claim. " + e.Message);
                return apiResponse;
            }
           
        }

        private ApiResponse<BulkBillStoreForwardResponseType> submitBulkBillGeneral(BulkBillStoreForwardRequestType bulkBillStoreForwardRequestType)
        {
            var apiResponse = new ApiResponse<BulkBillStoreForwardResponseType>();
            ProdaAuthentication prodaAuthentication = new ProdaAuthentication();
            bool isProdaValid = prodaAuthentication.ValidateProdaToken(tenantId.ToString(), locationId);
            if (!isProdaValid)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Proda Authentication Failed" );

            }
            
            
            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(tenantId.ToString(), locationId); // PRODA
            TenantMedicareProperty tenantMedicareProperty = authController.tenantMedicareProperty;
            if (string.IsNullOrWhiteSpace(authToken))
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Authentication Token Not generated properly.");
                _logger.LogError("Error in submitting BulkBill Claim. " + "Authentication Token Not generated properly.");


            }
            string authorization = "Bearer " + authToken;
            string MedicareNo = string.Empty;
            if(apiResponse.Errors.Count > 0)
            {
                return apiResponse;
            }
            try
            {
                apiResponse = SetEndPoint(bulkBillStoreForwardRequestType.Claim.ServiceTypeCode, apiResponse);
                if (apiResponse.Errors.Count > 0)
                {
                    return apiResponse;
                }
            }
            catch (Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add(e.Message );
            }
            try
            {
                MedicareNo = bulkBillStoreForwardRequestType.Claim.MedicalEvent.ElementAt(0).Patient.Medicare.MemberNumber;
                bool isValid = MedicareCommonValidations.MedicareValidityCheck(MedicareNo);
                if (!isValid)
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("Invalid Medicare Number.");
                    return apiResponse;
                }

               
            }
            catch(Exception e)
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add( "Invalid Medicare Number." + e.Message);
                _logger.LogError(e, "Error in submitting BulkBill Claim. " + e.Message);

            }
            //Header values
            string dhs_subjectId = MedicareNo; //Medicare Number
            string dhs_auditIdType = "Location Id";
            string dhs_subjectIdType = "Medicare Card";
            string dhs_auditId = locationId;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string apiKey = tenantMedicareProperty.Apikey;
            string corrID = MedicareUtilityHelper.GenerateCorrelationId();
            string dhs_correlationId = dhs_auditId + corrID;
            dhs_correlationId = dhs_correlationId.Substring(0, 24);
            string strReq = JsonConvert.SerializeObject(bulkBillStoreForwardRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = tenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = serviceName;
            Guid dhs_messageId = MedicareRequestLogHelper.LogRequest(requestLog);
            try
            {

                HttpClient apiClient = new HttpClient();

                BulkBillGeneral bulkBill = new BulkBillGeneral(apiClient);

                Task<BulkBillStoreForwardResponseType> taskResp = bulkBill.McpBulkBillStoreForwardGeneral(bulkBillStoreForwardRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey,endPoint);
                BulkBillStoreForwardResponseType bulkBillStoreForwardResponseType = taskResp.Result;
                string status = "";
                string response = "";
                string claimId = "";
                if (bulkBillStoreForwardResponseType != null)
                {
                    response = JsonConvert.SerializeObject(bulkBillStoreForwardResponseType);
                    status = bulkBillStoreForwardResponseType.Status;

                    claimId = bulkBillStoreForwardResponseType.ClaimId;
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, status, response, claimId);

                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = bulkBillStoreForwardResponseType;
                return apiResponse;
               // return response;

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
                {
                    resp = JsonConvert.SerializeObject(e.Message);
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    _logger.LogError(e, "Error in submitting BulkBill Claim. " + e.Message);

                }

                if (!Guid.Empty.Equals(dhs_messageId))
                    MedicareRequestLogHelper.updateResponse(dhs_messageId, "ERROR", resp);
                apiResponse.Result = null;
                apiResponse.Errors.Add(resp );
                apiResponse.TransactionId = dhs_correlationId;
                return apiResponse;
               // return resp;

            }
        }

        private ApiResponse<BulkBillStoreForwardResponseType> SetEndPoint(string serviceTypeCode, ApiResponse<BulkBillStoreForwardResponseType> apiResponse)
        {

            if (!string.IsNullOrWhiteSpace(serviceTypeCode)){
                switch (serviceTypeCode)
                {
                    case "O":
                        endPoint = "/mcp/bulkbillstoreforward/general/v1";
                        serviceName = "BBSW - General";
                        break;
                    case "S":
                        endPoint = "/mcp/bulkbillstoreforward/specialist/v1";
                        serviceName = "BBSW - Specialist";
                        break;
                    case "P":
                        endPoint = "/mcp/bulkbillstoreforward/pathology/v1";
                        serviceName = "BBSW - Pathology";
                        break;
                    default:
                        apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                        apiResponse.Result = null;
                        apiResponse.Errors.Add( "Invalid ServiceTypeCode" );
                        break;
                }
            }
            else
            {
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                apiResponse.Result = null;
                apiResponse.Errors.Add("Invalid ServiceTypeCode.");
            }
            return apiResponse;
        }
    }


}
