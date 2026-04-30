using Microsoft.Graph;
using Microsoft.Identity.Client;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VOEBackend.Email.Exceptions;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

//namespace VOEBackend.Email
//{
//    public class Office365 : BaseClass
//    {

        
//        private static string ClientId = "f5e831ca-ac9a-4526-80aa-9b26a7d75aa8";
//        private static string ClientSecret = @"Z018Q~pMFA4BW1eIDcn57OpISBBg.DnvdwVC6cRm";
//        private static string TenantId = "5b3538ca-e4fc-4828-a305-8e9c303853bd";
//        //private static string CertificateFileName = @"";
//        private static string CertificateName = "FirstHome VOESystem OOO Cert";
//        private static string graphUrl = @"https://graph.microsoft.com/v1.0/";


//        private static List<string> WellKnownFolderNames = new List<string>(){ "recoverableitemsdeletions" };

//        GraphServiceClient graphClient = null;

//        private enum WebMethod
//        {
//            GET,
//            POST,
//            DELETE
//        }

//        private enum MailMessageAttributes
//        {
//            receivedDateTime,
//            subject,
//            toRecipients,
//            ccRecipients,
//            internetMessageHeaders,
//            from,
//            body
//        }

//        public enum LoanStatus
//        {
//            ActiveLoan,
//            Applicationapprovedbutnotaccepted,
//            Applicationdenied,
//            Applicationwithdrawn,
//            FileClosedforincompleteness,
//            LoanOriginated
//        }

//        public Office365()
//        {
//            //create initial graph client
//            setGraphClient();
//        }

//        public void setGraphClient()
//        {

//            TokenHandler tHand = new TokenHandler();
//            string accessToken = tHand.getToken();

//            graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
//            {
//                requestMessage
//                    .Headers
//                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

//                return Task.FromResult(0);
//                }));

           
//        }

//        public List<VOESystem.Data.DBSchema.emdbUserInfoView> getOOOStatus(string[] emailAddresses = null)
//        {

//            List<VOESystem.Data.DBSchema.emdbUserInfoView> retVal = new List<VOESystem.Data.DBSchema.emdbUserInfoView>() { };

//            IGraphServiceUsersCollectionPage users = graphClient.Users.Request().GetAsync().Result;

//            while (users != null)
//            {

//                //make request
//                foreach (Microsoft.Graph.User user in users)
//                {

//                    bool isOOO = false;

//                    try
//                    {
//                        //first get mailsettings object
//                        MailboxSettings mailSettings = getMailboxSettings(user.Id);

//                        //then extract status
//                        string oooStatus = getAutoReplyStatus(mailSettings);

//                        if (oooStatus == null)
//                        {
//                            //there is not currently an ooo
//                        }
//                        else if (oooStatus == "disabled")
//                        {
//                            //there is not currently an ooo
//                        }
//                        else
//                        {
//                            //there is curently an ooo
//                            isOOO = true;
//                        }

//                        Log.Info(user.Mail + ":" + isOOO.ToString());

//                        if (user.Mail != null)
//                        {
//                            retVal.Add(new VOESystem.Data.DBSchema.emdbUserInfoView
//                            {
//                                Email = user.Mail.Replace("'", ""),
//                                IsOOO = isOOO
//                            });
//                        }
//                    }
//                    catch (Exception ex)
//                    {
//                        Log.Error("GraphAPI Error", ex);
//                    }


//                }

//                if (users != null)
//                {
//                    if (users.NextPageRequest != null)
//                    {
//                        users = users.NextPageRequest.GetAsync().Result;
//                    }
//                    else
//                    {
//                        users = null;
//                    }
//                }

//            }

//            return retVal;
   
//        }

//        public string getAutoReplyStatus(MailboxSettings mailSettings)
//        {
//            string retVal = null;

//            if (mailSettings != null)
//            {
//                if (mailSettings.AutomaticRepliesSetting != null)
//                {
//                    if (mailSettings.AutomaticRepliesSetting.Status != null)
//                    {
                       
//                       retVal = mailSettings.AutomaticRepliesSetting.Status.Value.ToString().ToLower();
                                                  
//                    }
                    
//                }
//            }

//            return retVal;

//        }

//        public MailboxSettings getMailboxSettings(string userId)
//        {
//            MailboxSettings retVal = null;

