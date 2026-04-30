using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Cache;
using System.IO;
using FHMC.NLogWrapper;
using System.Net.Mail;
using VOESystem.Data.DBSchema;

namespace VOEBackend.Xactus.Business
{
    public class BaseClass : VOESystem.Data.Business.BusinessBase   {

        protected FHMC.NLogWrapper.Logger logger { get; private set; }


        //protected const string XactusServiceURL = @"test.ultraamps.com/uaweb/mismo";  //dev url
        protected const string XactusServiceURL = @"www.ultraamps.com/uaweb/mismo"; //prod url

        protected const string VENDORID = "firsthome";
        //protected const string ACCOUNTNUMBER = "meg73"; //dev username
        //protected const string PASSWORD = @"Cr3dit5587!"; //dev password
        //protected const string ACCOUNTNUMBER = "mswinehart"; //prod username
        //protected const string PASSWORD = @"R4r8t5r%"; //prod password
        protected const string ACCOUNTNUMBER = "voe.master"; //prod username
        protected const string PASSWORD = @"J6d3gg2d7z$"; //prod password


        public enum VerificationType
        {

            Current,
            Prior,
            All
        }

        public enum OrderType
        {
            Verbal = 1,
            Written = 2
        }

        public enum SubVendor
        {
            TWN = 1,
            Experian = 2
        }

        protected BaseClass()
        {
            logger = new FHMC.NLogWrapper.Logger(GetType().FullName);
        }

        public string isNull(object inString, string replVal)
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

        public DateTime isNullDateTime(DateTime? inDate, string replVal)
        {

            if (inDate == null)
            {
                return DateTime.Parse(replVal);
            }
            else if (inDate <= DateTime.Parse("1900-01-01"))
            {
                return DateTime.Parse(replVal);
            }
            else
            {
                return (DateTime)inDate;
            }

        }

       

        //public void SendEmail(List<string> toEmailList, List<string> ccEmailList, List<string> bccEmailList, string Subject, string MessageText)
        //{

        //    try
        //    {
        //        MailMessage message = new MailMessage();
        //        if (ccEmailList == null) { ccEmailList = new List<string>() { }; };
        //        if (bccEmailList == null) { bccEmailList = new List<string>() { }; };

        //        foreach (string recip in toEmailList)
        //        {
        //            if (recip != "")
        //            {
        //                message.To.Add(new MailAddress(recip));
        //            }
        //        }

        //        if (message.To.Count == 0)
        //        {
        //            throw new Exception("No Valid Email Recipients Specified");
        //        }

        //        //add ccs
        //        foreach (string recip in ccEmailList)
        //        {
        //            if (recip != "")
        //            {
        //                message.CC.Add(new MailAddress(recip));
        //            }
        //        }

        //        //add bccs
        //        foreach (string recip in bccEmailList)
        //        {
        //            if (recip != "")
        //            {
        //                message.Bcc.Add(new MailAddress(recip));
        //            }
        //        }

        //        message.IsBodyHtml = true;
        //        message.Subject = Subject;

        //        message.Body = MessageText.Replace("\n", "<BR>");

        //        FHMC.Email.SMTP.SMTPClient emailClient = new FHMC.Email.SMTP.SMTPClient(logger);
        //        emailClient.Send(message, FHMC.Email.SMTP.EmailSender.Alerts);

        //        logger.Info("Email Notification Sent: " + Subject);
        //        logger.Info(MessageText);

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error("Error Sending Email", ex);
        //        logger.Info(MessageText);
        //        throw ex;
        //    }


        //}


    }
}
