//----------------------
// <auto-generated>
//     Generated using the NSwag toolchain v13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0)) (http://NSwag.org)
// </auto-generated>
//----------------------

#pragma warning disable 108 // Disable "CS0108 '{derivedDto}.ToJson()' hides inherited member '{dtoBase}.ToJson()'. Use the new keyword if hiding was intended."
#pragma warning disable 114 // Disable "CS0114 '{derivedDto}.RaisePropertyChanged(String)' hides inherited member 'dtoBase.RaisePropertyChanged(String)'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword."
#pragma warning disable 472 // Disable "CS0472 The result of the expression is always 'false' since a value of type 'Int32' is never equal to 'null' of type 'Int32?'
#pragma warning disable 1573 // Disable "CS1573 Parameter '...' has no matching param tag in the XML comment for ...
#pragma warning disable 1591 // Disable "CS1591 Missing XML comment for publicly visible type or member ..."

using Newtonsoft.Json.Linq;
using System;

namespace Capstone.Common.Medicare.ThirdParty.MedicareOnline.SDD
{
    [System.CodeDom.Compiler.GeneratedCode("NSwag", "13.0.5.0 (NJsonSchema v10.0.22.0 (Newtonsoft.Json v11.0.0.0))")]
    public partial class SameDayDelete 
    {
        private string _baseUrl = Medicare.Properties.Resources.BaseURL;
        private System.Net.Http.HttpClient _httpClient;
        private System.Lazy<Newtonsoft.Json.JsonSerializerSettings> _settings;
    
        public SameDayDelete(System.Net.Http.HttpClient httpClient)
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
        
        public System.Threading.Tasks.Task<SameDayDeleteResponseType> McpSameDayDelete(SameDayDeleteRequestType body, string authorization, string dhs_auditId, string dhs_subjectId, string dhs_messageId, string dhs_auditIdType, string dhs_correlationId, string dhs_productId, string dhs_subjectIdType,string apiKey)
        {
            return McpSameDayDelete(body, authorization, dhs_auditId, dhs_subjectId, dhs_messageId, dhs_auditIdType, dhs_correlationId, dhs_productId, dhs_subjectIdType,apiKey, System.Threading.CancellationToken.None);
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
        public async System.Threading.Tasks.Task<SameDayDeleteResponseType> McpSameDayDelete(SameDayDeleteRequestType body, string authorization, string dhs_auditId, string dhs_subjectId, string dhs_messageId, string dhs_auditIdType, string dhs_correlationId, string dhs_productId, string dhs_subjectIdType, string apiKey,System.Threading.CancellationToken cancellationToken)
        {
            var urlBuilder_ = new System.Text.StringBuilder();
            urlBuilder_.Append(BaseUrl != null ? BaseUrl.TrimEnd('/') : "").Append("/mcp/samedaydelete/v1");
    
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


                    //Header missed out
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
                            var objectResponse_ = await ReadObjectResponseAsync<SameDayDeleteResponseType>(response_, headers_).ConfigureAwait(false);
                            return objectResponse_.Object;
                        }
                        else
                        if (status_ == "400")
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var objectResponse_ = await ReadObjectResponseAsync<ServiceMessagesType>(response_, headers_).ConfigureAwait(false);
                            if (!(objectResponse_.Object is null) && objectResponse_.Object.ServiceMessage.Count == 0)
                            {
                                throw new ApiException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", (int)response_.StatusCode, responseData_, headers_, null);

                            }
                            else
                            {
                                throw new ApiException<ServiceMessagesType>("server cannot or will not process the request", (int)response_.StatusCode, objectResponse_.Text, headers_, objectResponse_.Object, null);

                            }
                        }
                        else
                        if (status_ != "200" && status_ != "204")
                        {
                            var responseData_ = response_.Content == null ? null : await response_.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new ApiException("The HTTP status code of the response was not expected (" + (int)response_.StatusCode + ").", (int)response_.StatusCode, responseData_, headers_, null);
                        }
            
