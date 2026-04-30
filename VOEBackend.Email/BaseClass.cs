using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using System.Data;
using System.Configuration;
using VOESystem.Data.DBSchema;
using System.Text.RegularExpressions;
using ServiceStack.ServiceHost;
using VOESystem.Data.Business;

namespace VOEBackend.Email
{
    public class BaseClass
    {

        protected BaseClass()
        {

            Log = new FHMC.NLogWrapper.Logger(GetType().FullName);
        }

        protected static FHMC.NLogWrapper.Logger Log { get; private set; }

        protected static string AttachmentLocalPath = @"C:\Temp\MailAttachments";
        protected static List<string> AllowedEmailAttachmentTypes = ConfigurationManager.AppSettings.Get("AllowedEmailAttachmentTypes").Split(","[0]).ToList();
        protected static string RepositoryPath = ConfigurationManager.AppSettings.Get("DocumentRepositoryProd");

        protected const string VOEEMAILADDRESS = "voe@firsthome.com";

        public enum LoanStatus
        {
            ActiveLoan,
            Applicationapprovedbutnotaccepted,
            Applicationdenied,
            Applicationwithdrawn,
            FileClosedforincompleteness,
            LoanOriginated
        }

        public class AttachmentFile : IFile
        {
            public long ContentLength { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
            public System.IO.Stream InputStream { get; set; }

        }

        public static string isNull(object inString, string replVal)
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

        public static string getSuffix(string FileName, string sepChar)
        {

            if (FileName.Contains(sepChar))
            {
                string revFileName = ReverseString(FileName);
                string revExt = revFileName.Substring(0, revFileName.IndexOf(sepChar));

                return ReverseString(revExt);
            }
            else
            {
                return string.Empty;
            }


        }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        public DateTime getLastDownloadDateTime()
        {
            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            SystemSettings setting = dbConn.Where<SystemSettings>("SettingKey", "LastEmailPickupDateTime").FirstOrDefault();

            return DateTime.Parse(setting.SettingValue);
        }

        public void setLastDownloadDateTime()
        {
            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            SystemSettings setting = new SystemSettings()
            {
                SettingValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };

            dbConn.UpdateOnly(setting,
                q => new { q.SettingValue },
                where: q => q.SettingKey == "LastEmailPickupDateTime");

        }

        public class EmailMatch
        {
            public List<int> OrderRequestIds { get; set; }
            public string LoanNumber { get; set; }
            public List<int> SendBounceReplyIds { get; set; }
        }

        public static EmailMatch getOrderRequest(IDbConnection dbConn, string emailSubject)
        {

            EmailMatch retVal = new EmailMatch();
            retVal.SendBounceReplyIds = new List<int>() { };
            retVal.OrderRequestIds = new List<int>() { };

            try
            {

                string orderRegex = @"\[[0-9]+\-[0-9]{1,2}\]";
                Match match = Regex.Match(emailSubject, orderRegex);
                string loannumber = string.Empty;
                string ordersuffix = string.Empty;
                string ordernumber = string.Empty;

                if (match.Success)
                {
                    //this is an order number
                    ordernumber = match.Value.Replace("[", "").Replace("]", "");
                    loannumber = ordernumber.Substring(0, match.Value.IndexOf("-") - 1);
                    ordersuffix = Int32.Parse(getSuffix(ordernumber, "-")).ToString("00");

                    OrderRequest order = dbConn.Select<OrderRequest>(q =>
                        q.LoanNumber == loannumber &&
                        q.OrderSuffix == ordersuffix).FirstOrDefault();

                    retVal.OrderRequestIds.Add(order.Id);
                    retVal.LoanNumber = loannumber;

                    Dictionary<string, object> prms = new Dictionary<string, object> { { "OrderRequestID", order.Id } };
                    string orderStatus = dbConn.SqlScalar<string>("SELECT dbo.fn_GetCurrentStatusForRequest(@OrderRequestID)", prms);

                    if (orderStatus == "Approved" || orderStatus == "Archived")
                    {
                        retVal.SendBounceReplyIds.Add(order.Id);
                    }
                }
                else
                {
                    //this might be a loan number only
                    orderRegex = @"\[[0-9]+\]";
                    match = Regex.Match(emailSubject, orderRegex);

                    if (match.Success)
                    {

                        loannumber = match.Value.Replace("[", "").Replace("]", "");

                        if (emailSubject.Contains("Confirmation of Closing Date"))
                        {
                            //if this is an order confirmation email then only match this to final orders
                            List<OrderRequest> orders = dbConn.Select<OrderRequest>(q =>
                            q.LoanNumber == loannumber && q.RequestTypeId == 3).ToList();

                            foreach (OrderRequest order in orders)
                            {
                                retVal.OrderRequestIds.Add(order.Id);
                            }
                        }
                        else
                        {
                            //this is a loan-level email (not a final confirm reply)
                            retVal.LoanNumber = loannumber;
                        }

                    }

                }


            }
            catch (Exception ex)
            {
                Log.Error("Error Matching Email: " + emailSubject, ex);
            }

            if (retVal.OrderRequestIds.Count == 0 && retVal.LoanNumber == null)
            {
                throw new Exceptions.EmailNotMatchedException();
            }

            return retVal;

        }

