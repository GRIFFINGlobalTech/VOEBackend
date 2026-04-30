using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
//using EllieMae.Encompass.BusinessObjects.Loans;
//using EllieMae.Encompass.Collections;
//using EllieMae.Encompass.Query;
using ServiceStack.OrmLite;
using VOESystem.Data.DBSchema;
using FHMC.EncompassREST;

namespace VOEBackend.Encompass
{
    public partial class Loans : BaseClass
    {

        //unused
     //   public void UpdateFieldsForLoan(string loanID, Dictionary<string, string> fields, string UserName, string Password,
     //string[] LoanFolders, object encompasssession, bool bUpdateVOEDocs, bool bAddVOEPermissions)
     //   {
     //       EllieMae.Encompass.Client.Session emSession;

     //       if (encompasssession == null)
     //       {
     //           emSession = new EllieMae.Encompass.Client.Session();
     //           emSession.Start(encompassServer, UserName, Password);
     //       }
     //       else
     //       {
     //           emSession = (EllieMae.Encompass.Client.Session)encompasssession;
     //       }



     //       //*** Define QUERY Criteria
     //       // Build the string criterion
     //       StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
     //       loanIDCriterion.FieldName = "Fields.364";
     //       loanIDCriterion.Value = loanID.Trim();
     //       loanIDCriterion.MatchType = StringFieldMatchType.Exact;

     //       //add folder criteria
     //       QueryCriterion folderCriteria = null;

     //       foreach (string loanfolder in LoanFolders)
     //       {
     //           StringFieldCriterion folderCriterion = new StringFieldCriterion();
     //           folderCriterion.FieldName = "Loan.LoanFolder";
     //           folderCriterion.Value = loanfolder;
     //           folderCriterion.MatchType = StringFieldMatchType.Exact;

     //           if (folderCriteria == null)
     //           {
     //               folderCriteria = folderCriterion;
     //           }
     //           else
     //           {
     //               folderCriteria = folderCriteria.Or(folderCriterion);
     //           }
     //       }

     //       // Join the criteria together using AND logic
     //       QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

     //       // Perform the query, retrieving the identities of the matching loans
     //       LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

     //       //should only return one loan
     //       if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }

     //       EllieMae.Encompass.BusinessObjects.Loans.Loan loan = emSession.Loans.Open(ids[0].Guid, true, true);

     //       //lock loan prior to modification - now obsolete since we can lock on open
     //       //loan.Lock();

     //       //loop through dictionary and modify values
     //       foreach (KeyValuePair<string, string> field in fields)
     //       {
     //           string key = field.Key;
     //           string value = field.Value;

     //           loan.Fields[key].Value = value;

     //       }

     //       //save changes to the loan and close
     //       loan.Commit();
     //       loan.Close();

     //       //cleanup local session

     //       if (encompasssession == null)
     //       {
     //           emSession.End();
     //       }

     //   }

        //unused
        
     //   public void addPipelineAlert(string loanID, string alertText, string UserName, string Password,
     //string[] LoanFolders, object encompasssession)
     //   {

     //       EllieMae.Encompass.Client.Session emSession;

     //       if (encompasssession == null)
     //       {
     //           emSession = new EllieMae.Encompass.Client.Session();
     //           emSession.Start(encompassServer, UserName, Password);
     //       }
     //       else
     //       {
     //           emSession = (EllieMae.Encompass.Client.Session)encompasssession;
     //       }



     //       //*** Define QUERY Criteria
     //       // Build the string criterion
     //       StringFieldCriterion loanIDCriterion = new StringFieldCriterion();
     //       loanIDCriterion.FieldName = "Fields.364";
     //       loanIDCriterion.Value = loanID.Trim();
     //       loanIDCriterion.MatchType = StringFieldMatchType.Exact;

     //       //add folder criteria
     //       QueryCriterion folderCriteria = null;

     //       foreach (string loanfolder in LoanFolders)
     //       {
     //           StringFieldCriterion folderCriterion = new StringFieldCriterion();
     //           folderCriterion.FieldName = "Loan.LoanFolder";
     //           folderCriterion.Value = loanfolder;
     //           folderCriterion.MatchType = StringFieldMatchType.Exact;

     //           if (folderCriteria == null)
     //           {
     //               folderCriteria = folderCriterion;
     //           }
     //           else
     //           {
     //               folderCriteria = folderCriteria.Or(folderCriterion);
     //           }
     //       }

     //       // Join the criteria together using AND logic
     //       QueryCriterion jointCriteria = folderCriteria.And(loanIDCriterion);

     //       // Perform the query, retrieving the identities of the matching loans
     //       LoanIdentityList ids = emSession.Loans.Query(jointCriteria);

     //       //should only return one loan
     //       if (ids.Count != 1) { throw new Exception("Error Finding Loan " + loanID); }

     //       EllieMae.Encompass.BusinessObjects.Loans.Loan loan = emSession.Loans.Open(ids[0].Guid, true, true);




     //       ////save changes to the loan and close
     //       //loan.Commit();
     //       //loan.Close();

     //       //cleanup local session

     //       if (encompasssession == null)
     //       {
     //           emSession.End();
     //       }


     //   }

        
        
        public void processEncompassUpdates()
        { //used
            OrmLiteConfig.CommandTimeout = 60;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            FHMC.NLogWrapper.Logger updateLogger = new FHMC.NLogWrapper.Logger("UpdateLogger");

            try
            {
                using (IDbConnection dbConn = factory.CreateDbConnection())
                {
                    dbConn.Open();

                    //order by so changes are processed in chronological order
                    List<VOESystem.Data.DBSchema.LoanUpdateView> updateList = dbConn.Select<VOESystem.Data.DBSchema.LoanUpdateView>()
                        .OrderBy(r => r.LoanUpdateId).ToList();

                    string accessToken = String.Empty;
                    FHMC.EncompassREST.Loan loan = null;

                    if (updateList.Count > 0)
                    {
                        FHMC.EncompassREST.Authentication auth = new FHMC.EncompassREST.Authentication(updateLogger);
                        accessToken = auth.getAccessToken();

                        loan = new FHMC.EncompassREST.Loan(updateLogger);
                    }

                    foreach (VOESystem.Data.DBSchema.LoanUpdateView update in updateList)
                    {

                        try
                        {
                            if (loan.updateCustomField(update.LoanNumber, update.EncFieldId, update.NewFieldValue, update.IsAppend, accessToken))
                            {

                                //update datetime
                                string SQLUpdate = String.Format("UPDATE LoanUpdate "
                                               + "SET UpdateDateTime = GETDATE() "
                                               + "WHERE Id = {0} ",
                                               update.LoanUpdateId.ToString());

                                dbConn.ExecuteSql(SQLUpdate);

                            }
                        }

                        catch (Exception ex)
                        {
                            //cut down on loggin here
                            if (!ex.Message.ToLower().Contains("loan is locked"))
                            {
                                updateLogger.Error("Error Updating Loan for LoanUpdateId = " + update.LoanUpdateId.ToString(), ex);
                            }


                        }

                    }


                }
            }
            catch (Exception ex)
            {
                updateLogger.Error("Error Updating Loans: ", ex);
            }


        }


    }
    

}
