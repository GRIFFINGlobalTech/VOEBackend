using FHMC.Interfaces.Email;
using Microsoft.Graph;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace FHMC.Graph
{
    public class Email : BaseClass
    {

        private const string SendKey = "3c7e4200-7904-45f1-ac7b-68d41a01d2c6";
        private string enteredKey;

        public Email(object logger, string emailKey) : base(logger) {

            enteredKey = emailKey;

        }

        private static List<string> WellKnownFolderNames = new List<string>() { "recoverableitemsdeletions" };

        private static List<string> allowedUnsafeHTMLRecips = new List<string> { "notifications@truv.com" };

        public enum MailMessageAttributes
        {
            receivedDateTime,
            sentDateTime,
            subject,
            toRecipients,
            ccRecipients,
            internetMessageHeaders,
            from,
            body
        }

        public class MailMessage : FHMC.Interfaces.Email.IMailMessage
        {
            public string Id { get; set; }
            public string Subject { get; set; }
            public DateTime ReceivedDateTime { get; set; }
            public List<IMailAttachment> Attachments { get; set; }
            public string Body { get; set; }
            public bool IsBodyHTML { get; set; }
            public string BodyHTML { get; set; }
            public string SenderEmailAddress { get; set; }
            public string SenderEmailName { get; set; }
            public string RawReceiptHeaders { get; set; }
            public Dictionary<string, string> CustomSendHeaders { get; set; }
            public bool IsReadReceiptRequested { get; set; }
            public EmailImportance? Importance { get; set; }
            public List<IMailRecipient> ToRecipients { get; set; }
            public List<IMailRecipient> CcRecipients { get; set; }
            public List<IMailRecipient> BccRecipients { get; set; }
            public List<IMailRecipient> ReplyToList { get; set; }
        }

        public class MailAttachment : FHMC.Interfaces.Email.IMailAttachment
        {
            public string Id { get; set; }
            public byte[] ContentBytes { get; set; }
            public string ContentType { get; set; }
            public string Name { get; set; }
            public int? Size { get; set; }
        }

        public class MailRecipient : FHMC.Interfaces.Email.IMailRecipient
        {
            public string EmailAddress { get; set; }
        }

        public class Message : Microsoft.Graph.Message { };

        public class FileAttachment : Microsoft.Graph.FileAttachment { };

        public IMailMessage transformGraphMessageToMessage(Message message, List<FileAttachment> attachments, List<string> headers = null, string bodyHTML = null)
        {

            MailMessage retVal = new MailMessage();
            retVal.Id = message.Id;
            retVal.Subject = message.Subject;
            retVal.Body = message.Body.Content;
            retVal.BodyHTML = bodyHTML;
            retVal.ReceivedDateTime = message.ReceivedDateTime.Value.LocalDateTime;
            retVal.SenderEmailAddress = message.Sender.EmailAddress.Address;
            retVal.SenderEmailName = message.Sender.EmailAddress.Name;
            if (headers != null)
            {
                retVal.RawReceiptHeaders = String.Join("::", headers.ToList());
            }
            retVal.Attachments = new List<IMailAttachment>() { };
            retVal.ToRecipients = new List<IMailRecipient>() { };
            retVal.CcRecipients = new List<IMailRecipient>() { };

            foreach (FileAttachment attachment in attachments)
            {
                retVal.Attachments.Add(new MailAttachment
                {
                    ContentBytes = attachment.ContentBytes,
                    ContentType = attachment.ContentType,
                    Id = attachment.Id,
                    Name = attachment.Name,
                    Size = attachment.Size
                });

            }

            foreach (Recipient recip in message.ToRecipients)
            {
                retVal.ToRecipients.Add(new MailRecipient
                {
                    EmailAddress = recip.EmailAddress.Address
                });

            }

            foreach (Recipient recip in message.CcRecipients)
            {
                retVal.CcRecipients.Add(new MailRecipient
                {
                    EmailAddress = recip.EmailAddress.Address
                });

            }


            return retVal;

        }

        private MailFolder getMailFolder(string emailAddress, string folderPath)
        {

            MailFolder retVal = null;

            //first entry in list must be parent folder
            List<string> folderTree = folderPath.Split("/"[0]).ToList();

            string requestUrlParentFolders = graphUrl + @"users/{0}/mailFolders";  //0 - Email Address
            string requestUrlChildFolders = graphUrl + @"users/{0}/mailFolders/{1}/childFolders";  //0 - Email Address, 1 - FolderId

            try
            {
                string folderId = null;


                if (WellKnownFolderNames.Contains(folderPath.ToLower()))
                {
                    retVal = getChildFolders(String.Format(requestUrlParentFolders + "/" + folderPath, emailAddress)).FirstOrDefault();
                }
                else
                {
                    //this is the normal case
                    List<MailFolder> childFolders = getChildFolders(String.Format(requestUrlParentFolders, emailAddress));
                    MailFolder folderParent = childFolders.Where(q => q.DisplayName == folderTree[0]).FirstOrDefault();

                    if (folderParent == null)
                    {
                        throw new Exception("Parent Folder Not Found for " + folderPath + " in " + emailAddress);
                    }

                    if (folderTree.Count > 1)
                    {

                        folderId = folderParent.Id;

                        int folderLevelCount = 1;
                        MailFolder targetFolder = null;

                        //get child folders
                        do
                        {
                            childFolders = getChildFolders(String.Format(requestUrlChildFolders, emailAddress, folderId));
                            targetFolder = childFolders.Where(q => q.DisplayName == folderTree[folderLevelCount]).FirstOrDefault();

                            if (targetFolder != null)
                            {
                                folderId = targetFolder.Id;
                            };
                            folderLevelCount++;

                        } while (childFolders.Count > 0 && folderLevelCount < folderTree.Count && targetFolder != null);

                        retVal = targetFolder;

                    }
                    else
                    {
                        retVal = folderParent;
                    }

                }

            }
            catch (Exception ex)
            {
                Log.Error("Error Retrieving Folder for " + folderPath + " in " + emailAddress, ex);
            }

            return retVal;

        }

        private List<MailFolder> getChildFolders(string url)
        {
            List<MailFolder> retVal = new List<MailFolder>() { };

            try
            {

                string response = makeGraphRequest(url, HttpMethod.Get);

                MailFolderChildFoldersCollectionResponse resp = graphClient.HttpProvider.Serializer.DeserializeObject<MailFolderChildFoldersCollectionResponse>(response);

                if (resp.Value != null)
                {
                    MailFolderChildFoldersCollectionPage resultPage = (MailFolderChildFoldersCollectionPage)resp.Value;
                    retVal = resultPage.CurrentPage.ToList();
                }
                else if (resp.AdditionalData != null)
                {
                    retVal.Add(getMailFolderFromKeyValuePair(resp.AdditionalData));
                }


            }

            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Retrieve Folders", ex);
                }

            }

            return retVal;

        }

        private List<FHMC.Graph.Email.Message> getMailPage(string emailAddress, string folderPath, ref string nextLink, bool inclChildFolders, List<MailMessageAttributes> mailAttributes, string filterCriteria)
        {

            List<FHMC.Graph.Email.Message> retVal = new List<FHMC.Graph.Email.Message>() { };

            string mailAttributeList = String.Join(",", mailAttributes);

            //receivedDateTime,subject
            string mailRequestUrl = graphUrl + @"users/{0}/mailFolders/{1}/messages?$select=" + mailAttributeList + "&" + isNull(filterCriteria, "");  //0 - Email Address, 1 - FolderId

            string response = String.Empty;



            if (nextLink == null)
            {
                MailFolder folder = getMailFolder(emailAddress, folderPath);

                if (folder != null)
                {
                    response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, folder.Id), HttpMethod.Get);
                }
            }
            else
            {
                response = makeGraphRequest(nextLink, HttpMethod.Get);
            }

            if (response != "")
            {
                MailFolderMessagesCollectionResponse resp = graphClient.HttpProvider.Serializer.DeserializeObject<MailFolderMessagesCollectionResponse>(response);
                KeyValuePair<string, object> nextLinkObj = resp.AdditionalData.Where(q => q.Key == "@odata.nextLink").FirstOrDefault();
                nextLink = nextLinkObj.Equals(new KeyValuePair<string, object>(null, null)) ? null : nextLinkObj.Value.ToString();
                if (nextLink == null)
                {
                    //try other location
                    nextLink = resp.NextLink;
                }

                MailFolderMessagesCollectionPage resultPage = (MailFolderMessagesCollectionPage)resp.Value;
                retVal = resultPage.CurrentPage.ToList().Select<Microsoft.Graph.Message, FHMC.Graph.Email.Message>(q =>
                     new FHMC.Graph.Email.Message
                     {
                         Id = q.Id,
                         Subject = q.Subject,
                         ReceivedDateTime = q.ReceivedDateTime,
                         Attachments = q.Attachments,
                         Body = q.Body,
                         Sender = q.Sender,
                         InternetMessageHeaders = q.InternetMessageHeaders
                     }).ToList();
            }
            else
            {
                Log.Trace("No Response");
            }


            return retVal;

        }

        public void deleteMessage(string emailAddress, string messageId, string emailSubject, string mailboxPath)
        {

            string mailDeleteUrl = graphUrl + @"users/{0}/messages/{1}";  //0 - Email Address, 1 - MessageId
            //DELETE /users/{id | userPrincipalName}/messages/{id}

            string response = makeGraphRequest(String.Format(mailDeleteUrl, emailAddress, messageId), HttpMethod.Delete);

            if (response == "")
            {
                Log.Info("Email Deleted: " + mailboxPath + "; " + emailSubject);
            }
            else
            {
                Log.Error("Error Deleting Email: " + mailboxPath + "; " + emailSubject, new Exception(response));
            }

        }

        private MailFolder getMailFolderFromKeyValuePair(IDictionary<string, object> data)
        {
            MailFolder retVal = new MailFolder();

            //get list of mailfolder properties
            List<string> mailFolderProperties = retVal.GetType().GetProperties().ToList().Select<System.Reflection.PropertyInfo, string>(q => q.Name.ToLower()).ToList();

            foreach (KeyValuePair<string, object> item in data)
            {
                if (mailFolderProperties.Contains(item.Key.ToLower()))
                {

                    //System.Reflection.PropertyInfo prop = typeof(MailFolder).GetProperty(item.Key, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                    System.Reflection.PropertyInfo prop = typeof(MailFolder).GetProperty(item.Key, System.Reflection.BindingFlags.IgnoreCase |
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public);

                    if (prop.PropertyType.Name == "String")
                    {
                        prop.SetValue(retVal, item.Value.ToString());
                    }
                    else
                    {
                        prop.SetValue(retVal, Int32.Parse(item.Value.ToString()));
                    }

                }

            }

            return retVal;

        }

        private FHMC.Graph.Email.Message transformMessageToGraphMessage(IMailMessage msg)
        {

            FHMC.Graph.Email.Message retVal = new FHMC.Graph.Email.Message();
    
            retVal.Subject = msg.Subject;
            retVal.Body = new ItemBody
            {
                Content = msg.Body
            };

            if (msg.IsBodyHTML)
            {
                retVal.Body.ContentType = BodyType.Html;

                //if (msg.Body.Contains("charset=utf-8"))
                //{
                //    retVal.SingleValueExtendedProperties = new MessageSingleValueExtendedPropertiesCollectionPage
                //    {
                //        new SingleValueLegacyExtendedProperty
                //            {
                //                Id = "Integer 0x3fde",
                //                Value = "20127"
                //            }
                //    };

                //}

            }
            else
            {
                retVal.Body.ContentType = BodyType.Text;
            }

            retVal.Sender = new Recipient
            {
                EmailAddress = new EmailAddress {
                    Address = msg.SenderEmailAddress,
                    Name = msg.SenderEmailName
                }
            };
            retVal.IsReadReceiptRequested = msg.IsReadReceiptRequested;

            if (msg.Importance != null) {
                retVal.Importance = (Microsoft.Graph.Importance)Enum.Parse(typeof(Microsoft.Graph.Importance), msg.Importance.Value.ToString());
            }

            if (msg.CustomSendHeaders != null) {
                List<InternetMessageHeader> headers = new List<InternetMessageHeader>() { };
                foreach(KeyValuePair<string, string> header in msg.CustomSendHeaders)
                {
                    headers.Add(new InternetMessageHeader {
                        Name = header.Key,
                        Value = header.Value
                    });

                }
                retVal.InternetMessageHeaders = headers;
            }

            if (msg.Attachments != null) {
                IMessageAttachmentsCollectionPage attachments = new MessageAttachmentsCollectionPage();

                foreach(IMailAttachment att in msg.Attachments)
                {
                    attachments.Add(new FileAttachment
                    {
                        Name = att.Name,
                        ContentBytes = att.ContentBytes,
                        ContentType = att.ContentType,
                        Size = att.Size,
                        ODataType = @"#microsoft.graph.fileAttachment"
                    });

                }

                retVal.Attachments = attachments;
            }

            if (msg.ToRecipients != null)
            {
                List<Microsoft.Graph.Recipient> toRecipients = new List<Microsoft.Graph.Recipient>() { };
                foreach (FHMC.Interfaces.Email.IMailRecipient recip in msg.ToRecipients)
                {
                    toRecipients.Add(new Microsoft.Graph.Recipient
                    {
                        EmailAddress = new Microsoft.Graph.EmailAddress {
                            Address = recip.EmailAddress
                        }
                    });
                }
                retVal.ToRecipients = toRecipients;
            }

            if (msg.CcRecipients != null)
            {
                List<Microsoft.Graph.Recipient> ccRecipients = new List<Microsoft.Graph.Recipient>() { };
                foreach (FHMC.Interfaces.Email.IMailRecipient recip in msg.CcRecipients)
                {
                    ccRecipients.Add(new Microsoft.Graph.Recipient
                    {
                        EmailAddress = new Microsoft.Graph.EmailAddress
                        {
                            Address = recip.EmailAddress
                        }
                    });
                }
                retVal.CcRecipients = ccRecipients;
            }

            if (msg.BccRecipients != null)
            {
                List<Microsoft.Graph.Recipient> bccRecipients = new List<Microsoft.Graph.Recipient>() { };
                foreach (FHMC.Interfaces.Email.IMailRecipient recip in msg.BccRecipients)
                {
                    bccRecipients.Add(new Microsoft.Graph.Recipient
                    {
                        EmailAddress = new Microsoft.Graph.EmailAddress
                        {
                            Address = recip.EmailAddress
                        }
                    });
                }
                retVal.BccRecipients = bccRecipients;
            }

            if (msg.ReplyToList != null)
            {
                List<Microsoft.Graph.Recipient> replyToRecipients = new List<Microsoft.Graph.Recipient>() { };
                foreach (FHMC.Interfaces.Email.IMailRecipient recip in msg.ReplyToList)
                {
                    replyToRecipients.Add(new Microsoft.Graph.Recipient
                    {
                        EmailAddress = new Microsoft.Graph.EmailAddress
                        {
                            Address = recip.EmailAddress
                        }
                    });
                }
                retVal.ReplyTo = replyToRecipients;
            }

            retVal.ODataType = null;

            return retVal;

        }

        public List<FHMC.Graph.Email.Message> getMessages(string emailAddress, string folderPath, bool inclChildFolders, List<MailMessageAttributes> mailAttributes, string filterCriteria)
        {

            List<FHMC.Graph.Email.Message> retVal = new List<FHMC.Graph.Email.Message>() { };

            try
            {
                string nextLink = null;

                Log.Info("Getting messages for folder " + emailAddress + ": " + folderPath);

                do
                {

                    retVal.AddRange(getMailPage(emailAddress, folderPath, ref nextLink, inclChildFolders, mailAttributes, filterCriteria));

                } while (nextLink != null);


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Download Mail", ex);
                }

            }

            return retVal;

        }

        public Message getMessage(string emailAddress, string id, bool getHTML = false, string senderEmailAddress = null)
        {

            Message retVal = null;

            try
            {

                Log.Info("Getting message " + emailAddress + ": " + id);

                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}";

                Dictionary<string, string> headers = new Dictionary<string, string>() { };

                if (getHTML)
                {    
                    if (allowedUnsafeHTMLRecips.Contains(senderEmailAddress))
                    {
                        headers.Add("Prefer", "outlook.allow-unsafe-html");
                    }
                    else
                    {
                        headers.Add("Prefer", "outlook.body-content-type=\"html\"");
                    }
                }
                else
                {
                    headers.Add("Prefer", "outlook.body-content-type=\"text\"");
                }

                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Get, null, headers);

                retVal = graphClient.HttpProvider.Serializer.DeserializeObject<Message>(response);

                return retVal;

            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Download Mail", ex);
                }

            }

            return retVal;

        }

        public string getMessageBodyHTML(string emailAddress, string id, string senderEmailAddress)
        {

            string retVal = null;

            try
            {

                Log.Info("Getting message HTML " + emailAddress + ": " + id);

                Message msg = getMessage(emailAddress, id, true, senderEmailAddress);

                retVal = msg.Body.Content;


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Download Mail HTML", ex);
                }

            }

            return retVal;

        }

        public string getMessageSenderEmailAddress (Message message)
        {

            string retVal = null;

            try
            {

                retVal = message.Sender.EmailAddress.Address;
               
            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Get Sender Email Address", ex);
                }

            }

            return retVal;

        }

        public void moveMessage(string emailAddress, string id, string destFolderPath)
        {


            try
            {

                Log.Info("Moving message " + emailAddress + ": " + id);

                MailFolder destFolder = getMailFolder(emailAddress, destFolderPath);

                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}/move";

                string content = "{ " + String.Format("\"destinationId\": \"{0}\"", destFolder.Id) + " }";

                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Post, content);


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Move Mail", ex);
                }

            }


        }

        public void duplicateMessage(string emailAddress, string id, string archiveFolderPath)
        {


            try
            {

                Log.Info("Archiving message " + emailAddress + ": " + id);

                MailFolder archiveFolder = getMailFolder(emailAddress, archiveFolderPath);

                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}/copy";

                string content = "{ " + String.Format("\"destinationId\": \"{0}\"", archiveFolder.Id) + " }";

                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Post, content);


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Archive Mail", ex);
                }

            }


        }

        public List<FileAttachment> getAttachments(string emailAddress, string id)
        {

            List<FileAttachment> retVal = new List<FileAttachment>() { };

            try
            {

                string mailRequestUrl = graphUrl + @"users/{0}/messages/{1}/attachments";

                string response = makeGraphRequest(String.Format(mailRequestUrl, emailAddress, id), HttpMethod.Get);

                if (response != null)
                {
                    Dictionary<string, object> respObj = graphClient.HttpProvider.Serializer.DeserializeObject<Dictionary<string, object>>(response);
                    retVal = graphClient.HttpProvider.Serializer.DeserializeObject<List<FileAttachment>>(respObj.Where(q => q.Key == "value").FirstOrDefault().Value.ToString());
                }

            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Download Attachments", ex);
                }

            }

            return retVal;

        }

        public string getMessageId(FHMC.Graph.Email.Message msg)
        {
            return msg.Id;
        }

        public List<string> getMessageHeaders(FHMC.Graph.Email.Message msg)
        {
            List<String> retVal = new List<string>() { };

            if (msg.InternetMessageHeaders != null)
            {
                foreach (InternetMessageHeader header in msg.InternetMessageHeaders)
                {
                    retVal.Add(header.Name + ":" + header.Value);
                }
            }

            return retVal;
        }

        public void sendMessageJSON(IMailMessage msg, FHMC.Interfaces.Email.EmailSender sender)
        {

            if (enteredKey != SendKey)
            {
                throw new GraphAPICustomException("Invalid Email Send Key");
            }

            try
            {
                
                string emailAddress = String.Empty;

                if (sender == EmailSender.Alerts)
                {
                    emailAddress = "alerts@firsthome.com";
                }
                else if (sender == EmailSender.VOE)
                {
                    emailAddress = "voe@firsthome.com";
                }
                else
                {
                    throw new GraphAPICustomException("Invalid Email Sender");
                }
                
                Log.Info("Sending message for " + emailAddress + ": " + msg.Subject);

                string mailSendUrl = graphUrl + @"users/{0}/sendMail";

                string content = "{ \"message\": " + JsonConvert.SerializeObject(transformMessageToGraphMessage(msg), 
                        Newtonsoft.Json.Formatting.None,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            Converters = new List<JsonConverter>() { new Newtonsoft.Json.Converters.StringEnumConverter(true) }
                        }) + "}";

                content = content.Replace("ODataType", "@odata.type");

                string response = makeGraphRequest(String.Format(mailSendUrl, emailAddress), HttpMethod.Post, content);


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Send Mail JSON", ex);
                    throw ex;
                }

            }

        }

        public void sendMessageMIME(IMailMessage msg, FHMC.Interfaces.Email.EmailSender sender)
        {

            if (enteredKey != SendKey)
            {
                throw new GraphAPICustomException("Invalid Email Send Key");
            }

            try
            {

                string emailAddress = String.Empty;

                if (sender == EmailSender.Alerts)
                {
                    emailAddress = "alerts@firsthome.com";
                }
                else if (sender == EmailSender.VOE)
                {
                    emailAddress = "voe@firsthome.com";
                }
                else
                {
                    throw new GraphAPICustomException("Invalid Email Sender");
                }

                msg.SenderEmailAddress = emailAddress;

                Log.Info("Sending MIME message for " + emailAddress + ": " + msg.Subject);

                string mailSendUrl = graphUrl + @"users/{0}/sendMail";

                byte[] content = transformMessageToMIMEMessage(msg);

                string response = makeGraphRequest(String.Format(mailSendUrl, emailAddress), HttpMethod.Post, content, null, ContentType.text_plain);


            }
            catch (Exception ex)
            {
                if (!skipError(ex))
                {
                    Log.Error("GraphAPI Failed to Send Mail MIME ", ex);
                    throw ex;
                }

            }

        }

        private byte[] transformMessageToMIMEMessage(IMailMessage msg)
        {

            byte[] retVal = null;

            MimeKit.MimeMessage mimemessage = new MimeKit.MimeMessage();

            mimemessage.From.Add(new MimeKit.MailboxAddress(isNull(msg.SenderEmailName, msg.SenderEmailAddress), msg.SenderEmailAddress));

            if (msg.ToRecipients != null)
            {
                foreach (IMailRecipient recip in msg.ToRecipients)
                {
                    mimemessage.To.Add(new MimeKit.MailboxAddress(recip.EmailAddress, recip.EmailAddress));
                }
            }
            if (msg.CcRecipients != null)
            {
                foreach (IMailRecipient recip in msg.CcRecipients)
                {
                    mimemessage.Cc.Add(new MimeKit.MailboxAddress(recip.EmailAddress, recip.EmailAddress));
                }
            }
            if (msg.BccRecipients != null)
            {
                foreach (IMailRecipient recip in msg.BccRecipients)
                {
                    mimemessage.Bcc.Add(new MimeKit.MailboxAddress(recip.EmailAddress, recip.EmailAddress));
                }
            }
            if (msg.ReplyToList != null)
            {
                foreach (IMailRecipient recip in msg.ReplyToList)
                {
                    mimemessage.ReplyTo.Add(new MimeKit.MailboxAddress(recip.EmailAddress, recip.EmailAddress));
                }
            }

            mimemessage.Subject = msg.Subject;

            // TODO:if there are multiple parts, text/html part should be added second
            MimeKit.MultipartAlternative content = new MimeKit.MultipartAlternative();
            MimeKit.TextPart textPart = null;
            if (msg.IsBodyHTML)
            {
                textPart = new MimeKit.TextPart("html");
            }
            else
            {
                textPart = new MimeKit.TextPart("plain");
            }
            textPart.Text = msg.Body;
            content.Add(textPart);

            if (msg.IsReadReceiptRequested)
            {
                mimemessage.Headers.Add(MimeKit.HeaderId.DispositionNotificationTo, msg.SenderEmailAddress);
            }

            if (msg.Importance != null)
            {
                mimemessage.Importance = (MimeKit.MessageImportance)Enum.Parse(typeof(MimeKit.MessageImportance), msg.Importance.Value.ToString());
            }

            if (msg.CustomSendHeaders != null)
            {
                foreach (KeyValuePair<string, string> header in msg.CustomSendHeaders)
                {
                    mimemessage.Headers.Add(new MimeKit.Header(header.Key, header.Value));
                }
            }

            if (msg.Attachments != null)
            {
                foreach (IMailAttachment att in msg.Attachments)
                {

                    MimeKit.MimePart attPart = new MimeKit.MimePart()
                    {
                        Content = new MimeKit.MimeContent(new System.IO.MemoryStream(att.ContentBytes), MimeKit.ContentEncoding.Default),
                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                        ContentTransferEncoding = MimeKit.ContentEncoding.Base64,
                        FileName = att.Name
                    };
                    content.Add(attPart);
                }

            }

            mimemessage.Body = content;

            Log.Trace(mimemessage.ToString());

            //write mime message to byte array
            using (System.IO.MemoryStream messageStream = new System.IO.MemoryStream())
            {
                mimemessage.WriteTo(messageStream);
                messageStream.Position = 0;
                retVal = messageStream.ToArray();
            }
            
            return retVal;


        }


    }
}
