using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using ServiceStack.OrmLite;
using VOEBackend.TrueWork.TrueWorkSchema;
using VOESystem.Data.DBSchema;


namespace VOEBackend.TrueWork.Business
{
    class CommOps : BaseClass
    {

        //private const string TrueworkServiceURL = @"api.truework-sandbox.com";  //dev url
        //private const string apiKey = @"tw_sk_test_9ebde60b2c9ef718413b7deae80ed561c4a6d6d4";  //dev api key

        //private const string TrueworkServiceURL = @"api.truework.com";  //internal testing url
        //private const string apiKey = @"tw_sk_5052cbc3890c965378fbf10d101b7152cbb2ba6f";  //internal testing url

        private const string TrueworkServiceURL = @"api.truework.com";  //prod url
        private const string apiKey = @"tw_sk_5732f3ffc91b276ea3ad8b38631786ed5873613b";  //prod api key


        public enum TrueWorkCommType
        {

            CreateInstant,
            CreateReverify,
            QueryReverify,
            CreateCredentials,
            QueryCredentials,
            CreateReverifyCredentials,
            QueryReverifyCredentials
        }

        public ResponseResult postRequest(IDbConnection dbConn, Request request, string OrderNumber, int orderRequestId, string UserName, bool IsInstant,
            TrueWorkCommType trueworkCommType, bool IsDay1, string VerificationId = null, string testResponse = null)
        {


            ResponseResult resObj = new ResponseResult()
            {
                Status = String.Empty,
                Files = new List<ResponseResult.ReportFile>() { },
                TrueworkOrderId = null
            };

            string responseString;
            int RequestLogId = 0;
            string RespFilePathName = "";
            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (testResponse == null)
            {

                if (trueworkCommType == TrueWorkCommType.CreateInstant)
                {
                    if (TrueworkServiceURL.ToLower().Contains("sandbox") && !request.target.social_security_number.ToString().StartsWith("000"))
                    {
                        request.target.social_security_number = "000-10-0000";
                    }
                }

                string postString = JsonConvert.SerializeObject(request, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

                //write to file, elimiating password
                string orderTag = trueworkCommType.ToString();
                string OrderFilePathName = writeRequestStringToFile(postString, OrderNumber, orderTag, DateTimeStamp);

                //log request to database
                RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, orderTag, UserName);

                //post to service
                Dictionary<string, string> addlHeaders = new Dictionary<string, string>() { };
                addlHeaders.Add("Authorization", "Bearer " + apiKey);
                addlHeaders.Add("Accept", "application/json; version=2022-08-01");

                if (IsInstant)
                {
                    addlHeaders.Add("Request-Sync", "sync");
                }
                else
                {
                    addlHeaders.Add("Request-Sync", "async");
                }

                string opEndpoint = null;
                WebRequestMethod method = WebRequestMethod.GET;

                if (trueworkCommType == TrueWorkCommType.CreateInstant || trueworkCommType == TrueWorkCommType.CreateCredentials)
                {
                    opEndpoint = "/verification-requests/";
                    method = WebRequestMethod.POST;
                }
                else if (trueworkCommType == TrueWorkCommType.CreateReverify || trueworkCommType == TrueWorkCommType.CreateReverifyCredentials)
                {
                    if (VerificationId == null || request.report_id == null)
                    {
                        throw new Exception("Missing Truework VerficationId/ReportId for Reverify");
                    }
                    opEndpoint = "/verification-requests/" + VerificationId + "/reverify/";
                    method = WebRequestMethod.PUT;
                }
                else if (trueworkCommType == TrueWorkCommType.QueryReverify || trueworkCommType == TrueWorkCommType.QueryCredentials || trueworkCommType == TrueWorkCommType.QueryReverifyCredentials)
                {
                    opEndpoint = "/verification-requests/" + VerificationId;
                    method = WebRequestMethod.GET;
                }





                byte[] content = null;
                if (request != null)
                {
                    content = Encoding.UTF8.GetBytes(postString);
                }

                //responseString = "d";
                //try
                //{
                    responseString = makeWebRequest(TrueworkServiceURL + opEndpoint, method, "application/json; version=2022-08-01", true, null,
                        content, addlHeaders);
                //}
                //catch (WebException ex)
                //{
  
                //    Stream oStream = ((HttpWebResponse)ex.Response).GetResponseStream();

                //    using (StreamReader reader = new StreamReader(oStream, Encoding.UTF8))
                //    {
                //        string contentString = reader.ReadToEnd();
                //        //if (responseStream == null)
                //        //{
                //        //    responseStream = new MemoryStream();
                //        //    byte[] responseBytes = Encoding.UTF8.GetBytes(contentString);
                //        //    responseStream.Write(responseBytes, 0, responseBytes.Length);
                //        //    responseStream.Seek(0, System.IO.SeekOrigin.Begin);
                //        //}

                //        //objErr = JsonConvert.DeserializeObject<EncRESTSchema.Error>(contentString);
                //        //errMessage = objErr.error_description ?? objErr.details;
                //    };

                //}

                //save response to file
                RespFilePathName = RepositoryPath + "Documents\\TrueworkComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + "Response.json";
                File.WriteAllText(RespFilePathName, responseString);
            }
            else
            {
                responseString = testResponse;
            }


            //deserialize response
            Response response = JsonConvert.DeserializeObject<Response>(responseString);

            //actions based on status
            string filePathName = null;
            string responseStatusCode = response.state;
            string responseStatusSubject = null;
            string responseStatusComments = null;
          
            if (responseStatusCode == "completed")
            {

                //the response containing the rVOE document does not contain the status element
                resObj.TrueworkOrderId = response.id;
                resObj.Status = "Done";

                List<ResponseResult.ReportFile> errFiles = new List<ResponseResult.ReportFile>() { };
                List<ResponseResult.ReportFile> okFiles = new List<ResponseResult.ReportFile>() { };

                //Request Documents
                int iCounter = 1;
                foreach (Response.Report report in response.reports)
                {
                    //save files
                    string fileName = String.Empty;
                    string fileTypeTag = trueworkCommType.ToString().Replace("Query","") + "Cert";                   

                    if (IsDay1)
                    {
                        fileTypeTag = "Day1OrderCert";
                    }
                        
                    fileName = DateTimeStamp + "_" + OrderNumber + "_TrueWork" + fileTypeTag + iCounter.ToString() + ".pdf";
                    filePathName = RepositoryPath + "Documents\\TrueWorkDocuments\\" + fileName;

                    if (DownloadDocument(response.id, report.id, filePathName))
                    {

                        DateTime? currDate = null;
                        if(report.current_as_of != null)
                        {
                            currDate = DateTime.Parse(report.current_as_of);
                        }

                        string employerName = null;
                        string employeeStatus = null;

                        if (report.employer != null)
                        {
                            employerName = report.employer.name;
                        }

                        if (report.employee != null)
                        {
                            employeeStatus = report.employee.status;
                        }

                        string duReferenceNumber = null;

                        if (report.du_reference_id != null)
                        {
                            duReferenceNumber = report.du_reference_id;
                        }

                        okFiles.Add(new ResponseResult.ReportFile {
                            FileName = fileName,
                            ReportId = report.id,
                            RequestId = response.id,
                            DataDate = currDate,
                            EmployerName = employerName,
                            EmployeeStatus = employeeStatus,
                            DUReferenceNumber = duReferenceNumber
                        });

                    }
                    else
                    {
                        errFiles.Add(new ResponseResult.ReportFile
                        {
                            FileName = fileName
                        });
                    }
                    iCounter++;
                }

                resObj.Files = okFiles;

                if (errFiles.Count > 0)
                {
                    resObj.ResultMessage = "Error saving downloaded files " + String.Join(",", errFiles.Select<ResponseResult.ReportFile, string>(q => q.FileName).ToArray());
                }
            }
            else if (responseStatusCode == "processing" || responseStatusCode == "pending-approval")
            {
                //just submitted a request for a reverify or is a status query
                resObj.TrueworkOrderId = response.id;
                resObj.Status = "Processing";
            }
            else if (responseStatusCode == "canceled")
            {
                resObj.Status = "Cancelled";
            }

            //log response to database
            LogServiceResponse(dbConn, orderRequestId, RequestLogId, RespFilePathName, responseStatusCode, responseStatusSubject,
            responseStatusComments, filePathName);

            return resObj;

        }

        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, string OrderTag, string UserName)
        {
            int retVal = 1;

            int VendorRequestTypeId = dbConn.Where<VendorRequestType>(q => q.Name == OrderTag).FirstOrDefault().Id;
            int VendorId = dbConn.Where<Vendor>(q => q.Name == "TrueWork").FirstOrDefault().Id;

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
            string StatusCode, string StatusSubject, string StatusComments, string DocumentFilePathName)
        {

            VendorResponseLog log = new VendorResponseLog()
            {
                VendorRequestLogId = VendorRequestLogId,
                OrderRequestId = orderRequestId,
                ResponseDateTime = DateTime.Now,
                ResponseFileName = ResponseFileName,
                StatusCode = StatusCode,
                StatusSubject = StatusSubject,
                StatusComments = StatusComments,
                DocumentFileName = DocumentFilePathName
            };

            dbConn.Insert<VendorResponseLog>(log);

        }

