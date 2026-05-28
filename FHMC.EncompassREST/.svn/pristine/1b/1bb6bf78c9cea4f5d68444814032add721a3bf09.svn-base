using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Organization : BaseClass
    {


        public Organization() : base() { }
        public Organization(object Log) : base(Log) { }
        public Organization(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog) 
            : base(Log, TrafficFileTag, TrafficDBLog) { }

        public Org getOrganization(string orgId, string accessToken)
        {
            Org retVal = null;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "organizations/" + orgId + "?view=Summary";

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<Org>(responseString);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting organization summary for " + orgId, ex);
                throw ex;
            }

            return retVal;

        }

        public List<Org._childOrg> getOrganizationChildren(string orgId, string accessToken)
        {
            List<Org._childOrg> retVal = null;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "organizations/" + orgId + "/children?recursive=true&type=organization";

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<List<Org._childOrg>>(responseString);
            }
            catch (Exception ex)
            {
                Log.Error("Error getting organization children for " + orgId, ex);
                throw ex;
            }

            return retVal;

        }

    }
}
