using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Organization : BaseClass
    {

        public class Org
        {
            public string id { get; set; }
            public string name { get; set; }
            public int numberOfChildOrganizations { get; set; }
            public int numberOfChildUsers { get; set; }
            public _orgInformation orgInformation { get; set; }
            public _parentOrg parentOrg { get; set; }
            public List<_childOrg> children { get; set; }

            public class _orgInformation
            {
                public string orgCode { get; set; }
            }

            public class _parentOrg
            {
                public string entityId { get; set; }
                public string entityType { get; set; }
                public string entityName { get; set; }
                public string entityUri { get; set; }
            }

            public class _childOrg
            {
                public string entityId { get; set; }
                public string entityType { get; set; }
                public string entityName { get; set; }
                public string entityUri { get; set; }
                public string orgId { get; set; }
            }

        }

    }
}
