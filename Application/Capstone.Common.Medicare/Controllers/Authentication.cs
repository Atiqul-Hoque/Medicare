using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using Jose;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Capstone.Common.Medicare;
using Capstone.Common.Medicare.Models;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Capstone.Common.Medicare.Helpers;

namespace Capstone.Common.Controllers.MedicareOnline
{
    [Route("api/[controller]")]
    [ApiController]
    public class Authentication : ControllerBase
    {


        /// <summary>
        /// Gets the authorisation code for authorisation with Medicare. If necessary it looks in memory first for an authorisation code.
        /// If the token has not expired, use the same one otherwise get a new one and return it.
        /// </summary
        /// <returns>String with the current authorisation code.</returns>
        /// 
        private TenantAzureProperty azureProperty;
        public TenantMedicareProperty tenantMedicareProperty;
        private MedicareTokenProperty medicareTokenProperty;

        [HttpGet]
        [Route("oauth2Token")]
        public string GetAuthorisationCodeUsingAzure(string tenantId, string locationId)
        {
            try
            {
                bool medPropsStatus = PopulateTenantMedicareProperties(tenantId, locationId);
                if (!medPropsStatus)
                {
                    return string.Empty;
                }

                PopulateMedicareTokenProperties(tenantId, locationId);
                PopulateTenantMedicareProperties(tenantId, locationId);


                if ((medicareTokenProperty is null) || (string.IsNullOrWhiteSpace(medicareTokenProperty.AccessToken) || medicareTokenProperty.TokenExpiry is null || medicareTokenProperty.TokenExpiry <= DateTime.UtcNow))
                {
                    bool status = AuthenticationWithMedicareAzure(Guid.Parse(tenantId), locationId).Result;
                    if (status)
                    {
                        return medicareTokenProperty.AccessToken;
                    }

                }

                else
                {
                    return medicareTokenProperty.AccessToken;
                }
                return string.Empty;
            }
            catch (FormatException f)
            {
                Console.WriteLine("Exception in GetAuthorisationCodeUsingAzure:" + f.Message);
                return string.Empty;
            }
            catch (ArgumentNullException arg)
            {
                Console.WriteLine("Exception in GetAuthorisationCodeUsingAzure:" + arg.Message);
                return string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in GetAuthorisationCodeUsingAzure:" + e.Message);
                return string.Empty;
            }
        }

