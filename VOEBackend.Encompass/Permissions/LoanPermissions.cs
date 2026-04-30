using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
//using EllieMae.Encompass.BusinessObjects.Loans;
//using EllieMae.Encompass.BusinessObjects.Users;
//using EllieMae.Encompass.Client;
using ServiceStack.OrmLite;
using VOESystem.Data.DBSchema;

namespace VOEBackend.Encompass
{
    public class LoanPermissions : BaseClass
    {
            
        //public void addLoanPermissionsSDK(ref Loan loan, ref Session emSession) 
        //{//used

        //    try 
        //    {

        //        OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
        //            ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
        //            true, SqlServerDialect.Provider);

        //        IDbConnection dbConn = factory.CreateDbConnection();
        //        dbConn.Open();

        //        //check to see if we have already imported permissions for this loan
        //        if (dbConn.Where<UserLoanPermission>("LoanNumber", loan.LoanNumber).Count > 0)
        //        {
        //            //don't need to update
        //            return;
        //        }

        //        //get list of usernames
        //        List<emdbUserInfoView> users = dbConn.Where<emdbUserInfoView>(q =>
        //            q.OrgId != 0
        //            && q.OrgId != 47
        //            && q.IsDisabled == 0);

        //        Log.Info("Starting Permission Set For " + users.Count + " Users");
        //        foreach ( emdbUserInfoView user in users ) {
        //            EllieMae.Encompass.BusinessObjects.Users.User encUser = emSession.Users.GetUser(user.UserName);
        //            LoanAccessRights rights = loan.GetEffectiveAccessRights(encUser);
        //            if ( rights != LoanAccessRights.None ) {
        //                //Log.Info("Adding Permission For " + user.UserName);
        //                //can read VOE System Data, so add to access table
        //                UserLoanPermission perm = new UserLoanPermission
        //                {
        //                    UserName = user.UserName,
        //                    LoanNumber = loan.LoanNumber,
        //                    EncUserPermissionLevel = (int)rights
        //                };
        //                dbConn.Insert<UserLoanPermission>(perm);
        //            }
        //        }
        //        Log.Info("Ending Permission Set For " + users.Count + " Users");

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("Error Adding Loan Permissionfor Loan: " + loan.LoanNumber , ex);
        //    }

            
        //}

        public void addLoanPermissionsREST(string loanNumber)
        {//used

            try
            {

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                //check to see if we have already imported permissions for this loan
                if (dbConn.Where<UserLoanPermission>(q => q.LoanNumber == loanNumber).Count > 0)
                {
                    //don't need to update
                    return;
                }

                //get list of usernames
                List<emdbUserInfoView> users = dbConn.Where<emdbUserInfoView>(q =>
                    q.OrgId != 0
                    && q.OrgId != 47
                    && q.IsDisabled == 0);

                Log.Info("Starting REST Permission Set For " + users.Count + " Users");

                FHMC.EncompassREST.Loan loan = new FHMC.EncompassREST.Loan();
                bool res = loan.checkLoanPermissionForUser(loanNumber, "tester", "Fhmc5355$");

                //foreach (emdbUserInfoView user in users)
                //{

                //    if (rights != LoanAccessRights.None)
                //    {
                //        //Log.Info("Adding Permission For " + user.UserName);
                //        //can read VOE System Data, so add to access table
                //        UserLoanPermission perm = new UserLoanPermission
                //        {
                //            UserName = user.UserName,
                //            LoanNumber = loan.LoanNumber,
                //            EncUserPermissionLevel = (int)rights
                //        };
                //        dbConn.Insert<UserLoanPermission>(perm);
                //    }
                //}
                Log.Info("Ending Permission Set For " + users.Count + " Users");

            }
            catch (Exception ex)
            {
                Log.Error("Error Adding Loan Permissionfor Loan: " + loanNumber, ex);
            }


        }

        public void addMissingLoanPermissions()
        {//used

            try
            {

                OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                    ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                    true, SqlServerDialect.Provider);

                IDbConnection dbConn = factory.CreateDbConnection();
                dbConn.Open();

                //get list of missing loan permissions
                List<MissingLoanPermissionsView> mperms = dbConn.Select<MissingLoanPermissionsView>();

                foreach (MissingLoanPermissionsView mperm in mperms)
                {

                    Log.Info("Adding Missing Permission For " + mperm.UserName + "; " + mperm.LoanNumber);
                    //can read VOE System Data, so add to access table
                    UserLoanPermission perm = new UserLoanPermission
                    {
                        UserName = mperm.UserName,
                        LoanNumber = mperm.LoanNumber
                        
                    };
                    dbConn.Insert<UserLoanPermission>(perm);
                    
                }
                

            }
            catch (Exception ex)
            {
                Log.Error("Error Adding Missing Loan Permissions", ex);
            }


        }
    }
}