//            try
//            {
//                string requestUrl = String.Format(@"https://graph.microsoft.com/v1.0/users/{0}/mailboxSettings", userId);

//                // Create the request message
//                HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Get, requestUrl);
//                // hrm.Content = new StringContent(htmlBody, System.Text.Encoding.UTF8, "text/html");

//                // Authenticate (add access token) our HttpRequestMessage
//                graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm);

//                // Send the request and get the response.
//                HttpResponseMessage response = graphClient.HttpProvider.SendAsync(hrm).Result;

//                if (response.IsSuccessStatusCode)
//                {

//                    string contentJSON = response.Content.ReadAsStringAsync().Result;

//                    MailboxSettings mailSettings = graphClient.HttpProvider.Serializer.DeserializeObject<MailboxSettings>(contentJSON);

//                    retVal = mailSettings;

//                }
//                else
//                {

//                    string httpCode = response.StatusCode.ToString();
//                    string httpMessage = response.Content.ReadAsStringAsync().Result;

//                    throw new GraphAPICustomException("GraphAPI Failed to Retrieve MailboxSettings for " + userId, httpCode, httpMessage);
//                }
//            }
//            catch (GraphAPICustomException gex)
//            {
//                Log.Error(gex.Message, gex);
//             } 
//            catch (Exception ex)
//            {
//                if (!skipError(ex)) {
//                    Log.Error("GraphAPI Failed to Retrieve MailboxSettings for " + userId, ex);
//                }

//            }


//            return retVal;
//        }

//        private bool skipError(Exception ex) {

//            if (ex.InnerException != null)
//            {
//                if (ex.InnerException.GetType() == typeof(Microsoft.Graph.ServiceException))
//                {
//                    if (((Microsoft.Graph.ServiceException)ex.InnerException).StatusCode.ToString() == "NotFound")
//                    {
//                        return true;
//                    }

//                }
//            }

//            return false;
//        }

//        public void deleteArchiveEmails(string mailbox, DateTime cutoffDate, string mailboxPath, LoanStatus? loanStatus)
//        {
//            try
//            {
//                string nextLink = null;
//                List<Message> msgs = new List<Message>() { };

//                Log.Info("Getting messages older than " + cutoffDate.ToString("yyyy-MM-dd"));

//                string filterCriteria = "$filter=receivedDateTime lt " + cutoffDate.ToString("yyyy-MM-dd"); //2020-02-10

//                List<MailMessageAttributes> attr = new List<MailMessageAttributes>() { };
//                attr.Add(MailMessageAttributes.subject);
//                attr.Add(MailMessageAttributes.receivedDateTime);

//                do
//                {
//                    //msgs.AddRange(getMailPage(emailAddress, "Inbox/Faxes", ref nextLink));
//                    msgs.AddRange(getMailPage(mailbox, mailboxPath, ref nextLink, true, attr, filterCriteria));

//                } while (nextLink != null && msgs.Count < 1000);

//                Log.Info("Deleting " + msgs.Count.ToString() + " messages");

//                foreach( Message msg in msgs)
//                {
                    
//                    deleteMessage(mailbox, msg.Id, msg.Subject, mailboxPath, loanStatus, cutoffDate); 
//                }


//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Delete Mail", ex);
//                }

//            }

//        }

//        private MailFolder getMailFolder(string emailAddress, string folderPath)
//        {

//            MailFolder retVal = null;

//            //first entry in list must be parent folder
//            List<string> folderTree = folderPath.Split("/"[0]).ToList();

//            string requestUrlParentFolders = graphUrl + @"users/{0}/mailFolders";  //0 - Email Address
//            string requestUrlChildFolders = graphUrl + @"users/{0}/mailFolders/{1}/childFolders";  //0 - Email Address, 1 - FolderId

//            try
//            {
//                string folderId = null;


//                if (WellKnownFolderNames.Contains(folderPath.ToLower()))
//                {
//                    retVal = getChildFolders(String.Format(requestUrlParentFolders + "/" + folderPath, emailAddress)).FirstOrDefault();
//                }
//                else
//                {
//                    //this is the normal case
//                    List<MailFolder> childFolders = getChildFolders(String.Format(requestUrlParentFolders, emailAddress));
//                    MailFolder folderParent = childFolders.Where(q => q.DisplayName == folderTree[0]).FirstOrDefault();

