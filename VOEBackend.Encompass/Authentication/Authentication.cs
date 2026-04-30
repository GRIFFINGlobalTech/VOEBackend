//using EllieMae.Encompass.BusinessObjects.Users;
using FHMC.Interfaces.Encompass;
using System;
//using EllieMae.Encompass.BusinessObjects.Users;

namespace VOEBackend.Encompass
{
    public class Authentication : BaseClass, IVerifyLogin
    {
        //public bool Verify(string UserName, string Password, out string emailAddress, out DateTime nextBusinessDay, out DateTime nextBusiness5thDay, out DateTime prevBusinessDay)
        public bool Verify(string UserName, string Password)
        {
            bool retval = false;
            //emailAddress = string.Empty;
            //nextBusinessDay = DateTime.Today;
            //nextBusiness5thDay = DateTime.Today;
            //prevBusinessDay = DateTime.Today;

            //EllieMae.Encompass.Client.Session emSession;
            //emSession = new EllieMae.Encompass.Client.Session();

            //try
            //{
            //    Log.Info("Connecting to Encompass server " + encompassServer);
            //    emSession.Start(encompassServer, UserName, Password);

            //    //get email address for user
            //    User encUser = emSession.Users.GetUser(UserName);
            //    //emailAddress = encUser.Email;
            //    //nextBusinessDay = EncBusinessDayAdd(DateTime.Today, 1, emSession, null, null);
            //    //nextBusiness5thDay = EncBusinessDayAdd(DateTime.Today, 5, emSession, null, null);
            //    //prevBusinessDay = EncBusinessDayAdd(DateTime.Today, -1, emSession, null, null);

            //    retval = true;
            //}
            //catch (Exception ex)
            //{
            //    Log.Error("Error Authenticating Encompass Login for UserName: " + UserName, ex);

            //}

            //if (emSession != null)
            //{
            //    emSession.End();
            //    emSession = null;
            //}


            return retval;

        }

        
       

        //public bool Verify(string UserName, string Password, out string emailAddress, out DateTime nextBusinessDay, out DateTime nextBusiness5thDay, out DateTime prevBusinessDay)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
