using Newtonsoft.Json;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VOEBackend.Truv.TruvSchema;
using VOESystem.Data.DBSchema;
using static VOEBackend.Truv.TruvSchema.Response;
using static VOESystem.Data.Business.BusinessBase;

namespace VOEBackend.Truv.Business
{
    public class CommOps : BaseClass
    {
        private const string TruvServiceURL = @"prod.truv.com/v1";  //prod url
        private const string ClientId = @"e56b3706a61944c7bf6e21ea5320a6df";  //client key
        //private const string SecretId = @"sandbox-519f10892176610fb9abf40ccf2a72d1ae2d03ea"; //sandbox secret id
        private const string SecretId = @"prod-cf03fe1221ee6cdfdf9a299dae6dd14150e2bc38"; //prod secret id

        public enum TruvCommType
        {
            CreateCredentials,
            QueryCredentials,
            CreateReverifyCredentials,
            QueryReverifyCredentials,
            SearchCompany
        }

        public List<ResponseResult> postRequest(IDbConnection dbConn, Request request, string OrderNumber, int orderRequestId, string UserName, 
            TruvCommType TruvCommType, string TruvOrderNumber, string testResponse = null, string SearchEmployerName = null)
        {

            List<ResponseResult> retVal = new List<ResponseResult>() { };

            string responseString;
            int RequestLogId = 0;
            string RespFilePathName = "";
            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            Dictionary<string, string> addlHeaders = new Dictionary<string, string>() { };

            if (testResponse == null)
            {

                string postString = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                //write to file, elimiating password
                string orderTag = TruvCommType.ToString();
                string OrderFilePathName = writeRequestStringToFile(postString, OrderNumber, orderTag, DateTimeStamp);

                //log request to database
                RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, orderTag, UserName);

                //post to service
                addlHeaders.Add("X-Access-Client-Id", ClientId);
                addlHeaders.Add("X-Access-Secret", SecretId);
                addlHeaders.Add("Accept", "application/json");

                string opEndpoint = null;
                WebRequestMethod method = WebRequestMethod.GET;

                if (TruvCommType == TruvCommType.CreateCredentials)
                {
                    method = WebRequestMethod.POST;
                    opEndpoint = "/orders/";
                }
                else if (TruvCommType == TruvCommType.CreateReverifyCredentials)
                {
                    method = WebRequestMethod.POST;
                    opEndpoint = "/orders/" + TruvOrderNumber;
                }
                else if (TruvCommType == TruvCommType.QueryCredentials || TruvCommType == TruvCommType.QueryReverifyCredentials)
                {
                    method = WebRequestMethod.GET;
                    opEndpoint = "/orders/" + TruvOrderNumber;
                }
                else if (TruvCommType == TruvCommType.SearchCompany)
                {
                    method = WebRequestMethod.GET;
                    opEndpoint = "/company-mappings-search/?query=" + SearchEmployerName;
                }
                
                byte[] content = null;
                if (request != null)
                {
                    content = Encoding.UTF8.GetBytes(postString);
                }

                responseString = makeWebRequest(TruvServiceURL + opEndpoint, method, "application/json", true, null,
                    content, addlHeaders);
                
                //save response to file
                RespFilePathName = RepositoryPath + "Documents\\TruvComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + "Response.json";
                File.WriteAllText(RespFilePathName, responseString);
            }
            else
            {
                responseString = testResponse;
            }

            string responseStatuses;
            string certFilePathName = null;