        public static EmailMatch getOrderRequestFromTruvRedirect(IDbConnection dbConn, string emailBody)
        {

            EmailMatch retVal = new EmailMatch();
            retVal.SendBounceReplyIds = new List<int>() { };
            retVal.OrderRequestIds = new List<int>() { };
            string matchURL = String.Empty;

            //this version was for the original html
            //string buttonRegex = @"<a href=""https.*?</a>";
            //Match buttonMatch = Regex.Match(emailBody, buttonRegex);

            //get the "continue" button
            //while (buttonMatch.Success)
            //{

            //    if (!buttonMatch.Value.ToLower().Contains("unsubscribe")) 
            //    {
            //        //this must be the button
            //        string urlRegex = @"https:\/\/.*truv.com";
            //        Match urlMatch = Regex.Match(buttonMatch.Value, urlRegex);

            //        if (urlMatch.Success)
            //        {
            //            matchURL = urlMatch.Value.ToString();
            //        }

            //    }

            //    buttonMatch = buttonMatch.NextMatch();

            //}


            //get the "continue" button
            string buttonRegex = @"Continue <https.*?>";
            Match buttonMatch = Regex.Match(emailBody, buttonRegex);

            if (buttonMatch.Success)
            {

                //this must be the button
                string urlRegex = @"https:\/\/.*truv.com";
                Match urlMatch = Regex.Match(buttonMatch.Value, urlRegex);

                if (urlMatch.Success)
                {
                    matchURL = urlMatch.Value.ToString();
                }

            }

            if (matchURL == String.Empty)
            {
                throw new Exception("Truv Redirect URL Not Found");
            }

            //get order url
            BusinessBase bbo = new BusinessBase();
            int tries = 1;
            string orderURL = null;
            while (tries <= 3)
            {
                try
                {
                    orderURL = bbo.getWebRequestRedirect(matchURL);
                    tries = 4;
                }
                catch (Exception ex)
                {
                    Log.Info("Retrying Redirect URL: " + ex.Message);
                    //retry timeout
                    tries++;
                }
            }

            if (orderURL == null)
            {
                throw new Exception("Truv Redirect Order URL Not Found");
            }

            //get order number
            string orderRegex = @"(?<=order_group_id=).*?(?=&|$)";
            Match orderMatch = Regex.Match(orderURL, orderRegex);
            string truvOrderId = String.Empty;

            if (orderMatch.Success)
            {
                truvOrderId = orderMatch.Value.ToString();
            }

            if (truvOrderId == String.Empty)
            {
                throw new Exception("Truv Order Number Not Found in URL");
            }

            OrderRequest orderRequest = dbConn.Where<OrderRequest>(q => q.TruvOrderNumber == truvOrderId).FirstOrDefault();

            if (orderRequest == null)
            {
                throw new Exception("Order Not Found for Truv Order Number " + truvOrderId);
            }
            
            retVal.OrderRequestIds.Add(orderRequest.Id);

            return retVal;

        }

        public static bool checkAutoReply(string RawHeader)
        {
            bool retVal = false;

            if (RawHeader.ToLower().Contains("auto-submitted"))
            {
                if (RawHeader.ToLower().Contains("auto-generated") ||
                    RawHeader.ToLower().Contains("auto-replied"))
                {

                    retVal = true;
                }
            }

            return retVal;
        }

