using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using VOEBackend.Email;
using VOEBackend.Encompass;
using VOEBackend.Encompass.Reports;
using ServiceStack.Text;


namespace VOEBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RetreiveEmailJob();
            try
            {
                if (args.Count() > 0)
                {
                    if (args[0] == "/test")
                    {

                        //Console.WriteLine(res);
                        //Console.ReadKey();

                    }
                    else if (args[0] == "/avo")
                    {
                        //create the automatic reverification orders
                        int iDaysOffset = 0;
                        //this offset days allows to manually create orders for days other than today
                        if (args.Length > 1)
                        {
                            iDaysOffset = Int32.Parse(args[1]);
                        }

                        AutoReverifJob(iDaysOffset);

                        return;
                    }
                    //no longer need this since permissions are governed via rest api on the fly
                    //if (args[0] == "/pd")
                    //{
                    //    //permissions and doc updates
                    //    PermDocUpdateJob();
                    //    return;
                    //}
                    if (args[0] == "/ge")
                    {
                        //retrieve email
                        RetreiveEmailJob();
                        return;
                    }
                    if (args[0] == "/cd")
                    {
                        //cleanup docs
                        CleanupEncDocsJob();
                        return;
                    }
                    if (args[0] == "/sc")
                    {
                        //subcontracted order status check
                        CheckSubcontractedOrderStatus();
                        return;
                    }
                    if (args[0] == "/ar")
                    {
                        //archive orders
                        ArchiveOrders();
                        return;
                    }

                    if (args[0] == "/rpt")
                    {
                        //email report
                        int reportId = Int32.Parse(args[1].Trim());

                        EmailReport(reportId);
                        return;
                    }
                    if (args[0] == "/ul")
                    {
                        //update loan
                        LoanUpdateJob();
                        return;
                    }
                    if (args[0] == "/ooo")
                    {
                        //update OOO exchnage status
                        OOOUpdateJob();
                        return;
                    }
                    if (args[0] == "/ou")
                    {
                        //update order info from encompass
                        OrderUpdateJob();
                        return;
                    }
                    if (args[0] == "/loou")
                    {
                        //update order info from encompass
                        OriginatedOrderUpdateJob();
                        return;
                    }
                    if (args[0] == "/exp")
                    {
                        //alerts for unapproved finals closing tomorrow
                        AlertExpiredFinals();
                        return;
                    }
                    if (args[0] == "/ao")
                    {
                        //update audit order info from encompass
                        AuditOrderUpdateJob();
                        return;
                    }
                    if (args[0] == "/eq")
                    {
                        //update employer review queue
                        UpdateEmployerReviewQueue();
                        return;
                    }
                    if (args[0] == "/ett")
                    {
                        //update employer turn times
                        UpdateEmployerTurnTimes();
                        return;
                    }
                    if (args[0] == "/ec")
                    {
                        //cleanup old email
                        EmailCleanup();
                        return;
                    }
                    if (args[0] == "/awn")
                    {
                        //send orders to work number automatically
                        AutoWorkNumberJob();
                        return;
                    }
                    if (args[0] == "/atw")
                    {
                        //send orders to true work automatically
                        AutoTrueWorkJob();
                        return;
                    }
                    if (args[0] == "/aex")
                    {
                        //send orders to true work automatically
                        AutoExperianJob();
                        return;
                    }
                    if (args[0] == "/atv")
                    {
                        //send orders to truv automatically
                        AutoTruvJob();
                        return;
                    }
                    if (args[0] == "/atf")
                    {
                        //send truv notifications to borrowers automatically
                        AutoTruvNotificationJob();
                        return;
                    }
                    if (args[0] == "/aas")
                    {
                        //send orders to work number automatically
                        AutoAssignOrdersJob();
                        return;
                    }
                    if (args[0] == "/fu")
                    {
                        OverrideFollowUpDate();
                        return;
                    }
                    if (args[0] == "/bn")
                    {
                        BranchNotificationsJob();
                        return;
                    }
                    if (args[0] == "/goe")
                    {
                        GenerateOrderActivityEventsJob();
                        return;
                    }
                    if (args[0] == "/qtw")
                    {
                        QueryTrueWorkOpenOrdersJob();
                        return;
                    }
                    if (args[0] == "/qtv")
                    {
                        QueryTruvOpenOrdersJob();
                        return;
                    }


                }
            }
            catch (Exception ex)
            {
                FHMC.NLogWrapper.Logger Log = new FHMC.NLogWrapper.Logger("Program");
                Log.Fatal("VOEBackend Error", ex);
            }

            //dev stuff here
            //AutoTruvJob();
            //AutoTruvNotificationJob();

            //EmailCleanup();
            //VOEBackend.Email.EmailOps eop = new EmailOps();
            //eop.downloadEmails();
            //eop.resendEmails();
            //eop.testOperation();
            //RetreiveEmailJob();

            //Truv.Business.TruvTestClass tOp = new Truv.Business.TruvTestClass();
            //tOp.testOperation(5);

            //AutoTrueWorkJob();
            //RetreiveEmailJob();
            /*
             Manual testing functions after this

            */





            RetreiveEmailJob();
            var test = "";
           // VOEBackend.Xactus.Business.BaseClass.CommOps test = new VOEBackend.Xactus.Business.BaseClass.CommOps();
            //test.postRequest(
            //VOEBackend.AdvancedData.Business.ADTestClass tst = new AdvancedData.Business.ADTestClass();
            //tst.testOperation();

            //AutoAssignOrdersJob();

            //BranchNotificationsJob();
            //OOOUpdateJob();
            // VOEBackend.Encompass.EncTestClass enc = new VOEBackend.Encompass.EncTestClass();
            //enc.testgetLoanInfoREST("cdesimone", "");

            //////get access token
            //FHMC.EncompassREST.Authentication auth = new FHMC.EncompassREST.Authentication();
            //string accessToken = auth.getAccessToken();
            //FHMC.EncompassREST.Loan loan = new FHMC.EncompassREST.Loan();
            //loan.addConversationLogEmail("1022405554", "Credit Monitoring Alert", "This is the text of the email", accessToken);

            //VOEBackend.Encompass.Loans loan = new Loans();
            //loan.getLoanInfoREST("1047505251", "cdesimone", "LemonLime92");

            ////get loan GUID
            //FHMC.EncompassREST.Loan loan = new FHMC.EncompassREST.Loan();
            //string loanGUID = loan.getGUIDforLoanNumber(LoanNumber, accessToken);

            //List<string> theFields = new List<string>
            //{
            //    "CX.3145",
            //    "CX.3146",
            //    "CX.3147",
            //    "CX.3148",
            //    "CX.3149",
            //    "CX.3150"
            //};
            //AutoWorkNumberJob();
            //loan.getLoanFields("1010291875", theFields, accessToken);

            //FHMC.EncompassREST.Documents docs = new FHMC.EncompassREST.Documents();
            //List<FHMC.EncompassREST.Documents.Document> theDocs = docs.getDocumentListForLoan("162d79d6-4145-4aec-99b0-ef7047b2f470", accessToken, "1012436340");

            //string attachmentId = "2e3c4ba2-2d32-42dd-a302-f3e5cb4882a9";
            //string attURL = docs.getPageURL(loanGUID, attachmentId, 5, accessToken, LoanNumber);
            //string re = "e";

            //Encompass.Users usr = new Users();
            //usr.updateUserOOOStatus();
            //OOOUpdateJob();
            //LoanUpdateJob();

            //Equifax.Business.EquifaxTestClass tc = new Equifax.Business.EquifaxTestClass();
            //tc.testOperation();

            //Encompass.EncTestClass tc = new EncTestClass();
            //tc.testGetEmployerData();

            //TrueWork.Business.TrueWorkTestClass top = new TrueWork.Business.TrueWorkTestClass();
            //top.testOperation();

            //DocuSign.TestClass tc = new DocuSign.TestClass();
            //tc.testOperation();

            //Encompass.Users usr = new Users();
            //usr.updateUserOOOStatus();

            //Xactus.Business.XactusTestClass top = new Xactus.Business.XactusTestClass();
            //top.testOperation();
            //string responseString = File.ReadAllText(@"C:\temp\20241218074009105_1024493175-02_XactusExperianInstantOrderSubmitResponse.xml");
            //top.deserializeRequest(responseString);

            //Office365 o365 = new Office365();
            //o365.testOperation();

        }


        static void AutoReverifJob(int iDaysOffset) { 
        

            //update moved to separate job

            //create the necessary VOE order
            Orders voe = new Orders();
            voe.createReverificationOrders(iDaysOffset);

        }

        static void OrderUpdateJob()
        {

            Orders voe = new Orders();
            voe.updateOrdersFromEncompass(false);

            //update loan permissions for missing permissions
            LoanPermissions perm = new LoanPermissions();
            perm.addMissingLoanPermissions();

        }

        static void OriginatedOrderUpdateJob()
        {

            Orders voe = new Orders();
            voe.updateOrdersFromEncompass(true);

        }

        static void AuditOrderUpdateJob()
        {
            Orders voe = new Orders();
            voe.updateAuditOrdersFromEncompass();
        }

        static void RetreiveEmailJob()
        {
            //get email from voe mailbox
            VOEBackend.Email.EmailOps eOp = new VOEBackend.Email.EmailOps();
            eOp.importEmail();
        }

        static void CleanupEncDocsJob()
        {
            //cleanup old documents
            Documents doc = new Documents();
            doc.cleanupEncDoucments();
        }

        //static void PermDocUpdateJob()
        //{
        //    //download documents and user permissions into VOE system
        //    string LoanFolders = ConfigurationManager.AppSettings["LoanFolders"];
        //    string[] LoanFolderList = LoanFolders.Split(","[0]);

        //    Loans lo = new Loans();
        //    lo.DownloadDocsAndPermissionsForLoans("sdkadmin1", "Updatefield1!", LoanFolderList);
        //}

        static void CheckSubcontractedOrderStatus()
        {
            AdvancedData.Business.ITV.OrderOps ADoOp = new AdvancedData.Business.ITV.OrderOps();
            ADoOp.checkOutstandingOrderStatus();

            Equifax.Business.OrderOps EQoOp = new Equifax.Business.OrderOps();
            EQoOp.checkOutstandingOrderStatus();

        }

        static void ArchiveOrders()
        {

            Orders oop = new Orders();
            oop.archiveOrders();

        }

        static void EmailReport(int reportId)
        {

            Reports oRpt = new Reports();
            oRpt.EmailReport(reportId);

        }

        static void LoanUpdateJob()
        {
            //update encompass with changes queued in VOE System
            Loans lo = new Loans();
            lo.processEncompassUpdates();
        }

        static void OOOUpdateJob()
        {
            Encompass.Users usr = new Users();
            usr.updateUserOOOStatus();
        }

        static void AlertExpiredFinals()
        {
            Orders oop = new Orders();
            oop.alertPendingExpiredClosingTomorrow();
        }

        static void UpdateEmployerReviewQueue()
        {
            VOESystem.Data.Business.EmployerOps eOp = new VOESystem.Data.Business.EmployerOps();
            eOp.updateEmployerReviewQueue();
        }

        static void UpdateEmployerTurnTimes()
        {
            VOESystem.Data.Business.EmployerOps eOp = new VOESystem.Data.Business.EmployerOps();
            eOp.updateEmployerTurnTimes();
        }

        static void EmailCleanup()
        {

            VOEBackend.Email.EmailOps eOp = new VOEBackend.Email.EmailOps();
            eOp.deleteArchiveEmails();

        }

        static void AutoWorkNumberJob()
        {

            string TWNVendor = ConfigurationManager.AppSettings["TWNVendor"].ToString();
            if (TWNVendor == "Equifax")
            {
                Equifax.Business.OrderOps eOp = new Equifax.Business.OrderOps();
                eOp.autoSubmitOrdersToWorkNumber(false);
                eOp.autoReverifyOrdersToWorkNumber(false);
            }
            else
            {
                //do the salary key orders, if any
                Equifax.Business.OrderOps eOp = new Equifax.Business.OrderOps();
                eOp.autoSubmitOrdersToWorkNumber(true);
                eOp.autoReverifyOrdersToWorkNumber(true);

                Xactus.Business.OrderOps XOp = new Xactus.Business.OrderOps();
                XOp.autoSubmitOrdersToWorkNumber();
                XOp.autoReverifyOrdersToWorkNumber(); 

            }

        }

        static void AutoTrueWorkJob()
        {

            TrueWork.Business.OrderOps tOp = new TrueWork.Business.OrderOps();
            tOp.autoSubmitOrdersToTrueWork();
            tOp.autoReverifyOrdersToTrueWork();


        }

        static void AutoTruvJob()
        {

            Truv.Business.OrderOps tOp = new Truv.Business.OrderOps();
            tOp.autoSubmitOrdersToTruv();
            tOp.autoReverifyOrdersToTruv();
        }

        static void AutoTruvNotificationJob()
        {
            Truv.Business.OrderOps tOp = new Truv.Business.OrderOps();
            tOp.forwardTruvNotifications();
        }


        static void AutoExperianJob()
        {

            Xactus.Business.OrderOps XOp = new Xactus.Business.OrderOps();
            XOp.autoSubmitOrdersToExperian();

        }

        static void AutoAssignOrdersJob()
        {
            VOEBackend.Encompass.Orders ooE = new Orders();
            ooE.autoAssignOrders();


        }

        static void OverrideFollowUpDate()
        {
            VOEBackend.Encompass.Orders oFu = new Orders();
            oFu.overrideFollowUpDate();

        }

        static void BranchNotificationsJob()
        {
            VOEBackend.Encompass.Orders oBn = new Orders();
            oBn.branchOrderNotifications();

        }

        static void GenerateOrderActivityEventsJob()
        {

            VOEBackend.Encompass.Orders oOa = new Orders();
            oOa.generateOrderActivityEvents();

        }

        static void QueryTrueWorkOpenOrdersJob()
        {

            TrueWork.Business.OrderOps top = new TrueWork.Business.OrderOps();
            top.autoQueryOpenOrderStatus();

        }

        static void QueryTruvOpenOrdersJob()
        {

            Truv.Business.OrderOps top = new Truv.Business.OrderOps();
            top.autoQueryOpenOrderStatus();

        }

    }
       
}
