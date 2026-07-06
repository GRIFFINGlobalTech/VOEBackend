using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VOESystem.Data.DTO;

namespace VOEBackend.Interfaces
{

    public interface ILoanInfo
    {
        List<LoanInfoResp> getLoanInfoSDK(string loanID, string UserName, string Password, string[] LoanFolders, object encompasssession);

        List<LoanInfoResp> getLoanInfoREST(string loanID, string UserName, string Password, string accessToken);
    }

    public interface ILoanAttachment
    {
        void AddAttachment(string loanID, string filePathname, string UserName, string Password, string[] LoanFolders, string eDocumentType, string sDocumentTitle, string sMilestone, string sDescription, object encompasssession, bool IsAlert = false);
    }

    public interface IEncompassUpdate
    {
        void UpdateFieldsForLoan(string loanID, System.Collections.Generic.Dictionary<string, string> fields, string UserName, string Password, string[] LoanFolders, object encompasssession, bool bUpdateVOEDocs, bool bAddVOEPermissions);
    }

    public interface IVerifyLogin
    {
        bool Verify(string UserName, string Password, out string emailAddress, out DateTime nextBusinessDay, out DateTime nextBusiness5thDay, out DateTime prevBusinessDay);
    }

    public interface IUserUpdate
    {
        bool updateUserOOOStatus();
    }

}
