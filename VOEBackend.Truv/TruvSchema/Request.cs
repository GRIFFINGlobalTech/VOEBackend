using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.Truv.TruvSchema
{
    public enum Product
    {
        [Description("employment")]
        employment,
        [Description("income")]
        income  
    }

    public class Request
    {

        public List<string> products { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string ssn { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string loan_number { get; set; }
        public List<Employer> employers { get; set; }
        public string template_id { get; set; }

        public class Employer
        {
            public string company_name { get; set; }
        }

    }


}