            //deserialize response
            if (TruvCommType == TruvCommType.SearchCompany)
            {
                List<CompanySearchResponse> companies = JsonConvert.DeserializeObject<List<CompanySearchResponse>>(responseString);

                retVal.Add(new ResponseResult
                {
                    Companies = companies.Select<CompanySearchResponse, CompanyResult>(q => new CompanyResult
                    {
                        CompanyMappingId = q.company_mapping_id,
                        CompanyName = q.name,
                        SuccessRate = q.success_rate,
                        ConfidenceLevel = q.confidence_Level
                    }).ToList()
                });

                responseStatuses = "OK";

            }
            else
            {

                Response response = JsonConvert.DeserializeObject<Response>(responseString);

                string certFileName;

                if (response.error != null)
                {
                    responseStatuses = response.error.code;
                    retVal.Add(new ResponseResult
                    {
                        OrderStatus = "Error"
                    });

                }
                else
                {

                    string fileName;
                    string filePathName;
                    int iCounter;
                    int iPayStubCounter;
                    int iW2Counter;

                    //List<string> pendingStatuses = new List<string>() { "pending", "sent" };
                    List<string> cancelledStatuses = new List<string>() { "canceled", "skipped", "no_data", "error", "expired" };

                    responseStatuses = String.Join(",", response.employers.Select(q => q.status).ToArray());

                    iCounter = 1;
                    iPayStubCounter = 1;
                    iW2Counter = 1;

                    foreach (Response.Employer employer in response.employers)
                    {

                        ResponseResult result = new ResponseResult();
                        result.W2s = new List<string>() { };
                        result.PayStubs = new List<string>() { };

                        result.OrderStatus = employer.status;
                        result.ShareURL = response.share_url;

                        if (employer.status == "completed")
                        {

                            bool certDownloaded = false;

                            //download cert
                            if (TruvCommType == TruvCommType.QueryReverifyCredentials)
                            {
                                //employment level cert w/o d1c number
                                certFileName = DateTimeStamp + "_" + OrderNumber + "_TruvReverify" + iCounter.ToString() + ".pdf";
                                certFilePathName = RepositoryPath + "Documents\\TruvDocuments\\" + certFileName;
                                certDownloaded = DownloadDocument(employer.pdf_report, certFilePathName);
                            }
                            else
                            {
                                //borrower level cert w/d1c number
                                certFileName = DateTimeStamp + "_" + OrderNumber + "_TruvCert" + iCounter.ToString() + ".pdf";
                                certFilePathName = RepositoryPath + "Documents\\TruvDocuments\\" + certFileName;
                                certDownloaded = DownloadBorrowerCertDocument(response.user_id, response.voie_report_id, certFilePathName, addlHeaders);
                            }

                            if (certDownloaded)
                            {
                                result.OrderStatus = "Completed";
                                result.CertFile = certFilePathName;

                                //get other files
                                foreach (Employment employment in employer.employments)
                                {
                                    if (employment.statements != null)
                                    {
                                        foreach (Statement statement in employment.statements)
                                        {
                                            if (statement.file != null)
                                            {
                                                fileName = DateTimeStamp + "_" + OrderNumber + "_TruvPayStub" + iPayStubCounter.ToString() + ".pdf";
                                                filePathName = RepositoryPath + "Documents\\TruvDocuments\\" + fileName;

                                                DownloadDocument(statement.file, filePathName);
                                                result.PayStubs.Add(filePathName);
                                                iPayStubCounter++;
                                            }
                                        }
                                    }

                                    if (employment.w2s != null)
                                    {
                                        foreach (W2 w2 in employment.w2s)
                                        {
                                            if (w2.file != null)
                                            {
                                                fileName = DateTimeStamp + "_" + OrderNumber + "_TruvW2" + iW2Counter.ToString() + ".pdf";
                                                filePathName = RepositoryPath + "Documents\\TruvDocuments\\" + fileName;

                                                DownloadDocument(w2.file, filePathName);
                                                result.W2s.Add(filePathName);
                                                iW2Counter++;
                                            }
                                        }
                                    }

                                }

                            }
                            else
                            {
                                result.OrderStatus = "Failed";
                            }

                        }
                        else if (cancelledStatuses.Contains(employer.status))
                        {
                            //cancelled
                            result.OrderStatus = "Cancelled";
                        }
                        else
                        {
                            //processing
                            result.OrderStatus = "Processing";
                        }

                        result.TruvOrderId = response.id;
                        retVal.Add(result);

                        iCounter++;
                        iPayStubCounter++;
                        iW2Counter++;

                    }


                }
            }

            //log response to database
            LogServiceResponse(dbConn, orderRequestId, RequestLogId, RespFilePathName, responseStatuses, certFilePathName);