                        return default(SameDayDeleteResponseType);
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
    public partial class SameDayDeleteRequestType 
    {
        [Newtonsoft.Json.JsonProperty("sameDayDelete", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public SameDayDeleteType SameDayDelete { get; set; } = new SameDayDeleteType();
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class SameDayDeleteType 
    {
        [Newtonsoft.Json.JsonProperty("patient", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public MedicarePatientType Patient { get; set; } = new MedicarePatientType();
    
        [Newtonsoft.Json.JsonProperty("reasonCode", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.StringLength(3, MinimumLength = 3)]
        public string ReasonCode { get; set; }
    
    
    }
    
    //[Newtonsoft.Json.JsonConverter(typeof(JsonInheritanceConverter), "medicarePatientType")]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class MedicarePatientType 
    {
        [Newtonsoft.Json.JsonProperty("identity", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public IdentityType Identity { get; set; } = new IdentityType();
    
        [Newtonsoft.Json.JsonProperty("medicare", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required]
        public MembershipType Medicare { get; set; } = new MembershipType();
    
    
    }
    
    //[Newtonsoft.Json.JsonConverter(typeof(JsonInheritanceConverter), "identityType")]
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class IdentityType 
    {
        [Newtonsoft.Json.JsonProperty("dateOfBirth", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [Newtonsoft.Json.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? DateOfBirth { get; set; }
    
        [Newtonsoft.Json.JsonProperty("familyName", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string FamilyName { get; set; }
    
        [Newtonsoft.Json.JsonProperty("givenName", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string GivenName { get; set; }
    
        [Newtonsoft.Json.JsonProperty("secondInitial", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(1, MinimumLength = 1)]
        public string SecondInitial { get; set; }
    
        [Newtonsoft.Json.JsonProperty("sex", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(1, MinimumLength = 1)]
        public string Sex { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class MembershipType 
    {
        [Newtonsoft.Json.JsonProperty("memberNumber", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(10, MinimumLength = 10)]
        public string MemberNumber { get; set; }
    
        [Newtonsoft.Json.JsonProperty("memberRefNumber", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.StringLength(1, MinimumLength = 1)]
        public string MemberRefNumber { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class SameDayDeleteResponseType 
    {
        [Newtonsoft.Json.JsonProperty("status", Required = Newtonsoft.Json.Required.Always)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public string Status { get; set; }
    
    
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    public partial class ServiceMessagesType 
    {
        [Newtonsoft.Json.JsonProperty("highestSeverity", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ServiceMessagesTypeHighestSeverity HighestSeverity { get; set; }
    
        [Newtonsoft.Json.JsonProperty("serviceMessage", Required = Newtonsoft.Json.Required.Default, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
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
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal class JsonInheritanceAttribute : System.Attribute
    {
        public JsonInheritanceAttribute(string key, System.Type type)
        {
            Key = key;
            Type = type;
        }
    
        public string Key { get; }
    
        public System.Type Type { get; }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    internal class JsonInheritanceConverter : Newtonsoft.Json.JsonConverter
    {
        internal static readonly string DefaultDiscriminatorName = "discriminator";
    
        private readonly string _discriminator;
    
        [System.ThreadStatic]
        private static bool _isReading;
    
        [System.ThreadStatic]
        private static bool _isWriting;
    
        public JsonInheritanceConverter()
        {
            _discriminator = DefaultDiscriminatorName;
        }
    
        public JsonInheritanceConverter(string discriminator)
        {
            _discriminator = discriminator;
        }
    
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            try
            {
                _isWriting = true;
    
                var jObject = Newtonsoft.Json.Linq.JObject.FromObject(value, serializer);
                jObject.AddFirst(new Newtonsoft.Json.Linq.JProperty(_discriminator, GetSubtypeDiscriminator(value.GetType())));
                writer.WriteToken(jObject.CreateReader());
            }
            finally
            {
                _isWriting = false;
            }
        }
    
        public override bool CanWrite
        {
            get
            {
                if (_isWriting)
                {
                    _isWriting = false;
                    return false;
                }
                return true;
            }
        }
    
        public override bool CanRead
        {
            get
            {
                if (_isReading)
                {
                    _isReading = false;
                    return false;
                }
                return true;
            }
        }
    
        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }
    
        public override object ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<Newtonsoft.Json.Linq.JObject>(reader);
            if (jObject == null)
                return null;
    
            var discriminator = Newtonsoft.Json.Linq.Extensions.Value<string>(jObject.GetValue(_discriminator));
            var subtype = GetObjectSubtype(objectType, discriminator);
           
            var objectContract = serializer.ContractResolver.ResolveContract(subtype) as Newtonsoft.Json.Serialization.JsonObjectContract;
            if (objectContract == null || System.Linq.Enumerable.All(objectContract.Properties, p => p.PropertyName != _discriminator))
            {
                jObject.Remove(_discriminator);
            }
    
            try
            {
                _isReading = true;
                return serializer.Deserialize(jObject.CreateReader(), subtype);
            }
            finally
            {
                _isReading = false;
            }
        }
    
        private System.Type GetObjectSubtype(System.Type objectType, string discriminator)
        {
            foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
            {
                if (attribute.Key == discriminator)
                    return attribute.Type;
            }
    
            return objectType;
        }
    
        private string GetSubtypeDiscriminator(System.Type objectType)
        {
            foreach (var attribute in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), true))
            {
                if (attribute.Type == objectType)
                    return attribute.Key;
            }
    
            return objectType.Name;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.0.22.0 (Newtonsoft.Json v11.0.0.0)")]
    internal class DateFormatConverter : Newtonsoft.Json.Converters.IsoDateTimeConverter
    {
        public DateFormatConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
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