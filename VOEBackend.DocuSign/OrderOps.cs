using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOESystem.Data.DBSchema;
using ServiceStack.OrmLite;
using System.IO;
using DocuSignSDK = DocuSign;
using System.Configuration;
using ServiceStack.Text;

namespace VOEBackend.DocuSign
{
    public class OrderOps : BaseClass
    {

        public class Recipient
        {
            public int RecipientId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public bool IsEmbedded { get; set; }
            public int OrderIndex { get; set; }
            public RecipientType RecipientType { get; set; }
            public List<SignatureDoc> SignatureDocs { get; set; }

            public class SignatureDoc
            {
                public int DocumentId { get; set; }
                public int XPos { get; set; }
                public int YPos { get; set; }
                public int DateXPos { get; set; }
                public int DateYPos { get; set; }
            }

        }

        public enum RecipientType 
        {
            Signer = 1,
            CarbonCopy = 2,
            EmbeddedSigner = 3
        }
        
        public VOESystem.Data.DTO.DocuSignResult requestSignatureOnDocument(IDbConnection dbConn,int OrderRequestId, List<Recipient> Recipients,
            List<int> Documents, string BaseUrl, string Subject, string MessageBody, List<string> InactiveFieldGroups)
        {
            //return null if no embedded recipient
            VOESystem.Data.DTO.DocuSignResult retVal = new VOESystem.Data.DTO.DocuSignResult
            {
                StatusMessage = "Error Requesting DocuSign Signature: ",
                EmbeddedSigningURL = null,
                EnvelopeId = null
            };

            //create authorization object here so we don't have to do work if can't auth
            CommOps cOp = new CommOps();

            //create dbconnn if non
            if (dbConn == null)
            {

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                dbConn = factory.CreateDbConnection();
                dbConn.Open();
            }

            //assert that there are either one or zero embedded recipeients in this list
            if (Recipients.Where<Recipient>(q => q.IsEmbedded == true).ToList().Count > 1)
            {
                throw new CustomException.TooManyEmbeddedRecipientsException("OrderRequestId = " + OrderRequestId);
            }

            //this automatically sends by default
            string status = "sent";

            //get loannumber and suffix to create subject tag
            DocuSignFieldValueView order = dbConn.Where<DocuSignFieldValueView>(q => q.OrderRequestId == OrderRequestId).FirstOrDefault();

            string loanNumber = order.LoanNumber;
            string ordersuffix = int.Parse(order.OrderSuffix).ToString("00");

            string subjectTag = " [" + loanNumber + "-" + ordersuffix + "]";

            //build email message
            DocuSignSDK.eSign.Model.EnvelopeDefinition envDef = new DocuSignSDK.eSign.Model.EnvelopeDefinition();
            envDef.EmailSubject = Subject + subjectTag;
            envDef.EmailBlurb = MessageBody;

            //queue up the recipients 
            List<DocuSignSDK.eSign.Model.Signer> docSigners = new List<DocuSignSDK.eSign.Model.Signer>() {};
            List<DocuSignSDK.eSign.Model.CarbonCopy> docCarbonCopies = new List<DocuSignSDK.eSign.Model.CarbonCopy>() {};

            foreach (Recipient recip in Recipients.OrderBy(q => q.OrderIndex).ToList()) {
                if (recip.RecipientType == RecipientType.Signer || recip.RecipientType == RecipientType.EmbeddedSigner)
                {
                    DocuSignSDK.eSign.Model.Signer signer = new DocuSignSDK.eSign.Model.Signer();
                    signer.RecipientId = recip.RecipientId.ToString();
                    signer.Email = recip.Email;
                    signer.Name = recip.Name;
                    if (recip.IsEmbedded)
                    {
                        //just needs to be non-null to indicate embedded
                        signer.ClientUserId = recip.RecipientId.ToString();
                    }

                    //init tab collections
                    signer.Tabs = new DocuSignSDK.eSign.Model.Tabs();
                    signer.Tabs.SignHereTabs = new List<DocuSignSDK.eSign.Model.SignHere>();
                    signer.Tabs.DateSignedTabs = new List<DocuSignSDK.eSign.Model.DateSigned>();
                    signer.Tabs.ApproveTabs = new List<DocuSignSDK.eSign.Model.Approve>();

                    docSigners.Add(signer);
                } 
                else if (recip.RecipientType == RecipientType.CarbonCopy) 
                {
                    DocuSignSDK.eSign.Model.CarbonCopy cc = new DocuSignSDK.eSign.Model.CarbonCopy();
                    cc.RecipientId = recip.RecipientId.ToString();
                    cc.Email = recip.Email;
                    cc.Name = recip.Name;

                    docCarbonCopies.Add(cc);

                }
            }

            envDef.Recipients = new DocuSignSDK.eSign.Model.Recipients();
            envDef.Documents = new List<DocuSignSDK.eSign.Model.Document>();
            envDef.Recipients.Signers = docSigners;
            envDef.Recipients.CarbonCopies = docCarbonCopies;

            foreach (int DocumentId in Documents)
            {

                // the document we want signed
                DocumentOrderView doc = dbConn.Where<DocumentOrderView>(q => q.DocumentId == DocumentId).FirstOrDefault();

                //determine doc path
                string repoSubFolder = DocumentRepositorySubFolder(doc.DocumentTypeName, doc.FormTag);
                string docFilePath = Path.Combine(RepositoryPath, repoSubFolder, doc.UniqueFileName);

                // Read a file from disk to use as a document.
                byte[] fileBytes = File.ReadAllBytes(docFilePath);

                // Add a document to the envelope
                DocuSignSDK.eSign.Model.Document eSignDoc = new DocuSignSDK.eSign.Model.Document();
                eSignDoc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
                eSignDoc.Name = doc.UniqueFileName;
                eSignDoc.DocumentId = doc.DocumentId.ToString();
                //eSignDoc.SignerMustAcknowledge = "accept"; //this is ignored when signing tabs are present

                envDef.Documents.Add(eSignDoc);

                //need signers in original order
                foreach (Recipient recip in Recipients.OrderBy(q => q.OrderIndex).ToList()) {
                    if (recip.RecipientType == RecipientType.Signer || recip.RecipientType == RecipientType.EmbeddedSigner)
                    {

                        DocuSignSDK.eSign.Model.Signer signer = envDef.Recipients.Signers
                               .Where(q => q.RecipientId == recip.RecipientId.ToString()).FirstOrDefault();

                        //if ((recip.RecipientType == RecipientType.Signer || recip.RecipientType == RecipientType.EmbeddedSigner)
                        if (recip.SignatureDocs.Where(q => q.DocumentId == DocumentId).ToList().Count() > 0) //only if this is the signing doc and not just supplementary
                        {
                            Recipient.SignatureDoc signDocInfo = recip.SignatureDocs.Where(q => q.DocumentId == DocumentId).FirstOrDefault();

                            // Create a |SignHere| tab somewhere on the document for the recipient to sign
                            DocuSignSDK.eSign.Model.SignHere signHere = new DocuSignSDK.eSign.Model.SignHere();
                            signHere.DocumentId = DocumentId.ToString();
                            signHere.PageNumber = "1"; //right now all are only 1 page.  add this to formdocumentrecipeient if two page docs come up
                            signHere.RecipientId = signer.RecipientId;
                            signHere.XPosition = signDocInfo.XPos.ToString();
                            signHere.YPosition = signDocInfo.YPos.ToString();
                            signHere.ScaleValue = "1";
                            signer.Tabs.SignHereTabs.Add(signHere);

                            if (signDocInfo.DateXPos != 0)
                            {
                                //add date signed field
                                DocuSignSDK.eSign.Model.DateSigned dateSignHere = new DocuSignSDK.eSign.Model.DateSigned();
                                dateSignHere.DocumentId = DocumentId.ToString();
                                dateSignHere.PageNumber = "1"; //right now all are only 1 page.  add this to formdocumentrecipeient if two page docs come up
                                dateSignHere.RecipientId = signer.RecipientId;
                                dateSignHere.XPosition = signDocInfo.DateXPos.ToString();
                                dateSignHere.YPosition = signDocInfo.DateYPos.ToString();
                                dateSignHere.Font = "Helvetica";
                                dateSignHere.FontSize = "Size10";
                                signer.Tabs.DateSignedTabs.Add(dateSignHere);
                            }


                            addDocumentFields(dbConn, ref signer, DocumentId, order, InactiveFieldGroups);

                        }
                        else if (recip.RecipientType == RecipientType.Signer)
                        {
                            //must be a supporting doc
                            // Create a |Acknowledge| tab
                            DocuSignSDK.eSign.Model.Approve approve = new DocuSignSDK.eSign.Model.Approve();
                            approve.DocumentId = DocumentId.ToString();
                            approve.PageNumber = "1"; //right now all are only 1 page.  add this to formdocumentrecipeient if two page docs come up
                            approve.RecipientId = signer.RecipientId;

                            signer.Tabs.ApproveTabs.Add(approve);

                        }

                    }
                }
            }

            //Log
            //logger.Info(envDef.Recipients.ToJson<DocuSignSDK.eSign.Model.Recipients>().ToString());
            
            // set envelope status to "sent" to immediately send the signature request
            envDef.Status = status;

            envDef.Notification = new DocuSignSDK.eSign.Model.Notification(
                new DocuSignSDK.eSign.Model.Expirations("60", "true"));

            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)
            DocuSignSDK.eSign.Api.EnvelopesApi envelopesApi = new DocuSignSDK.eSign.Api.EnvelopesApi(cOp.apiClient.Configuration);
            DocuSignSDK.eSign.Model.EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(cOp.accountId, envDef);

