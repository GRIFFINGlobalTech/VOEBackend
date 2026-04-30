using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VOEBackend.Equifax.EquifaxSchema;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOEBackend.Equifax.Business
{
    public class OrderOps : BaseClass
    {

        public string submitNewOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {

            string retVal = "Error creating Equifax order.";

            try
            {
                //get order information
                REQUEST_GROUP request = createOrder(dbConn, orderRequestId, UserName, TestMode);
                CommOps.ResponseResult res = null;

                string orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                if (!TestMode)
                {
                    //this is production
                    CommOps comm = new CommOps();
                    res = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, false, false, false);
                }
                else
                {
                    //this is test           
                    logger.Info("Equifax Service Test Mode for Order " + orderNumber);
                    res = new CommOps.ResponseResult
                    {
                        EquifaxOrderId = "EQ" + orderRequestId.ToString(),
                        Status = "OK"
                    };
                }

                if (res.Status == "OK")
                {
                    retVal = "Equifax order has been created";

                    logger.Info("Equifax order has been created for order " + orderNumber + ": EquifaxOrderNumber " + res.EquifaxOrderId);

                    //update equifax order number,  is subcontracted value
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.EquifaxOrderNumber = res.EquifaxOrderId;
                    order.IsSubcontracted = true;
                    order.EquifaxOrderStatus = "Accepted";
                    order.EquifaxOrderType = request.REQUEST.REQUEST_DATA.VOI_REQUEST.VOI_REQUEST_DATA.VOIReportTypeOtherDescription;

                    //write to activity log that order was subcontracted
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                    oa.ActivityNote = "Order Sent to Equifax; Equifax Order Number " + res.EquifaxOrderId;

                    using (IDbTransaction tr = dbConn.BeginTransaction())
                    {
                        dbConn.UpdateOnly(order, q => new { q.EquifaxOrderNumber, q.IsSubcontracted, q.EquifaxOrderStatus }, r => r.Id == orderRequestId);
                        dbConn.Insert<OrderActivity>(oa);
                        tr.Commit();
                    }

                }
                else
                {
                    retVal = "Error - Equifax order NOT created: " + res.ResultMessage;
                    logger.Error("Equifax order has been not been created for order " + orderNumber + ": " + res.ResultMessage,
                        new Exception("Error Creating Equifax Order"));
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error Submitting Equifax Order", ex);
            }

            return retVal;


        }

        public string submitNewInstantOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
            string salaryKey, string employerCode, int orderTypeId, string verifType, bool IsDay1, string firstName = null, string lastName = null, string TestResultMessage = null)
        {

            string retVal = "Failure";
            CommOps.ResponseResult result = null;
            string orderNumber = String.Empty;
            bool fileAttError = false;
            DateTime? dataDate = null;
            String referenceNumber = null;
            certFileIds = new List<int>();

            try
            {
                //get order information
                REQUEST_GROUP request = createInstantOrder(dbConn, orderRequestId, salaryKey, employerCode, (OrderType)orderTypeId,
                    (VerificationType)Enum.Parse(typeof(VerificationType), verifType), firstName, lastName);

                orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                CommOps comm = new CommOps();

                if (TestMode)
                {

                    //this is test           
                    logger.Info("Equifax Service Test Mode for Instant Order " + orderNumber);

                    //write file
                    string EquifaxReqType = request.REQUEST.REQUEST_DATA.VOI_REQUEST.VOI_REQUEST_DATA.VOIReportRequestActionType;
                    string reqestString = comm.serializeRequest(request);
                    string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "EquifaxInstantOrder", EquifaxReqType);

                    if (TestResultMessage == "Success")
                    {
                        retVal = "Success";
                        result = new CommOps.ResponseResult
                        {
                            EquifaxOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Done",
                            Files = new List<string>() {
                                Path.GetFileName(TestPDFFilePath)
                            },
                            xmlCDATA = File.ReadAllText(TestCDataFilePath),
                            ResultMessage = "Success"
                        };
                    }
                    else if (TestResultMessage == "Salary Key")
                    {
                        //simulate salary key
                        result = new CommOps.ResponseResult
                        {
                            Status = "Done",
                            ResultMessage = "Salary Key Required",
                            Files = new List<string>() { }
                        };

                    }
                    else if (TestResultMessage == "Employer Code")
                    {
                        //simulate employer code
                        result = new CommOps.ResponseResult
                        {
                            Status = "Done",
                            ResultMessage = "Employer Code Required",
                            Files = new List<string>() { }
                        };

                    }
                    else if (TestResultMessage == "Employer Blocked")
                    {
                        //simulate employer code
                        result = new CommOps.ResponseResult
                        {
                            Status = "Error",
                            ResultMessage = "Employer is Blocked",
                            Files = new List<string>() { }
                        };

                    }
                    else if (TestResultMessage == "Name Mismatch")
                    {
                        //simulate name mismatch
                        result = new CommOps.ResponseResult
                        {
                            Status = "Error",
                            ResultMessage = "Name Not Matched",
                            Files = new List<string>() { }
                        };

                    }
                    else if (TestResultMessage == "Multiple Individuals")
                    {
                        //simulate name mismatch
                        result = new CommOps.ResponseResult
                        {
                            Status = "Error",
                            ResultMessage = "Multiple Individuals with this SSN",
                            Files = new List<string>() { }
                        };

                    }
                    else
                    {
                        //simulate no hit
                        result = new CommOps.ResponseResult
                        {
                            EquifaxOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Error",
                            ResultMessage = "Employee not found in database"
                        };

                    }


                }
                else
                {
                    //this is production
                    result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, true, false, IsDay1);
                }


                //********************************************
                //Process Order Result
                //********************************************

                if (result.Status == "Done")
                {

                    //save the cert files to linked docs          
                    if (result.Files.Count > 0)
                    {
                        DocumentOps dOp = new DocumentOps();
                        PDFOps pOp = new PDFOps();

                        foreach (string file in result.Files)
                        {
                            string[] fileParts = file.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file;
                            string EquifaxDocFilePath = RepositoryPath + "Documents\\EquifaxDocuments\\" + file;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                            if (File.Exists(UploadFilePath))
                            {
                                File.Delete(UploadFilePath);
                            }
                            File.Copy(EquifaxDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            try
                            {
                                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                    DocumentOps.DocumentType.EquifaxDownload, res, true, UploadFilePath, pageCount, UserName);
                                int docId = res.DocumentId;

                                //try to get the reference number but only add to document record since they decided they needed manual review since they cannot ensure that the correct employer is here
                                referenceNumber = null;
                                if (result.xmlCDATA != null)
                                {
                                    referenceNumber = extractReferenceNumFromXML(result.xmlCDATA);
                                }

                                //try to get the datadate but only add to document record since they decided they needed manual review since they cannot ensure that the correct employer is here
                                dataDate = pOp.extractEquifaxDataDate(EquifaxDocFilePath);

                                //update document record
                                dbConn.Update<Document>(
                                 set: "VendorReferenceNum = {0}, VendorDataDate = {1} ".Params(referenceNumber, dataDate),
                                 where: "Id = {0}".Params(docId));

                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error Saving Document " + UploadFilePath, ex);
                                res.Result = false;
                            }

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Equifax Document to Order " + orderNumber + " " + file,
                                    new Exception("Error Attaching Equifax Document to Order"));
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
                            logger.Info("Equifax instant order has been completed for order " + orderNumber + ": EquifaxOrderNumber " + result.EquifaxOrderId);
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


                //update equifax order number, status, vendor data date, first and last names
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.EquifaxOrderNumber = result.EquifaxOrderId;
                order.EquifaxOrderStatus = retVal;  //will be success or failure
                order.EquifaxOrderType = request.REQUEST.REQUEST_DATA.VOI_REQUEST.VOI_REQUEST_DATA.VOIReportTypeOtherDescription;
                order.EquifaxFirstName = request.REQUEST.REQUEST_DATA.VOI_REQUEST.LOAN_APPLICATION.BORROWER._FirstName;
                order.EquifaxLastName = request.REQUEST.REQUEST_DATA.VOI_REQUEST.LOAN_APPLICATION.BORROWER._LastName;

                //write to activity log that order was processed
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = "Instant Order Sent to Equifax; Equifax Order Number " + result.EquifaxOrderId;
                if (result.ResultMessage.Contains("Error"))
                {
                    oa.ActivityNote += "The following errors ocurred during instant order processing: " + result.ResultMessage;

                    if (result.ResultMessage.ToLower().Contains("salary key"))
                    {
                        retVal += ": Salary Key Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("employer"))
                    {
                        retVal += ": Employer Code Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("name not matched"))
                    {
                        retVal += ": Borrower Name Mismatch";
                    }
                    else if (result.ResultMessage.ToLower().Contains("multiple individuals"))
                    {
                        retVal += ": Multiple Individuals with this SSN";
                    }
                }
                else
                {
                    oa.ActivityNote += "Instant Verification Received";
                }

                //keep changing mind on this
                if (order.RequestTypeId == 6)  //instant request
                {
                    oa.VendorDataDate = dataDate;
                    oa.VendorId = dbConn.Where<Vendor>(q => q.Name == "Work#").FirstOrDefault().Id;
                    oa.VendorReferenceNum = referenceNumber;
                }

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {
                    dbConn.UpdateOnly(order, q => new { q.EquifaxOrderNumber, q.EquifaxOrderStatus, q.EquifaxOrderType, q.EquifaxFirstName, q.EquifaxLastName }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);
                    tr.Commit();
                }

            }
            catch (Exception ex)
            {
                //some other type of error 
                logger.Error("Equifax instant order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            }

            return retVal;


        }

        public string submitReverifyInstantOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
            string TestResultMessage = null)
        {

            string retVal = "Failure";
            CommOps.ResponseResult result = null;
            string orderNumber = String.Empty;
            bool fileAttError = false;
            DateTime? dataDate = null;
            String referenceNumber = null;
            certFileIds = new List<int>();

            try
            {
                //get order information
                REQUEST_GROUP request = createInstantReverifyOrder(dbConn, orderRequestId);

                orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                CommOps comm = new CommOps();

                if (TestMode)
                {

                    //this is test           
                    logger.Info("Equifax Service Test Mode for Reverify Instant Order " + orderNumber);

                    //write file
                    string reqestString = comm.serializeRequest(request);
                    string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "EquifaxInstantOrderReverify", "");

                    if (TestResultMessage == "Success")
                    {
                        retVal = "Success";
                        result = new CommOps.ResponseResult
                        {
                            EquifaxOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Done",
                            Files = new List<string>() {
                                Path.GetFileName(TestPDFFilePath)
                            },
                            ResultMessage = "Success"
                        };
                    }
                    else
                    {
                        //simulate no failure
                        result = new CommOps.ResponseResult
                        {
                            EquifaxOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Error",
                            ResultMessage = "Initial Equifax Order Not Found"
                        };

                    }


                }
                else
                {
                    //this is production
                    result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, true, true, false);
                }


                //********************************************
                //Process Order Result
                //********************************************

                if (result.Status == "Done")
                {

                    //save the cert files to linked docs          
                    if (result.Files.Count > 0)
                    {
                        DocumentOps dOp = new DocumentOps();
                        PDFOps pOp = new PDFOps();

                        foreach (string file in result.Files)
                        {
                            string[] fileParts = file.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file;
                            string EquifaxDocFilePath = RepositoryPath + "Documents\\EquifaxDocuments\\" + file;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                            if (File.Exists(UploadFilePath))
                            {
                                File.Delete(UploadFilePath);
                            }
                            File.Copy(EquifaxDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                DocumentOps.DocumentType.EquifaxDownload, res, true, UploadFilePath, pageCount, UserName);

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Equifax Document to Order " + orderNumber + " " + file,
                                    new Exception("Error Attaching Equifax Document to Order"));
                                fileAttError = true;
                            }
                            else
                            {
                                certFileIds.Add(res.DocumentId);
                            }

                            //try to get data date for first document - no they decided they needed manual review since they cannot ensure that the correct employer is here
                            //try
                            //{
                            //    if (dataDate == null)
                            //    {
                            //        dataDate = pOp.extractEquifaxDataDate(EquifaxDocFilePath);
                            //    }
                            //}
                            //catch { }

                            //try to get the reference number for the first document - no they decided they needed manual review since they cannot ensure that the correct employer is here
                            //if (referenceNumber == null && result.xmlCDATA != null)
                            //{
                            //    referenceNumber = extractReferenceNumFromXML(result.xmlCDATA);
                            //}

                        }

                        if (fileAttError)
                        {
                            //could not attach files for some reason
                            result.ResultMessage = "Error: Cannnot Attach Downloaded Certification File(s) to Order";
                        }
                        else
                        {
                            retVal = "Success";
                            logger.Info("Equifax instant order has been completed for order " + orderNumber + ": EquifaxOrderNumber " + result.EquifaxOrderId);
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

                //write to activity log that order was processed
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = "Instant Reverify Order Sent to Equifax; Equifax Order Number " + result.EquifaxOrderId;
                if (result.ResultMessage.Contains("Error"))
                {
                    oa.ActivityNote += "The following errors ocurred during instant reverify order processing: " + result.ResultMessage;

                    if (result.ResultMessage.ToLower().Contains("salary key"))
                    {
                        retVal += ": Salary Key Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("employer"))
                    {
                        retVal += ": Employer Code Required";
                    }

                }
                else
                {
                    oa.ActivityNote += "Instant ReVerification Received";
                }

                //nope changed their minds on this too
                //oa.VendorDataDate = dataDate;
                //oa.VendorId = dbConn.Where<Vendor>(q => q.Name == "Work#").FirstOrDefault().Id;
                //oa.VendorReferenceNum = referenceNumber;

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {
                    dbConn.Insert<OrderActivity>(oa);
                    tr.Commit();
                }

                int OrderActivityId = (int)dbConn.GetLastInsertId();
                oOp.addOrderEvent(dbConn, OrderActivityId, "Equifax Reverify");


            }
            catch (Exception ex)
            {
                //some other type of error 
                logger.Error("Equifax instant reverify order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            }

            return retVal;


        }

        public string queryOrderStatus(IDbConnection dbConn, int orderRequestId, string UserName)
        {
            CommOps.ResponseResult result = new CommOps.ResponseResult() { Status = "Unknown" };

            try
            {

                REQUEST_GROUP request = createStatusRequest(dbConn, orderRequestId, StatusRequestType.StatusQuery);

                string orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                CommOps comm = new CommOps();
                result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, false, false, false);

                if (result.Status == "Completed")
                {
                    //now need to request voe download
                    logger.Info("Equifax Order Completed for Order: " + orderNumber);

                    REQUEST_GROUP retrieveRequest = createStatusRequest(dbConn, orderRequestId, StatusRequestType.RetrieveVOE);
                    result = comm.postRequest(dbConn, retrieveRequest, orderNumber, orderRequestId, UserName, false, false, false);

                    if (result.Status == "Done")
                    {

                        logger.Info("Equifax Order Done for Order: " + orderNumber);

                        //save the files to linked docs
                        bool fileAttError = false;
                        DateTime? dataDate = null;
                        if (result.Files.Count > 0)
                        {
                            DocumentOps dOp = new DocumentOps();
                            PDFOps pOp = new PDFOps();

                            foreach (string file in result.Files)
                            {
                                string[] fileParts = file.Split("_"[0]);
                                string displayName = fileParts[fileParts.Length - 1];
                                string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file;
                                string EquifaxDocFilePath = RepositoryPath + "Documents\\EquifaxDocuments\\" + file;

                                //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                                File.Copy(EquifaxDocFilePath, UploadFilePath);
                                logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                                int? pageCount = pOp.getPageCount(UploadFilePath);

                                UploadResult res = new UploadResult();
                                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                    DocumentOps.DocumentType.EquifaxDownload, res, true, UploadFilePath, pageCount, UserName);

                                if (!res.Result)
                                {
                                    logger.Error("Error Attaching Equifax Document to Order " + orderNumber + " " + file,
                                        new Exception("Error Attaching Equifax Document to Order"));
                                    fileAttError = true;
                                }

                                //try to get data date
                                dataDate = pOp.extractEquifaxDataDate(EquifaxDocFilePath);

                            }

                            if (fileAttError)
                            {
                                result.ResultMessage = "Error: Cannnot Attach Downloaded File(s) to Order";
                            }
                        }


                        //update eq order status
                        OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                        order.EquifaxOrderStatus = result.Status;
                        order.IsSubcontracted = false;
                        dbConn.UpdateOnly(order, q => new { q.EquifaxOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                        //add order activity
                        VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, "voesystem", false);

                        oa.ActivityNote = "Equifax Order Complete. ";
                        oa.VendorDataDate = dataDate;

                        if (result.ResultMessage.StartsWith("Error"))
                        {
                            oa.ActivityNote += "The following errors ocurred during order processing: " + result.ResultMessage;
                        }

                        dbConn.Insert<OrderActivity>(oa);

                        //notify VOES to do something;
                        EmailOps eOp = new EmailOps();
                        eOp.sendTemplateEmail(dbConn, "Equifax Order Complete", orderRequestId, null, false, false, order.RequestTypeId, false);

                        //notify accounting
                        eOp.sendTemplateEmail(dbConn, "Subcontracted Order Complete", orderRequestId, null, false, false, order.RequestTypeId, false);
                    }
                    else
                    {
                        logger.Error("Error Retrieving Equifax Order Documents for OrderRequestId = " + orderRequestId + ": " + result.ResultMessage,
                            new Exception("Error Retrieving Equifax Order Documents for Order"));
                    }

                }
                else if (result.Status == "Canceled")
                {

                    logger.Info("Equifax Order Canceled for Order: " + orderNumber);

                    //update eq order status
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.EquifaxOrderStatus = result.Status;
                    order.IsSubcontracted = false;
                    dbConn.UpdateOnly(order, q => new { q.EquifaxOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                    //notify VOES to do something;
                    EmailOps eOp = new EmailOps();
                    eOp.sendTemplateEmail(dbConn, "Equifax Order Cancelled", orderRequestId, null, false, false, order.RequestTypeId, false);

                }
                else if (result.Status == "Error")
                {
                    logger.Error("Equifax Status Query Error for Order " + orderNumber + ": " + result.ResultMessage,
                        new Exception("Error Querying Status of Equifax Order"));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error Querying Equifax Order Status for OrderRequestId = " + orderRequestId, ex);
            }

            return result.Status;

        }

        public REQUEST_GROUP createOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {

            OrderDetailView voeOrder = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();


            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            //header setup
            requestGroup.MISMOVersionID = "2.3.1";

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            //Equifax order parameters
            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.VOIRequest voiRequest = new REQUEST_GROUP.Request.RequestData.VOIRequest();
            voiRequest.LenderCaseIdentifier = voeOrder.LoanNumber;
            voiRequest.SpecialInstructionsDescription = voeOrder.OrderType;

            REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData voiRequestData = new REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData();

            voiRequestData.VOIReportTypeOtherDescription = "dvvoe";
            voiRequestData.VOIRequestType = "Individual";
            voiRequestData.VOIReportRequestActionType = "Submit";
            voiRequestData.VOIReportType = "Other";
            voiRequestData.BorrowerID = "Borrower";
            voiRequestData.VOIRequestID = "VOERequest1";

            voiRequest.VOI_REQUEST_DATA = voiRequestData;

            //borrower information
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_();

            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(voeOrder.BorrowerFullName, false);
            OrderAddress borrAddress = li.splitOrderAddress(voeOrder.BorrowerAddress);

            borrower.BorrowerID = "Borrower";
            borrower._FirstName = borr.FirstName;
            borrower._LastName = borr.LastName;
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = voeOrder.BorrowerSSN.Replace("-", "");

            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_.Residence residence = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_.Residence();
            residence._StreetAddress = borrAddress.Street;
            residence._City = borrAddress.City;
            residence._State = borrAddress.State;
            residence._PostalCode = borrAddress.Zip;
            residence.BorrowerResidencyType = "Current";
            borrower._RESIDENCE = residence;

            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_.Employer_ employer = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_.Employer_();
            employer._Name = voeOrder.EncEmployerName;
            OrderAddress empAddress = li.splitOrderAddress(voeOrder.EncEmployerAddress);
            employer._StreetAddress = empAddress.Street;
            employer._City = empAddress.City;
            employer._State = empAddress.State;
            employer._PostalCode = empAddress.Zip;
            employer.EmploymentBorrowerSelfEmployedIndicator = "N";
            employer._TelephoneNumber = voeOrder.EncEmployerPhone;
            employer.EmploymentPositionDescription = string.Empty;
            employer.PreviousEmploymentStartDate = string.Empty;
            employer.PreviousEmploymentEndDate = string.Empty;

            borrower.EMPLOYER = employer;
            loanApplication.BORROWER = borrower;

            voiRequest.LOAN_APPLICATION = loanApplication;

            //add forms.  need the borrower authorization form for all orders.
            //consolidate all borrower auth forms

            string FilePathName;
            PDFOps pdfOp = new PDFOps();

            if (TestMode)
            {
                FilePathName = ConfigurationManager.AppSettings["TestPDFFilePath"];
            }
            else
            {

                List<DocumentOrderView> docs = dbConn.Where<DocumentOrderView>(q => q.DocumentTypeName.StartsWith("Encompass")
                    && q.EncDocumentName.Contains("Borrower")
                    && q.EncDocumentName.Contains("Certification")
                    && q.OrderRequestId == orderRequestId);

                DocumentOps dOp = new DocumentOps();

                if (docs.Count == 0)
                {

                    //refresh the encompass document list - baseURL is used to build document URL - we don't need that here
                    string msg = string.Empty;
                    List<DocumentListItem> docItems = dOp.refreshEncDocumentList(dbConn, orderRequestId, "", UserName, ref msg);

                    if (docItems.Count > 0)
                    {
                        //need to requery here since we need the updated docs object later on
                        docs = dbConn.Where<DocumentOrderView>(q => q.DocumentTypeName == "EncompassCoud"
                            && q.EncDocumentName.Contains("Borrower")
                            && q.EncDocumentName.Contains("Certification")
                            && q.OrderRequestId == orderRequestId);

                    }
                    else
                    {

                        //there really are no auth forms
                        throw new Exception("No Borrower Authorization Forms Found.");
                    }

                }

                List<string> docsToSend = new List<string>() { };

                string docDLLocation = ConfigurationManager.AppSettings["EncDocumentDLLocation"];


                //make sure these are in pdf format prior to consolidation
                foreach (DocumentOrderView doc in docs)
                {

                    //if this is a cloud doc, see if we need to download it
                    if (doc.DocumentTypeName == "EncompassCloud" && !File.Exists(docDLLocation + doc.UniqueFileName))
                    {
                        dOp.downloadEncLoanAttachment(dbConn, isNullInt(orderRequestId, 0), doc.UniqueFileName, voeOrder.LoanNumber);
                    }

                    docsToSend.Add(
                        pdfOp.convertToPDF(docDLLocation + doc.UniqueFileName));
                }

                //consolidate docs
                FilePathName = RepositoryPath + "Documents\\Consolidated\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + voeOrder.LoanNumber + "_ADAttachments.pdf";
                pdfOp.consolidatePDFs(docsToSend, FilePathName);
            }

            //convert consolidated doc to tiff
            //FilePathName = pdfOp.convertToTIFF(FilePathName);

            REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_ extension = new REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_();
            REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection extensionSection = new REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection();
            REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection.ExtensionSectionData extensionSectionData = new REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection.ExtensionSectionData();
            REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection.ExtensionSectionData.EmbeddedFile embeddedFile = new REQUEST_GROUP.Request.RequestData.VOIRequest.Extension_.ExtensionSection.ExtensionSectionData.EmbeddedFile();

            embeddedFile._Type = "pdf";
            embeddedFile._EncodingType = "Base64";
            embeddedFile.MIMEType = "pdf";
            embeddedFile._Name = (new FileInfo(FilePathName)).Name;

            Byte[] fileBytes = File.ReadAllBytes(FilePathName);
            embeddedFile.DOCUMENT = Convert.ToBase64String(fileBytes);

            extensionSectionData.EMBEDDED_FILE = embeddedFile;
            extensionSection.EXTENSION_SECTION_DATA = extensionSectionData;
            extension.EXTENSION_SECTION = extensionSection;

            voiRequest.EXTENSION = extension;


            requestData.VOI_REQUEST = voiRequest;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }

        public REQUEST_GROUP createInstantOrder(IDbConnection dbConn, int orderRequestId, string salaryKey, string employerCode, OrderType orderType, VerificationType verifType,
            string firstName, string lastName)
        {

            OrderRequest voeOrder = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            //header setup
            //requestGroup.MISMOVersionID = "2.3.1";

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            REQUEST_GROUP.SubmittingParty.PreferredResponse preferredResponse = new REQUEST_GROUP.SubmittingParty.PreferredResponse();
            preferredResponse._Format = "PDF";

            submittingParty.PREFERRED_RESPONSE = preferredResponse;
            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            if (voeOrder.RequestTypeId == 6)
            {
                //this is a day 1 instant order
                request.LoginAccountIdentifier = ACCOUNTNUMBERDAY1;
                request.LoginAccountPassword = PASSWORDDAY1;
            }
            else
            {
                request.LoginAccountIdentifier = ACCOUNTNUMBER;
                request.LoginAccountPassword = PASSWORD;
            }

            List<REQUEST_GROUP.Request.Key> keys = new List<REQUEST_GROUP.Request.Key>() { };
            REQUEST_GROUP.Request.Key key = new REQUEST_GROUP.Request.Key();
            key._Name = "EmployeeStatusFilter";

            if (verifType.ToString() == "Current")
            {
                key._Value = "A";  //filtering by active employment
            }
            else if (verifType.ToString() == "Prior")
            {
                key._Value = "I";  //filtering by inactive employment
            }
            else
            {
                key._Value = "B";  //all employment
            }
            keys.Add(key);

            if (salaryKey != null)
            {
                REQUEST_GROUP.Request.Key sKey = new REQUEST_GROUP.Request.Key();
                sKey._Name = "EMSSALARYKEY";
                sKey._Value = salaryKey;  //salarykey
                keys.Add(sKey);
            }

            if (employerCode != null)
            {
                REQUEST_GROUP.Request.Key sKey = new REQUEST_GROUP.Request.Key();
                sKey._Name = "EMSEmployerCode";
                sKey._Value = employerCode;  //employerCode
                keys.Add(sKey);
            }


            request.KEY = keys.ToArray();

            //Equifax order parameters
            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.VOIRequest voiRequest = new REQUEST_GROUP.Request.RequestData.VOIRequest();
            voiRequest.LenderCaseIdentifier = voeOrder.LoanNumber;

            REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData voiRequestData = new REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData();

            if ((int)orderType == 1)
            {
                //this is verbal
                voiRequestData.VOIReportTypeOtherDescription = "VOE";
            }
            else
            {
                //must be written
                voiRequestData.VOIReportTypeOtherDescription = "VOI";
            }
            voiRequestData.VOIReportRequestActionType = "Submit";
            voiRequestData.VOIReportType = "Other";

            voiRequest.VOI_REQUEST_DATA = voiRequestData;

            //borrower information
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_();

            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(voeOrder.BorrowerFullName, false);

            borrower._FirstName = isNull(firstName, borr.FirstName);
            borrower._LastName = isNull(lastName, borr.LastName);
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = voeOrder.BorrowerSSN.Replace("-", "");

            loanApplication.BORROWER = borrower;

            voiRequest.LOAN_APPLICATION = loanApplication;

            requestData.VOI_REQUEST = voiRequest;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }

        public REQUEST_GROUP createStatusRequest(IDbConnection dbConn, int orderRequestId, StatusRequestType requestType)
        {

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            OrderRequest voeOrder = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();


            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            REQUEST_GROUP.Request.Key key = new REQUEST_GROUP.Request.Key();
            key._Name = "EMSOrderNumber";
            key._Value = voeOrder.EquifaxOrderNumber;

            request.KEY = new REQUEST_GROUP.Request.Key[1];
            request.KEY[0] = key;

            //Equifax order parameters
            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.VOIRequest voiRequest = new REQUEST_GROUP.Request.RequestData.VOIRequest();
            voiRequest.LenderCaseIdentifier = voeOrder.LoanNumber;

            REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData voiRequestData = new REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData();

            voiRequestData.VOIReportTypeOtherDescription = "dvvoe";
            voiRequestData.VOIRequestType = "Individual";
            if (requestType == StatusRequestType.StatusQuery)
            {
                voiRequestData.VOIReportRequestActionType = "StatusQuery";
            }
            else
            {
                voiRequestData.VOIReportRequestActionType = "Retrieve";
            }
            voiRequestData.VOIReportType = "Other";
            voiRequestData.BorrowerID = "Borrower";
            voiRequestData.VOIReportIdentifier = voeOrder.EquifaxOrderNumber;

            voiRequest.VOI_REQUEST_DATA = voiRequestData;

            //borrower information
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_();

            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(voeOrder.BorrowerFullName, false);

            borrower.BorrowerID = "Borrower";
            borrower._FirstName = borr.FirstName;
            borrower._LastName = borr.LastName;
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = voeOrder.BorrowerSSN.Replace("-", "");

            loanApplication.BORROWER = borrower;

            voiRequest.LOAN_APPLICATION = loanApplication;

            requestData.VOI_REQUEST = voiRequest;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;



            return requestGroup;

        }

        public REQUEST_GROUP createInstantReverifyOrder(IDbConnection dbConn, int orderRequestId)
        {

            OrderRequest voeOrder = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            //header setup
            //requestGroup.MISMOVersionID = "2.3.1";

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            REQUEST_GROUP.SubmittingParty.PreferredResponse preferredResponse = new REQUEST_GROUP.SubmittingParty.PreferredResponse();
            preferredResponse._Format = "PDF";

            submittingParty.PREFERRED_RESPONSE = preferredResponse;
            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            List<REQUEST_GROUP.Request.Key> keys = new List<REQUEST_GROUP.Request.Key>() { };
            REQUEST_GROUP.Request.Key key = new REQUEST_GROUP.Request.Key();
            key._Name = "EMSOrderNumber";
            key._Value = voeOrder.EquifaxOrderNumber;
            keys.Add(key);

            request.KEY = keys.ToArray();

            //Equifax order parameters
            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.VOIRequest voiRequest = new REQUEST_GROUP.Request.RequestData.VOIRequest();
            voiRequest.LenderCaseIdentifier = voeOrder.LoanNumber;

            REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData voiRequestData = new REQUEST_GROUP.Request.RequestData.VOIRequest.VOIRequestData();
            //use original order type
            voiRequestData.VOIReportTypeOtherDescription = voeOrder.EquifaxOrderType;
            voiRequestData.VOIReportRequestActionType = "Other";
            voiRequestData.VOIReportRequestActionTypeOtherDescription = "Reverify";

            voiRequest.VOI_REQUEST_DATA = voiRequestData;

            //borrower information
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.VOIRequest.LoanApplication.Borrower_();

            borrower._FirstName = voeOrder.EquifaxFirstName;
            borrower._LastName = voeOrder.EquifaxLastName;
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = voeOrder.BorrowerSSN.Replace("-", "");

            loanApplication.BORROWER = borrower;

            voiRequest.LOAN_APPLICATION = loanApplication;

            requestData.VOI_REQUEST = voiRequest;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }

        public void checkOutstandingOrderStatus()
        {

            try
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
                OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
                OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
                dbConn.Open();

                List<OrderRequest> orders = dbConn.Where<OrderRequest>(q => q.EquifaxOrderStatus == "Accepted").ToList();

                foreach (OrderRequest order in orders)
                {
                    string orderStatus = queryOrderStatus(dbConn, order.Id, "voesystem");
                    logger.Info("Equifax Order " + order.EquifaxOrderNumber + " Status is: " + orderStatus);

                }

            }
            catch (Exception ex)
            {
                logger.Error("Error Checking Status of Equifax Orders", ex);
            }

        }

        public enum StatusRequestType
        {
            StatusQuery,
            RetrieveVOE
        }

        public string requestOrderCancellation(IDbConnection dbConn, int orderRequestId, string UserName)
        {
            string retVal = "Error Requesting Order Cancellation";

            try
            {

                //updateequifax order status
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.EquifaxOrderStatus = "Pending Cancellation";
                order.IsSubcontracted = false;
                dbConn.UpdateOnly(order, q => new { q.EquifaxOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                //send order cancellation request
                EmailOps eOp = new EmailOps();
                eOp.sendTemplateEmail(dbConn, "Equifax Request Order Cancellation", orderRequestId, null, false, false, order.RequestTypeId, false);

                logger.Info("Equifax Order Cancellation Requested for Order: " + order.LoanNumber + '-' + Int32.Parse(order.OrderSuffix).ToString("00"));

                retVal = "OK";

            }
            catch (Exception ex)
            {
                logger.Error("Error Requesting Equifax Order Cancellation", ex);
            }


            return retVal;

        }

        string extractReferenceNumFromXML(string inputXML)
        {
            string retVal = null;

            try
            {
                string refNumRegex = @"(?<=<SRVRTID>)[A-Za-z0-9]*?(?=</SRVRTID>)";

                Match match = Regex.Match(inputXML, refNumRegex);

                //this only gets the first instance, if there is one
                if (match.Success)
                {
                    retVal = match.Value;
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error Extracting Reference Number", ex);
            }

            return retVal;

        }

        public void autoSubmitOrdersToWorkNumber(bool SalaryKeyOnly)
        {
           
            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTWNOrderView> eqOrders = dbConn.Select<AutoTWNOrderView>().ToList();

                if (SalaryKeyOnly)
                {
                    eqOrders = eqOrders.Where(q => q.EquifaxSalaryKey != null).ToList();
                }

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                Encompass.Orders eOrds = new Encompass.Orders();

                logger.Info("Automatically Submitting " + eqOrders.Count.ToString() + " orders to Equifax");

                foreach (AutoTWNOrderView eqOrder in eqOrders)
                {
                    try
                    {

                        Dictionary<string, object> prms = new Dictionary<string, object> { };
                        prms.Add("OrderRequestId", eqOrder.OrderRequestId);

                        List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);
                        string result = "Success";

                        //this will not repull orders that have a success for the day 1
                        if (day1OrderId.Count == 0) {
                            List<int> certIds = new List<int>() { };
                            result = submitNewInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", EquifaxServiceTestMode, out certIds,
                                eqOrder.EquifaxSalaryKey, eqOrder.EquifaxEmployerCode, eqOrder.OrderTypeId, eqOrder.EncEmploymentStatus, false, null, null, EquifaxTestResultMessage);
                        }
                        else
                        {
                            result = "Skipped";
                        }

                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, eqOrder.OrderRequestId, "voesystem", false);


                        if (result.StartsWith("Success"))
                        {

                            //yay it worked
                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //work number parent status
                            oa.CurrOrderSubStatusId = 25; //work number verified
                            oa.ActivityNote = "Move to Work# Verified Status";

                            dbConn.Insert<OrderActivity>(oa);
                        }
                        else if (result.ToLower().Contains("salary key") && eqOrder.EquifaxSalaryKey == null) //only retry one time
                        {
                            //hey we need salary key
                            //eo.sendTemplateEmail(dbConn, "Request Branch Enter Salary Key", eqOrder.OrderRequestId, BackendBaseURL, false, false, null, false);

                            //this returns only one next user in line
                            AutoAssignUserView user = dbConn.Select<AutoAssignUserView>().FirstOrDefault();

                            AutoAssignOrderView order = new AutoAssignOrderView
                            {
                                OrderRequestId = eqOrder.OrderRequestId,
                                RequestTypeId = eqOrder.RequestTypeId
                            };

                            //move to work number pending, reassign to an actual specialist
                            //eOrds.assignOrder<AutoAssignOrderView>(dbConn, ref oOp, order, true, user.UserName, 24, 26); //work number parent status, work number pending
                            eOrds.assignOrder<AutoAssignOrderView>(dbConn, ref oOp, order, true, user.UserName, 13, 18); //pending parent status, salary key pending
                        }
                        else if (result == "Skipped") //day 1 exists
                        {
                            //just update the Equifax status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "EquifaxOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(eqOrder.OrderRequestId));
                            logger.Info("Skipping Equifax Pull for " + eqOrder.OrderRequestId.ToString());

                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //work number parent status
                            oa.CurrOrderSubStatusId = 25; //work number verified
                            oa.ActivityNote = "Move to Work# Verified Status";

                            dbConn.Insert<OrderActivity>(oa);

                        }
                        else
                        {
                            //it failed 
                            if (eqOrder.EquifaxSalaryKey != null)
                            {
                                //if this was a salary key retry, put back in new
                                oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                                oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                                oa.CurrOrderStatusId = 1;
                                oa.CurrOrderSubStatusId = null;
                                oa.ActivityNote = "Move back to New Status";

                                dbConn.Insert<OrderActivity>(oa);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Equifax Order Error: ", ex);
                    }

                }


            }
        }

        public void autoReverifyOrdersToWorkNumber(bool SalaryKeyOnly)
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTWNReverifyView> eqOrders = dbConn.Select<AutoTWNReverifyView>().ToList();

                if (SalaryKeyOnly)
                {
                    eqOrders = eqOrders.Where(q => q.EquifaxSalaryKey != null && q.EquifaxOrderStatus == "Success").ToList();
                }

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                Encompass.Orders eOrds = new Encompass.Orders();

                logger.Info("Automatically Submitting Reverif " + eqOrders.Count.ToString() + " orders to Equifax");

                foreach (AutoTWNReverifyView eqOrder in eqOrders)
                {
                    try
                    {

                        List<int> certIds = new List<int>() { };
                        string result = submitReverifyInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", EquifaxServiceTestMode, out certIds, EquifaxTestResultMessage);

                        //really don't care what the status was, still moving to the TWN final queue
                        OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, eqOrder.OrderRequestId, "voesystem", false);
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
                        logger.Error("Auto Equifax Order Error: ", ex);
                    }

                }


            }
        }


    }
}

