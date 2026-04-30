using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.Business;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.UI.Model
{
    public class OrderDetail : PageObjectBase
    {

        public OrderDetail(NgWebDriver ngDriver)
            : base(ngDriver) {

            BorrowerInformation = new _BorrowerInformation(ngDriver);
            EmploymentInformation = new _EmploymentInformation(ngDriver);
            OrderInformation = new _OrderInformation(ngDriver);
            OrderActivity = new _OrderActivity(ngDriver);
            RelatedOrdersInformation = new _RelatedOrdersInformation(ngDriver);
            VendorEditPopup = new _VendorEditPopup(ngDriver);
            EmailComposePopup = new _EmailComposePopup(ngDriver);
            EmailHistoryPopup = new _EmailHistoryPopup(ngDriver);
            LinkedDocsPopup = new _LinkedDocsPopup(ngDriver);
            UploadDocsPopup = new _UploadDocsPopup(ngDriver);
            BulkEditPopup = new _BulkEditPopup(ngDriver);
            SubcontractPopup = new _SubcontractPopup(ngDriver);
            
        }

        public _BorrowerInformation BorrowerInformation { get; set; }
        public _EmploymentInformation EmploymentInformation { get; set; }
        public _OrderInformation OrderInformation { get; set; }
        public _OrderActivity OrderActivity { get; set; }
        public _RelatedOrdersInformation RelatedOrdersInformation { get; set; }
        public _VendorEditPopup VendorEditPopup { get; set; }
        public _EmailComposePopup EmailComposePopup { get; set; }
        public _EmailHistoryPopup EmailHistoryPopup { get; set; }
        public _LinkedDocsPopup LinkedDocsPopup { get; set; }
        public _UploadDocsPopup UploadDocsPopup { get; set; }
        public _BulkEditPopup BulkEditPopup { get; set; }
        public _SubcontractPopup SubcontractPopup { get; set; }
        
        public class _BorrowerInformation : PageObjectBase
        {
            public _BorrowerInformation(NgWebDriver ngDriver)
               : base(ngDriver) { }
            
            public TextBox BorrowerFullNameTextBox
            {
                get
                {
                    return GetObject<TextBox>("BorrowerFullName");
                }
            }

            public TextBox BorrowerGenderTextBox
            {
                get
                {
                    return GetObject<TextBox>("BorrowerGender");
                }
            }

            public TextBox BorrowerDOBTextBox
            {
                get
                {
                    return GetObject<TextBox>("BorrowerDOB");
                }
            }

            public TextBox BorrowerSSNTextBox
            {
                get
                {
                    return GetObject<TextBox>("BorrowerSSN");
                }
            }



        }

        public class _EmploymentInformation : PageObjectBase
        {
            public _EmploymentInformation(NgWebDriver ngDriver)
               : base(ngDriver) { }

            public TextBoxEditable EncEmployerNameTextBox
            {
                get
                {
                    return GetObject<TextBoxEditable>("EncEmployerName", new object[] { "encEmployerNameEditButton", this });
                }
            }

            public TextAreaEditable EncEmployerAddressTextArea
            {
                get
                {
                    return GetObject<TextAreaEditable>("EncEmployerAddress", new object[] { "encEmployerAddressEditButton", this });
                }
            }

            public TextBoxEditable EncEmployerPhoneTextBox
            {
                get
                {
                    return GetObject<TextBoxEditable>("EncEmployerPhone", new object[] { "encEmployerPhoneEditButton", this });
                }
            }

            public TextBoxEditable EncEmployerFaxTextBox
            {
                get
                {
                    return GetObject<TextBoxEditable>("EncEmployerFax", new object[] { "encEmployerFaxEditButton", this });
                }
            }

            public DropDownBoxEditable EncEmploymentStatusSelect
            {
                get
                {
                    return GetObject<DropDownBoxEditable>("EncEmploymentStatus", new object[] { "employmentStatusEditButton", this });
                }
            }

            public TextBox EncEmploymentTitleTextBox
            {
                get
                {
                    return GetObject<TextBox>("EncEmploymentTitle");
                }
            }

            public DropDownBoxEditable Status1099Select
            {
                get
                {
                    return GetObject<DropDownBoxEditable>("Status1099", new object[] { "status1099EditButton", this });
                }
            }



        }

        public class _OrderInformation : PageObjectBase
        {
            public _OrderInformation(NgWebDriver ngDriver)
               : base(ngDriver) { }

            public TextBox VerificationSpecialistTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerificationSpecialist");
                }
            }

            public TextBox RequestTypeTextBox
            {
                get
                {
                    return GetObject<TextBox>("RequestType");
                }
            }

            public TextBox RequestedDateTextBox
            {
                get
                {
                    return GetObject<TextBox>("RequestedDate");
                }
            }

            public TextBox RequestedByTextBox
            {
                get
                {
                    return GetObject<TextBox>("RequestedBy");
                }
            }

            public DropDownBoxEditable OrderTypeSelect
            {
                get
                {
                    return GetObject<DropDownBoxEditable>("OrderType", new object[] { "orderTypeEditButton", this });
                }
            }

            public TextBox VerificationStatusTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerificationStatus");
                }
            }

            public TextBox VerificationLastAttemptDateTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerificationLastAttemptDate");
                }
            }

            public TextAreaEditable RequestNoteTextArea
            {
                get
                {
                    return GetObject<TextAreaEditable>("RequestNote", new object[] { "requestNoteEditButton", this });
                }
            }

            public TextBox EncLoanOfficerNameTextBox
            {
                get
                {
                    return GetObject<TextBox>("EncLoanOfficerName");
                }
            }

            public TextBox EncProcessorNameTextBox
            {
                get
                {
                    return GetObject<TextBox>("EncProcessorName");
                }
            }

            public TextBox DataCorrectionReasonTextBox
            {
                get
                {
                    return GetObject<TextBox>("DataCorrectionReason");
                }
            }

            public TextBox EquifaxEmployerCodeTextBox
            {
                get
                {
                    return GetObject<TextBox>("EquifaxEmployerCode");
                }
            }

            public List<DocGroupLink> DocumentLinkGroup
            {
                get
                {
                    return GetObjectGroup<DocGroupLink>("docLinkGroup", false);
                }

            }

            public class DocGroupLink : Link
            {
         
                public DocGroupLink(NgWebDriver ngWebDriver, IWebElement element, string Id)
                    : base(ngWebDriver, element, Id)
                {
 
                    
                }

                public int DocumentCount
                {
                    get
                    {
                        Regex countRegex = new Regex(@"(?<=\()\d+(?=\))");
                        
                        int _docCount;
                        string textContents = countRegex.Match(this.Text).Value;

                        if (textContents == "")
                        {
                            _docCount = 0;
                        }
                        else
                        {
                            _docCount = Int32.Parse(countRegex.Match(this.Text).Value);
                        }

                        return _docCount;

                    }
                }
            }

        }

        public class _OrderActivity : PageObjectBase
        {
            public _OrderActivity(NgWebDriver ngDriver)
               : base(ngDriver) { }

            public TextBox EmployerEmailTextBox
            {
                get
                {
                    return GetObject<TextBox>("EmployerEmail");
                }
            }

            public TextBox EmployerNameTextBox
            {
                get
                {
                    return GetObject<TextBox>("EmployerName");
                }
            }

            public TextBox EmployerPhoneTextBox
            {
                get
                {
                    return GetObject<TextBox>("EmployerPhone");
                }
            }

            public TextBox EndDateTextBox
            {
                get
                {
                    return GetObject<TextBox>("EndDate");
                }
            }

            public TextBox EmploymentJobTitleTextBox
            {
                get
                {
                    return GetObject<TextBox>("EmploymentJobTitle");
                }
            }

            public TextBox StartDateTextBox
            {
                get
                {
                    return GetObject<TextBox>("StartDate");
                }
            }

            public TextBox VerifiedViaTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerifiedVia");
                }
            }

            public TextBox VerifiedByTitleTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerifiedByTitle");
                }
            }

            public TextBox VerifiedByTextBox
            {
                get
                {
                    return GetObject<TextBox>("VerifiedBy");
                }
            }

            public DropDownBox EmploymentOutlookSelect
            {
                get
                {
                    return GetObject<DropDownBox>("EmploymentOutlook");
                }
            }

            public DropDownBox EmploymentStatusSelect
            {
                get
                {
                    return GetObject<DropDownBox>("EmploymentStatus");
                }
            }

            public DropDownBox VerificationStatusSelect
            {
                get
                {
                    return GetObject<DropDownBox>("CompoundOrderStatus");
                }
            }

            public TextBox FollowupDateTextBox
            {
                get
                {
                    return GetObject<TextBox>("FollowUpDate");
                }
            }

            //public NgGrid OrderActivityGrid
            //{
            //    get
            //    {
            //        return GetNgGrid<NgGrid>(base._ngWebDriver, "gridOptions");
            //    }
            //}

            public UiGrid OrderActivityGrid
            {
                get
                {
                    return GetUiGrid<UiGrid>(base._ngWebDriver, "gridOptions");
                }
            }

            public List<ReportLink> ReportLinkGroup
            {
                get
                {
                    return GetObjectGroup<ReportLink>("downloadForms");
                }
            }

            public class ReportLink : Link
            {

                string _formTag;

                public ReportLink(NgWebDriver ngWebDriver, IWebElement element, string Id)
                    : base(ngWebDriver, element, Id)
                {

                    string clickAttribute = element.GetAttribute("ng-click");
                    Regex regex = new Regex(@"(?<=createNewForm\(\')(.*)(?=\'\))");

                    _formTag = regex.Match(clickAttribute).Value;

                }

                public string FormTag
                {
                    get
                    {
                        return _formTag;
                    }
                }

            }

            public List<Link> EmailLinkGroup
            {
                get
                {
                    return GetObjectGroup<Link>("emailLinkGroup", false);
                }

            }

            public TextArea ActivityNoteTextArea
            {
                get
                {
                    return GetObject<TextArea>("activityNote");
                }

            }
        
            public Button SaveActivityButton
            {
                get
                {
                    return GetObject<Button>("saveActivity");
                }


            }
        
            
        }

        public class _EmailComposePopup : PageObjectBase
        {
            public _EmailComposePopup(NgWebDriver ngDriver)
                : base(ngDriver) {

                    emailAttachmentFieldMap.Add("att", "AttachmentButton");

            }

            Dictionary<string, string> emailAttachmentFieldMap = new Dictionary<string, string>() { };

            public Button AddressBookButton
            {
                get
                {
                    return GetObject<Button>("AddressBookButton");
                }
            }

            public List<ListItem> AddressBookListItems
            {
                get
                {
                    return GetObjectGroup<ListItem>("addRecipient");
                }

            }

            public DropDownBox TemplateSelect
            {

                get
                {
                     return GetObject<DropDownBox>("templateSelect");
                }
            }

            public Button ApplyTemplateButton
            {
                get
                {
                    return GetObject<Button>("applyTemplate");
                }
            }

            public TextArea EmailBodyTextArea
            {
                get
                {
                    return GetObject<TextArea>("emailBody");

                }

            }

            public TextBox ToRecipientTextBox
            {
                get
                {
                    return GetObject<TextBox>("toRecipients");

                }
            }

            public Button SendEmailButton
            {
                get
                {
                    return GetObject<Button>("sendEmail");
                }

            }

            public Button SaveDraftButton
            {
                get
                {
                    return GetObject<Button>("saveDraft");
                }

            }

            public Button ClosePopupButton
            {
                get
                {
                    return GetObject<Button>("closeEmailPopup");
                }

            }
            
            public TextBox SubjectTextBox
            {
                get
                {
                    return GetObject<TextBox>("emlSubject");
                }

            }

            public List<Attachment> AttachmentButtons
            {
                get
                {
                    //attachemnts
                    List<Attachment> attachments = GetRepeater<Attachment>("attachment in paramobj.emailObj.Attachments", emailAttachmentFieldMap, false);
                    //forms
                    List<Attachment> forms = GetRepeater<Attachment>("form in paramobj.emailObj.Forms", emailAttachmentFieldMap, false);
                    attachments.AddRange(forms);

                    return attachments;
                }
            }

            public class Attachment
            {
                public Button AttachmentButton { get; set; }
            }
        
        }

        public class _EmailHistoryPopup : PageObjectBase
        {
            public _EmailHistoryPopup(NgWebDriver ngDriver)
                : base(ngDriver) 
            {

                //cell map for email history
                emailHistoryCellMap.Add("emailSubject");
                emailHistoryCellMap.Add("emailFrom");
                emailHistoryCellMap.Add("emailRecipients");
                emailHistoryCellMap.Add("emailDateTime");

                emailAttachmentFieldMap.Add("att", "AttachmentButton");
             
            }

            List<string> emailHistoryCellMap = new List<string>() { };
            Dictionary<string, string> emailAttachmentFieldMap = new Dictionary<string, string>() { };

            public Table EmailHistoryTable
            {
                get
                {
                    return GetTable("modalDialog", "email in paramobj.emailHistory", emailHistoryCellMap);
                }
            }

            public Div SelectedEmailToRecipientsDiv
            {
                get
                {
                    return GetObject<Div>("toEmails");
                }
            }

            public Div SelectedEmailFromDiv
            {
                get
                {
                    return GetObject<Div>("fromEmail");
                }
            }

            public Div SelectedEmailSubjectDiv
            {
                get
                {
                    return GetObject<Div>("subject");
                }
            }

            public TextArea SelectedEmailBodyTextBox
            {
                get
                {
                    return GetObject<TextArea>("emailBody");
                }

            }

            public Button SaveEmailAsPDF
            {
                get
                {
                    return GetObject<Button>("saveAsPDF");

                }

            }

            public Button ReplyToEmailButton
            {
                get
                {
                    return GetObject<Button>("replyToEmail");
                }
            }

            public Button ReplyToEmailAttachmentsButton
            {
                get
                {
                    return GetObject<Button>("replyToEmailAttachments");
                }
            }

            public List<Attachment> AttachmentButtons
            {
                get
                {
                    return GetRepeater<Attachment>("attachment in paramobj.emailObj.Attachments", emailAttachmentFieldMap, false);
                }
            }

            public class Attachment {
                public Button AttachmentButton { get; set; }
            }
        }

        public class _LinkedDocsPopup : PageObjectBase
        {

            public _LinkedDocsPopup(NgWebDriver ngDriver)
                : base(ngDriver) 
            {
                //cell map for linked docs
                linkedDocCellMap.Add("docFileDisplayName");
                linkedDocCellMap.Add("docFileName");
                linkedDocCellMap.Add("docFileDateTime");
                linkedDocCellMap.Add("docFileType");
                linkedDocCellMap.Add("docIsAcceptable");
                linkedDocCellMap.Add("chkUploadQueue");
                linkedDocCellMap.Add("docDeleteAction");

                //cell map for enc docs
                encDocCellMap.Add("encdocFileDisplayName");
                encDocCellMap.Add("encdocFileName");
                encDocCellMap.Add("encdocFileDateTime");
       
            }

            List<string> linkedDocCellMap = new List<string>() { };
            List<string> encDocCellMap = new List<string>() { };

            string linkedDocRepeaterString = "doc in paramobj.linkedDocList";
            string encDocRepeaterString = "encdoc in paramobj.linkedDocList";
            
            public List<LinkedDocRow> LinkedDocTableRows
            {
                get
                {
                    return GetTableRows<LinkedDocRow>(LinkedDocs, linkedDocRepeaterString, linkedDocCellMap);
                }
            } 

            public Table LinkedDocs
            {
                get
                {
                    return GetTable("modalDialog", linkedDocRepeaterString, linkedDocCellMap, false);                   
                }
            }

            public Table EncDocs
            {
                get
                {
                    return GetTable("modalDialogEnc", encDocRepeaterString, encDocCellMap, false);

                }

            }

            public Button CloseButton
            {
                get
                {
                    return GetObject<Button>("closeButton");

                }

            }


        }

        public class _UploadDocsPopup : PageObjectBase
        {

            public _UploadDocsPopup(NgWebDriver ngDriver)
                : base(ngDriver)
            { }

            public void UploadFile(string FilePathName)
            {

                //click on dropbox area to create intpu type=file in dom
                GetObject<Div>("dropBox").Click();

                //then upload file by interacting with new input
                GetObjectGroup<File>(new ReadOnlyCollection<NgWebElement>(
                            new List<NgWebElement> { (NgWebElement)this.BodyElement }
                        ))[0].SendKeys(FilePathName);

                //close the system open popup
                CloseOpenDialog();
            

            }

            public class UploadedFile
            {
                public string FileName { get; set; }
                public int DocumentId { get; set; }
                public Div ProgressBar { get; set; }
            }

            public List<UploadedFile> UploadedFiles
            {
                get
                {

                    Dictionary<string, string> fieldMap = new Dictionary<string, string>() {};
                    fieldMap.Add("fileName", "FileName");
                    fieldMap.Add("documentId", "DocumentId");
                    fieldMap.Add("progressBar", "ProgressBar");

                    return GetRepeater<UploadedFile>("f in files", fieldMap, false);

                }

            }

            public Button CloseButton
            {
                get
                {
                    return GetObject<Button>("closeButton");
                }
            }
        }

        public class _BulkEditPopup : PageObjectBase
        {

            public _BulkEditPopup(NgWebDriver ngDriver)
                : base(ngDriver)
            {   }

            public DropDownBox BorrowerSelect
            {
                get
                {
                    return GetObject<DropDownBox>("borrowerSSN");
                }
            }

            public TextArea ActivityNote
            {
                get
                {
                    return GetObject<TextArea>("bulkActivityNote");
                }
            }

            public List<Checkbox> OrderStatuses
            {
                get
                {
                    return GetObjectGroup<Checkbox>("orderStatusList");
                }
            }

            public List<Checkbox> RequestTypes
            {
                get
                {
                    return GetObjectGroup<Checkbox>("requestTypeList");
                }
            }

            public Button ProcessChangesButton
            {
                get
                {
                    return GetObject<Button>("processChanges");
                }
            }

        }

        public Button BulkOrderActionsButton
        {
            get
            {
                return GetObject<Button>("bulkOrderActions");
            }

        }

        public Div PanicModeButton
        {
            get
            {
                return GetObject<Div>("panicModeButton");
            }

        }

        public Button SubcontractOrderButton
        {
            get
            {
                return GetObject<Button>("subcontractOrder", null, false);
            }

        }

        public class _SubcontractPopup : PageObjectBase
        {

            public _SubcontractPopup(NgWebDriver ngDriver)
                : base(ngDriver)
            { }

            public Button OrderSubmitButton
            {
                get
                {
                    return GetObject<Button>("subcontractSubmit");
                }
            }



        }

        public class _RelatedOrdersInformation : PageObjectBase
        {
            public _RelatedOrdersInformation(NgWebDriver ngDriver)
                : base(ngDriver) { }

            public UiGrid OrderRelatedLoansGrid
            {
                get
                {
                    return GetUiGrid<UiGrid>(base._ngWebDriver, "gridRelOrderLoanOptions");
                }
            }

            public UiGrid OrderRelatedSSNGrid
            {
                get
                {
                    return GetUiGrid<UiGrid>(base._ngWebDriver, "gridRelOrderSSNOptions");
                }
            }


        }

        public class LinkedDocRow : Table.TableRow
        {
            public LinkedDocRow(NgWebDriver ngWebDriver, IWebElement element, string Id, object[] optionalArgs = null)
                : base(ngWebDriver, element, Id) {

                    _page = (PageObjectBase)optionalArgs[0];
            
            }

            PageObjectBase _page;

            public string FileName
            {
                get
                {
                    return this.GetCellValue("docFileName");
                }
            }

            public Checkbox UploadQueueCheckbox
            {
                get
                {
                    return _page.GetObjectGroup<Checkbox>(new ReadOnlyCollection<NgWebElement>(
                                new List<NgWebElement> { this.GetCell("chkUploadQueue") }
                            ))[0];
                }
            }

            public Link DeleteButton
            {
                get
                {
                    return _page.GetObjectGroup<Link>(new ReadOnlyCollection<NgWebElement>(
                        new List<NgWebElement> { this.GetCell("docDeleteAction") }
                    ))[0];
                }

            }

            
        }
       
        public class _VendorEditPopup : PageObjectBase
        {

            public _VendorEditPopup(NgWebDriver ngDriver)
                : base(ngDriver) { }

            public Link EditVendorButton
            {
                get
                {
                    return GetObject<Link>("editVendor");
                }

            }

            public Link RemoveVendorButton
            {
                get
                {
                    return GetObject<Link>("removeVendor");
                }

            }

            public DropDownBox VendorSelect
            {

                get
                {
                    return GetObject<DropDownBox>("currentVendor");
                }

            }

            public TextBox VendorCostTextBox
            {
                get
                {
                    return GetObject<TextBox>("VendorCost");
                }
            }

            public TextBox VerifiedByPhoneTextBox
            {
                get
                {
                    return GetObject<TextBox>("verifiedByPhone");
                }
            }

            public TextBox VendorReferenceNumTextBox
            {
                get
                {
                    return GetObject<TextBox>("vendorReferenceNum");
                }
            }

            public TextBox VendorWebsiteTextBox
            {
                get
                {
                    return GetObject<TextBox>("vendorWebsite");
                }
            }

            public TextBox VendorDataDate
            {
                get
                {
                    return GetObject<TextBox>("vendorDataDate");
                }
            }

            public Button VendorSaveButton
            {
                get
                {
                    return GetObject<Button>("vendorSave");
                }
            }

        }
        
        public enum DownloadFileType
        {
            [Description("createNewForm('CPAComfortFaxCover')")]
            CPAComfortFaxCover,
            [Description("exportOrderActivity()")]
            OrderActivtyPDF
        }

        public string GenerateFile(DownloadFileType type)
        {

            string scriptContents = "var scpe = angular.element(arguments[0]).scope(); ";
            scriptContents += "scpe.$apply(function(){ return scpe." + type.GetDescription() + "; }); ";
            this.ExecAngularScript(scriptContents);

            System.Threading.Thread.Sleep(1000);

            scriptContents = "var scpe = angular.element(arguments[0]).scope(); ";
            scriptContents += "return scpe.$apply(function(){ return scpe.getFileURL(); }); ";

            //return URL
            return this.ExecAngularScript(scriptContents);

        }

        public void ValidifyOrderActivityField(string ErrorFieldName)
        {
            PropertyInfo fieldObject = null;

            //first, find the field we are talking about
            fieldObject = typeof(OrderDetail._OrderActivity).GetProperty(ErrorFieldName);

            //second, determine what kind of field it is
            string typeName = fieldObject.PropertyType.Name;

            if (typeName == "DropDownBox")
            {
                DropDownBox ddObject = (DropDownBox)fieldObject.GetValue(this.OrderActivity);
                //click first option that is not the placeholder
                ddObject.Options.Where<DropDownBox.DropDownBoxOption>(q => !q.Text.Contains("...")).FirstOrDefault().Click();
            }
            else if (typeName == "TextBox")
            {
                TextBox tbObject = (TextBox)fieldObject.GetValue(this.OrderActivity);

                if (ErrorFieldName.ToLower().Contains("date"))
                {
                    //if it's a date, fill in a date
                    tbObject.SendKeys(DateTime.Now.ToString("MM/dd/yyyy"));
                }
                else
                {
                    //if it's text, fill in something
                    tbObject.SendKeys("test text for " + ErrorFieldName);
                }
            }
            else
            {
                throw new ModelExceptions.OrderDetailModelExceptions.UnsupportedValidifyFieldTypeException(fieldObject.ToString(), ErrorFieldName);
            }



        }
        
        public class FieldEditLink : Link 
        {

            public enum ClickActions {
                StartEdit,
                SaveEdit,
                CancelEdit
            }

            public string FieldName { get;set; }
            private string clickAttribute { get; set; }

            public FieldEditLink(NgWebDriver ngWebDriver, IWebElement element, string Id)
                : base(ngWebDriver, element, Id) 
            { 
            
                clickAttribute = element.GetAttribute("ng-click");
                FieldName = clickAttribute.Split("'"[0])[1];

            }

            public ClickActions ClickAction
            {

                get
                {
                    ClickActions retVal;

                    string currClickAction = clickAttribute.Split("("[0])[0];

                    if (currClickAction == "startFieldEdit")
                    {
                        retVal = FieldEditLink.ClickActions.StartEdit;
                    }
                    else if (currClickAction == "saveFieldEdit")
                    {
                        retVal = FieldEditLink.ClickActions.SaveEdit;
                    }
                    else if (currClickAction == "cancelFieldEdit")
                    {
                        retVal = FieldEditLink.ClickActions.CancelEdit;
                    }
                    else
                    {
                        throw new ModelExceptions.OrderDetailModelExceptions.UnknownFieldEditLinkClickActionException(FieldName, currClickAction);
                    }

                    return retVal;
                }


            }

        }

        public class TextBoxEditable : TextBox, IEditableField
        {

            PageObjectBase _page;
            string _groupName;

            public TextBoxEditable(NgWebDriver ngWebDriver, IWebElement element, string Id, object[] optParams)
                : base(ngWebDriver, element, Id)
            {

                _groupName = optParams[0].ToString();  //param 0: groupname
                _page = (PageObjectBase)optParams[1];  //param 1: page object reference

            }

            public List<FieldEditLink> GetButtons()
            {
                return _page.GetObjectGroup<FieldEditLink>(_groupName, false);
            }


        }
        public class TextAreaEditable : TextArea, IEditableField
        {

            PageObjectBase _page;
            string _groupName;

            public TextAreaEditable(NgWebDriver ngWebDriver, IWebElement element, string Id, object[] optParams)
                : base(ngWebDriver, element, Id)
            {

                _groupName = optParams[0].ToString();  //param 0: groupname
                _page = (PageObjectBase)optParams[1];  //param 1: page object reference

            }

            public List<FieldEditLink> GetButtons()
            {
                return _page.GetObjectGroup<FieldEditLink>(_groupName, false);
            }


        }

        public class DropDownBoxEditable : DropDownBox, IEditableField
        {

            PageObjectBase _page;
            string _groupName;

            public DropDownBoxEditable(NgWebDriver ngWebDriver, IWebElement element, string Id, object[] optParams)
                : base(ngWebDriver, element, Id)
            {

                _groupName = optParams[0].ToString();  //param 0: groupname
                _page = (PageObjectBase)optParams[1];  //param 1: page object reference

            }

            public List<FieldEditLink> GetButtons()
            {
                return _page.GetObjectGroup<FieldEditLink>(_groupName, false);
            }


        }

        public bool CanSaveOrderActivity(out string errorFieldName)
        {

            bool retVal = false;
            errorFieldName = null;

            //exec the validation JS
            //$scope.canSaveActivity(isReApproval, isVendorEvent, vendorRemovalReasonId, notifyDeferral, suppressAlert)
            string scriptContents = "var scpe = angular.element(arguments[0]).scope(); ";
            scriptContents += "return scpe.$apply(function(){ ";
            scriptContents += "return scpe.canSaveActivity(false, false, null, null, true); ";
            scriptContents += "}); ";

            string scriptResult = this.ExecAngularScript(scriptContents);

            if (scriptResult == "ok")
            {
                retVal = true;
            }
            else
            {
                errorFieldName = scriptResult;
            }


            return retVal;

        }
        
        public interface IEditableField {
            List<FieldEditLink> GetButtons();
        }


        

    }

    public class OrderDetailModelException : ModelException { }

    public partial class ModelExceptions
    {
        public static class OrderDetailModelExceptions
        {

            public class UnsupportedValidifyFieldTypeException : OrderDetailModelException
            {

                public string FieldType;
                public string FieldName;

                public override string Message
                {
                    get
                    {
                        return "Field Type " + FieldType + " is not supported for field validify on field " + FieldName + ".  Add support.";
                    }
                }

                public UnsupportedValidifyFieldTypeException(string fieldType, string fieldName)
                    : base()
                {
                    FieldType = fieldType;
                    FieldName = fieldName;
                }

            }

            public class UnknownFieldEditLinkClickActionException : OrderDetailModelException
            {
                string FieldName { get; set; }
                string ClickActionName { get; set; }

                public override string Message
                {
                    get
                    {
                        return "Field Edit Link '" + FieldName + "' has unknown click action: " + ClickActionName;
                    }
                }

                public UnknownFieldEditLinkClickActionException(string fieldName, string clickActionName)
                    : base()
                {
                    FieldName = fieldName;
                    ClickActionName = clickActionName;
                }

            }


        }

    }

}