            if (envelopeSummary == null) 
            {
                throw new CustomException.EnvelopeNotCreatedException("Envelope is null");
            } 
            else if (envelopeSummary.EnvelopeId == null) 
            {
                throw new CustomException.EnvelopeNotCreatedException("EnvelopeId is null");
            }

            retVal.EnvelopeId = envelopeSummary.EnvelopeId;

            //there can only be zero or 1 embedded recipients
            Recipient embeddedRecip = Recipients.Where(q => q.IsEmbedded == true).FirstOrDefault();
            
            if (embeddedRecip != null)
            {
                string returnUrl = BaseUrl + "/order-detail/" + OrderRequestId.ToString();

                //generate link for embedded (current user) signer
                DocuSignSDK.eSign.Model.RecipientViewRequest viewOptions = new DocuSignSDK.eSign.Model.RecipientViewRequest()
                {
                    ReturnUrl = returnUrl,  //should be order detail they were working on
                    ClientUserId = embeddedRecip.RecipientId.ToString(),  // must match clientUserId of the embedded recipient
                    AuthenticationMethod = "email",
                    UserName = embeddedRecip.Name,
                    Email = embeddedRecip.Email
                };

                // create the recipient view (aka signing URL)
                DocuSignSDK.eSign.Model.ViewUrl recipientView = envelopesApi.CreateRecipientView(cOp.accountId, envelopeSummary.EnvelopeId, viewOptions);
                retVal.EmbeddedSigningURL = recipientView.Url;

            }

