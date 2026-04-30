using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using ServiceStack.OrmLite;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;
using System.IO;

namespace VOEBackend.Encompass
{
    public class Orders : BaseClass
    {

        static string VOESystemBasePath = ConfigurationManager.AppSettings["VOESystemBasePath"].ToString();

        public void createReverificationOrders(int iDayOffset)
        {//used
            try
            {

                OrmLiteConfig.CommandTimeout = 600;

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                //get list of loans from database within a certain number of encompass business days of closing
                //without reverification orders already created - using a view

                List<AutoFinalOrderView> orders = dbConn.Select<AutoFinalOrderView>(
                    q => q.FinalOrderDate <= DateTime.Today.AddDays(iDayOffset));

                //create the reverification orders
                OrderOps op = new OrderOps();

                foreach (AutoFinalOrderView order in orders.OrderBy(q => q.LoanNumber).ToList())
                {

                    try
                    {

                        using (IDbTransaction trans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
                        {

                            if (!order.IsProspectOrderChain &&
                                order.OrderStatus !=  "Cancelled" &&
                                order.OrderStatus != "Adverse" &&
                                order.OrderStatus != "Manually Blocked" &&
                                order.OrderStatus != "Archived")
                            {
                                //create the final order
                                int finalOrderId = createFinalOrder(dbConn, order, op, trans);

                                //if the original order was marked "urgent" then we need to auto-approve final order
                                //if (order.IsUrgent)
                                //{
                                //    //get some values from the original order
                                //    OrderActivity oa = dbConn.Where<OrderActivity>(q => q.OrderRequestId == order.Id)
                                //        .OrderByDescending(r => r.Id).Take<OrderActivity>(1).FirstOrDefault();
                                //    int empStatusId = oa.EmploymentStatusId ?? 0;
                                //    int? empStatusReasonId = oa.EmploymentStatusReasonId;
                                //    DateTime? vendorDataDate = oa.VendorDataDate;

                                //    //auto approve order
                                //    autoApproveOrder(dbConn, finalOrderId, order.VerificationSpecialist, trans, empStatusId, 
                                //        empStatusReasonId, vendorDataDate, oa.IsAuditing);
                                //}

                            }

                            
                            if ((order.OrderStatus == "Pending" || order.OrderStatus == "On Hold") && order.IsUrgent == false)
                            {
                                markUrgentOrder(dbConn, order.Id, order.OrderStatus, order.VerificationSpecialist, trans);
                            }

                            trans.Commit();
                        }
                        
                    }
                    catch (Exception ex2)
                    {
                        Log.Error("Error Creating Autoreverification Order for Loan: " + order.LoanNumber, ex2);
                    }

                }

                EmailOps eops = new EmailOps();

            }
            catch (Exception ex)
            {
                Log.Error("Error Creating Autoreverification Orders", ex);
            }


        }

        public void updateOrdersFromEncompass(bool AllOriginatedOrders, string loanNumberFilter = null)
        {//used


            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();

            StatusOps sOp = new StatusOps();

            try
            {


                dbConn.Open();

                Log.Info("Starting Loan Update");

                OrderOps op = new OrderOps();

                DateTime adverseCutoffDate = DateTime.Today.AddDays(-30);

                //update the action taken date in orderrequest so next query is efficient
                string updateActiondateSQL = "UPDATE OrderRequest "
                    + " SET OrderRequest.EncActionTakenDate = emdbLoanInfoView.EncActionTakenDate"
                    + " FROM OrderRequest"
                    + " INNER JOIN emdbLoanInfoView ON emdbLoanInfoView.LoanNumber = OrderRequest.LoanNumber"
                    + " WHERE emdbLoanInfoView.EncActionTakenDate > '" + adverseCutoffDate.ToString("yyyy-MM-dd") + "' ";

                dbConn.ExecuteNonQuery(updateActiondateSQL);

                //get list of orders to update               
                string SQL = "SELECT * FROM OrderRequest "
                    + "LEFT OUTER JOIN EncLoanStatus ON OrderRequest.EncLoanStatus = EncLoanStatus.Name ";
                //+ "LEFT OUTER JOIN emdbLoanInfoView ON emdbLoanInfoView.LoanNumber = OrderRequest.LoanNumber "

                if (loanNumberFilter != null)
                {
                    SQL += " WHERE OrderRequest.LoanNumber = '" + loanNumberFilter + "'";
                }
                else
                {

                    SQL += "WHERE OrderRequest.EncLoanStatus = 'Active Loan' ";

                    //for nightly job updating all loan originated orders in order to try to catch orders in loan originated and then moved back to active
                    if (AllOriginatedOrders == true)
                    {
                        SQL += "OR (OrderRequest.EncLoanStatus = 'Loan Originated' "
                        + "AND OrderRequest.EncActionTakenDate > '" + DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd") + "') ";
                    }

                    SQL += "OR OrderRequest.EncLoanStatus is NULL "

                    + "OR (fxCurrentOrderStatusName = 'Adverse' "
                    + "AND fxLatestActivityDateTime > '" + adverseCutoffDate.ToString("yyyy-MM-dd") + "') "

                    //check all adverse status loans with action dates in the last 7 days
                    + "OR (OrderRequest.EncActionTakenDate > '" + adverseCutoffDate.ToString("yyyy-MM-dd") + "' "
                    + "AND EncLoanStatus.IsAdverse = 1) "

                    + "OR (fxCurrentOrderStatusName = 'Approved' "  //FUTURE - do not restrict by date, but by archive status
                    + "AND fxLatestActivityDateTime > '" + adverseCutoffDate.ToString("yyyy-MM-dd") + "' "
                    + "AND EncLoanStatus.IsAdverse = 1)";

                }

                Log.Info(SQL);

                List<OrderRequest> orders = dbConn.Query<OrderRequest>(SQL);
                List<string> adverseToActiveNotificationLoans = new List<string>() { };
                List<string> adverseStatusList = dbConn.Where<EncLoanStatus>(q => q.IsAdverse == true).Select<EncLoanStatus, string>(r => r.Name).ToList();
                List<ClosingDateChange> closingdateNotifications = new List<ClosingDateChange>() { };

                //for each loan, get closing date, last milestone and loan status
                foreach (OrderRequest order in orders)
                {

                    /*sdk solution did not work since the two new fields were coming up blank/missing
                     * List<LoanInfoResp> loans = lo.getLoanInfo(order.LoanNumber, null, null, LoanFolder, emSession);
                    LoanInfoResp loan = loans.FirstOrDefault();*/
                    try 
                    {
                            //Log.Info("Starting Update for Loan: " + order.LoanNumber);
                            updateOrderFromEncompass(ref dbConn, order.LoanNumber, order.FinalOrderLeadTimeDays, isNull(order.EncEmployerName, ""),
                                order.IsRIHousing, order.EncCurrentLoanFolder, order.EncInitialLoanFolder, order.EncLoanType, order.EncLoanProgram,
                                order.EncLoanStatus, ref adverseToActiveNotificationLoans, adverseStatusList, ref closingdateNotifications);
                    }
                    catch (Exception ex2)
                    {
                        Log.Error("Error Updating Loan Informtion in VOE System for loan: " + order.LoanNumber, ex2);   
                    }

                }

                EmailOps eops = new EmailOps();
                OrderOps oops = new OrderOps();
                BaseDataOps bops = new BaseDataOps();

                //notify final order specialists of closing date changes
                //closing date notifications disabled 4/17/2017
                //closing date notifications reenabled 3/28/2020
                DateTime nextBusinessDay = bops.CalcBusinessDate(dbConn, DateTime.Today, 1);

                foreach (ClosingDateChange change in closingdateNotifications)
                {
                    try
                    {

                        //check & update the all finals complete flag
                        oops.updateAllFinalVOEsCompleteFlag(dbConn, change.LoanNumber);

                        if (isActiveFinalOrder(dbConn, change.LoanNumber))
                        {
                            Dictionary<string, string> inlineData = new Dictionary<string, string>();
                            inlineData.Add("#oldclosingdate#", change.OldClosingDate.ToString("MM/dd/yyyy"));
                            inlineData.Add("#newclosingdate#", change.NewClosingDate.ToString("MM/dd/yyyy"));

                            DateTime notificationWindow = bops.CalcBusinessDate(dbConn, change.OldClosingDate, 3);

                            //if date moves back or if date move forward is within 3 business days
                            if (change.NewClosingDate < change.OldClosingDate 
                                || notificationWindow <= change.NewClosingDate
                                || (change.OldClosingDate == DateTime.Parse("1900-01-01") && change.NewClosingDate == DateTime.Today)
                                || (change.OldClosingDate == DateTime.Today && change.NewClosingDate == nextBusinessDay)) { 
                                eops.sendTemplateEmail(dbConn, "Closing Date Changed Internal Notification", null, "", false, null, null, false, null, change.LoanNumber, inlineData);
                            }
                        }

 
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error Notifying of Closing Date Change for Loan: " + change.LoanNumber, ex);
                    }

                }


                //notify specialists that adverse loan has moved to active
                List<string> exceptStatuses = new List<string> { "Approved", "Cancelled" };
                foreach (string loanNumber in adverseToActiveNotificationLoans)
                {
                    try
                    {
                        //move all non-approved, non-cancelled, non-corrected orders to pending
                        moveOrdersForLoanToStatus(dbConn, loanNumber, 2, 8, exceptStatuses, "Loan Moved from Adverse to Active");
                        eops.sendTemplateEmail(dbConn, "Loan Moved from Adverse to Active", null, "", false, false, null, false, null, loanNumber);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error Notifying of Loan Moving from Adverse to Active for Loan: " + loanNumber, ex);
                    }

                }


                //update order status to Adverse for open order for loans now in an adverse status                
                List<OpenAdverseOrderView> adverseorders = dbConn.Select<OpenAdverseOrderView>();

                foreach (OpenAdverseOrderView adverseorder in adverseorders)
                {
                    try
                    {
                        var activityRequest = new NewActivityReq
                        {
                            ActivityNote = "Automatic VOE System Update",
                            CurrOrderStatusId = 7,   //adverse status
                            EmployerEmail = adverseorder.EmployerEmail,
                            EmployerFax = adverseorder.EmployerFax,
                            EmployerName = adverseorder.EmployerName,
                            EmployerPhone = adverseorder.EmployerPhone,
                            EmploymentEndDate = adverseorder.EmploymentEndDate,
                            EmploymentJobTitle = adverseorder.EmploymentJobTitle,
                            EmploymentOutlookId = adverseorder.EmploymentOutlookId,
                            EmploymentStartDate = adverseorder.EmploymentStartDate,
                            EmploymentStatusId = adverseorder.EmploymentStatusId,
                            EmploymentStatusReasonId = adverseorder.EmploymentStatusReasonId,
                            PrevOrderStatusId = adverseorder.CurrOrderStatusId,
                            OrderRequestId = adverseorder.OrderRequestId,
                            VendorCost = adverseorder.VendorCost,
                            VendorId = adverseorder.VendorId,
                            VerifiedBy = adverseorder.VerifiedBy,
                            VerifiedVia = adverseorder.VerifiedVia,
                            FollowupDate = adverseorder.FollowupDate,
                            StickyNotes = adverseorder.StickyNotes,
                            OrderFollowupTypeId = adverseorder.OrderFollowupTypeId
                        };

                        op.saveOrderActivity(dbConn, activityRequest, "voesystem","", null);

                    }
                    catch (Exception ex3)
                    {
                        Log.Error("Error Updating Adverse Loan Status in VOE System for OrderRequestId: " + adverseorder.OrderRequestId, ex3);
                    }
                }



                //cancel final orders with no scheduled to close date or sched closing date yesterday or before        
                //2016-11-29 no longer cancel final orders so all activity can be consolidated into one final order
                //List<OpenFinalOrderView> finalorders = dbConn.Select<OpenFinalOrderView>(q => q.EncSchedClosingDate == DateTime.Parse("1900-01-01")
                //    || q.EncSchedClosingDate < DateTime.Today);

                //foreach (OpenFinalOrderView finalorder in finalorders)
                //{
                //    try
                //    {
                //        var activityRequest = new NewActivityReq
                //        {
                //            ActivityNote = "Automatic VOE System Update - Cancelled due to blank or expired scheduled closing date",
                //            CurrOrderStatusId = 6,   //cancelled
                //            EmployerEmail = finalorder.EmployerEmail,
                //            EmployerFax = finalorder.EmployerFax,
                //            EmployerName = finalorder.EmployerName,
                //            EmployerPhone = finalorder.EmployerPhone,
                //            EmploymentEndDate = finalorder.EmploymentEndDate,
                //            EmploymentJobTitle = finalorder.EmploymentJobTitle,
                //            EmploymentOutlookId = finalorder.EmploymentOutlookId,
                //            EmploymentStartDate = finalorder.EmploymentStartDate,
                //            EmploymentStatusId = finalorder.EmploymentStatusId,
                //            PrevOrderStatusId = finalorder.CurrOrderStatusId,
                //            OrderRequestId = finalorder.OrderRequestId,
                //            VendorCost = finalorder.VendorCost,
                //            VendorId = finalorder.VendorId,
                //            VerifiedBy = finalorder.VerifiedBy,
                //            VerifiedVia = finalorder.VerifiedVia,
                //            StickyNotes = finalorder.StickyNotes
                //        };

                //        op.saveOrderActivity(dbConn, activityRequest, "voesystem", null, null, null, null);

                //    }
                //    catch (Exception ex4)
                //    {
                //        Log.Error("Error Updating Cancelled Loan Status in VOE System for OrderRequestId: " + finalorder.OrderRequestId, ex4);
                //    }
                //}

                //send email alerts for alert conditions (specifically craeted for the work number orders that are not approved within 2 dys of closing)
                List<AlertConditionSP> alertChanges = dbConn.SqlList<AlertConditionSP>("EXEC usp_UpdateAlertConditions");

                foreach ( AlertConditionSP alertChange in alertChanges )
                {

                    if ( alertChange.NewState )
                    {
                        //this alert just triggered, so we should send an email
                        int RequestTypeId = dbConn.Where<OrderRequest>(q => q.Id == alertChange.OrderRequestId).FirstOrDefault().RequestTypeId;

                        if (alertChange.AlertConditionType == "Vendor Final")
                        {
                            eops.sendTemplateEmail(dbConn, "Unapproved Work Number Order Notification", alertChange.OrderRequestId, "", false, false, RequestTypeId, false);
                        }
                        else if (alertChange.AlertConditionType == "Closing Tomorrow Final")
                        {
                            eops.sendTemplateEmail(dbConn, "Closing Tomorrow Order Notification", alertChange.OrderRequestId, "", false, false, RequestTypeId, false);
                        }
                    }

                }

                sOp.AddJobLog(dbConn, VOESystem.Data.DTO.JobType.EncOrderUpdate, false);

            }
            catch (Exception ex)
            {
                Log.Error("Error Updating Loan Informtion in VOE System", ex);
                sOp.AddJobLog(dbConn, VOESystem.Data.DTO.JobType.EncOrderUpdate, true);
                throw ex;
            }

           

            Log.Info("Ending Loan Update");

        }

        public bool isActiveFinalOrder(IDbConnection dbConn, string loanNumber)
        {

            bool retVal = false;

            int iOpenFinalOrderCount = dbConn.Where<OrderRequest>(q => q.LoanNumber == loanNumber
                && q.RequestTypeId == 3 && q.fxCurrentOrderStatusName != "Cancelled").ToList().Count;

            if (iOpenFinalOrderCount > 0)
            {
                retVal = true;
            }

            return retVal;

        }

        public class ClosingDateChange
        {
            public string LoanNumber { get; set; }
            public DateTime OldClosingDate { get; set; }
            public DateTime NewClosingDate { get; set; }
        }

        public void updateAuditOrdersFromEncompass()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            Log.Info("Starting Audit Order Update");

            List<AuditOrderView> auditOrders = dbConn.Select<AuditOrderView>()
                .Where<AuditOrderView>(q => q.IsAuditing == true)
                .ToList();

            EmailOps eOp = new EmailOps();
            OrderOps oOp = new OrderOps();

            List<ClosingDateChange> closingdateNotifications = new List<ClosingDateChange>() { };

            foreach (AuditOrderView auditOrder in auditOrders)
            {
                
                try
                {

                    if (auditOrder.VOESchedClosingDate != auditOrder.EncSchedClosingDate ||
                        auditOrder.VOESchedClosingTime != auditOrder.EncSchedClosingTime ||
                        auditOrder.VOESchedClosingTimeAMPM != auditOrder.EncSchedClosingTimeAMPM)
                    {

                        //there has been an update so we need to kick off template email and also update system
                        //get old date/time
                        string fromData = "FROM ";
                        if (auditOrder.VOESchedClosingDate != null)
                        {
                            fromData += ((DateTime)auditOrder.VOESchedClosingDate).ToString("MM/dd/yyyy") + " "
                                + (auditOrder.VOESchedClosingTime ?? "")
                                + (auditOrder.VOESchedClosingTimeAMPM ?? "");
                        }
                        else
                        {
                            fromData += "(empty)";
                        }

                        string toData = "TO ";
                        if (auditOrder.EncSchedClosingDate != null)
                        {
                            toData += ((DateTime)auditOrder.EncSchedClosingDate).ToString("MM/dd/yyyy") + " "
                                + (auditOrder.EncSchedClosingTime ?? " ")
                                + (auditOrder.EncSchedClosingTimeAMPM ?? " ");
                        }
                        else
                        {
                            toData += "(empty)";
                        }

                        Dictionary<string, string> inlineData = new Dictionary<string, string>() { };
                        inlineData.Add("#modifiedfields#", fromData + " " + toData);
                        
                        //update system -> create new order activity, update scheduled closing date
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, auditOrder.OrderRequestId, "voesystem", null);
                        OrderRequest ord = dbConn.Where<OrderRequest>(q => q.Id == auditOrder.OrderRequestId).FirstOrDefault();

                        using (IDbTransaction trans = dbConn.BeginTransaction())
                        {
                            //add order activity
                            oa.ActivityNote = "Audit Order Modification: Scheduled Closing Date/Time Changed " + fromData + " " + toData;
                            oa.IsAuditing = true;
                            oa.EncSchedClosingTime = auditOrder.EncSchedClosingTime;
                            oa.EncSchedClosingTimeAMPM = auditOrder.EncSchedClosingTimeAMPM;

                            dbConn.Insert<OrderActivity>(oa);

                            //un-dismiss audit
                            dbConn.UpdateOnly(new OrderRequest { IsAuditDismissed = false }, q => q.IsAuditDismissed, r => r.Id == auditOrder.OrderRequestId);

                            //run an update of the other data points, just to make sure that logic runs
                            List<string> dummy = new List<string>() { };
                            updateOrderFromEncompass(ref dbConn, ord.LoanNumber, ord.FinalOrderLeadTimeDays, ord.EncEmployerName, 
                                ord.IsRIHousing, ord.EncCurrentLoanFolder, ord.EncInitialLoanFolder, ord.EncLoanType, ord.EncLoanProgram,
                                ord.EncLoanStatus, ref dummy, null, ref closingdateNotifications);

                            trans.Commit();
                        }

                        //send email
                        eOp.sendTemplateEmail(dbConn, "Audit Order Change", auditOrder.OrderRequestId, null, false, true, ord.RequestTypeId, false, null, null, inlineData, false);
                    }

                    Log.Info("Audit Order Updated: " + auditOrder.OrderRequestId);

                }
                catch (Exception ex)
                {

                    Log.Error("Error Updating Loan Informtion in VOE System for Audit Order: " + auditOrder.OrderRequestId, ex);

                }

            }

