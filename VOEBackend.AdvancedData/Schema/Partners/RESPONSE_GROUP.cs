using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.AdvancedData.Schema.Partners
{
    public class RESPONSE_GROUP
    {
        
        public Response_ RESPONSE { get; set; }

        public class Response_
        {
            [XmlAttribute]
            public string ResponseDateTime { get; set; }

            public ResponseData RESPONSE_DATA { get; set; }

            public class ResponseData
            {

                public Extension EXTENSION { get; set; }

                public Status_ STATUS { get; set; }

                public EMortgage_Package EMORTGAGE_PACKAGE { get; set; }

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
                                public string BorrowerID { get; set; }

                                [XmlAttribute]
                                public string VendorOrderIdentifier { get; set; }

                                [XmlAttribute]
                                public string LenderCaseIdentifier { get; set; }

                                public Employer_ EMPLOYER { get; set; }

                                public class Employer_
                                {
                                    [XmlAttribute]
                                    public string VoeType { get; set; }

                                    [XmlAttribute]
                                    public string _Name { get; set; }

                                    public EmployersInfo_ EMPLOYERS_INFO { get;set;}

                                    public class EmployersInfo_
                                    {
                                        public EmployerInfo_ EMPLOYER_INFO { get; set; }

                                        public class EmployerInfo_
                                        {

                                            [XmlAttribute]
                                            public string _Name { get; set; }

                                            [XmlAttribute]
                                            public string StreetAddress { get; set; }

                                            [XmlAttribute]
                                            public string _City { get; set; }

                                            [XmlAttribute]
                                            public string _State { get; set; }

                                            [XmlAttribute]
                                            public string _PostalCode { get; set; }

                                            [XmlAttribute]
                                            public string EmploymentCurrentIndicatorCurrent { get; set; }

                                            [XmlAttribute]
                                            public string EmployerCode { get; set; }

                                        }
                                        

                                    }

                                }

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

                [XmlAttribute]
                public string _Description { get; set; }

            }
        
        }
    }
}