//                    if (folderParent == null)
//                    {
//                        throw new Exception("Parent Folder Not Found for " + folderPath + " in " + emailAddress);
//                    }

//                    if (folderTree.Count > 1)
//                    {

//                        folderId = folderParent.Id;

//                        int folderLevelCount = 1;
//                        MailFolder targetFolder = null;

//                        //get child folders
//                        do
//                        {
//                            childFolders = getChildFolders(String.Format(requestUrlChildFolders, emailAddress, folderId));
//                            targetFolder = childFolders.Where(q => q.DisplayName == folderTree[folderLevelCount]).FirstOrDefault();

//                            if (targetFolder != null)
//                            {
//                                folderId = targetFolder.Id;
//                            };
//                            folderLevelCount++;

//                        } while (childFolders.Count > 0 && folderLevelCount < folderTree.Count && targetFolder != null);

//                        retVal = targetFolder;

//                    }
//                    else
//                    {
//                        retVal = folderParent;
//                    }

//                }

//            }
//            catch (Exception ex)
//            {
//                Log.Error("Error Retrieving Folder for " + folderPath + " in " + emailAddress, ex);
//            }

//            return retVal;

//        }

//        private List<MailFolder> getChildFolders(string url)
//        {
//            List<MailFolder> retVal = new List<MailFolder>() { };

//            try
//            {

//                string response = makeGraphRequest(url, HttpMethod.Get);

//                MailFolderChildFoldersCollectionResponse resp = graphClient.HttpProvider.Serializer.DeserializeObject<MailFolderChildFoldersCollectionResponse>(response);

//                if (resp.Value != null)
//                { 
//                    MailFolderChildFoldersCollectionPage resultPage = (MailFolderChildFoldersCollectionPage)resp.Value;
//                    retVal = resultPage.CurrentPage.ToList();
//                }
//                else if (resp.AdditionalData != null)
//                {
//                    retVal.Add(getMailFolderFromKeyValuePair(resp.AdditionalData));
//                }
                

//            }
//            catch (GraphAPICustomException gex)
//            {
//                Log.Error(gex.Message, gex);
//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Retrieve Folders", ex);
//                }

//            }

//            return retVal;

//        }

//        private List<Message> getMailPage(string emailAddress, string folderPath, ref string nextLink, bool inclChildFolders, List<MailMessageAttributes> mailAttributes, string filterCriteria = "")
//        {

//            List<Message> retVal = new List<Message>() { };

//            string mailAttributeList = String.Join(",", mailAttributes);

//            //receivedDateTime,subject
//            string mailRequestUrl = graphUrl + @"users/{0}/mailFolders/{1}/messages?$select=" + mailAttributeList + "&" + filterCriteria;  //0 - Email Address, 1 - FolderId

//            string response = String.Empty;

            

//            if (nextLink == null)
//            {
//                MailFolder folder = getMailFolder(emailAddress, folderPath);

//                if (folder != null)
//                {
//                    response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, folder.Id), HttpMethod.Get);
//                }
//            }
//            else
//            {
//                response = makeGraphRequest(nextLink, HttpMethod.Get);
//            }

//            if (response != "")
//            {
//                MailFolderMessagesCollectionResponse resp = graphClient.HttpProvider.Serializer.DeserializeObject<MailFolderMessagesCollectionResponse>(response);
//                KeyValuePair<string, object> nextLinkObj = resp.AdditionalData.Where(q => q.Key == "@odata.nextLink").FirstOrDefault();
//                nextLink = nextLinkObj.Equals(new KeyValuePair<string, object>(null, null)) ? null : nextLinkObj.Value.ToString();
//                MailFolderMessagesCollectionPage resultPage = (MailFolderMessagesCollectionPage)resp.Value;
//                retVal = resultPage.CurrentPage.ToList();
//            }
//            else
//            {
//                Log.Trace("No Response");
//            }
        

//            return retVal;

//        }

//        private string makeGraphRequest(string url, HttpMethod method, string content = null, Dictionary<string, string> headers = null)
//        {

//            string retVal = null;

//            // Create the request message
//            HttpRequestMessage hrm = new HttpRequestMessage(method, url);
//            if (content != null)
//            {
//                hrm.Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json");
//            }

//            if (headers != null)
//            {
//                foreach (KeyValuePair<string, string> header in headers)
//                {
//                    hrm.Headers.Add(header.Key, header.Value);
//                }
//            }

