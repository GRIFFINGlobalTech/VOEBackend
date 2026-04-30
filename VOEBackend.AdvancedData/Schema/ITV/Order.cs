using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.AdvancedData.Schema.ITV
{
    public class Order
    {
        public string OrderDate { get; set; }
        public string OrderTime { get; set; }
        public string ThirdPartyOrderID { get; set; }
        public string ClosingDate { get; set; }
        public string OrderType { get; set; }
        public string CCEmails { get; set; }
        public string RushVOE { get; set; }
        public string LoanNum { get; set; }
        public string LoanOfficer { get; set; }
        public string LoanProcessor { get; set; }
        public List<Participant> LoanParticipants { get; set; }
        public string VOEOrderID { get; set; }
        public string VoEOrderID { get; set; }

    }

    public class Borrower
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameSuffix { get; set; }
        public string SSN { get; set; }
        public string BirthDate { get; set; }
        public string StreetAddress1 { get; set; }
        public string StreetAddress2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Phone { get; set; }
        public string AuthOnFile { get; set; }

    }


    public class Employer
    {
        public string OrderType { get; set; }
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string EmpAddress { get; set; }
        public string EmpAddress2 { get; set; }
        public string EmpCity { get; set; }
        public string EmpState { get; set; }
        public string EmpZipCode { get; set; }
        public string Phone1 { get; set; }
        public string FaxNumber { get; set; }
        public string HRContact { get; set; }
        public string HREmail { get; set; }
        public string Requestor { get; set; }
        public string EmpType { get; set; }

   

    }

    public class DocumentVoE
    {
        public string FileType { get; set; }
        public string DocumentType { get; set; }
        public string Encoding { get; set; }
        public string Content { get; set; }

    }

    public class Participant
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Company { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
