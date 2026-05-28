using Newtonsoft.Json;
using System.Collections.Generic;

namespace TestConsole
{
    class Program
    {

        static FHMC.NLogWrapper.Logger Log;

        static void Main(string[] args)
        {

            Log = new FHMC.NLogWrapper.Logger("aLogger");

            //FHMC.Email.SMTP.SMTPClient client = new FHMC.Email.SMTP.SMTPClient(Log);

            //MailMessage mail = new MailMessage();
            //mail.To.Add(@"christine.desimone@gmail.com");
            //mail.Subject = "Test 777 send from fhmc.mail";
            //mail.Body = "Testing777!!!!<B>This is really a test of html</B>";
            //mail.IsBodyHtml = true;

            //client.Send(mail, FHMC.Email.SMTP.EmailSender.Alerts);

            FHMC.Email.Office365 eop = new FHMC.Email.Office365(Log);
            FHMC.Interfaces.Email.IMailMessage message = new FHMC.Graph.Email.MailMessage();

            message.Subject = "Test Graph Message 9";
            message.Body = "Testing100000000!!!!<B>This is really a test of html</B>";
            message.ToRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>();
            //message.CcRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>();
            //message.BccRecipients = new List<FHMC.Interfaces.Email.IMailRecipient>();
            message.ToRecipients.Add(new FHMC.Graph.Email.MailRecipient
            {
                EmailAddress = "christine.desimone@gmail.com"
            });
            //message.CcRecipients.Add(new FHMC.Graph.Email.MailRecipient
            //{
            //    EmailAddress = "christine.desimone@gmail.com"
            //});
            //message.BccRecipients.Add(new FHMC.Graph.Email.MailRecipient
            //{
            //    EmailAddress = "christine.desimone@gmail.com"
            //});
            //message.SenderEmailAddress = "alerts@firsthome.com";
            //message.SenderEmailName = "Alerts";

            //message.IsReadReceiptRequested = true;
            message.IsBodyHTML = true;
            //message.Importance = FHMC.Interfaces.Email.EmailImportance.High;

            //message.CustomSendHeaders = new Dictionary<string, string>() { };
            //message.CustomSendHeaders.Add("X-MyCustomHeader", "WooHoo!!!!");

            //byte[] content = System.IO.File.ReadAllBytes(@"C:\Temp\CurrentFaxCover_1010506042-07_20250902091734392.pdf");

            //List<FHMC.Interfaces.Email.IMailAttachment> atts = new List<FHMC.Interfaces.Email.IMailAttachment>() { };
            //atts.Add(new FHMC.Graph.Email.MailAttachment
            //{
            //    Name = "Test File",
            //    ContentType = "application/pdf",
            //    ContentBytes = content,
            //    Size = content.Length
            //});

            //message.Attachments = atts;
            //Log.Info(JsonConvert.SerializeObject(message));
            eop.sendEmail(message, FHMC.Interfaces.Email.EmailSender.VOE);

            //FHMC.Email.Office365.Office365Client o365Client = new FHMC.Email.Office365.Office365Client(Log);
            //o365Client.getEmail("voe@firsthome.com", "Inbox", null, null, MailMessageOperation);

            //FHMC.Graph.FileIO fOp = new FHMC.Graph.FileIO(Log);

            Log.Info("Test Complete");

            //fOp.updateFile("b!CfpbjgMjwE2srFljyfkUw4znbL-KceRJvxhRjxk1gFsZ3OArrDiBTpVDsXeNUh-l", "01RL37UK4UQXPFTADQ2VEIPRD3KI2THUCL",
              //  @"C:\Temp\ContactSyncFiles\Jive_TitleCompany_20240917160110.xlsx", FHMC.Graph.BaseClass.ContentType.application_vnd_ms_excel);



        }

        private static void MailMessageOperation(FHMC.Interfaces.Email.IMailMessage message)
        {
            Log.Info(message.Subject);
        }

    }
}
