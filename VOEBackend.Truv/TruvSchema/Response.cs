using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.Truv.TruvSchema
{
    public class Response
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string share_url { get; set; }
        public string voie_report_id { get; set; }
        public List<Employer> employers { get; set; }
        public Error error { get; set; }

        
        public class Employer
        {
            public string id { get; set; }
            public string status { get; set; }
            public string pdf_report { get; set; }
            public List<Employment> employments { get; set; }
        }

        public class Employment
        {
            public string id { get; set; }
            public DateTime? external_last_updated { get; set; }
            public List<Statement> statements { get; set; }
            public List<W2> w2s { get; set; }
        }

        public class Statement  //paystub
        {
            public string id { get; set; }
            public string file { get; set; }

        }

        public class W2  
        {
            public int year { get; set; } 
            public string file { get; set; }

        }

        public class Error
        {

            public string code { get; set; }
            public string message { get; set; }

        }
    }

    public class CompanySearchResponse
    {
        public string company_mapping_id { get; set; }
        public string name { get; set; }
        public string success_rate { get; set; }
        public string confidence_Level { get; set; }
    }

}
