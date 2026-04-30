using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.TrueWork.TrueWorkSchema
{
    public class Response
    {

        public string id { get; set; }
        public string state { get; set; }
        public string date_of_completion { get; set; }
        public string loan_id { get; set; }
        public string cancellation_reason { get; set; }
        public string cancellation_details { get; set; }
        public List<Document> documents { get; set; }
        public List<Report> reports { get; set; }
        public string metadata { get; set; }
        public Request request_parameters { get; set; }

        public class Document
        {
            public string filename { get; set; }
            public string content { get; set; }  //base64endcoded
        }

        public class Report
        {
            public string id { get; set; }
            public string current_as_of { get; set; }
            public Employer employer { get; set; }
            public Employee employee { get; set; }
            public string du_reference_id { get; set; }
        }

        public class Employer
        {
            public string name { get; set; }
        }

        public class Employee
        {
            public string status { get; set; }
        }
       
    }
}
