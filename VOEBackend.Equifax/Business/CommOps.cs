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
using VOEBackend.Equifax.EquifaxSchema;
using VOESystem.Data.DBSchema;
using System.Configuration;
using System.Text.RegularExpressions;

namespace VOEBackend.Equifax.Business
{
    class CommOps : BaseClass
    {

        //private const string EquifaxServiceURL = @"employment-uat.mconnect.equifax.com/talx/InteractionServlet";  //dev url
        private const string EquifaxServiceURL = @"employment.mconnect.equifax.com/talx/InteractionServlet"; //prod url


        public ResponseResult postRequest(IDbConnection dbConn, REQUEST_GROUP request, string OrderNumber, int orderRequestId, string UserName, bool IsInstant, 
            bool IsReverify, bool IsDay1, string testResponse = null)
        {


            ResponseResult resObj = new ResponseResult()
            {
                Status = String.Empty,
                ResultMessage = String.Empty,
                Files = new List<string>() { },
                EquifaxOrderId = null,
                xmlCDATA = null
            };
            string responseString;
            int RequestLogId = 0;
            string RespFilePathName = "";
            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (testResponse == null)
            {

                string postString = serializeRequest(request);

                string EquifaxReqType = request.REQUEST.REQUEST_DATA.VOI_REQUEST.VOI_REQUEST_DATA.VOIReportRequestActionType;
                //write to file, elimiating password
                string orderTag = "EquifaxOrder";
                if (IsInstant)
                {
                    if (IsDay1)
                    {
                        orderTag = "EquifaxDay1InstantOrder";
                    }
                    else
                    {
                        orderTag = "EquifaxInstantOrder";
                    }
                    
                }

                string OrderFilePathName = writeRequestStringToFile(postString, OrderNumber, orderTag, EquifaxReqType, DateTimeStamp);

                //log request to database
                RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, EquifaxReqType, UserName);

                //post to service
                responseString = makeWebRequest(EquifaxServiceURL, WebRequestMethod.POST, "text/xml; encoding=utf-8", true, null,
                    Encoding.UTF8.GetBytes(postString), null);

                //save response to file
                RespFilePathName = RepositoryPath + "Documents\\EquifaxComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + EquifaxReqType + "Response.xml";
                File.WriteAllText(RespFilePathName, responseString);

            } else
            {
                responseString = testResponse;
            }


            //deserialize response
            XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(RESPONSE_GROUP));
            XmlReader xmlRespReader = new XmlTextReader(new StringReader(responseString));
            RESPONSE_GROUP response = (RESPONSE_GROUP)xmlRespSerializer.Deserialize(xmlRespReader);

            //actions based on status
            string fileName = null;
            string filePathName = null;
            string responseStatusCondition = "OK";
            string responseStatusName = null;
            string responseStatusCode = null;
            string responseStatusDescription = null;


