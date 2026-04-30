using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.Equifax.EquifaxSchema
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

            [XmlElement]
            public Key[] KEY { get; set; }
            public ResponseData RESPONSE_DATA { get; set; }

            public Status_ STATUS { get; set; }

            public class Key
            {
                [XmlAttribute]
                public string _Name { get; set; }

                [XmlAttribute]
                public string _Value { get; set; }
            }

            public class ResponseData
            {
                public VOIResponse VOI_RESPONSE { get; set;}

                public class VOIResponse
                {
                    [XmlAttribute]
                    public string VOIResponseID { get; set;}

                    [XmlAttribute]
                    public string MISMOVersionID { get; set;}

                    [XmlAttribute]
                    public string VOIReportIdentifier { get; set;}

                    [XmlAttribute]
                    public string VOIReportType { get; set;}

                    [XmlAttribute]
                    public string VOIReportTypeOtherDescription { get; set; }
                    public Borrower_ BORROWER { get; set; }

                    [XmlElement]
                    public EmbeddedFile[] EMBEDDED_FILE { get; set; }
                    
                    public class Borrower_
                    {
                        [XmlAttribute]
                        public string BorrowerID  { get; set; }

                        [XmlAttribute]
                        public string _FirstName { get; set; }

                        [XmlAttribute]
                        public string _LastName { get; set; }

                        [XmlAttribute]
                        public string _SSN { get; set; }
                        public Employer_ EMPLOYER { get; set; }

                        public class Employer_
                        {
                            [XmlAttribute]
                            public string _Name { get; set; }

                            [XmlAttribute]
                            public string _StreetAddress { get; set; }

                            [XmlAttribute]
                            public string _City { get; set; }

                            [XmlAttribute]
                            public string _State { get; set; }

                            [XmlAttribute]
                            public string _PostalCode { get; set; }

                            [XmlAttribute]
                            public string CurrentEmploymentMonthsOnJob { get; set; }

                            [XmlAttribute]
                            public string CurrentEmploymentStartDate { get; set; }

                            [XmlAttribute]
                            public string EmploymentPositionDescription { get; set; }
                        }
                    }

                    public class EmbeddedFile
                    {
                        [XmlAttribute]
                        public string _Type { get; set; }

                        [XmlAttribute]
                        public string _Name { get; set; }

                        [XmlAttribute]
                        public string _Extensions { get; set; }

                        [XmlAttribute]
                        public string _EncodingType { get; set; }
                        public string DOCUMENT { get; set; }

                    }

                }

            }
        
            public class Status_
            {
                [XmlAttribute]
                public string _Name { get; set; }

                [XmlAttribute]
                public string _StatusDate { get; set; }

                [XmlAttribute]
                public string _Description { get; set; }

                [XmlAttribute]
                public string _Condition  { get; set; }

                [XmlAttribute]
                public string _Code { get; set; }
            }
        
        }
    }
}
