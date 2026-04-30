using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.Xactus.Schema
{
    public class RESPONSE_GROUP
    {
        [XmlAttribute]
        public string MISMOVersionID { get; set; }
        public RespondingParty RESPONDING_PARTY { get; set; }
        public Response_ RESPONSE { get; set; }

        public class RespondingParty
        {
            [XmlAttribute]
            public string _Name { get; set; }
        }

        public class Response_
        {
            [XmlAttribute]
            public DateTime ResponseDateTime { get; set; }

            public ResponseData RESPONSE_DATA { get; set; }

            public Status_ STATUS { get; set; }
            
            public class ResponseData
            {

                public CreditResponse CREDIT_RESPONSE { get; set; }

                public Extension EXTENSION { get; set;}

                public EMortgage_Package EMORTGAGE_PACKAGE { get; set; }

                public class CreditResponse {

                    public Borrower BORROWER { get; set; }

                    public class Borrower
                    {

                        [XmlElement]
                        public Employer[]  EMPLOYER { get; set; }

                        public class Employer
                        {
                            [XmlAttribute]
                            public string _Name { get; set; }

                            [XmlAttribute]
                            public string EmployeeStatus { get; set; }
                        }
                    }
                }

                public class Extension
                {

                    public Extension_Section EXTENSION_SECTION { get; set;}

                    public class Extension_Section
                    {
                        public Extension_Section_Data EXTENSION_SECTION_DATA { get; set; }

                        public class Extension_Section_Data
                        {
                            
                            public Verification_Response VERIFICATION_RESPONSE { get; set; }

                            public class Verification_Response
                            {
                                [XmlAttribute]
                                public string Borrower_SSN { get; set; }

                                [XmlAttribute]
                                public string VendorOrderIdentifier { get; set; }

                            }
                        }

                    }

                }

                public class EMortgage_Package
                {

                    [XmlElement]
                    public Embedded_File[] EMBEDDED_FILE { get; set; }

                    public class Embedded_File
                    {

                        [XmlAttribute]
                        public string _Type { get; set; }

                        [XmlAttribute]
                        public string _Name { get; set; }

                        [XmlAttribute]
                        public string _EncodingType { get; set; }

                        public string DOCUMENT { get; set; }

                    }

                }
            }
        
            public class Status_
            {

                [XmlAttribute]
                public string _Condition  { get; set; }

            }
        
        }
    }
}
