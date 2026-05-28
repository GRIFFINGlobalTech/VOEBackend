using FHMC.Interfaces.Email;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static FHMC.Graph.Email;

namespace FHMC.Email
{

    public class Office365 : BaseClass
    {

        private static string emailKey = "3c7e4200-7904-45f1-ac7b-68d41a01d2c6";

        public Office365(object logger) : base(logger) { }

        public delegate void MailMessageOperation(FHMC.Interfaces.Email.IMailMessage message);

        public delegate bool MailMessageDeleteOperation(FHMC.Interfaces.Email.IMailMessage message, DateTime? cutoffDate);

        public void getEmail(string emailAddress, string incomingMailbox, string processedMailbox, string errorMailbox, MailMessageOperation msgOperation, 
            string archiveMailbox = null, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {

            FHMC.Graph.Email graphEmailOps = new FHMC.Graph.Email(this.Log, emailKey);

            try
            {

                //download headers in inbox
                List<MailMessageAttributes> attr = new List<MailMessageAttributes>() { };
                attr.Add(MailMessageAttributes.receivedDateTime);
                attr.Add(MailMessageAttributes.sentDateTime);
                attr.Add(MailMessageAttributes.subject);
                attr.Add(MailMessageAttributes.internetMessageHeaders);             

                string filterCriteria = null;
                if (startDateTime != null && endDateTime != null)
                {
                    if (incomingMailbox == "Sent Items") 
                    {
                        filterCriteria = "$filter=sentDateTime gt " + (startDateTime ?? DateTime.Now).ToUniversalTime().ToString("yyyy-MM-ddThh:mmZ");
                        filterCriteria += "&sentDateTime lt " + (endDateTime ?? DateTime.Now).ToUniversalTime().ToString("yyyy-MM-ddThh:mmZ");
                    }
                    else
                    {
                        filterCriteria = "$filter=receivedDateTime gt " + (startDateTime ?? DateTime.Now).ToUniversalTime().ToString("yyyy-MM-ddThh:mmZ");
                        filterCriteria += "&receivedDateTime lt " + (endDateTime ?? DateTime.Now).ToUniversalTime().ToString("yyyy-MM-ddThh:mmZ");
                    }

                }


                List<Message> msgs = graphEmailOps.getMessages(emailAddress, incomingMailbox, false, attr, filterCriteria);

                //need to move messages last so not to mess up message indexes
                List<string> messagesToMoveProcessed = new List<string>() { };
                List<string> messagesToMoveError = new List<string>() { };

                foreach (FHMC.Graph.Email.Message msg in msgs)
                {

                    string messageId = graphEmailOps.getMessageId(msg);
                    List<string> headers = graphEmailOps.getMessageHeaders(msg);

                    try
                    {
                       
                        Message fullMessage = graphEmailOps.getMessage(emailAddress, messageId);
                        string senderEmailAddress = graphEmailOps.getMessageSenderEmailAddress(fullMessage);
                        string bodyHTML = graphEmailOps.getMessageBodyHTML(emailAddress, messageId, senderEmailAddress);  //this is only temporary??
                        List<FileAttachment> atts = graphEmailOps.getAttachments(emailAddress, messageId);
                        msgOperation(graphEmailOps.transformGraphMessageToMessage(fullMessage, atts, headers, bodyHTML));
                        messagesToMoveProcessed.Add(messageId);

                    }

                    catch (Exception ex)
                    {

                        messagesToMoveError.Add(messageId);

                    }

                }

                //archive message, if nec
                if (archiveMailbox != null)
                {
                    foreach (string messageUID in messagesToMoveProcessed)
                    {
                        graphEmailOps.duplicateMessage(emailAddress, messageUID, archiveMailbox);
                    }

                    foreach (string messageUID in messagesToMoveError)
                    {
                        graphEmailOps.duplicateMessage(emailAddress, messageUID, archiveMailbox);
                    }
                }

                //now move messages
                if (processedMailbox != null)
                {
                    foreach (string messageUID in messagesToMoveProcessed)
                    {
                        graphEmailOps.moveMessage(emailAddress, messageUID, processedMailbox);
                    }
                }

                if (errorMailbox != null)
                {
                    foreach (string messageUID in messagesToMoveError)
                    {
                        graphEmailOps.moveMessage(emailAddress, messageUID, errorMailbox);
                    }
                }

            }

            catch (Exception ex)
            {
                Log.Error("Email Retrieval Error", ex);
            }


        }

        public void deleteEmails(string emailAddress, string mailboxPath, DateTime cutoffDate, MailMessageDeleteOperation msgDeleteOperation)
        {

            FHMC.Graph.Email graphEmailOps = new FHMC.Graph.Email(this.Log, emailKey);

            try
            {
                                
                Log.Info("Getting messages older than " + cutoffDate.ToString("yyyy-MM-dd"));

                string filterCriteria = "$filter=receivedDateTime lt " + cutoffDate.ToString("yyyy-MM-dd"); 

                List<MailMessageAttributes> attr = new List<MailMessageAttributes>() { };
                attr.Add(MailMessageAttributes.subject);
                attr.Add(MailMessageAttributes.receivedDateTime);

                List<Message> msgs = graphEmailOps.getMessages(emailAddress, mailboxPath, false, attr, filterCriteria);

                Log.Info("Deleting " + msgs.Count.ToString() + " messages");

                foreach (FHMC.Graph.Email.Message msg in msgs)
                {

                    string messageId = graphEmailOps.getMessageId(msg);
                    
                    try
                    {

                        Message fullMessage = graphEmailOps.getMessage(emailAddress, messageId);
                        List<FileAttachment> atts = graphEmailOps.getAttachments(emailAddress, messageId);
                        IMailMessage message = graphEmailOps.transformGraphMessageToMessage(fullMessage, atts);
                        bool opResult = msgDeleteOperation?.Invoke(message, cutoffDate) ?? true;
                        if (opResult)
                        {
                            graphEmailOps.deleteMessage(emailAddress, messageId, message.Subject, mailboxPath);
                        }

                    }

                    catch (Exception ex)
                    {

                        Log.Error("Error Deleting Message", ex);
                        if (ex.Message.Contains("the token is expired"))
                        {
                            return;
                        }

                    }

                }


            }
            catch (Exception ex)
            {
                Log.Error("GraphAPI Failed to Delete Mail", ex);   

            }

        }

        public void sendEmail(IMailMessage message, EmailSender sender)
        {

            try
            {

                Log.Trace(JsonConvert.SerializeObject(message));

                FHMC.Graph.Email eop = new FHMC.Graph.Email(Log, emailKey);
                if (sender == EmailSender.VOE)
                {
                    eop.sendMessageMIME(message, sender);
                }
                else
                {
                    eop.sendMessageJSON(message, sender);


                }
                //eop.sendMessageJSON(message, sender);

                Log.Info("Email Sent - Subject: " + message.Subject);


            }
            catch (Exception ex)
            {
                Log.Error("FHMC.Email Error", ex);
                if (ex.InnerException != null)
                {
                    if (ex.InnerException.Message.Contains("ErrorInvalidRecipients"))
                    {
                        throw new InvalidRecipientException("One or more recipients is invalid.  Please check recipients and try again.");
                    }
                }
                
                throw ex;
                
            }

        }

        public class InvalidRecipientException : Exception, ISerializable
        {

            public InvalidRecipientException()
            {

            }
            public InvalidRecipientException(string message)
                : base(message)
            {

            }
            public InvalidRecipientException(string message, Exception inner)
                : base(message, inner)
            {

            }


        }


    }


}
