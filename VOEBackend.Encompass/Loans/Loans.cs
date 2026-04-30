using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
//using EllieMae.Encompass.BusinessObjects.Contacts;
//using EllieMae.Encompass.BusinessObjects.Loans;
//using EllieMae.Encompass.BusinessObjects.Loans.Logging;
//using EllieMae.Encompass.Collections;
//using EllieMae.Encompass.Query;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using VOEBackend.Interfaces;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;


namespace VOEBackend.Encompass
{

    public partial class Loans : BaseClass, ILoanInfo
    {

        public static string testLoanStatus = ConfigurationManager.AppSettings["TestLoanStatus"].ToString();

        #region SDK Version

        public List<LoanInfoResp> getLoanInfoSDK(string loanID, string UserName, string Password, string[] LoanFolders, object encompasssession)
        {
            throw new NotImplementedException();
        }

        ////unused
        //public List<LoanInfoResp> getLoanInfoSDK(string loanID, string UserName, string Password, string[] LoanFolders, object encompasssession)
        //{

        //    EllieMae.Encompass.Client.Session emSession = null;
        //    List<LoanInfoResp> retList = new List<LoanInfoResp>() { };

        //    Log.Info("Starting Loan Lookup for " + loanID);

        //    try
        //    {
        //        //start encompass session
        //        if (encompasssession == null)
        //        {
        //            emSession = new EllieMae.Encompass.Client.Session();
        //            emSession.Start(encompassServer, UserName, Password);
        //        }
        //        else
        //        {
        //            emSession = (EllieMae.Encompass.Client.Session)encompasssession;
        //        }

        //        // Fetch the loan folder

        //        //*** Define QUERY Criteria
        //        // Build the string criterion
        //        StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
        //        loanIDCriterion.FieldName = "Fields.364";
        //        loanIDCriterion.Value = loanID.Trim();
        //        loanIDCriterion.MatchType = StringFieldMatchType.Exact;

        //        //add folder criteria
        //        QueryCriterion folderCriteria = null;

        //        foreach (string loanfolder in LoanFolders)
        //        {
        //            StringFieldCriterion folderCriterion = new StringFieldCriterion();
        //            folderCriterion.FieldName = "Loan.LoanFolder";
        //            folderCriterion.Value = loanfolder;
        //            folderCriterion.MatchType = StringFieldMatchType.Exact;

        //            if (folderCriteria == null)
        //            {
        //                folderCriteria = folderCriterion;
        //            }
        //            else
        //            {
        //                folderCriteria = folderCriteria.Or(folderCriterion);
        //            }
        //        }

        //        // Join the criteria together using AND logic
        //        QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

        //        // Perform the query, retrieving the identities of the matching loans
        //        LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

        //        //should only return one loan
        //        if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }

        //        Loan loan = emSession.Loans.Open(ids[0].Guid);

        //        DateTime schedCloseDate = DateTime.Parse(getFieldValue(loan.Fields["cx.schclose"].Value,"1/1/1900"));

        //        string loanOfficerName = getFieldValue(loan.Fields["317"].Value);
        //        string processorName = getFieldValue(loan.Fields["362"].Value);
        //        string loanChannel = getFieldValue(loan.Fields["2626"].Value);
        //        string productType = getFieldValue(loan.Fields["1401"].Value);
        //        string loanType = getFieldValue(loan.Fields["1172"].Value);
        //        string sEncLoanStatus = getFieldValue(loan.Fields["1393"].Value, "");
        //        string sEncLoanProgram = getFieldValue(loan.Fields["1401"].Value, "");
        //        string sOrgId = getFieldValue(loan.Fields["ORGID"].Value, "");
        //        string mCCLoan = getFieldValue(loan.Fields["CX.SM.MCC"].Value);
        //        string sVAVeteranLoanCode = getFieldValue(loan.Fields["958"].Value);
               
        //        string loanAssistantName = null;
        //        EllieMae.Encompass.BusinessObjects.Loans.Role lo = emSession.Loans.Roles.GetRoleByName("LO Assistant");
        //        LoanAssociateList associates = loan.Associates.GetAssociatesByRole(lo);
        //        if (associates.Count > 0)
        //        {
        //            if (associates[0].ContactName != "") { loanAssistantName = associates[0].ContactName.Replace("  ", " "); };
        //        }
               
        //        List<Borrower> retBorrowerList = new List<Borrower>() { };

        //        //get list for all borrowers
        //        foreach (BorrowerPair bPair in loan.BorrowerPairs)
        //        {

        //            Borrower curBorrower = new Borrower();
        //            Borrower curCoBorrower = new Borrower();

        //            List<Employer> curBorrowerEmps = new List<Employer>() { };
        //            List<Employer> curCoBorrowerEmps = new List<Employer>() { };

        //            string B1FullName = bPair.Borrower.ToString().Trim();
        //            string B2FullName = bPair.CoBorrower.ToString().Trim();

        //            curBorrower.BorrowerFirstName = bPair.Borrower.FirstName;
        //            curBorrower.BorrowerLastName = bPair.Borrower.LastName;
        //            curBorrower.BorrowerAKAName = loan.Fields["1869"].GetValueForBorrowerPair(bPair);

