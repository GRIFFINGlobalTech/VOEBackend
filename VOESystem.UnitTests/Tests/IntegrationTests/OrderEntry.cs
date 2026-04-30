using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using Protractor;
using VOESystem.Data.DTO;
using VOESystem.UnitTests.Business;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;


namespace VOESystem.UnitTests.Tests.IntegrationTests
{

    [TestFixture]
    public class OrderEntry : UITestBase
    {
        [Test]
        public void Test_Intgr_OrderEntry_DuplicateOrderBlock()
        {

            UI.Model.OrderEntry orderEntryPage = null;

            OrderOps oOp = new OrderOps();
            int tryCount = 0;
            bool bLoanNumberFound = false;
            string LoanNumber = null;

            while (tryCount <= 20)
            {

                //make sure we get through all this part, if not, get another loan number
                try
                {
                    LoanNumber = getRandomLoanNumberByCriteria(
                        new List<string> { "Pending" },
                        new List<string> { "Initial" },
                        new List<string> { },
                        new List<string> { "Current" },
                        180
                        );

                    //this loan can't have any data corrections on it already
                    if (getOrdersByCriteria(new List<string> { },
                        new List<string> { "Data Correction" },
                        new List<string> { },
                        new List<string> { },
                        0, null, LoanNumber).Count > 0)
                    {
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }

                    LogLoanNumber(LoanNumber);

                    string pageURL = "orderentry?loannumber=" + LoanNumber;
                    Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

                    //init model
                    orderEntryPage = new UI.Model.OrderEntry(ngDriver);

                    //check for loan copy popup
                    if (orderEntryPage.YesNoPopup != null)
                    {
                        if (orderEntryPage.YesNoPopup.IsVisible)
                        {
                            orderEntryPage.YesNoPopup.NoButton.Click();
                        }
                    }

                    //select borrower drop-down and select first borrower
                    orderEntryPage.BorrowerDropDown.Options.ToList()
                        .Where<NgWebElement>(q => !q.Text.Contains("Select Borrower"))
                        .FirstOrDefault().Click();

                    //select request type and select initial
                    NgWebElement dcOption = orderEntryPage.RequestTypeDropDown.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Initial"))
                        .FirstOrDefault();

                    //make sure that request type of data correction is available first
                    if (dcOption == null)
                    {
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }
                    dcOption.Click();

                    //see if self employment checkbox is selected, if yes need to fill out CPA fields
                    if (orderEntryPage.SelfEmployedCheckbox.Checked == true)
                    {
                        orderEntryPage.Receives1099DropDown.Options.ToList()
                            .Where<NgWebElement>(q => q.Text.Contains("Yes"))
                            .FirstOrDefault().Click();

                        if (orderEntryPage.CPANameTextBox.Text == "") { orderEntryPage.CPANameTextBox.SendKeys("The CPA Name"); };
                        if (orderEntryPage.CPAPhoneTextBox.Text == "") { orderEntryPage.CPAPhoneTextBox.SendKeys("410-555-1212"); };
                        if (orderEntryPage.CPAEmailTextBox.Text == "") { orderEntryPage.CPAEmailTextBox.SendKeys("cpa@someplace.com"); };
                        if (orderEntryPage.BorrowerEmailTextBox.Text == "") { orderEntryPage.BorrowerEmailTextBox.SendKeys("person@needsmoney.com"); };

                    }

                    //get list of employers
                    List<UI.Model.OrderEntry.Employer> empList = orderEntryPage.Employers;

                    //select first employer that has an employment title
                    UI.Model.OrderEntry.Employer emp = empList.Where<UI.Model.OrderEntry.Employer>(q => q.EmploymentTitle.Text != "").First();
                    if (emp == null)
                    {
                        throw new OrderEntryExceptions.NoValidEmploymentException(LoanNumber);
                    }
                    emp.DoVerify.Click();

                    //select verbal order type
                    emp.VerficationType.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Verbal"))
                        .FirstOrDefault().Click();

                    //make sure there are linked orders
                    if (emp.LinkedOrder.Options.Count <= 1)
                    {
                        throw new OrderEntryExceptions.NoLinkedOrdersException(LoanNumber, emp.EmployerName);
                    }

                    //add order note
                    emp.OrderNote.SendKeys("this is an order note for the duplicate order block test!!!");

                    bLoanNumberFound = true;
                    break; //exit the while loop that reselects loannumbers
                }
                catch (OrderEntryException ex)
                {
                    logger.Info(ex.Message, ex);
                    tryCount++;
                    continue;
                }


            }

            if (!bLoanNumberFound)
            {
                Assert.Fail("Loan Number Not Found  TryCount Exceeded.");
            }

            
            //check the resulting popup
            try
            {
                //submit and create order
                orderEntryPage.SubmitButton.Click();
                Assert.That(orderEntryPage.WaitForAndDismissAlert("this is a duplicate order"));

            }
            catch (Exception ex)
            {
                Assert.Fail("Order was submitted");
            }


        }