//            //ask for immutable ids
//            hrm.Headers.Add("Prefer", "IdType = \"ImmutableId\"");

//            // Authenticate (add access token) our HttpRequestMessage
//            graphClient.AuthenticationProvider.AuthenticateRequestAsync(hrm);

//            // Send the request and get the response.
//            int iRequestCount = 0;
//            int iRequestLimit = 5;
//            HttpResponseMessage response = null;
//            do
//            {
//                iRequestCount++;
//                try
//                {
//                    response = graphClient.HttpProvider.SendAsync(hrm).Result;
//                }
//                catch (Exception ex)
//                {
//                    Exception reviewEx = ex.InnerException == null ? ex : ex.InnerException;
//                    if (!reviewEx.Message.Contains("UnknownError"))
//                    {
//                        //throw this up to caller..else retry
//                        throw ex;
//                    }
//                }
//            } while (iRequestCount < iRequestLimit && response == null);

//            if (response == null)
//            {
//                throw new Exception("Unable to complete Graph request");
//            }

//            if (response.IsSuccessStatusCode)
//            {
//                //get parent folder
//                string contentJSON = response.Content.ReadAsStringAsync().Result;
//                retVal = contentJSON;
//            }
//            else
//            {

//                string httpCode = response.StatusCode.ToString();
//                string httpMessage = response.Content.ReadAsStringAsync().Result;

//                throw new GraphAPICustomException("GraphAPI Request Failed", httpCode, httpMessage);
//            }

//            return retVal;

//        }

//        private void deleteMessage(string emailAddress, string messageId, string emailSubject, string mailboxPath, LoanStatus? loanStatus, DateTime cutoffDate)
//        {

//            //check loan status and closing date
//            if (loanStatus !=  null)
//            {
//                DateTime closingDate = DateTime.Parse("1900-01-01");
//                if (isLoanStatus(emailSubject, (LoanStatus)loanStatus, ref closingDate))
//                {
//                    if (closingDate >= cutoffDate)
//                    {
//                        return;  //don't delete this one just yet
//                    }

//                }
//                else
//                {
//                    return;  //don't delete ones that are not this status
//                }

//            }

//            string mailDeleteUrl = graphUrl + @"users/{0}/messages/{1}";  //0 - Email Address, 1 - MessageId
//            //DELETE /users/{id | userPrincipalName}/messages/{id}

//            string response = makeGraphRequest(String.Format(mailDeleteUrl, emailAddress, messageId), HttpMethod.Delete);

//            if (response == "")
//            {
//                Log.Info("Email Deleted: " + mailboxPath + "; " + emailSubject);
//            } else
//            {
//                Log.Error("Error Deleting Email: " + mailboxPath + "; " + emailSubject, new Exception(response));
//            }

//        }

//        private bool isLoanStatus(string emailSubject, LoanStatus loanStatus, ref DateTime closingDate)
//        {


//            bool retVal = false;

//            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(
//                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
//                true, SqlServerDialect.Provider);

//            using (IDbConnection dbConn = dbFactory.OpenDbConnection())
//            {
//                IMAP imp = new IMAP();
//                IMAP.EmailMatch match = imp.getOrderRequest(dbConn, emailSubject);

//                if (match != null)
//                {
//                    string loanNumber = match.LoanNumber;

//                    emdbLoanInfoView loan = dbConn.Where<emdbLoanInfoView>(q => q.LoanNumber == loanNumber).FirstOrDefault();

//                    if (loan != null)
//                    {
//                        if ( loan.EncLoanStatus.Replace(" ","") == loanStatus.ToString())
//                        {
//                            retVal = true;
//                            closingDate = loan.EncSchedClosingDate;
//                        }
//                    }

//                }

//            }

//            return retVal;

//        }
        
//        private MailFolder getMailFolderFromKeyValuePair(IDictionary<string, object> data)
//        {
//            MailFolder retVal = new MailFolder();

//            //get list of mailfolder properties
//            List<string> mailFolderProperties = retVal.GetType().GetProperties().ToList().Select<System.Reflection.PropertyInfo, string>(q => q.Name.ToLower()).ToList();

//            foreach (KeyValuePair<string, object> item in data)
//            {
//                if (mailFolderProperties.Contains(item.Key.ToLower())) 
//                {