        //            string B1Street = loan.Fields["FR0104"].GetValueForBorrowerPair(bPair);
        //            string B1City = loan.Fields["FR0106"].GetValueForBorrowerPair(bPair);
        //            string B1State = loan.Fields["FR0107"].GetValueForBorrowerPair(bPair);
        //            string B1Zip = loan.Fields["FR0108"].GetValueForBorrowerPair(bPair);
        //            curBorrower.BorrowerAddress = B1Street + "**" + B1City + ", " + B1State + " " + B1Zip;
        //            curBorrower.BorrowerSSN = loan.Fields["65"].GetValueForBorrowerPair(bPair);
        //            curBorrower.BorrowerHomePhone = loan.Fields["66"].GetValueForBorrowerPair(bPair);
        //            curBorrower.BorrowerMobilePhone = loan.Fields["1490"].GetValueForBorrowerPair(bPair);
        //            curBorrower.BorrowerGender = loan.Fields["471"].GetValueForBorrowerPair(bPair);

        //            curBorrower.BorrowerDOB = DateTime.Parse(getFieldValue(loan.Fields["1402"].GetValueForBorrowerPair(bPair), "1/1/1900"));
        //            curBorrower.BorrowerEmail = loan.Fields["1240"].GetValueForBorrowerPair(bPair);

        //            if (B2FullName != "")
        //            {
        //                //fill in coborrower fields
        //                curCoBorrower.BorrowerFirstName = bPair.CoBorrower.FirstName;
        //                curCoBorrower.BorrowerLastName = bPair.CoBorrower.LastName;
        //                curCoBorrower.BorrowerAKAName = loan.Fields["1874"].GetValueForBorrowerPair(bPair);

        //                string B2Street = loan.Fields["FR0204"].GetValueForBorrowerPair(bPair);
        //                string B2City = loan.Fields["FR0206"].GetValueForBorrowerPair(bPair);
        //                string B2State = loan.Fields["FR0207"].GetValueForBorrowerPair(bPair);
        //                string B2Zip = loan.Fields["FR0208"].GetValueForBorrowerPair(bPair);
        //                curCoBorrower.BorrowerAddress = B2Street + "**" + B2City + ", " + B2State + " " + B2Zip;
        //                curCoBorrower.BorrowerSSN = loan.Fields["97"].GetValueForBorrowerPair(bPair);
        //                curCoBorrower.BorrowerDOB = DateTime.Parse(loan.Fields["1403"].GetValueForBorrowerPair(bPair));
        //                curCoBorrower.BorrowerEmail = loan.Fields["1268"].GetValueForBorrowerPair(bPair);
        //                curCoBorrower.BorrowerHomePhone = loan.Fields["98"].GetValueForBorrowerPair(bPair);
        //                curCoBorrower.BorrowerMobilePhone = loan.Fields["1480"].GetValueForBorrowerPair(bPair);
        //                curCoBorrower.BorrowerGender = loan.Fields["478"].GetValueForBorrowerPair(bPair);

        //            }


        //            //loop through borrower employers
        //            //for (int i = 1; i <= loan.BorrowerEmployers.Count; i++)
        //            //BorrowerEmployers was not accurate when there is more tha one Borrower pair
        //            //just arbitrairily set limit at 20 --- 19 is the record
        //            for (int i = 1; i <= 20; i++)                   
        //            {
        //                Employer emp = getEmployerFromBorrowerPair(loan, bPair, i, "B");
        //                if (emp.EmployerName != "") { curBorrowerEmps.Add(emp); };
        //            }

        //            //loop through coborrower employers
        //            for (int i = 1; i <= 20; i++)
        //            {
        //                Employer emp = getEmployerFromBorrowerPair(loan, bPair, i, "C");
        //                if (emp.EmployerName != "") { curCoBorrowerEmps.Add(emp); };
        //            }

        //            //add employer lists to borrowers
        //            curBorrower.Employers = curBorrowerEmps;
        //            curCoBorrower.Employers = curCoBorrowerEmps;

        //            //add borrower to outputlist
        //            retBorrowerList.Add(curBorrower);
        //            if (B2FullName != "") { retBorrowerList.Add(curCoBorrower); }

        //        }


        //        //loop through documents, see if borrower auth form is present
        //        List<string> authFormNames = new List<string>() { };
        //        authFormNames.Add("Borrower's Certification and Authorization");
        //        authFormNames.Add("Borrowers Certification and Authorization");
        //        authFormNames.Add("Borrower's Certification & Authorization");
        //        authFormNames.Add("Borrowers Certification & Authorization");
        //        authFormNames.Add("Borrower's Certification & Authorization (Brokered)");
        //        //authFormNames.Add("Non Borrower Request for Information");
        //        bool bHasBorrowerAuth = hasDocInBucketSDK(loan.Log.TrackedDocuments, authFormNames);

        //        List<string> prospectAuthFormNames = new List<string>() { };
        //        prospectAuthFormNames.Add("Prequalification Request for Information");
        //        bool bHasProspectAuth = hasDocInBucketSDK(loan.Log.TrackedDocuments, prospectAuthFormNames);


        //        //build loaninforesp list from borrowerlist
        //        foreach (Borrower bor in retBorrowerList)
        //        {
                    
