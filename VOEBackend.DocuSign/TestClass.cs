using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ServiceStack.OrmLite;
using ServiceStack.Text;


namespace VOEBackend.DocuSign
{
    public class TestClass : BaseClass
    {


        public void testOperation()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DevConnectionString"].ToString();
            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            dbConn.Open();

            List<int> docIds = new List<int> { 1452060 };
            int orderrequestid = 191512;

            //List<OrderOps.Recipient.SignatureDoc> sigdocs = new List<OrderOps.Recipient.SignatureDoc>
            //{
            //    new OrderOps.Recipient.SignatureDoc
            //    {
            //        DocumentId = 751183,
            //        XPos = 340,
            //        YPos = 450
            //    }

            //};

            //List<OrderOps.Recipient> recips = new List<OrderOps.Recipient>
            //{
            //    new OrderOps.Recipient
            //    {
            //        Email = "cdesimone@clpconsultingllc.com",
            //        IsEmbedded = false,
            //        Name = "Christine CLP",
            //        OrderIndex = 1,
            //        RecipientId = 3,
            //        RecipientType = OrderOps.RecipientType.Signer, 
            //        SignatureDocs = sigdocs
            //    }
            //};

            List<OrderOps.Recipient> recips = getRecips();

            List<string> InactiveFieldGroups = new List<string>() { };
            InactiveFieldGroups.Add("Select Employment Type");

            VOEBackend.DocuSign.OrderOps dp = new VOEBackend.DocuSign.OrderOps();
            dp.requestSignatureOnDocument(dbConn, orderrequestid, recips, docIds, "voesystem.com", "Please Create a Furlough Letter", "Please do this.  Thanks.", InactiveFieldGroups);

        }

        public void testLoanLoop()
        {
            Dictionary<string, string> dic = new Dictionary<string, string> { };

            dic.Add("1015156484", "9902828960");
            dic.Add("1014163697", "9902829001");
            dic.Add("1080155325", "9902828972");
            dic.Add("4020120788", "9902828942");
            dic.Add("1010139379", "9902828962");
            dic.Add("1009142799", "9902829006");
            dic.Add("1012163922", "9902828961");
            dic.Add("1014163502", "9902828943");
            dic.Add("1014167319", "9902828947");
            dic.Add("1075156408", "9902828941");
            dic.Add("1017166979", "9902828998");
            dic.Add("1080162021", "9902828973");
            dic.Add("1037159629", "9902829007");
            dic.Add("1017157646", "9902828946");
            dic.Add("1050160083", "9902828963");
            dic.Add("1009162619", "9902828988");
            dic.Add("1080166905", "9902828949");
            dic.Add("1006165197", "9902828989");
            dic.Add("1023155009", "9902828983");
            dic.Add("1080164111", "9902828945");
            dic.Add("1014166893", "9902828944");
            dic.Add("1031041830", "9902828964");
            dic.Add("1043147263", "9902828987");
            dic.Add("1043164368", "9902828948");

            foreach (KeyValuePair<string, string> loanItem in dic)
            {
                string loanId = loanItem.Key;
                string value = loanItem.Value;

                //testCommit(loanId, LoanFolders, "CX.SM.INVLOANNUM", value, UserName, Password);

            }


        }

        public List<OrderOps.Recipient> getRecips()
        {

            List<OrderOps.Recipient> recips = null;

            string recipString = "[{\"RecipientId\":2,\"Name\":\"Chemn Gnr\",\"Email\":\"chemengnr@yahoo.com\",\"IsEmbedded\":false,\"OrderIndex\":3,\"RecipientType\":\"Signer\",\"SignatureDocs\":[{\"DocumentId\":1452060,\"XPos\":36,\"YPos\":671,\"DateXPos\":502,\"DateYPos\":696}]},{\"RecipientId\":1,\"Name\":\"Christine DeSimone\",\"Email\":\"cdesimone@firsthome.com\",\"IsEmbedded\":true,\"OrderIndex\":1,\"RecipientType\":\"EmbeddedSigner\",\"SignatureDocs\":[{\"DocumentId\":1452060,\"XPos\":36,\"YPos\":203,\"DateXPos\":0,\"DateYPos\":0}]}]";

            recips = JsonConvert.DeserializeObject<List<OrderOps.Recipient>>(recipString);

            return recips;

        }


    }
}
