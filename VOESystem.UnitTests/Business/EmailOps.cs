using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using System.Data;
using VOESystem.Data;
using System.Linq.Expressions;

namespace VOESystem.UnitTests.Business
{
    public class EmailOps : BusinessBase
    {
        public static List<string> RequiredEmailFormTags = new List<string>()
        {
            "#borrowerfullname#",
            "#borrowerssnlast4#",
            "#employername#",
            "#loannumber#",
            "#loanofficername#",
            "#orderapprovaldate#",
            "#ordernumber#",
            "#ordertype#",
            "#pipelinelink#",
            "#requesttype#",
            "#schedclosingdate#",
            "#VOESFAX#",
            "#VOESFullName#",
            "#voespecialist#",
            "#voespecialistfullname#",
            "#voespecialistphone#",
            "#VOESPhone#",
            "#voessignaturetemplate#",
            "#voesystememail#",
            "#voesystemsignature#"
        };

        public int getTemplateCount(bool ShowAll)
        {

            int retVal = 0;

            if (ShowAll)
            {
                retVal = (int)Db.Count<VOESystem.Data.DBSchema.EmailTemplateListView>();
            }
            else
            {
                retVal = (int)Db.Count<VOESystem.Data.DBSchema.EmailTemplateListView>(q => q.IsManual == true);
            }

            return retVal;

        }

        public VOESystem.Data.DTO.EmailTemplate getRandomEmailTemplate(bool ManualOnly)
        {

            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            return eOp.getEmailTemplates(Db, ManualOnly).OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        }

        public VOESystem.Data.DTO.EmailTemplate getEmailTemplate(int TemplateId)
        {

            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            return eOp.getEmailTemplates(Db, false).Where(q => q.Id == TemplateId).FirstOrDefault();

        }

        public List<VOESystem.Data.DBSchema.EmailTemplateListView> getEmailTemplateListViews(Func<Data.DBSchema.EmailTemplateListView, bool> predicate = null)
        {

            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            List<VOESystem.Data.DBSchema.EmailTemplateListView> tmps = Db.Select<Data.DBSchema.EmailTemplateListView>().ToList();

            if (predicate != null)
            {
                tmps = tmps.Where<VOESystem.Data.DBSchema.EmailTemplateListView>(predicate).ToList();
            }

            return tmps;
            

        }

        public Dictionary<string, string> getLoanDataForEmail(int OrderRequestId, int? TemplateId)
        {
            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            return eOp.getLoanDataForEmailTemplate(Db, OrderRequestId, baseUrl, null, TemplateId);

        }

        public VOESystem.Data.DTO.Email getRandomEmailForOrder(int OrderRequestId, Func<Data.DTO.Email,bool> predicate = null)
        {

            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            List<VOESystem.Data.DTO.Email> emails = eOp.getEmailHistory(Db, OrderRequestId, baseUrl, false).ToList();
            if (predicate != null)
            {
                emails = emails.Where<VOESystem.Data.DTO.Email>(predicate).ToList();
            }
                
            return emails.OrderBy(x => Guid.NewGuid()).FirstOrDefault();

        }

        public VOESystem.Data.DTO.Email getRandomEmail<T>(Expression<Func<T, bool>> predicate, out int? OrderRequestId)
            where T : VOESystem.Data.Interfaces.IEmailItem
        {

            //first get the email id
            T email = (T)Db.SelectParam(predicate)
                .OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            //set out parameter
            OrderRequestId = email.OrderRequestId;

            //then get the email DTO
            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            
            return eOp.mapEmailToDTO<T>(Db, email, true, null, baseUrl);
            
        }


        public VOESystem.Data.DTO.Email getReplyEmail(int EmailId)
        {
         
            //get the email object
            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            return eOp.getEmailForReply(Db, EmailId, false, baseUrl, UserName);


        }

        public int getUnreadEmailCountForUser(string user)
        {

            VOESystem.Data.Business.EmailOps eOp = new Data.Business.EmailOps();
            return eOp.getUnreadEmailCount(Db, user);


        }

    }
}