        //            if (bor.Employers.Count == 0)
        //            {
        //                //create loaninfo without employer - for instant voe only
        //                LoanInfoResp newInfo = new LoanInfoResp
        //                {
        //                    LoanNumber = loanID.Trim(),
        //                    BorrowerFirstName = bor.BorrowerFirstName,
        //                    BorrowerLastName = bor.BorrowerLastName,
        //                    BorrowerAKAName = bor.BorrowerAKAName,
        //                    BorrowerAddress = bor.BorrowerAddress,
        //                    SchedClosingDate = schedCloseDate,
        //                    BorrowerDOB = bor.BorrowerDOB,
        //                    BorrowerSSN = bor.BorrowerSSN,
        //                    BorrowerEmail = bor.BorrowerEmail,
        //                    BorrowerHomePhone = bor.BorrowerHomePhone,
        //                    BorrowerMobilePhone = bor.BorrowerMobilePhone,
        //                    BorrowerGender = bor.BorrowerGender,
        //                    EncCurrentLoanFolder = loan.LoanFolder,
        //                    EncLoanOfficerName = loanOfficerName,
        //                    EncLoanAssistantName = loanAssistantName,
        //                    EncProcessorName = processorName,
        //                    EncLoanChannel = loanChannel,
        //                    EncLoanProductType = productType,
        //                    HasBorrowerAuth = bHasBorrowerAuth,
        //                    HasProspectAuth = bHasProspectAuth,
        //                    EncLoanType = loanType,
        //                    EncLoanStatus = sEncLoanStatus,
        //                    EncLoanProgram = sEncLoanProgram,
        //                    OrgId = sOrgId,
        //                    MCCLoan = mCCLoan,
        //                    VAVeteranLoanCode = sVAVeteranLoanCode
        //                };

        //                retList.Add(newInfo);

        //                Log.Info(loanID.Trim() + ";" + bor.BorrowerFirstName + " " + bor.BorrowerLastName + "; NONE; NONE");


        //            }
        //            else
        //            {

        //                foreach (Employer emp in bor.Employers)
        //                {
        //                    LoanInfoResp newInfo = new LoanInfoResp
        //                    {
        //                        LoanNumber = loanID.Trim(),
        //                        BorrowerFirstName = bor.BorrowerFirstName,
        //                        BorrowerLastName = bor.BorrowerLastName,
        //                        BorrowerAKAName = bor.BorrowerAKAName,
        //                        BorrowerAddress = bor.BorrowerAddress,
        //                        SchedClosingDate = schedCloseDate,
        //                        BorrowerDOB = bor.BorrowerDOB,
        //                        BorrowerSSN = bor.BorrowerSSN,
        //                        BorrowerEmail = bor.BorrowerEmail,
        //                        BorrowerHomePhone = bor.BorrowerHomePhone,
        //                        BorrowerMobilePhone = bor.BorrowerMobilePhone,
        //                        BorrowerGender = bor.BorrowerGender,
        //                        EncEmployerName = emp.EmployerName,
        //                        EncEmployerPhone = emp.EmployerPhone,
        //                        EncEmployerAddress = emp.EmployerAddress,
        //                        EncEmploymentTitle = emp.EmploymentTitle,
        //                        EncEmploymentSelfFlag = emp.EmploymentSelfFlag,
        //                        CPAName = emp.CPAName,
        //                        CPAPhone = emp.CPAPhone,
        //                        CPAEmail = emp.CPAEmail,
        //                        EncMonthsOnJob = emp.MonthsOnJob,
        //                        EncYearsOnJob = emp.YearsOnJob,
        //                        EncYearsInLineOfWork = emp.YearsInLineOfWork,
        //                        EncEmployerFax = emp.EmployerFax,
        //                        EncEmployerEmail = emp.EmployerEmail,
        //                        EncStartDate = emp.StartDate,
        //                        EncTerminationDate = emp.TerminationDate,
        //                        EncEmploymentStatus = emp.EmploymentStatus,
        //                        EncCurrentLoanFolder = loan.LoanFolder,
        //                        EncLoanOfficerName = loanOfficerName,
        //                        EncLoanAssistantName = loanAssistantName,
        //                        EncProcessorName = processorName,
        //                        EncLoanChannel = loanChannel,
        //                        EncLoanProductType = productType,
        //                        HasBorrowerAuth = bHasBorrowerAuth,
        //                        HasProspectAuth = bHasProspectAuth,
        //                        EncLoanType = loanType,
        //                        EncLoanStatus = sEncLoanStatus,
        //                        EncLoanProgram = sEncLoanProgram,
        //                        OrgId = sOrgId,
        //                        MCCLoan = mCCLoan,
        //                        VAVeteranLoanCode = sVAVeteranLoanCode
        //                    };

        //                    retList.Add(newInfo);

        //                    //Log.Debug(newInfo.ToJson());

        //                    Log.Info(loanID.Trim() + ";" + bor.BorrowerFirstName + " " + bor.BorrowerLastName + ";" + emp.EmployerName + ";" + emp.StartDate.ToString());

        //                }
        //            }
        //        }

        //        loan.Close();
                

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Retrieving Loan " + loanID, ex);
        //        throw ex;
        //    }

            

        //    //cleaup local session
        //    if (encompasssession == null && emSession != null)
        //    {
        //        emSession.End();
        //    }

        //    return retList;
        //}

