using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FHMC.Graph
{

    public class BaseClass
    {

        private static string ClientId = "f5e831ca-ac9a-4526-80aa-9b26a7d75aa8";
        private static string ClientSecret = @"Z018Q~pMFA4BW1eIDcn57OpISBBg.DnvdwVC6cRm";
        private static string TenantId = "5b3538ca-e4fc-4828-a305-8e9c303853bd";
        //private static string CertificateFileName = @"";
        //private static string CertificateName = "FirstHome VOESystem OOO Cert";
        protected static string graphUrl = @"https://graph.microsoft.com/v1.0/";

        protected GraphServiceClient graphClient = null;
        protected dynamic Log;

        private enum WebMethod
        {
            GET,
            POST,
            DELETE
        }

        public enum ContentType
        {
            application_json,
            application_vnd_ms_excel,
            text_plain
        }

        protected BaseClass(object logger)
        {

            if (logger == null)
            {
                Log = new FHMC.NLogWrapper.Logger(GetType().FullName);
            }
            else
            {
                Log = logger;
            }

            //create initial graph client
            setGraphClient();
        }

        protected void setGraphClient()
        {

            TokenHandler tHand = new TokenHandler();
            string accessToken = tHand.getToken();

            ////using fiddler proxy
            //var httpClientHandler = new HttpClientHandler
            //{
            //    Proxy = new WebProxy("http://localhost:8888", true),
            //    UseProxy = true // Ensure proxy usage is enabled
            //};

            //var httpProvider = new HttpProvider(httpClientHandler, false);

            //graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            //{
            //    requestMessage
            //        .Headers
            //        .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

            //    return Task.FromResult(0);
            //}), httpProvider);


            //not using fiddler proxy
            graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
            {
                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));



        }

        protected string isNull(object inString, string replVal)
        {

            if (inString == null)
            {
                return replVal;
            }
            else
            {
                return inString.ToString();
            }

        }

        protected bool skipError(Exception ex)
        {

            if (ex.InnerException != null)
            {
                if (ex.InnerException.GetType() == typeof(Microsoft.Graph.ServiceException))
                {
                    if (((Microsoft.Graph.ServiceException)ex.InnerException).StatusCode.ToString() == "NotFound")
                    {
                        return true;
                    }

                }
            }

            return false;
        }

        protected string makeGraphRequest(string url, HttpMethod method, object content = null, Dictionary<string, string> headers = null, ContentType contentType = ContentType.application_json)
        {

            string retVal = null;

            // Create the request message
            HttpRequestMessage hrm = new HttpRequestMessage(method, url);
            if (content != null)
            {
                if (contentType == ContentType.application_json)
                {
                    hrm.Content = new StringContent(content.ToString(), System.Text.Encoding.UTF8, "application/json");
                }
                else if (contentType == ContentType.application_vnd_ms_excel)
                {
                    hrm.Content = new ByteArrayContent((byte[])content);
                    hrm.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/vnd.ms-excel");
                }
                else if (contentType == ContentType.text_plain)
                {
                    if (content.GetType().Name == "Byte[]")
                    {
                        hrm.Content = new StringContent(Convert.ToBase64String((byte[])content));
                    }
                    else
                    {
                        hrm.Content = new StringContent(content.ToString());
                    }
                    hrm.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
                }
                else
                {
                    throw new Exception("Unsuported Content Type");
                }
            }

            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    hrm.Headers.Add(header.Key, header.Value);
                }
            }

            //ask for immutable ids
            hrm.Headers.Add("Prefer", "IdType = \"ImmutableId\"");

            // Authenticate (add access token) our HttpRequestMessage
            graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm);

            // Send the request and get the response.
            int iRequestCount = 0;
            int iRequestLimit = 5;
            HttpResponseMessage response = null;
            do
            {
                iRequestCount++;
                try
                {
                    response = graphClient.HttpProvider.SendAsync(hrm).Result;
                }
                catch (Exception ex)
                {
                    Exception reviewEx = ex.InnerException == null ? ex : ex.InnerException;
                    if (!reviewEx.Message.Contains("UnknownError"))
                    {
                        //throw this up to caller..else retry
                        throw ex;
                    }
                }
            } while (iRequestCount < iRequestLimit && response == null);

            if (response == null)
            {
                throw new Exception("Unable to complete Graph request");
            }

            if (response.IsSuccessStatusCode)
            {
                //get parent folder
                string contentJSON = response.Content.ReadAsStringAsync().Result;
                retVal = contentJSON;
            }
            else
            {

                string httpCode = response.StatusCode.ToString();
                string httpMessage = response.Content.ReadAsStringAsync().Result;

                throw new GraphAPICustomException("GraphAPI Request Failed", httpCode, httpMessage);
            }

            return retVal;

        }

        protected class TokenHandler
        {


            public string getToken()
            {
                // Even if this is a console application here, a daemon application is a confidential client application
                IConfidentialClientApplication app;

                //#if !VariationWithCertificateCredentials

                app = ConfidentialClientApplicationBuilder.Create(ClientId)
                           .WithClientSecret(ClientSecret)
                           .WithTenantId(TenantId)
                           .Build();
                //#else
                // Building the client credentials from a certificate
                //makecert -r -pe -n "CN=FirstHome VOESystem OOO Cert" -b 05/01/2019 -e 05/01/2020 -ss my -len 2048

                ///BEGIN PROD CODE
                //X509Store store = new X509Store(StoreLocation.LocalMachine);
                //store.Open(OpenFlags.OpenExistingOnly);

                //X509Certificate2 cert = store.Certificates.OfType<X509Certificate2>()
                //    .Where(q => q.SubjectName.Name.Contains(CertificateName)).FirstOrDefault();

                //app = ConfidentialClientApplicationBuilder.Create(ClientId)
                //    .WithCertificate(cert)
                //    .WithTenantId(TenantId)
                //    .Build();

                //store.Close();

                //END PROD CODE

                //#endif

                // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the
                // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
                // a tenant administrator
                string[] scopes = new string[] { "https://graph.microsoft.com/.default" };


                string result = null;

                try
                {
                    result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result.AccessToken;
                }
                catch (MsalServiceException ex)
                {
                    // Case when ex.Message contains:
                    // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
                    // Mitigation: change the scope to be as expected
                }

                return result;
            }

        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

    }

    public class GraphAPICustomException : Exception, ISerializable
    {
        public string HTTPCode { get; private set; }
        public string HTTPMessage { get; private set; }

        public GraphAPICustomException()
        {

        }
        public GraphAPICustomException(string message)
            : base(message)
        {

        }
        public GraphAPICustomException(string message, Exception inner)
            : base(message, inner)
        {

        }


        public GraphAPICustomException(string message, string statuscode, string httpmessage)
            : base(message)
        {
            message += "; HTTP Status: " + statuscode;
            message += "; HTTP Message: " + httpmessage;

            HTTPCode = statuscode;
            HTTPMessage = httpmessage;

        }

        public GraphAPICustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }

}
