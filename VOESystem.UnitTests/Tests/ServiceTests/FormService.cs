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
    public class FormService : ServiceTestBase
    {

        //"/api/form/download"
        [Test]
        public void Test_Services_FormService_DownloadForm()
        {
            //download plain form, no extra data
            Services.FormService oService = GetServiceInstance<Services.FormService>();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            FormResp response = oService.Any(new Services.FormService.FormRequest
            {
                OrderRequestId = OrderRequestId,
                FormTag = "VerbalVOERequest"
            });

            //test that there is a URL taht has something on it
            Assert.That(response.URL.Length > baseUrl.Length );

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

        //"/api/form/download"
        [Test]
        public void Test_Services_FormService_DownloadFormWithFormNotes()
        {
            //download plain form, no extra data
            Services.FormService oService = GetServiceInstance<Services.FormService>();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            string formDataString = "This is a test form notes data string.";

            FormResp response = oService.Any(new Services.FormService.FormRequest
            {
                OrderRequestId = OrderRequestId,
                FormTag = "CurrentFaxCover",
                FormData = formDataString,
                FormDataType = "FormNotes"
            });

            //test that there is a URL taht has something on it
            Assert.That(response.URL.Length > baseUrl.Length);

            //test that it is a PDF
            Assert.That(response.URL.ToLower().EndsWith("pdf"));

            string filePathName = convertFileURLtoFilePathName(response.URL);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);

            //open PDF and ensure that it contains the test string
            PDFOps pOp = new PDFOps();
            string fileContents = pOp.extractAllText(filePathName);
            Assert.That(fileContents.Contains(formDataString));


        }

        //"/api/form/download"
        [Test]
        public void Test_Services_FormService_DownloadFormWithEmployerData()
        {
            //download plain form, no extra data
            Services.FormService oService = GetServiceInstance<Services.FormService>();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            string empName = "Test Employer Name";
            string empAddress = "123 Happy Street, Richmond VA 23060";
            string empPhone = "804-555-1212";

            Services.FormService.FormRequest.EmployerData empData = new FormReq.EmployerData
            {
                EmployerAddress = empAddress,
                EmployerName = empName,
                EmployerPhone = empPhone
            };

            FormResp response = oService.Any(new Services.FormService.FormRequest
            {
                OrderRequestId = OrderRequestId,
                FormTag = "ELMAVOERequest",
                FormData = JsonSerializer.SerializeToString<FormReq.EmployerData>(empData),
                FormDataType = "EmployerData"
            });

            //test that there is a URL taht has something on it
            Assert.That(response.URL.Length > baseUrl.Length);

            //test that it is a PDF
            Assert.That(response.URL.ToLower().EndsWith("pdf"));

            string filePathName = convertFileURLtoFilePathName(response.URL);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);

            //open PDF and ensure that it contains the test string
            PDFOps pOp = new PDFOps();
            string fileContents = pOp.extractAllText(filePathName);
            Assert.That(fileContents.Contains(empName));
            Assert.That(fileContents.Contains(empAddress));
            Assert.That(fileContents.Contains(empPhone));


        }


        //"/api/form/list"
        [Test]
        public void Test_Services_FormService_ListAttachableForms()
        {
            Services.FormService oService = GetServiceInstance<Services.FormService>();

            FormListResp response = oService.Any(new VOESystem.Services.FormService.FormListRequest() { });

            //test that there are entries
            Assert.That(response.FormList.Count > 0);

        }

    }
}