        [Test]
        public void Test_Intgr_OrderEntry_DataCorrectionOrderEntry()
        {

            UI.Model.OrderEntry orderEntryPage = null;
            
            OrderOps oOp = new OrderOps();
            int tryCount = 0;
            string LoanNumber = null;

            while (tryCount <= 5)
            {

                //make sure we get through all this part, if not, get another loan number
                try
                {
                    LoanNumber = getRandomLoanNumberByCriteria(
                        new List<string> { "Approved" },
                        new List<string> { "Initial" },
                        new List<string> { "Active Loan" },
                        new List<string> { }
                        );

                    if (LoanNumber == "")
                    {
                        throw new TestExceptions.NoAvailLoanNumbersException();
                    }

                    LogLoanNumber(LoanNumber);

                    //get all orders
                    List<OrderSearchResp> orders = getOrdersByCriteria(new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { },
                        0, null, LoanNumber);

                    //make sure there are no final orders - that goofs up the count business logic
                    List<OrderSearchResp> finalOrders = orders.Where<OrderSearchResp>(q => q.RequestType == "Final").ToList();
                    if (finalOrders.Count > 0)
                    {
                        logger.Info("Test_Intgr_OrderEntry_DataCorrectionOrderEntry - Has Finals: " + LoanNumber);
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }

                    //make sure there are no pendings so this can't become a revision request
                    List<OrderSearchResp> pendingOrders = orders.Where<OrderSearchResp>(q => q.OrderStatus == "Pending").ToList();
                    if (pendingOrders.Count > 0)
                    {
                        logger.Info("Test_Intgr_OrderEntry_DataCorrectionOrderEntry - Has Pendings: " + LoanNumber);
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }

                    string pageURL = "orderentry?loannumber=" + LoanNumber;
                    Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

                    //init model
                    orderEntryPage = new UI.Model.OrderEntry(ngDriver);

                    //check for loan copy popup
                    if (orderEntryPage.YesNoPopup != null)
                    {
                        if (orderEntryPage.YesNoPopup.IsVisible)
                        {
                            orderEntryPage.YesNoPopup.NoButton.Click();
                        }
                    }

                    //select borrower drop-down and select borrower from order in system 
                    //so that we dont select a borrower without orders
                    string borrowerToSelect = orders.FirstOrDefault().BorrowerName;
                    orderEntryPage.BorrowerDropDown.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains(borrowerToSelect))
                        .FirstOrDefault().Click();

                    //select request type and select data correction
                    NgWebElement dcOption = orderEntryPage.RequestTypeDropDown.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Data Correction"))
                        .FirstOrDefault();

                    //make sure that request type of data correction is available first
                    if (dcOption == null)
                    {
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }
                    dcOption.Click();

                    //see if self employment checkbox is selected, if yes need to fill out CPA fields
                    if (orderEntryPage.SelfEmployedCheckbox.Checked == true)
                    {
                        orderEntryPage.Receives1099DropDown.Options.ToList()
                            .Where<NgWebElement>(q => q.Text.Contains("Yes"))
                            .FirstOrDefault().Click();

                        orderEntryPage.EmploymentSelfCertDropDown.Options.ToList()
                           .Where<NgWebElement>(q => q.Text.Contains("CPA Letter"))
                           .FirstOrDefault().Click();

                        if (orderEntryPage.CPANameTextBox.Text == "") { orderEntryPage.CPANameTextBox.SendKeys("The CPA Name"); };
                        if (orderEntryPage.CPAPhoneTextBox.Text == "") { orderEntryPage.CPAPhoneTextBox.SendKeys("410-555-1212"); };
                        if (orderEntryPage.CPAEmailTextBox.Text == "") { orderEntryPage.CPAEmailTextBox.SendKeys("cpa@someplace.com"); };
                        if (orderEntryPage.BorrowerEmailTextBox.Text == "") { orderEntryPage.BorrowerEmailTextBox.SendKeys("person@needsmoney.com"); };

                    }

                    //get list of employers
                    List<UI.Model.OrderEntry.Employer> empList = orderEntryPage.Employers;

                    //select first employer that has an employment title
                    UI.Model.OrderEntry.Employer emp = empList.Where<UI.Model.OrderEntry.Employer>(q => q.EmploymentTitle.Text != "").First();
                    if (emp == null)
                    {
                        throw new OrderEntryExceptions.NoValidEmploymentException(LoanNumber);
                    }
                    emp.DoVerify.Click();

                    //select verbal order type
                    emp.VerficationType.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Verbal"))
                        .FirstOrDefault().Click();

                    //make sure there are linked orders
                    if (emp.LinkedOrder.Options.Count <= 1)
                    {
                        throw new Exception("No Linked Orders found for loan: " + LoanNumber);
                    }

                    //select last linkedorder
                    emp.LinkedOrder.Options.ToList()
                        .Where<NgWebElement>(q => !q.Text.Contains("Select Original Order"))
                        .LastOrDefault().Click();

                    ////make this a rush request
                    //emp.IsRushRequest.Options.ToList()
                    //    .Where<NgWebElement>(q => q.Text.Contains("Yes"))
                    //    .FirstOrDefault().Click();

                    //add order note
                    emp.OrderNote.SendKeys("this is an order note for the data correction order entry test!!!");

                    //select data correction reason
                    emp.DataCorrectionReason.Options.ToList()
                       .Where<NgWebElement>(q => !q.Text.Contains("Select Reason"))
                       .FirstOrDefault().Click();

                    break; //exit the while loop that reselects loannumbers
                }
                catch (OrderEntryException ex)
                {
                    logger.Info(ex.Message, ex);
                    tryCount++;
                    continue;
                }
                

            }