        public static void runCustomNotificationCheck(IDbConnection dbConn, string fromEmail, string Subject)
        {

            try
            {
                //this is for faxmaker failure
                if (fromEmail.EndsWith("@faxmaker.com") && Subject.StartsWith("Failure"))
                {
                    dbConn.ExecuteNonQuery("EXEC usp_ToastAlert_CreateAlerts");
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error Checking Custom Notifications: " + fromEmail + " " + Subject, ex);
            }

        }

        public static bool isReadReceipt(IDbConnection dbConn, string RawHeader, string Subject, string MessageText, int? OrderRequestId, out int? OrigEmailId)
        {

            bool retVal = false;
            OrigEmailId = null;

            RawHeader = RawHeader.ToLower();

            //this is a ReadReceipt if it contains: multipart/report and disposition-notification and displayed
            //and can be matched to an email in the system
            if (RawHeader.Contains("multipart/report"))
            {
                if (RawHeader.Contains("disposition-notification"))
                {
                    if (RawHeader.Contains("displayed"))
                    {
                        //now figure out which emailid it should me matched to
                        OrigEmailId = matchReadReceipt(dbConn, OrderRequestId, RawHeader);
                        if (OrigEmailId > 0)
                        {
                            retVal = true;
                        }
                    }
                }
            }

            //this is testing for another type of read receipt that just starts with the word "Read:"
            //and also has the words "was read on" in the first 300 characters of the message text
            if (Subject.ToLower().StartsWith("read:"))
            {
                string messageContents = MessageText;
                if (messageContents.Length > 300) { messageContents = messageContents.Substring(0, 300); };

                if (messageContents.ToLower().Contains("was read on"))
                {
                    //now figure out which emailid it should me matched to
                    OrigEmailId = matchReadReceipt(dbConn, OrderRequestId, RawHeader);
                    if (OrigEmailId > 0)
                    {
                        retVal = true;
                    }
                }

            }

            return retVal;

        }

        public static int matchReadReceipt(IDbConnection dbConn, int? OrderRequestId, string RawHeader)
        {

            int retVal = 0;

            //look at emails that were sent, have a messageid and belong to this order
            List<VOESystem.Data.DBSchema.Email> emails = dbConn.Where<VOESystem.Data.DBSchema.Email>(q => q.OrderRequestId == OrderRequestId
                && q.ExchangeUID != null && q.DateTimeSent != null).ToList();

            foreach (VOESystem.Data.DBSchema.Email email in emails)
            {
                if (RawHeader.Contains(email.ExchangeUID.ToLower()))
                {
                    retVal = email.Id;
                }

            }

            return retVal;

        }

        public static void sendBounceReplys(IDbConnection dbConn, int emailId, List<int> orderIds, string voeSystemEmailAddress)
        {

            if (orderIds.Count > 0)
            {
                VOESystem.Data.Business.EmailOps eOp = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.DTO.Email replyEmail = eOp.getEmailForReply(dbConn, emailId, false, null, "voesystem");
                VOESystem.Data.DBSchema.EmailTemplate tmpRecord = dbConn.Where<VOESystem.Data.DBSchema.EmailTemplate>(q => q.Name == "Approved Email Bounce Reply").FirstOrDefault();
                VOESystem.Data.DTO.EmailTemplate replyTemplate = eOp.getEmailTemplate(dbConn, tmpRecord);

                foreach (int orderRequestId in orderIds)
                {

                    Dictionary<string, string> loanDataVals = eOp.getLoanDataForEmailTemplate(dbConn, orderRequestId, null);
                    VOESystem.Data.DTO.Email templateEmail = eOp.generateEmail(dbConn, replyTemplate, loanDataVals, true, "voesystem", null);

                    //consolidate values from both email objects
                    replyEmail.Subject = templateEmail.Subject + " " + replyEmail.Subject;
                    replyEmail.Message = templateEmail.Message + "\r\n\r\n" + replyEmail.Message;
                    replyEmail.ToRecipientList.AddRange(templateEmail.ToRecipientList);
                    replyEmail.CcRecipientList.AddRange(templateEmail.CcRecipientList);
                    replyEmail.BccRecipientList.AddRange(templateEmail.BccRecipientList);
                    replyEmail.ReplyToRecipientList.AddRange(templateEmail.ReplyToRecipientList);

                    replyEmail.ToRecipientList = replyEmail.ToRecipientList.Where(q => q.EmailAddress.ToLower() != voeSystemEmailAddress.ToLower()).ToList();
                    replyEmail.CcRecipientList = replyEmail.CcRecipientList.Where(q => q.EmailAddress.ToLower() != voeSystemEmailAddress.ToLower()).ToList();
                    replyEmail.BccRecipientList = replyEmail.BccRecipientList.Where(q => q.EmailAddress.ToLower() != voeSystemEmailAddress.ToLower()).ToList();
                    replyEmail.ReplyToRecipientList = replyEmail.ReplyToRecipientList.Where(q => q.EmailAddress.ToLower() != voeSystemEmailAddress.ToLower()).ToList();

                    eOp.sendEmailFromTemplate(dbConn, replyEmail, orderRequestId, null, true, false, false);

                }

            }


        }

        public static void runOOOReplyCheck(IDbConnection dbConn, int OrderRequestId)
        {

            string assignedUserName = dbConn.SqlScalar<string>(String.Format("SELECT dbo.fn_GetUserForRequest({0})", OrderRequestId.ToString()));

            emdbUserInfoView assignedUser = dbConn.Where<emdbUserInfoView>(q => q.UserName == assignedUserName).FirstOrDefault();

            bool sendOOOReply = false;

            if (assignedUser != null)
            {
                sendOOOReply = assignedUser.IsOOO;
            }

            if (sendOOOReply)
            {
                VOESystem.Data.Business.EmailOps eOp = new VOESystem.Data.Business.EmailOps();
                eOp.sendTemplateEmail(dbConn, "Specialist Out of Office Notification", OrderRequestId, null, true, false, null, false);
            }

        }

        


    }

    public static class CustomExtensions
    {

        public static string ToJson<T>(this object thing) {

            return Newtonsoft.Json.JsonConvert.SerializeObject(thing);

        }
        
     }
    

}