            if (response.RESPONSE.STATUS == null)
            {

                //the response containing the VOE document does not contain the status element
                resObj.EquifaxOrderId = response.RESPONSE.RESPONSE_DATA.VOI_RESPONSE.VOIReportIdentifier;
                resObj.Status = "Done";

                List<string> errFiles = new List<string>() { };
                List<string> okFiles = new List<string>() { };
                //CDATA extraction assumes only one XML file
                string xmlCDATA = null;

                //save attached documents to filesystem only
                if (response.RESPONSE.RESPONSE_DATA.VOI_RESPONSE.EMBEDDED_FILE != null)
                {

                    List<RESPONSE_GROUP.Response_.ResponseData.VOIResponse.EmbeddedFile> fileList = response.RESPONSE.RESPONSE_DATA.VOI_RESPONSE.EMBEDDED_FILE.ToList();

                    int iCounter = 1;
                    foreach (RESPONSE_GROUP.Response_.ResponseData.VOIResponse.EmbeddedFile file in fileList)
                    {
                        if (file._Type.ToLower() == "pdf")
                        {
                            //this is the pdf version
                            fileName = DateTimeStamp + "_" + OrderNumber + "_EquifaxOrderCert" + iCounter.ToString() + ".pdf";
                            filePathName = RepositoryPath + "Documents\\EquifaxDocuments\\" + fileName;

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
                        else if (file._Type.ToLower() == "xml")
                        {
                            //this is the xml version
                            if (xmlCDATA == null)
                            {
                                xmlCDATA = file.DOCUMENT;
                            }

                        }

                    }
                }

                resObj.Files = okFiles;
                resObj.xmlCDATA = xmlCDATA;

                if (errFiles.Count > 0)
                {
                    resObj.ResultMessage = "Error saving downloaded files " + String.Join(",", errFiles.ToArray());
                }


            } else { 

                //check for status
                responseStatusCondition = response.RESPONSE.STATUS._Condition;
                responseStatusName =  response.RESPONSE.STATUS._Name;
                responseStatusCode =  response.RESPONSE.STATUS._Code;
                responseStatusDescription = response.RESPONSE.STATUS._Description;


                if (responseStatusCondition.ToUpper() == "ERROR" || responseStatusCondition.ToUpper() == "NACK" || responseStatusCondition.ToUpper() == "REJECTED")
                {
                    resObj.Status = "Error";
                    resObj.ResultMessage = responseStatusDescription;
                }
                else if (responseStatusCondition.ToUpper() == "ACK" || responseStatusCondition == "OK")
                {
                    resObj.Status = "OK";
                    resObj.ResultMessage = "This order has been submitted";
                    resObj.EquifaxOrderId = response.RESPONSE.RESPONSE_DATA.VOI_RESPONSE.VOIReportIdentifier;
                }
                else if (responseStatusCondition.ToUpper() == "COMPLETED" || responseStatusCondition.ToUpper() == "PENDING")
                {
                    resObj.Status = responseStatusCondition.ToTitleCase();
                    resObj.EquifaxOrderId = response.RESPONSE.RESPONSE_DATA.VOI_RESPONSE.VOIReportIdentifier;
                }

            }
           

            //log response to database
            LogServiceResponse(dbConn, orderRequestId, RequestLogId, RespFilePathName, responseStatusCondition, responseStatusName,
                responseStatusCode, responseStatusDescription, filePathName);

            
            return resObj;

        }


        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, string EquifaxRequestType, string UserName)
        {
            int retVal = 1;

            int EquifaxReqTypeId = dbConn.Where<VendorRequestType>(q => q.Name == EquifaxRequestType).FirstOrDefault().Id;
            int vendorId = dbConn.Where<Vendor>(q => q.Name == "Work#").FirstOrDefault().Id;

            VendorRequestLog log = new VendorRequestLog()
            {
                VendorRequestTypeId = EquifaxReqTypeId,
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

        public void LogServiceResponse(IDbConnection dbConn, int orderRequestId, int EquifaxRequestLogId, string ResponseFileName,
            string StatusCondition, string StatusName, string StatusCode, string StatusDescription, string DocumentFilePathName)
        {

            VendorResponseLog log = new VendorResponseLog()
            {
                VendorRequestLogId = EquifaxRequestLogId,
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
            public string Status { get; set; }
            public string ResultMessage { get; set; }
            public List<string> Files { get; set; }
            public string EquifaxOrderId { get; set; }
            public string xmlCDATA { get; set; }
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
                
                logger.Error("Error Saving Equifax File to " + filePathName, ex);
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

        public string writeRequestStringToFile(string requestString, string OrderNumber, string orderTag, string EquifaxReqType, string DateTimeStamp = null)
        {
            string retVal = null;

            if (DateTimeStamp == null)
            {
                DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            }

            string OrderFilePathName = RepositoryPath + "Documents\\EquifaxComm\\" + DateTimeStamp + "_" + OrderNumber + "_" + orderTag + EquifaxReqType + "Request.xml";
            File.WriteAllText(OrderFilePathName, requestString.Replace(PASSWORD, "XXXXXXX").Replace(PASSWORDDAY1, "XXXXXXX"));
            retVal = OrderFilePathName;

            return retVal;

        }

    }
}
