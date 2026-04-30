using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.AdvancedData.Schema.Partners
{
    public class REQUEST_GROUP
    {
        [XmlAttribute]
        public string MISMOVersionID { get; set; }
        public Request REQUEST { get; set; }

        public class Request
        {

            [XmlAttribute]
            public string LoginAccountIdentifier { get; set; }
            
            [XmlAttribute]
            public string LoginAccountPassword { get; set; }

            public RequestData REQUEST_DATA { get; set; }

            public class RequestData
            {

                public Extension EXTENSION { get; set; }

                public class Extension
                {

                    public ExtensionSection EXTENSION_SECTION { get; set; }

                    public class ExtensionSection
                    {
                        public ExtensionSectionData EXTENSION_SECTION_DATA { get; set; }

                        public class ExtensionSectionData
                        {

                            public VerificationRequest VERIFICATION_REQUEST { get; set; }

                            public class VerificationRequest
                            {

                                [XmlAttribute]
                                public string VendorOrderIdentifier { get; set; }

                                [XmlAttribute]
                                public string _ActionType { get; set; }

                                [XmlAttribute]
                                public string _ItemType { get; set; }

                                [XmlAttribute]
                                public string _CreditReportTypeOtherDescription { get; set; }

                                public Product _PRODUCT { get; set; }


                                public EmbeddedFile EMBEDDED_FILE { get; set; }

                                public class EmbeddedFile
                                {
                                    [XmlAttribute]
                                    public string _Type { get; set; }

                                    [XmlAttribute]
                                    public string _Name { get; set; }

                                    [XmlAttribute]
                                    public string _EncodingType { get; set; }

                                    [XmlAttribute]
                                    public string _Extension { get; set; }

                                    public string DOCUMENT { get; set; }

                                }
                                
                                public class Product
                                {
                                    public ProductChild _TYPE { get; set; }
                                    public ProductChild _NAME { get; set; }
                                    public ProductChild _REQTYPE { get; set; }

                                    public class ProductChild
                                    {
                                        [XmlAttribute]
                                        public string _Description { get; set; }

                                        [XmlAttribute]
                                        public string _Identifier { get; set; }

                                    }

                                }
                            }

                        }

                    }

                }

                public LoanApplication LOAN_APPLICATION { get; set; }

                public class LoanApplication
                {
                    public Borrower_ BORROWER { get; set; }

                    public MortgageTerms MORTGAGE_TERMS { get; set; }

                    public class MortgageTerms
                    {

                        [XmlAttribute]
                        public string LenderCaseIdentifier { get; set; }

                    }

                    public class Borrower_
                    {
                        [XmlAttribute]
                        public string BorrowerID { get; set; }

                        [XmlAttribute]
                        public string _FirstName { get; set; }

                        [XmlAttribute]
                        public string _LastName { get; set; }

                        [XmlAttribute]
                        public string _BirthDate { get; set; }

                        [XmlAttribute]
                        public string _PrintPositionType { get; set; }

                        [XmlAttribute]
                        public string _SSN { get; set; }

                        public Residence _RESIDENCE { get; set; }

                        public Employer_ Employer { get; set; }

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
                            public string EmploymentCurrentIndicator { get; set; }

                            [XmlAttribute]
                            public string _PostalCode { get; set; }

                            [XmlAttribute]
                            public string _PhoneNumber { get; set; }

                            [XmlAttribute]
                            public string EmployerCode { get; set; }
                        }
                    }

                }


            }

        }

    }

   
    

}
