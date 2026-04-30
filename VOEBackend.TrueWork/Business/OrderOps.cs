using Newtonsoft.Json;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using VOEBackend.TrueWork.TrueWorkSchema;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;
using static VOEBackend.TrueWork.Business.CommOps.ResponseResult;

namespace VOEBackend.TrueWork.Business
{
    public class OrderOps : BaseClass
    {

        public enum QueryType
        {
            [Description("Credentials")]
            Credentials,
            [Description("Reverify")]
            Reverify
        }

        public string submitNewInstantOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
            int orderTypeId, string verifType, bool IsDay1, string firstName = null, string lastName = null, string TestResultMessage = null)
        {

            string retVal = "Failure";
            string orderNumber = String.Empty;
            certFileIds = new List<int>();
            CommOps.ResponseResult result = null;

            //try
            //    {
                    //get order information
                    Request request = createInstantOrder(dbConn, orderRequestId, (OrderType)orderTypeId, (VerificationType)Enum.Parse(typeof(VerificationType), verifType), IsDay1, TestMode);  

                    orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                    CommOps comm = new CommOps();

                    if (TestMode)
                    {

                        //this is test           
                        logger.Info("Truework Service Test Mode for Instant Order " + orderNumber);

                        //write file
                        string TrueworkReqType = "Submit";
                        string reqestString = JsonConvert.SerializeObject(request);
                        string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "TrueworkInstantOrder", TrueworkReqType);

                        if (TestResultMessage == "Success")
                        {
                            retVal = "Success";
                            result = new CommOps.ResponseResult
                            {
                                TrueworkOrderId = "TRUI" + orderRequestId.ToString(),
                                Status = "Done",
                                Files = new List<ReportFile>() {
                                    new ReportFile {
                                         FileName = Path.GetFileName(TestPDFFilePath),
                                         ReportId = "AtestReportId",
                                         RequestId = "ATestRequestId"
                                    }
                                },
                                ResultMessage = "Success"
                            };
                        }
                        else
                        {
                            //simulate no hit
                            result = new CommOps.ResponseResult
                            {
                                TrueworkOrderId = "TRUI" + orderRequestId.ToString(),
                                Status = "Error",
                                ResultMessage = "Not Found"
                            };

                        }


                    }
                    else
                    {
                        //this is production
                        result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, true, CommOps.TrueWorkCommType.CreateInstant, IsDay1);
                    }


                //********************************************
                //Process Order Result
                //********************************************

                if (result.Status == "Done")
                {

                    bool fileAttError = false;

                    //save the cert files to linked docs          
                    if (result.Files.Count > 0)
                    {
                        DocumentOps dOp = new DocumentOps();
                        PDFOps pOp = new PDFOps();

                        foreach (ReportFile file in result.Files)
                        {
                            string[] fileParts = file.FileName.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file.FileName;
                            string VendorDocFilePath = RepositoryPath + "Documents\\TrueworkDocuments\\" + file.FileName;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert
                            if (File.Exists(UploadFilePath))
                            {
                                File.Delete(UploadFilePath);
                            }
                            File.Copy(VendorDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            try
                            {
                                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file.FileName,
                                    DocumentOps.DocumentType.TrueWorkDownload, res, true, UploadFilePath, pageCount, UserName);
                                int docId = res.DocumentId;

                                int VendorId = dbConn.Where<Vendor>(q => q.Name == "TrueWork").FirstOrDefault().Id;
                                string VendorReferenceNum = file.DUReferenceNumber;

                                //update document record
                                dbConn.Update<Document>(
                                     set: "TrueWorkReportId = {0}, TrueWorkRequestId = {1}, VendorId = {2}, VendorReferenceNum = {3}".Params(file.ReportId, file.RequestId, VendorId, VendorReferenceNum),
                                     where: "Id = {0}".Params(docId));

                                dbConn.Insert<DocumentEmployer>(new DocumentEmployer
                                {
                                    DocumentId = docId,
                                    EmployerName = file.EmployerName,
                                    EmployeeStatus = file.EmployeeStatus
                                });

                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error Saving Document " + UploadFilePath, ex);
                                res.Result = false;
                            }

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching TrueWork Document to Order " + orderNumber + " " + file,
                                    new Exception("Error Attaching TrueWork Document to Order"));
                                fileAttError = true;
                            }
                            else
                            {
                                certFileIds.Add(res.DocumentId);
                            }

                        }

                        if (fileAttError)
                        {
                            //could not attach files for some reason
                            result.ResultMessage = "Error: Cannnot Attach Downloaded Certification File(s) to Order";
                        }
                        else
                        {
                            retVal = "Success";
                            logger.Info("TrueWork instant order has been completed for order " + orderNumber + ": TrueWorkOrderNumber " + result.TrueworkOrderId);
                        }

                    }
                    else
                    {
                        result.ResultMessage += " Error: No Certification Files Found";
                    }


                }
                else
                {
                    //some other processing error - could be be no hit
                    //description will be in ResultMessage
                    result.ResultMessage = "Error: " + result.ResultMessage;
                }

                //update vendor order number, status, vendor data date, first and last names
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.TrueWorkOrderNumber = result.TrueworkOrderId;
                order.TrueWorkOrderStatus = retVal;  //will be success or failure
                order.TrueWorkOrderType = "Instant";

                //write to activity log that order was processed
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = "Instant Order Sent to TrueWork; TrueWork Order Number " + result.TrueworkOrderId;
               
                using (IDbTransaction tr = dbConn.BeginTransaction())
                {
                    dbConn.UpdateOnly(order, q => new { q.TrueWorkOrderNumber, q.TrueWorkOrderStatus, q.TrueWorkOrderType }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);
                    tr.Commit();
                }

            //}
            //catch (Exception ex)
            //{
            //    //some other type of error 
            //    logger.Error("Truework instant order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            //}

            return retVal;

        }

        public string submitNewReverifyOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, string VendorReportId,
            string VendorRequestId, bool AddToast = false, string TestResultMessage = null)
        {

            string retVal = "Failure";
            CommOps.ResponseResult result = null;

            //try
            //    {
            //get order information
            OrderDetailView orderDetail = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            Request request = createReverifyOrder(VendorReportId);  

            CommOps comm = new CommOps();

            if (TestMode)
            {

                //this is test           
                logger.Info("Truework Service Test Mode for Reverify Order " + orderDetail.OrderNumber);

                //write file
                string TrueworkReqType = "Submit";
                string reqestString = JsonConvert.SerializeObject(request);
                string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderDetail.OrderNumber, "TrueworkReverifyOrder", TrueworkReqType);

                if (TestResultMessage == "Processing")
                {
                    retVal = "Processing";
                    result = new CommOps.ResponseResult
                    {
                        TrueworkOrderId = "TRUR" + orderRequestId.ToString(),
                        Status = "Done",
                        Files = new List<ReportFile>() {
                                    new ReportFile {
                                         FileName = Path.GetFileName(TestPDFFilePath)
                                    }
                                },
                        ResultMessage = "Processing"
                    };
                }
                else
                {
                    //simulate no hit
                    result = new CommOps.ResponseResult
                    {
                        TrueworkOrderId = "TRUR" + orderRequestId.ToString(),
                        Status = "Error",
                        ResultMessage = "Not Found"
                    };

                }


            }
            else
            {
                //this is production
                CommOps.TrueWorkCommType commType = CommOps.TrueWorkCommType.CreateReverify;
                if (orderDetail.TrueWorkOrderType == "Credentials")
                {
                    commType = CommOps.TrueWorkCommType.CreateReverifyCredentials;
                }

                result = comm.postRequest(dbConn, request, orderDetail.OrderNumber, orderRequestId, UserName, false, commType, false, VendorRequestId);
            }


            //********************************************
            //Process Order Result
            //********************************************

            if (result.Status == "Processing" )
            {
                //no files retrieved yet
                retVal = result.Status;
            }
            else
            {
                //some other processing error - could be be no hit
                //description will be in ResultMessage
                result.ResultMessage = "Error: " + result.ResultMessage;
            }

            //update vendor order number, status, vendor data date, first and last names
            OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
            order.TrueWorkOrderNumber = result.TrueworkOrderId;
            order.TrueWorkOrderStatus = retVal;
            if (AddToast)
            {
                order.TrueWorkToastAlertUserName = UserName;
            }
            else
            {
                order.TrueWorkToastAlertUserName = null;
            }

            //write to activity log that order was processed
            VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
            OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

            oa.ActivityNote = "Reverify Order Sent to TrueWork; TrueWork Order Number " + result.TrueworkOrderId;

            using (IDbTransaction tr = dbConn.BeginTransaction())
            {
                dbConn.UpdateOnly(order, q => new { q.TrueWorkOrderStatus, q.TrueWorkOrderNumber, q.TrueWorkToastAlertUserName }, r => r.Id == orderRequestId);
                dbConn.Insert<OrderActivity>(oa);

                tr.Commit();
            }

            //}
            //catch (Exception ex)
            //{
            //    //some other type of error 
            //    logger.Error("Truework instant order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            //}

            return retVal;

        }

        public string submitNewCredentialsOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
            int orderTypeId, bool AddToast, string TestResultMessage = null)
        {

            string retVal = "Failure";
            string orderNumber = String.Empty;
            certFileIds = new List<int>();
            CommOps.ResponseResult result = null;

            //try
            //    {
            //get order information
            Request request = createCredentialsOrder(dbConn, orderRequestId, (OrderType)orderTypeId, TestMode);

            orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

            CommOps comm = new CommOps();

            if (TestMode)
            {

                //this is test           
                logger.Info("Truework Service Test Mode for Instant Order " + orderNumber);

                //write file
                string TrueworkReqType = "Submit";
                string reqestString = JsonConvert.SerializeObject(request);
                string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "TrueworkCredentialsOrder", TrueworkReqType);

                if (TestResultMessage == "Processing")
                {
                    retVal = "Processing";
                    result = new CommOps.ResponseResult
                    {
                        Status = "Processing",  
                    };
                }
                else
                {
                    //simulate no hit
                    result = new CommOps.ResponseResult
                    {
                        TrueworkOrderId = "TRUI" + orderRequestId.ToString(),
                        Status = "Error",
                        ResultMessage = "Not Found"
                    };

                }


            }
            else
            {
                //this is production
                result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, false, CommOps.TrueWorkCommType.CreateCredentials, false);
            }


            //********************************************
            //Process Order Result
            //********************************************

            if (result.Status == "Processing")
            {
                //no files retrieved yet
                retVal = result.Status;
            }
            else
            {
                //some other processing error - could be be no hit
                //description will be in ResultMessage
                result.ResultMessage = "Error: " + result.ResultMessage;
            }

            //update vendor order number, status, vendor data date, first and last names
            OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
            order.TrueWorkOrderNumber = result.TrueworkOrderId;
            order.TrueWorkOrderStatus = retVal;
            order.TrueWorkOrderType = "Credentials";

            if (AddToast)
            {
                order.TrueWorkToastAlertUserName = UserName;
            }
            else
            {
                order.TrueWorkToastAlertUserName = null;
            }

            //write to activity log that order was processed
            VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
            OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

            oa.ActivityNote = "Credentials Order Sent to TrueWork; TrueWork Order Number " + result.TrueworkOrderId + "; This process can take up to 36 hours to complete.";


            using (IDbTransaction tr = dbConn.BeginTransaction())
            {

                dbConn.UpdateOnly(order, q => new { q.TrueWorkOrderStatus, q.TrueWorkOrderNumber, q.TrueWorkOrderType, q.TrueWorkToastAlertUserName }, r => r.Id == orderRequestId);
                dbConn.Insert<OrderActivity>(oa);

                tr.Commit();
            }

            EmailOps eOp = new EmailOps();
            eOp.sendTemplateEmail(dbConn, "TW Credentialing Notification Email to Branches", orderRequestId, null, true, false, order.RequestTypeId, false);


            //}
            //catch (Exception ex)
            //{
            //    //some other type of error 
            //    logger.Error("Truework instant order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            //}

            return retVal;


        }

        public string queryOrderStatus(IDbConnection dbConn, int orderRequestId, string UserName, out List<int> certFileIds, QueryType type)
        {

            string retVal = "Failure";
            certFileIds = new List<int>();
            CommOps.ResponseResult result = null;
            string queryType = type.GetDescription();

            //try
            //    {
            //get order information

            OrderDetailView ordDetail = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            CommOps comm = new CommOps();

            //this is production
            CommOps.TrueWorkCommType commType = CommOps.TrueWorkCommType.QueryReverify;
            if (type == QueryType.Credentials)
            {
                commType = CommOps.TrueWorkCommType.QueryCredentials;
            }
            else if (ordDetail.TrueWorkOrderType == "Credentials")
            {
                commType = CommOps.TrueWorkCommType.QueryReverifyCredentials;
            }
                
            result = comm.postRequest(dbConn, null, ordDetail.OrderNumber, orderRequestId, UserName, false, commType, false, ordDetail.TrueWorkOrderNumber);


            //********************************************
            //Process Order Result
            //********************************************

            if (result.Status == "Done")
            {

                bool fileAttError = false;

                //save the cert files to linked docs          
                if (result.Files.Count > 0)
                {
                    DocumentOps dOp = new DocumentOps();
                    PDFOps pOp = new PDFOps();

                    foreach (ReportFile file in result.Files)
                    {
                        string[] fileParts = file.FileName.Split("_"[0]);
                        string displayName = fileParts[fileParts.Length - 1];
                        string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file.FileName;
                        string VendorDocFilePath = RepositoryPath + "Documents\\TrueworkDocuments\\" + file.FileName;

                        //TODO: this needs to be changed so that we don't have to have two copies of the cert
                        if (File.Exists(UploadFilePath))
                        {
                            File.Delete(UploadFilePath);
                        }
                        File.Copy(VendorDocFilePath, UploadFilePath);
                        logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                        int? pageCount = pOp.getPageCount(UploadFilePath);

                        UploadResult res = new UploadResult();
                        try
                        {
                            res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file.FileName,
                                DocumentOps.DocumentType.TrueWorkDownload, res, true, UploadFilePath, pageCount, UserName);
                            int docId = res.DocumentId;
                            string VendorReferenceNum = file.DUReferenceNumber;

                            //update document record
                            if (type == QueryType.Credentials)
                            {
                                int VendorId = dbConn.Where<Vendor>(q => q.Name == "TrueWork").FirstOrDefault().Id;

                                dbConn.Update<Document>(
                                     set: "TrueWorkReportId = {0}, TrueWorkRequestId = {1}, VendorId = {2}, VendorReferenceNum = {3}".Params(file.ReportId, file.RequestId, VendorId, VendorReferenceNum),
                                     where: "Id = {0}".Params(docId));
                            }
                            else if (type == QueryType.Reverify)
                            {
                                dbConn.Update<Document>(
                                     set: "VendorReferenceNum = {0} ".Params(file.ReportId),
                                     where: "Id = {0}".Params(docId));
                            }


                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error Saving Document " + UploadFilePath, ex);
                            res.Result = false;
                        }

                        if (!res.Result)
                        {
                            logger.Error("Error Attaching TrueWork Document to Order " + ordDetail.OrderNumber + " " + file,
                                new Exception("Error Attaching TrueWork Document to Order"));
                            fileAttError = true;
                        }
                        else
                        {
                            certFileIds.Add(res.DocumentId);
                        }

                    }

                    if (fileAttError)
                    {
                        //could not attach files for some reason
                        result.ResultMessage = "Error: Cannnot Attach Downloaded Certification File(s) to Order";
                    }
                    else
                    {
                        retVal = "Success";
                        logger.Info("TrueWork "+ type.GetDescription() + " order has been completed for order " + ordDetail.OrderNumber + ": TrueWorkOrderNumber " + result.TrueworkOrderId);
                    }

                    //update order
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.TrueWorkOrderNumber = result.TrueworkOrderId;
                    order.TrueWorkOrderStatus = retVal;

                    //write to activity log that order was processed
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                    oa.ActivityNote = type.GetDescription() + " Order Received from TrueWork; TrueWork Order Number " + result.TrueworkOrderId;

                    using (IDbTransaction tr = dbConn.BeginTransaction())
                    {
                        
                        if (type == QueryType.Credentials)
                        {
                            EmailOps eop = new EmailOps();
                            eop.sendTemplateEmail(dbConn, "TrueWork Credentials Results Received", orderRequestId, null, true, false, null, false, tr);

                            if (oa.CurrOrderStatusId == 24)  //if it is still in vendor pipeline
                            {
                                oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                                oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                                oa.CurrOrderStatusId = 24; //vendor parent status
                                oa.CurrOrderSubStatusId = 25; //vendor verified
                                oa.ActivityNote += "; Move to Vendor Verified Status";
                            }
                        }

                        if(order.TrueWorkToastAlertUserName != null)
                        {
                            ToastAlertOps top = new ToastAlertOps();
                            top.createAlert(dbConn, null, order.TrueWorkToastAlertUserName, orderRequestId, null, "TrueWork " + type.GetDescription() + " Results Recieved");
                        }

                        dbConn.UpdateOnly(order, q => new { q.TrueWorkOrderNumber, q.TrueWorkOrderStatus }, r => r.Id == orderRequestId);
                        dbConn.Insert<OrderActivity>(oa);

                        tr.Commit();
                    }

                }
                else
                {
                    result.ResultMessage += " Error: No Certification Files Found";
                }


            }
            else if (result.Status == "Cancelled")
            {
                //expired or otherwise goofed up
                retVal = result.Status;

                //update order
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.TrueWorkOrderNumber = result.TrueworkOrderId;
                order.TrueWorkOrderStatus = retVal;

                //write to activity log that order was cancelled
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = type.GetDescription() + " TrueWork Order Cancelled";

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {

                    if (type == QueryType.Credentials)
                    {
                        EmailOps eop = new EmailOps();
                        eop.sendTemplateEmail(dbConn, "TrueWork Credentials Order Cancelled", orderRequestId, null, true, false, null, false, tr);

                        if (oa.CurrOrderStatusId == 24)  //if it is still in vendor pipeline
                        {
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 1; //new
                            oa.CurrOrderSubStatusId = null;
                            oa.ActivityNote += "; Move to New Status";
                        }

                    }

                    dbConn.UpdateOnly(order, q => new { q.TrueWorkOrderNumber, q.TrueWorkOrderStatus }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);

                    tr.Commit();
                }

            }

            else if (result.Status == "Processing")
            {
                //no files retrieved yet
                retVal = result.Status;
            }
                       

            return retVal;

        }

        public Request createInstantOrder(IDbConnection dbConn, int orderRequestId, OrderType orderType, VerificationType verifType, bool IsDay1, bool IsTestMode)
        {

            Request retVal = new Request();

            OrderRequest voeOrder = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
            
            if ((int)orderType == 1)
            {
                //this is verbal
                retVal.type = TrueWorkSchema.RequestType.employment.GetDescription();
            }
            else
            {
                //must be written
                retVal.type = TrueWorkSchema.RequestType.employment_income.GetDescription();
            }

            retVal.permissible_purpose = PermissiblePurpose.credit_application.GetDescription();

            if (IsDay1)
            {
                retVal.use_case = UseCase.preapproval.GetDescription();
            }
            else
            {
                retVal.use_case = UseCase.mortgage.GetDescription();
            }


            Request.RequestParamaters reqParam = new Request.RequestParamaters();
            if (verifType == VerificationType.Current)
            {
                reqParam.employer_filter = EmployerFilter.current_employer.GetDescription();
            }
            else
            {
                reqParam.employer_filter = EmployerFilter.all_employers.GetDescription();
            }

            Request.VerificationMethods verifMethods = new Request.VerificationMethods();
            verifMethods.instant = new Request.MethodEnabled { enabled = true };
            verifMethods.credentials = new Request.MethodEnabled { enabled = false };
            verifMethods.smart_outreach = new Request.MethodEnabled { enabled = false };

            reqParam.verification_methods = verifMethods;

            retVal.request_parameters = reqParam;

            retVal.loan_id = voeOrder.LoanNumber;

            BorrowerName borr = splitBorrowerName(voeOrder.BorrowerFullName, false);

            Request.Target target = new Request.Target
            {
                first_name = borr.FirstName,
                last_name = borr.LastName,
                social_security_number = voeOrder.BorrowerSSN,
                date_of_birth = (voeOrder.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("yyyy-MM-dd")
            };

            retVal.target = target;

            return retVal;


        }

        public Request createCredentialsOrder(IDbConnection dbConn, int orderRequestId, OrderType orderType, bool IsTestMode)
        {

            Request retVal = new Request();

            OrderRequest voeOrder = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            if (isNull(voeOrder.BorrowerEmail,"") == "")
            {
                throw new Exception("Email Address Missing");
            }

            if ((int)orderType == 1)
            {
                //this is verbal
                retVal.type = TrueWorkSchema.RequestType.employment.GetDescription();
            }
            else
            {
                //must be written
                retVal.type = TrueWorkSchema.RequestType.employment_income.GetDescription();
            }

            retVal.permissible_purpose = PermissiblePurpose.credit_application.GetDescription();

            retVal.use_case = UseCase.mortgage.GetDescription();

            Request.RequestParamaters reqParam = new Request.RequestParamaters();
            reqParam.employer_filter = EmployerFilter.target_employer.GetDescription();

            Request.VerificationMethods verifMethods = new Request.VerificationMethods();
            verifMethods.instant = new Request.MethodEnabled { enabled = false };
            verifMethods.credentials = new Request.MethodEnabled { enabled = true };
            verifMethods.smart_outreach = new Request.MethodEnabled { enabled = false };

            reqParam.verification_methods = verifMethods;

            retVal.request_parameters = reqParam;

            retVal.loan_id = voeOrder.LoanNumber;

            BorrowerName borr = splitBorrowerName(voeOrder.BorrowerFullName, false);

            Request.Company company = new Request.Company
            {
                name = voeOrder.EncEmployerName
            };

            Request.Target target = new Request.Target
            {
                first_name = borr.FirstName,
                last_name = borr.LastName,
                social_security_number = voeOrder.BorrowerSSN,
                contact_email = voeOrder.BorrowerEmail,
                date_of_birth = (voeOrder.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("yyyy-MM-dd"),

                company = company
            };

            //Request.Target target = new Request.Target
            //{
            //    first_name = borr.FirstName,
            //    last_name = borr.LastName,
            //    social_security_number = "000-00-0000",
            //    contact_email = "MSwinehart@firsthome.com",
            //    date_of_birth = (voeOrder.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("yyyy-MM-dd"),

            //    company = company
            //};

            retVal.target = target;

            return retVal;


        }

        public Request createReverifyOrder(string VendorReportId)
        {

            Request retVal = new Request();

            retVal.report_id = VendorReportId;

            return retVal;

        }

        public void autoSubmitOrdersToTrueWork()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTrueWorkOrderView> twOrders = dbConn.Select<AutoTrueWorkOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting " + twOrders.Count.ToString() + " orders to TrueWork");

                foreach (AutoTrueWorkOrderView twOrder in twOrders)
                {
                    try
                    {

                        Dictionary<string, object> prms = new Dictionary<string, object> { };
                        prms.Add("OrderRequestId", twOrder.OrderRequestId);

                        List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);
                        string result = "Success";

                        //this will not repull orders that have a success for the day 1
                        if (day1OrderId.Count == 0)
                        {
                                                       
                            List<int> certIds = new List<int>() { };
                            if ((twOrder.TrueWorkOrderStatus == "Cancelled" && twOrder.TrueWorkOrderType == "Credentials") || 
                                (twOrder.XactusOrderStatus == "Failure" && twOrder.XactusOrderType == "Experian") ||
                                twOrder.ExcludeTWCredentials == true ||
                                isNullInt(twOrder.CurrentCount,0) > 1)
                            {
                                result = submitNewInstantOrder(dbConn, twOrder.OrderRequestId, "voesystem", TrueWorkServiceTestMode, out certIds,
                                    twOrder.OrderTypeId, twOrder.EncEmploymentStatus, false, null, null, TrueWorkTestResultMessage);
                            }
                            else
                            {
                                result = submitNewCredentialsOrder(dbConn, twOrder.OrderRequestId, "voesystem", TrueWorkServiceTestMode, out certIds,
                                twOrder.OrderTypeId, false, TrueWorkTestResultMessage);


                            }
                        }
                        else
                        {
                            result = "Skipped";
                        }

                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, twOrder.OrderRequestId, "voesystem", false);

                        if (result.StartsWith("Success"))
                        {

                            //yay it worked
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //vendor parent status
                            oa.CurrOrderSubStatusId = 25; //vendor verified
                            oa.ActivityNote = "Move to Vendor Verified Status";

                            dbConn.Insert<OrderActivity>(oa);
                        }
                        
                        else if (result == "Skipped") //day 1 exists
                        {
                            //just update the Equifax status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "TrueWorkOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(twOrder.OrderRequestId));
                            logger.Info("Skipping TrueWork Pull for " + twOrder.OrderRequestId.ToString());

                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //vendor parent status
                            oa.CurrOrderSubStatusId = 25; //vendor verified
                            oa.ActivityNote = "Move to Vendor Verified Status";

                            dbConn.Insert<OrderActivity>(oa);

                        }
                        if (result.StartsWith("Processing"))
                        {

                            //yay it worked - credentials
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //vendor parent status
                            oa.CurrOrderSubStatusId = 32; //pending vendor
                            oa.ActivityNote = "Move to Pending Vendor Status";

                            dbConn.Insert<OrderActivity>(oa);
                        }


                        else
                        {
                            //it failed
                                                      
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto TrueWork Order Error: ", ex);
                    }

                }


            }
        }

        public void autoQueryOpenOrderStatus()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                 ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                 true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTrueWorkOpenOrderView> twOrders = dbConn.Select<AutoTrueWorkOpenOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Querying " + twOrders.Count.ToString() + " open order on TrueWork");

                foreach (AutoTrueWorkOpenOrderView twOrder in twOrders)
                {
                    try
                    {
                        
                        List<int> certIds = new List<int>() { };
                        if (twOrder.RequestTypeId == 3)
                        {
                            queryOrderStatus(dbConn, twOrder.OrderRequestId, "voesystem", out certIds, QueryType.Reverify);
                        }
                        else
                        {
                            queryOrderStatus(dbConn, twOrder.OrderRequestId, "voesystem", out certIds, QueryType.Credentials);
                        }
                       
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto TrueWork Query Order Error: ", ex);
                    }
                
                }
            }

        }

        public void autoReverifyOrdersToTrueWork()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTrueWorkReverifyView> twOrders = dbConn.Select<AutoTrueWorkReverifyView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting Reverif " + twOrders.Count.ToString() + " orders to TrueWork");

                foreach (AutoTrueWorkReverifyView twOrder in twOrders)
                {
                    try
                    {

                        List<int> certIds = new List<int>() { };
                        string result = submitNewReverifyOrder(dbConn, twOrder.OrderRequestId, "voesystem", TrueWorkServiceTestMode, twOrder.VendorReportId, twOrder.VendorRequestId, false, TrueWorkTestResultMessage);

                        //really don't care what the status was, still moving to the TWN final queue
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, twOrder.OrderRequestId, "voesystem", false);
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
                        logger.Error("Auto TrueWork Order Error: ", ex);
                    }

                }


            }
        }

    }
}