//                    //System.Reflection.PropertyInfo prop = typeof(MailFolder).GetProperty(item.Key, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
//                    System.Reflection.PropertyInfo prop = typeof(MailFolder).GetProperty(item.Key, System.Reflection.BindingFlags.IgnoreCase |
//                        System.Reflection.BindingFlags.Instance |
//                        System.Reflection.BindingFlags.Public);

//                    if (prop.PropertyType.Name == "String")
//                    {
//                        prop.SetValue(retVal, item.Value.ToString());
//                    }
//                    else
//                    {
//                        prop.SetValue(retVal, Int32.Parse(item.Value.ToString()));
//                    }
                    
//                }

//            }

//            return retVal;

//        }

//        private class TokenHandler
//        {
          
    
//            public string getToken()
//            {
//                // Even if this is a console application here, a daemon application is a confidential client application
//                IConfidentialClientApplication app;

//                //#if !VariationWithCertificateCredentials

//                app = ConfidentialClientApplicationBuilder.Create(ClientId)
//                           .WithClientSecret(ClientSecret)
//                           .WithTenantId(TenantId)
//                           .Build();
//                //#else
//                // Building the client credentials from a certificate
//                //makecert -r -pe -n "CN=FirstHome VOESystem OOO Cert" -b 05/01/2019 -e 05/01/2020 -ss my -len 2048

//                ///BEGIN PROD CODE
//                //X509Store store = new X509Store(StoreLocation.LocalMachine);
//                //store.Open(OpenFlags.OpenExistingOnly);

//                //X509Certificate2 cert = store.Certificates.OfType<X509Certificate2>()
//                //    .Where(q => q.SubjectName.Name.Contains(CertificateName)).FirstOrDefault();

//                //app = ConfidentialClientApplicationBuilder.Create(ClientId)
//                //    .WithCertificate(cert)
//                //    .WithTenantId(TenantId)
//                //    .Build();

//                //store.Close();

//                //END PROD CODE

//                //#endif

//                // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the
//                // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
//                // a tenant administrator
//                string[] scopes = new string[] { "https://graph.microsoft.com/.default" };


//                string result = null;

//                try
//                {
//                    result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result.AccessToken;
//                }
//                catch (MsalServiceException ex)
//                {
//                    // Case when ex.Message contains:
//                    // AADSTS70011 Invalid scope. The scope has to be of the form "https://resourceUrl/.default"
//                    // Mitigation: change the scope to be as expected
//                }

//                return result;
//            }

//        }
        
//        private List<Message> getMessages(string emailAddress, string folderPath, bool inclChildFolders, List<MailMessageAttributes> mailAttributes)
//        {

//            List<Message> retVal = new List<Message>() { };

//            try
//            {
//                string nextLink = null;
    
//                Log.Info("Getting messages for folder " + emailAddress + ": " + folderPath);

//                do
//                {

//                    retVal.AddRange(getMailPage(emailAddress, folderPath, ref nextLink, inclChildFolders, mailAttributes));

//                } while (nextLink != null);


//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Download Mail", ex);
//                }

//            }

//            return retVal;

//        }

//        private Message getMessage(string emailAddress, string id)
//        {

//            Message retVal = null;

//            try
//            {

//                Log.Info("Getting message " + emailAddress + ": " + id);

//                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}";

//                Dictionary<string, string> headers = new Dictionary<string, string>() { };
//                headers.Add("Prefer", "outlook.body-content-type=\"text\"");

//                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Get, null, headers);

//                retVal = graphClient.HttpProvider.Serializer.DeserializeObject<Message>(response);

//                return retVal;

//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Download Mail", ex);
//                }

//            }

//            return retVal;

//        }

//        public void getEmail()
//        {

//            DateTime lastDownloadDateTime = getLastDownloadDateTime();

//            try
//            {

//                //download headers in inbox
//                List<MailMessageAttributes> attr = new List<MailMessageAttributes>() { };
//                attr.Add(MailMessageAttributes.receivedDateTime);
//                attr.Add(MailMessageAttributes.subject);
//                attr.Add(MailMessageAttributes.internetMessageHeaders);
       
//                List<Message> msgs = getMessages(VOEEMAILADDRESS, "Inbox", false, attr);

//                //need to move messages last so not to mess up message indexes
//                List<string> messagesToMoveMatched = new List<string>() { };
//                List<string> messagesToMoveUnMatched = new List<string>() { };