        //Employer getEmployerFromBorrowerPair(Loan loan, BorrowerPair bPair, int i, string sFieldPrefix)
        //{
        //    string empStreet = loan.Fields.GetFieldAt(sFieldPrefix + "E04", i).GetValueForBorrowerPair(bPair);
        //    string empCity = loan.Fields.GetFieldAt(sFieldPrefix + "E05", i).GetValueForBorrowerPair(bPair);
        //    string empState = loan.Fields.GetFieldAt(sFieldPrefix + "E06", i).GetValueForBorrowerPair(bPair);
        //    string empZip = loan.Fields.GetFieldAt(sFieldPrefix + "E07", i).GetValueForBorrowerPair(bPair);
        //    string empSelfEmployed = loan.Fields.GetFieldAt(sFieldPrefix + "E15", i).GetValueForBorrowerPair(bPair);

        //    bool empSEFlag = false;
        //    if (empSelfEmployed == "Y") { empSEFlag = true; }

        //    string strStartDate = loan.Fields.GetFieldAt(sFieldPrefix + "E11", i).GetValueForBorrowerPair(bPair);
        //    string strTerminationDate = loan.Fields.GetFieldAt(sFieldPrefix + "E14", i).GetValueForBorrowerPair(bPair);
        //    DateTime? dStartDate = null;
        //    DateTime? dTerminationDate = null;
        //    DateTime dateValue;

        //    if ( DateTime.TryParse(strStartDate, out dateValue) ) { dStartDate = DateTime.Parse(strStartDate); };
        //    if ( DateTime.TryParse(strTerminationDate, out dateValue) ) { dTerminationDate = DateTime.Parse(strTerminationDate); };
                        
        //    string EmpStatus = "Prior";
        //    if ( loan.Fields.GetFieldAt(sFieldPrefix + "E09", i).GetValueForBorrowerPair(bPair) == "Y" ) 
        //    {
        //        EmpStatus = "Current";
        //    }

        //    Employer emp = new Employer
        //    {
        //        EmployerName = loan.Fields.GetFieldAt(sFieldPrefix + "E02", i).GetValueForBorrowerPair(bPair),
        //        EmployerPhone = loan.Fields.GetFieldAt(sFieldPrefix + "E17", i).GetValueForBorrowerPair(bPair),
        //        EmployerAddress = empStreet + "**" + empCity + ", " + empState + " " + empZip,
        //        EmploymentTitle = loan.Fields.GetFieldAt(sFieldPrefix + "E10", i).GetValueForBorrowerPair(bPair),
        //        EmploymentSelfFlag = empSEFlag,
        //        CPAName = "",
        //        CPAPhone = "",
        //        CPAEmail = "",
        //        MonthsOnJob = loan.Fields.GetFieldAt(sFieldPrefix + "E33", i).GetValueForBorrowerPair(bPair),
        //        YearsOnJob = loan.Fields.GetFieldAt(sFieldPrefix + "E13", i).GetValueForBorrowerPair(bPair),
        //        YearsInLineOfWork = loan.Fields.GetFieldAt(sFieldPrefix + "E16", i).GetValueForBorrowerPair(bPair),
        //        EmployerFax = loan.Fields.GetFieldAt(sFieldPrefix + "E29", i).GetValueForBorrowerPair(bPair),
        //        EmployerEmail = loan.Fields.GetFieldAt(sFieldPrefix + "E30", i).GetValueForBorrowerPair(bPair),
        //        StartDate = dStartDate,
        //        TerminationDate = dTerminationDate,
        //        EmploymentStatus = EmpStatus
        //    };

        //    return emp;
        //}

        //private string getFieldValue(object Field, string defaultVal = null)
        //{
        //    string retVal = defaultVal;

        //    if (Field != null)
        //    {
        //        retVal = Field.ToString().Trim();
        //        if (retVal == "")
        //        {
        //            retVal = defaultVal;
        //        }
        //    }

        //    return retVal;
        //}

        //public bool hasDocInBucketSDK(LogTrackedDocuments TrackedDocList, List<string> bucketList)
        //{
        //    bool retVal = false;

        //    foreach (TrackedDocument document in TrackedDocList)
        //    {
        //        if (bucketList.Contains(document.Title))
        //        {
        //            //make sure there is an actual file there
        //            AttachmentList atts = document.GetAttachments();
        //            if (atts.Count > 0)
        //            {
        //                foreach (Attachment att in atts)
        //                {
        //                    if ( att.Size > 0 ) {
        //                        retVal = true;
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return retVal;
        //}

        //class Borrower
        //{

        //    public string BorrowerFirstName { get; set; }
        //    public string BorrowerLastName { get; set; }
        //    public string BorrowerAKAName { get; set; }
        //    public string BorrowerAddress { get; set; }
        //    public DateTime BorrowerDOB { get; set; }
        //    public string BorrowerSSN { get; set; }
        //    public string BorrowerEmail { get; set; }
        //    public string BorrowerHomePhone { get; set; }
        //    public string BorrowerMobilePhone { get; set; }
        //    public string BorrowerGender { get; set; }

        //    public List<Employer> Employers;

        //}

