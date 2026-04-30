using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Moq;
using NUnit.Framework;
using ServiceStack.OrmLite;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.Text;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;
using VOESystem.UnitTests.Business;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    [TestFixture]
    public class EmailService : ServiceTestBase
    {

        //"/api/email/template/list"
        [Test]
        public void Test_Services_EmailService_ListEmailTemplatesShowAll()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            VOESystem.Services.EmailService.TemplateListResponse response = oService.Any(new VOESystem.Services.EmailService.TemplateListRequest() 
            { 
                ShowAll = true
            });
            
            //test that there are entries
            Assert.That(response.Templates.Count > 0);

            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            int templateCount = eOp.getTemplateCount(true);

            Assert.That(response.Templates.Count == templateCount);
        }

        //"/api/email/template/list"
        [Test]
        public void Test_Services_EmailService_ListEmailTemplatesManualOnly()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            VOESystem.Services.EmailService.TemplateListResponse response = oService.Any(new VOESystem.Services.EmailService.TemplateListRequest()
            {
                ShowAll = false
            });

            //test that there are entries
            Assert.That(response.Templates.Count > 0);

            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            int templateCount = eOp.getTemplateCount(false);

            Assert.That(response.Templates.Count == templateCount);
        }

        //"/api/email/template/update"
        [Test]
        public void Test_Services_EmailService_UpdateEmailTemplate()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            //get a random template to modify
            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            Data.DTO.EmailTemplate tmp = eOp.getRandomEmailTemplate(false);
            
            //modify some fields
            string newTestName = "This is a test Template Name";
            string newTestSubject = "This is a test Template Subject";
            string newTestBody = "This is a test Template Body";
            string newTestPriority = "Low";

            //add a new recipient
            string newTestRecipientEmail = "unittest@firsthome.com";
            Data.DTO.EmailTemplate.Recipient recipNew = new Data.DTO.EmailTemplate.Recipient
            {
                RecipientSendTypeId = 1, //this is a To recipient
                StaticEmailAddress = newTestRecipientEmail
            };

            List<Data.DTO.EmailTemplate.Recipient> newRecipients = tmp.Recipients;
            newRecipients.Add(recipNew);

            try
            {
                VOESystem.Services.EmailService.TemplateSaveResponse response = oService.Any(new VOESystem.Services.EmailService.TemplateSaveRequest
                {
                    Id = tmp.Id,
                    Name = newTestName,
                    Subject = newTestSubject,
                    Body = newTestBody,
                    Priority = newTestPriority,
                    Recipients = newRecipients,
                    FormFields = tmp.FormFields,
                    SendTriggers = tmp.SendTriggers
                });

                //get our test template
                Data.DTO.EmailTemplate newTmp = eOp.getEmailTemplate(tmp.Id);

                //test that it still exists
                Assert.That(newTmp != null);

                //test modified fields
                Assert.That(newTmp.Name == newTestName);
                Assert.That(newTmp.Subject == newTestSubject);
                Assert.That(newTmp.Body == newTestBody);
                Assert.That(newTmp.Priority == newTestPriority);
                Assert.That(newTmp.Recipients == newRecipients);


            }
            catch (Exception ex) 
            {
                logger.Error("Error Posting Updated TemplateId:" + tmp.ToString(), ex);
            }
            finally
            {
                //put it back the way it was (try to)
                VOESystem.Services.EmailService.TemplateSaveResponse response = oService.Any(new VOESystem.Services.EmailService.TemplateSaveRequest
                {
                    Id = tmp.Id,
                    Name = tmp.Name,
                    Subject = tmp.Subject,
                    Body = tmp.Body,
                    Priority = tmp.Priority,
                    Recipients = tmp.Recipients,
                    FormFields = tmp.FormFields,
                    SendTriggers = tmp.SendTriggers
                });
            }


        }

        //"/api/email/template/preview"
        [Test]
        public void Test_Services_EmailService_PreviewEmailTemplate()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            //get a random template to preview
            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            Data.DTO.EmailTemplate tmp = eOp.getRandomEmailTemplate(false);

            //preview the email
            VOESystem.Data.DTO.Email response = oService.Any(new VOESystem.Services.EmailService.TemplatePreviewRequest
            {
                Id = tmp.Id,
                Name = tmp.Name,
                Subject = tmp.Subject,
                Body = tmp.Body,
                Priority = tmp.Priority,
                Recipients = tmp.Recipients,
                FormFields = tmp.FormFields,
                SendTriggers = tmp.SendTriggers
            });

            //check the individual email values
            checkEmailTemplateValues(tmp, response, VOESystem.Services.EmailService.previewVals);

        }

        //"/api/email/generate"
        [Test]
        public void Test_Services_EmailService_GenerateEmail()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            //get a random template to fill in
            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            Data.DTO.EmailTemplate tmp = eOp.getRandomEmailTemplate(true);

            //get random order to use
            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            //generate the email
            VOESystem.Data.DTO.Email response = oService.Any(new VOESystem.Services.EmailService.EmailGenerateRequest
            {
                OrderRequestId = OrderRequestId,
                Template = tmp
            });

            //check the individual email values
            Dictionary<string, string> emailValues = eOp.getLoanDataForEmail(OrderRequestId, tmp.Id);
            checkEmailTemplateValues(tmp, response, emailValues);


        }

        public void checkEmailTemplateValues(Data.DTO.EmailTemplate template, VOESystem.Data.DTO.Email email, Dictionary<string, string> emailValues )
        {

            //wipe out formfields so we can extract form tags from original template
            template.FormFields = null;

            //extract list of fieldTags
            string templateContents = JsonSerializer.SerializeToString<Data.DTO.EmailTemplate>(template);
            Regex tagRegex = new Regex(@"#\w+?#");

            List<string> formTags = tagRegex.Matches(templateContents)
                .Cast<System.Text.RegularExpressions.Match>()
                .GroupBy(q => q.Value)
                .Select(m => m.First())
                .Select(r => r.Value)
                .ToList();

            string emailContents = JsonSerializer.SerializeToString<Data.DTO.Email>(email);

            //test that preview email does not contain any unused tags
            Assert.That(tagRegex.Matches(emailContents).Count == 0);

            //loop through formtags and make sure they are appropriately filled in
            foreach (string formTag in formTags)
            {
                if (emailValues.Keys.ToList().Exists(q => q == formTag) 
                    && VOESystem.UnitTests.Business.EmailOps.RequiredEmailFormTags.Contains(formTag))
                {
                    string emailValue = emailValues[formTag];
                    if (emailValue == null)
                    {
                        throw new Exception("Form Tag not Found " + formTag);
                    }
                    else
                    {
                        Assert.That(emailContents.Contains(emailValue));
                    }
                }
            }


        }

        //"/api/email/generatereply"
        [Test]
        public void Test_Services_EmailService_GenerateEmailReply()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            VOESystem.Data.DTO.Email origEmail = null;
            int? OrderRequestId = 0;

            //get list of visible, manual templates
            Func<Data.DBSchema.EmailTemplateListView, bool> predicateTmps = q => q.IsManual == true;
            List<int> tmpIdList =  eOp.getEmailTemplateListViews(predicateTmps)
                .Select<Data.DBSchema.EmailTemplateListView, int>(q => q.Id).ToList();

            //get random email
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q => q.SenderEmail != "voe@firsthome.com"
                //&& !q.Message.Contains("Original Message")
                && !q.Subject.ToLower().StartsWith("automatic reply")
                && !q.Subject.ToLower().StartsWith("out of office")
                && !q.SenderEmail.ToLower().Contains("mail.efax.com")
                && Sql.In(q.EmailTemplateId, tmpIdList)
                && q.OrderRequestId != null;
            origEmail = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            logger.Info(JsonSerializer.SerializeToString<VOESystem.Data.DTO.Email>(origEmail));

            //get the template
            Data.DTO.EmailTemplate tmp = eOp.getEmailTemplate(origEmail.EmailTemplateId ?? 0);

            //generate the email
            VOESystem.Data.DTO.Email response = oService.Any(new VOESystem.Services.EmailService.EmailGenerateReplyRequest
            {
                EmailId = origEmail.Id,
                IncludeAttachments = true  //only really need to test this case
            });

            //check the individual email values
            Dictionary<string, string> emailValues = eOp.getLoanDataForEmail(OrderRequestId ?? 0, tmp.Id);
            checkEmailTemplateValues(tmp, response, emailValues);

            //check that the email contains the original body
            Assert.That(response.Message.Contains(origEmail.Message));

            //check that the reply header contains the orig email to, from, subject 
            Assert.That(response.Message.Contains("Original Message"));
            Regex regex = new Regex(@"(?<=(-+)Original\sMessage(-+)\n)(.+?)(?=(\n){2})", RegexOptions.Singleline);
            string emailReplyHeader = regex.Match(response.Message).Value;
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains(origEmail.Subject.CleanCharsForCompare())); //subject
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains(origEmail.SenderEmail.CleanCharsForCompare())); //from
            Assert.That(emailReplyHeader.CleanCharsForCompare().Contains(origEmail.ToRecipientList.FirstOrDefault().EmailAddress.CleanCharsForCompare()));  //to
                
            //Check the email signature 
            regex = new Regex(@"(?:(?!(-+)Original\sMessage(-+)\r\n).)*(?=(-+)Original\sMessage(-+)\r\n)?", RegexOptions.Singleline);
            string emailSignature = regex.Match(response.Message).Value;
            Assert.That(emailSignature.CleanCharsForCompare().Contains(UserFullName.CleanCharsForCompare()));
            Assert.That(emailSignature.CleanCharsForCompare().Contains("voe@firsthome.com".CleanCharsForCompare()));
            Assert.That(!emailSignature.Contains("#"), createEmailExceptionMessage(origEmail.Id, emailSignature));

            //check attachments
            Assert.AreEqual(origEmail.Attachments, response.Attachments);

          
        }

        public string createEmailExceptionMessage(int EmailId, string ExtraInfo)
        {
            return "EmailId:" + EmailId.ToString() + " " + ExtraInfo;
        }
        
        //"/api/email/send"
        [Test]
        public void Test_Services_EmailService_SendEmail()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            //to make this easy, we are going to generate a reply from a random email then try to send that
            VOESystem.UnitTests.Business.EmailOps eOp = new VOESystem.UnitTests.Business.EmailOps();
            VOESystem.Data.DTO.Email origEmail = null;
            int? OrderRequestId = 0;

            //get list of visible, manual templates
            Func<Data.DBSchema.EmailTemplateListView, bool> predicateTmps = q => q.IsManual == true;
            List<int> tmpIdList = eOp.getEmailTemplateListViews(predicateTmps)
                .Select<Data.DBSchema.EmailTemplateListView, int>(q => q.Id).ToList();

            //get random email
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q => q.SenderEmail != "voe@firsthome.com"
                //&& !q.Message.Contains("Original Message")
                && !q.Subject.ToLower().StartsWith("automatic reply")
                && !q.Subject.ToLower().StartsWith("out of office")
                && !q.SenderEmail.ToLower().Contains("mail.efax.com")
                && Sql.In(q.EmailTemplateId, tmpIdList)
                && q.OrderRequestId != null;
            origEmail = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            //generate the email reply
            VOESystem.Data.DTO.Email email = eOp.getReplyEmail(origEmail.Id);
            email.Forms = new List<FormListItem> { }; //normally this is done in the service
    
            var oResp = oService.Any(new VOESystem.Services.EmailService.ManualSendRequest
            {
                OrderRequestId = OrderRequestId ?? 0,
                Email = email,
                ConsolidateAttachments = false,
                FormNotes = null, 
                IsAuditing = false
            });

            VOESystem.Services.EmailService.ManualSendResponse response = (VOESystem.Services.EmailService.ManualSendResponse)oResp;

            //check that response is correct
            Assert.That(response.SendResult.Contains("Email Sent"));

            //check the database to find the email and ensure it is recent
            VOESystem.UnitTests.Business.OrderOps oOp = new VOESystem.UnitTests.Business.OrderOps();
            VOESystem.Data.DTO.Email newEmail = oOp.getOrderEmails(OrderRequestId ?? 0, null)
                .OrderByDescending(r => r.Id).FirstOrDefault();
                
            Assert.That(newEmail.EmailDateTime >= DateTime.Now.AddMinutes(-3)); 


        }

        //"/api/order/email/list"
        [Test]
        public void Test_Services_EmailService_ListEmail()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            //get random order to use
            int OrderRequestId = getRandomOrderByCriteria(new List<string> { "Approved" },
                new List<string> { "Initial" }, new List<string> { }, new List<string> { }).OrderRequestId;

            VOESystem.Services.EmailService.EmailHistoryResponse response = oService.Any(new VOESystem.Services.EmailService.EmailHistoryRequest
            {
                OrderRequestId = OrderRequestId
            });

            //get email history directly from business layer
            VOESystem.UnitTests.Business.OrderOps oOp = new VOESystem.UnitTests.Business.OrderOps();
            List<VOESystem.Data.DTO.Email> emails = oOp.getOrderEmails(OrderRequestId, null);

            //check that the results are the same
            Assert.That(response.Emails.Count == emails.Count);
            foreach(int emailId in emails.Select(q => q.Id).ToList())
            {
                Assert.That(response.Emails.Select(q => q.Id).ToList().Exists(q => q == emailId));
            }
            
        }
        
        //"/api/email/readstatus/update"
        [Test]
        public void Test_Services_EmailService_ReadEmailSuccess()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            int? OrderRequestId = 0;

            //get random unread email
            VOESystem.UnitTests.Business.EmailOps eOp = new Business.EmailOps();
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q => 
                q.DateTimeReceived != null
                && q.ReadUserName == null
                && q.OrderRequestId != null;
            Data.DTO.Email email = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            //make sure that the voes assigned to this order is the current user
            VOESystem.UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            oOp.updateOrderVOEAssignment(OrderRequestId ?? 0, UserName);

            //try to update email read status
            VOESystem.Services.EmailService.EmailMarkReadResponse response = oService.Any(new VOESystem.Services.EmailService.EmailMarkReadRequest
            {
                EmailId = email.Id
            });

            //check response value
            Assert.That(response.MarkResult);

            //check that the email is now "read"
            Expression<Func<Data.DBSchema.Email, bool>> predicateNewEmail = q =>
                q.Id == email.Id;
            Data.DTO.Email newEmail = eOp.getRandomEmail<Data.DBSchema.Email>(predicateNewEmail, out OrderRequestId);

            Assert.That(newEmail.IsRead);


        }

        //"/api/email/readstatus/update"
        [Test]
        public void Test_Services_EmailService_ReadEmailFailureNotCorrectUser()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            int? OrderRequestId = 0;

            //get random unread email
            VOESystem.UnitTests.Business.EmailOps eOp = new Business.EmailOps();
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q =>
                q.DateTimeReceived != null
                && q.ReadUserName == null
                && q.OrderRequestId != null;
            Data.DTO.Email email = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            //get random assignable user
            VOESystem.UnitTests.Business.BaseDataOps bOp = new Business.BaseDataOps();
            VOESystem.Data.DBSchema.UserRoleView voe = bOp.getVOESpecialistList()
                .Where(q => q.UserName != UserName).FirstOrDefault();

            //make sure that the voes assigned to this order is NOT the current user
            VOESystem.UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            oOp.updateOrderVOEAssignment(OrderRequestId ?? 0, voe.UserName);

            //try to update email read status
            VOESystem.Services.EmailService.EmailMarkReadResponse response = oService.Any(new VOESystem.Services.EmailService.EmailMarkReadRequest
            {
                EmailId = email.Id
            });

            //check response value is false
            Assert.That(!response.MarkResult);

            //check that the email is still unread
            Expression<Func<Data.DBSchema.Email, bool>> predicateNewEmail = q =>
                q.Id == email.Id;
            Data.DTO.Email newEmail = eOp.getRandomEmail<Data.DBSchema.Email>(predicateNewEmail, out OrderRequestId);

            Assert.That(!newEmail.IsRead);


        }

        //"/api/email/readstatus/update"
        [Test]
        public void Test_Services_EmailService_ReadEmailFailureLoanLevelEmail()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            int? OrderRequestId = 0;

            //get random unread email
            VOESystem.UnitTests.Business.EmailOps eOp = new Business.EmailOps();
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q =>
                q.DateTimeReceived != null
                && q.ReadUserName == null
                && q.OrderRequestId == null;
            Data.DTO.Email email = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            //try to update email read status
            VOESystem.Services.EmailService.EmailMarkReadResponse response = oService.Any(new VOESystem.Services.EmailService.EmailMarkReadRequest
            {
                EmailId = email.Id
            });

            //check response value is false
            Assert.That(!response.MarkResult);

            //check that the email is still unread
            Expression<Func<Data.DBSchema.Email, bool>> predicateNewEmail = q =>
                q.Id == email.Id;
            Data.DTO.Email newEmail = eOp.getRandomEmail<Data.DBSchema.Email>(predicateNewEmail, out OrderRequestId);

            Assert.That(!newEmail.IsRead);


        }

        //"/api/email/user/unread/count"
        [Test]
        public void Test_Services_EmailService_GetUnreadEmailCount()
        {

            //get random user
            VOESystem.UnitTests.Business.BaseDataOps bOp = new Business.BaseDataOps();
            VOESystem.Data.DBSchema.UserRoleView voe = bOp.getVOESpecialistList()
                .Where(q => q.UserName != UserName)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            //get unread email count from business layer
            UnitTests.Business.EmailOps eOp = new Business.EmailOps();
            int origUnreadEmailCount = eOp.getUnreadEmailCountForUser(voe.UserName);

            //get unread email count from service layer
            Services.EmailService oService = GetServiceInstance<Services.EmailService>(false, null, voe.UserName);
            int response = oService.Any(new VOESystem.Services.EmailService.EmailUnreadCountRequest {});

            Assert.That(origUnreadEmailCount == response);

        }

        //"/api/email/draft/detail"
        [Test]
        public void Test_Services_EmailService_GetEmailDraft()
        {
            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            UnitTests.Business.EmailOps eOp = new Business.EmailOps();

            //get random draft record
            int? OrderRequestId = 0;

            Expression<Func<VOESystem.Data.DBSchema.EmailDraft, bool>> predicateEmail = q => q.OrderRequestId != null;
            Data.DTO.Email emailDraft = eOp.getRandomEmail<VOESystem.Data.DBSchema.EmailDraft>(predicateEmail, out OrderRequestId);
            
            //get the draft
            VOESystem.Data.DTO.Email response = oService.Any(new VOESystem.Services.EmailService.EmailGetDraftRequest
            {
                OrderRequestId = OrderRequestId ?? 0
            });

            //test to see if values agree
            Assert.That(response.EmailDateTime == emailDraft.EmailDateTime);
            Assert.That(isNull(response.Subject, "") == isNull(emailDraft.Subject,""));
            Assert.That(isNull(response.Message, "") == isNull(emailDraft.Message, ""));
            Assert.That(response.SenderName == emailDraft.SenderName);
            Assert.That(response.SenderEmail == emailDraft.SenderEmail);
            Assert.That(response.EmailTemplateId == emailDraft.EmailTemplateId);

            Assert.That(response.Priority == emailDraft.Priority);
            Assert.That(response.ReadReceiptRequested == emailDraft.ReadReceiptRequested);
            Assert.That(response.IsRead == emailDraft.IsRead);

            Assert.AreEqual(JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(emailDraft.ToRecipientList),
                JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(response.ToRecipientList));
            Assert.AreEqual(JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(emailDraft.CcRecipientList),
                JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(response.CcRecipientList));
            Assert.AreEqual(JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(emailDraft.BccRecipientList),
                JsonSerializer.SerializeToString<List<Data.DTO.Email.Recipient>>(response.BccRecipientList));

            Assert.AreEqual(JsonSerializer.SerializeToString<List<Data.DTO.FormListItem>>(emailDraft.Forms),
                JsonSerializer.SerializeToString<List<Data.DTO.FormListItem>>(response.Forms));
            Assert.AreEqual(JsonSerializer.SerializeToString<List<Data.DTO.AttachmentListItem>>(emailDraft.Attachments),
                JsonSerializer.SerializeToString<List<Data.DTO.AttachmentListItem>>(response.Attachments));


        }

        //"/api/email/draft/save"
        [Test]
        public void Test_Services_EmailService_SaveEmailDraft()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            UnitTests.Business.EmailOps eOp = new Business.EmailOps();

            //get random draft record
            int? OrderRequestId = 0;

            Expression<Func<VOESystem.Data.DBSchema.EmailDraft, bool>> predicateEmail = q => q.OrderRequestId != null;
            Data.DTO.Email emailDraft = eOp.getRandomEmail<VOESystem.Data.DBSchema.EmailDraft>(predicateEmail, out OrderRequestId);

            string testMessageText = "This is the test part of the draft message";
            emailDraft.Message = testMessageText + emailDraft.Message;

            //save the draft
            VOESystem.Services.EmailService.EmailSaveDraftResponse response = oService.Any(new VOESystem.Services.EmailService.EmailSaveDraftRequest
            {
                OrderRequestId = OrderRequestId ?? 0,
                Email = emailDraft
            });

            //check the response
            Assert.That(response.Result == "Draft Saved");

            //get the draft back from the business layer
            Expression<Func<Data.DBSchema.EmailDraft, bool>> predicateNewEmail = q => q.OrderRequestId == OrderRequestId;
            Data.DTO.Email newEmail = eOp.getRandomEmail<Data.DBSchema.EmailDraft>(predicateNewEmail, out OrderRequestId);

            //check that it contains the new test text
            Assert.That(newEmail.Message.Contains(testMessageText));


        }


        //"/api/email/export"
        [Test]
        public void Test_Services_EmailService_ExportEmailToPDF()
        {

            Services.EmailService oService = GetServiceInstance<Services.EmailService>();

            int? OrderRequestId = 0;

            //get random email
            VOESystem.UnitTests.Business.EmailOps eOp = new Business.EmailOps();
            Expression<Func<Data.DBSchema.Email, bool>> predicateEmail = q => q.OrderRequestId != null && q.ReadDateTime > DateTime.Now.AddDays(-90);
            Data.DTO.Email email = eOp.getRandomEmail<Data.DBSchema.Email>(predicateEmail, out OrderRequestId);

            //export the email to pdf
            VOESystem.Services.EmailService.EmailSavePDFResponse response = oService.Any(new VOESystem.Services.EmailService.EmailSavePDFRequest
            {
                OrderRequestId = OrderRequestId ?? 0,
                EmailId = email.Id
            });

            Assert.That(response.Result == "Email Saved to Linked Docs");

            //check that file exists in repository and that it has a non-zero filesize
            VOESystem.UnitTests.Business.OrderOps oOp = new Business.OrderOps();
            DocumentOrderView doc = oOp.getOrderDocuments(OrderRequestId ?? 0)
                .OrderByDescending(q => q.DocumentId).FirstOrDefault();

            string UploadedFilePathName = UploadPath + doc.UniqueFileName;

            Assert.That(System.IO.File.Exists(UploadedFilePathName));
            System.IO.FileInfo fi = new System.IO.FileInfo(UploadedFilePathName);
            Assert.That(fi.Length > 0);

        }


    }
}
