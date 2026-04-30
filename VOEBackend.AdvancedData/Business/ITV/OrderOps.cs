using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VOEBackend.AdvancedData.Schema.ITV;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOEBackend.AdvancedData.Business.ITV
{
    public class OrderOps : BaseClass
    {

        public string submitNewOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {


            string retVal = "Error creating Advanced Data order.";

            try
            {
                //get order information
                CommWrapper commwrap = createOrder(dbConn, orderRequestId, UserName, TestMode);
                CommOps.ResponseResult res;

                if (!TestMode)
                {
                    //this is production
                    CommOps comm = new CommOps();
                    res = comm.postOrderRequest(dbConn, commwrap, orderRequestId, UserName);
                }
                else
                {
                    //this is test mode
                    res = new CommOps.ResponseResult()
                    {
                        ADOrderNumber = "AD" + commwrap.Order.ThirdPartyOrderID,
                        Status = "Accepted"
                    };
                    logger.Info("Advanced Data Service Test Mode for Order " + res.ADOrderNumber);
                }


                if (res.Status == "Accepted")
                {
                    retVal = "Advanced Data order has been created";

                    logger.Info("Advanced Data order has been created for order " + commwrap.Order.ThirdPartyOrderID + ": ADOrderNumber " + res.ADOrderNumber);

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
                    logger.Error("Advanced Data order has been not been created for order " + commwrap.Order.ThirdPartyOrderID + ": " + res.ResultMessage,
                        new Exception("Error Creating Advanced Data Order"));
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
                CommWrapper commwrap = createStatusRequest(dbConn, orderRequestId);

                CommOps comm = new CommOps();
                result = comm.postStatusRequest(dbConn, commwrap, orderRequestId, UserName);

                if (result.Status == "Completed")
                {

                    logger.Info("Advanced Data Order Complete for Order: " + commwrap.Order.ThirdPartyOrderID);

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
                                logger.Error("Error Attaching Advanced Data Document to Order " + commwrap.Order.ThirdPartyOrderID + " " + file,
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
                else if (result.Status == "Canceled")
                {

                    logger.Info("Advanced Data Order Canceled for Order: " + commwrap.Order.ThirdPartyOrderID);

                    //update advanced data order status
                    OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                    order.ADOrderStatus = result.Status;
                    order.IsSubcontracted = false;
                    dbConn.UpdateOnly(order, q => new { q.ADOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                    //notify VOES to do something;
                    EmailOps eOp = new EmailOps();
                    eOp.sendTemplateEmail(dbConn, "Advanced Data Order Cancelled", orderRequestId, null, false, false, order.RequestTypeId, true);
                    
                }
                else if (result.Status == "Error")
                {
                    logger.Error("Advanced Data Status Query Error for Order " + commwrap.Order.ThirdPartyOrderID + ": " + result.ResultMessage,
                        new Exception("Error Querying Status of Advanced Data Order"));
                }
            } 
            catch (Exception ex)
            {
                logger.Error("Error Querying Advanced Data Order Status for OrderRequestId = " + orderRequestId, ex);
            }

            return result.Status;

        }

        public CommWrapper createOrder(IDbConnection dbConn, int orderRequestId, string UserName, bool TestMode)
        {

            OrderDetailView voeOrder = dbConn.Select<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            CommWrapper commwrap = new CommWrapper();
            Order order = new Order();
            
            order.OrderDate = DateTime.Now.ToString("MM/dd/yyyy");
            order.OrderTime = DateTime.Now.ToString("HH:mm:ss");
            order.ThirdPartyOrderID = voeOrder.OrderNumber;
            order.ClosingDate = "12/31/9999";

            if (voeOrder.RequestType == "Final")
            {
                order.OrderType = "3";
            }
            else if (voeOrder.EncEmploymentSelfFlag) {
                order.OrderType = "2";
            }
            else if (voeOrder.OrderType == "Verbal")
            {
                order.OrderType = "1";
            }
            else
            {   //written
                order.OrderType = "0";
            }

            order.CCEmails = "voe@firsthome.com";
            order.RushVOE = "0";
            order.LoanNum = voeOrder.LoanNumber;
            order.LoanOfficer = voeOrder.EncLoanOfficerName;
            /*order.LoanProcessor = "Processor";
            order.LoanParticipants = new List<Participant>
            {
                new Participant {
                    Name = "Participant Name",
                    Role = "Participant Role",
                    Company = "Participant Company",
                    Phone = "Participant Phone",
                    Email = "Participant Email"
                }
            };*/

            commwrap.Order = order;

            //borrower information
            LoanInfoOps li = new LoanInfoOps();
            BorrowerName borr = li.splitBorrowerName(voeOrder.BorrowerFullName, false);
            OrderAddress borrAddress = li.splitOrderAddress(voeOrder.BorrowerAddress);

            Schema.ITV.Borrower borrower = new Schema.ITV.Borrower();
            borrower.FirstName = borr.FirstName;
            borrower.MiddleName = String.Empty;
            borrower.LastName = borr.LastName;
            borrower.NameSuffix = String.Empty;
            borrower.SSN = voeOrder.BorrowerSSN;
            borrower.BirthDate = voeOrder.BorrowerDOB.ToString("MM/dd/yyyy");
            borrower.StreetAddress1 = borrAddress.Street;
            borrower.StreetAddress2 = String.Empty;
            borrower.City = borrAddress.City;
            borrower.State = borrAddress.State;
            borrower.Zip = borrAddress.Zip;
            borrower.Phone = "000-000-0000";
            borrower.AuthOnFile = "1";

            commwrap.Borrower = borrower;

            //employer information
            OrderAddress emplAddress = li.splitOrderAddress(voeOrder.EncEmployerAddress);

            Schema.ITV.Employer employer = new Schema.ITV.Employer();
            employer.OrderType = "1"; //primary borrower
            employer.CompanyName = voeOrder.EncEmployerName;
            employer.Position = voeOrder.EncEmploymentTitle;
            employer.EmpAddress = emplAddress.Street;
            employer.EmpAddress2 = String.Empty;
            employer.EmpCity = emplAddress.City;
            employer.EmpState = emplAddress.State;
            employer.EmpZipCode = emplAddress.Zip;
            employer.Phone1 = voeOrder.EncEmployerPhone;
            employer.FaxNumber = isEmpty(voeOrder.EncEmployerFax,"999-999-9999");
            employer.HRContact = "0000";
            employer.HREmail = voeOrder.EncEmployerEmail;
            employer.Requestor = "First Home Mortgage";
            if ( voeOrder.EncEmploymentStatus == "Current") 
            {
                employer.EmpType = "1";
            } else {
                employer.EmpType = "0";
            }

            commwrap.Employer = employer;

            //add forms.  need the borrower authorization form for all orders.
            //for written voes we need to concat the written authorization form onto the borrower auth form prior to uploading
            Schema.ITV.DocumentVoE form = new Schema.ITV.DocumentVoE();

            form.FileType = "PDF";
            form.Encoding = "Base64";

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
                    
                    if (docItems.Count > 0) {
                        //need to requery here since we need the updated docs object later on
                        docs = dbConn.Where<DocumentOrderView>(q => q.DocumentTypeName == "EncompassCoud"
                            && q.EncDocumentName.Contains("Borrower")
                            && q.EncDocumentName.Contains("Certification")
                            && q.OrderRequestId == orderRequestId);

                    } else {

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
                        newFileName = dOp.downloadEncLoanAttachment(dbConn, isNullInt(orderRequestId, 0), doc.UniqueFileName, voeOrder.LoanNumber);
                    }

                    docsToSend.Add(
                        pdfOp.convertToPDF(newFileName));
                }

                //document type borrower auth only = 1, writen 1005 request form AND borrower auth = 2
                if (voeOrder.OrderType == "Written")
                {
                    form.DocumentType = "2";
                    //generating and adding the written voe form to consolidation list
                    VOESystem.Data.Business.OrderOps oOp = new VOESystem.Data.Business.OrderOps();
                    string FormTag = "ELMAVOERequest";
                    docsToSend.Add(RepositoryPath + "Documents\\ELMAVOERequest\\" +
                        oOp.createOrderRelatedForm(dbConn, orderRequestId, UserName, baseWebAppLocation, ref FormTag,
                        new FormReq.EmployerData { }, false));
                }
                else
                {
                    form.DocumentType = "1";
                }

                //consolidate docs
                string FilePathName = RepositoryPath + "Documents\\Consolidated\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + voeOrder.LoanNumber + "_ADAttachments.pdf";
                pdfOp.consolidatePDFs(docsToSend, FilePathName);

                Byte[] fileBytes = File.ReadAllBytes(FilePathName);
                form.Content = Convert.ToBase64String(fileBytes);
                commwrap.DocumentVoE = form;
            }

            return commwrap;

        }

        public CommWrapper createStatusRequest(IDbConnection dbConn, int orderRequestId)
        {

            CommWrapper commwrap = new CommWrapper();
            Order order = new Order();

            OrderDetailView orddetail = dbConn.Where<OrderDetailView>(q => q.OrderRequestId == orderRequestId).FirstOrDefault();

            order.ThirdPartyOrderID = orddetail.OrderNumber;
            order.VoEOrderID = orddetail.ADOrderNumber;

            commwrap.Order = order;

            return commwrap;

        }

        public string requestOrderCancellation(IDbConnection dbConn, int orderRequestId, string UserName)
        {
            string retVal = "Error Requesting Order Cancellation";

            try
            {
             
                //update advanced data order status
                OrderRequest order = dbConn.Where<OrderRequest>(q => q.Id == orderRequestId).FirstOrDefault();
                order.ADOrderStatus = "Pending Cancellation";
                order.IsSubcontracted = false;
                dbConn.UpdateOnly(order, q => new { q.ADOrderStatus, q.IsSubcontracted }, r => r.Id == orderRequestId);

                //send order cancellation request
                EmailOps eOp = new EmailOps();
                eOp.sendTemplateEmail(dbConn, "Advanced Data Request Order Cancellation", orderRequestId, null, false, false, order.RequestTypeId, true);

                logger.Info("Advanced Data Order Cancellation Requested for Order: " + order.LoanNumber + '-' + Int32.Parse(order.OrderSuffix).ToString("00"));

                retVal = "OK";

            }
            catch (Exception ex)
            {
                logger.Error("Error Requesting AD Order Cancellation", ex);
            }


            return retVal;

        }
        
        public void checkOutstandingOrderStatus()
        {

            try
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
                OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
                OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
                dbConn.Open();

                List<OrderRequest> orders = dbConn.Where<OrderRequest>(q => q.ADOrderStatus == "Accepted" || q.ADOrderStatus == "Pending Cancellation").ToList();

                foreach (OrderRequest order in orders)
                {
                    string orderStatus = queryOrderStatus(dbConn, order.Id, "voesystem");
                    logger.Info("Advanced Data Order " + order.ADOrderNumber + " Status is: " + orderStatus);

                }

            } 
            catch (Exception ex)
            {
                logger.Error("Error Checking Status of AD Orders", ex);
            }

        }

    }
}