        //class Employer
        //{
        //    public string EmployerName { get; set; }
        //    public string EmployerPhone { get; set; }
        //    public string EmployerAddress { get; set; }
        //    public string EmploymentTitle { get; set; }
        //    public bool EmploymentSelfFlag { get; set; }
        //    public string CPAName { get; set; }
        //    public string CPAPhone { get; set; }
        //    public string CPAEmail { get; set; }
        //    public string MonthsOnJob { get; set; }
        //    public string YearsOnJob { get; set; }
        //    public string YearsInLineOfWork { get; set; }
        //    public string EmployerFax { get; set; }
        //    public string EmployerEmail { get; set; }
        //    public DateTime? StartDate { get; set; }
        //    public DateTime? TerminationDate { get; set; }
        //    public string EmploymentStatus { get; set; }
        //}

#endregion

#region REST Version
        
        public List<LoanInfoResp> getLoanInfoREST(string loanNumber, string UserName, string Password)
        {
            List<LoanInfoResp> retList = new List<LoanInfoResp>() { };

            try
            {

                string loanGUID;
                string accessToken;
                FHMC.EncompassREST.Loan loan;
                Dictionary<string, string> loanFields;
                List<LoanValue> loanValues;

                //get basic fields for loan
                try
                {
                    FHMC.EncompassREST.Authentication auth = new FHMC.EncompassREST.Authentication();
                    accessToken = auth.getAccessToken(UserName, Password);

                    loan = new FHMC.EncompassREST.Loan();

                    loanGUID = loan.getGUIDforLoanNumber(loanNumber, accessToken);

                    loanValues = new List<LoanValue>() { };

                    loanValues.Add(new LoanValue("SchedClosingDate", "cx.schclose"));
                    loanValues.Add(new LoanValue("EncCurrentLoanFolder", "Loan.LoanFolder"));
                    loanValues.Add(new LoanValue("EncLoanOfficerName", "317"));
                    loanValues.Add(new LoanValue("EncLoanAssistantName", "LoanTeamMember.Name.LO Assistant"));
                    loanValues.Add(new LoanValue("EncBranchAdminName", "LoanTeamMember.Name.Branch Admin"));
                    loanValues.Add(new LoanValue("EncProcessorName", "362"));
                    loanValues.Add(new LoanValue("EncLoanChannel", "2626"));
                    loanValues.Add(new LoanValue("EncLoanProductType", "1401"));
                    loanValues.Add(new LoanValue("EncLoanType", "1172"));
                    loanValues.Add(new LoanValue("EncLoanStatus", "1393"));
                    loanValues.Add(new LoanValue("EncLoanProgram", "1401"));
                    loanValues.Add(new LoanValue("EncClosingDate", "748"));
                    loanValues.Add(new LoanValue("OrgId", "ORGID"));
                    loanValues.Add(new LoanValue("MCCLoan", "CX.SM.MCC"));
                    loanValues.Add(new LoanValue("VAVeteranLoanCode", "958"));
                    loanValues.Add(new LoanValue("EncCredentialingOptOut", "CX.3498"));

                    loanFields = loan.getReportingFieldsforLoanNumber(loanNumber,
                        loanValues.Select<LoanValue, string>(q => q.FieldId).ToList(), accessToken);

                    //------update loanValues with encompass data
                    //------outer = loanValues -> List<LoanValue> (lvs)
                    //------inner = loanFields -> Dictionary<string,string> (value)
                    loanValues = loanValues.SelectMany(lvs => loanFields.Where(value => lvs.FieldId == value.Key).DefaultIfEmpty(),
                        (y, z) => { y.FieldValue = y.FieldValue ?? z.Value; return y; }).ToList();

                    //test value override
                    if (testLoanStatus != "")
                    {
                        loanValues.Remove(loanValues.Where<LoanValue>(q => q.VarName == "EncLoanStatus").FirstOrDefault());
                        loanValues.Add(new LoanValue("EncLoanStatus", "1393", testLoanStatus));
                    }

                }
                catch (Exception fex)
                {
                    Log.Error("Error getting fields for loan " + loanNumber, fex);
                    throw fex;
                }

                //checking for authorization forms
                bool bHasBorrowerAuth = false;
                
                try
                {

                    //get attachment list for loan GUID
                    FHMC.EncompassREST.Documents docs = new FHMC.EncompassREST.Documents();
                    List<FHMC.EncompassREST.Documents.Attachment> attList = docs.getAttachmentListForLoan(loanGUID, accessToken, loanNumber);

                    //check for borrower auth form
                    List<string> authFormNames = new List<string>() { };
                    authFormNames.Add("Borrower's Certification and Authorization");
                    authFormNames.Add("Borrowers Certification and Authorization");
                    authFormNames.Add("Borrower's Certification & Authorization");
                    authFormNames.Add("Borrowers Certification & Authorization");
                    authFormNames.Add("Borrower's Certification & Authorization (Brokered)");
                    authFormNames.Add("Prequalification Request for Information");
                    authFormNames.Add("Prequalification Request for Information CoBorrower");

                    bHasBorrowerAuth = hasDocInBucketREST(attList, authFormNames);

                    //check for prospect auth form
                    //List<string> prospectAuthFormNames = new List<string>() { };
                    //prospectAuthFormNames.Add("Prequalification Request for Information");
                    //bHasProspectAuth = hasDocInBucketREST(attList, prospectAuthFormNames);

                }
                catch (Exception aex)
                {
                    Log.Error("Error checking authorization forms for loan " + loanNumber, aex);
                    throw aex;
                }

                try
                {

                    //create dictionary for use in sub
                    Dictionary<string, object> loanValuesDict = loanValues.Select<LoanValue, KeyValuePair<string, object>>(q =>
                        new KeyValuePair<string, object>(q.VarName, q.FieldValue)).ToDictionary(r => r.Key, r => r.Value);

                    List<FHMC.EncompassREST.Loan.BorrowerPair> bPairs = loan.getBorrowerPairs(loanGUID, loanNumber, accessToken);

                    foreach (FHMC.EncompassREST.Loan.BorrowerPair pair in bPairs)
                    {
                        retList.AddRange(getLoanInfosFromBorrowerPair(pair, BorrowerType.Borrower, loanNumber, loanValuesDict, bHasBorrowerAuth));
                        retList.AddRange(getLoanInfosFromBorrowerPair(pair, BorrowerType.CoBorrower, loanNumber, loanValuesDict, bHasBorrowerAuth));
                    }

                }
                catch (Exception bex)
                {
                    throw bex;
                }



            }
            catch (FHMC.EncompassREST.CustomException.CustomEMUserLoanPermissionException pex)
            {
                pex.LoanNumber = loanNumber;
                pex.UserName = UserName;
                throw pex;
            }
            

            return retList;

        }

