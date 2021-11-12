using Capstone.Common.Controllers.MedicareOnline;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Capstone.Common.Medicare.ThirdParty.MedicareOnline.SDD;
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
    public class SameDayDeleteForClaim : Controller
    {
        Guid TenantId;
        string locationId;

        [HttpPost]
        public async Task<ApiResponse<SameDayDeleteResponseType>> CallSameDayDelete(string tenantId, string locationId, string correlationId)
        {
            var apiResponse = new ApiResponse<SameDayDeleteResponseType>();

            try
            {
                if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(locationId))
                {
                    this.TenantId = Guid.Parse(tenantId);
                    this.locationId = locationId;
                    using var reader = new StreamReader(HttpContext.Request.Body);
                    var body = await reader.ReadToEndAsync();
                    SameDayDeleteRequestType sameDayDeleteRequestType = JsonConvert.DeserializeObject<SameDayDeleteRequestType>(body);
                    return SubmitForSameDayDelete(correlationId,sameDayDeleteRequestType );

                }
                else
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("TenantId and LocationId required.");

                }
                if (string.IsNullOrWhiteSpace(correlationId))
                {
                    apiResponse.Result = null;
                    apiResponse.Errors.Add("Transaction Id is required.");
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

        /*
                   * The Reason Code must have one of 
                          the following values.
                          001, 002, 003, 004, 005, 006, 007, 
                          where:
                          001 - Incorrect Patient Selection
                          002 - Incorrect Provider Details
                          003 - Incorrect Date of Service
                          004 - Incorrect Item Number 
                          Claimed
                          005 - Omitted Text on Original Claim
                          006 - Incorrect Payment Type (i.e.
                          Paid / Unpaid)
                          007 – Other
                  */




        private ApiResponse<SameDayDeleteResponseType> SubmitForSameDayDelete(string dhs_correlationId, SameDayDeleteRequestType sameDayDeleteRequestType)
        {
            var apiResponse = new ApiResponse<SameDayDeleteResponseType>();

            Authentication authController = new Authentication();
            string authToken = authController.GetAuthorisationCodeUsingAzure(TenantId.ToString(), locationId); // PRODA
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
                MedicareNo = sameDayDeleteRequestType.SameDayDelete.Patient.Medicare.MemberNumber;
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
            //string dhs_messageId = "urn:uuid:" + getNextMessageId();
            string strReq = JsonConvert.SerializeObject(sameDayDeleteRequestType);
            MedicareRequestLog requestLog = new MedicareRequestLog();
            requestLog.TenantId = TenantId;
            requestLog.Request = strReq;
            requestLog.CorrelationId = dhs_correlationId;
            requestLog.ServiceName = "SDDW";


            Guid dhs_messageId  = MedicareRequestLogHelper.LogRequest(requestLog);
            HttpClient apiClient = new HttpClient();
            try
            {
                SameDayDelete sameDayDelete = new SameDayDelete(apiClient);
                Task<SameDayDeleteResponseType> taskResponse = sameDayDelete.McpSameDayDelete
                    (sameDayDeleteRequestType, authorization, dhs_auditId, dhs_subjectId, "urn:uuid:" + dhs_messageId, dhs_auditIdType, "urn:uuid:" + dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey);

                SameDayDeleteResponseType sameDayDeleteResponseType = taskResponse.Result;

                string status = "";
                string response = "";
               
                if (sameDayDeleteResponseType != null)
                {
                    response = JsonConvert.SerializeObject(sameDayDeleteResponseType);
                    status = sameDayDeleteResponseType.Status;
                    int iRows = MedicareRequestLogHelper.updateResponse(dhs_messageId, status, response);
                }
                apiResponse.StatusCode = StatusCodes.Status200OK;
                apiResponse.TransactionId = dhs_correlationId;
                apiResponse.Result = sameDayDeleteResponseType;
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
        public static string getNextMessageId()
        {
            return Guid.NewGuid().ToString();

        }
    }
}
