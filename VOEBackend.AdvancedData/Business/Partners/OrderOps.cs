using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOEBackend.AdvancedData.Schema.Partners;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOEBackend.AdvancedData.Business.Partners
{
    public class OrderOps : BaseClass
    {


        public enum CommOperation
        {
            Create,
            Query
        }


        public string submitNewOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {


            string retVal = "Error creating Advanced Data order.";

            try
            {
                //get order information
                REQUEST_GROUP adOrder = createOrder(dbConn, orderRequestId, UserName, TestMode);
                CommOps.ResponseResult res;

                string orderNumber = dbConn.Where<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                if (!TestMode)
                {
                    //this is production
                    CommOps comm = new CommOps();
                    res = comm.postRequest(dbConn, adOrder, orderNumber, orderRequestId, CommOperation.Create, UserName);
                }
                else
                {
                    //this is test mode
                    res = new CommOps.ResponseResult()
                    {
                        ADOrderNumber = "AD" + orderNumber,
                        Status = "Pending"
                    };
                    logger.Info("Advanced Data Service Test Mode for Order " + res.ADOrderNumber);
                }


                if (res.Status == "Pending")
                {
                    retVal = "Advanced Data order has been created";

                    logger.Info("Advanced Data order has been created for order " + orderNumber + ": ADOrderNumber " + res.ADOrderNumber);

                    //update advanced data order number,  is subcontracted value
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.ADOrderNumber = res.ADOrderNumber;
                    order.IsSubcontracted = true;
                    order.ADOrderStatus = res.Status;

                    //write to activity log that order was subcontracted
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                    oa.ActivityNote = "Order Sent to Advanced Data; AD Order Number " + res.ADOrderNumber;

                    using (IDbTransaction tr = dbConn.BeginTransaction())
                    {
                        dbConn.UpdateOnly(order, q => new { q.ADOrderNumber, q.IsSubcontracted, q.ADOrderStatus }, r => r.Id == orderRequestId);
                        dbConn.Insert<OrderActivity>(oa);
                        tr.Commit();
                    }

                }
                else
                {
                    retVal = "Error - Advanced Data order NOT created: " + res.ResultMessage;
                    logger.Error("Advanced Data order has been not been created for order " + orderNumber + ": " + res.ResultMessage,
                        new Exception("Error Creating Advanced Data Order"));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error Submitting Advanced Data Order", ex);
            }

            return retVal;


        }



        public string submitNewOrderNoAPI(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode, string Status, string ADOrderNumber)
        {


            string retVal = "Error creating Advanced Data order.";

            try
            {
                //get order information
                //REQUEST_GROUP adOrder = createOrder(dbConn, orderRequestId, UserName, TestMode);
                //CommOps.ResponseResult res;

                string orderNumber = dbConn.Where<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;
                /*
                if (!TestMode)
                {
                    //this is production
                    CommOps comm = new CommOps();
                    res = comm.postRequest(dbConn, adOrder, orderNumber, orderRequestId, CommOperation.Create, UserName);
                }
                else
                {
                    //this is test mode
                    res = new CommOps.ResponseResult()
                    {
                        ADOrderNumber = "AD" + orderNumber,
                        Status = "Pending"
                    };
                    logger.Info("Advanced Data Service Test Mode for Order " + res.ADOrderNumber);
                }

    */
                if (Status == "Pending")
                {
                    retVal = "Advanced Data order has been created";

                    logger.Info("Advanced Data order has been created for order " + orderNumber + ": ADOrderNumber " + ADOrderNumber);

                    //update advanced data order number,  is subcontracted value
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.ADOrderNumber = ADOrderNumber;
                    order.IsSubcontracted = true;
                    order.ADOrderStatus = Status;

                    //write to activity log that order was subcontracted
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, UserName, false);

                    oa.ActivityNote = "Order Sent to Advanced Data; AD Order Number " + ADOrderNumber;

                    using (IDbTransaction tr = dbConn.BeginTransaction())
                    {
                        dbConn.UpdateOnly(order, q => new { q.ADOrderNumber, q.IsSubcontracted, q.ADOrderStatus }, r => r.Id == orderRequestId);
                        dbConn.Insert<OrderActivity>(oa);
                        tr.Commit();
                    }

                }
                else
                {
                    //retVal = "Error - Advanced Data order NOT created: " + res.ResultMessage;
                    //logger.Error("Advanced Data order has been not been created for order " + orderNumber + ": " + res.ResultMessage,
                        //new Exception("Error Creating Advanced Data Order"));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Error Submitting Advanced Data Order", ex);
            }

            return retVal;


        }

        public string queryOrderStatus(IDbConnection dbConn, int orderRequestId, string UserName)
        {
            CommOps.ResponseResult result = new CommOps.ResponseResult() { Status = "Unknown" };

            try
            {
                //get order information
                Schema.Partners.REQUEST_GROUP queryReq = createQuery(dbConn, orderRequestId);

                string orderNumber = dbConn.Where<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault().OrderNumber;

                CommOps comm = new CommOps();
                result = comm.postRequest(dbConn, queryReq, orderNumber, orderRequestId, CommOperation.Query, UserName);

                if (result.Status == "Completed")
                {

                    logger.Info("Advanced Data Order Complete for Order: " + orderNumber + ": ADOrderNumber " + result.ADOrderNumber);
                    
                    //update advanced data order status
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.ADOrderStatus = result.Status;
                    order.IsSubcontracted = false;
                    dbConn.UpdateOnly(order, q => new { q.ADOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                    //save the files to linked docs
                    bool fileAttError = false;
                    if (result.Files.Count > 0)
                    {
                        DocumentOps dOp = new DocumentOps();
                        PDFOps pOp = new PDFOps();

                        foreach (string file in result.Files)
                        {
                            string[] fileParts = file.Split("_"[0]);
                            string displayName = fileParts[fileParts.Length - 1];
                            string UploadFilePath = RepositoryPath + "Documents\\Upload\\" + file;
                            string ADDocFilePath = RepositoryPath + "Documents\\ADDocuments\\" + file;

                            //TODO: this needs to be changed so that we don't have to have two copies of the cert - so we can read from ADDocuments location
                            File.Copy(ADDocFilePath, UploadFilePath);
                            logger.Info("File Uploaded to Local Directory: " + UploadFilePath);

                            int? pageCount = pOp.getPageCount(UploadFilePath);

                            UploadResult res = new UploadResult();
                            res = dOp.saveDocument(dbConn, null, orderRequestId, displayName, file,
                                DocumentOps.DocumentType.AdvancedDataDownload, res, true, UploadFilePath, pageCount, null);

                            if (!res.Result)
                            {
                                logger.Error("Error Attaching Advanced Data Document to Order " + result.ADOrderNumber + " " + file,
                                    new Exception("Error Attaching Advanced Data Document to Order"));
                                fileAttError = true;
                            }

                        }

                        if (fileAttError)
                        {
                            result.ResultMessage = "Error: Cannnot Attach Downloaded File(s) to Order";
                        }
                    }

                    //add order activity
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    OrderActivity oa = oOp.getOrderActvityForNewActivty(dbConn, orderRequestId, "voesystem", false);

                    oa.ActivityNote = "Advanced Data Order Complete. ";

                    if (result.ResultMessage.StartsWith("Error"))
                    {
                        oa.ActivityNote += "The following errors ocurred during order processing: " + result.ResultMessage;
                    }

                    dbConn.Insert<OrderActivity>(oa);

                    //notify VOES to do something;
                    EmailOps eOp = new EmailOps();
                    eOp.sendTemplateEmail(dbConn, "Advanced Data Order Complete", orderRequestId, null, false, false, order.RequestTypeId, true);

                    //notify accounting
                    eOp.sendTemplateEmail(dbConn, "Subcontracted Order Complete", orderRequestId, null, false, false, order.RequestTypeId, true);

                }
                else if (result.Status == "Error")
                {
                    logger.Error("Advanced Data Status Query Error for Order " + result.ADOrderNumber + ": " + result.ResultMessage,
                        new Exception("Error Querying Status of Advanced Data Order"));
                }
                
            }
            catch (Exception ex)
            {
                logger.Error("Error Querying Advanced Data Order Status for OrderRequestId = " + orderRequestId, ex);
            }

            return result.Status;

        }

        public REQUEST_GROUP createOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {


            OrderRequest order = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            string mismoVersion = "2.3.1";
            string borrowerId = "Borrower";

            //header setup
            requestGroup.MISMOVersionID = mismoVersion;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.Extension extension = new REQUEST_GROUP.Request.RequestData.Extension();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection extensionSection = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData extensionSectionData = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest verificationRequest = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest();

            verificationRequest._ActionType = "Original";
            verificationRequest._ItemType = "Verifications";

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product product = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product();

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild typeProductChild = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild();
            if (order.OrderTypeId == 1)
            {
                //verbal
                typeProductChild._Description = "Verbal Voe";
                typeProductChild._Identifier = "VOE";
            }
            else
            {
                //written
                typeProductChild._Description = "Written Voe";
                typeProductChild._Identifier = "VOI";
            }
            product._TYPE = typeProductChild;

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild nameProductChild = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild();
            if (order.EncEmploymentStatus == "Current")
            {
                //curent
                nameProductChild._Description = "Current Employment";
                nameProductChild._Identifier = "CE";
            }
            else
            {
                //prior
                nameProductChild._Description = "Previous Employment";
                nameProductChild._Identifier = "PE";
            }
            product._NAME = nameProductChild;

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild reqProductChild = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild();
            reqProductChild._Description = "Manual Only";
            reqProductChild._Identifier = "MO";
            product._REQTYPE = reqProductChild;

            //add forms.  need the borrower authorization form for all orders.
            //for written voes we need to concat the written authorization form onto the borrower auth form prior to uploading
            Schema.Partners.REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.EmbeddedFile file = new Schema.Partners.REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.EmbeddedFile();

            file._Type = "PDF";
            file._EncodingType = "Base64";
            file._Name = "BorrowercertificationAuthorization";
            file._Extension = ".pdf";

            //consolidate all borrower auth forms
            if (!TestMode)
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

                PDFOps pdfOp = new PDFOps();
                List<string> docsToSend = new List<string>() { };

                string docDLLocation = ConfigurationManager.AppSettings["EncDocumentDLLocation"];
                string baseWebAppLocation = ConfigurationManager.AppSettings["VOESystemBasePath"];

                //make sure these are in pdf format prior to consolidation
                foreach (DocumentOrderView doc in docs)
                {
                    //if this is a cloud doc, see if we need to download it
                    string newFileName = docDLLocation + doc.UniqueFileName;

                    if (doc.DocumentTypeName == "EncompassCloud" && !File.Exists(docDLLocation + doc.UniqueFileName))
                    {
                        newFileName = dOp.downloadEncLoanAttachment(dbConn, isNullInt(orderRequestId, 0), doc.UniqueFileName, order.LoanNumber);
                    }

                    docsToSend.Add(
                        pdfOp.convertToPDF(newFileName));
                }

                //document type borrower auth only = 1, writen 1005 request form AND borrower auth = 2
                if (order.OrderTypeId == 2)
                {
                    //generating and adding the written voe form to consolidation list
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    string FormTag = "ELMAVOERequest";
                    docsToSend.Add(RepositoryPath + "Documents\\ELMAVOERequest\\" +
                        oOp.createOrderRelatedForm(dbConn, orderRequestId, UserName, baseWebAppLocation, ref FormTag,
                        new FormReq.EmployerData { }, false));
                }

                //consolidate docs
                string FilePathName = RepositoryPath + "Documents\\Consolidated\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + order.LoanNumber + "_ADAttachments.pdf";
                pdfOp.consolidatePDFs(docsToSend, FilePathName);

                Byte[] fileBytes = File.ReadAllBytes(FilePathName);
                file.DOCUMENT = Convert.ToBase64String(fileBytes);
                verificationRequest.EMBEDDED_FILE = file;

            }

            verificationRequest._PRODUCT = product;
            extensionSectionData.VERIFICATION_REQUEST = verificationRequest;
            extensionSection.EXTENSION_SECTION_DATA = extensionSectionData;
            extension.EXTENSION_SECTION = extensionSection;
            requestData.EXTENSION = extension;

            //loan application section
            Schema.Partners.REQUEST_GROUP.Request.RequestData.LoanApplication loanApplication = new REQUEST_GROUP.Request.RequestData.LoanApplication();
            Schema.Partners.REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_ borrower = new REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_();
            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(order.BorrowerFullName, true);

            borrower._FirstName = borr.FirstName;
            borrower._LastName = borr.LastName;
            borrower._PrintPositionType = "Borrower";
            borrower._SSN = order.BorrowerSSN;
            borrower.BorrowerID = borrowerId;
            borrower._BirthDate = ((DateTime)order.BorrowerDOB).ToString("MM/dd/yyyy");

            Schema.Partners.REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_.Residence residence = new REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_.Residence();
            OrderAddress orderAddr = li.splitOrderAddress(order.BorrowerAddress);
            residence._StreetAddress = orderAddr.Street;
            residence._City = orderAddr.City;
            residence._State = orderAddr.State;
            residence._PostalCode = orderAddr.Zip;
            residence.BorrowerResidencyType = "Current";

            borrower._RESIDENCE = residence;

            Schema.Partners.REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_.Employer_ employer = new REQUEST_GROUP.Request.RequestData.LoanApplication.Borrower_.Employer_();
            OrderAddress empAddr = li.splitOrderAddress(order.EncEmployerAddress);
            employer._StreetAddress = empAddr.Street;
            employer._City = empAddr.City;
            employer._State = empAddr.State;
            employer._PostalCode = empAddr.Zip;
            employer._PhoneNumber = order.EncEmployerPhone;
            employer._Name = order.EncEmployerName;
            if (order.EncEmploymentStatus == "Current")
            {
                employer.EmploymentCurrentIndicator = "Y";
            }
            else
            {
                employer.EmploymentCurrentIndicator = "N";
            }

            borrower.Employer = employer;
            loanApplication.BORROWER = borrower;

            Schema.Partners.REQUEST_GROUP.Request.RequestData.LoanApplication.MortgageTerms mortgageTerms = new REQUEST_GROUP.Request.RequestData.LoanApplication.MortgageTerms();
            mortgageTerms.LenderCaseIdentifier = order.LoanNumber;
            loanApplication.MORTGAGE_TERMS = mortgageTerms;

            requestData.LOAN_APPLICATION = loanApplication;
            request.REQUEST_DATA = requestData;
            requestGroup.REQUEST = request;
            
            return requestGroup;


        }

        public REQUEST_GROUP createQuery(IDbConnection dbConn, int orderRequestId)
        {

            OrderRequest order = dbConn.Select<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();

            REQUEST_GROUP requestGroup = new REQUEST_GROUP();

            string mismoVersion = "2.3.1";
            
            //header setup
            requestGroup.MISMOVersionID = mismoVersion;

            REQUEST_GROUP.Request request = new REQUEST_GROUP.Request();
            request.LoginAccountIdentifier = ACCOUNTNUMBER;
            request.LoginAccountPassword = PASSWORD;

            REQUEST_GROUP.Request.RequestData requestData = new REQUEST_GROUP.Request.RequestData();
            REQUEST_GROUP.Request.RequestData.Extension extension = new REQUEST_GROUP.Request.RequestData.Extension();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection extensionSection = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData extensionSectionData = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData();
            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest verificationRequest = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest();

            verificationRequest.VendorOrderIdentifier = order.ADOrderNumber;
            verificationRequest._ActionType = "StatusQuery";
            verificationRequest._ItemType = "Verifications";
            verificationRequest._CreditReportTypeOtherDescription = "VOE";

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product product = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product();

            REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild typeProductChild = new REQUEST_GROUP.Request.RequestData.Extension.ExtensionSection.ExtensionSectionData.VerificationRequest.Product.ProductChild();
            if (order.OrderTypeId == 1)
            {
                //verbal
                typeProductChild._Description = "Verbal Voe";
                typeProductChild._Identifier = "VOE";
            }
            else
            {
                //written
                typeProductChild._Description = "Written Voe";
                typeProductChild._Identifier = "VOI";
            }
            product._TYPE = typeProductChild;

            verificationRequest._PRODUCT = product;
            extensionSectionData.VERIFICATION_REQUEST = verificationRequest;
            extensionSection.EXTENSION_SECTION_DATA = extensionSectionData;
            extension.EXTENSION_SECTION = extensionSection;
            requestData.EXTENSION = extension;
            request.REQUEST_DATA = requestData;
            requestGroup.REQUEST = request;

            return requestGroup;


        }



    }
}
