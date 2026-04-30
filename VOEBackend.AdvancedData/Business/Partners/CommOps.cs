using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ServiceStack.OrmLite;
using VOEBackend.AdvancedData.Schema.Partners;
using VOESystem.Data.DBSchema;
using static VOEBackend.AdvancedData.Business.Partners.OrderOps;

namespace VOEBackend.AdvancedData.Business.Partners
{
    public class CommOps : BaseClass
    {

        public class ResponseResult
        {
            public string Status { get; set; }
            public string ResultMessage { get; set; }
            public string ADOrderNumber { get; set; }
            public List<string> Files { get; set; }
        }


        public ResponseResult postRequest(IDbConnection dbConn, REQUEST_GROUP wrapper, string OrderNumber, int orderRequestId, CommOperation operation, string UserName)
        {

            ResponseResult resObj = new ResponseResult()
            {
                Status = String.Empty,
                ResultMessage = String.Empty,
                ADOrderNumber = String.Empty,
                Files = new List<string>() { }
            };

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(REQUEST_GROUP), "");
            string postString = String.Empty;

            StringWriter textWriter = new StringWriter();

            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter,
                              new XmlWriterSettings()
                              {
                                  OmitXmlDeclaration = true,
                                  ConformanceLevel = ConformanceLevel.Auto,
                                  NewLineHandling = NewLineHandling.Replace,
                                  NewLineChars = ""
                              }))
            {
                var nsSerializer = new XmlSerializerNamespaces();
                nsSerializer.Add("", "");
                xmlSerializer.Serialize(xmlWriter, wrapper, nsSerializer);
                postString = textWriter.ToString();
            }

            //string postStringEncoded = EncodeTo64(postString);

            //write to file, eliminating password
            string FileTag = operation.ToString();

            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string OrderFilePathName = RepositoryPath + "Documents\\ADComm\\" + DateTimeStamp + "_" + OrderNumber + "_ADOrder" + FileTag + "Request.xml";
            File.WriteAllText(OrderFilePathName, postString.Replace(PASSWORD, "XXXXXXX"));

            //log request to database
            int RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, operation, UserName);

            //post to service
            string responseString = makeWebRequest(ADURL, WebRequestMethod.POST, "application/xml", true, null, Encoding.UTF8.GetBytes(postString), null);

            //save response to file
            string RespFilePathName = RepositoryPath + "Documents\\ADComm\\" + DateTimeStamp + "_" + OrderNumber + "_ADOrder" + FileTag + "Response.xml";
            File.WriteAllText(RespFilePathName, responseString);

            //deserialize response
            XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(RESPONSE_GROUP));
            XmlReader xmlRespReader = new XmlTextReader(new StringReader(responseString));
            RESPONSE_GROUP response = (RESPONSE_GROUP)xmlRespSerializer.Deserialize(xmlRespReader);

            //determine voe orderid
            resObj.ADOrderNumber = response.RESPONSE.RESPONSE_DATA.EXTENSION.EXTENSION_SECTION.EXTENSION_SECTION_DATA.VERIFICATION_RESPONSE.VendorOrderIdentifier;

            //actions based on status
            string fileName = null;
            string filePathName = null;

            if (response.RESPONSE.RESPONSE_DATA.STATUS._Condition == "Error")
            {
                resObj.Status = "Error";
                resObj.ResultMessage = isNull(response.RESPONSE.RESPONSE_DATA.STATUS._Description, "");
            }
            else if (response.RESPONSE.RESPONSE_DATA.STATUS._Condition == "Pending")
            {
                resObj.Status = "Pending";
                resObj.ResultMessage = "This order has been submitted";
            }
            else if (response.RESPONSE.RESPONSE_DATA.STATUS._Condition == "Success")
            {
                resObj.Status = "Completed";

                List<string> errFiles = new List<string>() { };
                List<string> okFiles = new List<string>() { };

                //save attached document to filesystem only
                if (response.RESPONSE.RESPONSE_DATA.EMORTGAGE_PACKAGE.EMBEDDED_FILE != null)
                {
                    fileName = DateTimeStamp + "_" + OrderNumber + "_ADOrderCert.pdf";
                    filePathName = RepositoryPath + "Documents\\ADDocuments\\" + fileName;
                    if (SaveDocument(response.RESPONSE.RESPONSE_DATA.EMORTGAGE_PACKAGE.EMBEDDED_FILE.FirstOrDefault().DOCUMENT, filePathName))
                    {
                        okFiles.Add(fileName);
                    }
                    else
                    {
                        errFiles.Add(fileName);
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
                throw new Exception("Unsupported AD Partners Response: " + response.RESPONSE.RESPONSE_DATA.STATUS._Condition);
            }

            //log response to database
            LogServiceResponse(dbConn, orderRequestId, RequestLogId, resObj.ADOrderNumber, RespFilePathName, response.RESPONSE.RESPONSE_DATA.STATUS._Condition,
               null, response.RESPONSE.RESPONSE_DATA.STATUS._Description, filePathName);


            return resObj;

        }

        static public byte[] DecodeFrom64(string toDecode)
        {
            return Convert.FromBase64String(toDecode);
        }

        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, CommOperation Operation, string UserName)
        {
            int retVal = 1;

            int ADReqTypeId = dbConn.Where<VendorRequestType>(q => q.Name == Operation.ToString()).FirstOrDefault().Id;
            int vendorId = dbConn.Where<Vendor>(q => q.Name == "Advanced Data").FirstOrDefault().Id;

            VendorRequestLog log = new VendorRequestLog()
            {
                VendorRequestTypeId = ADReqTypeId,
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

        public void LogServiceResponse(IDbConnection dbConn, int orderRequestId, int ADRequestLogId, string ADVOEOrderId, string ResponseFileName,
            string StatusCode, string StatusSubject, string StatusComments, string DocumentFilePathName)
        {

            VendorResponseLog log = new VendorResponseLog()
            {

                VendorVOEOrderId = ADVOEOrderId,
                VendorRequestLogId = ADRequestLogId,
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
                logger.Error("Error Saving AD File to " + filePathName, ex);
            }

            return retVal;

        }

    }
}