        public class ResponseResult
        {
            public string Status { get; set; }
            public List<ReportFile> Files { get; set; }
            public string TrueworkOrderId { get; set; }
            public string ResultMessage { get; set; }
            
            public class ReportFile
            {
                public string FileName { get; set; }
                public string ReportId { get; set; }
                public string RequestId { get; set; }
                public DateTime? DataDate { get; set; }
                public string EmployerName { get; set; }
                public string EmployeeStatus { get; set; }
                public string DUReferenceNumber { get; set; }
            }

        }
        
        public bool DownloadDocument(string requestId, string reportId, string filePathName)
        {
            bool retVal = false;

            string opEndpoint = "/verification-requests/" + requestId + "/reports/" + reportId;

            Dictionary<string, string> addlHeaders = new Dictionary<string, string>() { };
            addlHeaders.Add("Authorization", "Bearer " + apiKey);
            addlHeaders.Add("Accept", "application/pdf");

            Stream file = makeWebRequestStream(TrueworkServiceURL + opEndpoint, WebRequestMethod.GET, "application/json", true, null, null, addlHeaders);

            //byte[] fileContents = DecodeFrom64(file.Trim());
           
            try
            {
                if (File.Exists(filePathName))
                {
                    File.Delete(filePathName);
                }

                using (Stream outStream = File.OpenWrite(filePathName))
                {
                    file.CopyTo(outStream);
                }

                retVal = true;

            }
            catch (Exception ex)
            {

                logger.Error("Error Saving TrueWork File to " + filePathName, ex);
            }

            return retVal;

        }

        static public byte[] DecodeFrom64(string toDecode)
        {
            return Convert.FromBase64String(toDecode);
        }

        public string writeRequestStringToFile(string requestString, string OrderNumber, string orderTag, string DateTimeStamp = null)
        {
            string retVal = null;

            if (DateTimeStamp == null)
            {
                DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            string OrderFilePathName = RepositoryPath + "Documents\\TrueworkComm\\" + DateTimeStamp + "_" + OrderNumber + "_TrueWork" + orderTag + "Request.json";
            //File.WriteAllText(OrderFilePathName, requestString.Replace(PASSWORD, "XXXXXXX").Replace(PASSWORDDAY1, "XXXXXXX"));
            File.WriteAllText(OrderFilePathName, requestString);
            retVal = OrderFilePathName;

            return retVal;

        }


    }
}