            //set return values
            retVal.StatusMessage = "DocuSign Request Sent";

            return retVal;
        }

        public void addDocumentFields(IDbConnection dbConn, ref DocuSignSDK.eSign.Model.Signer Signer, int DocumentId, DocuSignFieldValueView order, List<string> InactiveFieldGroups) {

            //lookup to see if there are any fields for this document/signer
            int FormDocuSignRecipientId = Int32.Parse(Signer.RecipientId);
            List<FormDocuSignField> fields = dbConn.Where<FormDocuSignField>(q => q.FormDocuSignRecipientId == FormDocuSignRecipientId).ToList();

            if (fields.Count == 0) { return; };

            //get list of field types for reference
            List<DocuSignFieldType> DocuSignFieldTypes = dbConn.Select<DocuSignFieldType>().ToList();

            foreach (FormDocuSignField field in fields)
            {
                string fieldTypeName = DocuSignFieldTypes.Where(q => q.Id == field.DocuSignFieldTypeId).FirstOrDefault().Name;
                if (field.ValueField != null) { field.Value = typeof(DocuSignFieldValueView).GetProperty(field.ValueField).GetValue(order).ToString(); };

                if (fieldTypeName =="Date") {
                    Signer.Tabs.DateTabs = addTab<DocuSignSDK.eSign.Model.Date>(Signer.Tabs.DateTabs, field, DocumentId, InactiveFieldGroups);
                } else if (fieldTypeName == "Date or Text") {
                    Signer.Tabs.DateTabs = addTab<DocuSignSDK.eSign.Model.Date>(Signer.Tabs.DateTabs, field, DocumentId, InactiveFieldGroups, @"^\d{2}\/\d{2}\/\d{4}|[a-zA-Z\s]+$");
                } else if (fieldTypeName =="Full Name") {
                    Signer.Tabs.FullNameTabs = addTab<DocuSignSDK.eSign.Model.FullName>(Signer.Tabs.FullNameTabs, field, DocumentId, InactiveFieldGroups);
                } else if (fieldTypeName =="Number") {
                    Signer.Tabs.NumberTabs = addTab<DocuSignSDK.eSign.Model.Number>(Signer.Tabs.NumberTabs, field, DocumentId, InactiveFieldGroups);
                } else if (fieldTypeName =="Radio Group" && !InactiveFieldGroups.Contains(isNull(field.GroupName,""))) {
                    if (Signer.Tabs.RadioGroupTabs == null) {
                        Signer.Tabs.RadioGroupTabs = new List<DocuSignSDK.eSign.Model.RadioGroup>() { };
                    };
                    DocuSignSDK.eSign.Model.RadioGroup radioGroup = Signer.Tabs.RadioGroupTabs.Where(q => q.GroupName == field.GroupName).FirstOrDefault();
                    if (radioGroup == null)
                    {
                        radioGroup = new DocuSignSDK.eSign.Model.RadioGroup();
                        radioGroup.GroupName = field.GroupName;
                        radioGroup.DocumentId = DocumentId.ToString();
                        radioGroup.Radios = addTab<DocuSignSDK.eSign.Model.Radio>(radioGroup.Radios, field, DocumentId, InactiveFieldGroups);
                        setTabConditions(ref radioGroup, field, InactiveFieldGroups);
                        Signer.Tabs.RadioGroupTabs.Add(radioGroup);
                    }
                    else
                    {
                        radioGroup.Radios = addTab<DocuSignSDK.eSign.Model.Radio>(radioGroup.Radios, field, DocumentId, InactiveFieldGroups);
                    }
                } else if (fieldTypeName == "Text") {
                    Signer.Tabs.TextTabs = addTab<DocuSignSDK.eSign.Model.Text>(Signer.Tabs.TextTabs, field, DocumentId, InactiveFieldGroups);
                }
            }

        }

