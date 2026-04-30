using System;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VOEBackend.Email.ExchangeWebService; 

namespace VOEBackend.Email
{
    public class Exchange
    {

        private void testMain()
        {
            // Connect to Exchange Web Services
            //ExchangeService service = new ExchangeService();
            //service.Credentials = new WebCredentials("iadmin", "xx");
            //service.AutodiscoverUrl("iadministrator@firsthome.com");

            // Create the e-mail message, set its properties, and send it to user2@contoso.com, saving a copy to the Sent Items folder. 
            //EmailMessage message = new EmailMessage(service);
            //message.Subject = "Test from Exchange API";
            //message.Body = "This is only a test";
            //message.ToRecipients.Add("cdesimone@firsthome.com");
            //message.SendAndSaveCopy();

            //service.ImpersonatedUserId = new ImpersonatedUserId(ConnectingIdType.SmtpAddress, "cbicknell@firsthome.com");


            //OofSettings oofSet = service.GetUserOofSettings("cbicknell@firsthome.com");
            //OofState oofSta = oofSet.State;

            //string msg = oofCall(service.Url.ToString(), "iadministrator@firsthome.com");

        }

        //public List<VOESystem.Data.DBSchema.emdbUserInfoView> getOOOStatus(string[] emailAddresses)
        //{

        //    List<VOESystem.Data.DBSchema.emdbUserInfoView> retVal = new List<VOESystem.Data.DBSchema.emdbUserInfoView>() { };

        //    ExchangeWebService.ExchangeServiceBinding ews = new ExchangeWebService.ExchangeServiceBinding();
        //    ews.Url = "https://email.firsthome.com/EWS/Exchange.asmx";
        //    //ews.Url = "https://10.1.0.54/EWS/Exchange.asmx";
        //    ews.Credentials = new NetworkCredential("iadmin", "Typescogentbrisknecks1", "FHMC1");
        //    ews.RequestServerVersionValue = new RequestServerVersion();
        //    ews.RequestServerVersionValue.Version = ExchangeVersionType.Exchange2010;

        //    //build exchange request
        //    ExchangeWebService.GetMailTipsType mtRequest = new ExchangeWebService.GetMailTipsType();



        //    ExchangeWebService.EmailAddressType emailFrom = new ExchangeWebService.EmailAddressType();
        //    emailFrom.EmailAddress = "iadministrator@firsthome.com";
        //    emailFrom.RoutingType = "SMTP";
        //    mtRequest.SendingAs = emailFrom;

        //    mtRequest.MailTipsRequested = MailTipTypes.OutOfOfficeMessage;
            


        //    //batch email addresses in groups of 50
        //    int iEmailArrayPosition = 0;
        //    int iRecipPosition = 0;
        //    int batchSize = 50;
        //    int iLoopStartPosition = 0;


        //    //init recipient array
        //    if ( emailAddresses.Length < batchSize ) { batchSize = emailAddresses.Length; };
            
        //    while (iEmailArrayPosition < emailAddresses.Length)
        //    {
        //        mtRequest.Recipients = new EmailAddressType[batchSize];

        //        for (int counter = iLoopStartPosition; counter < emailAddresses.Length; counter++)
        //        {
        //            string emailAddress = emailAddresses[counter];

        //            if (emailAddress != "")
        //            {
        //                mtRequest.Recipients[iRecipPosition] = new EmailAddressType()
        //                {
        //                    RoutingType = "SMTP",
        //                    EmailAddress = emailAddress
        //                };
        //                iRecipPosition++;
        //            }
        //            iEmailArrayPosition++;

        //            //restrict to batch size
        //            if (iRecipPosition == batchSize)
        //            {
        //                break; //break out of batch loop
        //            }

        //        }

        //        //settings for next loop
        //        iLoopStartPosition = iEmailArrayPosition;
        //        if (emailAddresses.Length - iEmailArrayPosition < batchSize) { 
        //            batchSize = emailAddresses.Length - iEmailArrayPosition;
        //        };
        //        iRecipPosition = 0;

        //        //make request       
        //        ServicePointManager.ServerCertificateValidationCallback += (o, c, ch, er) => true;
        //        ExchangeWebService.GetMailTipsResponseMessageType mtResponse = ews.GetMailTips(mtRequest);

        //        foreach (MailTipsResponseMessageType responseMsg in mtResponse.ResponseMessages) {

        //            bool isOOO = false;

        //            if (responseMsg.MailTips.OutOfOffice == null)
        //            {
        //                //there is not currently an ooo
        //            }
        //            else if (responseMsg.MailTips.OutOfOffice.ReplyBody.Message == String.Empty)
        //            {
        //                //there is not currently an ooo
        //            }
        //            else
        //            {
        //                //there is curently an ooo
        //                isOOO = true;
        //            }

        //            retVal.Add(new VOESystem.Data.DBSchema.emdbUserInfoView
        //            {
        //                Email = responseMsg.MailTips.RecipientAddress.EmailAddress,
        //                IsOOO = isOOO
        //            });

        //        }

        //    }

        //    return retVal;

        //}

       

    }


}
