using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Contacts
    {

        public class ContactGroup
        {
            public string id { get; set; }
            public string contactType { get; set; }
            public string groupType { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public DateTime createdDate { get; set; }

        }

        public class ContactEntity
        {
            public string entityId { get; set; }
            public string entityType { get; set; }
            public string entityName { get; set; }
            public string entityUri { get; set; }
        }

        public class CategoryField
        {
            public int? fieldId { get; set; }
            public string fieldValue { get; set; }

        }

        public class ContactLicense
        {
            public string licenseAuthName { get; set; }
            public string licenseAuthType { get; set; }
            public string licenseNumber { get; set; }
            public string licenseStateCode { get; set; }
        }

        public class ContactAddress
        {
            public string street1 { get; set; }
            public string street2 { get; set; }
            public string city { get; set; }
            public string state { get; set; }
            public string zip { get; set; }
            public string unitType { get; set; }
        }

        public class Contact
        {

            public string id { get; set; }
            public List<int> groupIDs { get; set; }
            public int? categoryId { get; set; }
            public List<CategoryField> categoryFields { get; set; }
            public string companyName { get; set; }
            public string contactType { get; set; }
            public string email { get; set; }
            public float? fees { get; set; }
            public ContactLicense personalContactLicense { get; set; }
            public ContactLicense businessContactLicense { get; set; }
            public bool? noSpam { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string ownerId { get; set; }
            public int? accessLevel { get; set; }
            public ContactAddress currentMailingAddress { get; set; }
            public ContactAddress bizAddress { get; set; }
            public string businessWebUrl { get; set; }
            public string jobTitle { get; set; }
            public string workPhone { get; set; }
            public string homePhone { get; set; }
            public string mobilePhone { get; set; }
            public string faxNumber { get; set; }
            public string personalEmail { get; set; }
            public string businessEmail { get; set; }
            public string primaryEmail { get; set; }
            public string primaryPhone { get; set; }
            public string salutation { get; set; }

        }


        public class ContactsRequest : FilterRequest { }

        public class ContactsResponse
        {
            public string id { get; set; }
            public Dictionary<string, string> fields { get; set; } 
        }

        public class ContactGroupAdd : List<ContactEntity> { }
       
    }

}