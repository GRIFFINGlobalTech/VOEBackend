using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
//using EllieMae.Encompass.BusinessObjects.Loans;
//using EllieMae.Encompass.BusinessObjects.Loans.Logging;
//using EllieMae.Encompass.Collections;
//using EllieMae.Encompass.Query;
using ServiceStack.OrmLite;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;


namespace VOEBackend.Encompass
{
    public class Documents : BaseClass
    {

        public void cleanupEncDoucments()
        {
            //get list of documents in Loan Originated with scheduled closing date < today - 30 days and > today - 45 days
            //and remove the documents from the system since Encompass is the system of record for those docs

            int docId = 0;

            try
            {

                

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                DocumentOps dop = new DocumentOps();

                List<DocumentMaintView> delDocs = dbConn.Select<DocumentMaintView>();

                foreach (DocumentMaintView doc in delDocs)
                {
                    docId = doc.DocumentId;
                    Log.Info("Removing DocId = " + docId);
                    dop.removeDocument(dbConn, doc.DocumentId);
                }

                dbConn.Close();
                dbConn.Dispose();

            } 
            catch (Exception ex)
            {
                Log.Error("Error Removing DocumentId: " + docId.ToString(), ex);
            }


        }

        //public void UpdateVOEDocs(ref Loan loan)
        //{//not used

        //    try
        //    {

        //        //get list of documents to update
        //        string sEncDocuments = ConfigurationManager.AppSettings["EncDocumentsForDL"];

        //        List<string> EncDocuments = new List<string>() { };
        //        EncDocuments.Add("Borrower's Certification and Authorization");
        //        EncDocuments.Add("Borrower's Certification & Authorization (Brokered)");
        //        //EncDocuments.Add("Non Borrower Request for Information");

        //        string docDLLocation = ConfigurationManager.AppSettings["EncDocumentDLLocation"];

        //        //get list of current documents for this loan
        //        OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
        //            ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
        //            true, SqlServerDialect.Provider);

        //        IDbConnection dbConn = factory.CreateDbConnection();
        //        dbConn.Open();

        //        List<string> dlist = dbConn.SqlList<string>("SELECT UniqueFileName FROM DocumentOrderView "
        //            + "WHERE LoanNumber = '" + loan.LoanNumber + "'");

        //        //loop through documents
        //        foreach (TrackedDocument document in loan.Log.TrackedDocuments)
        //        {
        //            if (EncDocuments.Contains(document.Title))
        //            {
        //                //loop through files associated with document
        //                AttachmentList atts = document.GetAttachments();

        //                foreach (Attachment att in atts)
        //                {
        //                    //sometimes encompass docs dont have extensions now
        //                    string fileExtension = string.Empty;
        //                    if ( getFileExtension(att.Name,".") == String.Empty ) {
        //                        fileExtension = ".pdf";
        //                    } 

        //                    if (!dlist.Contains(att.Name + fileExtension))
        //                    {
        //                        //add new document
        //                        string newFileName = docDLLocation + att.Name + fileExtension;
        //                        if (File.Exists(newFileName))
        //                        {
        //                            File.Delete(newFileName);
        //                        }
        //                        att.SaveToDisk(newFileName);
        //                        Log.Info("File Downloaded for Loan: " + loan.LoanNumber + ": " + newFileName);

        //                        //add to db
        //                        dbConn.Insert<Document>(new Document
        //                        {
        //                            LoanNumber = loan.LoanNumber,
        //                            OrderRequestId = null,
        //                            EncDocumentName = document.Title,
        //                            FileDisplayName = att.Title,
        //                            UniqueFileName = att.Name + fileExtension,
        //                            FileDateTime = att.Date,
        //                            DocumentTypeId = 1
        //                        });
        //                    }
        //                }
        //            }

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Downloading Encompass Docs for Loan: " + loan.LoanNumber, ex);
        //    }

        //}

        public void deleteUnsupportedDocuments()
        {

            string RepositoryPath = ConfigurationManager.AppSettings["DocumentRepositoryProd"].ToString();

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            List<UnsupportedDocumentView> delDocs = dbConn.Select<UnsupportedDocumentView>();

            foreach (UnsupportedDocumentView docM in delDocs)
            {

                using (IDbTransaction tr = dbConn.BeginTransaction())
                {

                    Document doc = dbConn.Where<Document>(q => q.Id == docM.Id).FirstOrDefault();

                    //delete file from local directory
                    string documentPath = Path.Combine(RepositoryPath, "EncDocuments\\", doc.UniqueFileName);
                    File.Delete(documentPath);

                    List<EmailAttachment> emls = dbConn.Where<EmailAttachment>(q => q.DocumentId == docM.Id).ToList();

                    foreach (EmailAttachment eml in emls)
                    {
                        dbConn.Delete(eml);
                    }

                    //DocumentUpload dup = dbConn.Where<DocumentUpload>(q => q.DocumentId == docM.Id).FirstOrDefault();
                    //if (dup != null)
                    //{
                    //    dbConn.Delete(dup);
                    //}

                    //delete document record
                    dbConn.Delete(doc);

                    tr.Commit();

                    Log.Info("Document Removed: " + doc.UniqueFileName);
                }
              
            }

            dbConn.Close();
            dbConn.Dispose();


        }

