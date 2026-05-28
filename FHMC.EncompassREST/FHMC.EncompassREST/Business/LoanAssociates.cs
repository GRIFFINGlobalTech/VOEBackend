using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class LoanAssociates : BaseClass
    {
        int apiVersion = 3;

        public LoanAssociates() : base() { }
        public LoanAssociates(object Log) : base(Log) { }
        public LoanAssociates(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog)
            : base(Log, TrafficFileTag, TrafficDBLog) { }
    
        public List<LoanAssociate> getLoanAssociates(string loanGuid, string accessToken, List<Role.enuRoleType> inRoles = null)
        {
            List<LoanAssociate> retVal = new List<LoanAssociate>() { };

            try
            {

                string methodURL = EncRESTServiceBaseURL() + "loans/" + loanGuid + "/associates";

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                List<LoanAssociate> assocs = JsonConvert.DeserializeObject<List<LoanAssociate>>(responseString);

                if (inRoles == null)
                {
                    retVal = assocs;
                }
                else
                {
                    Role rop = new Role();
                    foreach (LoanAssociate assoc in assocs)
                    {
                        Role.enuRoleType? aRole = rop.getRoleEnum(assoc.roleName);
                        if (aRole != null)
                        {
                            if (inRoles.Contains(aRole ?? Role.enuRoleType.Accounting))  //just to avoid nullable type
                            {
                                retVal.Add(assoc);
                            }
                        }
                        
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error getting Loan Associates for " + loanGuid, ex);
                throw ex;
            }


            return retVal;

        }

        public bool assignLoanAssociate(string loanGuid, string accessToken, string encompassUserId, string milestoneLogId)
        {
            bool retVal = false;

            try
            {

                string methodURL = EncRESTServiceBaseURL() + "loans/" + loanGuid + "/associates/" + milestoneLogId;

                LoanAssociates.AssignmentRequest request = new AssignmentRequest();
                request.id = encompassUserId;
                request.loanAssociateType = "User";

                string requestString = JsonConvert.SerializeObject(request);

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.PUT, requestString, accessToken);

                if (responseString == "")
                {
                    retVal = true;
                }
                else
                {
                    throw new Exception(responseString);
                }

                
            }
            catch (Exception ex)
            {
                Log.Error("Error assigning Loan Associates for " + loanGuid, ex);
                throw ex;
            }
            
            return retVal;

        }



    }

}
