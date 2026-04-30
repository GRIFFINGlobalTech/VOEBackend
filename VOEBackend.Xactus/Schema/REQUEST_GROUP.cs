using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VOEBackend.Xactus.Schema
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

            public RequestData REQUEST_DATA { get; set; }

            public class RequestData
            {
                public CreditRequest CREDIT_REQUEST { get; set; }

                public class CreditRequest
                {

                    [XmlAttribute]
                    public string MISMOVersionID { get; set; }

                    [XmlAttribute]
                    public string LenderCaseIdentifier { get; set; }

                    public CreditRequestData CREDIT_REQUEST_DATA { get; set; }
                    public LoanApplication LOAN_APPLICATION { get; set; }
                    
                    public class CreditRequestData
                    {

                        [XmlAttribute]
                        public string CreditRequestID { get; set; }

                        [XmlAttribute]
                        public string BorrowerID { get; set; }

                        [XmlAttribute]
                        public string CreditReportTypeOtherDescription { get; set; }

                        [XmlAttribute]
                        public string CreditReportRequestActionType { get; set; }

                        [XmlAttribute]
                        public string CreditReportType { get; set; }

                        [XmlAttribute]
                        public string CreditRequestType { get; set; }

                        [XmlAttribute]
                        public string VerifyIncome { get; set; }

                        [XmlAttribute]
                        public string RecordsFrom { get; set; }

                        [XmlAttribute]
                        public string CreditReportIdentifier { get; set; }

                        [XmlAttribute]
                        public string ReverifyEmployerID { get; set; }

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
                            public string _BirthDate { get; set; }

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

                                [XmlAttribute]
                                public string EmployerCode { get; set; }
                            }
                        }

                    }

                   



                }

               

               

            }

        }

    }

   
    

}
