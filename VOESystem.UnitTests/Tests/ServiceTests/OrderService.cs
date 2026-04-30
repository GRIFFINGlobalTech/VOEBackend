using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    [TestFixture]
    public class OrderService : ServiceTestBase
    {

        //"/api/order/add" - add regular order
        [Test]
        public void Test_Services_OrderService_CreateInitialOrder()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer> {
                new VOESystem.Data.DTO.Employer {
                    EncEmployerName = orderDetail.EncEmployerName + " NEW TEST",
                    EncEmployerPhone = orderDetail.EncEmployerPhone,
                    EncEmployerAddress = orderDetail.EncEmployerAddress,
                    EncEmploymentTitle = orderDetail.EncEmploymentTitle,
                    VerificationTypeID = orderDetail.OrderTypeId,
                    EncYearsOnJob = isNull(orderDetail.EncYearsOnJob,""),
                    EncMonthsOnJob = isNull(orderDetail.EncMonthsOnJob,""),
                    EncYearsInLineOfWork = isNull(orderDetail.EncYearsInLineOfWork,""),
                    EncEmployerFax = orderDetail.EncEmployerFax,
                    EncEmployerEmail = orderDetail.EncEmployerEmail,
                    EncStartDate = orderDetail.EncStartDate,
                    EncTerminationDate = orderDetail.EncTerminationDate,
                    EncEmploymentStatus = orderDetail.EncEmploymentStatus,
                    RequestNote = "This is a test of a normal initial order",
                    DoVerify = true,
                    VendorId = null,
                    ReverificationOrderRequestId = null,
                    LinkedOrderRequestId = null,
                    MilitaryStatus = null,
                    EncEmploymentSelfFlag = orderDetail.EncEmploymentSelfFlag,
                    DataCorrectionReasonId = null,
                    IsRushRequest = false,
                    IsNewEmployer = false
                }
            };

            Services.OrderService.NewOrderRequest request = new Services.OrderService.NewOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerAKAName = orderDetail.BorrowerAKAName,
                BorrowerDOB = (orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"),
                BorrowerSSN = orderDetail.BorrowerSSN,
                BorrowerEmail = orderDetail.BorrowerEmail,
                BorrowerHomePhone = orderDetail.BorrowerHomePhone,
                BorrowerMobilePhone = orderDetail.BorrowerMobilePhone,
                BorrowerGender = orderDetail.BorrowerGender,
                BorrowerAddress = orderDetail.BorrowerAddress,
                LoanNumber = orderDetail.LoanNumber,
                RequestTypeID = 1,
                Employers = emps,
                CPAName = orderDetail.CPAName,
                CPAPhone = orderDetail.CPAPhone,
                CPAEmail = orderDetail.CPAEmail,
                Status1099 = orderDetail.Status1099,
                SchedClosingDate = orderDetail.ScheduledClosingDate.ToString("MM/dd/yyyy"),
                EncLastMilestone = "Started",
                EncLoanStatus = "Active Loan",
                EncCurrentLoanFolder = "Active Loans",
                EncLoanOfficerName = orderDetail.EncLoanOfficerName,
                EncProcessorName = orderDetail.EncProcessorName,
                EncLoanAssistantName = null,
                IsNonBorrower = false,
                EncLoanType = orderDetail.EncLoanType,
                OrgId = orderDetail.LoanNumber.Substring(0,4),
                IsRIHousing = orderDetail.IsRIHousing
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem", 
                request.ToJson<Services.OrderService.NewOrderRequest>());

            NewOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            List<Data.DTO.Email> emails = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation");
            Assert.That(emails.Count, Is.EqualTo(1));

            
        }

        //"/api/order/add" - add datacorrection order
        [Test]
        public void Test_Services_OrderService_CreateDataCorrectionOrder()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer> {
                new VOESystem.Data.DTO.Employer {
                    EncEmployerName = orderDetail.EncEmployerName,
                    EncEmployerPhone = orderDetail.EncEmployerPhone,
                    EncEmployerAddress = orderDetail.EncEmployerAddress,
                    EncEmploymentTitle = orderDetail.EncEmploymentTitle,
                    VerificationTypeID = orderDetail.OrderTypeId,
                    EncYearsOnJob = isNull(orderDetail.EncYearsOnJob,""),
                    EncMonthsOnJob = isNull(orderDetail.EncMonthsOnJob,""),
                    EncYearsInLineOfWork = isNull(orderDetail.EncYearsInLineOfWork,""),
                    EncEmployerFax = orderDetail.EncEmployerFax,
                    EncEmployerEmail = orderDetail.EncEmployerEmail,
                    EncStartDate = orderDetail.EncStartDate,
                    EncTerminationDate = orderDetail.EncTerminationDate,
                    EncEmploymentStatus = orderDetail.EncEmploymentStatus,
                    RequestNote = "This is a test of a data correction order",
                    DoVerify = true,
                    VendorId = null,
                    ReverificationOrderRequestId = null,
                    LinkedOrderRequestId = OrderRequestId,  //link to initial order
                    MilitaryStatus = null,
                    EncEmploymentSelfFlag = orderDetail.EncEmploymentSelfFlag,
                    DataCorrectionReasonId = "1",
                    IsRushRequest = false,
                    IsNewEmployer = false
                }
            };

            Services.OrderService.NewOrderRequest request = new Services.OrderService.NewOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerAKAName = orderDetail.BorrowerAKAName,
                BorrowerDOB = (orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"),
                BorrowerSSN = orderDetail.BorrowerSSN,
                BorrowerEmail = orderDetail.BorrowerEmail,
                BorrowerHomePhone = orderDetail.BorrowerHomePhone,
                BorrowerMobilePhone = orderDetail.BorrowerMobilePhone,
                BorrowerGender = orderDetail.BorrowerGender,
                BorrowerAddress = orderDetail.BorrowerAddress,
                LoanNumber = orderDetail.LoanNumber,
                RequestTypeID = 2,
                Employers = emps,
                CPAName = orderDetail.CPAName,
                CPAPhone = orderDetail.CPAPhone,
                CPAEmail = orderDetail.CPAEmail,
                Status1099 = orderDetail.Status1099,
                SchedClosingDate = orderDetail.ScheduledClosingDate.ToString("MM/dd/yyyy"),
                EncLastMilestone = "Started",
                EncLoanStatus = "Active Loan",
                EncCurrentLoanFolder = "Active Loans",
                EncLoanOfficerName = orderDetail.EncLoanOfficerName,
                EncProcessorName = orderDetail.EncProcessorName,
                EncLoanAssistantName = null,
                IsNonBorrower = false,
                EncLoanType = orderDetail.EncLoanType,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                IsRIHousing = orderDetail.IsRIHousing
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem",
                request.ToJson<Services.OrderService.NewOrderRequest>());

            NewOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            List<Data.DTO.Email> emails = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation");
            Assert.That(emails.Count, Is.EqualTo(1));

            OrderDetailResp newOrderDetail = oOp.getOrderDetail(lastCreatedOrderId);

            //check that if the OLD specialist is order assignable it was assigned to that specialist and put into pending status
            UserRoleView user = getUserDetails(orderDetail.VerificationSpecialist);
            if (user != null)
            {
                if (user.IsEligibleOrderAssignment)
                {
                    Assert.That(newOrderDetail.VerificationSpecialist == newOrderDetail.VerificationSpecialist);
                    Assert.That(newOrderDetail.VerificationStatus == "Pending");
                }
            }
            else
            {
                //if not, it is in new
                Assert.That(newOrderDetail.VerificationStatus == "New");
            }

            //check that the data from the old order was copied to the new order (only need to check a few fields here)
            OrderActivityView oldOrderActivity = oOp.getLastOrderActivity(OrderRequestId);
            OrderActivityView newOrderActivity = oOp.getLastOrderActivity(lastCreatedOrderId);

            Assert.That(newOrderActivity.EmployerName == oldOrderActivity.EmployerName);
            Assert.That(newOrderActivity.EmploymentJobTitle == oldOrderActivity.EmploymentJobTitle);
            Assert.That(newOrderActivity.EmployerPhone == oldOrderActivity.EmployerPhone);


        }


        //"/api/order/add" - add revision request order
        [Test]
        public void Test_Services_OrderService_CreateRevisionRequestOrder()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer> {
                new VOESystem.Data.DTO.Employer {
                    EncEmployerName = orderDetail.EncEmployerName,
                    EncEmployerPhone = orderDetail.EncEmployerPhone,
                    EncEmployerAddress = orderDetail.EncEmployerAddress,
                    EncEmploymentTitle = orderDetail.EncEmploymentTitle,
                    VerificationTypeID = orderDetail.OrderTypeId,
                    EncYearsOnJob = isNull(orderDetail.EncYearsOnJob,""),
                    EncMonthsOnJob = isNull(orderDetail.EncMonthsOnJob,""),
                    EncYearsInLineOfWork = isNull(orderDetail.EncYearsInLineOfWork,""),
                    EncEmployerFax = orderDetail.EncEmployerFax,
                    EncEmployerEmail = orderDetail.EncEmployerEmail,
                    EncStartDate = orderDetail.EncStartDate,
                    EncTerminationDate = orderDetail.EncTerminationDate,
                    EncEmploymentStatus = orderDetail.EncEmploymentStatus,
                    RequestNote = "This is a test of a revision request order",
                    DoVerify = true,
                    VendorId = null,
                    ReverificationOrderRequestId = null,
                    LinkedOrderRequestId = OrderRequestId,  //link to initial order
                    MilitaryStatus = null,
                    EncEmploymentSelfFlag = orderDetail.EncEmploymentSelfFlag,
                    DataCorrectionReasonId = "1",
                    IsRushRequest = false,
                    IsNewEmployer = false
                }
            };

            Services.OrderService.NewOrderRequest request = new Services.OrderService.NewOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerAKAName = orderDetail.BorrowerAKAName,
                BorrowerDOB = (orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"),
                BorrowerSSN = orderDetail.BorrowerSSN,
                BorrowerEmail = orderDetail.BorrowerEmail,
                BorrowerHomePhone = orderDetail.BorrowerHomePhone,
                BorrowerMobilePhone = orderDetail.BorrowerMobilePhone,
                BorrowerGender = orderDetail.BorrowerGender,
                BorrowerAddress = orderDetail.BorrowerAddress,
                LoanNumber = orderDetail.LoanNumber,
                RequestTypeID = 2,
                Employers = emps,
                CPAName = orderDetail.CPAName,
                CPAPhone = orderDetail.CPAPhone,
                CPAEmail = orderDetail.CPAEmail,
                Status1099 = orderDetail.Status1099,
                SchedClosingDate = orderDetail.ScheduledClosingDate.ToString("MM/dd/yyyy"),
                EncLastMilestone = "Started",
                EncLoanStatus = "Active Loan",
                EncCurrentLoanFolder = "Active Loans",
                EncLoanOfficerName = orderDetail.EncLoanOfficerName,
                EncProcessorName = orderDetail.EncProcessorName,
                EncLoanAssistantName = null,
                IsNonBorrower = false,
                EncLoanType = orderDetail.EncLoanType,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                IsRIHousing = orderDetail.IsRIHousing
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem",
                request.ToJson<Services.OrderService.NewOrderRequest>());

            NewOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            //test that this is the same orderid as the original - that no new order was created
            Assert.That(lastCreatedOrderId == OrderRequestId);

            //check that the order is now in the pending status
            Assert.That(oOp.getOrderDetail(lastCreatedOrderId).VerificationStatus == "Pending");

            //check that the order confirm email record is in the email table
            Assert.That(oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation").Count >= 1);

            //check that the revision request email record is in the email table
            Assert.That(oOp.getOrderEmails(lastCreatedOrderId, "Revision Request Received").Count >= 1);

            //check that toast alert was created
            Assert.That(oOp.getOrderToastAlerts(lastCreatedOrderId)
                .Where<OpenToastAlertView>(q => q.AlertType.Contains("Revision Request")).ToList().Count > 0);


        }

        //"/api/order/add" - add regular order with Non-Borrower
        [Test]
        public void Test_Services_OrderService_CreateInitialNonBorrowerOrder()
        {

            int OrderRequestId = getOrdersByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90, null, null, null, 20, true)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer> {
                new VOESystem.Data.DTO.Employer {
                    EncEmployerName = orderDetail.EncEmployerName + " NEW TEST",
                    EncEmployerPhone = orderDetail.EncEmployerPhone,
                    EncEmployerAddress = orderDetail.EncEmployerAddress,
                    EncEmploymentTitle = orderDetail.EncEmploymentTitle,
                    VerificationTypeID = orderDetail.OrderTypeId,
                    EncYearsOnJob = isNull(orderDetail.EncYearsOnJob,""),
                    EncMonthsOnJob = isNull(orderDetail.EncMonthsOnJob,""),
                    EncYearsInLineOfWork = isNull(orderDetail.EncYearsInLineOfWork,""),
                    EncEmployerFax = orderDetail.EncEmployerFax,
                    EncEmployerEmail = orderDetail.EncEmployerEmail,
                    EncStartDate = orderDetail.EncStartDate,
                    EncTerminationDate = orderDetail.EncTerminationDate,
                    EncEmploymentStatus = orderDetail.EncEmploymentStatus,
                    RequestNote = "This is a test of a non borrower initial order",
                    DoVerify = true,
                    VendorId = null,
                    ReverificationOrderRequestId = null,
                    LinkedOrderRequestId = null,
                    MilitaryStatus = null,
                    EncEmploymentSelfFlag = orderDetail.EncEmploymentSelfFlag,
                    DataCorrectionReasonId = null,
                    IsRushRequest = false,
                    IsNewEmployer = true
                }
            };

            Services.OrderService.NewOrderRequest request = new Services.OrderService.NewOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerAKAName = orderDetail.BorrowerAKAName,
                BorrowerDOB = (orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"),
                BorrowerSSN = orderDetail.BorrowerSSN,
                BorrowerEmail = orderDetail.BorrowerEmail,
                BorrowerHomePhone = orderDetail.BorrowerHomePhone,
                BorrowerMobilePhone = orderDetail.BorrowerMobilePhone,
                BorrowerGender = orderDetail.BorrowerGender,
                BorrowerAddress = orderDetail.BorrowerAddress,
                LoanNumber = orderDetail.LoanNumber,
                RequestTypeID = 1,
                Employers = emps,
                CPAName = orderDetail.CPAName,
                CPAPhone = orderDetail.CPAPhone,
                CPAEmail = orderDetail.CPAEmail,
                Status1099 = orderDetail.Status1099,
                SchedClosingDate = orderDetail.ScheduledClosingDate.ToString("MM/dd/yyyy"),
                EncLastMilestone = "Started",
                EncLoanStatus = "Active Loan",
                EncCurrentLoanFolder = "Active Loans",
                EncLoanOfficerName = orderDetail.EncLoanOfficerName,
                EncProcessorName = orderDetail.EncProcessorName,
                EncLoanAssistantName = null,
                IsNonBorrower = true,
                EncLoanType = orderDetail.EncLoanType,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                IsRIHousing = orderDetail.IsRIHousing
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem",
                request.ToJson<Services.OrderService.NewOrderRequest>());

            NewOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            List<Data.DTO.Email> emails = oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation");
            Assert.That(emails.Count, Is.EqualTo(1));


        }

        //"/api/order/add" - add rush request order
        [Test]
        public void Test_Services_OrderService_CreateRushRequestOrder()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer> {
                new VOESystem.Data.DTO.Employer {
                    EncEmployerName = orderDetail.EncEmployerName + " NEW TEST",
                    EncEmployerPhone = orderDetail.EncEmployerPhone,
                    EncEmployerAddress = orderDetail.EncEmployerAddress,
                    EncEmploymentTitle = orderDetail.EncEmploymentTitle,
                    VerificationTypeID = orderDetail.OrderTypeId,
                    EncYearsOnJob = isNull(orderDetail.EncYearsOnJob,""),
                    EncMonthsOnJob = isNull(orderDetail.EncMonthsOnJob,""),
                    EncYearsInLineOfWork = isNull(orderDetail.EncYearsInLineOfWork,""),
                    EncEmployerFax = orderDetail.EncEmployerFax,
                    EncEmployerEmail = orderDetail.EncEmployerEmail,
                    EncStartDate = orderDetail.EncStartDate,
                    EncTerminationDate = orderDetail.EncTerminationDate,
                    EncEmploymentStatus = orderDetail.EncEmploymentStatus,
                    RequestNote = "This is a test of a rush request initial order",
                    DoVerify = true,
                    VendorId = null,
                    ReverificationOrderRequestId = null,
                    LinkedOrderRequestId = null,
                    MilitaryStatus = null,
                    EncEmploymentSelfFlag = orderDetail.EncEmploymentSelfFlag,
                    DataCorrectionReasonId = null,
                    IsRushRequest = true,
                    IsNewEmployer = false
                }
            };

            Services.OrderService.NewOrderRequest request = new Services.OrderService.NewOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerAKAName = orderDetail.BorrowerAKAName,
                BorrowerDOB = (orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"),
                BorrowerSSN = orderDetail.BorrowerSSN,
                BorrowerEmail = orderDetail.BorrowerEmail,
                BorrowerHomePhone = orderDetail.BorrowerHomePhone,
                BorrowerMobilePhone = orderDetail.BorrowerMobilePhone,
                BorrowerGender = orderDetail.BorrowerGender,
                BorrowerAddress = orderDetail.BorrowerAddress,
                LoanNumber = orderDetail.LoanNumber,
                RequestTypeID = 1,
                Employers = emps,
                CPAName = orderDetail.CPAName,
                CPAPhone = orderDetail.CPAPhone,
                CPAEmail = orderDetail.CPAEmail,
                Status1099 = orderDetail.Status1099,
                SchedClosingDate = orderDetail.ScheduledClosingDate.ToString("MM/dd/yyyy"),
                EncLastMilestone = "Started",
                EncLoanStatus = "Active Loan",
                EncCurrentLoanFolder = "Active Loans",
                EncLoanOfficerName = orderDetail.EncLoanOfficerName,
                EncProcessorName = orderDetail.EncProcessorName,
                EncLoanAssistantName = null,
                IsNonBorrower = false,
                EncLoanType = orderDetail.EncLoanType,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                IsRIHousing = orderDetail.IsRIHousing
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem",
                request.ToJson<Services.OrderService.NewOrderRequest>());

            NewOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            Assert.That(oOp.getOrderEmails(lastCreatedOrderId, "Order Confirmation").Count >= 1);

            //check that the rush request email is there
            Assert.That(oOp.getOrderEmails(lastCreatedOrderId, "Rush Request Receipt").Count >= 1);

        }


        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderSuccess()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                SalaryKey = null,
                EmployerCode = null,
                OrderTypeId = orderDetail.OrderTypeId,
                EquifaxTestResultMessage = "Success"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            Assert.That(oOp.getOrderEmails(lastCreatedOrderId, "Instant Verification Order Confirmation").Count >= 1);

            //check that cert doc is there and has nonzero filesize
            string filePathName = convertFileURLtoFilePathName(response.certURL.FromJson<List<string>>().FirstOrDefault());

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);
        }

        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderNoHit()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                OrderTypeId = orderDetail.OrderTypeId,
                SalaryKey = null,
                EmployerCode = null,
                EquifaxTestResultMessage = "Failure"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is failure
            Assert.That(response.message.ToLower().Contains("failure"));

            //test that there is still one orderrequest returned
            Assert.That(response.OrderRequestIds.Count == 1);

            
        }

        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderSalaryKey()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                OrderTypeId = orderDetail.OrderTypeId,
                SalaryKey = null,
                EmployerCode = null,
                EquifaxTestResultMessage = "Salary Key"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is failure
            Assert.That(response.message.ToLower().Contains("salary key"));

            //test that there is still one orderrequest returned
            Assert.That(response.OrderRequestIds.Count == 1);


        }

        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderEmployerCode()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                OrderTypeId = orderDetail.OrderTypeId,
                SalaryKey = null,
                EmployerCode = null,
                EquifaxTestResultMessage = "Employer Code"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is failure
            Assert.That(response.message.ToLower().Contains("employer code"));

            //test that there is still one orderrequest returned
            Assert.That(response.OrderRequestIds.Count == 1);


        }


        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderSalaryKeyRetry()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            //create initial order request that comes back with a salary key response
            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                OrderTypeId = orderDetail.OrderTypeId,
                SalaryKey = null,
                EmployerCode = null,
                EquifaxTestResultMessage = "Salary Key"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is failure
            Assert.That(response.message.ToLower().Contains("salary key"));

            //now retry it with the salary key
            request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = response.OrderRequestIds.FirstOrDefault(),
                SalaryKey = "AKEYOFSOMEKIND",
                EmployerCode = null,
                EquifaxTestResultMessage = "Success"
            };

            response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            //check that cert doc is there and has nonzero filesize
            string filePathName = convertFileURLtoFilePathName(response.certURL.FromJson<List<string>>().FirstOrDefault());

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);


        }

        //"/api/instantorder/add"
        [Test]
        public void Test_Services_OrderService_CreateInstantOrderEmployerCodeRetry()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            //create initial order request that comes back with a salary key response
            Services.OrderService.InstantOrderRequest request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = null,
                OrderTypeId = orderDetail.OrderTypeId,
                SalaryKey = null,
                EmployerCode = null,
                EquifaxTestResultMessage = "Employer Code"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>(false, null, null, @"http://localhost/voesystem");

            InstantOrderResp response = oService.Post(request);

            //test that there is failure
            Assert.That(response.message.ToLower().Contains("employer code"));

            //now retry it with the salary key
            request = new Services.OrderService.InstantOrderRequest
            {
                BorrowerFullName = orderDetail.BorrowerFullName,
                BorrowerSSN = orderDetail.BorrowerSSN,
                LoanNumber = orderDetail.LoanNumber,
                OrgId = orderDetail.LoanNumber.Substring(0, 4),
                OrderRequestId = response.OrderRequestIds.FirstOrDefault(),
                SalaryKey = null,
                EmployerCode = "ACODEOFSOMEKIND",
                EquifaxTestResultMessage = "Success"
            };

            response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.message.ToLower().Contains("error"));

            //test that there is one orderrequestid returned
            Assert.That(response.OrderRequestIds.Count == 1);

            //check that the email record is in the email table
            int lastCreatedOrderId = response.OrderRequestIds.FirstOrDefault();

            //check that cert doc is there and has nonzero filesize
            string filePathName = convertFileURLtoFilePathName(response.certURL.FromJson<List<string>>().FirstOrDefault());

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);


        }

        //"/api/order/copy"
        [Test]
        public void Test_Services_OrderService_CopyOrder()
        {
            string OriginalLoanNumber = null;
            string NewLoanNumber = null;
            bool bNeedLoanNumber = true;

            while(bNeedLoanNumber) {

                OriginalLoanNumber = getRandomLoanNumberByCriteria(new List<string> { "Approved" },
                    new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90);

                NewLoanNumber = "8888" + OriginalLoanNumber.Substring(4);

                //check to make sure we have not alraedy copied orders to this loan
                if(getOrdersByCriteria(new List<string> { "Approved" },
                    new List<string> { "Initial" }, new List<string> { }, new List<string> { },
                    90, null, NewLoanNumber).ToList().Count == 0)
                {
                    bNeedLoanNumber = false;
                }


            }

            List<int> orderIds = getOrdersByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90, null, OriginalLoanNumber)
                .Select<OrderSearchResp, int>(q => q.OrderRequestId).ToList();

            List<CopyOrdersReq.CopyOrders> copyOrders = new List<CopyOrdersReq.CopyOrders>() { };

            foreach(int orderId in orderIds)
            {
                copyOrders.Add(new CopyOrdersReq.CopyOrders
                {
                    OrderRequestId = orderId,
                    RequestTypeId = null, //this causes order to be initial
                    RequestNote = "This is a copy order test!",
                    IsRushRequest = false
                });
            }


            Services.OrderService.CopyOrdersRequest request = new Services.OrderService.CopyOrdersRequest
            {
                Orders = copyOrders,
                ToLoanNumber = NewLoanNumber,
                IsRIHousing = false
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            CopyOrdersResp response = oService.Post(request);

            //test that there is no error
            Assert.That(!response.Result.ToLower().Contains("error"));

            //test that the number of orders created is correct
            //this no longer works since there is a filter for 8888 loans in order search
            //List<int> newOrderIds = getOrdersByCriteria(new List<string> { },
            //    new List<string> { }, new List<string> { }, new List<string> { }, 90, null, NewLoanNumber)
            //    .Select<OrderSearchResp, int>(q => q.OrderRequestId).ToList();

            //Assert.That(newOrderIds.Count == orderIds.Count);

            //test that the order confirm email went out for all of them
            //UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            //foreach (int orderId in newOrderIds)
            //{
            //    Assert.That(oOp.getOrderEmails(orderId, "Order Confirmation", DateTime.Now.AddMinutes(-5)).Count == 1);
            //}
            
        }


        //"/api/order/search"
        [Test]
        public void Test_Services_OrderService_OrderSearchByLoanNumber()
        {

            //just running this to confirm there are no errors since we can only compare the results to the return of the same proc directly
            string LoanNumber = getRandomLoanNumberByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90);

            UnitTests.Business.BaseDataOps bOp = new Business.BaseDataOps();

            Services.OrderService.OrderSearchRequest request = new Services.OrderService.OrderSearchRequest
            {
                LoanNumber = LoanNumber,
                BorrowerNamePart = null,
                EmployerNamePart = null,
                VendorId = null,
                VendorReferenceNum = null,
                OrderStatuses = bOp.getOrderStatuses(true)
                    .Select<OrderStatus,OrderSearchReq.OrderStatus>(q => new OrderSearchReq.OrderStatus { Id = q.Id, Value = true }).ToList(),
                RequestTypes = bOp.getRequestTypes()
                    .Select<RequestType, OrderSearchReq.RequestType>(q => new OrderSearchReq.RequestType { Id = q.Id, Value = true }).ToList(),
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<OrderSearchResp> response = oService.Post(request);

            //test that there are records
            Assert.That(response.Count > 0);

        }

        //"/api/order/search"
        [Test]
        public void Test_Services_OrderService_OrderSearchByNameAndEmployer()
        {

            //just running this to confirm there are no errors since we can only compare the results to the return of the same proc directly
            UnitTests.Business.BaseDataOps bOp = new Business.BaseDataOps();

            Services.OrderService.OrderSearchRequest request = new Services.OrderService.OrderSearchRequest
            {
                LoanNumber = null,
                BorrowerNamePart = "Pom",
                EmployerNamePart = "N",
                VendorId = null,
                VendorReferenceNum = null,
                OrderStatuses = bOp.getOrderStatuses(true)
                    .Select<OrderStatus, OrderSearchReq.OrderStatus>(q => new OrderSearchReq.OrderStatus { Id = q.Id, Value = true }).ToList(),
                RequestTypes = bOp.getRequestTypes()
                    .Select<RequestType, OrderSearchReq.RequestType>(q => new OrderSearchReq.RequestType { Id = q.Id, Value = true }).ToList(),
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<OrderSearchResp> response = oService.Post(request);

            //test that there are records
            Assert.That(response.Count > 0);

        }

        //"/api/order/detail"
        [Test]
        public void Test_Services_OrderService_OrderDetail()
        {

            //just running this to confirm there are no errors since we can only compare the results to the return of the same proc directly
            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            Services.OrderService.OrderDetailRequest request = new Services.OrderService.OrderDetailRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderDetailResp response = oService.Post(request);

            //test that there are records
            Assert.That(response != null);

        }

        //"/api/order/revision/field/list"
        [Test]
        public void Test_Services_OrderService_ListRevisedFields()
        {

            //we need an order that has only been approved once (since the service has that restriction)
            int OrderRequestId = 0;
            int OrderApprovalCount = 0;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            while (OrderApprovalCount > 1) {

                OrderRequestId = getOrdersByCriteria(new List<string> { },
                    new List<string> { }, new List<string> { }, new List<string> { }, 90, null, null, true)
                    .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

                OrderApprovalCount = oOp.getOrderApprovalCount(OrderRequestId);

            }

            Services.OrderService.RevisedFieldListRequest request = new Services.OrderService.RevisedFieldListRequest
            {
               OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            RevisedFieldListResp response = oService.Post(request);

            //now get field list directly from DB
            List<FieldAudit> fieldList = oOp.getRevisedFieldList(OrderRequestId);

            //test that the response list is the same for both
            CollectionAssert.AreEqual(
                fieldList.Select<FieldAudit, string>(q => q.TableFieldName.Split("."[0])[1]).ToList().OrderBy(q => q).ToList(),
                response.FieldList.OrderBy(q => q).ToList());

        }



        //"/api/order/revision/cancel" 
        [Test]
        public void Test_Services_OrderService_CancelRevision()
        {

            int OrderRequestId = 0;

            //need a revision order
            OrderRequestId = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 180, null, null, true)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

            //need to create order lock first
            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            Services.OrderService.CancelRevisionRequest request = new Services.OrderService.CancelRevisionRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            //make cancellation request
            CancelRevisionResp response = oService.Post(request);

            //make sure it is no longer a revision in the db
            Assert.IsFalse(oOp.getOrderDetail(OrderRequestId).IsRevision);


        }


        //"/api/loan/borrower/list"
        [Test]
        public void Test_Services_OrderService_BorrowersForLoan()
        {

            string LoanNumber = getRandomLoanNumberByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { });

            Services.OrderService.BorrowersForLoanRequest request = new Services.OrderService.BorrowersForLoanRequest
            {
                LoanID = LoanNumber
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<Borrower> response = oService.Post(request);

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            List<Borrower> borrList = oOp.getBorrowerListForLoan(LoanNumber);

            //test that there are the same number of elements in both lists
            Assert.That(response.Count == borrList.Count);

            //test that the elements are the same
            foreach (Borrower bor in borrList)
            {
                Assert.That(response.Exists(q => q.BorrowerName == bor.BorrowerName && q.SSN == bor.SSN));
            }


        }

        //"/api/loan/order/list"
        [Test]
        public void Test_Services_OrderService_OrdersByLoanNumber()
        {

            string LoanNumber = getRandomLoanNumberByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { });

            Services.OrderService.OrdersByLoanNumberRequest request = new Services.OrderService.OrdersByLoanNumberRequest
            {
                LoanNumber = LoanNumber
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<OrdersByLoanNumberResp> response = oService.Post(request);
            
            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            List<OrderDetailView> orderList = oOp.getOrderDetailForLoanNumber(LoanNumber);

            //test that there are the same number of elements in both lists
            Assert.That(response.Count == orderList.Count);

            //test that the elements are the same
            foreach (OrderDetailView order in orderList)
            {
                Assert.That(response.Exists(q => q.OrderRequestId == order.OrderRequestId));
            }


        }

        //"/api/order/activity/list"
        [Test]
        public void Test_Services_OrderService_OrderActivity()
        {

            OrderSearchResp order = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { });

            Services.OrderService.OrderActivityRequest request = new Services.OrderService.OrderActivityRequest
            {
                OrderRequestId = order.OrderRequestId.ToString()
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<Data.DBSchema.OrderActivityView> response = oService.Post(request);

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            List<OrderActivityView> activityList = oOp.getOrderActivity(order.OrderRequestId);

            //test that there are the same number of elements in both lists
            Assert.That(response.Count == activityList.Count);

            //test that the elements are the same
            foreach (OrderActivityView activity in activityList)
            {
                Assert.That(response.Exists(q => q.OrderActivityId == activity.OrderActivityId));
            }


        }

        //"/api/order/activity/add"
        [Test]
        public void Test_Services_OrderService_AddOrderActivity()
        {

            OrderSearchResp order = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { });

            //get last orderactivity
            UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            Data.DBSchema.OrderActivityView oaV = oOp.getLastOrderActivity(order.OrderRequestId);

            OrderActivity oa = oOp.getOrderActivityById(oaV.OrderActivityId);

            if (!oOp.getOrderLock(oa.OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(oa.OrderRequestId);
            }

            Services.OrderService.NewActivityRequest request = new Services.OrderService.NewActivityRequest
            {
                OrderRequestId = oa.OrderRequestId,
                PrevOrderStatusId = oa.CurrOrderStatusId,
                CurrOrderStatusId = oa.CurrOrderStatusId,
                OrderStatusReasonId = oa.OrderStatusReasonId,
                PrevOrderSubStatusId = oa.CurrOrderSubStatusId,
                CurrOrderSubStatusId = oa.CurrOrderSubStatusId,
                ActivityNote = "A test activity!!!!!",
                EmploymentStatusId = oa.EmploymentStatusId ?? 0,
                EmploymentStatusReasonId = isZero(oa.EmploymentStatusReasonId, null),
                EmploymentOutlookId = oa.EmploymentOutlookId ?? 0,
                EmploymentStartDate = oa.EmploymentStartDate,
                EmploymentEndDate = oa.EmploymentEndDate,
                EmploymentJobTitle = oa.EmploymentJobTitle,
                EmployerFax = oa.EmployerFax,
                EmployerEmail = oa.EmployerEmail,
                EmployerName = oa.EmployerName,
                EmployerPhone = oa.EmployerPhone,
                VerifiedBy = oa.VerifiedBy,
                VerifiedByLanguageId = oa.VerifiedByLanguageId,
                VerifiedVia = oa.VerifiedVia,
                VendorId = oa.VendorId,
                VendorCost = oa.VendorCost,
                FollowupDate = oa.FollowupDate,
                VerifiedByTitle = oa.VerifiedByTitle,
                VerifiedByPhone = oa.VerifiedByPhone,
                PanicMode = oa.PanicMode,
                StickyNotes = oa.StickyNotes,
                OrderFollowupTypeId = oa.OrderFollowupTypeId,
                VendorDataDate = oa.VendorDataDate,
                SelfEmplDataDate = oa.SelfEmplDataDate,
                IsRevision = oa.IsRevision,
                VendorReferenceNum = oa.VendorReferenceNum,
                VendorRemovalReasonId = oa.VendorRemovalReasonId ?? 0,
                EncSchedClosingTime = oa.EncSchedClosingTime,
                EncSchedClosingTimeAMPM = oa.EncSchedClosingTimeAMPM,
                IsAuditing = oa.IsAuditing,
                IsReApproval = false,
                IsVendorEvent = false
                
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            NewActivityResp response = oService.Post(request);

            //test that response is correct
            Assert.That(response.message.ToLower().Contains("order activity saved"));

          

        }

        //"/api/order/activity/export"
        [Test]
        public void Test_Services_OrderService_ExportOrderActivity()
        {
            //download order activity
            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            OrderActivityExportResp response = oService.Post(new Services.OrderService.OrderActivityExportRequest
            {
                OrderRequestId = OrderRequestId
            });

            //test that there is a URL taht has something on it
            Assert.That(response.URL.Length > baseUrl.Length);

            //test that it is a PDF
            Assert.That(response.URL.ToLower().EndsWith("pdf"));

            logger.Info("URL: " + response.URL);

            string filePathName = convertFileURLtoFilePathName(response.URL);

            logger.Info("FilePath: " + filePathName);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);
        }

       
        //"/api/order/linkedorder/list"
        [Test]
        public void Test_Services_OrderService_ListLinkedOrders()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            Data.DTO.OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.LinkedOrderRequest request = new Services.OrderService.LinkedOrderRequest
            {  
                BorrowerFullName = order.BorrowerFullName,
                BorrowerSSN = order.BorrowerSSN,
                LoanNumber = order.LoanNumber
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<LoanInfoLinkedOrder> response = oService.Post(request);

            //now get list directly from DB
            List<Data.DTO.LoanInfoLinkedOrder> list = oOp.getLinkedOrders(order.LoanNumber, order.BorrowerFullName, order.BorrowerSSN);

            //check that item count is the same
            Assert.That(response.Count == list.Count);

            //check each element in the list
            foreach (LoanInfoLinkedOrder li in response)
            {
                Assert.That(list.Where<LoanInfoLinkedOrder>(q => q.OrderRequestId == li.OrderRequestId).ToList().Count == 1);
            }
            

        }

        //"/api/order/relatedorder/list"
        [Test]
        public void Test_Services_OrderService_ListRelatedOrdersLoanNumber()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            Data.DTO.OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.RelatedOrderRequest request = new Services.OrderService.RelatedOrderRequest
            {
                OrderRequestId = OrderRequestId,
                RelatedOrderRequestType = RelatedOrderReqType.LoanNumber
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<RelatedOrderResp> response = oService.Post(request);

            //now get list directly from DB
            List<Data.DTO.RelatedOrderResp> list = oOp.getRelatedOrders(OrderRequestId, RelatedOrderReqType.LoanNumber);

            //check that item count is the same
            Assert.That(response.Count == list.Count);

            //check each element in the list
            foreach (RelatedOrderResp li in response)
            {
                Assert.That(list.Where<RelatedOrderResp>(q => q.OrderRequestId == li.OrderRequestId).ToList().Count == 1);
            }


        }

        //"/api/order/relatedorder/list"
        [Test]
        public void Test_Services_OrderService_ListRelatedOrdersSSN()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            Data.DTO.OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);

            Services.OrderService.RelatedOrderRequest request = new Services.OrderService.RelatedOrderRequest
            {
                OrderRequestId = OrderRequestId,
                RelatedOrderRequestType = RelatedOrderReqType.SSN
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            List<RelatedOrderResp> response = oService.Post(request);

            //now get list directly from DB
            List<Data.DTO.RelatedOrderResp> list = oOp.getRelatedOrders(OrderRequestId, RelatedOrderReqType.SSN);

            //check that item count is the same
            Assert.That(response.Count == list.Count);

            //check each element in the list
            foreach (RelatedOrderResp li in response)
            {
                Assert.That(list.Where<RelatedOrderResp>(q => q.OrderRequestId == li.OrderRequestId).ToList().Count == 1);
            }


        }

        //"/api/order/subcontract/add"
        [Test]
        public void Test_Services_OrderService_SubcontractOrderEquifaxAdd()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
               new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //get lock on order
            Business.OrderOps oOp = new Business.OrderOps();
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }


            Services.OrderService.SubcontractOrderRequest request = new Services.OrderService.SubcontractOrderRequest
            {
                OrderRequestId = OrderRequestId,
                SubcontractType = "equifax"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            SubcontractOrderResp response = oService.Post(request);

            //check that there were no errors
            Assert.That(!response.Result.ToLower().Contains("error"));

            //check that the order number is fillled in
            Assert.That(response.EquifaxOrderNumber != null);


        }

        //"/api/order/subcontract/add"
        [Test]
        public void Test_Services_OrderService_SubcontractOrderAdvancedDataAdd()
        {

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
               new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //get lock on order
            Business.OrderOps oOp = new Business.OrderOps();
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }


            Services.OrderService.SubcontractOrderRequest request = new Services.OrderService.SubcontractOrderRequest
            {
                OrderRequestId = OrderRequestId,
                SubcontractType = "ad"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            SubcontractOrderResp response = oService.Post(request);

            //check that there were no errors
            Assert.That(!response.Result.ToLower().Contains("error"));

            //check that the order number is fillled in
            Assert.That(response.ADOrderNumber != null);


        }

        //"/api/order/subcontract/cancel"
        [Test]
        public void Test_Services_OrderService_SubcontractOrderCancelNoReassign()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            bool IsSubcontracted = false;
            int OrderRequestId = 0;
            string orderUserName = null;
            OrderDetailResp orderDetail;

            List<OrderSearchResp> orders = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).ToList();

            foreach (OrderSearchResp ord in orders) {

                OrderRequestId = ord.OrderRequestId;
                orderDetail = oOp.getOrderDetail(OrderRequestId);
                IsSubcontracted = orderDetail.IsSubcontracted;
                orderUserName = orderDetail.VerificationSpecialist;

                if (IsSubcontracted) { break; };
            }

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }


            Services.OrderService.SubcontractOrderCancellationRequest request = new Services.OrderService.SubcontractOrderCancellationRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            SubcontractOrderCancellationResp response = oService.Post(request);

            //check that there were no errors
            Assert.That(!response.Result.ToLower().Contains("error"));

            //check that the order is no longer subciontracted
            orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.IsSubcontracted == false);

            //check that the user was not reassigned
            Assert.That(orderUserName == orderDetail.VerificationSpecialist);


        }

        //"/api/order/subcontract/cancel"
        [Test]
        public void Test_Services_OrderService_SubcontractOrderCancelWithReassign()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            bool IsSubcontracted = false;
            int OrderRequestId = 0;
            string orderUserName = null;
            OrderDetailResp orderDetail;

            List<OrderSearchResp> orders = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).ToList();

            foreach (OrderSearchResp ord in orders)
            {

                OrderRequestId = ord.OrderRequestId;
                orderDetail = oOp.getOrderDetail(OrderRequestId);
                IsSubcontracted = orderDetail.IsSubcontracted;
                orderUserName = orderDetail.VerificationSpecialist;

                if (IsSubcontracted) { break; };
            }

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            string newUserName = "aleitk152";
            if (newUserName == orderUserName) { newUserName = "cspori111"; };

            Services.OrderService.SubcontractOrderCancellationRequest request = new Services.OrderService.SubcontractOrderCancellationRequest
            {
                OrderRequestId = OrderRequestId,
                ReassignUserName = newUserName
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            SubcontractOrderCancellationResp response = oService.Post(request);

            //check that there were no errors
            Assert.That(!response.Result.ToLower().Contains("error"));

            //check that the order is no longer subcontracted
            orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.IsSubcontracted == false);

            //check that the user was reassigned
            Assert.That(newUserName == orderDetail.VerificationSpecialist);


        }

        //"/api/order/panicmode/update"
        [Test]
        public void Test_Services_OrderService_PanicModeUpdate()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = 0;

            List<OrderSearchResp> orders = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).ToList();

            foreach (OrderSearchResp ord in orders)
            {
                if (!ord.IsPanicMode) {
                    OrderRequestId = ord.OrderRequestId;
                    break; 
                };
            }

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            //turn on panic mode
            Services.OrderService.OrderUpdatePanicModeRequest request = new Services.OrderService.OrderUpdatePanicModeRequest
            {
                OrderRequestId = OrderRequestId,
                PanicMode = true
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            bool response = oService.Post(request);

            //check that the order is now in panic mode
            OrderActivityView orderActivity = oOp.getLastOrderActivity(OrderRequestId);
            Assert.That(orderActivity.PanicMode == true);

            //turn off panic mode
            request = new Services.OrderService.OrderUpdatePanicModeRequest
            {
                OrderRequestId = OrderRequestId,
                PanicMode = false
            };

            response = oService.Post(request);

            //check that the order is no longer in panic mode
            orderActivity = oOp.getLastOrderActivity(OrderRequestId);
            Assert.That(orderActivity.PanicMode == false);

        }

        //"/api/order/rushrequest/update"
        [Test]
        public void Test_Services_OrderService_RushRequestApprove()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = oOp.getOrderLastRushRequest();

            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);
            string origUserName = orderDetail.VerificationSpecialist;
            
            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            Services.OrderService.RushRequestDispositionRequest request = new Services.OrderService.RushRequestDispositionRequest
            {
                OrderRequestId = OrderRequestId,
                Approved = true
            };

            RushRequestDispositionResp response = oService.Post(request);

            //check that the order is now a rush
            orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.RushRequestStatus == "Approved");

            //check that voes is the same
            Assert.That(orderDetail.VerificationSpecialist == origUserName);
        }

        //"/api/order/rushrequest/update"
        [Test]
        public void Test_Services_OrderService_RushRequestApproveWithReassign()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = oOp.getOrderLastRushRequest();

            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);
            string origUserName = orderDetail.VerificationSpecialist;
            string newUserName = "aleitk152";
            if (newUserName == origUserName) { newUserName = "cspori111"; };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            Services.OrderService.RushRequestDispositionRequest request = new Services.OrderService.RushRequestDispositionRequest
            {
                OrderRequestId = OrderRequestId,
                Approved = true,
                AssignedVOES = newUserName
            };

            RushRequestDispositionResp response = oService.Post(request);

            //check that the order is now a rush
            orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.RushRequestStatus == "Approved");

            //check that voes has been reassigned
            Assert.That(orderDetail.VerificationSpecialist == newUserName);

        }

        //"/api/order/rushrequest/update"
        [Test]
        public void Test_Services_OrderService_RushRequestDeny()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = oOp.getOrderLastRushRequest();

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            Services.OrderService.RushRequestDispositionRequest request = new Services.OrderService.RushRequestDispositionRequest
            {
                OrderRequestId = OrderRequestId,
                Approved = false,
                DenialNote = "Request has been DENIED!!!"
            };

            RushRequestDispositionResp response = oService.Post(request);

            //check that the order is now not a rush
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.RushRequestStatus == "Denied");

        }

        //"/api/order/cancellationrequest/update"
        [Test]
        public void Test_Services_OrderService_CancellationRequestApprove()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

            //create a cancellation request
            OrderActivityView oAv = oOp.getLastOrderActivity(OrderRequestId);
            OrderActivity oa = oOp.getOrderActivityById(oAv.OrderActivityId);

            if (!oOp.getOrderLock(oa.OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(oa.OrderRequestId);
            }

            Services.OrderService.NewActivityRequest requestCancellation = new Services.OrderService.NewActivityRequest
            {
                OrderRequestId = oa.OrderRequestId,
                PrevOrderStatusId = oa.CurrOrderStatusId,
                CurrOrderStatusId = 6,  //cancellation
                OrderStatusReasonId = 3,  //salary correction reason
                PrevOrderSubStatusId = oa.CurrOrderSubStatusId,
                CurrOrderSubStatusId = null,
                ActivityNote = "Trying to create cancellation request",
                EmploymentStatusId = oa.EmploymentStatusId ?? 0,
                EmploymentStatusReasonId = isZero(oa.EmploymentStatusReasonId, null),
                EmploymentOutlookId = oa.EmploymentOutlookId ?? 0,
                EmploymentStartDate = oa.EmploymentStartDate,
                EmploymentEndDate = oa.EmploymentEndDate,
                EmploymentJobTitle = oa.EmploymentJobTitle,
                EmployerFax = oa.EmployerFax,
                EmployerEmail = oa.EmployerEmail,
                EmployerName = oa.EmployerName,
                EmployerPhone = oa.EmployerPhone,
                VerifiedBy = oa.VerifiedBy,
                VerifiedByLanguageId = oa.VerifiedByLanguageId,
                VerifiedVia = oa.VerifiedVia,
                VendorId = oa.VendorId,
                VendorCost = oa.VendorCost,
                FollowupDate = oa.FollowupDate,
                VerifiedByTitle = oa.VerifiedByTitle,
                VerifiedByPhone = oa.VerifiedByPhone,
                PanicMode = oa.PanicMode,
                StickyNotes = oa.StickyNotes,
                OrderFollowupTypeId = oa.OrderFollowupTypeId,
                VendorDataDate = oa.VendorDataDate,
                SelfEmplDataDate = oa.SelfEmplDataDate,
                IsRevision = oa.IsRevision,
                VendorReferenceNum = oa.VendorReferenceNum,
                VendorRemovalReasonId = oa.VendorRemovalReasonId ?? 0,
                EncSchedClosingTime = oa.EncSchedClosingTime,
                EncSchedClosingTimeAMPM = oa.EncSchedClosingTimeAMPM,
                IsAuditing = oa.IsAuditing,
                IsReApproval = false,
                IsVendorEvent = false
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            NewActivityResp oaResp = oService.Post(requestCancellation);

            Assert.That(oaResp.message.ToLower().Contains("order activity saved"));

            //approve cancellation request
            Services.OrderService.CancellationRequestDispositionRequest request = new Services.OrderService.CancellationRequestDispositionRequest
            {
                OrderRequestId = OrderRequestId,
                Approved = true
            };

            CancellationRequestDispositionResp response = oService.Post(request);
            
            //check that the order is now cancelled
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.VerificationStatus == "Cancelled");

        }

        //"/api/order/cancellationrequest/update"
        [Test]
        public void Test_Services_OrderService_CancellationRequestDeny()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

            //create a cancellation request
            OrderActivityView oAv = oOp.getLastOrderActivity(OrderRequestId);
            OrderActivity oa = oOp.getOrderActivityById(oAv.OrderActivityId);

            if (!oOp.getOrderLock(oa.OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(oa.OrderRequestId);
            }

            Services.OrderService.NewActivityRequest requestCancellation = new Services.OrderService.NewActivityRequest
            {
                OrderRequestId = oa.OrderRequestId,
                PrevOrderStatusId = oa.CurrOrderStatusId,
                CurrOrderStatusId = 6,  //cancellation
                OrderStatusReasonId = 3,  //salary correction reason
                PrevOrderSubStatusId = oa.CurrOrderSubStatusId,
                CurrOrderSubStatusId = null,
                ActivityNote = "Trying to create cancellation request",
                EmploymentStatusId = oa.EmploymentStatusId ?? 0,
                EmploymentStatusReasonId = isZero(oa.EmploymentStatusReasonId, null),
                EmploymentOutlookId = oa.EmploymentOutlookId ?? 0,
                EmploymentStartDate = oa.EmploymentStartDate,
                EmploymentEndDate = oa.EmploymentEndDate,
                EmploymentJobTitle = oa.EmploymentJobTitle,
                EmployerFax = oa.EmployerFax,
                EmployerEmail = oa.EmployerEmail,
                EmployerName = oa.EmployerName,
                EmployerPhone = oa.EmployerPhone,
                VerifiedBy = oa.VerifiedBy,
                VerifiedByLanguageId = oa.VerifiedByLanguageId,
                VerifiedVia = oa.VerifiedVia,
                VendorId = oa.VendorId,
                VendorCost = oa.VendorCost,
                FollowupDate = oa.FollowupDate,
                VerifiedByTitle = oa.VerifiedByTitle,
                VerifiedByPhone = oa.VerifiedByPhone,
                PanicMode = oa.PanicMode,
                StickyNotes = oa.StickyNotes,
                OrderFollowupTypeId = oa.OrderFollowupTypeId,
                VendorDataDate = oa.VendorDataDate,
                SelfEmplDataDate = oa.SelfEmplDataDate,
                IsRevision = oa.IsRevision,
                VendorReferenceNum = oa.VendorReferenceNum,
                VendorRemovalReasonId = oa.VendorRemovalReasonId ?? 0,
                EncSchedClosingTime = oa.EncSchedClosingTime,
                EncSchedClosingTimeAMPM = oa.EncSchedClosingTimeAMPM,
                IsAuditing = oa.IsAuditing,
                IsReApproval = false,
                IsVendorEvent = false
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            NewActivityResp oaResp = oService.Post(requestCancellation);

            Assert.That(oaResp.message.ToLower().Contains("order activity saved"));

            //approve cancellation request
            Services.OrderService.CancellationRequestDispositionRequest request = new Services.OrderService.CancellationRequestDispositionRequest
            {
                OrderRequestId = OrderRequestId,
                Approved = false,
                DenialNote = "This cancellation request has been DENIED!!!!!!"
            };

            CancellationRequestDispositionResp response = oService.Post(request);

            //check that the order is still pending
            OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);
            Assert.That(orderDetail.VerificationStatus == "Pending");

        }

        //"/api/order/field/update"
        [Test]
        public void Test_Services_OrderService_SaveFieldEdit()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { })
                    .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderRequestId;

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            string fieldName = "CPAName";
            string fieldValue = "CPAName Updated Value";

            Services.OrderService.SaveFieldEditRequest request = new Services.OrderService.SaveFieldEditRequest
            {
                OrderRequestId = OrderRequestId,
                FieldName = fieldName,
                NewValue = fieldValue           
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            SaveFieldEditResp response = oService.Post(request);

            //check that the response field is correct
            Assert.That(response.ResultFieldName == fieldName);

            //check that the new value is correct
            OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);
            Assert.That(typeof(OrderDetailResp).GetProperty(fieldName).GetValue(order).ToString() == fieldValue);
            

        }

        //"/api/order/bulkaction"
        [Test]
        public void Test_Services_OrderService_BulkAction()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int orderCount = getOrdersByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).Count();

            //OrderProcessBulkActionsRequest
            /*  public string LoanNumber { get; set; }
                public List<OrderStatus> OrderStatuses { get; set; }
                public List<RequestType> RequestTypes { get; set; }
                public string BorrowerSSN { get; set; }
                public string ActivityNote { get; set; }
                public int NewStatusId { get; set; }
                public string NewVOEAssignmentId { get; set; }
                public DateTime? OrderDate { get; set; }
                public int? LimitCount { get; set; }
                public DateTime? SchedClosingDate { get; set; }*/


            Services.OrderService.OrderProcessBulkActionsRequest request = new Services.OrderService.OrderProcessBulkActionsRequest
            {
                OrderStatuses = new List<OrderProcessBulkActionsReq.OrderStatus>()
                {
                    new OrderProcessBulkActionsReq.OrderStatus { Id = 1, Value = true}
                },
                RequestTypes = new List<OrderProcessBulkActionsReq.RequestType>()
                {
                    new OrderProcessBulkActionsReq.RequestType { Id = 1, Value = true}
                },
                OrderTypes = new List<OrderProcessBulkActionsReq.OrderType>() { },
                ActivityNote = "Updated Note!!",
                LoanNumber = "",
                NewStatusId = 0,
                NewVOEAssignmentId = "0"
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderProcessBulkActionsResp response = oService.Post(request);
            Assert.That(response.Result.ToLower().Contains("record(s) updated"));

            //check order count
            Assert.That(response.Result.StartsWith(orderCount.ToString()));


        }

        //"/api/order/certexpirenotification/dismiss"
        [Test]
        public void Test_Services_OrderService_CertExpireDismiss()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Final" }, new List<string> { }, new List<string> { }).OrderRequestId;

            Services.OrderService.CertExpirationNoticeDismissRequest request = new Services.OrderService.CertExpirationNoticeDismissRequest
            {
                OrderRequestId = OrderRequestId                
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            CertExpirationNoticeDismissResp response = oService.Post(request);
            Assert.That(response.Result.ToLower() == "certification expiration dismissed");

            //make sure cert expire is dismissed
            OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);
            Assert.That(order.IsCertExpireDismissed == true);

        }

        //"/api/order/audit/dismiss"
        [Test]
        public void Test_Services_OrderService_AuditDismiss()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Final" }, new List<string> { }, new List<string> { }).OrderRequestId;

            Services.OrderService.AuditDismissRequest request = new Services.OrderService.AuditDismissRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            AuditDismissResp response = oService.Post(request);

            //check response
            Assert.That(response.Result.ToLower() == "audit approved");

            //check audit flag
            OrderDetailResp order = oOp.getOrderDetail(OrderRequestId);
            Assert.That(order.IsAuditDismissed == true);

            //check cert file
            List<OrderActivityView> activity = oOp.getOrderActivity(OrderRequestId);

            //first record should be the audit approval, second is the cert link
            int iCounter = 1;
            string certFilePath = String.Empty;
            foreach (OrderActivityView oa in activity.OrderByDescending(q => q.OrderActivityId))
            {
                if (iCounter == 1)
                {
                    Assert.That(oa.ActivityNote.ToLower().Contains("audit approved"));
                    iCounter += 1;
                }
                else if (iCounter == 2)
                {
                    //should be cert link
                    Assert.That(oa.CertificationFilePath != null);
                    certFilePath = oa.CertificationFilePath;
                    break;
                } 
            }

            //check cert file path
            string certPath = RepositoryPath + certFilePath;

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(certPath));
            System.IO.FileInfo fi = new System.IO.FileInfo(certPath);
            Assert.That(fi.Length > 0);

        }


        //"/api/order/lock/update"
        [Test]
        public void Test_Services_OrderService_UpdateOrderLock()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            Services.OrderService.OrderLockUpdateRequest request = new Services.OrderService.OrderLockUpdateRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderLockUpdateResp response = oService.Post(request);
            Assert.That(response.message == "1");

        }

        //"/api/order/lock/delete"
        [Test]
        public void Test_Services_OrderService_DeleteOrderLock()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            Services.OrderService.OrderLockDeleteRequest request = new Services.OrderService.OrderLockDeleteRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderLockDeleteResp response = oService.Post(request);
            Assert.That(response.message == "1");

        }

        //"/api/order/lock/detail"
        [Test]
        public void Test_Services_OrderService_CheckOrderLockTrue()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //get lock on order
            if (!oOp.getOrderLock(OrderRequestId, "cbicknell"))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotObtainOrderLockException(OrderRequestId);
            }

            Services.OrderService.OrderLockCheckRequest request = new Services.OrderService.OrderLockCheckRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderLockCheckResp response = oService.Post(request);
            Assert.That(response.result == 1);

        }


        //"/api/order/lock/detail"
        [Test]
        public void Test_Services_OrderService_CheckOrderLockFalse()
        {

            UnitTests.Business.OrderOps oOp = new Business.OrderOps();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //delete lock on order
            if (!oOp.deleteOrderLock(OrderRequestId))
            {
                throw new VOESystem.UnitTests.Tests.ServiceTests.OrderService.OrderServiceExceptions.CouldNotDeleteOrderLockException(OrderRequestId);
            }

            Services.OrderService.OrderLockCheckRequest request = new Services.OrderService.OrderLockCheckRequest
            {
                OrderRequestId = OrderRequestId
            };

            Services.OrderService oService = GetServiceInstance<Services.OrderService>();

            OrderLockCheckResp response = oService.Post(request);
            Assert.That(response.result == 0);

        }

         public class OrderServiceException : ServiceTestException { }

         public static class OrderServiceExceptions
         {

             public class CouldNotObtainOrderLockException : OrderServiceException
             {

                 public int? OrderRequestId = null;

                 public override string Message
                 {
                     get
                     {
                         string msg = "Could not obtain order lock on selected order ";
                         if (OrderRequestId != null)
                         {
                             msg += OrderRequestId.ToString();
                         }
                         return msg;
                     }
                 }

                 public CouldNotObtainOrderLockException(int? orderRequestId = null)
                     : base() 
                {

                    OrderRequestId = orderRequestId;

                }

             }

             public class CouldNotDeleteOrderLockException : OrderServiceException
             {

                 public int? OrderRequestId = null;

                 public override string Message
                 {
                     get
                     {
                         string msg = "Could not delete order lock on selected order ";
                         if (OrderRequestId != null)
                         {
                             msg += OrderRequestId.ToString();
                         }
                         return msg;
                     }
                 }

                 public CouldNotDeleteOrderLockException(int? orderRequestId = null)
                     : base()
                 {

                     OrderRequestId = orderRequestId;

                 }

             }

         }


    }
}