        public List<T> addTab<T>(List<T> TabList, FormDocuSignField Field, int DocumentId, List<string> InactiveFieldGroups, string ValidationPattern = null)
            where T : new()
        {
            if (TabList == null)
            {
                TabList = new List<T>() { };
            }

            T newTab = new T();

            setTabProperty(ref newTab, "XPosition", Field.XPos.ToString());
            setTabProperty(ref newTab, "YPosition", Field.YPos.ToString());
            setTabProperty(ref newTab, "PageNumber", "1");
            setTabProperty(ref newTab, "Font", "Helvetica");
            setTabProperty(ref newTab, "FontSize", "Size10");
            setTabProperty(ref newTab, "Width", Field.Width ?? 100);
            setTabProperty(ref newTab, "Height", Field.Height ?? 22);
            setTabProperty(ref newTab, "DocumentId", DocumentId.ToString());
            setTabProperty(ref newTab, "TabId", Field.Id.ToString());
            setTabProperty(ref newTab, "TabLabel", Field.Id.ToString());
            setTabProperty(ref newTab, "TabOrder", Field.OrderIndex.ToString());
            setTabProperty(ref newTab, "Name", Field.Description);
            setTabProperty(ref newTab, "RecipientId", Field.FormDocuSignRecipientId.ToString());
            setTabProperty(ref newTab, "Required", Field.RequiredField ? "True" : "False");
            
            if (Field.Value != null)
            {
                setTabProperty(ref newTab, "Value", Field.Value);
                setTabProperty(ref newTab, "Locked", Field.Readonly ? "True" : "False");
            }

            if (Field.ConditionValue != null)
            {
                setTabProperty(ref newTab, "Value", Field.ConditionValue.ToString());
            }

            if (ValidationPattern != null)
            {
                setTabProperty(ref newTab, "ValidationPattern", ValidationPattern);
            }

            setTabConditions(ref newTab, Field, InactiveFieldGroups);

            TabList.Add(newTab);

            return TabList;

        }

        public void setTabProperty<T>(ref T newTab, string propertyName, object propertyValue )
        {
            if (typeof(T).GetProperty(propertyName) != null)
            {
                typeof(T).GetProperty(propertyName).SetValue(newTab, propertyValue.ToString());
            }

        }

        public void setTabConditions<T>(ref T newTab, FormDocuSignField Field, List<string> InactiveFieldGroups)
        {
            if (Field.ConditionalParentLabel != null)
            {
                if (!InactiveFieldGroups.Contains(Field.ConditionalParentLabel))
                {
                    setTabProperty(ref newTab, "ConditionalParentLabel", Field.ConditionalParentLabel);
                    setTabProperty(ref newTab, "ConditionalParentValue", Field.ConditionalParentValue);
                }
            }

        }

        public void createEnvenlopeFromEnvelope(IDbConnection dbConn, string envelopeId)
        {


            throw new NotImplementedException();

            //create authorization object here so we don't have to do work if can't auth
            CommOps cOp = new CommOps();

            //create dbconnn if non
            if (dbConn == null)
            {

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                dbConn = factory.CreateDbConnection();
                dbConn.Open();
            }

            DocuSignSDK.eSign.Api.EnvelopesApi envelopesApi = new DocuSignSDK.eSign.Api.EnvelopesApi(cOp.apiClient.Configuration);

            DocuSignSDK.eSign.Model.Envelope env = envelopesApi.GetEnvelope(cOp.accountId, envelopeId, new DocuSignSDK.eSign.Api.EnvelopesApi.GetEnvelopeOptions
            {
               advancedUpdate = "true",
               include = "recipients,documents"  //this may not be sufficient.  we will see
            });

            



        }

    }
}
