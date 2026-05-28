using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;

namespace FHMC.EncompassREST
{
    public partial class Company : BaseClass
    {
        public class User
        {
            public string id { get; set; }
            public string lastName { get; set; }
            public string firstName { get; set; }
            public string fullName { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string fax { get; set; }
            public _Organization organization { get; set; }
            public List<_Persona> personas { get; set; }

            public class _Organization
            {
                public string entityId { get; set; }
                public string entityType { get; set; }
                public string entityName { get; set; }
                public string entityUri { get; set; }
            }

            public class _Persona
            {
                public string entityId { get; set; }
                public string entityType { get; set; }
                public string entityName { get; set; }
                public string entityUri { get; set; }
            }
        }




    }
}
