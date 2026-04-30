using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.Equifax.EquifaxSchema
{
    public class REQUEST_GROUP
    {
        [XmlAttribute]
        public string MISMOVersionID { get; set; }
        public SubmittingParty SUBMITTING_PARTY { get; set; }
        public Request REQUEST { get; set; }


       
        public class SubmittingParty
        {
            [XmlAttribute]
            public string _Name { get; set; }

            public PreferredResponse PREFERRED_RESPONSE { get; set; }

            public class PreferredResponse
            {
                [XmlAttribute]
                public string _Format { get; set; }

            }
           
        }


        public class Request
        {

            [XmlAttribute]
            public string LoginAccountIdentifier { get; set; }
            
            [XmlAttribute]
            public string LoginAccountPassword { get; set; }

            [XmlAttribute]
            public string RequestingPartyBranchIdentifier { get; set; }

            [XmlElement]
            public Key[] KEY { get; set; }

            public RequestData REQUEST_DATA { get; set; }
                 

            public class Key
            {
                [XmlAttribute]
                public string _Name { get; set; }

                [XmlAttribute]
                public string _Value { get; set; }
            }

            public class RequestData
            {
                public VOIRequest VOI_REQUEST { get; set; }

                public class VOIRequest
                {
                    [XmlAttribute]
                    public string LenderCaseIdentifier { get; set; }

                    [XmlAttribute]
                    public string SpecialInstructionsDescription { get; set; }

                    public VOIRequestData VOI_REQUEST_DATA { get; set; }
                    public LoanApplication LOAN_APPLICATION { get; set; }
                    public Extension_ EXTENSION { get; set; }

                    public class VOIRequestData
                    {

                        [XmlAttribute]
                        public string VOIReportRequestActionTypeOtherDescription { get; set; }

                        [XmlAttribute]
                        public string VOIReportTypeOtherDescription { get; set; }

                        [XmlAttribute]
                        public string VOIRequestType { get; set; }

                        [XmlAttribute]
                        public string VOIReportRequestActionType { get; set; }

                        [XmlAttribute]
                        public string VOIReportType { get; set; }

                        [XmlAttribute]
                        public string VOIReportIdentifier { get; set; }

                        [XmlAttribute]
                        public string BorrowerID { get; set; }
                        
                        [XmlAttribute]
                        public string VOIRequestID { get; set; }

                    }

   
                    public class LoanApplication
                    {
                        public Borrower_ BORROWER { get; set; }
                        

                        public class Borrower_
                        {
                            [XmlAttribute]
                            public string BorrowerID { get; set; }

                            [XmlAttribute]
                            public string _FirstName { get; set; }

                            [XmlAttribute]
                            public string _LastName { get; set; }

                            [XmlAttribute]
                            public string _PrintPositionType { get; set; }

                            [XmlAttribute]
                            public string _SSN { get; set; }

                            public Residence _RESIDENCE { get; set; }

                            public Employer_ EMPLOYER { get; set; }

                            public class Residence
                            {

                                [XmlAttribute]
                                public string _StreetAddress { get; set; }

                                [XmlAttribute]
                                public string _City { get; set; }

                                [XmlAttribute]
                                public string _State { get; set; }

                                [XmlAttribute]
                                public string _PostalCode { get; set; }

                                [XmlAttribute]
                                public string BorrowerResidencyType { get; set; }
                            }

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
                                public string EmploymentBorrowerSelfEmployedIndicator { get; set; }

                                [XmlAttribute]
                                public string _PostalCode { get; set; }

                                [XmlAttribute]
                                public string EmploymentPositionDescription { get; set; }

                                [XmlAttribute]
                                public string PreviousEmploymentEndDate { get; set; }

                                [XmlAttribute]
                                public string PreviousEmploymentStartDate { get; set; }

                                [XmlAttribute]
                                public string _TelephoneNumber { get; set; }
                              
                            }
                        }

                    }

                    public class Extension_
                    {
                        public ExtensionSection EXTENSION_SECTION { get; set; }

                        public class ExtensionSection
                        {
                            public ExtensionSectionData EXTENSION_SECTION_DATA { get; set; }

                            public class ExtensionSectionData
                            {
                                public EmbeddedFile EMBEDDED_FILE { get; set; }

                                public class EmbeddedFile
                                {
                                    [XmlAttribute]
                                    public string MIMEType { get; set; }

                                    [XmlAttribute]
                                    public string _Name { get; set; }

                                    [XmlAttribute]
                                    public string _EncodingType { get; set; }

                                    [XmlAttribute]
                                    public string _Type { get; set; }
                                    public string DOCUMENT { get; set; }
                                }
                            }
                        }

                    }



                }


            }

        }

    }

   
    

}