//                //now see if any are older than last download time
//                foreach (Message msg in msgs)
//                {
                    
//                    try
//                    {
                        
//                        insertMail(msg);
//                        messagesToMoveMatched.Add(msg.Id);
                        
//                    }
                   
//                    catch (Exception ex)
//                    {
//                        //move email to reject folder for manual adjudication - any kind of error
//                        messagesToMoveUnMatched.Add(msg.Id);
                        
//                    }
                    
//                }

//                //set last download time at the end
//                setLastDownloadDateTime();

//                ////now move messages
//                foreach (string messageUID in messagesToMoveMatched)
//                {
//                    moveMessage(VOEEMAILADDRESS, messageUID, "Inbox/Completed/VOESystemMatched");
//                    //moveMessage(VOEEMAILADDRESS, messageUID, "Inbox/Archive-DoNotTouch");
//                }
//                foreach (string messageUID in messagesToMoveUnMatched)
//                {
//                    moveMessage(VOEEMAILADDRESS, messageUID, "Inbox/VOESystemUnmatched");
//                    //moveMessage(VOEEMAILADDRESS, messageUID, "Inbox/Archive-DoNotTouch");
//                }

//            }
            
//            catch (Exception ex)
//            {
//                Log.Error("Email Retrieval Error", ex);
//            }
           

//        }

//        public void insertMail(Message msg)
//        {
//            string ExchangeUID = msg.Id;
//            EmailMatch eMatch = null;

//            try
//            {
//                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
//                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
//                    true, SqlServerDialect.Provider);

//                IDbConnection dbConn = factory.CreateDbConnection();
//                dbConn.Open();

//                //try to match email to existing order
//                eMatch = getOrderRequest(dbConn, msg.Subject);

//                //testing exists in email table along with exchange id
//                string matchLoanNumber = eMatch.LoanNumber ?? "no match";

//                //check to see if message id already exists in table
//                if (dbConn.Where<VOESystem.Data.DBSchema.Email>(q => q.ExchangeUID == ExchangeUID && q.Subject.Contains(matchLoanNumber)).Count == 0)
//                {
                    
//                    string rawHeaders = String.Join(";", msg.InternetMessageHeaders.Select(q => q.Name + ":" + q.Value).ToList());

//                    msg = getMessage(VOEEMAILADDRESS, msg.Id);

//                    //concat recip list
//                    string reciplist = String.Join(",", msg.ToRecipients.Select(q => q.EmailAddress.Address).ToList());

//                    if (msg.CcRecipients.ToList().Count > 0) {
//                        string CCreciplist = String.Join(",", msg.CcRecipients.Select(q => q.EmailAddress.Address).ToList());
//                        reciplist += "," + CCreciplist;
//                    }

//                    string[] toEmaiList = reciplist.Split(","[0]);
//                    List<VOESystem.Data.DTO.Email.Recipient> toEmaiRecipientList = toEmaiList.Select<string, VOESystem.Data.DTO.Email.Recipient>(q =>
//                        new VOESystem.Data.DTO.Email.Recipient { EmailAddress = q }).ToList();

                    
//                    string fromEmail = msg.From.EmailAddress.Address;
//                    string fromName = msg.From.EmailAddress.Name;
//                    string Subject = msg.Subject;
//                    string MessageText = msg.Body.Content;
//                    int EmailTemplateId;
//                    DateTime ReceivedDate = msg.ReceivedDateTime.Value.LocalDateTime;
//                    bool IsAutoReply = checkAutoReply(rawHeaders);

//                    List<FileAttachment> atts = getAttachments(VOEEMAILADDRESS, msg.Id);
//                    List<AttachmentListItem> Attachments = null;

//                    //download attachments and add to table in database
//                    if (atts.Count > 0)
//                    {

//                        VOESystem.Data.Business.DocumentOps dop = new VOESystem.Data.Business.DocumentOps();

//                        foreach (FileAttachment attach in atts)
//                        {