            if (tryCount > 5) {
                throw new TestExceptions.NoAvailLoanNumbersException();
            }

            //submit and create order
            orderEntryPage.SubmitButton.Click();
            Assert.That(orderEntryPage.WaitForAndDismissAlert());
                
            //check that the email record is in the email table
            int lastCreatedOrderId = Int32.Parse(orderEntryPage.LastCreatedOrderRequestId.Text);

            List<Data.DTO.Email> emails = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation");
            Assert.That(emails.Count, Is.GreaterThanOrEqualTo(1));

            ////check that the email and rush request records are in the tables
            //List<Data.DTO.Email> rremails = oOp.getOrderEmails(lastCreatedOrderId, "Rush Request Receipt");
            //Assert.That(rremails.Count, Is.GreaterThanOrEqualTo(1));

            //Data.DTO.OrderDetailResp order = oOp.getOrderDetail(lastCreatedOrderId);
            //Assert.That((order.RushRequestStatus ?? "") == "Requested");

        }

        [Test]
        public void Test_Intgr_OrderEntry_CheckLinkedOrders()
        {

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp order = null;

            List<Data.DTO.OrderSearchResp> orders = null;
            int tryCount = 0;

            while (tryCount <= 10)  //gave this 10 when I restricted to loans w/o finals
            {

                //make sure we get through all this part, if not, get another loan number
                try
                {
                    string CancelledLoanNumber = getRandomLoanNumberByCriteria(
                        new List<string> { "Cancelled" },
                        new List<string> { "Initial" },
                        new List<string> { "Active Loan" },
                        new List<string> { "Current" }
                        );

                    if (CancelledLoanNumber == "")
                    {
                        throw new TestExceptions.NoAvailLoanNumbersException();
                    }


                    LogLoanNumber(CancelledLoanNumber);

                    //get all orders
                    orders = getOrdersByCriteria(new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { },
                        0, null, CancelledLoanNumber);

                    //make sure there are no final orders - that goofs up the count business logic
                    List<OrderSearchResp> finalOrders = orders.Where<OrderSearchResp>(q => q.RequestType == "Final").ToList();
                    if (finalOrders.Count > 0)
                    {
                        logger.Info("Test_Intgr_OrderEntry_CheckLinkedOrders - Has Finals: " + CancelledLoanNumber);
                        throw new OrderEntryExceptions.InvalidLoanNumberException(CancelledLoanNumber);
                    }

                    //get approved order
                    Data.DTO.OrderSearchResp approvedOrder = orders.Where(q => q.OrderStatus == "Approved" || q.OrderStatus == "Archived").FirstOrDefault();
                    if (approvedOrder == null)
                    {
                        logger.Info("Test_Intgr_OrderEntry_CheckLinkedOrders - Has No Approved: " + CancelledLoanNumber);
                        throw new OrderEntryExceptions.InvalidLoanNumberException(CancelledLoanNumber);
                    }

                    order = oOp.getOrderDetail(approvedOrder.OrderRequestId);

                    break; //exit the while loop that reselects loannumbers
                }
                catch (OrderEntryException ex)
                {
                    logger.Info(ex.Message, ex);
                    tryCount++;
                    continue;
                }


            }

            //check that there are no cancelled orders
            List<Data.DTO.LoanInfoLinkedOrder> linkedOrders = oOp.getLinkedOrders(order.LoanNumber, order.BorrowerFullName, order.BorrowerSSN);
            Assert.AreEqual(linkedOrders.Where(q => q.OrderStatus == "Cancelled").ToList().Count, 0);

            //ensure that the most recent order for each borrower/employer/empstatus is showing in linked orders
            List<Data.DTO.OrderSearchResp> mostRecentOrders = orders.Where(q => q.BorrowerName == order.BorrowerFullName && q.OrderStatus != "Cancelled")
                            .OrderBy(t => t.BorrowerName)
                            .ThenByDescending(w => w.OrderRequestId)
                            .GroupBy(q => q.BorrowerName + q.EmployerName + q.EncEmploymentStatus)
                            .Select(r => r.First())
                            .OrderBy(p => p.BorrowerName)
                            .ToList();

            //count should be the same**this is no longer correct as we are now showing data corrections as well
            //Assert.AreEqual(linkedOrders.Count, mostRecentOrders.Count);

            //all orders should be there
            List<int> linkedOrderIds = linkedOrders.Select<Data.DTO.LoanInfoLinkedOrder, int>(q => q.OrderRequestId).ToList();
            foreach (Data.DTO.OrderSearchResp recentOrder in mostRecentOrders)
            {
                Assert.Contains(recentOrder.OrderRequestId, linkedOrderIds);
            }

        }

