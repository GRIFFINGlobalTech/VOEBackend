using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOEBackend.Xactus.Schema;
using VOESystem.Data.DBSchema;
using VOESystem.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using VOESystem.Data.Business;
using VOESystem.Data.DTO;
using ServiceStack.OrmLite;
using System.Configuration;

namespace VOEBackend.Xactus.Business
{
    public class OrderOps : BaseClass
    {

       
        public string submitNewInstantOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
           string employerCode, int orderTypeId, string verifType, string subVendorName, bool IsDay1, string firstName = null, string lastName = null, string TestResultMessage = null)
        {

            string retVal = "Failure";
            CommOps.ResponseResult result = null;
            string orderNumber = String.Empty;
            bool fileAttError = false;
            DateTime? dataDate = null;
            String referenceNumber = null;
            certFileIds = new List<int>();
            REQUEST_GROUP request = null;
            SubVendor subVendor;

            try
            {
                //get subvendor
                subVendor = (SubVendor)Enum.Parse(typeof(SubVendor), subVendorName);

                //get order information
                if (subVendor == SubVendor.TWN) {
                    request = createTWNInstantOrder(dbConn, orderRequestId, employerCode, (OrderType)orderTypeId,
                    (VerificationType)Enum.Parse(typeof(VerificationType), verifType), firstName, lastName);
                }
                else if (subVendor == SubVendor.Experian)
                {
                    request = createExperianInstantOrder(dbConn, orderRequestId, employerCode, (OrderType)orderTypeId,
                    (VerificationType)Enum.Parse(typeof(VerificationType), verifType), firstName, lastName);
                }

                orderNumber = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                CommOps comm = new CommOps();

                if (TestMode)
                {

                    //this is test           
                    logger.Info("Xactus Service Test Mode for " + subVendorName + " Instant Order " + orderNumber);

                    //write file
                    string XactusReqType = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.CREDIT_REQUEST_DATA.CreditReportRequestActionType;
                    string reqestString = comm.serializeRequest(request);
                    string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "Xactus" + subVendorName + "InstantOrder", XactusReqType);

                    if (TestResultMessage == "Success")
                    {
                        retVal = "Success";
                        result = new CommOps.ResponseResult
                        {
                            XactusOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Done",
                            Files = new List<string>() {
                                Path.GetFileName(TestPDFFilePath)
                            },
                            ResultMessage = "Success"
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
                    else if (TestResultMessage == "Salary Key")
                    {
                        //simulate employer code
                        result = new CommOps.ResponseResult
                        {
                            Status = "Done",
                            ResultMessage = "Salary Key Required",
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
                            XactusOrderId = "XACT" + orderRequestId.ToString(),
                            Status = "Error",
                            ResultMessage = "Employee not found in database"
                        };

                    }


                }
                else
                {
                    //this is production
                    result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, true, false, IsDay1, subVendor);
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
                            string XactusDocFilePath = RepositoryPath + "Documents\\XactusDocuments\\" + file;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                            if (File.Exists(UploadFilePath))
                            {
                                File.Delete(UploadFilePath);
                            }
                            File.Copy(XactusDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            try
                            {
                                DocumentOps.DocumentType docType = DocumentOps.DocumentType.XactusTWNDownload;

                                if (subVendor == SubVendor.TWN)
                                {
                                    docType = DocumentOps.DocumentType.XactusTWNDownload;
                                }
                                else if (subVendor == SubVendor.Experian)
                                {
                                    docType = DocumentOps.DocumentType.XactusExperianDownload;
                                }

                                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                    docType, res, true, UploadFilePath, pageCount, UserName);
                                int docId = res.DocumentId;

                                //try to get the reference number but only add to document record since they decided they needed manual review since they cannot ensure that the correct employer is here
                                referenceNumber = pOp.extractReferenceNumber(XactusDocFilePath);

                                //try to get the datadate but only add to document record since they decided they needed manual review since they cannot ensure that the correct employer is here
                                dataDate = pOp.extractEquifaxDataDate(XactusDocFilePath);

                                int VendorId = dbConn.Where<Vendor>(q => q.Name == "Xactus").FirstOrDefault().Id;

                                //update document record
                                dbConn.Update<Document>(
                                 set: "VendorReferenceNum = {0}, VendorDataDate = {1}, VendorId = {2}".Params(referenceNumber, dataDate, VendorId),
                                 where: "Id = {0}".Params(docId));

                                //add document employer data
                                foreach (CommOps.ResponseResult.Employer emp in result.Employers)
                                {
                                    dbConn.Insert<DocumentEmployer>(new DocumentEmployer
                                    {
                                        DocumentId = docId,
                                        EmployerName = emp.EmployerName,
                                        EmployeeStatus = emp.EmployeeStatus
                                    });
                                }

                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error Saving Document " + UploadFilePath, ex);
                                res.Result = false;
                            }

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Xactus Document to Order " + orderNumber + " " + file,
                                    new Exception("Error Attaching Xactus Document to Order"));
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
                            logger.Info("Xactus " + subVendorName + " instant order has been completed for order " + orderNumber + ": XactusOrderNumber " + result.XactusOrderId);
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


                //update Xactus order number, status, vendor data date, first and last names
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.XactusOrderNumber = result.XactusOrderId;
                order.XactusOrderStatus = retVal;  //will be success or failure
                order.XactusOrderType = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.CREDIT_REQUEST_DATA.CreditReportTypeOtherDescription.Replace(" Verify","");
                order.XactusFirstName = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.LOAN_APPLICATION.BORROWER._FirstName;
                order.XactusLastName = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.LOAN_APPLICATION.BORROWER._LastName;
                if (request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.LOAN_APPLICATION.BORROWER.EMPLOYER != null)
                {
                    order.XactusEmployerCode = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.LOAN_APPLICATION.BORROWER.EMPLOYER.EmployerCode;
                }

                //write to activity log that order was processed
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = "Instant Order Sent to Xactus/" + subVendorName + "; Xactus Order Number " + result.XactusOrderId;
                if (result.ResultMessage.Contains("Error"))
                {
                    oa.ActivityNote += "The following errors ocurred during instant order processing: " + result.ResultMessage;

                    if (result.ResultMessage.ToLower().Contains("employer"))
                    {
                        retVal += ": Employer Code Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("salary key"))
                    {
                        retVal += ": Salary Key Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("multiple individuals"))
                    {
                        retVal += ": Multiple Individuals with this SSN";
                    }

                }
                else
                {
                    oa.ActivityNote += " Instant Verification Received - " + subVendorName;
                }

                //keep changing mind on this
                if (order.RequestTypeId == 6)  //instant request
                {
                    oa.VendorDataDate = dataDate;
                    oa.VendorId = dbConn.Where<Vendor>(q => q.Name == "Xactus").FirstOrDefault().Id;
                    oa.VendorReferenceNum = referenceNumber;
                }

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {
                    dbConn.UpdateOnly(order, q => new { q.XactusOrderNumber, q.XactusOrderStatus, q.XactusOrderType, q.XactusFirstName, q.XactusLastName }, r => r.Id == orderRequestId);
                    dbConn.Insert<OrderActivity>(oa);
                    tr.Commit();
                }

            }
            catch (Exception ex)
            {
                //some other type of error 
                logger.Error("Xactus " + subVendorName + " instant order has been NOT been processed for order " + orderNumber + ": " + result.ResultMessage, ex);
            }

            return retVal;


        }


        public string submitReverifyInstantOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, out List<int> certFileIds,
           string subVendorName, bool IsDay1, string firstName = null, string lastName = null, string TestResultMessage = null)
        {

            string retVal = "Failure";
            CommOps.ResponseResult result = null;
            string orderNumber = String.Empty;
            string employerId = String.Empty;
            bool fileAttError = false;
            certFileIds = new List<int>();
            REQUEST_GROUP request = null;
            SubVendor subVendor;

            try
            {
                //get subvendor
                subVendor = (SubVendor)Enum.Parse(typeof(SubVendor), subVendorName);

                OrderDetailView orderDetail = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();
                orderNumber = orderDetail.OrderNumber;
                employerId = orderDetail.VendorReferenceNum;

                if (employerId == null && !TestMode)
                {
                    throw new Exception("Missing Vendor Reference Number");
                }

                //get order information
                if (subVendor == SubVendor.TWN)
                {
                    request = createTWNReverifyOrder(dbConn, orderRequestId, firstName, lastName, employerId);
                }
                else if (subVendor == SubVendor.Experian)
                {
                    //throw order type not supported error
                    throw new Exception("Experian Reverify Order Not Supported");
                    //request = createExperianInstantOrder(dbConn, orderRequestId, employerCode, (OrderType)orderTypeId,
                    //(VerificationType)Enum.Parse(typeof(VerificationType), verifType), firstName, lastName);
                }

                

                CommOps comm = new CommOps();

                if (TestMode)
                {

                    //this is test           
                    logger.Info("Xactus Service Test Mode for " + subVendorName + " Instant Order " + orderNumber);

                    //write file
                    string XactusReqType = request.REQUEST.REQUEST_DATA.CREDIT_REQUEST.CREDIT_REQUEST_DATA.CreditReportRequestActionType;
                    string reqestString = comm.serializeRequest(request);
                    string OrderFilePathName = comm.writeRequestStringToFile(reqestString, orderNumber, "Xactus" + subVendorName + "InstantOrder", XactusReqType);

                    if (TestResultMessage == "Success")
                    {
                        retVal = "Success";
                        result = new CommOps.ResponseResult
                        {
                            XactusOrderId = "EQI" + orderRequestId.ToString(),
                            Status = "Done",
                            Files = new List<string>() {
                                Path.GetFileName(TestPDFFilePath)
                            },
                            ResultMessage = "Success"
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
                    else if (TestResultMessage == "Salary Key")
                    {
                        //simulate employer code
                        result = new CommOps.ResponseResult
                        {
                            Status = "Done",
                            ResultMessage = "Salary Key Required",
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
                            XactusOrderId = "XACT" + orderRequestId.ToString(),
                            Status = "Error",
                            ResultMessage = "Employee not found in database"
                        };

                    }


                }
                else
                {
                    //this is production
                    result = comm.postRequest(dbConn, request, orderNumber, orderRequestId, UserName, true, true, IsDay1, subVendor);
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
                            string XactusDocFilePath = RepositoryPath + "Documents\\XactusDocuments\\" + file;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                            if (File.Exists(UploadFilePath))
                            {
                                File.Delete(UploadFilePath);
                            }
                            File.Copy(XactusDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            try
                            {
                                DocumentOps.DocumentType docType = DocumentOps.DocumentType.XactusTWNDownload;

                                if (subVendor == SubVendor.TWN)
                                {
                                    docType = DocumentOps.DocumentType.XactusTWNDownload;
                                }
                                else if (subVendor == SubVendor.Experian)
                                {
                                    docType = DocumentOps.DocumentType.XactusExperianDownload;
                                }

                                res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                    docType, res, true, UploadFilePath, pageCount, UserName);
                                int docId = res.DocumentId;

                                int VendorId = dbConn.Where<Vendor>(q => q.Name == "Xactus").FirstOrDefault().Id;

                                //update document record
                                dbConn.Update<Document>(
                                 set: "VendorId = {0}".Params(VendorId),
                                 where: "Id = {0}".Params(docId));

                                //add document employer data
                                foreach (CommOps.ResponseResult.Employer emp in result.Employers)
                                {
                                    dbConn.Insert<DocumentEmployer>(new DocumentEmployer
                                    {
                                        DocumentId = docId,
                                        EmployerName = emp.EmployerName,
                                        EmployeeStatus = emp.EmployeeStatus
                                    });
                                }

                            }
                            catch (Exception ex)
                            {
                                logger.Error("Error Saving Document " + UploadFilePath, ex);
                                res.Result = false;
                            }

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Xactus Document to Order " + orderNumber + " " + file,
                                    new Exception("Error Attaching Xactus Document to Order"));
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
                            logger.Info("Xactus " + subVendorName + " reverify order has been completed for order " + orderNumber + ": XactusOrderNumber " + result.XactusOrderId);
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


                //update Xactus order number, status, vendor data date, first and last names
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                
                //write to activity log that order was processed
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                oa.ActivityNote = "Reverify Order Sent to Xactus/" + subVendorName + "; Xactus Order Number " + result.XactusOrderId;
                if (result.ResultMessage.Contains("Error"))
                {
                    oa.ActivityNote += "The following errors ocurred during instant order processing: " + result.ResultMessage;

                    if (result.ResultMessage.ToLower().Contains("employer"))
                    {
                        retVal += ": Employer Code Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("salary key"))
                    {
                        retVal += ": Salary Key Required";
                    }
                    else if (result.ResultMessage.ToLower().Contains("multiple individuals"))
                    {
                        retVal += ": Multiple Individuals with this SSN";
                    }

                }
                else
                {
                    oa.ActivityNote += " Reverification Received - " + subVendorName;
                }

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {
                    dbConn.Insert<OrderActivity>(oa);
                    tr.Commit();
                }

            }
            catch (Exception ex)
            {
                //some other type of error 
                string msg = isNull(result.ResultMessage, ex.Message);
                logger.Error("Xactus " + subVendorName + " reverify order has been NOT been processed for order " + orderNumber + ": " + msg, ex);
            }

            return retVal;


        }

        public REQUEST_GROUP createTWNInstantOrder(IDbConnection dbConn, int orderRequestId, string employerCode, OrderType orderType, VerificationType verifType,
            string firstName, string lastName)
        {


            OrderRequest order = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            string mismoVersion = "2.3.1";
            string borrowerId = "1";

            //header setup
            requestGroup.MISMOVersionID = mismoVersion;

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            REQUEST_GROUP.SubmittingParty.PreferredResponse preferredResponse = new REQUEST_GROUP.SubmittingParty.PreferredResponse();
            preferredResponse._Format = "PDF";

            submittingParty.PREFERRED_RESPONSE = preferredResponse;
            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            //services section
            //credit
            REQUEST_GROUP.Request.RequestData.CreditRequest CREDIT_REQUEST = new REQUEST_GROUP.Request.RequestData.CreditRequest();
            CREDIT_REQUEST.MISMOVersionID = mismoVersion;
            CREDIT_REQUEST.LenderCaseIdentifier = order.LoanNumber;
 
            REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData CREDIT_REQUEST_DATA = new REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData();
            CREDIT_REQUEST_DATA.BorrowerID = borrowerId;
            CREDIT_REQUEST_DATA.CreditReportType = "Other";
            CREDIT_REQUEST_DATA.CreditReportTypeOtherDescription = "TWN";
            CREDIT_REQUEST_DATA.CreditReportRequestActionType = "Submit";
            CREDIT_REQUEST_DATA.CreditRequestType = "Individual";

            if (orderType.ToString() == "Written")
            {
                CREDIT_REQUEST_DATA.VerifyIncome = "Y";
            }

            if (verifType.ToString() == "Current")
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Current";  //filtering by active employment
            }
            else if (verifType.ToString() == "Prior")
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Previous";  //filtering by inactive employment
            }
            else
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Both";  //all employment
            }

            CREDIT_REQUEST.CREDIT_REQUEST_DATA = CREDIT_REQUEST_DATA;

            //borrower information
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_();

            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(order.BorrowerFullName, true);
            
            borrower._FirstName = isNull(firstName, borr.FirstName);
            borrower._LastName = isNull(lastName, borr.LastName);
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = order.BorrowerSSN.Replace("-", "");
            borrower.BorrowerID = borrowerId;

            //OrderAddress orderAddr = li.splitOrderAddress(order.BorrowerAddress);
            //REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence residence = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence();
            //residence._State = orderAddr.State;
            //borrower._RESIDENCE = residence;

            employerCode = employerCode ?? order.XactusEmployerCode;

            if (employerCode != null)
            {
                REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Employer_ employer = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Employer_();
                employer.EmployerCode = employerCode;
                borrower.EMPLOYER = employer;
            }

            loanApplication.BORROWER = borrower;

            CREDIT_REQUEST.LOAN_APPLICATION = loanApplication;

            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            requestData.CREDIT_REQUEST = CREDIT_REQUEST;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }

        public REQUEST_GROUP createExperianInstantOrder(IDbConnection dbConn, int orderRequestId, string employerCode, OrderType orderType, VerificationType verifType,
            string firstName, string lastName)
        {


            OrderRequest order = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            string mismoVersion = "2.3.1";
            string borrowerId = "1";

            //header setup
            requestGroup.MISMOVersionID = mismoVersion;

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            REQUEST_GROUP.SubmittingParty.PreferredResponse preferredResponse = new REQUEST_GROUP.SubmittingParty.PreferredResponse();
            preferredResponse._Format = "PDF";

            submittingParty.PREFERRED_RESPONSE = preferredResponse;
            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            //services section
            //credit
            REQUEST_GROUP.Request.RequestData.CreditRequest CREDIT_REQUEST = new REQUEST_GROUP.Request.RequestData.CreditRequest();
            CREDIT_REQUEST.MISMOVersionID = mismoVersion;
            CREDIT_REQUEST.LenderCaseIdentifier = order.LoanNumber;

            REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData CREDIT_REQUEST_DATA = new REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData();
            CREDIT_REQUEST_DATA.BorrowerID = borrowerId;
            CREDIT_REQUEST_DATA.CreditReportType = "Other";
            CREDIT_REQUEST_DATA.CreditReportTypeOtherDescription = "Experian Verify";
            CREDIT_REQUEST_DATA.CreditReportRequestActionType = "Submit";
            CREDIT_REQUEST_DATA.CreditRequestType = "Individual";

            if (orderType.ToString() == "Written")
            {
                CREDIT_REQUEST_DATA.VerifyIncome = "Y";
            }

            if (verifType.ToString() == "Current")
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Current";  //filtering by active employment
            }
            else if (verifType.ToString() == "Prior")
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Previous";  //filtering by inactive employment
            }
            else
            {
                CREDIT_REQUEST_DATA.RecordsFrom = "Both";  //all employment
            }

