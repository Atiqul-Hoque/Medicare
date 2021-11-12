﻿//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."

namespace Capstone.Common.Medicare.ThirdParty.MedicareOnline.Veteran.Payment.Report
{
    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class VeteranPaymentReport 
    {
        private string _baseUrl = Medicare.Properties.Resources.BaseURL;
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<Newtonsoft.Json.JsonSerializerSettings> _settings;
    
        public VeteranPaymentReport(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient; 
            _settings = new System.Lazy<Newtonsoft.Json.JsonSerializerSettings>(() => 
            {
                var settings = new Newtonsoft.Json.JsonSerializerSettings();
                UpdateJsonSerializerSettings(settings);
                return settings;
            });
        }
    
        public string BaseUrl 
        {
            get { return _baseUrl; }
            set { _baseUrl = value; }
        }
    
        protected Newtonsoft.Json.JsonSerializerSettings JsonSerializerSettings { get { return _settings.Value; } }
    
        partial void UpdateJsonSerializerSettings(Newtonsoft.Json.JsonSerializerSettings settings);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url);
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, System.Text.StringBuilder urlBuilder);
        partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response);
    
        /// <summary>This is the request</summary>
        /// <param name="authorization">JWT header for authorization</param>
        /// <param name="dhs_auditId">DHS Audit ID</param>
        /// <param name="dhs_subjectId">DHS Subject ID</param>
        /// <param name="dhs_messageId">DHS Message ID</param>
        /// <param name="dhs_auditIdType">DHS Audit Type</param>
        /// <param name="dhs_correlationId">DHS Correlation ID</param>
        /// <param name="dhs_productId">DHS Product ID</param>
        /// <param name="dhs_subjectIdType">DHS Subject ID Type</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public System.Threading.Tasks.Task<DVAPaymentReportResponseType> McpDvaPaymentReport(DVAReportRequestType body, string authorization, string dhs_auditId, string dhs_subjectId, string dhs_messageId, string dhs_auditIdType, string dhs_correlationId, string dhs_productId, string dhs_subjectIdType, string apiKey)
        {
            return McpDvaPaymentReport(body, authorization, dhs_auditId, dhs_subjectId, dhs_messageId, dhs_auditIdType, dhs_correlationId, dhs_productId, dhs_subjectIdType, apiKey, System.Threading.CancellationToken.None);
        }
    
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <summary>This is the request</summary>
        /// <param name="authorization">JWT header for authorization</param>
        /// <param name="dhs_auditId">DHS Audit ID</param>
        /// <param name="dhs_subjectId">DHS Subject ID</param>
        /// <param name="dhs_messageId">DHS Message ID</param>
        /// <param name="dhs_auditIdType">DHS Audit Type</param>
        /// <param name="dhs_correlationId">DHS Correlation ID</param>
        /// <param name="dhs_productId">DHS Product ID</param>
        /// <param name="dhs_subjectIdType">DHS Subject ID Type</param>
        /// <returns>successful operation</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async System.Threading.Tasks.Task<DVAPaymentReportResponseType> McpDvaPaymentReport(DVAReportRequestType body, string authorization, string dhs_auditId, string dhs_subjectId, string dhs_messageId, string dhs_auditIdType, string dhs_correlationId, string dhs_productId, string dhs_subjectIdType, string apiKey ,System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/mcp/dvapaymentreport/v1");
    
            var client_ = _httpClient;
            try
            {
                using (var request_ = new System.Net.Http.HttpRequestMessage())
                {
                    if (authorization == null)
                        throw new System.ArgumentNullException("authorization");
                    request_.Headers.TryAddWithoutValidation("Authorization", ConvertToString(authorization, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_auditId == null)
                        throw new System.ArgumentNullException("dhs_auditId");
                    request_.Headers.TryAddWithoutValidation("dhs-auditId", ConvertToString(dhs_auditId, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_subjectId == null)
                        throw new System.ArgumentNullException("dhs_subjectId");
                    request_.Headers.TryAddWithoutValidation("dhs-subjectId", ConvertToString(dhs_subjectId, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_messageId == null)
                        throw new System.ArgumentNullException("dhs_messageId");
                    request_.Headers.TryAddWithoutValidation("dhs-messageId", ConvertToString(dhs_messageId, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_auditIdType == null)
                        throw new System.ArgumentNullException("dhs_auditIdType");
                    request_.Headers.TryAddWithoutValidation("dhs-auditIdType", ConvertToString(dhs_auditIdType, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_correlationId == null)
                        throw new System.ArgumentNullException("dhs_correlationId");
                    request_.Headers.TryAddWithoutValidation("dhs-correlationId", ConvertToString(dhs_correlationId, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_productId == null)
                        throw new System.ArgumentNullException("dhs_productId");
                    request_.Headers.TryAddWithoutValidation("dhs-productId", ConvertToString(dhs_productId, System.Globalization.CultureInfo.InvariantCulture));
                    if (dhs_subjectIdType == null)
                        throw new System.ArgumentNullException("dhs_subjectIdType");
                    request_.Headers.TryAddWithoutValidation("dhs-subjectIdType", ConvertToString(dhs_subjectIdType, System.Globalization.CultureInfo.InvariantCulture));
                    //Header missing from generated file
                    if (apiKey == null)
                        throw new System.ArgumentNullException("apiKey");
                    request_.Headers.TryAddWithoutValidation("X-IBM-Client-Id", ConvertToString(apiKey, System.Globalization.CultureInfo.InvariantCulture));

                    var content_ = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body, _settings.Value));
                    content_.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("application/json");
                    request_.Content = content_;
                    request_.Method = new System.Net.Http.HttpMethod("POST");
                    request_.Headers.Accept.Add(System.Net.Http.Headers.MediaTypeWithQualityHeaderValue.Parse("application/json"));
    
                    PrepareRequest(client_, request_, urlBuilder_);
                    var url_ = urlBuilder_.ToString();
                    request_.RequestUri = new System.Uri(url_, System.UriKind.RelativeOrAbsolute);
                    PrepareRequest(client_, request_, url_);
    
                    var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    try
                    {
                        var headers_ = System.Linq.Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
                        if (response_.Content != null && response_.Content.Headers != null)
                        {
                            foreach (var item_ in response_.Content.Headers)
                                headers_[item_.Key] = item_.Value;
                        }
    
                        ProcessResponse(client_, response_);
    
                        var status_ = ((int)response_.StatusCode).ToString();
                        if (status_ == "200") 
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<DVAPaymentReportResponseType>(response_, headers_).ConfigureAwait(false);
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == "400") 
                        {
                            var objectResponse_ = await ReadObjectResponseAsync<ServiceMessagesType>(response_, headers_).ConfigureAwait(false);
                            throw new ApiException<ServiceMessagesType>("server cannot or will not process the request", (int)response_.StatusCode, objectResponse_.Text, headers_, objectResponse_.Object, null);
                        }
                        else
                        if (status_ != "200" && status_ != "204")
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false); 
                            throw new ApiException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", (int)response_.StatusCode, responseData_, headers_, null);
                        }
            
                        return default(DVAPaymentReportResponseType);
                    }
                    finally
                    {
                        if (response_ != null)
                            response_.Dispose();
                    }
                }
            }
            finally
            {
            }
        }
    
        protected struct ObjectResponseResult<T>
        {
            public ObjectResponseResult(T responseObject, string responseText)
            {
                this.Object = responseObject;
                this.Text = responseText;
            }
    
            public T Object { get; }
    
            public string Text { get; }
        }
    
        public bool ReadResponseAsString { get; set; }
        
        protected virtual async System.Threading.Tasks.Task<ObjectResponseResult<T>> ReadObjectResponseAsync<T>(System.Net.Http.HttpResponseMessage response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers)
        {
            if (response == null || response.Content == null)
            {
                return new ObjectResponseResult<T>(default(T), string.Empty);
            }
        
            if (ReadResponseAsString)
            {
                var responseText = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                try
                {
                    var typedBody = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText, JsonSerializerSettings);
                    return new ObjectResponseResult<T>(typedBody, responseText);
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body string as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, responseText, headers, exception);
                }
            }
            else
            {
                try
                {
                    using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var streamReader = new System.IO.StreamReader(responseStream))
                    using (var jsonTextReader = new Newtonsoft.Json.JsonTextReader(streamReader))
                    {
                        var serializer = Newtonsoft.Json.JsonSerializer.Create(JsonSerializerSettings);
                        var typedBody = serializer.Deserialize<T>(jsonTextReader);
                        return new ObjectResponseResult<T>(typedBody, string.Empty);
                    }
                }
                catch (Newtonsoft.Json.JsonException exception)
                {
                    var message = "Could not deserialize the response body stream as " + typeof(T).FullName + ".";
                    throw new ApiException(message, (int)response.StatusCode, string.Empty, headers, exception);
                }
            }
        }
    
        private string ConvertToString(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value is System.Enum)
            {
                string name = System.Enum.GetName(value.GetType(), value);
                if (name != null)
                {
                    var field = System.Reflection.IntrospectionExtensions.GetTypeInfo(value.GetType()).GetDeclaredField(name);
                    if (field != null)
                    {
                        var attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) 
                            as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null)
                        {
                            return attribute.Value != null ? attribute.Value : name;
                        }
                    }
                }
            }
            else if (value is bool) {
                return System.Convert.ToString(value, cultureInfo).ToLowerInvariant();
            }
            else if (value is byte[])
            {
                return System.Convert.ToBase64String((byte[]) value);
            }
            else if (value != null && value.GetType().IsArray)
            {
                var array = System.Linq.Enumerable.OfType<object>((System.Array) value);
                return string.Join(",", System.Linq.Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }
        
            return System.Convert.ToString(value, cultureInfo);
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class DVAReportRequestType 
    {
        [Newtonsoft.Json.JsonProperty("payeeProvider", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public ProviderType PayeeProvider { get; set; } = new ProviderType();
    
        [Newtonsoft.Json.JsonProperty("claimId", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(6, MinimumLength = 6)]
        public string ClaimId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("lodgementDate", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? LodgementDate { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ProviderType 
    {
        [Newtonsoft.Json.JsonProperty("providerNumber", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(8, MinimumLength = 8)]
        public string ProviderNumber { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class DVAPaymentReportResponseType 
    {
        [Newtonsoft.Json.JsonProperty("paymentRun", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public PaymentRunType PaymentRun { get; set; }
    
        [Newtonsoft.Json.JsonProperty("paymentInfo", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public PaymentType PaymentInfo { get; set; }
    
        [Newtonsoft.Json.JsonProperty("claimSummary", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.MinLength(1)]
        public System.Collections.Generic.ICollection<ClaimSummaryType> ClaimSummary { get; set; }
    
        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Status { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class PaymentRunType 
    {
        [Newtonsoft.Json.JsonProperty("payerName", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string PayerName { get; set; }
    
        [Newtonsoft.Json.JsonProperty("runDate", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? RunDate { get; set; }
    
        [Newtonsoft.Json.JsonProperty("runNumber", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string RunNumber { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class PaymentType 
    {
        [Newtonsoft.Json.JsonProperty("accountInfo", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public BankAccountType AccountInfo { get; set; } = new BankAccountType();
    
        [Newtonsoft.Json.JsonProperty("depositAmount", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string DepositAmount { get; set; }
    
        [Newtonsoft.Json.JsonProperty("paymentReference", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string PaymentReference { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class BankAccountType 
    {
        [Newtonsoft.Json.JsonProperty("accountName", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(30, MinimumLength = 1)]
        public string AccountName { get; set; }
    
        [Newtonsoft.Json.JsonProperty("accountNumber", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(9, MinimumLength = 1)]
        public string AccountNumber { get; set; }
    
        [Newtonsoft.Json.JsonProperty("bsbCode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(6, MinimumLength = 6)]
        public string BsbCode { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ClaimSummaryType 
    {
        [Newtonsoft.Json.JsonProperty("accountReferenceId", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string AccountReferenceId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("benefit", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Benefit { get; set; }
    
        [Newtonsoft.Json.JsonProperty("chargeAmount", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(9, MinimumLength = 1)]
        public string ChargeAmount { get; set; }
    
        [Newtonsoft.Json.JsonProperty("claimChannelCode", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ClaimChannelCode { get; set; }
    
        [Newtonsoft.Json.JsonProperty("claimId", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ClaimId { get; set; }
    
        [Newtonsoft.Json.JsonProperty("lodgementDate", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? LodgementDate { get; set; }
    
        [Newtonsoft.Json.JsonProperty("transactionId", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string TransactionId { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ServiceMessagesType 
    {
        [Newtonsoft.Json.JsonProperty("highestSeverity", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ServiceMessagesTypeHighestSeverity HighestSeverity { get; set; }
    
        [Newtonsoft.Json.JsonProperty("serviceMessage", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(1)]
        public System.Collections.Generic.ICollection<ServiceMessageType> ServiceMessage { get; set; } = new System.Collections.ObjectModel.Collection<ServiceMessageType>();
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ServiceMessageType 
    {
        [Newtonsoft.Json.JsonProperty("code", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Code { get; set; }
    
        [Newtonsoft.Json.JsonProperty("severity", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ServiceMessageTypeSeverity Severity { get; set; }
    
        [Newtonsoft.Json.JsonProperty("reason", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Reason { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum ServiceMessagesTypeHighestSeverity
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Fatal")]
        Fatal = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Error")]
        Error = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Warning")]
        Warning = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Informational")]
        Informational = 3,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public enum ServiceMessageTypeSeverity
    {
        [System.Runtime.Serialization.EnumMember(Value = @"Fatal")]
        Fatal = 0,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Error")]
        Error = 1,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Warning")]
        Warning = 2,
    
        [System.Runtime.Serialization.EnumMember(Value = @"Informational")]
        Informational = 3,
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    internal class DateFormatConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public DateFormatConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class ApiException : System.Exception
    {
        public int StatusCode { get; private set; }

        public string Response { get; private set; }

        public System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> Headers { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, System.Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + response.Substring(0, response.Length >= 512 ? 512 : response.Length), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class ApiException<TResult> : ApiException
    {
        public TResult Result { get; private set; }

        public ApiException(string message, int statusCode, string response, System.Collections.Generic.IReadOnlyDictionary<string, System.Collections.Generic.IEnumerable<string>> headers, TResult result, System.Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}

#pragma warning restore 1591
#pragma warning restore 1573
#pragma warning restore  472
#pragma warning restore  114
#pragma warning restore  108