//                            //do not import attachments that are pngs or jpg less than 50 k 
//                            //so we are not junking it up with signature images
//                            if ((attach.ContentType.ToLower().Contains("png") || attach.ContentType.ToLower().Contains("jpg") ||
//                                attach.ContentType.ToLower().Contains("jpeg")) && attach.Size < 10000)
//                            {
//                                Log.Info("Skipping Attachment - Possible Signature Image: " + attach.Name + "(" + attach.Size + "K) in " + ExchangeUID);
//                            }
//                            else if (!AllowedEmailAttachmentTypes.Contains(getSuffix(attach.Name, ".").ToLower()))
//                            {
//                                Log.Info("Skipping Attachment - DisallowedFileType: " + attach.Name + " in " + ExchangeUID);
//                            }
//                            else
//                            {
//                                //proceed with attachment
//                                if (Attachments == null)
//                                {
//                                    Attachments = new List<AttachmentListItem>() { };
//                                }

//                                if (!System.IO.Directory.Exists(AttachmentLocalPath))
//                                {
//                                    System.IO.Directory.CreateDirectory(AttachmentLocalPath);
//                                }

//                                string filenamepath = AttachmentLocalPath + "\\" + dop.cleanFileChars(ExchangeUID + attach.Name).Replace("-", "");

//                                // Save the attachment to disk
//                                if (System.IO.File.Exists(filenamepath))
//                                {
//                                    System.IO.File.Delete(filenamepath);
//                                }

//                                System.IO.File.WriteAllBytes(filenamepath, attach.ContentBytes);
//                                System.IO.FileStream fs = System.IO.File.OpenRead(filenamepath);

//                                IFile file = new AttachmentFile()
//                                {
//                                    ContentLength = (long)attach.Size,
//                                    ContentType = attach.ContentType,
//                                    FileName = attach.Name,
//                                    InputStream = fs
//                                };

//                                //upload the attachment to voe system
//                                foreach (int OrderRequestId in eMatch.OrderRequestIds)
//                                {
//                                    VOESystem.Data.DTO.UploadResult upres = dop.uploadFile(dbConn, file, "exchangesvc",
//                                           OrderRequestId, null, VOESystem.Data.Business.DocumentOps.DocumentType.ExchangeUploaded, false, eMatch.LoanNumber);

//                                    if (!upres.Result)
//                                    {
//                                        Log.Error("Error Uploading Attachment to VOE System: " + filenamepath, new Exception());
//                                    }
//                                    else
//                                    {
//                                        //if there are multiple orders, then the document should not be added twice to the Attachments collection
//                                        if (!Attachments.Contains(new DocumentListItem() { DocumentId = upres.DocumentId }))
//                                        {
//                                            Attachments.Add(new AttachmentListItem()
//                                            {
//                                                DocumentId = upres.DocumentId
//                                            });
//                                        }
//                                    }
//                                }
//                            }

//                        }

//                    }

//                    VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                    
//                    EmailTemplateId = dbConn.Where<VOESystem.Data.DBSchema.EmailTemplate>(q => q.Name == "Imported Email").FirstOrDefault().Id;

//                    if (eMatch.OrderRequestIds.Count == 0)
//                    {
//                        //this is a loan-level email
//                        eo.LogEmail(dbConn, toEmaiRecipientList, new List<VOESystem.Data.DTO.Email.Recipient>() { }, new List<VOESystem.Data.DTO.Email.Recipient>() { },
//                            fromEmail, fromName, Subject, MessageText, ExchangeUID, false, EmailTemplateId, false, eMatch.LoanNumber, null, Attachments, ReceivedDate,
//                            null, false, System.Net.Mail.MailPriority.Normal, false, false, null, null, IsAutoReply);
//                        runCustomNotificationCheck(dbConn, fromEmail, Subject);
//                    }
//                    else
//                    {
//                        //these are order-specific emails
//                        foreach (int? OrderRequestId in eMatch.OrderRequestIds)
//                        {

//                            //if this is a read receipt need to log it separately and update the original order
//                            int? OrigEmailId = null;
//                            if (isReadReceipt(dbConn, rawHeaders, msg.Subject, msg.Body.Content, OrderRequestId, out OrigEmailId))
//                            {
//                                //Log Read Receipt and Update 
//                                eo.LogReadReceipt(dbConn, fromEmail, fromName, Subject, MessageText, rawHeaders, ExchangeUID,
//                                    OrigEmailId ?? 0, OrderRequestId ?? 0, ReceivedDate);

//                            }
//                            else
//                            {
//                                //Log Normal Email
//                                bool IsAuditing = dbConn.SqlScalar<bool>(String.Format("SELECT dbo.fn_IsAuditingOrderRequestId({0})", OrderRequestId.ToString()));

