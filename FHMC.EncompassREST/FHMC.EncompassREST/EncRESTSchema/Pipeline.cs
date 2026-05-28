using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Pipeline
    {

        public class PipelineRequest : FilterRequest
        {
            public List<string> loanIds { get; set; }

        }

        public class PipelineResponse
        {
            //public string loanGuid { get; set; }  V1
            public string loanId { get; set; }
            public Dictionary<string, string> fields { get; set; }

        }

    }
}
