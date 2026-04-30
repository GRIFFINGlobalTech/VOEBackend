using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using ServiceStack.OrmLite;
using VOEBackend.Xactus.Schema;
using VOESystem.Data.DBSchema;
using System.Configuration;
using System.Text.RegularExpressions;

namespace VOEBackend.Xactus.Business
{
    class CommOps : BaseClass
    {



        public ResponseResult postRequest(IDbConnection dbConn, REQUEST_GROUP request, string OrderNumber, int orderRequestId, string UserName, bool IsInstant, 
            bool IsReverify, bool IsDay1, SubVendor subVendor, string testResponse = null, string testResponseFile = null)
        {


            ResponseResult resObj = new ResponseResult();
            
            string responseString;
            int RequestLogId = 0;
            string RespFilePathName = "";
            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string subVendorName = subVendor.ToString();
            string orderTag = "";

            if (testResponse == null)
            {

                if (testResponseFile == null) { 

                    string postString = serializeRequest(request);

                    string XactusReqType = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.CREDIT_REQUEST_DATA.CreditReportRequestActionType;

                    //write to file, elimiating password
                    orderTag = "Xactus" + subVendorName + "Order";
                    if (IsInstant)
                    {
                        if (IsDay1)
                        {
                            orderTag = "Xactus" + subVendorName + "Day1InstantOrder";
                        }
                        else if (IsReverify)
                        {
                            orderTag = "Xactus" + subVendorName + "ReverifyOrder";
                        }
                        else
                        {
                            orderTag = "Xactus" + subVendorName + "InstantOrder";
                        }

                    }

                    string OrderFilePathName = writeRequestStringToFile(postString, OrderNumber, orderTag, XactusReqType, DateTimeStamp);

                    //log request to database
                    RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, XactusReqType, subVendor, UserName);

                    //post to service
                    responseString = makeWebRequest(XactusServiceURL, WebRequestMethod.POST, "text/xml; encoding=utf-8", true, null,
                        Encoding.UTF8.GetBytes(postString), null);

                    //save response to file
                    RespFilePathName = RepositoryPath + "Documents\\XactusComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + XactusReqType + "Response.xml";
                    File.WriteAllText(RespFilePathName, responseString);

                }
                else
                {
                    responseString = File.ReadAllText(testResponseFile);

                }

                //deserialize response
                XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(RESPONSE_GROUP));
                XmlReader xmlRespReader = new XmlTextReader(new StringReader(responseString));
                RESPONSE_GROUP response = (RESPONSE_GROUP)xmlRespSerializer.Deserialize(xmlRespReader);

                //actions based on status
                string fileName = null;
                string filePathName = null;
                string responseStatusCondition = "OK";


                if (response.RESPONSE.STATUS != null)
                {

                    if (response.RESPONSE.STATUS._Condition == "Successful")
                    {
                        //the response containing the VOE document does not contain the status element
                        resObj.XactusOrderId = response.RESPONSE.RESPONSE_DATA.EXTENSION.EXTENSION_SECTION.EXTENSION_SECTION_DATA.VERIFICATION_RESPONSE.VendorOrderIdentifier;
                        resObj.Status = "Done";

                        if (response.RESPONSE.RESPONSE_DATA.CREDIT_RESPONSE.BORROWER.EMPLOYER != null)
                        {
                            foreach (RESPONSE_GROUP.Response_.ResponseData.CreditResponse.Borrower.Employer emp in response.RESPONSE.RESPONSE_DATA.CREDIT_RESPONSE.BORROWER.EMPLOYER)
                            {
                                resObj.Employers.Add(new ResponseResult.Employer
                                {
                                    EmployerName = emp._Name,
                                    EmployeeStatus = emp.EmployeeStatus
                                });

                            }
                        }

                        List<string> errFiles = new List<string>() { };
                        List<string> okFiles = new List<string>() { };

                        //save attached documents to filesystem only
                        if (response.RESPONSE.RESPONSE_DATA.EMORTGAGE_PACKAGE.EMBEDDED_FILE != null)
                        {

                            List<RESPONSE_GROUP.Response_.ResponseData.EMortgage_Package.Embedded_File> fileList = response.RESPONSE.RESPONSE_DATA.EMORTGAGE_PACKAGE.EMBEDDED_FILE.ToList();

                            int iCounter = 1;
                            foreach (RESPONSE_GROUP.Response_.ResponseData.EMortgage_Package.Embedded_File file in fileList)
                            {
                                if (file._Type.ToLower() == "pdf")
                                {
                                    //this is the pdf version
                                    fileName = DateTimeStamp + "_" + OrderNumber + "_" + orderTag + "Cert" + iCounter.ToString() + ".pdf";
                                    filePathName = RepositoryPath + "Documents\\XactusDocuments\\" + fileName;

                                    if (SaveDocument(file.DOCUMENT, filePathName))
                                    {
                                        okFiles.Add(fileName);
                                    }
                                    else
                                    {
                                        errFiles.Add(fileName);
                                    }
                                    iCounter++;
                                }

                            }
                        }

                        resObj.Files = okFiles;

                        if (errFiles.Count > 0)
                        {
                            resObj.ResultMessage = "Error saving downloaded files " + String.Join(",", errFiles.ToArray());
                        }

                    }
                    else
                    {

                        //check for status
                        responseStatusCondition = response.RESPONSE.STATUS._Condition;
                    
                        resObj.Status = "Error";
                        resObj.ResultMessage = responseStatusCondition;

                    }

                }