        public class LoanValue
        {
            public LoanValue(string varName, string fieldId)  {
                VarName = varName;
                FieldId = fieldId;
                FieldValue = null;
            }

            public LoanValue(string varName, string fieldId, string fieldValue)
            {
                VarName = varName;
                FieldId = fieldId;
                FieldValue = fieldValue;
            }

            public string VarName { get; set;}
            public string FieldId { get; set;}
            public object FieldValue { get; set;}
        }

        public List<LoanInfoResp> getLoanInfosFromBorrowerPair(FHMC.EncompassREST.Loan.BorrowerPair pair, BorrowerType bType,
            string loanNumber, Dictionary<string, object> loanValues, bool bHasBorrowerAuth)
        {

            List<LoanInfoResp> retVal = new List<LoanInfoResp>() { };

            bool bIsCoborrower = false;
            FHMC.EncompassREST.Loan.Borrower bor = null;
            List<FHMC.EncompassREST.Loan.Employment> employments = null;
            int? EncCurrEmploymentCount = null;

            //get borrower employments
            try
            {
                //get borrower
                bor = (FHMC.EncompassREST.Loan.Borrower)typeof(FHMC.EncompassREST.Loan.BorrowerPair)
                    .GetProperty(bType.ToString().ToLower()).GetValue(pair);

                if (bType == BorrowerType.CoBorrower)
                {
                    bIsCoborrower = true;
                }

                //get list of employments for borrower
                employments = new List<FHMC.EncompassREST.Loan.Employment>() { };
                if (pair.employment != null)
                {
                    employments = pair.employment.Where<FHMC.EncompassREST.Loan.Employment>(
                        q => q.owner == bType.ToString()).ToList();

                    EncCurrEmploymentCount = employments.Where(q => q.currentEmploymentIndicator == true).ToList().Count;

                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting employments for loan " + loanNumber + " " + bor.firstName + " " + bor.lastName, ex);
                throw ex;
            }

            //get borrower address
            string borrAddress = "";

            try
            {
                //get address
                FHMC.EncompassREST.Loan.Residence res = pair.residences.Where<FHMC.EncompassREST.Loan.Residence>
                    (q => q.residencyType == "Current" && q.applicantType == bType.ToString()).FirstOrDefault();

                if (res != null)
                {
                    borrAddress = formatAddress(res.addressStreetLine1, res.addressCity, res.addressState, res.addressPostalCode);
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error getting address for loan " + loanNumber + " " + bor.firstName + " " + bor.lastName, ex);
                throw ex;
            }


            if (employments.Count == 0 && bor.firstNameWithMiddleName != null)
            {

                try
                {
                    //create loaninfo without employer - for instant voe only
                    retVal.Add(new LoanInfoResp
                    {
                        LoanNumber = loanNumber,
                        BorrowerFirstName = bor.firstNameWithMiddleName,
                        BorrowerLastName = bor.lastNameWithSuffix,
                        BorrowerAKAName = bor.aliasName,
                        BorrowerAddress = borrAddress,
                        SchedClosingDate = loanValues["SchedClosingDate"] == null || loanValues["SchedClosingDate"] == "" ? (DateTime?)DateTime.Parse("1900-01-01") : (DateTime?)DateTime.Parse(loanValues["SchedClosingDate"].ToString()),
                        BorrowerDOB = DateTime.Parse(bor.birthDate.ToString()),
                        BorrowerSSN = bor.taxIdentificationIdentifier,
                        BorrowerEmail = bor.emailAddressText,
                        BorrowerHomePhone = bor.homePhoneNumber,
                        BorrowerMobilePhone = bor.mobilePhone,
                        BorrowerGender = bor.hmdaGenderType,
                        EncCurrentLoanFolder = loanValues["EncCurrentLoanFolder"].ToString(),
                        EncCurrEmploymentCount = EncCurrEmploymentCount,
                        EncLoanOfficerName = loanValues["EncLoanOfficerName"].ToString().Replace("  ", ""),
                        EncLoanAssistantName = loanValues["EncLoanAssistantName"].ToString().Replace("  ", ""),
                        EncBranchAdminName = loanValues["EncBranchAdminName"].ToString().Replace("  ", ""),
                        EncProcessorName = loanValues["EncProcessorName"].ToString().Replace("  ", ""),
                        EncLoanChannel = loanValues["EncLoanChannel"].ToString(),
                        EncLoanProductType = loanValues["EncLoanProductType"].ToString(),
                        EncClosingDate = loanValues["EncClosingDate"] == null || loanValues["EncClosingDate"] == "" ? (DateTime?)DateTime.Parse("1900-01-01") : (DateTime?)DateTime.Parse(loanValues["EncClosingDate"].ToString()),
                        HasBorrowerAuth = bHasBorrowerAuth,                        
                        EncLoanType = loanValues["EncLoanType"].ToString(),
                        EncLoanStatus = loanValues["EncLoanStatus"].ToString(),
                        EncLoanProgram = loanValues["EncLoanProgram"].ToString(),
                        OrgId = loanValues["OrgId"].ToString(),
                        MCCLoan = loanValues["MCCLoan"].ToString(),
                        VAVeteranLoanCode = loanValues["VAVeteranLoanCode"].ToString(),
                        IsCoBorrower = bIsCoborrower,
                        BorrowerPairIndex = pair.applicationIndex
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("Error creating loan information for Instant Order " + loanNumber + " " + bor.firstName + " " + bor.lastName, ex);
                    throw ex;
                }

            }
            else
            {
                foreach (FHMC.EncompassREST.Loan.Employment emp in employments)
                {
                    try
                    {
                        DateTime? startDate = null;
                        DateTime? endDate = null;
                        DateTime? birthDate = null;

                        if (emp.employmentStartDate != null) { startDate = DateTime.Parse(emp.employmentStartDate.ToString()); };
                        if (emp.endDate != null) { endDate = DateTime.Parse(emp.endDate.ToString()); };
                        if (bor.birthDate != null) { birthDate = DateTime.Parse(bor.birthDate.ToString()); };

                        retVal.Add(new LoanInfoResp
                        {
                            LoanNumber = loanNumber,
                            BorrowerFirstName = bor.firstNameWithMiddleName,
                            BorrowerLastName = bor.lastNameWithSuffix,
                            BorrowerAKAName = bor.aliasName,
                            BorrowerAddress = borrAddress,
                            SchedClosingDate = loanValues["SchedClosingDate"] == null || loanValues["SchedClosingDate"] == "" ? (DateTime?)DateTime.Parse("1900-01-01") : (DateTime?)DateTime.Parse(loanValues["SchedClosingDate"].ToString()),
                            BorrowerDOB = birthDate,
                            BorrowerSSN = bor.taxIdentificationIdentifier,
                            BorrowerEmail = bor.emailAddressText,
                            BorrowerHomePhone = bor.homePhoneNumber,
                            BorrowerMobilePhone = bor.mobilePhone,
                            BorrowerGender = bor.hmdaGenderType,
                            EncEmployerName = emp.employerName,
                            EncEmployerPhone = emp.phoneNumber ?? emp.businessPhone,
                            EncEmployerAddress = formatAddress(emp.addressStreetLine1, emp.addressCity, emp.addressState, emp.addressPostalCode),
                            EncEmploymentTitle = emp.positionDescription,
                            EncEmploymentSelfFlag = emp.selfEmployedIndicator,
                            CPAName = "",
                            CPAPhone = "",
                            CPAEmail = "",
                            EncMonthsOnJob = emp.timeOnJobTermMonths.ToString(),
                            EncYearsOnJob = emp.timeOnJobTermYears.ToString(),
                            EncYearsInLineOfWork = emp.timeInLineOfWorkYears.ToString(),
                            EncEmployerFax = emp.fax,
                            EncEmployerEmail = emp.email,
                            EncStartDate = startDate,
                            EncTerminationDate = endDate,
                            EncEmploymentStatus = emp.currentEmploymentIndicator ? "Current" : "Prior",
                            EncCurrentLoanFolder = loanValues["EncCurrentLoanFolder"].ToString(),
                            EncCurrEmploymentCount = EncCurrEmploymentCount,
                            EncLoanOfficerName = loanValues["EncLoanOfficerName"].ToString().Replace("  ", ""),
                            EncLoanAssistantName = loanValues["EncLoanAssistantName"].ToString().Replace("  ", ""),
                            EncBranchAdminName = loanValues["EncBranchAdminName"].ToString().Replace("  ", ""),
                            EncProcessorName = loanValues["EncProcessorName"].ToString().Replace("  ", ""),
                            EncLoanChannel = loanValues["EncLoanChannel"].ToString(),
                            EncLoanProductType = loanValues["EncLoanProductType"].ToString(),
                            EncClosingDate = loanValues["EncClosingDate"] == null || loanValues["EncClosingDate"] == "" ? (DateTime?)DateTime.Parse("1900-01-01") : (DateTime?)DateTime.Parse(loanValues["EncClosingDate"].ToString()),
                            HasBorrowerAuth = bHasBorrowerAuth,
                            EncLoanType = loanValues["EncLoanType"].ToString(),
                            EncLoanStatus = loanValues["EncLoanStatus"].ToString(),
                            EncLoanProgram = loanValues["EncLoanProgram"].ToString(),
                            OrgId = loanValues["OrgId"].ToString(),
                            MCCLoan = loanValues["MCCLoan"].ToString(),
                            VAVeteranLoanCode = loanValues["VAVeteranLoanCode"].ToString(),
                            IsCoBorrower = bIsCoborrower,
                            BorrowerPairIndex = pair.applicationIndex
                        });

                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error creating loan information for Loan " + loanNumber + " " + bor.firstName + " " + bor.lastName + "; " + emp.employerName, ex);
                        throw ex;
                    }


                }
            }


            return retVal;
        }

        public string formatAddress(string street, string city, string state, string zip)
        {
            return street + "**" + city + ", " + state + " " + zip;
        }

        public string formatName(string name, string optionalAddOn)
        {
            if (isNull(optionalAddOn, "") == "")
            {
                return name;
            }
            else
            {
                return name + " " + optionalAddOn;
            }

        }

        public enum BorrowerType
        {
            Borrower,
            CoBorrower
        }

        public bool hasDocInBucketREST(List<FHMC.EncompassREST.Documents.Attachment> attachmentList, List<string> bucketList)
        {

            bool retVal = false;

            foreach (FHMC.EncompassREST.Documents.Attachment att in attachmentList)
            {
                if (att.document != null)
                {
                    if (bucketList.Contains(att.document.entityName))
                    {
                        retVal = true;
                        break;
                    }
                }
            }

            return retVal;

        }



#endregion

#region Docs and Permission Download
        //public void DownloadDocsAndPermissionsForLoan(string loanID, string UserName, string Password,
        //    string[] LoanFolders, object encompasssession)
        //{//used

        //    EllieMae.Encompass.Client.Session emSession = null;
        //    Loan loan = null;

        //    try
        //    {

        //        if (encompasssession == null)
        //        {
        //            emSession = new EllieMae.Encompass.Client.Session();
        //            emSession.Start(encompassServer, UserName, Password);
        //        }
        //        else
        //        {
        //            emSession = (EllieMae.Encompass.Client.Session)encompasssession;
        //        }

        //        //*** Define QUERY Criteria
        //        // Build the string criterion
        //        StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
        //        loanIDCriterion.FieldName = "Fields.364";
        //        loanIDCriterion.Value = loanID.Trim();
        //        loanIDCriterion.MatchType = StringFieldMatchType.Exact;

        //        //add folder criteria
        //        QueryCriterion folderCriteria = null;

        //        foreach (string loanfolder in LoanFolders)
        //        {
        //            StringFieldCriterion folderCriterion = new StringFieldCriterion();
        //            folderCriterion.FieldName = "Loan.LoanFolder";
        //            folderCriterion.Value = loanfolder;
        //            folderCriterion.MatchType = StringFieldMatchType.Exact;

        //            if (folderCriteria == null)
        //            {
        //                folderCriteria = folderCriterion;
        //            }
        //            else
        //            {
        //                folderCriteria = folderCriteria.Or(folderCriterion);
        //            }
        //        }

        //        // Join the criteria together using AND logic
        //        QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

        //        // Perform the query, retrieving the identities of the matching loans
        //        LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

        //        //should only return one loan
        //        if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }

        //        loan = emSession.Loans.Open(ids[0].Guid);

        //        //download missing docs into VOE system
        //        //Documents doc = new Documents();
        //        //doc.UpdateVOEDocs(ref loan);

        //        //update permissions in VOE system
        //        LoanPermissions perm = new LoanPermissions();
        //        //perm.addLoanPermissions(ref loan, ref emSession);

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Downloading Docs and Permissions for Loan: " + loanID, ex);

        //    }
        //    finally
        //    {
        //        if (loan != null)
        //        {
        //            loan.Close();
        //        }

        //        //cleanup local session
        //        if (encompasssession == null)
        //        {
        //            emSession.End();
        //        }
        //    }

        //}

        //public void DownloadDocsAndPermissionsForLoans(string UserName, string Password, string[] LoanFolders)
        //{//not used
        //    try
        //    {

        //        OrmLiteConfig.CommandTimeout = 120;

        //        OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
        //            ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
        //            true, SqlServerDialect.Provider);

        //        IDbConnection dbConn = factory.CreateDbConnection();
        //        dbConn.Open();

        //        //get activity list for last four days
        //        DateTime cutoffDate = DateTime.Now.AddDays(-4);

        //        /*List<LatestActivityView> activities =
        //            dbConn.Select<LatestActivityView>(
        //            q => q.ActivityDateTime >= cutoffDate).OrderBy(ob => ob.ActivityDateTime).OrderByDescending(ob2 => ob2.LoanNumber).ToList();*/

        //        /*List<LatestActivityView> activities =
        //            dbConn.Select<LatestActivityView>(
        //            q => q.ActivityDateTime >= cutoffDate).ToList();*/

        //        //get order creation activity so that will always be on the list
        //        List<CreationActivityView> creations =
        //            dbConn.Select<CreationActivityView>(
        //            q => q.ActivityDateTime >= cutoffDate).ToList();

        //        //merge two lists
        //        //List<UpdateActvity> updateactivity = MergeActivity(activities, creations);

        //        List<string> loanList = creations.Select(q => q.LoanNumber).Distinct().ToList();

        //        foreach (string loan in loanList)
        //        {
        //            Log.Info("Downloading Permissions and Docs for Loan: " + loan);
        //            DownloadDocsAndPermissionsForLoan(loan, UserName, Password, LoanFolders, null);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Downloading Docs and Permissions for Loans", ex);
        //    }

        //}
#endregion


        
    }
}

