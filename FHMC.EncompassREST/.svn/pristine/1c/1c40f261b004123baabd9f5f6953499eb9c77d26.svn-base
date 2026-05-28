using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Role : BaseClass
    {

        public Role() : base() { }
        public Role(object Log) : base(Log) { }
        public Role(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog)
            : base(Log, TrafficFileTag, TrafficDBLog) { }

        #region Enums

        public enum enuRoleType
        {

            [System.ComponentModel.Description("203K")]
            _203K,
            [System.ComponentModel.Description("Accounting")]
            Accounting,
            [System.ComponentModel.Description("Branch Admin")]
            BranchAdmin,
            [System.ComponentModel.Description("Branch Manager")]
            BranchManager,
            [System.ComponentModel.Description("Candor")]
            Candor,
            [System.ComponentModel.Description("Closer")]
            Closer,
            [System.ComponentModel.Description("Closing Coordinator")]
            ClosingCoordinator,
            [System.ComponentModel.Description("Compliance")]
            Compliance,
            [System.ComponentModel.Description("Corporate Admin")]
            CorporateAdmin,
            [System.ComponentModel.Description("Department Manager")]
            DepartmentManager,
            [System.ComponentModel.Description("Final Docs")]
            FinalDocs,
            [System.ComponentModel.Description("Funder")]
            Funder,
            [System.ComponentModel.Description("Imager")]
            Imager,
            [System.ComponentModel.Description("LO Assistant")]
            LOAssistant,
            [System.ComponentModel.Description("Lender")]
            Lender,
            [System.ComponentModel.Description("Loan Officer")]
            LoanOfficer,
            [System.ComponentModel.Description("Loan Processor")]
            LoanProcessor,
            [System.ComponentModel.Description("Lock Desk")]
            LockDesk,
            [System.ComponentModel.Description("Note Shipper")]
            NoteShipper,
            [System.ComponentModel.Description("Office Manager")]
            OfficeManager,
            [System.ComponentModel.Description("Post Closer")]
            PostCloser,
            [System.ComponentModel.Description("PreFunding Auditor")]
            PreFundingAuditor,
            [System.ComponentModel.Description("Project Reviewer")]
            ProjectReviewer,
            [System.ComponentModel.Description("Protected Documents")]
            ProtectedDocuments,
            [System.ComponentModel.Description("Quality Control")]
            QualityControl,
            [System.ComponentModel.Description("SAR Underwriter")]
            SARUnderwriter,
            [System.ComponentModel.Description("Secondary Marketing")]
            SecondaryMarketing,
            [System.ComponentModel.Description("Underwriter")]
            Underwriter

        }


        #endregion Enums

        public EncompassREST.Role.RoleEntity getRole(enuRoleType role, string accessToken)
        {
            EncompassREST.Role.RoleEntity retVal = new EncompassREST.Role.RoleEntity();

            string methodURL = EncRESTServiceBaseURL() + "settings/roles";

            //get list of roles
            string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

            List<EncompassREST.Role.RoleResponse> roles = JsonConvert.DeserializeObject<List<EncompassREST.Role.RoleResponse>>(responseString);

            EncompassREST.Role.RoleResponse ro = roles.Where(q => q.roleName == role.GetDescription()).FirstOrDefault();

            retVal.entityId = ro.roleID;
            retVal.entityName = ro.roleName;
            retVal.entityType = "Role";

            return retVal;

        }

        public List<EncompassREST.Role.RoleEntity> getRoles(List<enuRoleType> roles, string accessToken)
        {
            List<EncompassREST.Role.RoleEntity> retVal = new List<RoleEntity>() { };

            string methodURL = EncRESTServiceBaseURL() + "settings/roles";

            //get list of roles
            string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

            List<EncompassREST.Role.RoleResponse> roleResp = JsonConvert.DeserializeObject<List<EncompassREST.Role.RoleResponse>>(responseString);

            foreach (enuRoleType role in roles)
            {
                EncompassREST.Role.RoleResponse roResp = roleResp.Where(q => q.roleName == role.GetDescription()).FirstOrDefault();
                if (roResp != null)
                {
                    retVal.Add(new RoleEntity {
                        entityId = roResp.roleID,
                        entityName = roResp.roleName,
                        entityType = "Role"
                    });
                }
            }
            
            return retVal;

        }

        public enuRoleType? getRoleEnum(string RoleName)
        {

            enuRoleType? retVal = null;

            enuRoleType aRole;
            if (Enum.TryParse(RoleName.Replace(" ", ""), out aRole) || Enum.TryParse("_" + RoleName.Replace(" ", ""), out aRole)) { 
                retVal = aRole;
            }
            
            return retVal;

        }
    }
}