                //log response to database
                LogServiceResponse(dbConn, orderRequestId, RequestLogId, RespFilePathName, responseStatusCondition, resObj.Status,
                    null, null, filePathName);

            }
            else
            {
                resObj.Status = testResponse;
            }

            
            return resObj;

        }


        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, string XactusRequestType, SubVendor subVendor, string UserName)
        {
            int retVal = 1;

            string subVendorName = null;

            int vendorId = dbConn.Where<Vendor>(q => q.Name == "Xactus").FirstOrDefault().Id;

            if (subVendor == SubVendor.TWN)
            {
                subVendorName = "TWN";
            }
            else if (subVendor == SubVendor.Experian)
            {
                subVendorName = "Experian";
            }

            int XactusReqTypeId = dbConn.Where<VendorRequestType>(q => q.Name == subVendorName && q.VendorId == vendorId).FirstOrDefault().Id;

            VendorRequestLog log = new VendorRequestLog()
            {
                VendorRequestTypeId = XactusReqTypeId,
                OrderRequestId = orderRequestId,
                RequestDateTime = DateTime.Now,
                RequestFileName = RequestFileName,
                UserName = UserName,
                VendorId = vendorId
            };

            dbConn.Insert<VendorRequestLog>(log);
            retVal = (int)dbConn.GetLastInsertId();

            return retVal;

        }

        public void LogServiceResponse(IDbConnection dbConn, int orderRequestId, int XactusRequestLogId, string ResponseFileName,
            string StatusCondition, string StatusName, string StatusCode, string StatusDescription, string DocumentFilePathName)
        {

            VendorResponseLog log = new VendorResponseLog()
            {
                VendorRequestLogId = XactusRequestLogId,
                OrderRequestId = orderRequestId,
                ResponseDateTime = DateTime.Now,
                ResponseFileName = ResponseFileName,
                StatusCode = StatusCondition, 
                StatusSubject = StatusName,
                StatusComments = StatusDescription,
                DocumentFileName = DocumentFilePathName
            };

            dbConn.Insert<VendorResponseLog>(log);

        }

        public class ResponseResult
        {

            public ResponseResult()
            {
                Status = String.Empty;
                ResultMessage = String.Empty;
                Files = new List<string>() { };
                XactusOrderId = null;
                Employers = new List<Employer>() { };
            }

            public string Status { get; set; }
            public string ResultMessage { get; set; }
            public List<string> Files { get; set; }
            public string XactusOrderId { get; set; }
            public List<Employer> Employers { get; set; }

            public class Employer
            {
                public string EmployerName { get; set; }
                public string EmployeeStatus { get; set; }
            }
        }

        public bool SaveDocument(string file, string filePathName)
        {
            bool retVal = false;

            byte[] fileContents = DecodeFrom64(file.Trim());

            try
            {
                if (File.Exists(filePathName))
                {
                    File.Delete(filePathName);
                }

                File.WriteAllBytes(filePathName, fileContents);

                retVal = true;

            }
            catch (Exception ex)
            {
                
                logger.Error("Error Saving Xactus File to " + filePathName, ex);
            }

            return retVal;

        }

        static public byte[] DecodeFrom64(string toDecode)
        {
            return Convert.FromBase64String(toDecode);
        }

        public string serializeRequest(REQUEST_GROUP request)
        {

            string retVal = null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(REQUEST_GROUP), "");
            string postString = String.Empty;

            StringWriter textWriter = new StringWriter();

            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter,
                              new XmlWriterSettings()
                              {
                                  OmitXmlDeclaration = false,
                                  ConformanceLevel = ConformanceLevel.Auto,
                                  NewLineHandling = NewLineHandling.Replace,
                                  NewLineChars = ""
                              }))
            {
                var nsSerializer = new XmlSerializerNamespaces();
                nsSerializer.Add("", "");
                xmlSerializer.Serialize(xmlWriter, request, nsSerializer);
                retVal = textWriter.ToString().Replace("utf-16", "utf-8");
            }

            return retVal;
        }

        public string writeRequestStringToFile(string requestString, string OrderNumber, string orderTag, string XactusReqType, string DateTimeStamp = null)
        {
            string retVal = null;

            if (DateTimeStamp == null)
            {
                DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            string OrderFilePathName = RepositoryPath + "Documents\\XactusComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + XactusReqType + "Request.xml";
            File.WriteAllText(OrderFilePathName, requestString.Replace(PASSWORD, "XXXXXXX"));
            retVal = OrderFilePathName;

            return retVal;

        }

    }
}
