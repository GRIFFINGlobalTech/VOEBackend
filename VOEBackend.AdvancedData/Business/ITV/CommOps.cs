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
using VOEBackend.AdvancedData.Schema.ITV;
using VOESystem.Data.DBSchema;
using System.Configuration;

namespace VOEBackend.AdvancedData.Business.ITV
{
    class CommOps : BaseClass
    {

        private static string ADServiceURL = @"itv.advanceddata.com/voews/webservice.php"; //production url itvws
        //private static string ADServiceURL = @"itvdev.gnetconsulting.com/voews/webservice.php";  //dev url
        private static string ThirdPartyID = @"11";
        //private static string UserName = @"cdesimone@firsthome.com";
        //private static string UserName = @"voe@firsthome.com"; OLD PROD USERNAME 7/13/2017
        private static string UserName = @"voe1";
        //private static string Password = @"Pass-11567";  //cdesimone dev password
        //private static string Password = @"kj26eBB2!f"; //cdesimone prod password
        //private static string Password = @"kj585BB2!f"; //voeuserprod
        //private static string Password = "Pass-101@!"; //voeuserprod new 2017-07-17
        //private static string Password = "KIDe?fzhk1yTB0"; //voeuserprod new 2020-12-15
        //private static string Password = "Fhmcvoe2021%!%!"; //voeuserprod new 2021-07-02
        private static string Password = ConfigurationManager.AppSettings["ADPassword"].ToString();

        //to reset password https://itv.advanceddata.com/index.php

        public ResponseResult postOrderRequest(IDbConnection dbConn, CommWrapper commwrap, int orderRequestId, string UserName)
        {
            return postRequest(dbConn, commwrap, commwrap.Order.ThirdPartyOrderID, orderRequestId, Login.CommOperation.CreateOrder, UserName);
        
        }

        public ResponseResult postStatusRequest(IDbConnection dbConn, CommWrapper commwrap, int orderRequestId, string UserName)
        {
            return postRequest(dbConn, commwrap, commwrap.Order.ThirdPartyOrderID, orderRequestId, Login.CommOperation.CheckStatus, UserName);

        }

        ResponseResult postRequest(IDbConnection dbConn, CommWrapper wrapper, string OrderNumber, int orderRequestId, Login.CommOperation Operation, string UserName)
        {
            
            ResponseResult resObj = new ResponseResult()
            {
                Status = String.Empty,
                ResultMessage = String.Empty,
                ADOrderNumber = String.Empty,
                Files = new List<string>() { }
            };

            wrapper.Login = createLogin(Operation);
            
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(CommWrapper),"");
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
                xmlSerializer.Serialize(xmlWriter, wrapper, nsSerializer);
                postString = textWriter.ToString().Replace("utf-16", "utf-8");
            }

            //update root node since dash in object name is not suported
            postString = postString.Replace("CommWrapper", "ThirdParty-XML");
            string postStringEncoded = EncodeTo64(postString);

            //write to file, elimiating password
            string FileTag = String.Empty;
            if (Operation == Login.CommOperation.CheckStatus)
            {
                FileTag = "Status";
            } 
            else if ( Operation == Login.CommOperation.CreateOrder )
            {
                FileTag = "Create";
            }

            string DateTimeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string OrderFilePathName = RepositoryPath + "Documents\\ADComm\\" + DateTimeStamp + "_" + OrderNumber + "_ADOrder" + FileTag + "Request.xml";
            File.WriteAllText(OrderFilePathName, postString.Replace(Password,"XXXXXXX"));
            
            //log request to database
            int RequestLogId = LogServiceRequest(dbConn, orderRequestId, OrderFilePathName, Operation, UserName);
            
            //post to service
            string responseString = makeWebRequest(ADServiceURL, WebRequestMethod.POST, "application/x-www-form-urlencoded", true, null, 
                Encoding.UTF8.GetBytes("xmlpost=" + HttpUtility.UrlEncode(postStringEncoded)), null);

            //save response to file
            string RespFilePathName = RepositoryPath + "Documents\\ADComm\\" + DateTimeStamp + "_" + OrderNumber + "_ADOrder" + FileTag + "Response.xml";
            File.WriteAllText(RespFilePathName, responseString);
            
            //deserialize response
            XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(ResponseWrapper));
            XmlReader xmlRespReader = new XmlTextReader(new StringReader(responseString.Replace("ITV-XML", "ResponseWrapper")));
            ResponseWrapper response = (ResponseWrapper)xmlRespSerializer.Deserialize(xmlRespReader);

            //determine voe orderid
            resObj.ADOrderNumber = response.Order.VoEOrderID ?? response.Order.VOEOrderID;

            //actions based on status
            string fileName = null;
            string filePathName = null;

            if ( response.Status.Code == "Error")
            {
                resObj.Status = "Error";
                resObj.ResultMessage = isNull(response.Status.Subject, "");
            }
            else if ( response.Status.Subject == "New" )
            {
                resObj.Status = "New";
                resObj.ResultMessage = "This order has been submitted";
            }
            else if ( response.Status.Subject.Contains("Completed") )
            {
                resObj.Status = "Completed";

                List<string> errFiles = new List<string>() { };
                List<string> okFiles = new List<string>() { };

                //save attached document to filesystem only
                if (response.Status.Document != null)
                {  
                    fileName =  DateTimeStamp + "_" + OrderNumber + "_ADOrderCert.pdf";
                    filePathName = RepositoryPath + "Documents\\ADDocuments\\" + fileName;
                    if (SaveDocument(response.Status.Document, filePathName))
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
                //could be in-process or accepted status code
                resObj.Status = response.Status.Subject;
            }

            //log response to database
            LogServiceResponse(dbConn, orderRequestId, RequestLogId, resObj.ADOrderNumber, RespFilePathName, response.Status.Code,
                response.Status.Subject, response.Status.Comments, filePathName);

            
            return resObj;

        }

   
        Login createLogin(Login.CommOperation Operation)
        {
            Login login = new Login();

            login.ThirdPartyID = ThirdPartyID;
            login.ClientID = UserName;
            login.Password = Password;
            login.Timestamp = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            login.Method = Operation.Value;
            login.StatusUpdate = String.Empty;

            return login;
        }

        static public string EncodeTo64(string toEncode)
        {

            byte[] toEncodeAsBytes = System.Text.UTF8Encoding.UTF8.GetBytes(toEncode);
            return System.Convert.ToBase64String(toEncodeAsBytes);

        }

        static public byte[] DecodeFrom64(string toDecode)
        {
            return Convert.FromBase64String(toDecode);
        }

        public int LogServiceRequest(IDbConnection dbConn, int orderRequestId, string RequestFileName, Login.CommOperation Operation, string UserName)
        {
            int retVal = 1;

            int ADReqTypeId = dbConn.Where<VendorRequestType>(q => q.Name == Operation.Value).FirstOrDefault().Id;
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

        public class ResponseResult
        {
            public string Status { get; set; }
            public string ResultMessage { get; set; }
            public string ADOrderNumber { get; set; }
            public List<string> Files { get; set; }
        }

    }
}