        [Test]
        public void Test_Intgr_OrderEntry_NonBorrowerOrderEntry()
        {

            UI.Model.OrderEntry orderEntryPage = null;

            OrderOps oOp = new OrderOps();

            //need to get an order is a bond loan so we can add the non-borrower
            string LoanNumber = oOp.getLoanNumberBondProductType();

            if (LoanNumber == "")
            {
                throw new TestExceptions.NoAvailLoanNumbersException();
            }

            LogLoanNumber(LoanNumber);

            string pageURL = "orderentry?loannumber=" + LoanNumber;
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            orderEntryPage = new UI.Model.OrderEntry(ngDriver);

            //check for loan copy popup
            if (orderEntryPage.YesNoPopup != null)
            {
                if (orderEntryPage.YesNoPopup.IsVisible)
                {
                    orderEntryPage.YesNoPopup.NoButton.Click();
                }
            }

            //click on add non borrower link
            orderEntryPage.AddNonBorrowerLink.Click();

            //fill in non borrower information
            orderEntryPage.NonBorrowerNameTextBox.SendKeys("NonBorrower Guy");

            orderEntryPage.NonBorrowerAddressTextBox.SendKeys("123 Happy St\r\nRichmond, VA 23060");

            orderEntryPage.NonBorrowerDOBTextBox.SendKeys("01/01/1975");

            string nonBorrowerSSN = getRandomSSN();
            orderEntryPage.NonBorrowerSSNTextBox.SendKeys(nonBorrowerSSN);

            orderEntryPage.NonBorrowerGenderDropDown.Options.ToList()
                .Where<NgWebElement>(q => !q.Text.Contains("Select Gender"))
                .FirstOrDefault().Click();

            //fill in employer information
            orderEntryPage.NonBorrowerEmployerNameTextBox.SendKeys("Big Company");

            orderEntryPage.NonBorrowerEmployerAddressTextBox.SendKeys("888 Commerce St\r\nRichmond, VA 23060");

            orderEntryPage.NonBorrowerEmployerPhoneTextBox.SendKeys("804-555-1212");

            orderEntryPage.NonBorrowerEmploymentTitleTextBox.SendKeys("Worker");

            orderEntryPage.NonBorrowerEmploymentStatusDropDown.Options.ToList()
                .Where<NgWebElement>(q => q.Text.Contains("Current"))
                .FirstOrDefault().Click();

            orderEntryPage.NonBorrowerAuthFormFile.SendKeys(ResourcesPath + ResourcesFileNames.BorrowerAuthFormPDF.GetDescription().ToString());

            //add nonborrower
            orderEntryPage.AddContactButton.Click();

            //select request type
            orderEntryPage.RequestTypeDropDown.Options.ToList()
                .Where<NgWebElement>(q => !q.Text.Contains("Select") && !q.Text.Contains("Correction"))
                .FirstOrDefault().Click();

            //get list of employers
            List<UI.Model.OrderEntry.Employer> empList = orderEntryPage.Employers;

            //select first employer that has an employment title
            UI.Model.OrderEntry.Employer emp = empList.Where<UI.Model.OrderEntry.Employer>(q => q.EmploymentTitle.Text != "").First();
            if (emp == null)
            {
                throw new OrderEntryExceptions.NoValidEmploymentException(LoanNumber);
            }
            emp.DoVerify.Click();

            //select verbal order type
            emp.VerficationType.Options.ToList()
                .Where<NgWebElement>(q => q.Text.Contains("Verbal"))
                .FirstOrDefault().Click();

            //add order note
            emp.OrderNote.SendKeys("this is an order note for the nonborrower order entry test!!!");

            //submit and create order
            orderEntryPage.SubmitButton.Click();
            Assert.That(orderEntryPage.WaitForAndDismissAlert());

            //check that the email record is in the email table
            int lastCreatedOrderId = Int32.Parse(orderEntryPage.LastCreatedOrderRequestId.Text);

            List<Data.DTO.Email> emails = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation");
            Assert.That(emails.Count, Is.EqualTo(1));

        }

