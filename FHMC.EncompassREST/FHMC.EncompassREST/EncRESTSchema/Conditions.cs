using FHMC.EncompassREST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{

    public partial class Conditions : BaseClass
    {

        public class Condition
        {

            public string id { get; set; }
            public string priorTo { get; set; }
            public string category { get; set; }
            public Role.RoleEntity ownerRole { get; set; }
            public string conditionType { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public bool forAllApplications { get; set; }
            public bool printExternally { get; set; }
            public bool printInternally { get; set; }
            public string source { get; set; }

        }

        
             
    }

}
