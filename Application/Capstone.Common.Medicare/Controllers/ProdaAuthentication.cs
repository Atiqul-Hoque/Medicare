using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Capstone.Common.Medicare;
using Capstone.Common.Medicare.Helpers;
using Capstone.Common.Medicare.Models;
using Capstone.Common.Medicare.Models.Responses;
using Jose;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Capstone.Common.Controllers.MedicareOnline
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdaAuthentication : ControllerBase
    {
        private TenantAzureProperty azureProperty;
        private ProdaTokenProperty prodaTokenProperty;
        private TenantMedicareProperty tenantMedicareProperty;
        [HttpPut]
        [Route("deviceActivation")]
        public ApiResponse<dynamic> DeviceActivation(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            try
            {
                PopulateTenantMedicareProperties(tenantId, locationId);
                if(tenantMedicareProperty is null)
                {
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    apiResponse.Errors.Add("Medicare properties not loaded properly.");
                    return apiResponse;
                }
                PopulateProdaTokenProperties(tenantId, locationId);

                byte[] cert = CreateRSAKeys(tenantId, locationId);
                if (cert is null)
                {
                    apiResponse.Errors.Add("Device Not activated.Certificate creation failed.");
                    return apiResponse;

                }



                string publicKeyJWT = CreatePublicKeyJWTFromCert(cert);
                apiResponse = GenerateActivationRequest(publicKeyJWT, tenantId, locationId).Result;
                if (!(apiResponse.Result is null))
                {
                    JObject jresult = JObject.Parse(apiResponse.Result);
                    string status = (string)jresult["deviceStatus"];
                    if (!string.IsNullOrWhiteSpace(status) && "ACTIVE".Equals(status.ToUpper()))
                    {
                        var apiResp = StoreCertificate(cert, tenantId, locationId);
                        if (apiResp.StatusCode == StatusCodes.Status200OK)
                        {
                            return apiResponse;
                        }
                        else
                        {
                            apiResponse.Errors = apiResp.Errors;
                            apiResponse.StatusCode = apiResp.StatusCode;
                        }
                        
                       
                    }
                   

                }
                apiResponse.Errors.Add("Device Cannot be Activated.Check the logs for more information");
                return apiResponse;
            }
            catch(Exception e)
            {
                apiResponse.Errors.Add(e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
        }

        private void PopulateTenantMedicareProperties(string tenantId, string locationId)
        {
            using (TenantDBContext tCtx = new TenantDBContext())
            {
                this.tenantMedicareProperty = tCtx.TenantMedicareProperties.Find(Guid.Parse(tenantId), locationId);
            }
        }

        [HttpPut]
        [Route("keyRefresh")]
        public ApiResponse<dynamic> KeyRefresh(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            try
            {
                PopulateTenantMedicareProperties(tenantId, locationId);
                apiResponse = GetAuthorisationCodeForProda(tenantId, locationId);
                string token = string.Empty;
                if (apiResponse.StatusCode == StatusCodes.Status200OK && !(apiResponse.Result is null))
                {
                    JObject jresult = JObject.Parse(apiResponse.Result);
                    token = (string)jresult["access_token"];
                }
                else
                {
                    apiResponse.Errors.Add("Auth code not successful.");
                    return apiResponse;
                }
                byte[] cert = CreateRSAKeys(tenantId, locationId);
                string publicKeyJWT = CreatePublicKeyJWTFromCert(cert);
                var jsonKey = JObject.Parse(publicKeyJWT);
                string keyToken = jsonKey["key"].ToString();
                apiResponse = GenerateKeyRefreshRequest(keyToken, tenantId, locationId, token).Result;
                if (apiResponse.StatusCode == StatusCodes.Status200OK && !(apiResponse.Result is null))
                {
                   var apiResp = StoreCertificate(cert, tenantId, locationId);
                    if (apiResp.StatusCode == StatusCodes.Status200OK)
                    {
                        return apiResponse;
                    }
                    else
                    {
                        apiResponse.Errors = apiResp.Errors;
                        apiResponse.StatusCode = apiResp.StatusCode;
                        apiResponse.Errors.Add("Store certificate not successful.");
                    }
                   
                }
               
                apiResponse.Errors.Add("Key Reresh is not successful.");
                return apiResponse;
            }
            catch(Exception e)
            {
                apiResponse.Errors.Add("Key Refresh not Successful. "+e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }


        }

        
        private async Task<ApiResponse<dynamic>> GenerateKeyRefreshRequest(string publicKey, string tenantId,string locationId,string token)
        {
            var apiResponse = new ApiResponse<dynamic>();
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
            HttpClient client = new HttpClient(handler);
            string deviceName = tenantMedicareProperty.DeviceName;
            string prodaRA = tenantMedicareProperty.ProdaOrgRa;

            string baseUrl = string.Format(Medicare.Properties.Resources.DeviceKeyRefreshUrl, prodaRA, deviceName); 
            string dhs_auditIdType = Medicare.Properties.Resources.Proda_Audit_Type;
            string dhs_subjectId = deviceName;// tenantMedicareProperty.DeviceName;
            string dhs_productId = tenantMedicareProperty.ApplicationName;
            string dhs_auditId = prodaRA;// tenantMedicareProperty.ProdaOrgRa;
            string corrId = System.Guid.NewGuid().ToString();
            string dhs_correlationId = "uuid:" + corrId;
            Guid dhs_messageId = Guid.Empty;
            string dhs_subjectIdType = Medicare.Properties.Resources.Proda_Subject_Type;
            try
            {
                ProdaRequestLog reguestLog = new ProdaRequestLog();
                reguestLog.Correlationid = Guid.Parse(corrId);
                reguestLog.TenantId = Guid.Parse(tenantId);
                reguestLog.LocationId = locationId;
                reguestLog.Request = publicKey;
                dhs_messageId = ProdaRequestLogHelper.CreateProdaRequestLog(reguestLog);
                
                Uri reqUrl = new Uri(baseUrl);
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("dhs-auditIdType", dhs_auditIdType);
                client.DefaultRequestHeaders.Add("dhs-subjectId", dhs_subjectId);
                client.DefaultRequestHeaders.Add("dhs-productId", dhs_productId);
                client.DefaultRequestHeaders.Add("dhs-auditId", dhs_auditId);
                client.DefaultRequestHeaders.Add("dhs-messageId", "urn:uuid:" + dhs_messageId);
                client.DefaultRequestHeaders.Add("dhs-correlationId", dhs_correlationId);
                client.DefaultRequestHeaders.Add("dhs-subjectIdType", dhs_subjectIdType);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + token);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer " + token);
                System.Net.Http.StringContent data = new System.Net.Http.StringContent(publicKey, Encoding.UTF8, "application/json");

                Task<HttpResponseMessage> taskResp = client.PutAsync(reqUrl, data);
                taskResp.Wait();
                string result = await taskResp.Result.Content.ReadAsStringAsync();
                JObject jresult = JObject.Parse(result);
                bool success = false;
                apiResponse.Result = result;
                apiResponse.StatusCode = (int)taskResp.Result.StatusCode;
                if (taskResp.Result.IsSuccessStatusCode)
                {
                    DateTime keyExpiry = (DateTime)jresult["keyExpiry"];
                    keyExpiry  = keyExpiry.ToUniversalTime().AddDays(-4);
                    DateTime deviceExpiry = (DateTime)jresult["deviceExpiry"];
                    deviceExpiry = deviceExpiry.ToUniversalTime().AddDays(-10);
                    if (prodaTokenProperty is null)
                    {  
                        prodaTokenProperty = new ProdaTokenProperty();
                        prodaTokenProperty.TenantId = Guid.Parse(tenantId);
                        prodaTokenProperty.LocationId = locationId;
                        prodaTokenProperty.DeviceName = (string)jresult["deviceName"];
                        prodaTokenProperty.DeviceExpiry = deviceExpiry;
                        prodaTokenProperty.KeyExpiry = keyExpiry;
                        //prodaTokenProperty.AccessToken = string.Empty;
                       // prodaTokenProperty.TokenExpiry = null;
                        ProdaTokenPropertyHelper.CreateProdaTokenProperty(prodaTokenProperty);
                    }
                    else
                    {
                        prodaTokenProperty.DeviceName = (string)jresult["deviceName"];
                        prodaTokenProperty.DeviceExpiry = deviceExpiry;
                        prodaTokenProperty.KeyExpiry = keyExpiry;
                       // prodaTokenProperty.AccessToken = string.Empty;
                       // prodaTokenProperty.TokenExpiry = null;
                        ProdaTokenPropertyHelper.updateProdaTokenProperty(prodaTokenProperty);
                    }
                    success= true;
                    apiResponse.StatusCode = StatusCodes.Status200OK;
                }
                
                client.Dispose();
                ProdaRequestLogHelper.UpdateProdaRequestLog(dhs_messageId, result);
                return apiResponse;
            }
            catch(Exception e)
            {
                ProdaRequestLogHelper.UpdateProdaRequestLog(dhs_messageId, e.Message.ToString());
                apiResponse.Errors.Add(e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
            
        }

       

        private async Task<ApiResponse<dynamic>> GenerateActivationRequest(string publicKey,string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
            HttpClient client = new HttpClient(handler);
            
           
            string deviceName = tenantMedicareProperty.DeviceName;
            string dhs_auditIdType = Medicare.Properties.Resources.Proda_Audit_Type;
            string dhs_subjectId = tenantMedicareProperty.DeviceName;//Medicare.Properties.Resources.DeviceName;
            string dhs_productId = tenantMedicareProperty.ApplicationName;//Medicare.Properties.Resources.ApplicationName;
            string dhs_auditId = tenantMedicareProperty.ProdaOrgRa;//Medicare.Properties.Resources.ProdaOrgRA;
            string corrId = System.Guid.NewGuid().ToString();
            string dhs_correlationId = "uuid:" + corrId;
            string dhs_subjectIdType = Medicare.Properties.Resources.Proda_Subject_Type;
            string endpoint = deviceName + "/jwk";
            string baseUrl = Medicare.Properties.Resources.DeviceActivationUrl+"/"+endpoint;
            Guid dhs_messageId = Guid.Empty;
            try
            {
                ProdaRequestLog reguestLog = new ProdaRequestLog();
                reguestLog.Correlationid = Guid.Parse(corrId);
                reguestLog.TenantId = Guid.Parse(tenantId);
                reguestLog.LocationId = locationId;
                reguestLog.Request = publicKey;
                dhs_messageId = ProdaRequestLogHelper.CreateProdaRequestLog(reguestLog);

                Uri reqUrl = new Uri(baseUrl);
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Add("dhs-auditIdType", dhs_auditIdType);
                client.DefaultRequestHeaders.Add("dhs-subjectId", dhs_subjectId);
                client.DefaultRequestHeaders.Add("dhs-productId", dhs_productId);
                client.DefaultRequestHeaders.Add("dhs-auditId", dhs_auditId);
                client.DefaultRequestHeaders.Add("dhs-messageId", "urn:uuid:"+dhs_messageId);
                client.DefaultRequestHeaders.Add("dhs-correlationId", dhs_correlationId);
                client.DefaultRequestHeaders.Add("dhs-subjectIdType", dhs_subjectIdType);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                System.Net.Http.StringContent data = new System.Net.Http.StringContent(publicKey, Encoding.UTF8, "application/json");
           
                Task<HttpResponseMessage> taskResp =  client.PutAsync(reqUrl, data);
                taskResp.Wait();
                string result = await taskResp.Result.Content.ReadAsStringAsync();
                
                JObject jresult = JObject.Parse(result);
                apiResponse.Result = result;
                if (taskResp.Result.IsSuccessStatusCode)
                {
                    apiResponse.StatusCode = StatusCodes.Status200OK;
                    DateTime keyExpiry = (DateTime)jresult["keyExpiry"];
                    keyExpiry = keyExpiry.ToUniversalTime().AddDays(-4);
                    DateTime deviceExpiry = (DateTime)jresult["deviceExpiry"];
                    deviceExpiry = deviceExpiry.ToUniversalTime().AddDays(-10);
                    if (prodaTokenProperty is null)          
                    {
                        prodaTokenProperty = new ProdaTokenProperty();
                        prodaTokenProperty.TenantId = Guid.Parse(tenantId);
                        prodaTokenProperty.LocationId = locationId;
                        prodaTokenProperty.DeviceName = (string)jresult["deviceName"];
                        prodaTokenProperty.DeviceExpiry = (DateTime?)deviceExpiry;
                        prodaTokenProperty.KeyExpiry = (DateTime?)keyExpiry;
                        prodaTokenProperty.AccessToken = string.Empty;
                        prodaTokenProperty.TokenExpiry = null;
                        ProdaTokenPropertyHelper.CreateProdaTokenProperty(prodaTokenProperty);
                    }
                    else
                    {
                        prodaTokenProperty.DeviceName = (string)jresult["deviceName"];                                    
                        prodaTokenProperty.DeviceExpiry = (DateTime?)deviceExpiry;
                        prodaTokenProperty.KeyExpiry = (DateTime?)keyExpiry;
                        prodaTokenProperty.AccessToken = string.Empty;
                        prodaTokenProperty.TokenExpiry = null;
                        ProdaTokenPropertyHelper.updateProdaTokenProperty(prodaTokenProperty);
                    }
                }

                client.Dispose();
                ProdaRequestLogHelper.UpdateProdaRequestLog(dhs_messageId, result);
                return apiResponse;
            }
            catch (Exception e)
            {
                apiResponse.Errors.Add(e.Message);
                ProdaRequestLogHelper.UpdateProdaRequestLog(dhs_messageId, e.Message.ToString());
                return apiResponse;
            }
            
        }
      


      
       
        private ApiResponse<dynamic> GetAuthorisationCodeForProda(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            string token = string.Empty;
            try
            {
                using(TenantDBContext tCtx = new TenantDBContext())
                {
                    this.prodaTokenProperty = tCtx.ProdaTokenProperties.Find(Guid.Parse(tenantId), locationId);
                    //if(this.prodaTokenProperty!=null && prodaTokenProperty.KeyExpiry < DateTime.Now)
                    //{
                        if(prodaTokenProperty is null || (string.IsNullOrWhiteSpace(prodaTokenProperty.AccessToken)  || (prodaTokenProperty.TokenExpiry is null) || prodaTokenProperty.TokenExpiry < DateTime.UtcNow))
                        {
                            if (this.tenantMedicareProperty is null)
                            {
                                PopulateTenantMedicareProperties(tenantId, locationId);
                            }
                            apiResponse = AuthenticationWithProdaAsync(tenantId, locationId).Result;
                       
                        }
                        else
                        {
                            JObject jresult = new JObject();
                            jresult["access_token"] = prodaTokenProperty.AccessToken;
                            apiResponse.Result = jresult.ToString();
                            apiResponse.StatusCode = StatusCodes.Status200OK;

                        }
                    
                    return apiResponse;
                }
                
              

            }
            catch (Exception e)
            {
                apiResponse.Errors.Add(e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }


        }
        private async Task<ApiResponse<dynamic>> AuthenticationWithProdaAsync(string tenantId, string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
            HttpClient authClient = new HttpClient(handler);
            string authUrl = Medicare.Properties.Resources.AuthEndPoint;
            authClient.BaseAddress = new Uri(authUrl);
            authClient.DefaultRequestHeaders.Accept.Clear();
            authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            authClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-type", Medicare.Properties.Resources.AuthContentType);
            var requestMsg = new HttpRequestMessage();
            String grantType = Medicare.Properties.Resources.GrantType;
            String clientId = tenantMedicareProperty.ClientId;//Medicare.Properties.Resources.clientId;
            Guid messageId = Guid.Empty;                                                  // var assertionKey = GenerateAuthorizationToken();
            try
            {
                var apiResp = GenerateAuthorizationTokenFromAzure(tenantId,locationId);
                if (!(apiResp.StatusCode == StatusCodes.Status200OK))
                {
                    return apiResp;
                }
                string assertionKey = (string)apiResp.Result;
                string body = "client_id=" + clientId + "&grant_type=" + grantType + "&assertion=" + assertionKey;

                messageId = ProdaRequestLogHelper.CreateProdaRequestLog(Guid.Parse(tenantId), locationId, body);

                System.Net.Http.StringContent data = new System.Net.Http.StringContent(body,
                                                     Encoding.UTF8,
                                                     "application/x-www-form-urlencoded");
                //HttpResponseMessage response = await authClient.PostAsync(authUrl, data);
                Task<HttpResponseMessage> task = authClient.PostAsync(authUrl, data);
                task.Wait();
                //string content = await response.Content.ReadAsStringAsync();
                string content = await task.Result.Content.ReadAsStringAsync();
                apiResponse.Result = content;
                JObject jsonResp = JObject.Parse(content);
                apiResponse.StatusCode = (int)task.Result.StatusCode;
                if (task.Result.IsSuccessStatusCode)
                {
                    string accessToken = (string)jsonResp["access_token"];
                    DateTime tokenExpiry = DateTime.UtcNow.AddSeconds(2700);
                    if (prodaTokenProperty is null)
                    {
                        prodaTokenProperty = new ProdaTokenProperty();
                        prodaTokenProperty.TenantId = Guid.Parse(tenantId);
                        prodaTokenProperty.LocationId = locationId;
                        prodaTokenProperty.AccessToken = accessToken;
                        prodaTokenProperty.TokenExpiry = tokenExpiry;
                        ProdaTokenPropertyHelper.CreateProdaTokenProperty(prodaTokenProperty);
                    }
                    else
                    {
                        prodaTokenProperty.AccessToken = accessToken;
                        prodaTokenProperty.TokenExpiry = tokenExpiry;
                        ProdaTokenPropertyHelper.updateProdaTokenProperty(prodaTokenProperty);
                    }
                }

             ProdaRequestLogHelper.UpdateProdaRequestLog(messageId, content);
             return apiResponse;

            }
            catch (Exception e)
            {
                //TODO: Log event in the logs
                ProdaRequestLogHelper.UpdateProdaRequestLog(messageId, e.Message.ToString());
                apiResponse.Errors.Add(e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }

        }  
       
        //method to generate RSA key pair and certificate
        private byte[] CreateRSAKeys(string tenantId,string locationId )
        {
            string deviceName = this.tenantMedicareProperty.DeviceName;
           

            using (RSA parent = RSA.Create(4096))
            using (RSA myRSA = RSA.Create(2048))
            {

                byte[] pubByte = myRSA.ExportRSAPublicKey();
                String pubKeyStr = Convert.ToBase64String(pubByte);
                byte[] pvtByte = myRSA.ExportRSAPrivateKey();
                String pvtKeyStr = Convert.ToBase64String(pvtByte);
               
                CertificateRequest parentReq = new CertificateRequest(

                    "CN=Capstone Systems Certificate Authority",
                    parent,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                parentReq.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                parentReq.CertificateExtensions.Add(
                    new X509SubjectKeyIdentifierExtension(parentReq.PublicKey, false));

                using (X509Certificate2 parentCert = parentReq.CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddDays(-45),
                    DateTimeOffset.UtcNow.AddDays(365)))
                {
                    CertificateRequest req = new CertificateRequest(
                        "CN=" + this.tenantMedicareProperty.DeviceName + ",O=Capstone Systems,L=Canberra,S=ACT,C=AU", //TODO: Device Name
                        myRSA,
                        HashAlgorithmName.SHA256,
                        RSASignaturePadding.Pkcs1);

                    req.CertificateExtensions.Add(
                        new X509BasicConstraintsExtension(false, false, 0, false));

                    req.CertificateExtensions.Add(
                        new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.NonRepudiation,
                                                    false)
                        );

                    req.CertificateExtensions.Add(
                        new X509EnhancedKeyUsageExtension(
                                new OidCollection
                                {
                                    new Oid("1.3.6.1.5.5.7.3.8")
                                },
                            true));

                    req.CertificateExtensions.Add(
                        new X509SubjectKeyIdentifierExtension(req.PublicKey, false));

                    try
                    {
                        using (X509Certificate2 cert = req.Create(
                            parentCert,
                            DateTimeOffset.UtcNow.AddDays(-1),
                            DateTimeOffset.UtcNow.AddDays(90),
                            new byte[] { 1, 2, 3, 4 }))
                        {
                            using (X509Certificate2 certnew = cert.CopyWithPrivateKey(myRSA))
                            {
                                //return certnew;
                                return certnew.Export(X509ContentType.Pfx);
                               //return StoreCertificate(certnew.Export(X509ContentType.Pfx),tenantId,locationId, isKeyRefresh);
                            }

                        }

                    }
                    catch(Exception e)
                    {
                        //TODO:logging
                        return null;
                    }


                }
            }
        }
        private string CreatePublicKeyJWTFromCert(byte[] cert)
        {
            X509Certificate2 certificate = new X509Certificate2(cert, string.Empty, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            RSA rsa = certificate.GetRSAPublicKey();
            RSAParameters rsp = rsa.ExportParameters(false);

            RsaSecurityKey rsaSec = new RsaSecurityKey(rsa);
            if (this.tenantMedicareProperty != null)
            {
                Dictionary<string, object> publicKeyDict = new Dictionary<string, object>()
            {
                {"kty", "RSA"},
                {"kid", tenantMedicareProperty.DeviceName},
                {"use", "sig"},
                {"alg", "RS256"},
                {"n", Jose.Base64Url.Encode(rsp.Modulus) },
                {"e", Jose.Base64Url.Encode(rsp.Exponent)}
            };


                var body = new
                {
                    orgId = tenantMedicareProperty.ProdaOrgRa,
                    otac = tenantMedicareProperty.DeviceActivationCode,
                    key = publicKeyDict
                };


                string jwkPublicKeyJson = System.Text.Json.JsonSerializer.Serialize(body).ToString();
               



                return jwkPublicKeyJson;
            }
            else
            {
                return "";
            }
        }
      
        private void GetAzureCreds(string strTenant,string locationId)
        {   if (azureProperty is null)
            {
                
                using (TenantDBContext tCTX = new TenantDBContext())
                {
                    TenantAzureProperty property = tCTX.TenantAzureProperties.Find(Guid.Parse(strTenant), locationId);
                    if (property != null)
                    {
                        string clientId = property.ClientId;
                        string secretKey = property.SecretKey;
                        string azureTenant = property.AzureTenantId;

                        
                            this.azureProperty = property;
                           
                        
                    }
                }
            }
            
                       
            
        }

        private ApiResponse<dynamic> StoreCertificate(byte[] cert, string tenantId, string locationId )
        {
            var apiResponse = new ApiResponse<dynamic>();

            try
            {
                
                GetAzureCreds(tenantId, locationId);
                if(azureProperty is null)
                {
                    apiResponse.Errors.Add("Storage of Certificate is unsuccessful.Azure Properties cannot be loaded.");
                    apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                    return apiResponse;
                }
                string certName = azureProperty.CertificateName;//env + "_" + locationId + "_" + tenantId;
                

               
                string keyVaultUri = this.azureProperty.CertificateUri;

                ClientSecretCredential credential = new ClientSecretCredential(azureProperty.AzureTenantId, azureProperty.ClientId, azureProperty.SecretKey);
                var certClient = new CertificateClient(new Uri(keyVaultUri), credential);
               
                ImportCertificateOptions options = new ImportCertificateOptions(certName, cert);
                Azure.Response<KeyVaultCertificateWithPolicy> resp = certClient.ImportCertificate(options);

                apiResponse.StatusCode = StatusCodes.Status200OK;
                return apiResponse;
            }
            catch (Exception e)
            {
                apiResponse.Errors.Add("Storage of Certificate is unsuccessful." + e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
        }
      
        
        private ApiResponse<dynamic> GenerateAuthorizationTokenFromAzure(string tenantId,string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            try
            {
                X509Certificate2 cert;
                var apiResp = RetrieveAzureCertPrivateKey(tenantId, locationId);
                if(apiResp.StatusCode == StatusCodes.Status200OK)
                {
                    cert = (X509Certificate2)apiResp.Result;
                    var prodaAud = Medicare.Properties.Resources.accessTokenAudience;
                    if (cert is null)
                    {
                        
                        apiResponse.Errors.Add("Retrieval of Certificate is unsuccessful." );
                        apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                        return apiResponse;
                    }
                    var rsa = cert.GetRSAPrivateKey();
                    var issueTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    //expiry set at 10 minutes
                    var expriryTime = DateTimeOffset.Now.ToUnixTimeSeconds() + 600;
                    var payload = new Dictionary<string, object>()
                    {
                        {"iss",tenantMedicareProperty.ProdaOrgRa},
                        {"sub", tenantMedicareProperty.DeviceName},
                        {"aud",prodaAud },
                        {"token.aud", prodaAud },
                        {"exp", expriryTime},
                        {"iat" ,issueTime }


                    };
                    var extraHeader = new Dictionary<string, object>()
                    {
                        {"kid", tenantMedicareProperty.DeviceName}
                    };
                    var encryptedToken =
                      JWT.Encode(
                        payload,
                        rsa,
                        JwsAlgorithm.RS256,
                        extraHeader,
                        null,
                        null);
                    apiResponse.StatusCode = StatusCodes.Status200OK;
                    apiResponse.Result = encryptedToken;
                    return apiResponse;
                    //return encryptedToken;
                }
                else
                {
                    return apiResp;
                }
                 
               
            }
            catch(Exception e)
            {
                apiResponse.Errors.Add("Generation of Proda token is unsuccessful." + e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }


           
        }
        //To retrieve the pririvate key from the keyvault.
        private ApiResponse<dynamic> RetrieveAzureCertPrivateKey(string tenantId, string locationId)
        {
            var apiResponse = new ApiResponse<dynamic>();
            GetAzureCreds(tenantId, locationId);
            if (azureProperty is null)
            {
                apiResponse.Errors.Add("Retrieval of Certificate is unsuccessful.Azure Properties cannot be loaded.");
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
            string certName = azureProperty.CertificateName;

            string keyVaultUri = azureProperty.CertificateUri;
            try
            {
                ClientSecretCredential credential = new ClientSecretCredential(azureProperty.AzureTenantId, azureProperty.ClientId, azureProperty.SecretKey);
                SecretClient client = new SecretClient(new Uri(keyVaultUri), credential);
                Azure.Response<KeyVaultSecret> resp1 = client.GetSecret(certName);
                KeyVaultSecret vaultsec = resp1.Value;
                var certificate = System.Convert.FromBase64String(vaultsec.Value);
                X509Certificate2 cert = new X509Certificate2(certificate, string.Empty, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                apiResponse.Result = cert;
                apiResponse.StatusCode = StatusCodes.Status200OK;
                return apiResponse;
            }
            catch (Exception e)
            {
                apiResponse.Errors.Add("Retrieval of Certificate is unsuccessful." + e.Message);
                apiResponse.StatusCode = StatusCodes.Status400BadRequest;
                return apiResponse;
            }
        }
        private void PopulateProdaTokenProperties(string tenantId, string locationId)
        {
            try
            {
                using (TenantDBContext tCtx = new TenantDBContext())
                {
                    this.prodaTokenProperty = tCtx.ProdaTokenProperties.Find(Guid.Parse(tenantId), locationId);
                }
            }
           
            catch (Exception  e)
            {//TODO:Logging
               // Console.WriteLine("Exception in PopulateProdaTokenProperties:" + e.Message);
            }
        }
        internal bool ValidateProdaToken(string tenantId,string locationId)
        {

            if(prodaTokenProperty is null)
            {
                PopulateProdaTokenProperties(tenantId, locationId);
            }
            if(! (prodaTokenProperty is null))
            {
                if(!(prodaTokenProperty.KeyExpiry is null ))
                {
                    DateTime keyExpiry = (DateTime)prodaTokenProperty.KeyExpiry;
                    if (keyExpiry.Date > DateTime.UtcNow.Date)
                    {
                        return true;
                    }
                    else
                    {

                         var apiResp =  KeyRefresh(tenantId, locationId);
                        if (apiResp.StatusCode == StatusCodes.Status200OK)
                        {
                            return true;
                        }
                    }
                    
                }
            }
            return false;

        }
    }
}