        [Test]
        public void Test_Intgr_OrderEntry_RevisionRequestOrderEntry()
        {

            UI.Model.OrderEntry orderEntryPage = null;
            OrderOps oOp = new OrderOps();

            int linkedOrderRequestId = 0;
            int tryCount = 0;

            while (tryCount <= 5)
            {

                //make sure we get through all this part, if not, get another loan number
                try
                {
                    string LoanNumber = getRandomLoanNumberByCriteria(
                        new List<string> { "Pending" },
                        new List<string> { "Initial" },
                        new List<string> { "Active Loan" },
                        new List<string> { "Current" }
                        );

                    LogLoanNumber(LoanNumber);
                                     
                    //login using forward url
                    string pageURL = "orderentry?loannumber=" + LoanNumber;
                    Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

                    //init model
                    orderEntryPage = new UI.Model.OrderEntry(ngDriver);

                    //check for loan copy popup
                    if (orderEntryPage.YesNoPopup != null)
                    {
                        if (orderEntryPage.YesNoPopup.IsVisible)
                        {
                            orderEntryPage.YesNoPopup.NoButton.Click();
                        }
                    }

                    //select borrower drop-down and select borrower from order
                    orderEntryPage.BorrowerDropDown.Options.ToList()
                        .Where<NgWebElement>(q => !q.Text.Contains("Select Borrower"))
                        .FirstOrDefault().Click();

                    //get the order number we are trying to revise
                    OrderSearchResp order = getOrdersByCriteria(
                        new List<string> { "Pending" },
                        new List<string> { "Initial" },
                        new List<string> { "Active Loan" },
                        new List<string> { "Current" },
                        0, null,
                        LoanNumber)
                        .Where<OrderSearchResp>(q => q.BorrowerName.Contains(
                            orderEntryPage.BorrowerDropDown.SelectedText.Substring(0,5)))
                            .FirstOrDefault();

                    if (order == null)
                    {
                        throw new OrderEntryExceptions.InvalidLoanNumberException(LoanNumber);
                    }
                          
                    //select request type and select data correction
                    NgWebElement dcOption = orderEntryPage.RequestTypeDropDown.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Data Correction"))
                        .FirstOrDefault();
                    dcOption.Click();

                    //see if self employment checkbox is selected, if yes need to fill out CPA fields
                    if (orderEntryPage.SelfEmployedCheckbox.Checked == true)
                    {
                        orderEntryPage.Receives1099DropDown.Options.ToList()
                            .Where<NgWebElement>(q => q.Text.Contains("Yes"))
                            .FirstOrDefault().Click();

                        orderEntryPage.EmploymentSelfCertDropDown.Options.ToList()
                            .Where<NgWebElement>(q => q.Text.Contains("CPA Letter"))
                            .FirstOrDefault().Click();

                        orderEntryPage.CPANameTextBox.SendKeys("The Revised CPA Name"); //this is to force a change for the revision
                        if (orderEntryPage.CPAPhoneTextBox.Text == "") { orderEntryPage.CPAPhoneTextBox.SendKeys("410-555-1212"); };
                        if (orderEntryPage.CPAEmailTextBox.Text == "") { orderEntryPage.CPAEmailTextBox.SendKeys("cpa@someplace.com"); };
                        if (orderEntryPage.BorrowerEmailTextBox.Text == "") { orderEntryPage.BorrowerEmailTextBox.SendKeys("person@needsmoney.com"); };

                    }

                    //get list of employers
                    List<UI.Model.OrderEntry.Employer> empList = orderEntryPage.Employers;

                    //select first employer that has an employment title
                    UI.Model.OrderEntry.Employer emp = empList.Where<UI.Model.OrderEntry.Employer>(
                        q => q.EmployerName.StartsWith(order.EmployerName.Substring(0,5)) && q.EmploymentTitle.Text != "").FirstOrDefault();
                    if (emp == null)
                    {
                        throw new OrderEntryExceptions.NoValidEmploymentException(LoanNumber);
                    }
                    emp.DoVerify.Click();

                    //select verbal order type
                    emp.VerficationType.Options.ToList()
                        .Where<NgWebElement>(q => q.Text.Contains("Verbal"))
                        .FirstOrDefault().Click();

                    //make sure there are linked orders
                    if (emp.LinkedOrder.Options.Count <= 1)
                    {
                        throw new OrderEntryExceptions.NoLinkedOrdersException(LoanNumber, emp.EmployerName);
                    }

                    //select last linkedorder
                    DropDownBox.DropDownBoxOption linkedOrder = emp.LinkedOrder.Options.ToList()
                        .Where<DropDownBox.DropDownBoxOption>(q => !q.Text.Contains("Select Original Order")
                            && q.Text.Contains(emp.EmployerName))
                        .OrderBy(r => Int32.Parse(r.Value))
                        .LastOrDefault();

                    //make sure linked order was found
                    if (linkedOrder == null)
                    {
                        throw new OrderEntryExceptions.NoLinkedOrdersException(LoanNumber, emp.EmployerName);
                    }

                    linkedOrder.Click();

                    //if the revision request goes off right - then this should be the order requestid
                    linkedOrderRequestId = Int32.Parse(linkedOrder.Value);

                    //add order note
                    emp.OrderNote.SendKeys("this is an order note for the revision request order entry test!!!");

                    //select data correction reason
                    emp.DataCorrectionReason.Options.ToList()
                       .Where<NgWebElement>(q => !q.Text.Contains("Select Reason"))
                       .FirstOrDefault().Click();

                    break; //exit the while lookp that reselects loannumbers
                }
                catch (OrderEntryException ex)
                {
                    logger.Info(ex.Message, ex);
                    tryCount++;
                    continue;
                }

                
            }


            //submit and create order
            orderEntryPage.SubmitButton.Click();
            Assert.That(orderEntryPage.WaitForAndDismissAlert());

            //check that the the orderrequestid returned to the browser is the same as the selected linked order
            int lastCreatedOrderId = Int32.Parse(orderEntryPage.LastCreatedOrderRequestId.Text);
            Assert.That(lastCreatedOrderId, Is.EqualTo(linkedOrderRequestId));

            Data.DTO.OrderDetailResp revisedOrder = oOp.getOrderDetail(lastCreatedOrderId);

            //check that the email records are in the email table
            Data.DTO.Email email = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation")
                .OrderByDescending(q => q.EmailDateTime).FirstOrDefault();
            Assert.That((email.EmailDateTime ?? DateTime.Today).Date, Is.EqualTo(DateTime.Today));

            email = oOp.getOrderEmails(lastCreatedOrderId, "Revision Request Received")
                .OrderByDescending(q => q.EmailDateTime).FirstOrDefault();
            Assert.That((email.EmailDateTime ?? DateTime.Today).Date, Is.EqualTo(DateTime.Today));
                
            //check for revision request flag  orderactivity isrevision
            Data.DBSchema.OrderActivityView orderActivity = oOp.getLastOrderActivity(lastCreatedOrderId);
            Assert.That(orderActivity.IsRevision, Is.True);

          
        }

