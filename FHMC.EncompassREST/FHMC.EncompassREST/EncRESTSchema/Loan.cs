using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FHMC.EncompassREST
{
    public partial class Loan
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string encompassId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Application> applications { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Contact> contacts { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string loanNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ReferralSource { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Property property { get; set; }

        public class _CustomFields
        {
            public List<CustomField> customFields { get; set; }

            public class CustomField
            {
                public string id { get; set; }
                public DateTime? dateValue { get; set; }
                public string fieldName { get; set; }
                public decimal? numericValue { get; set; }
                public string stringValue { get; set; }
            }

        }

        protected class FolderMoveRequest
        {
            public string loanGuid { get; set; }
        }

        public class LoanLockRequest
        {

            public Resource resource { get; set; }

            public class Resource
            {
                public string entityId { get; set; }
                public string entityType { get; set; }
            }

            public LockType lockType { get; set; }

            [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
            public enum LockType
            {
                shared,
                exclusive
            }
        }

        public class LoanLockResponse
        {
            public string id { get; set; }
        }

        public class FieldValue
        {
            public string fieldId { get; set;}
            public string value { get; set; }
            public string format { get; set; }
            public bool readOnly { get; set; }
            public string description { get; set; }
            public string type { get; set; }

        }

        public class LoanContactResponse
        {
            public List<Contacts.Contact> contacts { get; set; }
        }

        public class ConversationLog
        {
            public string comments { get; set; }
            public bool inLogIndicator { get; set; }
            public bool isEmailIndicator { get; set; }
            public string name { get; set; }
            
        }

        public class Application
        {
            public Borrower borrower { get; set; }
            public string loanOfficerName { get; set; }
            public string creditReportReferenceIdentifier { get; set; }
            public bool prequalCreditReportIndicator { get; set; }
        }

        public class Contact
        {
            public string conatctType { get; set; }
            public string name { get; set; }
        }

        public class Property
        {
            public string streetAddress { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string postalCode { get; set; }
        }


    }
}
