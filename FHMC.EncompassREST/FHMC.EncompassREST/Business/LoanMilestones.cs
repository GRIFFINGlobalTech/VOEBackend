using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class LoanMilestones : BaseClass
    {


        public LoanMilestones() : base() { }
        public LoanMilestones(object Log) : base(Log) { }
        public LoanMilestones(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog)
            : base(Log, TrafficFileTag, TrafficDBLog) { }


        public List<LoanMilestone> getLoanMilestones(string loanGuid, string accessToken)
        {
            List<LoanMilestone> retVal = new List<LoanMilestone>() { };

            try
            {

                string methodURL = EncRESTServiceBaseURL() + "loans/" + loanGuid + "/milestones";

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<List<LoanMilestone>>(responseString);
                
            }
            catch (Exception ex)
            {
                Log.Error("Error getting Loan Milestones for " + loanGuid, ex);
                throw ex;
            }

            return retVal;

        }


    }
}

