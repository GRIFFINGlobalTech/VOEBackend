using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    [TestFixture]
    public class DocumentService : ServiceTestBase
    {

        //"/api/order/document/list"
        [Test]
        public void Test_Services_DocumentService_ListDocumentsForOrder()
        {
            Services.DocumentService oService = GetServiceInstance<Services.DocumentService>();

            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

            LogOrderNumber(OrderRequestId);

            DocumentListResp response = oService.Any(new Services.DocumentService.DocumentListRequest
            {
                OrderRequestId = OrderRequestId
            });

            //test that there are documents there...not sure what else to do here
            Assert.That(response.DocumentList.Count > 0);
           

        }

        //"/api/document/delete"
        [Test]
        public void Test_Services_DocumentService_DeleteDocument()
        {

            Services.DocumentService oService = GetServiceInstance<Services.DocumentService>();
            var userAuthId = oService.GetSession().UserAuthId;

            VOESystem.UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            List<Data.DBSchema.DocumentOrderView> documents = new List<Data.DBSchema.DocumentOrderView>() { };
            int OrderRequestId = oOp.getOrderWithDeletableDocuments(out documents, UserName);
            int DocumentId = documents[0].DocumentId;

            DocumentDeleteResp response = oService.Any(new Services.DocumentService.DocumentDeleteRequest
            {   
                OrderRequestId = OrderRequestId,
                DocumentId = DocumentId
            });

            //test that response message contains success
            Assert.That(response.DeleteStatus.Contains("Success"));

            //test that document is marked as deleted
            Document verifDoc = getDocumentById(DocumentId);
            Assert.That(verifDoc.Deleted);

        }

        //"/api/document/upload"
        [Test]
        public void Test_Services_DocumentService_UploadDocument()
        {

            Services.DocumentService oService = GetServiceInstance<Services.DocumentService>(true);

            int orderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;


            DocumentUploadFileResp response = oService.Any(new Services.DocumentService.DocumentUploadFileRequest
                {
                    OrderRequestId = orderRequestId.ToString(), 
                    UploadToEncompass = false
                });

            Assert.That(response.UploadResults.Count == 1);

            UploadResult res = response.UploadResults[0];

            //check that response to upload is good
            Assert.That(res.Result == true);
            Assert.That(res.DocumentId > 0);
            Assert.That(res.ErrorMessage == null);
            Assert.That(res.FileName!= null);

            //check that new doc exists
            Document verifDoc = getDocumentById(res.DocumentId);
            Assert.NotNull(verifDoc);

        }

        //"/api/order/document/encqueue/toggle"
        [Test]
        public void Test_Services_DocumentService_ToggleUploadRequest()
        {

            Services.DocumentService oService = GetServiceInstance<Services.DocumentService>(true);

            int orderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //upload a document
            DocumentUploadFileResp response = oService.Any(new Services.DocumentService.DocumentUploadFileRequest
                {
                    OrderRequestId = orderRequestId.ToString(),
                    UploadToEncompass = false
                });

            //check to make sure it is not queued for encompass upload
            UnitTests.Business.OrderOps oOp = new UnitTests.Business.OrderOps();
            List<DocumentUploadQueueView> queue = oOp.getOrderDocumentUploadQueue(orderRequestId);
            Assert.That(queue.Where(q => q.DocumentId == response.UploadResults[0].DocumentId).ToList().Count == 0);

            int DocumentId = response.UploadResults[0].DocumentId;

            VOESystem.Services.DocumentService.DocumentToggleUploadRequest toggleReq = new VOESystem.Services.DocumentService.DocumentToggleUploadRequest
            {
                DocumentId = DocumentId,
                OrderRequestId = orderRequestId
            };

            oService.Any(toggleReq);

            //check to make sure it is now queued for encompass upload
            queue = oOp.getOrderDocumentUploadQueue(orderRequestId);
            Assert.That(queue.Where(q => q.DocumentId == DocumentId).ToList().Count == 1);


        }

        //"/api/document/update"
        [Test]
        public void Test_Services_DocumentService_UpdateNameRequest()
        {

            Services.DocumentService oService = GetServiceInstance<Services.DocumentService>();

            int orderRequestId = 0;
            UnitTests.Business.OrderOps oOp = new UnitTests.Business.OrderOps();
            int tryCount = 0;
            List<DocumentOrderView> docs = null;

            while (orderRequestId == 0)
            {
                int ordId = getRandomOrderByCriteria(new List<string> { "Approved" },
                    new List<string> { "Initial" }, new List<string> { }, new List<string> { }, 90).OrderRequestId;

                docs = oOp.getOrderDocuments(ordId);
                tryCount++;

                if (docs.Count() > 0)
                {
                    orderRequestId = ordId;
                }
                if (tryCount > 5)
                {
                    throw new VOESystem.UnitTests.Tests.ServiceTests.ServiceTestBase.ServiceTestExceptions.NoValidOrderForTestException();
                }
            }

            DocumentOrderView doc = docs.Where<DocumentOrderView>(q => q.DocumentTypeName == "UserUploaded").FirstOrDefault();

            string newDocDispName = "TESTING " + doc.FileDisplayName;

            oService.Any(new VOESystem.Services.DocumentService.DocumentUpdateRequest
                {
                    DocumentId = doc.DocumentId,
                    NewFileDisplayName = newDocDispName,
                    UpdateFieldName = "NewFileDisplayName"
            });

            Document verifDoc = getDocumentById(doc.DocumentId);

            //check that new name was saved
            Assert.That(verifDoc.CustomFileDisplayName == newDocDispName);


        }

 
    }
}