        private bool PopulateMedicareTokenProperties(string tenantId, string locationId)
        {
            try
            {
                using (TenantDBContext tCtx = new TenantDBContext())
                {
                    this.medicareTokenProperty = tCtx.MedicareTokenProperties.Find(Guid.Parse(tenantId), locationId);
                    if (!(medicareTokenProperty is null))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (FormatException f)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + f.Message);
                return false;
            }
            catch (ArgumentNullException ax)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + ax.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + e.Message);
                return false;
            }
        }

        private async Task<bool> AuthenticationWithMedicareAzure(Guid TenantId, string locationid)
        {
            Guid messageId = Guid.Empty;
            bool status = false;
            try
            {
                if (tenantMedicareProperty is null)
                {
                    PopulateTenantMedicareProperties(TenantId.ToString(), locationid);
                    if (tenantMedicareProperty is null)
                    {
                        return false;
                    }
                }
                HttpClientHandler handler = new HttpClientHandler() { UseDefaultCredentials = false };
                HttpClient authClient = new HttpClient(handler);
                string authUrl = Medicare.Properties.Resources.AuthEndPoint;
                authClient.BaseAddress = new Uri(authUrl);
                authClient.DefaultRequestHeaders.Accept.Clear();
                authClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                authClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-type", Medicare.Properties.Resources.AuthContentType);
                var requestMsg = new HttpRequestMessage();
                String grantType = Medicare.Properties.Resources.GrantType;

                String clientId = tenantMedicareProperty.ClientId;
                var assertionKey = GenerateAuthorizationTokenAzure(TenantId.ToString(), locationid);

                string body = "client_id=" + clientId + "&grant_type=" + grantType + "&assertion=" + assertionKey;

                System.Net.Http.StringContent data = new System.Net.Http.StringContent(body,
                                                     Encoding.UTF8,
                                                     "application/x-www-form-urlencoded");
                messageId = ProdaRequestLogHelper.CreateProdaRequestLog(TenantId, locationid, body);
                Task<HttpResponseMessage> task = authClient.PostAsync(authUrl, data);
                task.Wait();

                string content = await task.Result.Content.ReadAsStringAsync();

                JObject jsonResp = JObject.Parse(content);
                if (task.Result.IsSuccessStatusCode)
                {
                    string accessToken = (string)jsonResp["access_token"];
                    DateTime keyExpiry = DateTime.UtcNow.AddSeconds(3000);// DateTime.Now.AddSeconds((double)jsonResp["expires_in"]);
                    if (medicareTokenProperty is null)
                    {
                        this.medicareTokenProperty = new MedicareTokenProperty();
                        medicareTokenProperty.TenantId = TenantId;
                        medicareTokenProperty.LocationId = locationid;
                        medicareTokenProperty.AccessToken = accessToken;
                        medicareTokenProperty.TokenExpiry = keyExpiry;
                        medicareTokenProperty.Modified = DateTime.UtcNow;
                        MedicareTokenPropertyHelper.createProperty(medicareTokenProperty);
                    }
                    else
                    {
                        medicareTokenProperty.AccessToken = accessToken;
                        medicareTokenProperty.TokenExpiry = keyExpiry;
                        medicareTokenProperty.Modified = DateTime.UtcNow;
                        MedicareTokenPropertyHelper.updateProperty(medicareTokenProperty);
                    }
                    status = true;

                }

                ProdaRequestLogHelper.UpdateProdaRequestLog(messageId, content);
                return status;
            }
            catch (Exception e)
            {
                //TODO: Log event in the logs

                Console.WriteLine(e.Message.ToString());
                ProdaRequestLogHelper.UpdateProdaRequestLog(messageId, e.Message);
                return false;
            }

        }

        private X509Certificate2 RetrieveAzureCert(string tenantId, string locationId)
        {
            try
            {
                GetAzureCreds(tenantId, locationId);
                if (azureProperty is null)
                {
                    return null;
                }
                //string certName = "DevWBD00000";//azureProperty.CertificateName;//env + locationId;
                string certName = azureProperty.CertificateName;

                string keyVaultUri = azureProperty.CertificateUri;

                ClientSecretCredential credential = new ClientSecretCredential(azureProperty.AzureTenantId, azureProperty.ClientId, azureProperty.SecretKey);
                var certClient = new CertificateClient(new Uri(keyVaultUri), credential);
                Azure.Response<KeyVaultCertificateWithPolicy> resp = certClient.GetCertificate(certName);
                KeyVaultCertificateWithPolicy certPolicy = resp.Value;
                X509Certificate2 cert = new X509Certificate2(certPolicy.Cer);
                return cert;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        private bool GetAzureCreds(string tenantId, string locationId)
        {
            try
            {
                using (TenantDBContext tCtx = new TenantDBContext())
                {
                    this.azureProperty = tCtx.TenantAzureProperties.Find(Guid.Parse(tenantId), locationId);
                    if (!(azureProperty is null))
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (FormatException f)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + f.Message);
                return false;
            }
            catch (ArgumentNullException ax)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + ax.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + e.Message);
                return false;
            }

        }

        private bool PopulateTenantMedicareProperties(string tenantId, string locationId)
        {
            try
            {
                using (TenantDBContext tCtx = new TenantDBContext())
                {
                    this.tenantMedicareProperty = tCtx.TenantMedicareProperties.Find(Guid.Parse(tenantId), locationId);
                    if (!(tenantMedicareProperty is null))
                    {
                        return true;
                    }
                }
                return false;
            }

            catch (FormatException f)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + f.Message);
                return false;
            }
            catch (ArgumentNullException ax)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + ax.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in PopulateMedicareTokenProperties:" + e.Message);
                return false;
            }
        }
        private string GenerateAuthorizationTokenAzure(string tenantId, string locationId)
        {
            //string Certificate_location = Medicare.Properties.Resources.CertificateFolder + "capstonesystems_private.pfx";

            X509Certificate2 cert = RetrieveAzureCertPrivateKey(tenantId, locationId);
            var rsa = cert.GetRSAPrivateKey();

            var issueTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            var expriryTime = DateTimeOffset.Now.ToUnixTimeSeconds() + 3500;
            var payload = new Dictionary<string, object>()
            {
                {"iss",tenantMedicareProperty.ProdaOrgRa },//Medicare.Properties.Resources.ProdaOrgRA},
                {"sub",tenantMedicareProperty.DeviceName },// Medicare.Properties.Resources.DeviceName},
                {"aud",Medicare.Properties.Resources.accessTokenAudience },
                {"token.aud", Medicare.Properties.Resources.MedicareTokenAudience },
                {"exp", expriryTime},
                {"iat" ,issueTime }
            };
            var extraHeader = new Dictionary<string, object>()
            {
                {"kid", tenantMedicareProperty.DeviceName }//Medicare.Properties.Resources.DeviceName}
            };
            var encryptedToken =
              JWT.Encode(
                payload,
                rsa,
                JwsAlgorithm.RS256,
                extraHeader,
                null,
                null);
            return encryptedToken;
        }

        private X509Certificate2 RetrieveAzureCertPrivateKey(string tenantId, string locationId)
        {
            GetAzureCreds(tenantId, locationId);
            if (azureProperty is null)
            {
                return null;
            }
            //string certName = "DevWBD00000";//azureProperty.CertificateName;//env + locationId;
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
                return cert;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
