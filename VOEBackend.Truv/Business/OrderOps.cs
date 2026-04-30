using Newtonsoft.Json;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOEBackend.Truv.TruvSchema;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOEBackend.Truv.Business
{
    public class OrderOps : BaseClass
    {

        public const string truvWrittenTemplateId = "5065cafc8c1c4bffa4dcc94b0f273728";
        public const string truvVerbalTemplateId = "f62a0030fcf44cf68c749ec9172d8867";

        public enum QueryType
        {
            [Description("Credentials")]
            Credentials,
            [Description("Reverify")]
            Reverify
        }

        public string submitNewCredentialsOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, 
            int orderTypeId, bool AddToast, string TestResultMessage = null)
        {

            string retVal = "Failure";
            string orderNumber = String.Empty;
            List<CommOps.ResponseResult> results = new List<CommOps.ResponseResult>() { };

            //get order information
            Request request = createCredentialsOrder(dbConn, orderRequestId, (OrderType)orderTypeId);

            orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

            CommOps comm = new CommOps();

            if (TestMode)
            {

                //this is test           
                logger.Info("Truv Service Test Mode for Credentials Order " + orderNumber);

                //write file
                string TruvReqType = "Submit";
                string reqestString = JsonConvert.SerializeObject(request);
                string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "TruvCredentialsOrder", TruvReqType);

                if (TestResultMessage == "Processing")
                {
                    retVal = "Processing";
                    results.Add(new CommOps.ResponseResult
                    {
                        TruvOrderId = "TRUV" + orderRequestId.ToString(),
                        OrderStatus = "Processing",
                    });
                }
                else
                {
                    //simulate no hit
                    results.Add(new CommOps.ResponseResult
                    {
                        TruvOrderId = "TRUV" + orderRequestId.ToString(),
                        OrderStatus = "Error",
                    });

                }


            }
            else
            {
                //this is production
                results = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, CommOps.TruvCommType.CreateCredentials, null);
            }


            //********************************************
            //Process Order Result
            //********************************************
            EmailOps eOp = new EmailOps();
            string truvOrderId = null;
            string truvShareURL = string.Empty;

            foreach (CommOps.ResponseResult result in results)
            {
                truvOrderId = result.TruvOrderId;

                if (result.OrderStatus == "Processing")
                {
                    //no files retrieved yet
                    retVal = result.OrderStatus;
                    truvShareURL = result.ShareURL;
                }
                else
                {
                    //some other processing error
                    retVal = "Error";
                }
            }

            //update vendor order number, status, vendor data date, first and last names
            OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
            order.TruvOrderNumber = truvOrderId;
            order.TruvOrderStatus = retVal;
                
            if (AddToast)
            {
                order.TruvToastAlertUserName = UserName;
            }
            else
            {
                order.TruvToastAlertUserName = null;
            }

            //write to activity log that order was processed
            VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
            OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

            oa.ActivityNote = "Credentials Order Sent to Truv; Truv Order Number " + truvOrderId + "; This process can take up to 36 hours to complete.";


            using (IDbTransaction tr = dbConn.BeginTransaction())
            {

                dbConn.UpdateOnly(order, q => new { q.TruvOrderStatus, q.TruvOrderNumber, q.TruvToastAlertUserName }, r => r.Id == orderRequestId);
                dbConn.Insert<OrderActivity>(oa);

                tr.Commit();
            }

            eOp.sendTemplateEmail(dbConn, "Truv Credentialing Notification Email to Branches", orderRequestId, null, true, false, order.RequestTypeId, false);

            if (order.RequestTypeId == 7)
            {
                Dictionary<string, string> inlineData = new Dictionary<string, string>() { };
                inlineData.Add("#shareurl#", truvShareURL);
                eOp.sendTemplateEmail(dbConn, "Truv Post Close QC Reverify Sent", orderRequestId, null, true, false, order.RequestTypeId, false, null, null, inlineData);
            }


            return retVal;


        }

        public string submitNewReverifyOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, 
            bool AddToast = false, string TestResultMessage = null)
        {

            string retVal = "Failure";
            string orderNumber = String.Empty;
            List<CommOps.ResponseResult> results = new List<CommOps.ResponseResult>() { };

            //try
            //    {
            //get order information
            OrderDetailView orderDetail = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();
            orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

            Request request = createReverifyOrder();

            CommOps comm = new CommOps();

            if (TestMode)
            {

                //this is test           
                logger.Info("Truv Service Test Mode for Reverify Order " + orderDetail.OrderNumber);

                //write file
                string TruvReqType = "Submit";
                string reqestString = JsonConvert.SerializeObject(request);
                string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderDetail.OrderNumber, "TruvReverifyOrder", TruvReqType);

                if (TestResultMessage == "Processing")
                {
                    retVal = "Processing";
                    results.Add(new CommOps.ResponseResult
                    {
                        TruvOrderId = "TRUV" + orderRequestId.ToString(),
                        OrderStatus = "Processing",
                    });
                }
                else
                {
                    //simulate no hit
                    results.Add(new CommOps.ResponseResult
                    {
                        TruvOrderId = "TRUV" + orderRequestId.ToString(),
                        OrderStatus = "Error",
                    });

                }


            }
            else
            {
                results = comm.postRequest(dbConn, request, orderDetail.OrderNumber, orderRequestId, UserName, CommOps.TruvCommType.CreateReverifyCredentials, orderDetail.TruvOrderNumber, null);
            }


            //********************************************
            //Process Order Result
            //********************************************

            EmailOps eOp = new EmailOps();
            string truvOrderId = null;
            string truvShareURL = string.Empty;

            foreach (CommOps.ResponseResult result in results)
            {
                truvOrderId = result.TruvOrderId;

                if (result.OrderStatus == "Processing")
                {
                    //no files retrieved yet
                    retVal = result.OrderStatus;
                    truvShareURL = result.ShareURL;
                }
                else
                {
                    //some other processing error
                    retVal = "Error";
                }
            }


            //update vendor order number, status, vendor data date, first and last names
            OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
            order.TruvOrderNumber = truvOrderId;
            order.TruvOrderStatus = retVal;

            if (AddToast)
            {
                order.TruvToastAlertUserName = UserName;
            }
            else
            {
                order.TruvToastAlertUserName = null;
            }

            //write to activity log that order was processed
            VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
            OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

            oa.ActivityNote = "Reverify Order Sent to Truv; Truv Order Number " + truvOrderId;

            using (IDbTransaction tr = dbConn.BeginTransaction())
            {
                dbConn.UpdateOnly(order, q => new { q.TruvOrderStatus, q.TruvOrderNumber, q.TruvToastAlertUserName }, r => r.Id == orderRequestId);
                dbConn.Insert<OrderActivity>(oa);

                tr.Commit();
            }

            //email share url
            EmailOps eop = new EmailOps();
            Dictionary<string, string> inlineData = new Dictionary<string, string>() { };
            inlineData.Add("#shareurl#", truvShareURL);
            eop.sendTemplateEmail(dbConn, "Truv Reverify Sent", orderRequestId, null, true, false, order.RequestTypeId, false, null, null, inlineData);

            return retVal;

        }

        public string queryOrderStatus(IDbConnection dbConn, int orderRequestId, string UserName, out List<int> certFileIds, QueryType type)
        {

            string retVal = "Failure";
            certFileIds = new List<int>();
            List<CommOps.ResponseResult> results = null;
            
            //try
            //    {
            //get order information

            OrderDetailView ordDetail = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            CommOps comm = new CommOps();

            //this is production
            CommOps.TruvCommType commType = CommOps.TruvCommType.QueryCredentials;
            if (type == QueryType.Credentials)
            {
                commType = CommOps.TruvCommType.QueryCredentials;
            }
            else if (type == QueryType.Reverify)
            {
                commType = CommOps.TruvCommType.QueryReverifyCredentials;
            }

            results = comm.postRequest(dbConn, null, ordDetail.OrderNumber, orderRequestId, UserName, commType, ordDetail.TruvOrderNumber);


            //********************************************
            //Process Order Result
            //********************************************

            DocumentOps dOp = new DocumentOps();
            PDFOps pOp = new PDFOps();
            string truvOrderId = string.Empty;
            string truvShareURL = string.Empty;
            int documentContentTypeId = dbConn.Where<DocumentContentType>(q => q.Name == "Credentials Income").FirstOrDefault().Id;

            foreach (CommOps.ResponseResult result in results) {

                truvOrderId = result.TruvOrderId;
                truvShareURL = result.ShareURL;

                if (result.OrderStatus == "Completed")
                {

                    retVal = "Success";

                    if (result.CertFile != null)
                    {

                        string fileName = Path.GetFileName(result.CertFile);
                        string[] fileParts = result.CertFile.Split("_"[0]);
                        string displayName = fileParts[fileParts.Length - 1];
                        string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + fileName;
                        string VendorDocFilePath = RepositoryPath + "Documents\\TruvDocuments\\" + fileName;

                        UploadResult res = saveDocument(dbConn, UploadFilePath, VendorDocFilePath, orderRequestId, displayName, fileName, null, UserName);

                        if (!res.Result)
                        {
                            logger.Error("Error Attaching Truv Document to Order " + ordDetail.OrderNumber + " " + fileName,
                                new Exception("Error Attaching Truv Document to Order"));
                        }
                        else
                        {
                            certFileIds.Add(res.DocumentId);
                        }
                    }

                    if (result.W2s != null)
                    {
                        foreach (string w2 in result.W2s)
                        {

                            string fileName = Path.GetFileName(w2);
                            string[] fileParts = w2.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + fileName;
                            string VendorDocFilePath = RepositoryPath + "Documents\\TruvDocuments\\" + fileName;

                            UploadResult res = saveDocument(dbConn, UploadFilePath, VendorDocFilePath, orderRequestId, displayName, fileName, documentContentTypeId, UserName);

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Truv Document to Order " + ordDetail.OrderNumber + " " + fileName,
                                    new Exception("Error Attaching Truv Document to Order"));
                            }

                        }
                    }

                    if (result.PayStubs != null)
                    {
                        foreach (string paystub in result.PayStubs)
                        {

                            string fileName = Path.GetFileName(paystub);
                            string[] fileParts = paystub.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + fileName;
                            string VendorDocFilePath = RepositoryPath + "Documents\\TruvDocuments\\" + fileName;

                            UploadResult res = saveDocument(dbConn, UploadFilePath, VendorDocFilePath, orderRequestId, displayName, fileName, documentContentTypeId, UserName);

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Truv Document to Order " + ordDetail.OrderNumber + " " + fileName,
                                    new Exception("Error Attaching Truv Document to Order"));
                            }

                        }
                    }

                }
                else if (result.OrderStatus == "Cancelled")
                {
                    retVal = "Cancelled";
                }
                else if (result.OrderStatus == "Processing")
                {
                    retVal = "Processing";
                }
                else if (result.OrderStatus == "Failed")
                {
                    retVal = "Failed";
                }

            }

            if (retVal == "Success")
            {

                EmailOps eop = new EmailOps();
                using (IDbTransaction tr = dbConn.BeginTransaction())
                {

                    //update order
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.TruvOrderNumber = truvOrderId;
                    order.TruvOrderStatus = retVal;

                    //write to activity log that order was processed
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                    oa.ActivityNote = type.GetDescription() + " Order Received from Truv; Truv Order Number " + truvOrderId;

                    if (type == QueryType.Credentials)
                    {
                        
                        eop.sendTemplateEmail(dbConn, "Truv Credentials Results Received", orderRequestId, null, true, false, null, false, tr);

                        if (oa.CurrOrderStatusId == 24)  //if it is still in vendor pipeline
                        {
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //vendor parent status
                            oa.CurrOrderSubStatusId = 25; //vendor verified
                            oa.ActivityNote += "; Move to Vendor Verified Status";
                        }
                    }

                    if (order.TruvToastAlertUserName != null)
                    {
                        ToastAlertOps top = new ToastAlertOps();
                        top.createAlert(dbConn, null, order.TruvToastAlertUserName, orderRequestId, null, "Truv " + type.GetDescription() + " Results Recieved");
                    }

                    dbConn.UpdateOnly(order, q => new { q.TruvOrderNumber, q.TruvOrderStatus }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);

                    tr.Commit();
                }



            }           
            else if (retVal == "Cancelled" || retVal == "Failed")
            {

                //update order
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.TruvOrderNumber = truvOrderId;
                order.TruvOrderStatus = retVal;

                //write to activity log that order was cancelled
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = type.GetDescription() + " Truv Order " + retVal;

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {

                    if (type == QueryType.Credentials)
                    {
                        EmailOps eop = new EmailOps();
                        eop.sendTemplateEmail(dbConn, "Truv Credentials Order Cancelled", orderRequestId, null, true, false, null, false, tr);

                        if (oa.CurrOrderStatusId == 24)  //if it is still in vendor pipeline
                        {
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 1; //new
                            oa.CurrOrderSubStatusId = null;
                            oa.ActivityNote += "; Move to New Status";
                        }

                    }

                    dbConn.UpdateOnly(order, q => new { q.TruvOrderNumber, q.TruvOrderStatus }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);

                    tr.Commit();
                }

            }
            else if (retVal == "Processing")
            {
                //no files retrieved yet

            }


            return retVal;

        }

        private UploadResult saveDocument(IDbConnection dbConn, string UploadFilePath, string VendorDocFilePath, int orderRequestId, string displayName, string fileName, int? documentContentTypeId, string UserName)
        {

            UploadResult res = new UploadResult();
            res.Result = false;

            //TODO: this needs to be changed so that we don't have to have two copies of the cert
            if (File.Exists(UploadFilePath))
            {
                File.Delete(UploadFilePath);
            }
            File.Copy(VendorDocFilePath, UploadFilePath);
            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

            PDFOps pOp = new PDFOps();
            int? pageCount = pOp.getPageCount(UploadFilePath);

            DocumentOps dOp = new DocumentOps();
            
            try
            {
                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, fileName,
                    DocumentOps.DocumentType.TruvDownload, res, true, UploadFilePath, pageCount, UserName, null, false, documentContentTypeId);
                int docId = res.DocumentId;

                int VendorId = dbConn.Where<Vendor>(q => q.Name == "Truv-Credentials").FirstOrDefault().Id;

                dbConn.Update<Document>(
                        set: "VendorId = {0}".Params(VendorId),
                        where: "Id = {0}".Params(docId));

            }
            catch (Exception ex)
            {
                logger.Error("Error Saving Document " + UploadFilePath, ex);
                res.Result = false;
            }

            return res;

        }

        private Request createCredentialsOrder(IDbConnection dbConn, int orderRequestId, OrderType orderType)
        {

            Request retVal = new Request();
            bool sendMobileNumber = true;
            bool sendEmailAddress = true;

            OrderRequest voeOrder = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            LoanOfficerOverride looverride = dbConn.Where<LoanOfficerOverride>(q => q.LoanOfficerName == voeOrder.EncLoanOfficerName).FirstOrDefault();
            if (looverride != null)
            {
                if (looverride.CredentialsExcludeSMS == true)
                {
                    sendMobileNumber = false;
                }
            }

            if (TruvSendBorrowerEmailFromVOESystem)
            {
                sendEmailAddress = false;
            }

            if (isNull(voeOrder.BorrowerEmail, "") == "" && sendEmailAddress)
            {
                throw new Exception("Email Address Missing");
            }

            retVal.products = new List<string>() { };

            if ((int)orderType == 1)
            {
                //this is verbal
                retVal.products.Add(TruvSchema.Product.employment.GetDescription());
                retVal.template_id = truvVerbalTemplateId;
            }
            else
            {
                //must be written
                retVal.products.Add(TruvSchema.Product.income.GetDescription());
                retVal.template_id = truvWrittenTemplateId;
            }

            BorrowerName borr = splitBorrowerName(voeOrder.BorrowerFullName, false);

            retVal.first_name = borr.FirstName;
            retVal.last_name = borr.LastName;
            retVal.ssn = voeOrder.BorrowerSSN.Replace("-", "");

            if (sendEmailAddress)
            {
                retVal.email = voeOrder.BorrowerEmail;
            }
            else
            {
                retVal.email = "voe@firsthome.com";
            }

            if (isNull(voeOrder.BorrowerMobilePhone, "") != "" && sendMobileNumber)
            {
                retVal.phone = voeOrder.BorrowerMobilePhone.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");
            }
            retVal.loan_number = voeOrder.LoanNumber;

            TruvSchema.Request.Employer employer = new TruvSchema.Request.Employer();
            employer.company_name = voeOrder.EncEmployerName;

            retVal.employers = new List<Request.Employer>() { };
            retVal.employers.Add(employer);

            //testing values
            //retVal.email = "MSwinehart@firsthome.com";
            //if (sendMobileNumber)
            //{
            //    retVal.phone = "4438072026";
            //}
            //retVal.ssn = "000-00-0000".Replace("-", "");

            return retVal;


        }

        private Request createReverifyOrder()
        {

            Request retVal = new Request();
            
            retVal.products = new List<string>() { };

            //this is verbal
            retVal.products.Add(TruvSchema.Product.employment.GetDescription());

            return retVal;

        }

        public bool companySearch(IDbConnection dbConn, int orderRequestId)
        {

            bool retVal = false;
            
            CommOps.ResponseResult result = null;

            OrderDetailView order = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            CommOps comm = new CommOps();

            //this is production
            CommOps.TruvCommType commType = CommOps.TruvCommType.SearchCompany;
            
            result = comm.postRequest(dbConn, null, order.OrderNumber, orderRequestId, "voesystem", commType, null, null, order.EncEmployerName).FirstOrDefault();

            foreach (CommOps.CompanyResult company in result.Companies)
            {
                logger.Info(order.OrderNumber + ";" + order.EncEmployerName + ";" + company.CompanyName + ";" + company.ConfidenceLevel + ";" + company.SuccessRate);
            }

            return retVal;

        }

        public void autoQueryOpenOrderStatus()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                 ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                 true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTruvOpenOrderView> trOrders = dbConn.Select<AutoTruvOpenOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Querying " + trOrders.Count.ToString() + " open order on Truv");

                foreach (AutoTruvOpenOrderView trOrder in trOrders)
                {
                    try
                    {

                        List<int> certIds = new List<int>() { };
                        if (trOrder.RequestTypeId == 3)
                        {
                            queryOrderStatus(dbConn, trOrder.OrderRequestId, "voesystem", out certIds, QueryType.Reverify);
                        }
                        else
                        {
                            queryOrderStatus(dbConn, trOrder.OrderRequestId, "voesystem", out certIds, QueryType.Credentials);
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Truv Query Order Error: ", ex);
                    }

                }
            }

        }

        public void autoSubmitOrdersToTruv()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTruvOrderView> tvOrders = dbConn.Select<AutoTruvOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting " + tvOrders.Count.ToString() + " orders to Truv");

                foreach (AutoTruvOrderView tvOrder in tvOrders)
                {
                    try
                    {

                        Dictionary<string, object> prms = new Dictionary<string, object> { };
                        prms.Add("OrderRequestId", tvOrder.OrderRequestId);

                        List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);
                        string result = "Success";

                        //day 1 exists or opted out or employer excluded
                        if (tvOrder.IsEmployerExcluded || tvOrder.IsTWCredentialsOptOut || tvOrder.CurrentCount > 1)
                        {
                            result = "Skipped";

                            //update the Truv status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "TruvOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(tvOrder.OrderRequestId));
                            logger.Info("Skipping Truv Pull for " + tvOrder.OrderRequestId.ToString());

                        }

                        //this will not repull orders that have a success for the day 1
                        else if (day1OrderId.Count > 0)
                        {

                            OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, tvOrder.OrderRequestId, "voesystem", false);

                            result = "Skipped";

                            //update the Truv status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "TruvOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(tvOrder.OrderRequestId));
                            logger.Info("Skipping Truv Pull for " + tvOrder.OrderRequestId.ToString());

                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //vendor parent status
                            oa.CurrOrderSubStatusId = 25; //vendor verified
                            oa.ActivityNote = "Move to Vendor Verified Status";

                            dbConn.Insert<OrderActivity>(oa);


                        }
                        else
                        {
                            //submit order
                            List<int> certIds = new List<int>() { };
                            result = submitNewCredentialsOrder(dbConn, tvOrder.OrderRequestId, "voesystem", TruvServiceTestMode, tvOrder.OrderTypeId, false, TruvTestResultMessage);

                            if (result.StartsWith("Processing"))
                            {
                                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, tvOrder.OrderRequestId, "voesystem", false);

                                //yay it worked - credentials
                                oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                                oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                                oa.CurrOrderStatusId = 24; //vendor parent status
                                oa.CurrOrderSubStatusId = 32; //pending vendor
                                oa.ActivityNote = "Move to Pending Vendor Status";

                                dbConn.Insert<OrderActivity>(oa);
                            }
                        }

                      

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Truv Order Error: ", ex);
                    }

                }


            }
        }

        public void autoReverifyOrdersToTruv()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTruvReverifyView> tvOrders = dbConn.Select<AutoTruvReverifyView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting Reverif " + tvOrders.Count.ToString() + " orders to Truv");

                foreach (AutoTruvReverifyView tvOrder in tvOrders)
                {
                    try
                    {

                        List<int> certIds = new List<int>() { };
                        string result = submitNewReverifyOrder(dbConn, tvOrder.OrderRequestId, "voesystem", false, false);

                        //really don't care what the status was, still moving to the TWN final queue
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, tvOrder.OrderRequestId, "voesystem", false);
                        if (oa.CurrOrderSubStatusId != 29)
                        {
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.CurrOrderStatusId = 24;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderSubStatusId = 29;

                            oa.ActivityNote = "Order Moved to AutoWork# Final Order Status";
                            dbConn.Insert<OrderActivity>(oa);
                        }


                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Truv Order Error: ", ex);
                    }

                }


            }
        }

        public void forwardTruvNotifications()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                 ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                 true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();
                int emailTemplateId = dbConn.Where<VOESystem.Data.DBSchema.EmailTemplate>(q => q.Name == "Truv Notification Forwarded").FirstOrDefault().Id;

                List <EmailOpenTruvForwardView> nots = dbConn.Select<EmailOpenTruvForwardView>();

                EmailOps eop = new EmailOps();

                foreach(EmailOpenTruvForwardView not in nots)
                {
                    eop.ForwardEmail(dbConn, not.OrderRequestId, emailTemplateId, not.EmailId);

                }


            }

        }

            public void tempGetCompanyInfo()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<OrderDetailView> tvOrders = dbConn.Where<OrderDetailView>(q => q.TruvOrderStatus == "Cancelled" && q.RequestedDate > DateTime.Parse("2025-04-20"))
                    .OrderByDescending(r => r.OrderRequestId).ToList();

                foreach (OrderDetailView tvOrder in tvOrders)
                {
                    companySearch(dbConn, tvOrder.OrderRequestId);
                } 


            }
        }



    }
}