        public class OrderEntryException : TestException
        {
            
            public string LoanNumber = String.Empty;

            public OrderEntryException() { }

            public OrderEntryException(string loanNumber)
                : base() {
                    LoanNumber = loanNumber;
            }
        }

        public static class OrderEntryExceptions
        {
            public class NoValidEmploymentException : OrderEntryException
            {

                public override string Message
                {
                    get
                    {
                        return "No Employments with Valid Titles Found for loan: " + this.LoanNumber;
                    }
                }

                public NoValidEmploymentException(string loanNumber)
                    : base(loanNumber) { }

            }

            public class NoLinkedOrdersException : OrderEntryException
            {

                public string Employment;

                public override string Message
                {
                    get
                    {
                        return "No Linked Orders for Employment: " + Employment + "; Loan: " + LoanNumber;
                    }
                }

                public NoLinkedOrdersException(string loanNumber, string employment)
                    : base(loanNumber)
                {
                    Employment = employment;
                }

            }

            public class InvalidLoanNumberException : OrderEntryException
            {

                public override string Message
                {
                    get
                    {
                        return "Loan Number is not valid for this test: " + this.LoanNumber;
                    }
                }

                public InvalidLoanNumberException(string loanNumber)
                    : base(loanNumber) { }

            }

        }

        

        

    }
}