            return retVal;

        }

        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, string OrderTag, string UserName)
        {
            int retVal = 1;

            int VendorRequestTypeId = dbConn.Where<VendorRequestType>(q => q.Name == OrderTag).FirstOrDefault().Id;
            int VendorId = dbConn.Where<Vendor>(q => q.Name == "Truv").FirstOrDefault().Id;

            VendorRequestLog log = new VendorRequestLog()
            {
                VendorRequestTypeId = VendorRequestTypeId,
                OrderRequestId = orderRequestId,
                RequestDateTime = DateTime.Now,
                RequestFileName = RequestFileName,
                UserName = UserName,
                VendorId = VendorId
            };

            dbConn.Insert<VendorRequestLog>(log);
            retVal = (int)dbConn.GetLastInsertId();

            return retVal;

        }

        public void LogServiceResponse(IDbConnection dbConn, int orderRequestId, int VendorRequestLogId, string ResponseFileName,
            string StatusCode,string DocumentFilePathName)
        {

            VendorResponseLog log = new VendorResponseLog()
            {
                VendorRequestLogId = VendorRequestLogId,
                OrderRequestId = orderRequestId,
                ResponseDateTime = DateTime.Now,
                ResponseFileName = ResponseFileName,
                StatusCode = StatusCode,
                DocumentFileName = DocumentFilePathName
            };

            dbConn.Insert<VendorResponseLog>(log);

        }

        public class ResponseResult
        {

            public string OrderStatus { get; set; }
            public string TruvOrderId { get; set; }
            public string ShareURL { get; set; }

            public string CertFile { get; set; }
            public List<string> PayStubs { get; set; }
            public List<string> W2s { get; set; }

            public List<CompanyResult> Companies { get; set; }
           
        }

        public class CompanyResult
        {
            public string CompanyMappingId { get; set; }
            public string CompanyName { get; set; }
            public string SuccessRate { get; set; }
            public string ConfidenceLevel { get; set; }

        }

        public bool DownloadDocument(string pdfUrl, string filePathName)
        {
            bool retVal = false;

            try
            {
                if (File.Exists(filePathName))
                {
                    File.Delete(filePathName);
                }

                using (WebClient client = new WebClient())
                {

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    UriBuilder uriB = new UriBuilder(pdfUrl);
                    client.DownloadFile(uriB.Uri, filePathName);
                    retVal = true;

                }

            }
            catch (Exception ex)
            {

                logger.Info("Error Saving Truv File to " + filePathName, ex);
            }

            return retVal;

        }

        public bool DownloadBorrowerCertDocument(string userId, string voieReportId, string certFilePathName, Dictionary<string, string> addlHeaders)
        {

            bool retVal = false;

            try
            {
                if (File.Exists(certFilePathName))
                {
                    File.Delete(certFilePathName);
                }

                string endpoint = "/users/" + userId + "/reports/" + voieReportId + @"/?fmt=pdf";

                Stream responseStream = makeWebRequestStream(TruvServiceURL + endpoint, WebRequestMethod.GET, "text/plain", true, null,
                   null, addlHeaders);

                using (FileStream fstream = new FileStream(certFilePathName, FileMode.OpenOrCreate))
                {
                    responseStream.CopyTo(fstream);
                    fstream.Flush();
                }

                retVal = true;

            }
            catch (Exception ex)
            {

                logger.Error("Error Saving Truv Cert File to " + certFilePathName, ex);
            }

            return retVal;

        }

        public string writeRequestStringToFile(string requestString, string OrderNumber, string orderTag, string DateTimeStamp = null)
        {
            string retVal = null;

            if (DateTimeStamp == null)
            {
                DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            string OrderFilePathName = RepositoryPath + "Documents\\TruvComm\\" + DateTimeStamp + "_" + OrderNumber + "_Truv" + orderTag + "Request.json";
            File.WriteAllText(OrderFilePathName, requestString);
            retVal = OrderFilePathName;

            return retVal;

        }

        public byte[] DecodeFrom64(string toDecode)
        {
            return Convert.FromBase64String(toDecode);
        }

        public void testDownload()
        {
            Dictionary<string, string> addlHeaders = new Dictionary<string, string>() { };

            addlHeaders.Add("X-Access-Client-Id", ClientId);
            addlHeaders.Add("X-Access-Secret", SecretId);
            addlHeaders.Add("Accept", "application/json");

            DownloadBorrowerCertDocument("c1597e7cec9b4d6bab2f866cf8cb6ae9", "1276ee0a29fc48e68911150e8820eabb", @"C:\Temp\truvcertilfe.pdf", addlHeaders);


        }

    }
}
