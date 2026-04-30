using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOEBackend.Encompass
{
    public class EncTestClass : BaseClass
    {
        
        public void testCommit(string loanID, string[] LoanFolders, string FieldName, string theValue,
            string UserName, string Password)
        {


            //try
            //{

            //EllieMae.Encompass.Client.Session emSession;

            //emSession = new EllieMae.Encompass.Client.Session();
            //emSession.Start(encompassServer, UserName, Password);
      
            ////*** Define QUERY Criteria
            //// Build the string criterion
            //StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
            //loanIDCriterion.FieldName = "Fields.364";
            //loanIDCriterion.Value = loanID.Trim();
            //loanIDCriterion.MatchType = StringFieldMatchType.Exact;

            ////add folder criteria
            //QueryCriterion folderCriteria = null;

            //foreach (string loanfolder in LoanFolders)
            //{
            //    StringFieldCriterion folderCriterion = new StringFieldCriterion();
            //    folderCriterion.FieldName = "Loan.LoanFolder";
            //    folderCriterion.Value = loanfolder;
            //    folderCriterion.MatchType = StringFieldMatchType.Exact;

            //    if (folderCriteria == null)
            //    {
            //        folderCriteria = folderCriterion;
            //    }
            //    else
            //    {
            //        folderCriteria = folderCriteria.Or(folderCriterion);
            //    }
            //}

            //// Join the criteria together using AND logic
            //QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

            //// Perform the query, retrieving the identities of the matching loans
            //LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

            ////should only return one loan
            //if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }


            
            //    Loan loan = emSession.Loans.Open(ids[0].Guid, true, true);

            //    //lock loan prior to modification - now obsolete since we can lock on open
            //    //loan.Lock();


            //    loan.Fields[FieldName].Value = theValue;

         
            //    //save changes to the loan and close
            //    loan.Commit();
            //    loan.Close();

            //    //cleanup local session
            //    emSession.End();

            //    Log.Info("Test Class Update Enc Complete: " + loanID);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("Error in Test Class: " + loanID, ex);

            //}

        }

        public void testLoanLoop(string[] LoanFolders, string UserName, string Password)
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

                testCommit(loanId, LoanFolders, "CX.SM.INVLOANNUM", value, UserName, Password);

            }


        }

        public void testgetLoanInfoREST(string UserName, string Password)
        {

            List<string> loanNums = new List<string>() { };

            loanNums.Add("1081469605");
            //loanNums.Add("1008446520");

            VOEBackend.Encompass.Loans loan = new Loans();

            foreach (string loanNumber in loanNums)
            {
                Console.WriteLine(loanNumber);
                List<VOESystem.Data.DTO.LoanInfoResp> los = loan.getLoanInfoREST(loanNumber, UserName, Password);

                foreach (VOESystem.Data.DTO.LoanInfoResp lo in los)
                {
                    
                    Log.Info(lo.ToJson());
                }

            }

            


        }

        public void testgetLoanInfoSDK()
        {

            List<string> loanNums = new List<string>() { };

            //loanNums.Add("1006283100");
            //loanNums.Add("296957");
            loanNums.Add("1044287415");

            //loanNums.Add("1006286613");
            //loanNums.Add("1006287464");
            //loanNums.Add("1008276317");
            //loanNums.Add("1008279450");
            //loanNums.Add("1008281715");
            //loanNums.Add("1008286306");
            //loanNums.Add("1009281177");
            //loanNums.Add("1009284309");
            //loanNums.Add("1009286825");
            //loanNums.Add("1009287640");
            //loanNums.Add("1010252262");
            //loanNums.Add("1010267520");
            //loanNums.Add("1010275730");
            //loanNums.Add("1010277096");
            //loanNums.Add("1010278761");
            //loanNums.Add("1010283507");
            //loanNums.Add("1010284195");
            //loanNums.Add("1010284856");
            //loanNums.Add("1010286389");
            //loanNums.Add("1010286565");
            //loanNums.Add("1010287300");
            //loanNums.Add("1010287332");
            //loanNums.Add("1010287387");
            //loanNums.Add("1010287508");
            //loanNums.Add("1010287576");
            //loanNums.Add("1012287112");
            //loanNums.Add("1014270607");
            //loanNums.Add("1014276453");
            //loanNums.Add("1014281484");
            //loanNums.Add("1014283935");
            //loanNums.Add("1014284062");
            //loanNums.Add("1014284087");
            //loanNums.Add("1014286268");
            //loanNums.Add("1014286367");
            //loanNums.Add("1014286433");
            //loanNums.Add("1014286477");
            //loanNums.Add("1014286701");
            //loanNums.Add("1014287001");
            //loanNums.Add("1014287044");
            //loanNums.Add("1015266817");
            //loanNums.Add("1017277794");
            //loanNums.Add("1017282359");
            //loanNums.Add("1017283198");
            //loanNums.Add("1017283782");
            //loanNums.Add("1017284797");
            //loanNums.Add("1017287462");
            //loanNums.Add("1022217939");
            //loanNums.Add("1022279243");
            //loanNums.Add("1022287341");
            //loanNums.Add("1022287414");
            //loanNums.Add("1022288129");
            //loanNums.Add("1024277698");
            //loanNums.Add("1024281711");
            //loanNums.Add("1024282114");
            //loanNums.Add("1024285413");
            //loanNums.Add("1024286738");
            //loanNums.Add("1030272654");
            //loanNums.Add("1030284464");
            //loanNums.Add("1030287374");
            //loanNums.Add("1030287497");
            //loanNums.Add("1031238857");
            //loanNums.Add("1034262628");
            //loanNums.Add("1043228008");
            //loanNums.Add("1043266874");
            //loanNums.Add("1043272331");
            //loanNums.Add("1043275473");
            //loanNums.Add("1043279678");
            //loanNums.Add("1043280432");
            //loanNums.Add("1043281730");
            //loanNums.Add("1043285186");
            //loanNums.Add("1043285894");
            //loanNums.Add("1043286981");
            //loanNums.Add("1044280599");
            //loanNums.Add("1044286656");
            //loanNums.Add("1044287600");
            //loanNums.Add("1044288409");
            //loanNums.Add("1050259108");
            //loanNums.Add("1050272097");
            //loanNums.Add("1050284358");
            //loanNums.Add("1050286041");
            //loanNums.Add("1071284921");
            //loanNums.Add("1071285088");
            //loanNums.Add("1071285417");
            //loanNums.Add("1075284658");
            //loanNums.Add("1076287865");
            //loanNums.Add("1081233980");
            //loanNums.Add("1081241883");
            //loanNums.Add("1081263086");
            //loanNums.Add("1081263547");
            //loanNums.Add("1081274002");
            //loanNums.Add("1081284103");
            //loanNums.Add("1081285283");
            //loanNums.Add("1081285311");
            //loanNums.Add("1081286139");
            //loanNums.Add("1083275963");
            //loanNums.Add("1083277033");
            //loanNums.Add("1083278520");
            //loanNums.Add("1083279035");
            //loanNums.Add("1083280348");
            //loanNums.Add("1083280387");
            //loanNums.Add("1083282422");
            //loanNums.Add("1083285558");
            //loanNums.Add("1083285659");
            //loanNums.Add("1201284815");
            //loanNums.Add("1201287290");
            //loanNums.Add("4010263878");
            //loanNums.Add("4015286842");
            //loanNums.Add("4022268971");
            //loanNums.Add("4022274595");
            //loanNums.Add("4022288008");
            //loanNums.Add("4026280767");
            //loanNums.Add("5012238043");
            //loanNums.Add("5012273471");
            //loanNums.Add("7010244444");
            //loanNums.Add("7010279373");
            //loanNums.Add("7010283970");
            //loanNums.Add("7010285657");
            //loanNums.Add("7010286440");
            //loanNums.Add("7030284058");
            //loanNums.Add("7040222127");
            //loanNums.Add("7040263060");
            //loanNums.Add("7040277752");
            //loanNums.Add("7040281575");
            //loanNums.Add("7040282832");
            //loanNums.Add("7040283139");
            //loanNums.Add("7040286029");
            //loanNums.Add("7040287230");
            //loanNums.Add("7040288531");

            VOEBackend.Encompass.Loans loan = new Loans();
            string[] LoanFolders = new string[] { "Active Loans", "Training", "Prospects", "Adverse Loans" };

            foreach (string loanNumber in loanNums)
            {

                
                Console.WriteLine(loanNumber);           
                List<VOESystem.Data.DTO.LoanInfoResp> los = loan.getLoanInfoSDK(loanNumber, "sdkadmin1", "Updatefield1!", LoanFolders, null);

                foreach (VOESystem.Data.DTO.LoanInfoResp lo in los)
                {
                    Log.Info(lo.ToJson());
                }

            }




        }

        public void testGetEmployerData()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

            using (IDbConnection dbConn = factory.CreateDbConnection())
            {
                dbConn.Open();

                LoanInfoOps lOp = new LoanInfoOps();
                List<EncVOEEmploymentApproval> encVOEData = lOp.getVOEEmploymentData(dbConn, "1004377949");
            }

        }

        public void testOrderOpsOperation()
        {

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

            IDbConnection dbConn = factory.CreateDbConnection();
            dbConn.Open();

            OrderRequest order = dbConn.Where<OrderRequest>(q => q.LoanNumber == "").FirstOrDefault();


        }
    }
}
