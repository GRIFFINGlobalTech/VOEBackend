using FHMC.Interfaces.emdb;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOEBackend.Email;
using VOEBackend.Interfaces;
using VOESystem.Data.DBSchema;

namespace VOEBackend.Encompass
{
    public class Users : BaseClass, IUserUpdate
    {

        public bool updateUserOOOStatus()
        {

            bool retVal = false;

            OrmLiteConnectionFactory factory = new OrmLiteConnectionFactory(
                ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString(),
                true, SqlServerDialect.Provider);

            try
            {
           

                Log.Info("Begin OOO Retrieval from O365");

                FHMC.Graph.User users = new FHMC.Graph.User(Log);
                List<IEmdbUserInfoView> updatedUsers = users.getOOOStatus();
                Log.Info("End OOO Retrieval from O365");

                Log.Info("Begin Local Database Update");
                using (IDbConnection dbConn = factory.CreateDbConnection())
                {
                    dbConn.Open();

                    foreach (IEmdbUserInfoView user in updatedUsers)
                    {
                        if (user.Email != null)
                        {
                            ExchangeUser exUser = new ExchangeUser()
                            {
                                EmailAddress = user.Email,
                                IsOOO = user.IsOOO,
                                LastUpdateDate = DateTime.Now
                            };

                            int recordsUpdated = dbConn.Update<ExchangeUser>(exUser, r => r.EmailAddress.ToLower() == exUser.EmailAddress.ToLower());

                            if (recordsUpdated == 0)
                            {
                                //need to insert record
                                dbConn.Insert<ExchangeUser>(exUser);

                            }
                        }
                    }
                }

                retVal = true;

                Log.Info("End Local Database Update");


            }
            catch (Exception ex)
            {
                Log.Error("Error Updating OOO", ex);
            }

            Log.Info("End OOO Update Job");

            return retVal;

        }
    }
}
