using System;
using System.Collections.Generic;
using ServiceStack.OrmLite;
using VOESystem.Data.DBSchema;

namespace VOESystem.UnitTests.Business
{
    public class PipelineOps : BusinessBase
    {

        public void refreshUnreadMessagesForUser(string UserName)
        {

            Db.ExecuteSql("EXEC usp_Dev_CreateUnreadEmailsForUser '" + UserName + "'");

        }


        //public void saveTempPipelineSPRecord(List<PipelineSP> pipeineRecords, int testId)
        //{

        //    foreach (PipelineSP pip in pipeineRecords)
        //    {

        //        tempPipelineSP tPip = new tempPipelineSP
        //        {
        //            OrderRequestId = pip.OrderRequestId.ToString(),
        //            VerificationSpecialist = pip.VerificationSpecialist,
        //            OrderNumber = pip.OrderNumber,
        //            RequestType = pip.RequestType,
        //            OrderType = pip.OrderType,
        //            EncEmployerName = pip.EncEmployerName,
        //            EncEmployerPhone = pip.EncEmployerPhone,
        //            EncSchedClosingDate = pip.EncSchedClosingDate.ToString("dd-MM-yyyy"),
        //            RequestedDate = pip.RequestedDate.ToString("dd-MM-yyyy"),
        //            RequestedBy = pip.RequestedBy,
        //            VerificationLastAttemptDate = (pip.VerificationLastAttemptDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
        //            OrderStatus = pip.OrderStatus,
        //            VerifNote = pip.VerifNote,
        //            BorrowerFullName = pip.BorrowerFullName,
        //            EncLastMilestone = pip.EncLastMilestone,
        //            EncEmploymentStatus = pip.EncEmploymentStatus,
        //            OrderSubStatus = pip.OrderSubStatus,
        //            FollowupDate = (pip.FollowupDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
        //            LoanNumber = pip.LoanNumber,
        //            MessageCount = pip.MessageCount.ToString(),
        //            HasUnreadMsg = pip.HasUnreadMsg.ToString(),
        //            EncLoanOfficerName = pip.EncLoanOfficerName,
        //            EncProcessorName = pip.EncProcessorName,
        //            PanicMode = pip.PanicMode.ToString(),
        //            IsSubcontracted = pip.IsSubcontracted.ToString(),
        //            IsNonBorrower = pip.IsNonBorrower.ToString(),
        //            OnHoldCount = pip.OnHoldCount.ToString(),
        //            BorrowerL4SSN = pip.BorrowerL4SSN,
        //            IsRush = pip.IsRush.ToString(),
        //            IsFunded = pip.IsFunded.ToString(),
        //            IsMyLoan = pip.IsMyLoan.ToString(),
        //            FinalOrderLeadTimeDays = pip.FinalOrderLeadTimeDays.ToString(),
        //            IsPrelimVendorOrder = pip.IsPrelimVendorOrder.ToString(),
        //            PredictedDaysToApprovalExpiration = pip.PredictedDaysToApprovalExpiration.ToString(),
        //            EncLoanStatus = pip.EncLoanStatus,
        //            PredictedApprovalStatus = pip.PredictedApprovalStatus,
        //            InFinalPipeline = pip.InFinalPipeline.ToString(),
        //            EncLoanType = pip.EncLoanType,
        //            IsRevision = pip.IsRevision.ToString(),
        //            IsUrgent = pip.IsUrgent.ToString(),
        //            UsesAltDateForStatusCalc = pip.UsesAltDateForStatusCalc.ToString(),
        //            IsAuditing = pip.IsAuditing.ToString(),
        //            IsStandardLeadTime = pip.IsStandardLeadTime.ToString(),
        //            LockUserName = pip.LockUserName,
        //            LockExpirationDate = (pip.LockExpirationDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
        //            EncLoanAssistantName = pip.EncLoanAssistantName,
        //            VendorName = pip.VendorName,
        //            VendorDataDate = (pip.VendorDataDate ?? DateTime.Today).ToString("dd-MM-yyyy"),
        //            FollowupType = pip.FollowupType,
        //            PermissionTestId = testId

        //        };

        //        Db.Insert<tempPipelineSP>(tPip);

        //    }
        //}

        //public List<tempPermissionTestCases> getTempPermissionTestCases() {

        //    List<tempPermissionTestCases> retVal = new List<tempPermissionTestCases>() { };

        //    retVal = Db.Select<tempPermissionTestCases>();

        //    return retVal;

        //} 


    }
}