            Log.Info("Ending Audit Order Update");

        }

        public void archiveOrders()
        {
            try
            {

                OrmLiteConfig.CommandTimeout = 600;

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

                using (IDbConnection dbConn = factory.CreateDbConnection())
                {
                    dbConn.Open();

                    //get order status id
                    int ArchivedOrderStatusId = dbConn.Select<OrderStatus>(q => q.Name == "Archived").FirstOrDefault().Id;

                    //get list of loans from database to be archived
                    List<int> orderIds = dbConn.Select<OrderArchiveView>().Select(q => q.OrderRequestId).ToList();

                    OrderOps oop = new OrderOps();

                    foreach (int orderId in orderIds)
                    {

                        try
                        {
                            OrderActivity activity = oop.getOrderActvityForNewActivty(dbConn, orderId, "voesystem", false);

                            activity.PrevOrderStatusId = activity.CurrOrderStatusId;
                            activity.PrevOrderSubStatusId = activity.CurrOrderSubStatusId;
                            activity.CurrOrderStatusId = ArchivedOrderStatusId;
                            activity.CurrOrderSubStatusId = null;
                            activity.ActivityNote = "Automated Archive of Order";

                            dbConn.Insert<OrderActivity>(activity);

                            Log.Info("Order Archived Id = " + orderId.ToString());
                        }
                        catch (Exception ex2)
                        {
                            Log.Error("Error Archiving Order Id: " + orderId.ToString(), ex2);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                Log.Error("Error Archiving Orders", ex);
            }


        }

        public void bulkCancelOrders()
        {

            string inputFile = @"E:\FileTemp\OrderInputFile.txt";
            try
            {


                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                //read from input file and update each loan
                using (StreamReader sr = new StreamReader(inputFile))
                {
                    while (sr.Peek() >= 0)
                    {
                        string orderRequestId = sr.ReadLine();
                        try
                        {
                            Log.Info("Cancelling Order: ;" + orderRequestId);
                            cancelOrder(dbConn, Int32.Parse(orderRequestId));
                        }
                        catch (Exception ex)
                        {
                            Log.Error("Failed to Cancel Orer: ;" + orderRequestId, ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Bulk Cancel Order Error: ", ex);
            }
        }

        public void markUrgentOrder(IDbConnection dbConn, int OrderRequestId, string OrderStatus, string VerificationSpecialist, IDbTransaction trans)
        {

            //mark this order as urgent
            dbConn.UpdateOnly(new OrderRequest { IsUrgent = true }, q => q.IsUrgent, r => r.Id == OrderRequestId);

            if (OrderStatus == "Pending")
            {
                //create toast alert
                ToastAlertOps to = new ToastAlertOps();
                to.createAlert(dbConn, null, VerificationSpecialist, OrderRequestId, null, "Pending Order to Close Soon");
            }
        
            //send email to turn on red bell
            int RequestTypeId = dbConn.Where<OrderRequest>(q => q.Id == OrderRequestId).FirstOrDefault().RequestTypeId;

            EmailOps eo = new EmailOps();
            eo.sendTemplateEmail(dbConn, "Order Escalated to Urgent", OrderRequestId, null, false, false, RequestTypeId, false);

        }

        public int createFinalOrder(IDbConnection dbConn, AutoFinalOrderView order, OrderOps op, IDbTransaction trans)
        {
            int retVal = 0;

            //check if approved on basis of day 1 order
            if (order.VendorName == "Work#" && order.EquifaxOrderStatus == null && order.XactusOrderStatus == null)
            {
                //need to get Equifax information from Day1 order
                Dictionary<string, object> prms = new Dictionary<string, object> { };
                prms.Add("OrderRequestId", order.Id);

                List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);

                if (day1OrderId.Count == 1) {
                    OrderRequest day1Order = dbConn.Where<OrderRequest>(q => q.Id == day1OrderId.FirstOrDefault()).FirstOrDefault();
                    order.EquifaxEmployerCode = day1Order.EquifaxEmployerCode;
                    order.EquifaxFirstName = day1Order.EquifaxFirstName;
                    order.EquifaxLastName = day1Order.EquifaxLastName;
                    order.EquifaxOrderNumber = day1Order.EquifaxOrderNumber;
                    order.EquifaxOrderStatus = day1Order.EquifaxOrderStatus;
                    order.EquifaxOrderType = day1Order.EquifaxOrderType;
                    order.XactusEmployerCode = day1Order.XactusEmployerCode;
                    order.XactusOrderType = day1Order.XactusOrderType;
                    order.XactusOrderStatus = day1Order.XactusOrderStatus;
                    order.XactusOrderNumber = day1Order.XactusOrderNumber;
                    order.XactusFirstName = day1Order.XactusFirstName;
                    order.XactusLastName = day1Order.XactusLastName;
                }

            }
            else if (isNull(order.VendorName, "").Contains("TrueWork") && order.TrueWorkOrderStatus == null)
            {
                //need to get TrueWork information from Day1 order
                Dictionary<string, object> prms = new Dictionary<string, object> { };
                prms.Add("OrderRequestId", order.Id);

                List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);

                if (day1OrderId.Count == 1)
                {
                    OrderRequest day1Order = dbConn.Where<OrderRequest>(q => q.Id == day1OrderId.FirstOrDefault()).FirstOrDefault();
                    order.TrueWorkOrderNumber = day1Order.TrueWorkOrderNumber;
                    order.TrueWorkOrderStatus = day1Order.TrueWorkOrderStatus;
                    order.TrueWorkOrderType = day1Order.TrueWorkOrderType;
                }
            }

            NewOrderReq reveriforder = new NewOrderReq
            {
                BorrowerFullName = order.BorrowerFullName,
                BorrowerDOB = order.BorrowerDOB.ToString("yyyy-MM-dd"),
                BorrowerSSN = order.BorrowerSSN,
                BorrowerEmail = order.BorrowerEmail,
                BorrowerGender = order.BorrowerGender,
                BorrowerAddress = order.BorrowerAddress,
                LoanNumber = order.LoanNumber,
                RequestTypeId = 3,
                SchedClosingDate = order.EncSchedClosingDate.ToString("yyyy-MM-dd"),
                CPAName = order.CPAName,
                CPAPhone = order.CPAPhone,
                CPAEmail = order.CPAEmail,
                EncLastMilestone = order.EncLastMilestone,
                EncLoanStatus = order.EncLoanStatus,
                EncCurrentLoanFolder = order.EncInitialLoanFolder, ///this is to preserve the initital loan folder for the loan
                EncLoanOfficerName = order.EncLoanOfficerName,
                EncProcessorName = order.EncProcessorName,
                EncLoanAssistantName = order.EncLoanAssistantName,
                EncBranchAdminName = order.EncBranchAdminName,
                IsNonBorrower = order.IsNonBorrower,
                BorrowerHomePhone = order.BorrowerHomePhone,
                BorrowerMobilePhone = order.BorrowerMobilePhone,
                IsRIHousing = order.IsRIHousing,
                Status1099 = order.Status1099,
                EncLoanType = order.EncLoanType,
                OrgId = order.OrgCode, 
                IsCoBorrower = order.IsCoBorrower,
                BorrowerPairIndex = order.BorrowerPairIndex,
                EncEmploymentSelfFlag = order.EncEmploymentSelfFlag,
            };

            //add one employer
            List<VOESystem.Data.DTO.Employer> emps = new List<VOESystem.Data.DTO.Employer>() { };

            VOESystem.Data.DTO.Employer emp = new VOESystem.Data.DTO.Employer
            {
                VerificationTypeId = 1,  //all autoreverifs are verbals
                EncEmployerName = order.EncEmployerName,
                EncEmployerPhone = order.EncEmployerPhone,
                EncEmployerAddress = order.EncEmployerAddress,
                EncEmploymentTitle = order.EncEmploymentTitle,
                EncYearsOnJob = order.EncYearsOnJob.ToString(),
                EncMonthsOnJob = order.EncMonthsOnJob.ToString(),
                EncYearsInLineOfWork = order.EncYearsInLineOfWork.ToString(),
                EncEmployerFax = order.EncEmployerFax,
                EncEmployerEmail = order.EncEmployerEmail,
                EncStartDate = order.EncStartDate,
                EncTerminationDate = order.EncTerminationDate,
                EncEmploymentStatus = order.EncEmploymentStatus,
                VendorId = order.VendorId,
                RequestNote = "This reverification order was created by the VOE automated system.",
                MilitaryStatus = order.MilitaryStatus,
                DoVerify = true
            };

            //add note to the order
            if (order.OrderStatusReason == "Paystub or Asset Waiver")
            {
                emp.RequestNote = emp.RequestNote + "\r\n\r\n" + "NOTE: THE INITIAL ORDER HAS A PAYSTUB OR ASSET WAIVER";
            }

            //add original note to order, minus branch override
            emp.RequestNote = emp.RequestNote + "\r\n\r\n" + order.RequestNote;

            BranchOverride branchOverride = dbConn.Where<BranchOverride>(q => q.OrgCode == order.OrgCode && q.OrderInstructions != null).FirstOrDefault();
            if (branchOverride != null)
            {
                emp.RequestNote = emp.RequestNote.Replace(branchOverride.OrderInstructions, "");
            }

            emps.Add(emp);
            reveriforder.Employers = emps;

            NewOrderResp resp = op.saveRequest(dbConn, reveriforder, order.OrderRequestor, null, trans, "");

            //only one employer in, only one request id out
            int reqId = resp.OrderRequestIds.FirstOrDefault();
            retVal = reqId; //returning the order id of the newly created order

            OrderRequest ordOld = dbConn.Where<OrderRequest>(q => q.Id == order.Id).FirstOrDefault();
            ordOld.ReverificationOrderRequestId = reqId;
            dbConn.UpdateOnly(ordOld,
                q => new { q.ReverificationOrderRequestId }, r => r.Id == ordOld.Id);

            //Update Fields not included in NewOrderRequest
            OrderRequest ordNew = dbConn.Where<OrderRequest>(q => q.Id == reqId).FirstOrDefault();
            ordNew.EquifaxOrderStatus = order.EquifaxOrderStatus;
            ordNew.EquifaxOrderNumber = order.EquifaxOrderNumber;
            ordNew.EquifaxFirstName = order.EquifaxFirstName;
            ordNew.EquifaxLastName = order.EquifaxLastName;
            ordNew.EquifaxOrderType = order.EquifaxOrderType;
            ordNew.EquifaxEmployerCode = order.EquifaxEmployerCode;
            ordNew.TrueWorkOrderNumber = order.TrueWorkOrderNumber;
            ordNew.TrueWorkOrderStatus = order.TrueWorkOrderStatus;
            ordNew.TrueWorkOrderType = order.TrueWorkOrderType;
            ordNew.XactusEmployerCode = order.XactusEmployerCode;
            ordNew.XactusOrderType = order.XactusOrderType;
            ordNew.XactusOrderStatus = order.XactusOrderStatus;
            ordNew.XactusOrderNumber = order.XactusOrderNumber;
            ordNew.XactusFirstName = order.XactusFirstName;
            ordNew.XactusLastName = order.XactusLastName;
            ordNew.TruvOrderNumber = order.TruvOrderNumber;
            ordNew.TruvOrderStatus = order.TruvOrderStatus;

            dbConn.UpdateOnly(ordNew,
                q => new { q.EquifaxOrderStatus,
                    q.EquifaxOrderNumber,
                    q.EquifaxFirstName,
                    q.EquifaxLastName,
                    q.EquifaxOrderType,
                    q.EquifaxEmployerCode,
                    q.TrueWorkOrderNumber,
                    q.TrueWorkOrderStatus,
                    q.TrueWorkOrderType,
                    q.XactusEmployerCode,
                    q.XactusOrderType,
                    q.XactusOrderStatus,
                    q.XactusOrderNumber,
                    q.XactusFirstName,
                    q.XactusLastName,
                    q.TruvOrderNumber,
                    q.TruvOrderStatus
                }, r => r.Id == ordNew.Id);

            //update order activity fields from original request
            OrderActivity oaNew = dbConn.Select<OrderActivity>(q => q.OrderRequestId == reqId)
                .OrderByDescending(ob => ob.Id).FirstOrDefault();

            OrderActivity oaOld = dbConn.Select<OrderActivity>(q => q.OrderRequestId == order.Id)
                .OrderByDescending(ob => ob.Id).FirstOrDefault();

            string branchNotes = String.Empty;
            BranchOverride bo = dbConn.Where<BranchOverride>(q => q.OrgCode == order.OrgCode).FirstOrDefault();
            if ( bo != null ) { branchNotes = "\r\n\r\n" + bo.OrderInstructions; };

            oaNew.EmployerEmail = oaOld.EmployerEmail;
            oaNew.EmployerFax = oaOld.EmployerFax;
            oaNew.EmployerName = oaOld.EmployerName;
            oaNew.EmployerPhone = oaOld.EmployerPhone;
            oaNew.EmploymentEndDate = oaOld.EmploymentEndDate;
            oaNew.EmploymentJobTitle = oaOld.EmploymentJobTitle;
            oaNew.EmploymentOutlookId = oaOld.EmploymentOutlookId;
            oaNew.EmploymentStartDate = oaOld.EmploymentStartDate;
            oaNew.VerifiedBy = oaOld.VerifiedBy;
            oaNew.VerifiedVia = oaOld.VerifiedVia;
            oaNew.VerifiedByTitle = oaOld.VerifiedByTitle;
            oaNew.VerifiedByPhone = oaOld.VerifiedByPhone;
            oaNew.VerifiedViaShortURL = oaOld.VerifiedViaShortURL;
            oaNew.VerifiedByEmailAddress = oaOld.VerifiedByEmailAddress;
            oaNew.VendorId = oaOld.VendorId;
            oaNew.StickyNotes = isNull(oaOld.StickyNotes, "") + branchNotes;
            oaNew.PanicMode = false;
            oaNew.SelfEmplDataDate = oaOld.SelfEmplDataDate;
            oaNew.VendorReferenceNum = oaOld.VendorReferenceNum;

            dbConn.Update<OrderActivity>(oaNew);

            Log.Info("Autoreverification Order Created: " + ordNew.LoanNumber + "-" + Int32.Parse(ordNew.OrderSuffix).ToString("00"));

            VOESystem.Data.Business.EmailOps eop = new EmailOps();
            if (order.OrderStatusReason == "Paystub or Asset Waiver")
            {
                eop.sendTemplateEmail(dbConn, "Paystub or Asset Waiver Action Required", reqId, "", oaNew.IsAuditing, false, reveriforder.RequestTypeId, false, trans);

                markUrgentOrder(dbConn, reqId, "", "", null);
            }

            if (order.OrderStatus == "Pending" || order.OrderStatus == "On Hold")
            {
                eop.sendTemplateEmail(dbConn, "Final Generated on Pending Order", reqId, "", oaNew.IsAuditing, false, reveriforder.RequestTypeId, false, trans);

                VOESystem.Data.Business.ToastAlertOps tOp = new ToastAlertOps();
                tOp.createAlert(dbConn, "Final Created on Pending Order", order.VerificationSpecialist, order.Id, null, "Final Created for Pending Order");

            }

            //if this was a work# order, then put into Autowork# queue 
            if ((order.VendorName == "Work#" && order.TruvOrderStatus != "Success")
                || (isNull(order.VendorName, "").Contains("TrueWork") && order.TruvOrderStatus != "Success")
                || isNull(order.VendorName, "").Contains("Truv"))
            {

                OrderActivity vendorOA = op.getOrderActvityForNewActivty(dbConn, reqId, "voesystem", false);
                vendorOA.PrevOrderStatusId = oaNew.CurrOrderStatusId;
                vendorOA.CurrOrderStatusId = 24;
                vendorOA.PrevOrderSubStatusId = oaNew.CurrOrderSubStatusId;
                vendorOA.CurrOrderSubStatusId = 30;

                vendorOA.ActivityNote = "Order Moved to AutoWork# Final Reverify Status";
                dbConn.Insert<OrderActivity>(vendorOA);

            }
            else if ((order.VendorName == "Work#" && order.TruvOrderStatus == "Success")
                || (isNull(order.VendorName, "").Contains("TrueWork") && order.TruvOrderStatus == "Success"))
            {
                OrderActivity vendorOA = op.getOrderActvityForNewActivty(dbConn, reqId, "voesystem", false);

                vendorOA.ActivityNote = "Truv Connection Identified- Other TPV Utilized";
                vendorOA.StickyNotes += "\r\n\r\nTruv Connection Identified- Other TPV Utilized";
                dbConn.Insert<OrderActivity>(vendorOA);

            }


            return retVal;
        }
        
        public void cancelOrder(IDbConnection dbConn, int OrderRequestId)
        {

            VOESystem.Data.Business.OrderOps oOp = new OrderOps();
            VOESystem.Data.DBSchema.OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, OrderRequestId, "cdesimone", false);

            NewActivityReq request = new NewActivityReq {
                OrderRequestId = OrderRequestId,
                PrevOrderStatusId = oa.CurrOrderStatusId,
                CurrOrderStatusId = 6,  //cancelled status
                ActivityNote = "Cancelled in order to remove duplicate order from Final Pipeline View",
                EmploymentStatusId = oa.EmploymentStatusId ?? 0,
                EmploymentStatusReasonId = oa.EmploymentStatusReasonId,
                EmploymentOutlookId = oa.EmploymentOutlookId ?? 0,
                EmploymentStartDate = oa.EmploymentStartDate,
                EmploymentEndDate = oa.EmploymentEndDate,
                EmploymentJobTitle = oa.EmploymentJobTitle,
                EmployerFax = oa.EmployerFax,
                EmployerEmail = oa.EmployerEmail,
                EmployerName = oa.EmployerName,
                EmployerPhone = oa.EmployerPhone,
                VerifiedBy = oa.VerifiedBy,
                VerifiedVia = oa.VerifiedVia,
                VendorId = oa.VendorId,
                VendorCost = oa.VendorCost,
                PrevOrderSubStatusId = oa.CurrOrderSubStatusId,
                CurrOrderSubStatusId = null,
                FollowupDate = oa.FollowupDate,
                VerifiedByTitle = oa.VerifiedByTitle,
                VerifiedByPhone = oa.VerifiedByPhone,
                PanicMode = oa.PanicMode,
                StickyNotes = oa.StickyNotes,
                OrderFollowupTypeId = oa.OrderFollowupTypeId,
                IsReApproval = false
            };

            oOp.saveOrderActivity(dbConn, request, "voesystem","", null);

        }

        public void autoApproveOrder(IDbConnection dbConn, int OrderRequestId, string origSpecialistUserName, IDbTransaction trans,
            int employmentStatusId, int? employmentStatusReasonId, DateTime? vendorDataDate, bool IsAuditing, int RequestTypeId)
        {
            
            OrderOps op = new OrderOps();
            RequestUser ru;

            //get activity note form original certification
            string certNote = dbConn.Where<LatestActivityView>(q => q.OrderRequestId == OrderRequestId).FirstOrDefault().ActivityNote;

            //reassign to original specialist
            string newSpecialist = "voesystem";

            if (op.canUserBeAssignedOrders(dbConn, origSpecialistUserName, "voesystem", false, false, RequestTypeId))
            {
                ru = new RequestUser
                {
                    AssignmentDateTime = DateTime.Now,
                    OrderRequestId = OrderRequestId,
                    UserName = origSpecialistUserName
                };

                dbConn.Insert<RequestUser>(ru);

                OrderActivity oaAssign = op.getOrderActvityForNewActivty(dbConn, OrderRequestId, "voesystem", IsAuditing);

                oaAssign.ActivityNote = "Order Assigned to Original VOES";
                dbConn.Insert<OrderActivity>(oaAssign);

                newSpecialist = origSpecialistUserName;

            }

            //create autoapproval note
            OrderActivity oaAA = op.getOrderActvityForNewActivty(dbConn, OrderRequestId, "voesystem", IsAuditing);

            //generate cert
            NewActivityReq certReq = new NewActivityReq
            {
                OrderRequestId = OrderRequestId,
                PrevOrderStatusId = oaAA.CurrOrderStatusId,
                CurrOrderStatusId = 3,
                PrevOrderSubStatusId = oaAA.CurrOrderSubStatusId,
                CurrOrderSubStatusId = 0,
                ActivityNote = certNote,
                EmploymentStatusId = employmentStatusId,
                EmploymentStatusReasonId = employmentStatusReasonId,
                EmploymentOutlookId = oaAA.EmploymentOutlookId ?? 0,
                EmploymentStartDate = oaAA.EmploymentStartDate,
                EmploymentEndDate = oaAA.EmploymentEndDate,
                EmploymentJobTitle = oaAA.EmploymentJobTitle,
                EmployerFax = oaAA.EmployerFax,
                EmployerEmail = oaAA.EmployerEmail,
                EmployerName = oaAA.EmployerName,
                EmployerPhone = oaAA.EmployerPhone,
                VerifiedBy = oaAA.VerifiedBy,
                VerifiedVia = oaAA.VerifiedVia,
                VendorId = oaAA.VendorId,
                VendorCost = oaAA.VendorCost,
                FollowupDate = oaAA.FollowupDate,
                VerifiedByTitle = oaAA.VerifiedByTitle,
                VerifiedByPhone = oaAA.VerifiedByPhone,
                PanicMode = oaAA.PanicMode,
                StickyNotes = oaAA.StickyNotes,
                OrderFollowupTypeId = oaAA.OrderFollowupTypeId,
                IsReApproval = false,
                VendorDataDate = vendorDataDate,
                SelfEmplDataDate = oaAA.SelfEmplDataDate,
                IsRevision = oaAA.IsRevision,
                IsAuditing = oaAA.IsAuditing,
                OrderStatusReasonId = oaAA.OrderStatusReasonId
            };

            op.saveOrderActivity(dbConn, certReq, newSpecialist, VOESystemBasePath, "", trans);

            //we don't need this since this is happening already within the certification code
            //UPDATE - well I thought it was happening in the regular cert code but it is not...
            string finalsUserName = op.getFinalsSpecialist(dbConn);

            ru = new RequestUser
            {
                AssignmentDateTime = DateTime.Now,
                OrderRequestId = OrderRequestId,
                UserName = finalsUserName
            };

            dbConn.Insert<RequestUser>(ru);

            OrderActivity oaReAssign = op.getOrderActvityForNewActivty(dbConn, OrderRequestId, "voesystem", IsAuditing);

            oaReAssign.ActivityNote = "Order Resssigned to Finals Specialist";
            dbConn.Insert<OrderActivity>(oaReAssign);

        }

        public void alertPendingExpiredClosingTomorrow()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {

                dbConn.Open();

                //send email to turn on red bell
                EmailOps eo = new EmailOps();
                string SQL = "SELECT OrderRequestId FROM dbo.fn_RptClosingTomorrowFinalsData()";

                List<int> orders = dbConn.SqlList<int>(SQL);

                foreach (int orderId in orders)
                {
                    int RequestTypeId = dbConn.Where<OrderRequest>(q => q.Id == orderId).FirstOrDefault().RequestTypeId;
                    eo.sendTemplateEmail(dbConn, "Expired/Pending Order Closing Tomorrow", orderId, null, false, false, RequestTypeId, false);
                }

            }
        }

        public void testEncompassFunctions()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            using (IDbTransaction trans = dbConn.OpenTransaction(IsolationLevel.ReadCommitted))
            {
                int RequestTypeId = dbConn.Where<OrderRequest>(q => q.Id == 78496).FirstOrDefault().RequestTypeId;

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                eo.sendTemplateEmail(dbConn, "Order Escalated to Urgent", 78496, null, false, false, RequestTypeId, false, trans);

                trans.Commit();

            }

        }

        public void updateOrderFromEncompass(ref IDbConnection dbConn, string LoanNumber, int FinalOrderLeadTimeDays, string EncEmployerName, 
            bool OldIsRIHousing, string CurrentLoanFolder, string InitialLoanFolder, string CurrentLoanType, string CurrentLoanProgram,
            string CurrentLoanStatus, ref List<string> adverseToActiveLoans, List<string> adverseStatusList, ref List<ClosingDateChange> closingdateNotifications)
        {

           
                emdbLoanInfoView loan = dbConn.SingleWhere<emdbLoanInfoView>("LoanNumber", LoanNumber);

                if (loan != null)
                {
                    DateTime SchedClosingDate = loan.EncSchedClosingDate;
                    string LastMilestone = loan.EncLastMilestone;
                    string LoanStatus = loan.EncLoanStatus;
                    string LoanCloserName = loan.EncCloserName;
                    string LoanProcessorName = loan.EncProcessorName;
                    string LoanClosingCoordName = loan.EncClosingCoordName;
                    string LoanAssistantName = loan.EncLoanAssistantName;
                    string LoanBranchAdminName = loan.EncBranchAdminName;
                    string LoanUnderwriterName = loan.EncUnderwriterName;
                    DateTime LoanFundingDate = loan.EncFundingDate;
                    bool NewIsRIHousing = loan.IsRIHousing;
                    string LoanFolder = loan.EncLoanFolder;
                    string LoanType = loan.EncLoanType;
                    string LoanProgram = loan.EncLoanProgram;

                    string loannum = LoanNumber;

                    updateSchedClosingAudit(dbConn, loannum, SchedClosingDate);
               
                    //update the closing date, last milestone and loan statuses for every VOE order for that loan
                    if (SchedClosingDate > DateTime.Parse("1900-01-01"))
                    {

                        /*if this is going to be a change to the scheduled closing date 
                        AND there is already an APPROVED or Pending final order 
                        we need to sever the link with the reverif order
                        by nulling it out (which will cause a new reverif order to be generated*/
                        OrderRequest currLoan = dbConn.SingleWhere<OrderRequest>("LoanNumber", loannum);

                        if (currLoan.EncSchedClosingDate != loan.EncSchedClosingDate
                            && currLoan.EncSchedClosingDate != null)
                        {
                            //DateTime NewFinalOrderExpireDate = EncBusinessDayAdd(SchedClosingDate, -1 * order.FinalOrderLeadTimeDays, null, UserName, Password);

                            DateTime NewFinalOrderExpireDate = BusinessDayAdd(dbConn, SchedClosingDate, -1 * FinalOrderLeadTimeDays);

                            //update closing date, milestone and status and null out reverif order for approved or pending reverif orders
                            //2016-11-29 no longer null out initial orderlink in order to consolidadte all final activity into one order
                            //could probably simplify this whole block due to this change, but let's see how this flies first
                            string strUpdateApproved = String.Format("UPDATE OrderRequest "
                                + "SET EncSchedClosingDate = '{0}', EncLastMilestone = '{1}', EncLoanStatus = '{2}',  EncCloserName = '{3}', "
                                // + "EncProcessorName = '{4}', EncClosingCoordName = '{5}', ReverificationOrderRequestId = null, "
                                + "EncProcessorName = '{4}', EncClosingCoordName = '{5}', IsCertExpireDismissed = 0, "
                                + "EncFundingDate = '{6}',  EncUnderwriterName = '{7}',  IsAuditDismissed = 0,  "
                                + "EncCurrentLoanFolder = '{8}', EncLoanAssistantName = '{9}', EncBranchAdminName = '{10}' "
                                + "FROM OrderRequest "
                                + "INNER JOIN (SELECT Id as ReverifId, fxCurrentOrderStatusName "
                                + "FROM OrderRequest "
                                + "WHERE fxCurrentOrderStatusName = 'Approved' "
                                + "AND OrderRequest.fxLatestActivityDateTime < '{11}') tblReverifOrders "
                                + "ON OrderRequest.ReverificationOrderRequestId = tblReverifOrders.ReverifId "
                                + "WHERE LoanNumber = '{12}' ",
                                SchedClosingDate.ToString("yyyy-MM-dd"),
                                LastMilestone,
                                LoanStatus,
                                LoanCloserName,
                                LoanProcessorName,
                                LoanClosingCoordName,
                                LoanFundingDate.ToString("yyyy-MM-dd"),
                                LoanUnderwriterName,
                                LoanFolder,
                                LoanAssistantName,
                                LoanBranchAdminName,
                                NewFinalOrderExpireDate.ToString("yyyy-MM-dd"),
                                loannum);

                            dbConn.ExecuteSql(strUpdateApproved);

                            //update closing date, milestone and status for orders without approved reverif orders
                            //probably can take the left join out here - no longer being used
                            string strUpdateUnApproved = String.Format("UPDATE OrderRequest "
                                + "SET EncSchedClosingDate = '{0}', EncLastMilestone = '{1}', EncLoanStatus = '{2}', EncCloserName = '{3}', "
                                + "EncProcessorName = '{4}', EncClosingCoordName = '{5}', EncFundingDate = '{6}', IsCertExpireDismissed = 0, "
                                + "IsAuditDismissed = 0,  EncUnderwriterName = '{7}', EncCurrentLoanFolder = '{8}', EncLoanAssistantName = '{9}', EncBranchAdminName = '{10}' "
                                + "FROM OrderRequest "
                                + "LEFT JOIN (SELECT Id as ReverifId, fxCurrentOrderStatusName "
                                + "FROM OrderRequest "
                                + "WHERE fxCurrentOrderStatusName <> 'Approved') tblReverifOrders "
                                + "ON OrderRequest.ReverificationOrderRequestId = tblReverifOrders.ReverifId "
                                + "WHERE LoanNumber = '{11}' ",
                                SchedClosingDate.ToString("yyyy-MM-dd"),
                                LastMilestone,
                                LoanStatus,
                                LoanCloserName,
                                LoanProcessorName,
                                LoanClosingCoordName,
                                LoanFundingDate.ToString("yyyy-MM-dd"),
                                LoanUnderwriterName,
                                LoanFolder,
                                LoanAssistantName,
                                LoanBranchAdminName,
                                loannum);

                            dbConn.ExecuteSql(strUpdateUnApproved);

                            //undo urgent flag if this is outside of closing window
                            if (NewFinalOrderExpireDate > DateTime.Today)
                            {
                                dbConn.Update<OrderRequest>(
                                set: "IsUrgent = 0",
                                where: "LoanNumber = {0}".Params(loannum));
                                Log.Info("Urgent Flag Removed for " + loannum);
                            }

                            //add this loan to list of notifications
                            if (!closingdateNotifications.Exists(q => q.LoanNumber == loannum))
                            {
                                ClosingDateChange change = new ClosingDateChange
                                {
                                    LoanNumber = loannum,
                                    OldClosingDate = ((DateTime)currLoan.EncSchedClosingDate),
                                    NewClosingDate = SchedClosingDate
                                };

                                closingdateNotifications.Add(change);
                            }


                        }
                        else
                        {
                            //just update closing date, milestone and status and closer name and funding date
                            //NOTE: these things really don't like being wrappped to multiline
                            dbConn.Update<OrderRequest>(
                            set: "EncSchedClosingDate = {0}, EncLastMilestone = {1}, EncLoanStatus = {2}, EncCloserName = {3}, EncProcessorName = {4}, EncClosingCoordName = {5}, EncFundingDate = {6},  EncUnderwriterName = {7}, EncCurrentLoanFolder = {8}, EncLoanAssistantName = {9}, EncBranchAdminName = {10} ".Params(
                                SchedClosingDate, LastMilestone, LoanStatus, LoanCloserName, LoanProcessorName, LoanClosingCoordName, LoanFundingDate, LoanUnderwriterName, LoanFolder, LoanAssistantName, LoanBranchAdminName),
                            where: "LoanNumber = {0}".Params(loannum));

                        }

                    }
                    else
                    {
                        //sched closing date not available yet (or removed in Encompass)
                        dbConn.Update<OrderRequest>(
                            set: "EncSchedClosingDate = {0}, EncLastMilestone = {1}, EncLoanStatus = {2}, EncCloserName = {3}, EncProcessorName = {4}, EncClosingCoordName = {5}, EncFundingDate = {6},  EncUnderwriterName = {7},  EncCurrentLoanFolder = {8}, EncLoanAssistantName = {9},  EncBranchAdminName = {10}, IsUrgent = 0 ".Params(
                                "1900-01-01", LastMilestone, LoanStatus, LoanCloserName, LoanProcessorName, LoanClosingCoordName, LoanFundingDate, LoanUnderwriterName, LoanFolder, LoanAssistantName, LoanBranchAdminName),
                            where: "LoanNumber = {0}".Params(loannum));
                    }

                    //check on the RIHousing Flag
                    if (NewIsRIHousing && !OldIsRIHousing)
                    {
                        //order was incorrect or flag was added after the fact.  we need to update RIHousing Flag
                        //first, calculate the updated lead time days
                        OrderOps op = new OrderOps();
                        int newFinalOrderLeadTimeDays = op.calcFinalOrderLeadTimeDays(dbConn, EncEmployerName, NewIsRIHousing);

                        dbConn.Update<OrderRequest>(
                           set: "IsRIHousing = {0}, FinalOrderLeadTimeDays = {1} ".Params(NewIsRIHousing, newFinalOrderLeadTimeDays),
                           where: "LoanNumber = {0}".Params(loannum));
                        Log.Info("Added RIHousing Flag for LoanNumber = " + loannum);

                    }

                    //check the active loan folder date
                    if (isNull(CurrentLoanFolder, "") != LoanFolder) //if this is a change in folder
                    {
                        if (LoanFolder == "Active Loans") {

                            //this is when a loan first moves from Prospects to Active Loans
                            if (InitialLoanFolder == "Prospects") {

                                dbConn.Update<OrderRequest>(
                                   set: "EncActiveLoansFolderDate = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'",
                                   where: "LoanNumber = {0}".Params(loannum));
                               
                            }
                            else if (InitialLoanFolder == "Active Loans") { 
                                //this is when a loan is created directly in the Active Loans folder

                                dbConn.Update<OrderRequest>(
                                   set: "EncActiveLoansFolderDate = OrderRequestDate",
                                   where: "LoanNumber = {0}".Params(loannum));
                                
                            }

                        }
                    }

                    //check the LoanType 
                    if (isNull(CurrentLoanType,"") != LoanType)
                    {
                        dbConn.Update<OrderRequest>(
                            set: "EncLoanType = '" + LoanType + "'",
                            where: "LoanNumber = {0}".Params(loannum));
                    }

                    //check the LoanProgram 
                    if (isNull(CurrentLoanProgram, "") != LoanProgram)
                    {
                        dbConn.Update<OrderRequest>(
                            set: "EncLoanProgram = {0}".Params(LoanProgram),
                            where: "LoanNumber = {0}".Params(loannum));
                    }

                    //check the loan status
                    if (adverseStatusList != null) {
                        if (adverseStatusList.Contains(CurrentLoanStatus) && LoanStatus == "Active Loan"
                            && !adverseToActiveLoans.Contains(loannum))
                        {
                            adverseToActiveLoans.Add(loannum);
                        }
                    }

                }
           



        }

        public void updateSchedClosingAudit(IDbConnection dbConn, string loannum, DateTime? SchedClosingDate)
        {

            try
            {
                string schedCloseFieldId = "CX.SCHCLOSE";
                EncFieldAudit schedClsAudit = dbConn.Where<EncFieldAudit>(q => q.LoanNumber == loannum && q.EncFieldId == schedCloseFieldId)
                    .OrderByDescending(r => r.Id).FirstOrDefault();
                string newSchedCloseDate = isNotNullDateTime(SchedClosingDate, "yyyy-MM-dd");

                if (schedClsAudit == null)
                {
                    //only update if this is a new date
                    if (newSchedCloseDate != null)
                    {
                        dbConn.Insert<EncFieldAudit>(new EncFieldAudit
                        {
                            LoanNumber = loannum,
                            EncFieldId = schedCloseFieldId,
                            LastUpdateDateTime = DateTime.Now,
                            //PrevValue ,
                            CurrValue = newSchedCloseDate
                        });
                    }

                }
                else
                {
                    //only update if this is a new date
                    if (schedClsAudit.CurrValue != newSchedCloseDate)
                    {
                        dbConn.Insert<EncFieldAudit>(new EncFieldAudit
                        {
                            LoanNumber = loannum,
                            EncFieldId = schedCloseFieldId,
                            LastUpdateDateTime = DateTime.Now,
                            PrevValue = schedClsAudit.CurrValue,
                            CurrValue = newSchedCloseDate
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error Updating Scheduled Close Date Audit for " + loannum, ex);
            }
        }

        public void bulkEditOrderRequest()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            OrderOps op = new OrderOps();

            //OrderRequest orders = dbConn.Where<OrderRequest>(q => q.EncLoanStatus != 'Loan Originate')
            string SQL = "SELECT [Id], [FinalOrderLeadTimeDays] FROM [dbo].[OrderRequest] "
                         + " WHERE EncLoanStatus <> 'Loan Originated' "
                         + " and fxCurrentOrderStatusName <> 'Cancelled' "
                         + "  and fxCurrentOrderStatusName <> 'Archived' "
                         + "  and(EncSchedClosingDate = '1900-01-01' OR EncSchedClosingDate > '2019-01-01') "
                         + "  and OrderRequestDate > '2019-01-01' "
                         + "  and(requesttypeid = 1  or requesttypeid = 2)";

            Log.Info("Start Updating Orders");
            List<OrderRequest> orders = dbConn.Query<OrderRequest>(SQL);

            foreach (OrderRequest order in orders)
            {

                int newFOLD = 0;

                if (order.FinalOrderLeadTimeDays != 3)
                {
                    newFOLD = 3;

                    op.saveFieldEdit(dbConn, order.Id, "FinalOrderLeadTimeDays", newFOLD.ToString(), "voesystem", 5);

                    Log.Info("Order Updated: " + order.Id.ToString());
                }
               
                

            }
            Log.Info("Done Updating Orders");


        }

        public void autoAssignOrders()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            //get all orders from autoassign orders view
            List<AutoAssignOrderView> orders = dbConn.Select<AutoAssignOrderView>().OrderBy(e => e.RulePriority).ToList();
            
            VOESystem.Data.Business.OrderOps oOp = new OrderOps();

            List<int> assignedOrders = new List<int>() { };

            foreach (AutoAssignOrderView order in orders)
            {
                //checking to see if this order has already been assigned to avoid duplicate reassignments for different assignment types
                if (!assignedOrders.Contains(order.OrderRequestId)) {
                    //need to refresh this on every loop
                    List<AutoAssignUserView> users = dbConn.Select<AutoAssignUserView>().ToList()
                    .Select<AutoAssignUserView, AutoAssignUserView>(q =>
                    {
                        if (q.AutoAssignTypeExclusions == null) { q.AutoAssignTypeExclusions = new List<string>() { }; };
                        if (q.BranchExclusions == null) { q.BranchExclusions = new List<string>() { }; };
                        return q;

                    }).ToList();

                    //this returns only one next user in line
                    AutoAssignUserView user = users.Where<AutoAssignUserView>(q =>
                        !q.AutoAssignTypeExclusions.Contains(order.AutoAssignType)
                        && !q.BranchExclusions.Contains(order.OrgCode))
                        .OrderBy(q => q.TodayAutoAssignCount)
                        .ThenBy(q => q.PendingOrderCount)
                        .FirstOrDefault();

                    if (user != null)
                    {
                        assignOrder<AutoAssignOrderView>(dbConn, ref oOp, order, true, user.UserName, 2, 8);  //pending, pending
                        assignedOrders.Add(order.OrderRequestId);
                    }
                    else
                    {
                        Log.Info("Cannot autoassign orders - no eligible users");
                    }
                }

            }

        }
        
        public void assignOrder<T>(IDbConnection dbConn, ref VOESystem.Data.Business.OrderOps oOp, T order, bool IsAutoAssign, 
            string newUserName, int? newStatusId = null, int? newSubStatusId = null)
            where T: AutoAssignOrderView
        {

            //first check to see if there is someone in the order
            if ((oOp.checkOrderLock(dbConn, order.OrderRequestId, "voesystem")).result == 0)
            {
                
                using (IDbTransaction trans = dbConn.OpenTransaction())
                {

                    string opTag;
                    if (IsAutoAssign)
                    {
                        opTag = "Auto";
                    }
                    else
                    {
                        opTag = "AdHoc";
                    }

                    //assign order, update status
                    RequestUser assign = new RequestUser
                    {
                        OrderRequestId = order.OrderRequestId,
                        UserName = newUserName,
                        AssignmentDateTime = DateTime.Now,
                        IsAutoAssign = IsAutoAssign
                    };

                    dbConn.Insert<RequestUser>(assign);

                    //update status
                    OrderActivity oaNew = oOp.getOrderActvityForNewActivty(dbConn, order.OrderRequestId, "voesystem", false);

                    if (newStatusId != null)
                    {
                        oaNew.PrevOrderStatusId = oaNew.CurrOrderStatusId;
                        oaNew.CurrOrderStatusId = newStatusId ?? 0;
                        oaNew.PrevOrderSubStatusId = oaNew.CurrOrderSubStatusId;
                        oaNew.CurrOrderSubStatusId = newSubStatusId;
                    }

                    oaNew.ActivityNote = "Order " + opTag + "Assigned to Specialist";
                    oaNew.PrevAssignedVOES = oaNew.AssignedVOES;
                    oaNew.AssignedVOES = newUserName;

                    dbConn.Insert<OrderActivity>(oaNew);

                    trans.Commit();
                    
                    Log.Info("Order " + order.OrderRequestId.ToString() + " " + opTag + "Assigned to " + newUserName);
                }

            }
            else
            {
                Log.Info("Order " + order.OrderRequestId.ToString() + " locked.  Cannot assign to " + newUserName);
            }




        }
        
        public void adHocAssignOrders(string newUserName)
        {


            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            //get all orders from adhocassign orders view
            List<AdhocAssignOrderView> orders = dbConn.Select<AdhocAssignOrderView>().ToList();

            VOESystem.Data.Business.OrderOps oOp = new OrderOps();

            foreach (AdhocAssignOrderView order in orders)
            {

                assignOrder<AdhocAssignOrderView>(dbConn, ref oOp, order, false, newUserName);  //do not modify status

            }

        }
        
        public void moveOrdersForLoanToStatus(IDbConnection dbConn, string loanNumber, int newStatusId, int newSubStatusId, List<string> exceptStatuses, string activityNote)
        {

            List<OrderDetailView> orders = dbConn.Where<OrderDetailView>(q => q.LoanNumber == loanNumber).ToList();

            OrderOps oOp = new OrderOps();

            foreach (OrderDetailView order in orders)
            {
                if (!exceptStatuses.Contains(order.VerificationStatus))
                {
                    //always exclude data corrected orders 
                    if (order.LinkedOrderRequestId == null)
                    {
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, order.OrderRequestId, "voesystem", false);
                        oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                        oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                        oa.CurrOrderStatusId = newStatusId;
                        oa.CurrOrderSubStatusId = newSubStatusId;
                        oa.ActivityNote = activityNote;

                    }


                }
            }

        }

        public void branchOrderNotifications()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            List<PendingOrderView> orders = dbConn.Where<PendingOrderView>(q => q.CutoffDate == DateTime.Today).ToList();

            EmailOps eOp = new EmailOps();
            
            foreach(PendingOrderView order in orders)
            {
                Dictionary<string, string> inlineData = new Dictionary<string, string>() { };
                inlineData.Add("#cutoffdays#", order.CutoffDays.ToString());

                eOp.sendTemplateEmail(dbConn, "Order Pending for 8 Days", order.OrderRequestId, null, false, false, null, false, null, null, inlineData);
                Log.Info("Branch Notification Sent for " + order.OrderRequestId);
            }


        }

        public void overrideFollowUpDate()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            List<OverrideFollowupDateView> orders = dbConn.Select<OverrideFollowupDateView>().ToList();

            OrderOps oOp = new OrderOps();

            foreach (OverrideFollowupDateView order in orders)
            {

                OrderActivity oaFU = oOp.getOrderActvityForNewActivty(dbConn, order.OrderRequestId, "voesystem", null);

                oaFU.ActivityNote = "Folloup Date Automatically Updated";
                oaFU.FollowupDate = DateTime.Today;

                dbConn.Insert<OrderActivity>(oaFU);

            }


        }

        public void generateOrderActivityEvents()
        {

            OrmLiteConfig.CommandTimeout = 600;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            dbConn.ExecuteNonQuery("EXEC usp_GenerateOrderActivityEvents");

        }

        

    }


}
