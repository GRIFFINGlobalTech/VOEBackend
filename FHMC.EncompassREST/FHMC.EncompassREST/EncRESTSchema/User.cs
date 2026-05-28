using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class User : BaseClass
    {

        public class InternalUser
        {
            public string id { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public bool? loginEnabled { get; set; }
            public bool? accountEnabled { get; set; }
            public string email { get; set; }
            public string password { get; set; }
            public bool? requirePasswordChange { get; set; }
        }
    }
}
