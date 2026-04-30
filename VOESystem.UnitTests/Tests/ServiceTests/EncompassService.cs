using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class EncompassService : ServiceTestBase
    {

        //"/api/enc/loan/detail"
        [Test]
        public void Test_Services_EncompassService_GetLoanInfo()
        {

            Services.EncompassService oService = GetServiceInstance<Services.EncompassService>(false, null, null, @"http://localhost/voesystem");
            
            string LoanNumber = getRandomLoanNumberByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90);

            //fill in loan info dependency
            oService.li = new VOEBackend.Encompass.Loans();

            List<LoanInfoResp> response = oService.Any(new Services.EncompassService.LoanInfoRequest
            {
                LoanNumber = LoanNumber
            });

            //test that there are loaninfos
            Assert.That(response.Count > 0);

            //make sure the required fields are filled out
            foreach (LoanInfoResp loaninfo in response)
            {
                Assert.IsNotNull(loaninfo.BorrowerAddress);
                Assert.IsNotNull(loaninfo.BorrowerDOB);
                Assert.IsNotNull(loaninfo.BorrowerFirstName);
                Assert.IsNotNull(loaninfo.BorrowerLastName);
                Assert.IsNotNull(loaninfo.BorrowerGender);
                Assert.IsNotNull(loaninfo.BorrowerSSN);
                Assert.IsNotNull(loaninfo.EncCurrentLoanFolder);
                Assert.IsNotNull(loaninfo.EncEmployerAddress);
                Assert.IsNotNull(loaninfo.EncEmployerName);
                Assert.IsNotNull(loaninfo.EncEmployerPhone);
                Assert.IsNotNull(loaninfo.EncEmploymentSelfFlag);
                Assert.IsNotNull(loaninfo.EncEmploymentStatus);
                Assert.IsNotNull(loaninfo.EncEmploymentTitle);
                Assert.IsNotNull(loaninfo.EncCurrentLoanFolder);
                Assert.IsNotNull(loaninfo.EncLoanOfficerName);
                Assert.IsNotNull(loaninfo.EncLoanType);
                Assert.IsNotNull(loaninfo.EncProcessorName);
                Assert.IsNotNull(loaninfo.LoanNumber);
                Assert.IsNotNull(loaninfo.OrgId);
            }


        }

        
        //"/api/enc/loan/attachment/list"
        [Test]
        public void Test_Services_EncompassService_GetLoanAttachmentList()
        {

            Services.EncompassService oService = GetServiceInstance<Services.EncompassService>(false, null, null, @"http://localhost/voesystem");

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            LoanAttachmentListResp response = oService.Any(new Services.EncompassService.LoanAttachmentListRequest
            {
                OrderRequestId = OrderRequestId
            });

            //test that there are attachments
            Assert.That(response.AttachmentList.Count > 0);

        }
        
        //"/api/enc/loan/attachment/list/update/"
        [Test]
        public void Test_Services_EncompassService_UpdateLoanAttachmentList()
        {

            Services.EncompassService oService = GetServiceInstance<Services.EncompassService>(false, null, null, @"http://localhost/voesystem");

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            LoanAttachmentListUpdateResp response = oService.Any(new Services.EncompassService.LoanAttachmentListUpdateRequest
            {
                OrderRequestId = OrderRequestId
            });

            //test that there are attachments
            Assert.That(response.DocumentList.Count > 0);

            //test that there are no errors
            Assert.That(!isNull(response.Message,"").ToLower().Contains("error"));

           
        }


        //"/attachments/download/{OrderRequestId}/{UniqueFileName}"
        [Test]
        public void Test_Services_EncompassService_DownloadLoanAttachment()
        {

            Services.EncompassService oService = GetServiceInstance<Services.EncompassService>(false, null, null, @"http://localhost/voesystem");

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new UnitTests.Business.OrderOps();
            List<DocumentOrderView> docs = oOp.getOrderDocuments(OrderRequestId);

            string uniqFileName = docs.Where<DocumentOrderView>(q => q.UniqueFileName != null && q.DocumentTypeName == "EncompassCloud")
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().UniqueFileName;

            LoanAttachmentDownloadResp response = oService.Any(new Services.EncompassService.LoanAttachmentDownloadRequest
            {
                OrderRequestId = OrderRequestId,
                UniqueFileName = uniqFileName
            });

            string attURL = response.URL;

            //test that is not null/empty
            Assert.That(isNull(attURL,"") != "");

            //test that file exists and has on zero filesize
            string attFilePath = convertFileURLtoFilePathName(attURL);
            Assert.That(File.Exists(attFilePath));
            System.IO.FileInfo fi = new System.IO.FileInfo(attFilePath);
            Assert.That(fi.Length > 0);


        }


        //"/attachments/view/{OrderRequestId}/{UniqueFileName}"
        [Test]
        public void Test_Services_EncompassService_ViewLoanAttachment()
        {

            Services.EncompassService oService = GetServiceInstance<Services.EncompassService>(false, null, null, @"http://localhost/voesystem");

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            UnitTests.Business.OrderOps oOp = new UnitTests.Business.OrderOps();
            List<DocumentOrderView> docs = oOp.getOrderDocuments(OrderRequestId);

            string uniqFileName = docs.Where<DocumentOrderView>(q => q.UniqueFileName != null && q.DocumentTypeName == "EncompassCloud")
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().UniqueFileName;

            ServiceStack.Common.Web.HttpResult response = oService.Any(new Services.EncompassService.LoanAttachmentViewRequest
            {
                OrderRequestId = OrderRequestId,
                UniqueFileName = uniqFileName
            });

            //test for OK status
            string statusCode = response.Status.ToString();
            Assert.That(statusCode == "200");

            //test for content type
            Assert.That(response.ContentType == "text/html");

            //test for base 64 encoded string
            Regex regEx = new Regex(@"data\:image\/[a-zA-Z]{3}\;base64");
            Assert.That(regEx.IsMatch(response.ResponseText));

            //test for string length
            Assert.That(response.ResponseText.Length > 900);




        }


    }
}
