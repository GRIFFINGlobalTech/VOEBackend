using ServiceStack.Logging;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VOESystem.Data.DTO;
using VOESystem.Data.DBSchema;
using Newtonsoft.Json;

namespace VOEBackend.Email
{

    public class EmailOps : BaseClass
    {

        //protected static ILog Log = LogManager.GetLogger("EmailLogger");
        protected static new FHMC.NLogWrapper.Logger Log = new FHMC.NLogWrapper.Logger("EmailLogger");

        public void importEmail()
        {

            FHMC.Email.Office365 office365 = new FHMC.Email.Office365(Log);

            try
            {

                //set last download time at the end
                //DateTime lastDownloadDateTime = getLastDownloadDateTime();
                office365.getEmail(VOEEMAILADDRESS, "Inbox", "Inbox/Completed/VOESystemMatched", "Inbox/VOESystemUnmatched", insertMail, null, null, null);

                //set last download time at the end
                setLastDownloadDateTime();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //Log.Error("Email Retrieval Error", ex);
            }


        }

        public static void insertMail(FHMC.Interfaces.Email.IMailMessage msg)
        {
            string ExchangeUID = msg.Id;
            EmailMatch eMatch = null;

            try
            {
                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                //check to see if message id already exists in table
                //if (dbConn.Where<VOESystem.Data.DBSchema.Email>("ExchangeUID", ExchangeUID).Count == 0)  disabled this check since under some circumstances o365 reuses ids event though it's not supposed to
                //{

                    bool IsTruvNotification = false;

                    //try to match email to existing order
                    if (msg.SenderEmailAddress == "notifications@truv.com")
                    {
                        //match on 301 redirect url
                        eMatch = getOrderRequestFromTruvRedirect(dbConn, msg.Body);
                        IsTruvNotification = true;
                    }
                    else
                    {
                        eMatch = getOrderRequest(dbConn, msg.Subject);
                    }
                    

                    //concat recip list
                    string reciplist = String.Join(",", msg.ToRecipients.Select(q => q.EmailAddress).ToList());

                    if (msg.CcRecipients.Count > 0) { reciplist += "," + String.Join(",", msg.CcRecipients.Select(q => q.EmailAddress).ToList()); }

                    //msg.Parser.HtmlToPlainMode = HtmlToPlainAutoConvert.IfNoPlain;

                    string[] toEmaiList = reciplist.Split(","[0]);
                    List<VOESystem.Data.DTO.Email.Recipient> toEmaiRecipientList = toEmaiList.Select<string, VOESystem.Data.DTO.Email.Recipient>(q =>
                        new VOESystem.Data.DTO.Email.Recipient { EmailAddress = q }).ToList();

                    string fromEmail = msg.SenderEmailAddress;
                    string fromName = msg.SenderEmailName;
                    string Subject = msg.Subject;
                    string MessageText = msg.Body;
                    string MessageHTML = msg.BodyHTML;
                    int EmailTemplateId;
                    DateTime ReceivedDate = msg.ReceivedDateTime;
                    bool IsAutoReply = checkAutoReply(msg.RawReceiptHeaders);

                    List<AttachmentListItem> Attachments = null;

                    //download attachments and add to table in database
                    if (msg.Attachments.Count > 0)
                    {

                        VOESystem.Data.Business.DocumentOps dop = new VOESystem.Data.Business.DocumentOps();

                        foreach (FHMC.Interfaces.Email.IMailAttachment attach in msg.Attachments)
                        {

                            if (attach.ContentBytes != null)
                            {
                                //do not import attachments that are pngs or jpg less than 50 k 
                                //so we are not junking it up with signature images
                                string contentType = isNull(attach.ContentType, "").ToLower();

                                if ((contentType.Contains("png") || contentType.Contains("jpg") || contentType.Contains("jpeg")) && attach.Size < 10000)
                                {
                                    Log.Info("Skipping Attachment - Possible Signature Image: " + attach.Name + "(" + attach.Size + "K) in " + ExchangeUID);
                                }
                                else if (!AllowedEmailAttachmentTypes.Contains(getSuffix(attach.Name, ".").ToLower()))
                                {
                                    Log.Info("Skipping Attachment - DisallowedFileType: " + attach.Name + " in " + ExchangeUID);
                                }
                                else
                                {
                                    //proceed with attachment
                                    if (Attachments == null)
                                    {
                                        Attachments = new List<AttachmentListItem>() { };
                                    }

                                    if (!Directory.Exists(AttachmentLocalPath))
                                    {
                                        Directory.CreateDirectory(AttachmentLocalPath);
                                    }

                                    string filenamepath = AttachmentLocalPath + "\\" + dop.cleanFileChars(ExchangeUID + attach.Name).Replace("-", "");

                                    // Save the attachment to disk
                                    if (File.Exists(filenamepath))
                                    {
                                        File.Delete(filenamepath);
                                    }

                                    System.IO.File.WriteAllBytes(filenamepath, attach.ContentBytes);

                                    FileStream fs = File.OpenRead(filenamepath);

                                    IFile file = new AttachmentFile()
                                    {
                                        ContentLength = (long)attach.Size,
                                        ContentType = attach.ContentType,
                                        FileName = attach.Name,
                                        InputStream = fs
                                    };

                                    //upload the attachment to voe system
                                    foreach (int OrderRequestId in eMatch.OrderRequestIds)
                                    {
                                        VOESystem.Data.DTO.UploadResult upres = dop.uploadFile(dbConn, file, "exchangesvc",
                                               OrderRequestId, null, VOESystem.Data.Business.DocumentOps.DocumentType.ExchangeUploaded, false, eMatch.LoanNumber);

                                        if (!upres.Result)
                                        {
                                            Log.Error("Error Uploading Attachment to VOE System: " + filenamepath, new Exception());
                                        }
                                        else
                                        {
                                            //if there are multiple orders, then the document should not be added twice to the Attachments collection
                                            if (!Attachments.Contains(new DocumentListItem() { DocumentId = upres.DocumentId }))
                                            {
                                                Attachments.Add(new AttachmentListItem()
                                                {
                                                    DocumentId = upres.DocumentId
                                                });
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }

                    VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                    //((ServiceStack.OrmLite.OrmLiteConnection)dbConn).Transaction.Dispose();

                    EmailTemplateId = dbConn.Where<VOESystem.Data.DBSchema.EmailTemplate>(q => q.Name == "Imported Email").FirstOrDefault().Id;

                    if (eMatch.OrderRequestIds.Count == 0)
                    {
                        //this is a loan-level email
                        eo.LogEmail(dbConn, toEmaiRecipientList, new List<VOESystem.Data.DTO.Email.Recipient>() { }, new List<VOESystem.Data.DTO.Email.Recipient>() { },
                            fromEmail, fromName, Subject, MessageText, ExchangeUID, false, EmailTemplateId, false, eMatch.LoanNumber, null, Attachments, ReceivedDate,
                            null, false, System.Net.Mail.MailPriority.Normal, false, false, null, null, IsAutoReply, MessageHTML);
                        runCustomNotificationCheck(dbConn, fromEmail, Subject);
                    }
                    else
                    {
                        //these are order-specific emails
                        foreach (int? OrderRequestId in eMatch.OrderRequestIds)
                        {

                            //if this is a read receipt need to log it separately and update the original order
                            int? OrigEmailId = null;
                            if (isReadReceipt(dbConn, msg.RawReceiptHeaders, msg.Subject, msg.Body, OrderRequestId, out OrigEmailId))
                            {
                                //Log Read Receipt and Update 
                                eo.LogReadReceipt(dbConn, fromEmail, fromName, Subject, MessageText, msg.RawReceiptHeaders, ExchangeUID,
                                    OrigEmailId ?? 0, OrderRequestId ?? 0, ReceivedDate);

                            }
                            else
                            {

                            if (IsTruvNotification)
                            {
                                EmailTemplateId = dbConn.Where<VOESystem.Data.DBSchema.EmailTemplate>(q => q.Name == "Truv Notification Received").FirstOrDefault().Id;
                            }

                            //Log Normal Email
                            bool IsAuditing = dbConn.SqlScalar<bool>(String.Format("SELECT dbo.fn_IsAuditingOrderRequestId({0})", OrderRequestId.ToString()));

                                int emailId = eo.LogEmail(dbConn, toEmaiRecipientList, new List<VOESystem.Data.DTO.Email.Recipient>() { }, new List<VOESystem.Data.DTO.Email.Recipient>() { },
                                   fromEmail, fromName, Subject, MessageText, ExchangeUID, false, EmailTemplateId, IsAuditing, eMatch.LoanNumber, OrderRequestId, Attachments,
                                   ReceivedDate, null, false, System.Net.Mail.MailPriority.Normal, false, false, null, null, IsAutoReply, MessageHTML);

                                sendBounceReplys(dbConn, emailId, eMatch.SendBounceReplyIds, VOEEMAILADDRESS);
                            }

                            runCustomNotificationCheck(dbConn, fromEmail, Subject);
                            runOOOReplyCheck(dbConn, OrderRequestId ?? 0);
                        }
                    }

                

                //do nothing to the message on the server
                Log.Info("Email Logged to Database: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.ToShortTimeString());
            }
            catch (Exceptions.EmailNotMatchedException enm)
            {
                Log.Info("Email Not Matched: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.ToShortTimeString());
                throw enm;  //throw up to main loop so email can be moved
            }
            catch (Exception ex)
            {
                Log.Error("Error Importing Email: " + ExchangeUID + " " + msg.Subject + " " + msg.ReceivedDateTime.ToShortTimeString(), ex);
                throw ex;  //throw up to main loop so email can be moved
            }


        }

        public void deleteArchiveEmails()
        {

            FHMC.Email.Office365 o365Client = new FHMC.Email.Office365(Log);

            o365Client.deleteEmails("voe@firsthome.com", "Inbox/Archive-DoNotTouch", DateTime.Today.AddMonths(-3), null);
            o365Client.deleteEmails("voe@firsthome.com", "Inbox/VOESystemUnmatched", DateTime.Today.AddMonths(-3), null);

            o365Client.deleteEmails("voe@firsthome.com", "Inbox/Completed", DateTime.Today.AddMonths(-3), null);
            o365Client.deleteEmails("voe@firsthome.com", "Inbox/Completed/FAX confirms", DateTime.Today.AddMonths(-3), null);

            o365Client.deleteEmails("voe@firsthome.com", "Inbox/VOESystemMatched", DateTime.Today.AddMonths(-3), null);
            o365Client.deleteEmails("voe@firsthome.com", "Inbox/Completed/VOESystemMatched", DateTime.Today.AddMonths(-3), null);


        }

        public void downloadEmails()
        {

            Log = new FHMC.NLogWrapper.Logger("Program");
            FHMC.Email.Office365 office365 = new FHMC.Email.Office365(Log);

            try
            {

                office365.getEmail(VOEEMAILADDRESS, "Sent Items", null, null, logEmailId, null, DateTime.Parse("2025-04-09 07:00"), DateTime.Parse("2025-04-10 11:15"));


            }
            catch (Exception ex)
            {
                Log.Error("Email Retrieval Error", ex);
            }



        }

        private static void logEmailId(FHMC.Interfaces.Email.IMailMessage msg)
        {

            //"Message-ID:<4e86f3fe-dbf2-4fe8-a38b-d6197f9e02e7@HQExc4V.firsthomemtg.com>";
            string msgIdRegex = @"Message-ID:<(.*)@HQExc4V.firsthomemtg.com>";
            Match match = Regex.Match(msg.RawReceiptHeaders, msgIdRegex);

            if (match.Success)
            {
                Log.Info(match.Value.Replace("Message-ID:", "") + ";;" + msg.Subject);
            }
        }

        public void resendEmails()
        {

            string inputFile = @"C:\Temp\ResendEmails.txt";
            List<int> emailIdList = new List<int>() { };

            //read from input file and update each loan
            using (StreamReader sr = new StreamReader(inputFile))
            {
                while (sr.Peek() >= 0)
                {
                    string emailId = sr.ReadLine();
                    if (emailId != "")
                    {
                        emailIdList.Add(Int32.Parse(emailId));
                    }
                }
            }

            foreach(int emailId in emailIdList)
            {
                resendEmail(emailId);
            }



        }
        
        public void resendEmail(int emailId)
        {


            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            try
            {
                using (IDbConnection dbConn = factory.CreateDbConnection())
                {
                    dbConn.Open();

                    VOESystem.Data.DBSchema.Email email = dbConn.Where<VOESystem.Data.DBSchema.Email>(q => q.Id == emailId).FirstOrDefault();

                    FHMC.Interfaces.Email.IMailMessage message = new FHMC.Graph.Email.MailMessage();

                    message.Subject = email.Subject;
                    message.Body = email.Message;

                    message.ToRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>() { };
                    message.ToRecipients.AddRange(JsonConvert.DeserializeObject<List<FHMC.Graph.Email.MailRecipient>>(email.ToRecipientList));

                    if (email.CcRecipientList != "[]")
                    {
                        message.CcRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>() { };
                        message.CcRecipients.AddRange(JsonConvert.DeserializeObject<List<FHMC.Graph.Email.MailRecipient>>(email.CcRecipientList));
                    }

                    message.IsBodyHTML = false;
                    message.IsReadReceiptRequested = email.ReadReceiptRequested;

                    if (email.EmailPriorityId == 3)
                    {
                        message.Importance = FHMC.Interfaces.Email.EmailImportance.High;
                    }

                    message.CustomSendHeaders = new Dictionary<string, string>() { };
                    message.CustomSendHeaders.Add("X-Message-ID", email.ExchangeUID);

                    List<EmailAttachmentView> atts = dbConn.Where<EmailAttachmentView>(q => q.EmailId == emailId).ToList();

                    List<FHMC.Interfaces.Email.IMailAttachment> attachments = new List<FHMC.Interfaces.Email.IMailAttachment>() { };

                    foreach (EmailAttachmentView att in atts)
                    {

                        string docFileName = "";

                        if (att.DocumentTypeName == "AutoGeneratedForm")
                        {
                            docFileName = RepositoryPath + "Documents\\" + att.FormTag + "\\" + att.UniqueFileName;
                        }
                        else if (att.DocumentTypeName == "EncompassCloud")
                        {
                            docFileName = RepositoryPath + "EncDocuments\\" + att.UniqueFileName;
                        }
                        else if (att.DocumentTypeName == "ExchangeUploaded")
                        {
                            docFileName = RepositoryPath + "Documents\\Upload\\" + att.UniqueFileName;
                        }
                        else if (att.DocumentTypeName == "UserUploaded")
                        {
                            docFileName = RepositoryPath + "Documents\\Upload\\" + att.UniqueFileName;
                        }
                        else
                        {
                            throw new Exception("Unsupported Document Type");
                        }

                        byte[] content = System.IO.File.ReadAllBytes(docFileName);

                        attachments.Add(new FHMC.Graph.Email.MailAttachment
                        {
                            Name = att.FileDisplayName,
                            ContentType = "application/pdf",
                            ContentBytes = content,
                            Size = content.Length
                        });

                    }

                    if (attachments.Count > 0)
                    {
                        message.Attachments = attachments;
                    }

                    FHMC.Email.Office365 eop = new FHMC.Email.Office365(Log);
                    eop.sendEmail(message, FHMC.Interfaces.Email.EmailSender.VOE);

                    Log.Info(emailId + " Sent");

                }
            }
            catch (Exception ex)
            {
                Log.Error("Error Resending Email", ex);

            }



        }

        public void testOperation()
        {

            FHMC.Email.Office365 eop = new FHMC.Email.Office365(Log);
            FHMC.Interfaces.Email.IMailMessage message = new FHMC.Graph.Email.MailMessage();

            message.Subject = "Test Graph Message 10";
            message.Body = "Testing100000000!!!!";

            message.ToRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>();
            message.ToRecipients.Add(new FHMC.Graph.Email.MailRecipient
            {
                EmailAddress = "christine.desimone@gmail.com"
            });
            
            message.IsBodyHTML = false;
            
            eop.sendEmail(message, FHMC.Interfaces.Email.EmailSender.VOE);



        }


    }
}
