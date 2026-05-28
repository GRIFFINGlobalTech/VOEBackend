using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Interfaces
{

    namespace Email
    {
        public interface IMailMessage
        {

            string Id { get; set; }
            string Subject { get; set; }
            DateTime ReceivedDateTime { get; set; }
            List<IMailAttachment> Attachments { get; set; }
            string Body { get; set; }
            bool IsBodyHTML { get; set; }
            string BodyHTML { get; set; }
            string SenderEmailAddress { get; set; }
            string SenderEmailName { get; set; }
            string RawReceiptHeaders { get; }
            Dictionary<string, string> CustomSendHeaders { get; set; }
            List<IMailRecipient> ToRecipients { get; set; }
            List<IMailRecipient> CcRecipients { get; set; }
            List<IMailRecipient> BccRecipients { get; set; }
            List<IMailRecipient> ReplyToList { get; set; }
            bool IsReadReceiptRequested { get; set; }
            EmailImportance? Importance { get; set; }
        }

        public interface IMailAttachment
        {
            string Id { get; set; }
            byte[] ContentBytes { get; set; }
            string ContentType { get; set; }
            string Name { get; set; }
            int? Size { get; set; }
        }

        public interface IMailRecipient
        {
            string EmailAddress { get; set; }
        }

        public enum EmailSender
        {
            Alerts,
            VOE
        }

        public enum EmailImportance
        {
            High,
            Low,
            Normal
        }

    }

}