//                                int emailId = eo.LogEmail(dbConn, toEmaiRecipientList, new List<VOESystem.Data.DTO.Email.Recipient>() { }, new List<VOESystem.Data.DTO.Email.Recipient>() { },
//                                   fromEmail, fromName, Subject, MessageText, ExchangeUID, false, EmailTemplateId, IsAuditing, eMatch.LoanNumber, OrderRequestId, Attachments,
//                                   ReceivedDate, null, false, System.Net.Mail.MailPriority.Normal, false, false, null, null, IsAutoReply);

//                                sendBounceReplys(dbConn, emailId, eMatch.SendBounceReplyIds, VOEEMAILADDRESS);
//                            }

//                            runCustomNotificationCheck(dbConn, fromEmail, Subject);
//                            runOOOReplyCheck(dbConn, OrderRequestId ?? 0);
//                        }
//                    }

//                }

//                //do nothing to the message on the server
//                Log.Info("Email Already Logged: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.Value.DateTime.ToShortTimeString());
//            }
//            catch (Exceptions.EmailNotMatchedException enm)
//            {
//                Log.Info("Email Not Matched: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.Value.DateTime.ToShortTimeString());
//                throw enm;  //throw up to main loop so email can be moved
//            }
//            catch (Exception ex)
//            {
//                Log.Error("Error Importing Email: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.Value.DateTime.ToShortTimeString(), ex);
//                throw ex;  //throw up to main loop so email can be moved
//            }


//        }

//        private void moveMessage(string emailAddress, string id, string destFolderPath)
//        {

           
//            try
//            {

//                Log.Info("Moving message " + emailAddress + ": " + id);

//                MailFolder destFolder = getMailFolder(emailAddress, destFolderPath);

//                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}/move";

//                string content = "{ " + String.Format("\"destinationId\": \"{0}\"", destFolder.Id) + " }";

//                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Post, content);


//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Move Mail", ex);
//                }

//            }

           
//        }

//        public List<FileAttachment> getAttachments(string emailAddress, string id)
//        {

//            List<FileAttachment> retVal = new List<FileAttachment>() { };

//            try
//            {

//                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}/attachments";

//                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Get);

//                if (response != null)
//                {
//                    Dictionary<string, object> respObj = graphClient.HttpProvider.Serializer.DeserializeObject<Dictionary<string, object>>(response);
//                    retVal = graphClient.HttpProvider.Serializer.DeserializeObject<List<FileAttachment>>(respObj.Where(q => q.Key == "value").FirstOrDefault().Value.ToString());
//                }

//            }
//            catch (Exception ex)
//            {
//                if (!skipError(ex))
//                {
//                    Log.Error("GraphAPI Failed to Download Attachments", ex);
//                }

//            }

//            return retVal;

//        }

//        public void UpdateBody()
//        {

//            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
//                   ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
//                   true, SqlServerDialect.Provider);

//            IDbConnection dbConn = factory.CreateDbConnection();
//            dbConn.Open();

//            List<VOESystem.Data.DBSchema.Email> emails = dbConn.Where<VOESystem.Data.DBSchema.Email>(q => q.DateTimeReceived > DateTime.Parse("2022-07-15")).ToList();

//            foreach (VOESystem.Data.DBSchema.Email email in emails)
//            {

//                if (email.Message.Contains("<html"))
//                {
//                    try
//                    {
//                        MailBee.Mime.MailMessage message = new MailBee.Mime.MailMessage
//                        {
//                            BodyHtmlText = email.Message
//                        };
//                        message.MakePlainBodyFromHtmlBody();
//                        string cleanText = message.BodyPlainText;

//                        email.Message = cleanText;

//                        dbConn.UpdateOnly(email,
//                                q => new { q.Message }, r => r.Id == email.Id);

//                        Log.Info("Email Id " + email.Id.ToString() + " updated");
                           

//                    }
//                    catch (Exception ex)
//                    {

//                        Log.Error("Update failed for Email Id " + email.Id, ex);
//                    }
//                }

//            }






//        }

//        public void testOperation()
//        {

//            Message msg = getMessage("voe@firsthome.com", "AAkALgAAAAAAHYQDEapmEc2byACqAC-EWg0A-XGp6OdBg0CxQrF5bBnBPAADjn9wfQAA");

//            insertMail(msg);
//        }

//    }

//}

