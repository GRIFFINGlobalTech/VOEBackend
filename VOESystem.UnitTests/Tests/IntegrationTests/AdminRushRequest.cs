using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using VOESystem.UnitTests.Business;
using VOESystem.UnitTests.UI;

namespace VOESystem.UnitTests.Tests.IntegrationTests
{

    [TestFixture]
    public class AdminRushRequest : UITestBase
    {

        [Test]
        public void Test_Intgr_Admin_RushRequestDenial()
        {
            OrderOps oOp = new OrderOps();

            int OrderRequestId = oOp.getOrderLastRushRequest();

            LogOrderNumber(oOp.getOrderNumberFromOrderRequestId(OrderRequestId));

            //get the email that was generated
            Data.DTO.Email rrEmail = oOp.getOrderEmails(OrderRequestId, "Rush Request Receipt")
                .OrderByDescending(q => q.Id).FirstOrDefault();
                
            //extract the link contained in the email
            //Regex regEx = new Regex("http(.+)");
            //string RushRequestApprovalLink = regEx.Match(rrEmail.Message).Value;

            //login and navigate to that link to the rush request approval screen
            string pageURL = "Administrator/RushRequest/order/" + OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);
           
            //init model
            UI.Model.AdminRushRequest adminRushRequestPage = null;
            adminRushRequestPage = new UI.Model.AdminRushRequest(ngDriver);

            //add a test denial note
            string testDenialNote = "This is a test denial note";
            adminRushRequestPage.DenialNoteTextArea.Clear();
            adminRushRequestPage.DenialNoteTextArea.SendKeys(testDenialNote);

            //click on deny button
            adminRushRequestPage.DenyRequestButton.Click();
            adminRushRequestPage.WaitForAndDismissAlert("rush request denied");

            //check that the email record is in the email table and contains the denial note
            List<Data.DTO.Email> emails = oOp.getOrderEmails(OrderRequestId, "Rush Request Denied");
            Assert.That(emails.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(emails[0].Message.ToLower().Contains(testDenialNote.ToLower()));


        }



    }
}
