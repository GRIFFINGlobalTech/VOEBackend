using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOESystem.UnitTests.Business
{
    public class OrderOps : BusinessBase
    {

        public List<Data.DTO.Email> getOrderEmails(int OrderRequestId, string EmailTemplateName, DateTime? sinceDateTime = null)
        {

            List<Data.DTO.Email> retVal = new List<Data.DTO.Email>() { };

            if (EmailTemplateName == null)
            {
                //get all order emails
                Data.Business.EmailOps eops = new Data.Business.EmailOps();
                retVal = eops.getEmailHistory(Db, OrderRequestId, "", false).ToList();
            }
            else
            {
                //get template id
                int emailTemplateId = Db.Where<Data.DBSchema.EmailTemplate>(q => q.Name == EmailTemplateName).FirstOrDefault().Id;

                Data.Business.EmailOps eops = new Data.Business.EmailOps();
                retVal = eops.getEmailHistory(Db, OrderRequestId, "", false)
                    .Where<Data.DTO.Email>(q => q.EmailTemplateId == emailTemplateId).ToList();
            }

            //restrict by datetime
            if (sinceDateTime != null)
            {
                retVal = retVal.Where<Data.DTO.Email>(q => q.EmailDateTime >= sinceDateTime).ToList();
            }

            return retVal;

        }

        public List<Data.DTO.LoanInfoLinkedOrder> getLinkedOrders(string LoanNumber, string BorrowerFullName, string BorrowerSSN)
        {
            List<Data.DTO.LoanInfoLinkedOrder> retVal = new List<Data.DTO.LoanInfoLinkedOrder>();

            serviceAppHost.Container.RegisterAutoWired<VOESystem.Services.OrderService>();
            VOESystem.Services.OrderService oService = serviceAppHost.Container.Resolve<VOESystem.Services.OrderService>();

            retVal = oService.Post(new VOESystem.Services.OrderService.LinkedOrderRequest
            {
                LoanNumber = LoanNumber,
                BorrowerFullName = BorrowerFullName,
                BorrowerSSN = BorrowerSSN
            });

            return retVal;

        }

        public List<Data.DTO.RelatedOrderResp> getRelatedOrders(int OrderRequestId, RelatedOrderReqType reqType)
        {
            List<Data.DTO.RelatedOrderResp> retVal = new List<Data.DTO.RelatedOrderResp>();

            Data.Business.OrderOps oOp = new Data.Business.OrderOps();
            retVal = oOp.getRelatedOrders(Db, OrderRequestId, reqType);

            return retVal;

        }

        public Data.DBSchema.OrderActivityView getLastOrderActivity(int OrderRequestId)
        {

            Data.DBSchema.OrderActivityView retVal = new Data.DBSchema.OrderActivityView();

            retVal = Db.Where<Data.DBSchema.OrderActivityView>(q => q.OrderRequestId == OrderRequestId)
                        .OrderByDescending(r => r.OrderActivityId)
                        .FirstOrDefault();

            return retVal;


        }

        public Data.DBSchema.OrderActivityView getLastOrderActivity(string OrderNumber)
        {

            Data.DTO.OrderDetailResp retVal = new Data.DTO.OrderDetailResp();

            int OrderRequestId = getOrderRequestIdFromOrderNumber(OrderNumber);

            return getLastOrderActivity(OrderRequestId);
        
        }

        public List<Data.DBSchema.OrderActivityView> getOrderActivity(int OrderRequestId)
        {

            List<Data.DBSchema.OrderActivityView> retVal = new List<Data.DBSchema.OrderActivityView>() { };

            retVal = Db.Where<Data.DBSchema.OrderActivityView>(q => q.OrderRequestId == OrderRequestId)
                        .OrderByDescending(r => r.OrderActivityId).ToList();

            return retVal;


        }

        public OrderActivity getOrderActivityById(int OrderActivityId) {

            return Db.Where<OrderActivity>(q => q.Id == OrderActivityId).FirstOrDefault();

        }

        public int getOrderRequestIdFromOrderNumber(string OrderNumber)
        {
            int retVal = 0;
            
            retVal = Db.Where<Data.DBSchema.OrderDetailView>(q => q.OrderNumber == OrderNumber).FirstOrDefault().OrderRequestId;
            
            return retVal;
        }

        public string getOrderNumberFromOrderRequestId(int OrderRequestId)
        {
            string retVal;

            retVal = Db.Where<Data.DBSchema.OrderDetailView>(q => q.OrderRequestId == OrderRequestId).FirstOrDefault().OrderNumber;

            return retVal;
        }

        public List<Data.DBSchema.OrderDetailView> getOrderDetailForLoanNumber(string LoanNumber)
        {

            return Db.Where<Data.DBSchema.OrderDetailView>(q => q.LoanNumber == LoanNumber).ToList();

        }

        public Data.DTO.OrderDetailResp getOrderDetail(int OrderRequestId)
        {

            Data.DTO.OrderDetailResp retVal = new Data.DTO.OrderDetailResp();

            Data.Business.OrderOps oOp = new Data.Business.OrderOps();
            retVal = oOp.getOrderDetail(Db, OrderRequestId, null, null, false);

            return retVal;

        }
        
        public Data.DTO.OrderDetailResp getOrderDetail(string OrderNumber)
        {

            Data.DTO.OrderDetailResp retVal = new Data.DTO.OrderDetailResp();

            int OrderRequestId = getOrderRequestIdFromOrderNumber(OrderNumber);

            return getOrderDetail(OrderRequestId);

        }

        public Data.DTO.Email getOrderEmailDraft(int OrderRequestId)
        {

            Data.DTO.Email retVal = null;

            Data.Business.EmailOps eOp = new Data.Business.EmailOps();
                retVal = eOp.getEmailDraft(Db, OrderRequestId);

            
            return retVal;

        }

        public List<Data.DTO.Borrower> getBorrowerListForLoan(string LoanNumber)
        {
            List<Data.DTO.Borrower> retVal = new List<Data.DTO.Borrower>() { };

            Data.Business.LoanInfoOps lOp = new Data.Business.LoanInfoOps();
            retVal = lOp.getBorrowersForLoan(Db, LoanNumber) ;
                           
            return retVal;

        }

        public int getOrderLastRushRequest()
        {
            int retVal = 0;

            int openRushRequestStatusId = Db.Where<Data.DBSchema.RushRequestStatus>(q => q.Name == "Requested")
                .FirstOrDefault().Id;

            retVal = Db.Where<Data.DBSchema.RushRequest>(q => q.RushRequestStatusId == openRushRequestStatusId)
                        .OrderByDescending(r => r.OrderRequestId).FirstOrDefault().OrderRequestId;

            return retVal;


        }
        
        public int getOrderWithDeletableDocuments(out List<Data.DBSchema.DocumentOrderView> documents, string username)
        {
            int retVal = 0;

            documents = Db.Where<Data.DBSchema.DocumentOrderView>(q => q.DocumentTypeName == "UserUploaded"
                    && q.LoanLevelDoc == 0
                    && q.OwnerUser == username
                    ).OrderByDescending(r => r.DocumentId).ToList();

            if (documents != null)
            {
                retVal = documents.FirstOrDefault().OrderRequestId;
            }     
           
            return retVal;


        }

        public int getOrderWithEmailAttachment()
        {
            int retVal = 0;

            int emailId = Db.Select<Data.DBSchema.EmailAttachment>()
                    .OrderByDescending(r => r.Id).FirstOrDefault().EmailId;

            retVal = Db.Where<Data.DBSchema.Email>(q => q.Id == emailId && q.OrderRequestId != null)
                .FirstOrDefault().OrderRequestId ?? 0;

            return retVal;


        }

        public string getLoanNumberBondProductType()
        {

            string retVal = string.Empty;

            List<string> orderLoanNumbers = getOrdersByCriteria(
                new List<string> { "New", "Pending" },
                new List<string> { "Initial" },
                new List<string> { "Active Loan" },
                new List<string> { })
                .Where<OrderSearchResp>(q => q.IsNonBorrower == true)
                .Select<OrderSearchResp, string>(r => r.LoanNumber)
                .ToList();

            retVal = Db.Where<Data.DBSchema.emdbLoanInfoView>(q => Sql.In(q.LoanNumber, orderLoanNumbers)
                && q.EncProductType.Contains("Bond"))
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().LoanNumber;

            return retVal;

        }

        public List<Data.DBSchema.DocumentUploadQueueView> getOrderDocumentUploadQueue(int OrderRequestId)
        {
            
            return Db.Where<Data.DBSchema.DocumentUploadQueueView>(q => q.OrderRequestId == OrderRequestId).ToList();

        }

        public List<Data.DBSchema.DocumentOrderView> getOrderDocuments(int OrderRequestId)
        {

            return Db.Where<Data.DBSchema.DocumentOrderView>(q => q.OrderRequestId == OrderRequestId).ToList();

        }

        public void updateOrderVOEAssignment(int OrderRequestId, string UserName)
        {

            VOESystem.UnitTests.Business.OrderOps oOp = new UnitTests.Business.OrderOps();
            VOESystem.Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            if (orderDetail.VerificationSpecialist.ToLower() != UserName.ToLower())
            {
                //ok so assign it
                RequestUser assign = new RequestUser
                {
                    OrderRequestId = OrderRequestId,
                    UserName = UserName,
                    AssignmentDateTime = DateTime.Now
                };

                Db.Insert<RequestUser>(assign);
            }
            
        }
    
        public List<OpenToastAlertView> getOrderToastAlerts(int OrderRequestId)
        {
            return Db.Where<OpenToastAlertView>(q => q.OrderRequestId == OrderRequestId).ToList();

        }

        public List<FieldAudit> getRevisedFieldList(int OrderRequestId)
        {
            return Db.Where<FieldAudit>(q => q.OrderRequestId == OrderRequestId).ToList();
        }

        public int getOrderApprovalCount(int OrderRequestId)
        {
            return Db.Where<OrderActivityView>(q => q.OrderRequestId == OrderRequestId && q.CertificationFilePath != null).ToList().Count;
        }

        public bool getOrderLock(int OrderRequestId, string userName = null)
        {
            if (userName == null)
            {
                userName = UserName;
            }

            Data.Business.OrderOps oOp = new Data.Business.OrderOps();
            return oOp.updateOrderLock(Db, OrderRequestId, 5, userName).message == "1";

        }

        public bool deleteOrderLock(int OrderRequestId)
        {

            Data.Business.OrderOps oOp = new Data.Business.OrderOps();
            return oOp.deleteOrderLock(Db, OrderRequestId, UserName).message == "1";

        }


    }
}