            CREDIT_REQUEST.CREDIT_REQUEST_DATA = CREDIT_REQUEST_DATA;

            //borrower information
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_();

            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(order.BorrowerFullName, false);

            borrower._FirstName = isNull(firstName, borr.FirstName);
            borrower._LastName = isNull(lastName, borr.LastName);
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = order.BorrowerSSN.Replace("-", "");
            borrower.BorrowerID = borrowerId;
            borrower._BirthDate = (order.BorrowerDOB ?? DateTime.Parse("1900-01-01")).ToString("yyyy-MM-dd");

            OrderAddress orderAddr = li.splitOrderAddress(order.BorrowerAddress);
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence residence = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence();
            residence._StreetAddress = orderAddr.Street;
            residence._City = orderAddr.City;
            residence._State = orderAddr.State;
            residence._PostalCode = orderAddr.Zip;
            borrower._RESIDENCE = residence;

            employerCode = employerCode ?? order.XactusEmployerCode;

            if (employerCode != null)
            {
                REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Employer_ employer = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Employer_();
                employer.EmployerCode = employerCode;
                borrower.EMPLOYER = employer;
            }

            loanApplication.BORROWER = borrower;

            CREDIT_REQUEST.LOAN_APPLICATION = loanApplication;

            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            requestData.CREDIT_REQUEST = CREDIT_REQUEST;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }

        public REQUEST_GROUP createTWNReverifyOrder(IDbConnection dbConn, int orderRequestId, string firstName, string lastName, string employerId)
        {


            OrderRequest order = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            string mismoVersion = "2.3.1";
            string borrowerId = "1";

            //header setup
            requestGroup.MISMOVersionID = mismoVersion;

            REQUEST_GROUP.SubmittingParty submittingParty = new REQUEST_GROUP.SubmittingParty();
            submittingParty._Name = VENDORID;

            //REQUEST_GROUP.SubmittingParty.PreferredResponse preferredResponse = new REQUEST_GROUP.SubmittingParty.PreferredResponse();
            //preferredResponse._Format = "PDF";

            //submittingParty.PREFERRED_RESPONSE = preferredResponse;
            requestGroup.SUBMITTING_PARTY = submittingParty;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            //services section
            //credit
            REQUEST_GROUP.Request.RequestData.CreditRequest CREDIT_REQUEST = new REQUEST_GROUP.Request.RequestData.CreditRequest();
            CREDIT_REQUEST.MISMOVersionID = mismoVersion;
            CREDIT_REQUEST.LenderCaseIdentifier = order.LoanNumber;

            REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData CREDIT_REQUEST_DATA = new REQUEST_GROUP.Request.RequestData.CreditRequest.CreditRequestData();
            CREDIT_REQUEST_DATA.BorrowerID = borrowerId;
            CREDIT_REQUEST_DATA.CreditReportType = "Other";
            CREDIT_REQUEST_DATA.CreditReportTypeOtherDescription = "TWN / Reverify";
            CREDIT_REQUEST_DATA.CreditReportRequestActionType = "Submit";
            CREDIT_REQUEST_DATA.CreditRequestType = "Individual";
            CREDIT_REQUEST_DATA.CreditReportIdentifier = order.XactusOrderNumber;
            CREDIT_REQUEST_DATA.ReverifyEmployerID = employerId;

            CREDIT_REQUEST.CREDIT_REQUEST_DATA = CREDIT_REQUEST_DATA;

            //borrower information
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication();
            REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_();

            borrower._FirstName = isNull(firstName, order.XactusFirstName);
            borrower._LastName = isNull(lastName, order.XactusLastName);
            borrower._SSN = order.BorrowerSSN.Replace("-", "");
            borrower.BorrowerID = borrowerId;

            //OrderAddress orderAddr = li.splitOrderAddress(order.BorrowerAddress);
            //REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence residence = new REQUEST_GROUP.Request.RequestData.CreditRequest.LoanApplication.Borrower_.Residence();
            //residence._State = orderAddr.State;
            //borrower._RESIDENCE = residence;

            loanApplication.BORROWER = borrower;

            CREDIT_REQUEST.LOAN_APPLICATION = loanApplication;

            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            requestData.CREDIT_REQUEST = CREDIT_REQUEST;

            request.REQUEST_DATA = requestData;

            requestGroup.REQUEST = request;

            return requestGroup;

        }


        private string cleanBorrowerName(string BorrowerName)
        {

            string retVal = BorrowerName;

            //allow only characters, numbers, dash, single quote and space
            string restrictPattern = @"[^\-\'\s\w]";
            retVal = Regex.Replace(retVal, restrictPattern,"");

            return retVal;
        }

        public void autoSubmitOrdersToWorkNumber()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTWNOrderView> eqOrders = dbConn.Select<AutoTWNOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                Encompass.Orders eOrds = new Encompass.Orders();

                logger.Info("Automatically Submitting " + eqOrders.Count.ToString() + " TWN orders to Xactus");

                foreach (AutoTWNOrderView eqOrder in eqOrders)
                {
                    try
                    {

                        Dictionary<string, object> prms = new Dictionary<string, object> { };
                        prms.Add("OrderRequestId", eqOrder.OrderRequestId);

                        List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);
                        string result = "Success";

                        //this will not repull orders that have a success for the day 1
                        if (day1OrderId.Count == 0)
                        {
                            List<int> certIds = new List<int>() { };
                            result = submitNewInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", XactusServiceTestMode, out certIds,
                                eqOrder.XactusEmployerCode, eqOrder.OrderTypeId, eqOrder.EncEmploymentStatus, "TWN", false, null, null, XactusTestResultMessage);
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
                        else if (result.ToLower().Contains("salary key")) 
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
                            //just update the Xactus status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "XactusOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(eqOrder.OrderRequestId));
                            logger.Info("Skipping Xactus Pull for " + eqOrder.OrderRequestId.ToString());

                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //work number parent status
                            oa.CurrOrderSubStatusId = 25; //work number verified
                            oa.ActivityNote = "Move to Work# Verified Status";

                            dbConn.Insert<OrderActivity>(oa);

                        }
                        

                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Xactus Order Error: ", ex);
                    }

                }


            }
        }

        public void autoSubmitOrdersToExperian()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoExperianOrderView> eqOrders = dbConn.Select<AutoExperianOrderView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                Encompass.Orders eOrds = new Encompass.Orders();

                logger.Info("Automatically Submitting " + eqOrders.Count.ToString() + " Experian orders to Xactus");

                foreach (AutoExperianOrderView eqOrder in eqOrders)
                {
                    try
                    {

                        Dictionary<string, object> prms = new Dictionary<string, object> { };
                        prms.Add("OrderRequestId", eqOrder.OrderRequestId);

                        List<int> day1OrderId = dbConn.SqlList<int>("EXEC usp_GetDay1BorrowerOrder @OrderRequestId", prms);
                        string result = "Success";

                        //this will not repull orders that have a success for the day 1
                        if (day1OrderId.Count == 0)
                        {
                            List<int> certIds = new List<int>() { };
                            result = submitNewInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", XactusServiceTestMode, out certIds,
                                eqOrder.XactusEmployerCode, eqOrder.OrderTypeId, eqOrder.EncEmploymentStatus, "Experian", false, null, null, XactusTestResultMessage);
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
                        else if (result.ToLower().Contains("salary key"))
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
                            //just update the Xactus status to skipped so that it won't get back into the queue
                            dbConn.Update<OrderRequest>(
                                set: "XactusOrderStatus = 'Skipped'",
                                where: "Id = {0}".Params(eqOrder.OrderRequestId));
                            logger.Info("Skipping Xactus Experian Pull for " + eqOrder.OrderRequestId.ToString());

                            oa.PrevOrderStatusId = oa.CurrOrderStatusId;
                            oa.PrevOrderSubStatusId = oa.CurrOrderSubStatusId;
                            oa.CurrOrderStatusId = 24; //work number parent status
                            oa.CurrOrderSubStatusId = 25; //work number verified
                            oa.ActivityNote = "Move to Work# Verified Status";

                            dbConn.Insert<OrderActivity>(oa);

                        }


                    }
                    catch (Exception ex)
                    {
                        logger.Error("Auto Xactus Order Error: ", ex);
                    }

                }


            }
        }

        public void autoReverifyOrdersToWorkNumber()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoTWNReverifyView> eqOrders = dbConn.Select<AutoTWNReverifyView>().ToList();
                //eqOrders = eqOrders.Where(q => q.OrderRequestId == 408649).ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting Reverif " + eqOrders.Count.ToString() + " TWN orders to Xactus");

                foreach (AutoTWNReverifyView eqOrder in eqOrders)
                {

                    try
                    {

                        List<int> certIds = new List<int>() { };

                        string result = submitReverifyInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", XactusServiceTestMode, out certIds,
                            "TWN", false, null, null, XactusTestResultMessage);

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
                        logger.Error("Auto Xactus Order Error: ", ex);
                    }

                }


            }
        }

        public void autoReverifyOrdersToExperian()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                  ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                  true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                List<AutoExperianReverifyView> eqOrders = dbConn.Select<AutoExperianReverifyView>().ToList();

                VOESystem.Data.Business.EmailOps eo = new VOESystem.Data.Business.EmailOps();
                VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();

                logger.Info("Automatically Submitting Reverif " + eqOrders.Count.ToString() + " Experian orders to Xactus");

                foreach (AutoExperianReverifyView eqOrder in eqOrders)
                {

                    try
                    {

                        List<int> certIds = new List<int>() { };
                        string result = submitNewInstantOrder(dbConn, eqOrder.OrderRequestId, "voesystem", XactusServiceTestMode, out certIds,
                                eqOrder.XactusEmployerCode, eqOrder.OrderTypeId, VerificationType.Current.ToString(), "Experian", false, null, null, XactusTestResultMessage);

                        //really don't care what the status was, still moving to the vendor final queue
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
                        logger.Error("Auto Xactus Order Error: ", ex);
                    }

                }


            }
        }

    }
}
