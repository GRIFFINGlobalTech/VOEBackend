using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Email.SMTP
{

//    public enum EmailSender
//    {
//        Alerts,
//        VOE
//    }


//    public class SMTPClient : BaseClass
//    {

//        //public EmailClient() : base() { }
//        public SMTPClient(object Log) : base(Log) { }


//        public void Send(MailMessage message, EmailSender sender)
//        {

//            int iEmailTryCounter = 0;

//emRetry:    try
//            {
//                iEmailTryCounter += 1;

//                string userName = null;
//                string password = null;


//                if (sender == EmailSender.Alerts)
//                {
//                    userName = "alerts@firsthome.com";
//                    password = @"";  //03-10-2025
//                }
//                else if (sender == EmailSender.VOE)
//                {
//                    userName = "voe@firsthome.com";
//                    password = "";
//                }

//                if (password == "")
//                {
//                    throw new Exception("Email Password not Updated");
//                }

//                string serverAddress = "outlook.office365.com";
//                int portNumber = 587;

//                message.From = new MailAddress(userName);
//                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

//                SmtpClient emailClient = new SmtpClient(serverAddress);
//                emailClient.Port = portNumber;

//                emailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
//                emailClient.EnableSsl = true;

//                Log.Info("Subject: " + message.Subject);

//                emailClient.Credentials = new System.Net.NetworkCredential(userName, password);
//                emailClient.Send(message);
//            }
//            catch (SmtpException smex)
//            {
//                //retry up to 5 times.  if still no success then throw error up to caller
//                if (iEmailTryCounter <= 5)
//                {
//                    Log.Info("Retrying FHMC.Mail Error", smex);
//                    System.Threading.Thread.Sleep(1000);
//                    goto emRetry;
//                }
//                else 
//                {
//                    Log.Error("FHMC.Mail Error", smex);
//                    throw smex;
//                }

//            }

//        }





//    }



}