        public class UnsupportedDocumentView
        {
            public int Id { get; set; }
            public string LoanNumber { get; set; }
            public int OrderRequestId { get; set; }
            public string EncDocumentName { get; set; }
            public string FileDisplayName { get; set; }
            public DateTime FileDateTime { get; set; }
            public string UniqueFileName { get; set; }
            public int DocumentTypeId { get; set; }
            public int FormTypeId { get; set; }
            public bool Deleted { get; set; }
            public string FileExtension { get; set; }

        }

        //public void downloadEncDocumentSDK(List<string> docBuckets, string folderPath, string loanID, string UserName, string Password, string[] LoanFolders, object encompasssession ) {


        //        Loan loan = null;

        //        EllieMae.Encompass.Client.Session emSession = null;
    
        //        Log.Info("Starting Doc Download for " + loanID);

        //        try
        //        {
        //            //start encompass session
        //            if (encompasssession == null)
        //            {
        //                emSession = new EllieMae.Encompass.Client.Session();
        //                emSession.Start(encompassServer, UserName, Password);
        //            }
        //            else
        //            {
        //                emSession = (EllieMae.Encompass.Client.Session)encompasssession;
        //            }

        //            // Fetch the loan folder

        //            //*** Define QUERY Criteria
        //            // Build the string criterion
        //            StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
        //            loanIDCriterion.FieldName = "Fields.364";
        //            loanIDCriterion.Value = loanID.Trim();
        //            loanIDCriterion.MatchType = StringFieldMatchType.Exact;

        //            //add folder criteria
        //            QueryCriterion folderCriteria = null;

        //            foreach (string loanfolder in LoanFolders)
        //            {
        //                StringFieldCriterion folderCriterion = new StringFieldCriterion();
        //                folderCriterion.FieldName = "Loan.LoanFolder";
        //                folderCriterion.Value = loanfolder;
        //                folderCriterion.MatchType = StringFieldMatchType.Exact;

        //                if (folderCriteria == null)
        //                {
        //                    folderCriteria = folderCriterion;
        //                }
        //                else
        //                {
        //                    folderCriteria = folderCriteria.Or(folderCriterion);
        //                }
        //            }

        //            // Join the criteria together using AND logic
        //            QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

        //            // Perform the query, retrieving the identities of the matching loans
        //            LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

        //            //should only return one loan
        //            if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }

        //            loan = emSession.Loans.Open(ids[0].Guid);

        //            foreach (TrackedDocument document in loan.Log.TrackedDocuments)
        //            {
        //                if (docBuckets.Contains(document.Title))
        //                {
        //                    //make sure there is an actual file there
        //                    AttachmentList atts = document.GetAttachments();
        //                    if (atts.Count > 0)
        //                    {
        //                        foreach (Attachment att in atts)
        //                        {
        //                            if (att.Size > 0)
        //                            {
        //                                byte[] attBytes = att.DataOriginal;

        //                                string fileExt = getAttachmentOriginalFileExtensionREST(att.Name, loan.Guid, loanID);
        //                                string fileName = cleanEncDocName(loan.BorrowerPairs[0].Borrower.LastName + loan.BorrowerPairs[0].Borrower.FirstName) 
        //                                    + "_" + cleanEncDocName(document.Title) + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExt;
        //                                string filePathName = Path.Combine(folderPath, fileName);

        //                                if (File.Exists(filePathName)) { File.Delete(filePathName); };
        //                                File.WriteAllBytes(filePathName, attBytes);
        //                                Log.Info("Document Downloaded: " + filePathName);
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error("Error Downloading Encompass Document", ex);
        //        }
        //        finally
        //        {
        //            if (loan != null)
        //            {
        //                loan.Close();
        //            }
        //        }

        //}

        public string cleanEncDocName(string fileString)
        {
            fileString = fileString.Replace(".", String.Empty);
            fileString = fileString.Replace(",", String.Empty);
            fileString = fileString.Replace("'", String.Empty);
            fileString = fileString.Replace("@", String.Empty);
            fileString = fileString.Replace("<", String.Empty);
            fileString = fileString.Replace(">", String.Empty);
            fileString = fileString.Replace("$", String.Empty);
            fileString = fileString.Replace("&", String.Empty);
            fileString = fileString.Replace(":", String.Empty);
            fileString = fileString.Replace("|", String.Empty);
            fileString = fileString.Replace("*", String.Empty);
            fileString = fileString.Replace("_", String.Empty);
            fileString = fileString.Replace("-", String.Empty);
            fileString = fileString.Replace(" ", String.Empty);

            return fileString;
        }

        public string getAttachmentOriginalFileExtensionREST(string attachmentId, string loanGUID, string loanNumber)
        {
            string retVal = String.Empty;

            FHMC.EncompassREST.Authentication auth = new FHMC.EncompassREST.Authentication();
            string accessToken = auth.getAccessToken();

            FHMC.EncompassREST.Documents restDoc = new FHMC.EncompassREST.Documents();
            FHMC.EncompassREST.Documents.Attachment restAtt = restDoc.getAttachmentForLoan(loanGUID, attachmentId, accessToken, loanNumber);
            string nativeFileName = restAtt.pages[0].originalKey;

            retVal = Path.GetExtension(nativeFileName);

            return retVal;

        }

    }
}
