using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Protractor;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;
using VOESystem.UnitTests.Business;
using VOESystem.UnitTests.UI.Model;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.Tests.IntegrationTests
{


    [TestFixture]
    public class OrderDetail : UITestBase
    {

        [Test]
        public void Test_Intgr_OrderDetail_Order_DataVerification()
        {

            //get a pending initial order
            OrderOps oOp = new OrderOps();
            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            //get the order data from the db
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);
            
            //check borrower fields
            Assert.AreEqual(isNull(orderDetail.BorrowerFullName, ""), orderDetailPage.BorrowerInformation.BorrowerFullNameTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.BorrowerGender, ""), orderDetailPage.BorrowerInformation.BorrowerGenderTextBox.Text);
            Assert.AreEqual((orderDetail.BorrowerDOB ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"), isEmpty(orderDetailPage.BorrowerInformation.BorrowerDOBTextBox.Text, "01/01/1900"));
            Assert.AreEqual(isNull(orderDetail.BorrowerSSN, ""), orderDetailPage.BorrowerInformation.BorrowerSSNTextBox.Text);

            //check employer fields
            Assert.AreEqual(isNull(orderDetail.EncEmployerName, ""), orderDetailPage.EmploymentInformation.EncEmployerNameTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.EncEmployerAddress, "").CleanCharsForCompare(), orderDetailPage.EmploymentInformation.EncEmployerAddressTextArea.Text.CleanCharsForCompare());
            Assert.AreEqual(isNull(orderDetail.EncEmployerPhone, ""), orderDetailPage.EmploymentInformation.EncEmployerPhoneTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.EncEmploymentStatus, ""), orderDetailPage.EmploymentInformation.EncEmploymentStatusSelect.Text);
            Assert.AreEqual(isNull(orderDetail.EncEmploymentTitle, ""), orderDetailPage.EmploymentInformation.EncEmploymentTitleTextBox.Text);

            //check order fields
            Assert.AreEqual(isNull(orderDetail.VerificationSpecialist, ""), orderDetailPage.OrderInformation.VerificationSpecialistTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.RequestType, ""), orderDetailPage.OrderInformation.RequestTypeTextBox.Text);
            Assert.AreEqual(orderDetail.RequestedDate.ToString("MM/dd/yyyy"), orderDetailPage.OrderInformation.RequestedDateTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.RequestedBy, ""), orderDetailPage.OrderInformation.RequestedByTextBox.Text);
            Assert.AreEqual(orderDetail.OrderTypeId.ToString(), orderDetailPage.OrderInformation.OrderTypeSelect.Text);
            Assert.AreEqual(isNull(orderDetail.VerificationStatus, ""), orderDetailPage.OrderInformation.VerificationStatusTextBox.Text);
            Assert.AreEqual(orderDetail.VerificationLastAttemptDate.ToString("MM/dd/yyyy"), orderDetailPage.OrderInformation.VerificationLastAttemptDateTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.RequestNote.CleanCharsForCompare(), ""), orderDetailPage.OrderInformation.RequestNoteTextArea.Text.CleanCharsForCompare());
            Assert.AreEqual(isNull(orderDetail.EncLoanOfficerName, ""), orderDetailPage.OrderInformation.EncLoanOfficerNameTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.EncProcessorName, ""), orderDetailPage.OrderInformation.EncProcessorNameTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.DataCorrectionReason, "N/A"), orderDetailPage.OrderInformation.DataCorrectionReasonTextBox.Text);
            Assert.AreEqual(isNull(orderDetail.EquifaxEmployerCode, "N/A"), orderDetailPage.OrderInformation.EquifaxEmployerCodeTextBox.Text);

            //check most recent orderactivity record
            Data.DBSchema.OrderActivityView orderActivity = oOp.getLastOrderActivity(OrderNumber);
           
            //check activity
            Assert.AreEqual(orderActivity.OrderStatusId.ToString(), orderDetailPage.OrderActivity.VerificationStatusSelect.Text.Split("|"[0])[0]);
            Assert.AreEqual(isNull(orderActivity.EmploymentStatus, ""), orderDetailPage.OrderActivity.EmploymentStatusSelect.SelectedText);
            Assert.AreEqual(isNull(orderActivity.EmploymentOutlook, ""), orderDetailPage.OrderActivity.EmploymentOutlookSelect.SelectedText);
            Assert.AreEqual(isNull(orderActivity.EmployerEmail, ""), orderDetailPage.OrderActivity.EmployerEmailTextBox.Text);
            Assert.AreEqual(isNull(orderActivity.EmployerName, ""), orderDetailPage.OrderActivity.EmployerNameTextBox.Text);
            Assert.AreEqual(isNull(orderActivity.EmployerPhone, ""), orderDetailPage.OrderActivity.EmployerPhoneTextBox.Text);
            Assert.AreEqual((orderActivity.EmploymentEndDate ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"), isEmpty(orderDetailPage.OrderActivity.EndDateTextBox.Text, "01/01/1900"));
            Assert.AreEqual(isNull(orderActivity.EmploymentJobTitle, ""), orderDetailPage.OrderActivity.EmploymentJobTitleTextBox.Text);
            Assert.AreEqual((orderActivity.EmploymentStartDate ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"), isEmpty(orderDetailPage.OrderActivity.StartDateTextBox.Text, "01/01/1900"));
            Assert.AreEqual(isNull(orderActivity.VerifiedVia, ""), orderDetailPage.OrderActivity.VerifiedViaTextBox.Text);
            Assert.AreEqual(isNull(orderActivity.VerifiedByTitle, ""), orderDetailPage.OrderActivity.VerifiedByTitleTextBox.Text);
            Assert.AreEqual(isNull(orderActivity.VerifiedBy, ""), orderDetailPage.OrderActivity.VerifiedByTextBox.Text);
            Assert.AreEqual((orderActivity.FollowupDate ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"), isEmpty(orderDetailPage.OrderActivity.FollowupDateTextBox.Text, "01/01/1900"));
            
            //check orderactivity grid has records.  not sure what other checks i can do here
            Assert.NotZero(orderDetailPage.OrderActivity.OrderActivityGrid.RowCount);
            
            //check related orders by loan number
            Assert.NotZero(orderDetailPage.RelatedOrdersInformation.OrderRelatedLoansGrid.RowCount);

            //check related orders by ssn
            Assert.NotZero(orderDetailPage.RelatedOrdersInformation.OrderRelatedSSNGrid.RowCount);

            //create a pdf form
            string fileURL = orderDetailPage.GenerateFile(UI.Model.OrderDetail.DownloadFileType.CPAComfortFaxCover);
            string filePathname = convertFileURLtoFilePathName(fileURL);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathname));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathname);
            Assert.That(fi.Length > 0);


        }

        [Test]
        public void Test_Intgr_OrderDetail_Order_FieldModification()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //edit the order type
            string newOrderType = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.DropDownBoxEditable>(orderDetailPage.OrderInformation.OrderTypeSelect, orderDetailPage);

            //edit the requestnote
            string newRequestNote = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.TextAreaEditable>(orderDetailPage.OrderInformation.RequestNoteTextArea, orderDetailPage);

            //edit the employment status
            string newEmploymentStatus = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.DropDownBoxEditable>(orderDetailPage.EmploymentInformation.EncEmploymentStatusSelect, orderDetailPage);

            //edit the 1099 status
            string new1099Status = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.DropDownBoxEditable>(orderDetailPage.EmploymentInformation.Status1099Select, orderDetailPage);

            //edit the employer name
            string newEncEmployerName = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.TextBoxEditable>(orderDetailPage.EmploymentInformation.EncEmployerNameTextBox, orderDetailPage);

            //edit the employer address
            string newEncEmployerAddress = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.TextAreaEditable>(orderDetailPage.EmploymentInformation.EncEmployerAddressTextArea, orderDetailPage);

            //edit the employer phone
            string newEncEmployerPhone = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.TextBoxEditable>(orderDetailPage.EmploymentInformation.EncEmployerPhoneTextBox, orderDetailPage);

            //edit the employer fax
            string newEncEmployerFax = OrderDetail_ModifyOrderField<UI.Model.OrderDetail.TextBoxEditable>(orderDetailPage.EmploymentInformation.EncEmployerFaxTextBox, orderDetailPage);

            //and check the db that the new values are there
            orderDetail = oOp.getOrderDetail(OrderNumber);
            Assert.AreEqual(newOrderType.CleanCharsForCompare(), orderDetail.OrderType.CleanCharsForCompare());
            Assert.AreEqual(newRequestNote.CleanCharsForCompare(), orderDetail.RequestNote.CleanCharsForCompare());
            Assert.AreEqual(newEmploymentStatus.CleanCharsForCompare(), orderDetail.EncEmploymentStatus.CleanCharsForCompare());
            Assert.AreEqual(new1099Status.CleanCharsForCompare(), orderDetail.Status1099.CleanCharsForCompare());
            Assert.AreEqual(newEncEmployerName.CleanCharsForCompare(), orderDetail.EncEmployerName.CleanCharsForCompare());
            Assert.AreEqual(newEncEmployerAddress.CleanCharsForCompare(), orderDetail.EncEmployerAddress.CleanCharsForCompare());
            Assert.AreEqual(newEncEmployerPhone.CleanCharsForCompare(), orderDetail.EncEmployerPhone.CleanCharsForCompare());
            Assert.AreEqual(newEncEmployerFax.CleanCharsForCompare(), orderDetail.EncEmployerFax.CleanCharsForCompare());

         
        }
        
        private string OrderDetail_ModifyOrderField<T>(ElementObjectBase EditingField, PageObjectBase page)
            where T : ElementObjectBase, UI.Model.OrderDetail.IEditableField
        {

            string retVal = String.Empty;

            //check that button is in normal, readonly mode, if so click on it
            Assert.That(EditingField.CanEdit == false);
            Assert.That(((T)EditingField).GetButtons().Count == 1);
            UI.Model.OrderDetail.FieldEditLink editButton = ((T)EditingField).GetButtons()[0];
            Assert.That(editButton.ClickAction == UI.Model.OrderDetail.FieldEditLink.ClickActions.StartEdit);
            editButton.Click();

            //check that button is in edit mode.  if so, edit it
            Assert.That(EditingField.CanEdit == true);
            Assert.That(((T)EditingField).GetButtons().Count == 2);

            string currentFieldValue = String.Empty;
            string newFieldValue = String.Empty;

            if (typeof(T) == typeof(UI.Model.OrderDetail.DropDownBoxEditable))
            {
                //this is if we are testing a drop-down box field
                currentFieldValue = ((UI.Model.OrderDetail.DropDownBoxEditable)EditingField).SelectedText;
                ((UI.Model.OrderDetail.DropDownBoxEditable)EditingField).Options.Where<UI.Model.OrderDetail.DropDownBoxEditable.DropDownBoxOption>(q => q.Text != currentFieldValue)
                    .FirstOrDefault().Click();
                newFieldValue = ((UI.Model.OrderDetail.DropDownBoxEditable)EditingField).SelectedText;
            }
            else if (typeof(T) == typeof(UI.Model.OrderDetail.TextBoxEditable) || typeof(T) == typeof(UI.Model.OrderDetail.TextAreaEditable))
            {
                //text boxes
                currentFieldValue = EditingField.Text;
                EditingField.SendKeys(" EDIT TEST");
                newFieldValue = EditingField.Text;

            }
            else
            {
                throw new OrderDetailExceptions.UnsupportedEditingFieldTypeException(typeof(T).Name);

            }

            Assert.AreNotEqual(currentFieldValue, newFieldValue);

            //save the new value
            ((T)EditingField).GetButtons().Where<UI.Model.OrderDetail.FieldEditLink>
                (q => q.ClickAction == UI.Model.OrderDetail.FieldEditLink.ClickActions.SaveEdit).FirstOrDefault().Click();
            Assert.That(page.WaitForAndDismissAlert());

            //check the state of the field again
            Assert.That(EditingField.CanEdit == false);
            Assert.That(((T)EditingField).GetButtons().Count == 1);

            return newFieldValue;
        }

        [Test]
        public void Test_Intgr_OrderDetail_Vendor_NewEntry()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //find a vendor that triggers an auto-notify to accounting
            BaseDataOps bOp = new BaseDataOps();
            Data.DBSchema.Vendor vendor = bOp.getVendorList().VendorList.Where<Data.DBSchema.Vendor>(q => q.AutoNotifyAccounting == true
                && q.IsActive == true).OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            string newVendorName = vendor.Name;
            int newVendorId = vendor.Id;

            //open vendor popup
            orderDetailPage.VendorEditPopup.EditVendorButton.Click();

            //select a vendor
            orderDetailPage.VendorEditPopup.VendorSelect.Options.Where<DropDownBox.DropDownBoxOption>
                (q => q.Text == newVendorName).FirstOrDefault().Click();
            
            //add a report cost, ref unm and data date
            string newVendorCost = "5.25";
            string newVendorRefNum = "REF123456789";
            string newVendorDataDate = DateTime.Now.ToString("MM/dd/yyyy");

            orderDetailPage.VendorEditPopup.VendorDataDate.SendKeys(newVendorDataDate, true);
            orderDetailPage.VendorEditPopup.VendorCostTextBox.SendKeys(newVendorCost, true);
            orderDetailPage.VendorEditPopup.VendorReferenceNumTextBox.SendKeys(newVendorRefNum, true);

            //bring the button to the top of the stacking order so we can click on it.
            string scriptContents = "var scpe = angular.element(arguments[0]).scope(); ";
            scriptContents += "scpe.$apply(function(){ document.getElementById('vendorSave').style.zIndex = '9999'; }); ";
            orderDetailPage.ExecAngularScript(scriptContents);

            //click save
            orderDetailPage.VendorEditPopup.VendorSaveButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check that the values were saved
            Data.DBSchema.OrderActivityView orderActivity = oOp.getLastOrderActivity(OrderNumber);

            Assert.AreEqual(newVendorId, orderActivity.VendorId);
            Assert.AreEqual(newVendorCost, orderActivity.VendorCost.ToString());
            Assert.AreEqual(newVendorRefNum.CleanCharsForCompare(), orderActivity.VendorReferenceNum.CleanCharsForCompare());
            Assert.AreEqual(newVendorDataDate, (orderActivity.VendorDataDate ?? DateTime.Parse("01/01/1900")).ToString("MM/dd/yyyy"));

            //check that an email went to accounting
            List<Data.DTO.Email> emails = oOp.getOrderEmails(orderDetail.OrderRequestId, "Accounting Verification Confirmation");
            Assert.That(emails.Count, Is.GreaterThanOrEqualTo(1));
            
            //check that the order activity note contains the proper info
            Assert.That(orderActivity.ActivityNote.ToLower().Contains("vendor report event"));
            Assert.That(orderActivity.ActivityNote.Contains(newVendorName));
            Assert.That(orderActivity.ActivityNote.Contains(newVendorRefNum));
            Assert.That(orderActivity.ActivityNote.Contains(newVendorCost));

        }

        [Test]
        public void Test_Intgr_OrderDetail_Email_Send()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //click on open email compose link
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text == "Compose New Email").FirstOrDefault().Click();
            
            //hover over button
            orderDetailPage.EmailComposePopup.AddressBookButton.HoverOver(ngDriver);
            Assert.That(orderDetailPage.EmailComposePopup.AddressBookListItems[0].Displayed);

            //see if address book items are there
            Assert.That(orderDetailPage.EmailComposePopup.AddressBookListItems.Count > 1);

            //open template drop down and check elements
            Assert.That(orderDetailPage.EmailComposePopup.TemplateSelect.Options.Count > 1);

            //select a template and apply it
            DropDownBox.DropDownBoxOption templateOption = orderDetailPage.EmailComposePopup.TemplateSelect.Options.Where<DropDownBox.DropDownBoxOption>
                (q => q.Text.ToLower().Contains("mybiz")).FirstOrDefault();
            string emailTemplateName = templateOption.Text;
            templateOption.Click();

            orderDetailPage.EmailComposePopup.ApplyTemplateButton.Click();
            ngDriver.WaitForAngular();  //this takes a bit sometimes

            //check to make sure there are no field tags still in the email body
            Assert.That(!orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text.Contains("#"));

            //check to make sure there is at least one recipient
            Assert.That(orderDetailPage.EmailComposePopup.ToRecipientTextBox.Text.Length > 1);

            //send email
            orderDetailPage.EmailComposePopup.SendEmailButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check the database to find the email by template and ensure it is recent
            Data.DTO.Email email = oOp.getOrderEmails(orderDetail.OrderRequestId, emailTemplateName)
                .OrderByDescending(r => r.EmailDateTime).FirstOrDefault();
            Assert.That(email.EmailDateTime >= DateTime.Now.AddMinutes(-3)); 



        }

        [Test]
        public void Test_Intgr_OrderDetail_Email_DraftSave()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //click on open email compose link
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text == "Compose New Email").FirstOrDefault().Click();

            //select a template and apply it
            DropDownBox.DropDownBoxOption templateOption = orderDetailPage.EmailComposePopup.TemplateSelect.Options.Where<DropDownBox.DropDownBoxOption>
                (q => q.Text.ToLower().Contains("mybiz")).FirstOrDefault();
            string emailTemplateName = templateOption.Text;
            templateOption.Click();

            orderDetailPage.EmailComposePopup.ApplyTemplateButton.Click();
            ngDriver.WaitForAngular();  //this takes a bit sometimes

            //check to make sure there is at least one recipient
            Assert.That(orderDetailPage.EmailComposePopup.ToRecipientTextBox.Text.Length > 1);

            //save the draft
            orderDetailPage.EmailComposePopup.SaveDraftButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check the database to find the email draft 
            Data.DTO.Email email = oOp.getOrderEmailDraft(orderDetail.OrderRequestId);
            Assert.NotNull(email);

            //close the popup
            orderDetailPage.EmailComposePopup.ClosePopupButton.Click();

            //ensure the draft link is visible
            Assert.That(orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("draft")).ToList().Count == 1);

            //nav to another page
            ngDriver.WrappedDriver.Url = baseUrl + "orderdetails/order/" + (orderDetail.OrderRequestId - 1).ToString();
            
            //nav back
            ngDriver.WrappedDriver.Url = baseUrl + "orderdetails/order/" + orderDetail.OrderRequestId.ToString();

            //check link is still there
            Assert.That(orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("draft")).ToList().Count == 1);

            //click it
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("draft")).FirstOrDefault().Click();

            //send the email
            orderDetailPage.EmailComposePopup.SendEmailButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check the database to find the email by template and ensure it is recent
            email = oOp.getOrderEmails(orderDetail.OrderRequestId, emailTemplateName)
                .OrderByDescending(r => r.EmailDateTime).FirstOrDefault();
            Assert.That(email.EmailDateTime >= DateTime.Now.AddMinutes(-3)); 

            //check that there are no more draft records for this order
            email = oOp.getOrderEmailDraft(orderDetail.OrderRequestId);
            Assert.IsNull(email);



        }
        
        [Test]
        public void Test_Intgr_OrderDetail_Email_History()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //Click on the “Email History” link.
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("history")).FirstOrDefault().Click();

            //Ensure there are items in the grid
            int iHistoryGridCount = orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Count;
            Assert.That(iHistoryGridCount > 0);

            //Select and view an email in the list - would rather select something other than the first row since that loads by default
            int selectRow = 0;
            if (iHistoryGridCount >= 2)
            {
                selectRow = 1;
            }
            orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Where<Table.TableRow>(q => q.Id == selectRow.ToString()).FirstOrDefault().Click();

            //Ensure there are recipients and an email body
            Assert.That(orderDetailPage.EmailHistoryPopup.SelectedEmailToRecipientsDiv.Text != "");
            Assert.That(orderDetailPage.EmailHistoryPopup.SelectedEmailFromDiv.Text != "");
            Assert.That(orderDetailPage.EmailHistoryPopup.SelectedEmailSubjectDiv.Text != "");
            Assert.That(orderDetailPage.EmailHistoryPopup.SelectedEmailBodyTextBox.Text != "");


        }
        
        [Test]
        public void Test_Intgr_OrderDetail_Email_HistorySaveAsPDF()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //Click on the “Email History” link.
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("history")).FirstOrDefault().Click();

            //Ensure there are items in the grid
            int iHistoryGridCount = orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Count;
            Assert.That(iHistoryGridCount > 0);

            //Select and view an email in the list - would rather select something other than the first row since that loads by default
            int selectRow = 0;
            if (iHistoryGridCount >= 2)
            {
                selectRow = 1;
            }
            orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Where<Table.TableRow>(q => q.Id == selectRow.ToString()).FirstOrDefault().Click();

            //click on save as pdf
            orderDetailPage.EmailHistoryPopup.SaveEmailAsPDF.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //Click on the “Linked Docs” link.
            orderDetailPage.OrderInformation.DocumentLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("linked")).FirstOrDefault().Click();
            
            //make sure it appears in linked docs
            Table.TableRow docRow = orderDetailPage.LinkedDocsPopup.LinkedDocs.TableRows.OrderByDescending(q => 
                DateTime.Parse(q.TableCells.Where<Table.TableRow.TableCell>(r => r.Id == "docFileDateTime").FirstOrDefault().Text)
                ).FirstOrDefault();

            //check that the datetime is recent
            Assert.That(DateTime.Parse(docRow.GetCellValue("docFileDateTime")) >= DateTime.Now.AddMinutes(-3)); 

            //check that the display name begins with "PrintedEmail"
            Assert.That(docRow.GetCellValue("docFileDisplayName").StartsWith("PrintedEmail"));
            
            //get the file URL
            string fileURL = orderDetailPage.GetObjectGroup<Link>(
                new ReadOnlyCollection<NgWebElement>(
                    new List<NgWebElement> { docRow.GetCell("docFileDisplayName") }
                ))[0].URL;
                
            //make sure file is in file system
            string filePathname = convertFileURLtoFilePathName(fileURL);

            ////check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathname));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathname);
            Assert.That(fi.Length > 0);


        }

        [Test]
        public void Test_Intgr_OrderDetail_Email_EmailReply()
        {

            OrderOps oOp = new OrderOps();
            int OrderRequestId = oOp.getOrderWithEmailAttachment();

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            LogOrderNumber(orderDetail.OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //Click on the “Email History” link.
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("history")).FirstOrDefault().Click();

            //Ensure there are items in the grid
            int iHistoryGridCount = orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Count;
            Assert.That(iHistoryGridCount > 0);

            //Select and view an email in the list
            int selectRow = 0;
            while (orderDetailPage.EmailHistoryPopup.AttachmentButtons.Count == 0)
            {

                if (selectRow == iHistoryGridCount)
                {
                    throw new OrderDetailExceptions.EmailWithAttachmentNotFoundException(orderDetail.OrderNumber);
                }

                orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Where<Table.TableRow>(
                    q => q.Id == selectRow.ToString()).FirstOrDefault().Click();

                selectRow++;
            }

            string emailSubject = orderDetailPage.EmailHistoryPopup.SelectedEmailSubjectDiv.Text;
            string emailToList = orderDetailPage.EmailHistoryPopup.SelectedEmailToRecipientsDiv.Text;
            string emailBody = orderDetailPage.EmailHistoryPopup.SelectedEmailBodyTextBox.Text;

            //click on reply
            orderDetailPage.EmailHistoryPopup.ReplyToEmailButton.Click();

            //Ensure there are recipients and an email body
            Assert.That(orderDetailPage.EmailComposePopup.SubjectTextBox.Text.Contains(emailSubject));
            Assert.That(orderDetailPage.EmailComposePopup.ToRecipientTextBox.Text.Contains(emailToList));
            Assert.That(orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text.Contains(emailBody));

            //ensure that no attachments came over
            Assert.Zero(orderDetailPage.EmailComposePopup.AttachmentButtons.Count);

            //Check the email reply header
            Assert.That(orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text.Contains("Original Message"));
            Regex regex = new Regex(@"(?<=(-+)Original\sMessage(-+)\r\n)(.+?)(?=(\r\n){2})", RegexOptions.Singleline);
            string emailReplyHeader = regex.Match(orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text).Value;
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains(
                orderDetailPage.EmailComposePopup.SubjectTextBox.Text.CleanCharsForCompare()));
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains(
                orderDetailPage.EmailComposePopup.ToRecipientTextBox.Text.CleanCharsForCompare()));
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains("voe@firsthome.com"));
                
            //Check the email signature 
            regex = new Regex(@"(?:(?!(-+)Original\sMessage(-+)\r\n).)*(?=(-+)Original\sMessage(-+)\r\n)?", RegexOptions.Singleline);
            string emailSignature = regex.Match(orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text).Value;
            Assert.That(emailSignature.CleanCharsForCompare().Contains(UserFullName.CleanCharsForCompare()));
            Assert.That(emailSignature.CleanCharsForCompare().Contains("voe@firsthome.com".CleanCharsForCompare()));
            Assert.That(!emailSignature.Contains("#"));

            //send the email
            orderDetailPage.EmailComposePopup.SendEmailButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check the database to find the email by template and ensure it is recent
            Data.DTO.Email email = oOp.getOrderEmails(orderDetail.OrderRequestId, null)
                .OrderByDescending(r => r.EmailDateTime).FirstOrDefault();
            Assert.That(email.EmailDateTime >= DateTime.Now.AddMinutes(-3)); 

            


        }

        [Test]
        public void Test_Intgr_OrderDetail_Email_EmailReplyWithAttachment()
        {

            OrderOps oOp = new OrderOps();
            int OrderRequestId = oOp.getOrderWithEmailAttachment();

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            LogOrderNumber(orderDetail.OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //Click on the “Email History” link.
            orderDetailPage.OrderActivity.EmailLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("history")).FirstOrDefault().Click();

            //Ensure there are items in the grid
            int iHistoryGridCount = orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Count;
            Assert.That(iHistoryGridCount > 0);

            //Select and view an email in the list
            int selectRow = 0;
            int attachmentCount = 0;
            while (attachmentCount == 0)
            {
                if (selectRow == iHistoryGridCount)
                {
                    throw new OrderDetailExceptions.EmailWithAttachmentNotFoundException(orderDetail.OrderNumber);
                }

                orderDetailPage.EmailHistoryPopup.EmailHistoryTable.TableRows.Where<Table.TableRow>(
                    q => q.Id == selectRow.ToString()).FirstOrDefault().Click();
                attachmentCount = orderDetailPage.EmailHistoryPopup.AttachmentButtons.Count;

                selectRow++;

            }

            string emailSubject = orderDetailPage.EmailHistoryPopup.SelectedEmailSubjectDiv.Text;
            string emailToList = orderDetailPage.EmailHistoryPopup.SelectedEmailToRecipientsDiv.Text;
            string emailBody = orderDetailPage.EmailHistoryPopup.SelectedEmailBodyTextBox.Text;

            //click on reply
            orderDetailPage.EmailHistoryPopup.ReplyToEmailAttachmentsButton.Click();

            //Ensure there are recipients and an email body
            Assert.That(orderDetailPage.EmailComposePopup.SubjectTextBox.Text.Contains(emailSubject));
            Assert.That(orderDetailPage.EmailComposePopup.ToRecipientTextBox.Text.Contains(emailToList));
            Assert.That(orderDetailPage.EmailComposePopup.EmailBodyTextArea.Text.Contains(emailBody));

            //ensure that all attachments came over
            Assert.That(orderDetailPage.EmailComposePopup.AttachmentButtons.Count == attachmentCount);

            //send the email
            orderDetailPage.EmailComposePopup.SendEmailButton.Click();
            Assert.That(orderDetailPage.WaitForAndDismissAlert());

            //check the database to find the email by template and ensure it is recent
            Data.DTO.Email email = oOp.getOrderEmails(orderDetail.OrderRequestId, null)
                .OrderByDescending(r => r.EmailDateTime).FirstOrDefault();
            Assert.That(email.EmailDateTime >= DateTime.Now.AddMinutes(-3));

            //check the database to make sure attachments are there
            Assert.That(email.Attachments.Count == attachmentCount);


        }
        
        [Test]
        public void Test_Intgr_OrderDetail_Order_OrderActivitySave()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //this order needs to be a new order so we can test when a voes picks up the order for the first time
            string oldOrderStatus = "New";
            if (orderDetailPage.OrderActivity.VerificationStatusSelect.SelectedText != oldOrderStatus)
            {
                throw new OrderDetailExceptions.UnexpectedOrderStatusException(OrderNumber, oldOrderStatus);
            }

            //update order status to pending
            string newStatus = "Pending";
            orderDetailPage.OrderActivity.VerificationStatusSelect.Options.Where<DropDownBox.DropDownBoxOption>
                (q => q.Text == newStatus).FirstOrDefault().Click();

            string ActivityNote = "Test Updating Status";
            orderDetailPage.OrderActivity.ActivityNoteTextArea.SendKeys(ActivityNote);

            orderDetailPage.OrderActivity.SaveActivityButton.Click();
            orderDetailPage.WaitForAndDismissAlert();

            //get contents of first row of order activity
            Dictionary<string, UiGrid.UiGridCellElement> firstRow = UiGrid.getGridRow(ngDriver, orderDetailPage.OrderActivity.OrderActivityGrid, 0);
              
            //check that note and status are now in the order activity table
            Assert.That(firstRow["Note"].Text.Contains(ActivityNote));
            Assert.That(firstRow["Status"].Text.Contains(newStatus));
            Assert.That(DateTime.Parse(firstRow["Date/Time"].Text) >= DateTime.Now.AddMinutes(-3));

            //check that the order status and voes are updated in the order information area
            Assert.That(orderDetailPage.OrderInformation.VerificationStatusTextBox.Text == newStatus);
            Assert.That(orderDetailPage.OrderInformation.VerificationSpecialistTextBox.Text == UserName);


        }

        [Test]
        public void Test_Intgr_OrderDetail_Order_OrderActivityExportToPDF()
        {

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            if (OrderNumber == null)
            {
                throw new TestExceptions.NoAvailOrdersException();
            }

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //this order needs to be a new order so we can get some decent actvity data to export
            string oldOrderStatus = "Approved";
            if (orderDetailPage.OrderActivity.VerificationStatusSelect.SelectedText != oldOrderStatus)
            {
                throw new OrderDetailExceptions.UnexpectedOrderStatusException(OrderNumber, oldOrderStatus);
            }

            //export to PDF
            string fileURL = orderDetailPage.GenerateFile(UI.Model.OrderDetail.DownloadFileType.OrderActivtyPDF);
            string filePathName = convertFileURLtoFilePathName(fileURL);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(filePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(filePathName);
            Assert.That(fi.Length > 0);


        }

        [Test]
        public void Test_Intgr_OrderDetail_Document_DeleteDoc()
        {

            string DeletedDocFileName = String.Empty;

            OrderOps oOp = new OrderOps();
            List<Data.DBSchema.DocumentOrderView> docs = new List<DocumentOrderView>();
            int OrderRequestId = oOp.getOrderWithDeletableDocuments(out docs, UserName);
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderRequestId);

            LogOrderNumber(orderDetail.OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            UI.Model.OrderDetail._OrderInformation.DocGroupLink linkedDocLink = orderDetailPage.OrderInformation.DocumentLinkGroup
                .Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>(q => q.Text.ToLower().Contains("linked")).FirstOrDefault();

            //get the count of documents
            int docCount = linkedDocLink.DocumentCount;

            //Click on the “Linked Docs” link.
            linkedDocLink.Click();

            //make sure count is at least the same as in the link
            Assert.AreEqual(docCount, orderDetailPage.LinkedDocsPopup.LinkedDocs.TableRows.Count +
                orderDetailPage.LinkedDocsPopup.EncDocs.TableRows.Count);

            //find a deletable document and delete it           
            foreach (UI.Model.OrderDetail.LinkedDocRow row in orderDetailPage.LinkedDocsPopup.LinkedDocTableRows)
            {

                if (row.DeleteButton.Displayed)
                {
                    DeletedDocFileName = row.FileName.Replace(" ", "");
                    row.DeleteButton.Click();

                    //are you sure popup box
                    orderDetailPage.WaitForAndAcceptAlert();
                    orderDetailPage.WaitForAndDismissAlert("Successfully Deleted");
                    break;
                }
            }

            //if this is empty then no doc found to delete
            Assert.IsNotEmpty(DeletedDocFileName);

            //make sure file is not in file system
            Assert.That(!System.IO.File.Exists(DeletedDocFileName));

        }

        [Test]
        public void Test_Intgr_OrderDetail_Document_UploadDoc()
        {

            string OrderNumber = getRandomOrderByCriteria(
                        new List<string> { "Pending" },
                        new List<string> { "Initial" },
                        new List<string> { },
                        new List<string> { }
                        ).OrderNumber;

            LogOrderNumber(OrderNumber);

            OrderOps oOp = new OrderOps();
            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            UI.Model.OrderDetail._OrderInformation.DocGroupLink linkedDocLink = orderDetailPage.OrderInformation.DocumentLinkGroup
                .Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>(q => q.Text.ToLower().Contains("linked")).FirstOrDefault();

            //get the original count of documents
            int origDocCount = linkedDocLink.DocumentCount;

            //Click on the "Add New Document" link.
            Link addnewDocLink = orderDetailPage.OrderInformation.DocumentLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("new")).FirstOrDefault();
            addnewDocLink.Click();

            //upload test file - put this proc in model
            string FileName = ResourcesFileNames.BorrowerAuthFormPDF.GetDescription();
            string FilePathName = ResourcesPath + FileName;
            orderDetailPage.UploadDocsPopup.UploadFile(FilePathName);

            //check that the file is listed in the upload results area
            Assert.That(orderDetailPage.UploadDocsPopup.UploadedFiles.Count == 1);

            //check that the file name is correct
            UI.Model.OrderDetail._UploadDocsPopup.UploadedFile upFile = orderDetailPage.UploadDocsPopup.UploadedFiles[0];
            Assert.That(upFile.FileName == FileName);

            //check that progressbar is green class, indicating success
            Assert.That(!upFile.ProgressBar.GetAttribute("class").ToLower().Contains("fail"));

            //check that file exists in repository and that it has a non-zero filesize
            Document doc = getDocumentById(upFile.DocumentId);
            string UploadedFilePathName = UploadPath + doc.UniqueFileName;

            Assert.That(System.IO.File.Exists(UploadedFilePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(UploadedFilePathName);
            Assert.That(fi.Length > 0);

            //close popup and check that new doc appears in linked docs
             orderDetailPage.UploadDocsPopup.CloseButton.Click();
            
            //get the new count of documents
            ngDriver.WaitForAngular();
            int newDocCount =  orderDetailPage.OrderInformation.DocumentLinkGroup
                .Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>(q => q.Text.ToLower().Contains("linked")).FirstOrDefault().DocumentCount;
            Assert.That(newDocCount == origDocCount + 1);
            
           
        }

        [Test]
        public void Test_Intgr_OrderDetail_Document_UploadDocCheckbox()
        {
            OrderOps oOp = new OrderOps();

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            LogOrderNumber(OrderNumber);

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //open linked docs popup
            orderDetailPage.OrderInformation.DocumentLinkGroup.Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>
                (q => q.Text.ToLower().Contains("linked")).FirstOrDefault().Click();

            //find a doc that has the checkbox checked and is enabled
            //we know there is one if we just did the upload test 
            //otherwise, just upload one
            UI.Model.OrderDetail.LinkedDocRow uploadableDoc = null;

            //THIS IS WRONG.  need to exclude loan-level documents.  Also fix bug here with the upload status..see trello
            uploadableDoc = orderDetailPage.LinkedDocsPopup.LinkedDocTableRows.Where<UI.Model.OrderDetail.LinkedDocRow>
                (q => q.UploadQueueCheckbox.Enabled && q.UploadQueueCheckbox.Checked).FirstOrDefault();

            if (uploadableDoc == null)
            {
                //upload a document
                orderDetailPage.LinkedDocsPopup.CloseButton.Click();
                orderDetailPage.OrderInformation.DocumentLinkGroup.Where<Link>(q => q.Text.ToLower().Contains("new")).FirstOrDefault().Click();

                string FileName = ResourcesFileNames.BorrowerAuthFormPDF.GetDescription();
                string FilePathName = ResourcesPath + FileName;
                orderDetailPage.UploadDocsPopup.UploadFile(FilePathName);

                orderDetailPage.UploadDocsPopup.CloseButton.Click();

                orderDetailPage.OrderInformation.DocumentLinkGroup.Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>
                    (q => q.Text.ToLower().Contains("linked")).FirstOrDefault().Click();

                uploadableDoc = orderDetailPage.LinkedDocsPopup.LinkedDocTableRows.Where<UI.Model.OrderDetail.LinkedDocRow>
                    (q => q.UploadQueueCheckbox.Enabled && q.UploadQueueCheckbox.Checked).FirstOrDefault();

            }

            //uncheck the box
            uploadableDoc.UploadQueueCheckbox.Click();
            string uploadableDocFileName = uploadableDoc.FileName;
            ngDriver.WaitForAngular();

            //refresh the page
            ngDriver.WrappedDriver.Url = baseUrl + "orderdetails/order/" + orderDetail.OrderRequestId.ToString();

            //init model
            orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //open linked docs popup
            orderDetailPage.OrderInformation.DocumentLinkGroup.Where<UI.Model.OrderDetail._OrderInformation.DocGroupLink>
                (q => q.Text.ToLower().Contains("linked")).FirstOrDefault().Click();

            //find the document, assuming that the box is no longer checked
            uploadableDoc = orderDetailPage.LinkedDocsPopup.LinkedDocTableRows.Where<UI.Model.OrderDetail.LinkedDocRow>
                    (q => q.UploadQueueCheckbox.Enabled && !q.UploadQueueCheckbox.Checked && q.FileName == uploadableDocFileName).FirstOrDefault();

            Assert.NotNull(uploadableDoc);

        }

        [Test]
        public void Test_Intgr_OrderDetail_Order_ApproveOrder()
        {
            OrderOps oOp = new OrderOps();

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            LogOrderNumber(OrderNumber);

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //change to approved status
            orderDetailPage.OrderActivity.VerificationStatusSelect.Options.Where<DropDownBox.DropDownBoxOption>
               (q => q.Text == "Approved").FirstOrDefault().Click();

            //fixing fields so we can save activity/approve
            string errorFieldName = null;
            while (!orderDetailPage.CanSaveOrderActivity(out errorFieldName))
            {
                orderDetailPage.ValidifyOrderActivityField(errorFieldName);
            }

            //get current copy of order activity table
            int oldActivityCount = orderDetailPage.OrderActivity.OrderActivityGrid.RowCount;

            //click on save order activity
            orderDetailPage.OrderActivity.SaveActivityButton.Click();
            orderDetailPage.WaitForAndDismissAlert("order activity saved");

            int newActivityCount = orderDetailPage.OrderActivity.OrderActivityGrid.RowCount;

            //check that there are new activity entries
            //Assert.That(newActivityCount >= oldActivityCount + 2);  //sometimes there are additional activity entries, but 2 is the min
            //this is not useful with the ui-grid as the count coming back is just in the viewport
            
            //check that the link to the cert is there
            //finding cert link row
            int certRowNumber = newActivityCount - oldActivityCount - 2;
            if (certRowNumber < 0) { certRowNumber = 0; };
            Dictionary<string, UiGrid.UiGridCellElement> firstRow = UiGrid.getGridRow(ngDriver, orderDetailPage.OrderActivity.OrderActivityGrid, certRowNumber);
            KeyValuePair<string, UiGrid.UiGridCellElement> certCellElement = firstRow.Where<KeyValuePair<string, UiGrid.UiGridCellElement>>(q => q.Key == "Note").FirstOrDefault();

            Assert.That(certCellElement.Value.Text.ToLower().Contains("click here to view certification"));

            //get link to document
            Link certLink = orderDetailPage.GetObjectGroup<Link>(new ReadOnlyCollection<NgWebElement>(
                    new List<NgWebElement> { certCellElement.Value }
                ))[0];

            string certPath = convertFileURLtoFilePathName(certLink.URL);

            //check that file exists in repository and that it has a non-zero filesize
            Assert.That(System.IO.File.Exists(certPath));
            System.IO.FileInfo fi = new System.IO.FileInfo(certPath);
            Assert.That(fi.Length > 0);


        }

        [Test]
        public void Test_Intgr_OrderDetail_BulkActions_Update()
        {
            OrderOps oOp = new OrderOps();

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "New" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            LogOrderNumber(OrderNumber);

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //GUIOps guiOp = new GUIOps();
            //guiOp.OpenDeveloperTools();

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //get count of loan orders for later use
            int ordersforLoanCount = orderDetailPage.RelatedOrdersInformation.OrderRelatedLoansGrid.RowCount;

            //click on bulk order actions
            orderDetailPage.BulkOrderActionsButton.Click();

            //test to see that all borrowers on the loan are in the drop-down
            List<string> buBorrowerSSNs = orderDetailPage.BulkEditPopup.BorrowerSelect.Options.Where<DropDownBox.DropDownBoxOption>(
                r => r.Value != "0").Select<DropDownBox.DropDownBoxOption, string> (q => q.Value).OrderBy(x => x).ToList();

            List<string> borrowerSSNs = oOp.getBorrowerListForLoan(orderDetail.LoanNumber).Select<Data.DTO.Borrower, string>(
                q => q.SSN).OrderBy(x => x).ToList();
            
            Assert.AreEqual(buBorrowerSSNs, borrowerSSNs);

            //add activity note to all orders on the loan
            string testNote = "Test ActivityNote for Bulk Edit";
            orderDetailPage.BulkEditPopup.ActivityNote.SendKeys(testNote);

            //check all order statuses
            foreach (Checkbox status in orderDetailPage.BulkEditPopup.OrderStatuses)
            {
                status.Click();
            }

            //check all request types
            foreach (Checkbox rtype in orderDetailPage.BulkEditPopup.RequestTypes)
            {
                rtype.Click();
            }

            //process changes, putting expected loan count in message
            //if the loan count is off, then this will casue failed assertion
            orderDetailPage.BulkEditPopup.ProcessChangesButton.Click();
            orderDetailPage.WaitForAndDismissAlert(ordersforLoanCount.ToString());

            //now need to verify in related orders that the new note is there
            List<int> relOrderIds = getOrdersByCriteria(new List<string> { }, new List<string> { }, new List<string> { }, new List<string> { },
                0, null, orderDetail.LoanNumber)
                .Select<Data.DTO.OrderSearchResp, int>(q => q.OrderRequestId).ToList();

            foreach(int relOrderId in relOrderIds)
            {
                Assert.That(oOp.getLastOrderActivity(relOrderId).ActivityNote.Contains(testNote));
            }




        }

        [Test]
        public void Test_Intgr_OrderDetail_Order_PanicMode()
        {
            OrderOps oOp = new OrderOps();

            List<OrderSearchResp> orders = getOrdersByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).ToList();

            //make sure this is not already in panic mode
            string OrderNumber = orders.Where<OrderSearchResp>(q => q.IsPanicMode == false)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault().OrderNumber;

            LogOrderNumber(OrderNumber);

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //GUIOps guiOp = new GUIOps();
            //guiOp.OpenDeveloperTools();

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //make sure panic mode button starts out red
            Assert.That(!orderDetailPage.PanicModeButton.GetAttribute("class").Contains("leavePanicMode"));

            //click panic mode button
            orderDetailPage.PanicModeButton.Click();

            //close out of email popup
            orderDetailPage.EmailComposePopup.ClosePopupButton.Click();

            //make sure panic mode button is now green
            Assert.That(orderDetailPage.PanicModeButton.GetAttribute("class").Contains("leavePanicMode"));

            //make sure that there is now a panic mode entry in order activity
            Assert.That(String.Join("|", orderDetailPage.OrderActivity.OrderActivityGrid.RowText[0])
                .ToLower().Contains("panic mode entered"));


        }

        [Test]
        public void Test_Intgr_OrderDetail_Order_Subcontract()
        {
            OrderOps oOp = new OrderOps();

            string OrderNumber = getRandomOrderByCriteria(new List<string> { "Pending" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderNumber;

            LogOrderNumber(OrderNumber);

            Data.DTO.OrderDetailResp orderDetail = oOp.getOrderDetail(OrderNumber);

            //just nav right to order detail
            string pageURL = "orderdetails/order/" + orderDetail.OrderRequestId.ToString();
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password, pageURL);

            //GUIOps guiOp = new GUIOps();
            //guiOp.OpenDeveloperTools();

            //init model
            UI.Model.OrderDetail orderDetailPage = null;
            orderDetailPage = new UI.Model.OrderDetail(ngDriver);

            //click on subcontractorder button
            orderDetailPage.SubcontractOrderButton.Click();

            //click on actual order submit button
            orderDetailPage.SubcontractPopup.OrderSubmitButton.Click();
            orderDetailPage.WaitForAndDismissAlert("order has been created");

            //make sure button text has updated
            Assert.That(orderDetailPage.SubcontractOrderButton.Text.ToLower().Contains("cancel subcontracted order"));

            //ensure the order activity contains entry for subcontracting
            Assert.That(String.Join("|", orderDetailPage.OrderActivity.OrderActivityGrid.RowText[0])
                .ToLower().Contains("order sent to"));


        }

        public class OrderDetailException : TestException { }

        public static class OrderDetailExceptions
        {

            public class EmailWithAttachmentNotFoundException : OrderDetailException
            {

                public string OrderNumber;

                public override string Message
                {
                    get
                    {
                        return "An Email with an attachment was not found for this order.  Create an email with an attachment for Order Number " + OrderNumber;
                    }
                }

                public EmailWithAttachmentNotFoundException(string orderNumber)
                    : base()
                {
                    OrderNumber = orderNumber;
                }

            }

            public class UnexpectedOrderStatusException : OrderDetailException
            {

                public string OrderNumber;
                public string RequiredStatusName;

                public override string Message
                {
                    get
                    {
                        return "The current order " + OrderNumber + " does not have the status requried to complete this test (" + RequiredStatusName + ")";
                    }
                }

                public UnexpectedOrderStatusException(string orderNumber, string requiredStatusName)
                    : base()
                {
                    OrderNumber = orderNumber;
                    RequiredStatusName = requiredStatusName;
                }

            }

            public class UnexpectedGroupMemberException : OrderDetailException
            {
                public string GroupName;

                public override string Message
                {
                    get
                    {
                        return "Link Group " + GroupName + " contains unexpected elements";
                    }
                }

                public UnexpectedGroupMemberException(string groupName)
                    : base()
                {
                    GroupName = groupName;
                }

            }

            public class UnsupportedEditingFieldTypeException : OrderDetailException
            {

                public string FieldType;

                public override string Message
                {
                    get
                    {
                        return "Field " + FieldType + " is not supported for field edit check.  Add support.";
                    }
                }

                public UnsupportedEditingFieldTypeException(string fieldType)
                    : base()
                {
                    FieldType = fieldType;
                }

            }

            public class OrderWithDeletableDocumentNotFoundException : OrderDetailException
            {

                public override string Message
                {
                    get
                    {
                        return "An order with a deletable document was not found.  Run the Non-Borrower "
                            + "Entry test prior to this test in order to create an order with a deletable document.";
                    }
                }



            }

        }

        
    }

  
}
