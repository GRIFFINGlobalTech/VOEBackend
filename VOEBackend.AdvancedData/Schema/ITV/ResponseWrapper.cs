using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.AdvancedData.Schema.ITV
{
    public class ResponseWrapper
    {

        public ResponseOrder Order { get; set; }
        public ResponseStatus Status { get; set; }
        public ResponseStatus SSNStatus { get; set; }
          
    }

    public class ResponseOrder
    {
        public string ThirdPartyOrderID { get; set; }
        public string VOEOrderID { get; set; }
        public string VoEOrderID { get; set; }
        public string SSNOrderID { get; set; }
        public string VoEOrderType { get; set; }
    }

    public class ResponseStatus
    {
        public string Code { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public string Document { get; set; }
    }

    //this is not used
    public class ResponseDocument
    {
        public string FileType { get; set; }
        public string Encoding { get; set; }
        public string Content { get; set; }
        public string DocType { get; set; }
    }

   
}
