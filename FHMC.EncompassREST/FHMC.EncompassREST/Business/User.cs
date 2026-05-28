using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class User : BaseClass
    {

        public User() : base() { }
        public User(object Log) : base(Log) { }
        public User(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog)
            : base(Log, TrafficFileTag, TrafficDBLog) { }

       
        public EncompassREST.User.InternalUser getUser(string userId, string accessToken)
        {

            EncompassREST.User.InternalUser retVal = null;

            try
            {

                string methodURL = EncRESTServiceBaseURL(3) + "users/" + userId;

                //get list of roles
                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<EncompassREST.User.InternalUser>(responseString);

            }
            catch (Exception ex)
            {
                Log.Error("Error Retrieving User Information for " + userId, ex);
                throw ex;

            }


            return retVal;



        }

        public bool unlockUserAndResetPassword(string userId, string password, string accessToken)
        {
            bool retVal = false;

            try
            {

                string methodURL = EncRESTServiceBaseURL(3) + "users/" + userId;

                EncompassREST.User.InternalUser user = new InternalUser();

                user.password = password;
                user.loginEnabled = true;
                user.requirePasswordChange = true;

                string requestString = JsonConvert.SerializeObject(user, Newtonsoft.Json.Formatting.None,
                           new JsonSerializerSettings
                           {
                               NullValueHandling = NullValueHandling.Ignore
                           });

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.PATCH, requestString, accessToken);
                retVal = true;

            }
            catch (Exception ex)
            {
                Log.Error("Error Resetting Password for User " + userId, ex);

            }

            return retVal;

        }


    }
}
