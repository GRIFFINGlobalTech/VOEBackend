using FHMC.Interfaces.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FHMC.EncompassREST
{
    public partial class Contacts : BaseClass
    {

        public Contacts() : base() { }
        public Contacts(object Log) : base(Log) { }
        public Contacts(object Log, string TrafficFileTag, ITrafficDBLog TrafficDBLog)
            : base(Log, TrafficFileTag, TrafficDBLog) { }

        #region Enums
        public enum ContactGroupVisibility
        {
            Private = 0,
            Public = 1
        }

        public enum ContactType
        {
            Borrower,
            Business
        }

        public enum ContactGroupType
        {
            Companywide = 1
        }

        public enum ContactCategory
        {
            [System.ComponentModel.Description("Appraiser")]
            Appraiser = 0,
            [System.ComponentModel.Description("Attorney")]
            Attorney = 1,
            [System.ComponentModel.Description("Credit Company")]
            CreditCompany = 2,
            [System.ComponentModel.Description("Doc Signing")]
            DocSigning = 3,
            [System.ComponentModel.Description("Escrow Company")]
            EscrowCompany = 4,
            [System.ComponentModel.Description("Flood Insurance")]
            FloodInsurance = 5,
            [System.ComponentModel.Description("Hazard Insurance")]
            HazardInsurance = 6,
            [System.ComponentModel.Description("Lender")]
            Lender = 7,
            [System.ComponentModel.Description("Mortgage Insurance")]
            MortgageInsurance = 8,
            [System.ComponentModel.Description("Real Estate Agent")]
            RealEstateAgent = 9,
            [System.ComponentModel.Description("Servicing")]
            Servicing = 10,
            [System.ComponentModel.Description("Title Insurance")]
            TitleInsurance = 11,
            [System.ComponentModel.Description("Underwriter")]
            Underwriter = 12,
            [System.ComponentModel.Description("Surveyor")]
            Surveyor = 13,
            [System.ComponentModel.Description("No Category")]
            NoCategory = 14,
            [System.ComponentModel.Description("Organization")]
            Organization = 15,
            [System.ComponentModel.Description("Verification")]
            Verification = 16,
            [System.ComponentModel.Description("Investor")]
            Investor = 17,
            [System.ComponentModel.Description("Warehouse Bank")]
            WarehouseBank = 18,
            [System.ComponentModel.Description("Builder")]
            Builder = 19,
            [System.ComponentModel.Description("Dealer")]
            Dealer = 20,
            [System.ComponentModel.Description("Trade Assignee")]
            TradeAssignee = 21
        }

 



        #endregion Enums

        public List<ContactGroup> getContactGroups(ContactGroupVisibility groupVisibility, ContactType contactType, string accessToken)
        {
            List<ContactGroup> retVal = new List<ContactGroup>() { };

            throw new NotImplementedException("At this time, this function only returns groups created by this user.");

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "contactGroups";

                Dictionary<string, string> urlparams = new Dictionary<string, string>() { };

                urlparams.Add("contactType", contactType.ToString());
                urlparams.Add("groupType", groupVisibility.ToString());

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken, urlparams);

                retVal = JsonConvert.DeserializeObject<List<ContactGroup>>(responseString);

                
            }
            catch (Exception ex)
            {
                Log.Error("Error getting contact groups " + contactType.ToString() + ":" + groupVisibility.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public ContactGroup getContactGroup(int contactGroupId, string accessToken)
        {
            ContactGroup retVal = null;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "contactGroups/" + contactGroupId.ToString();

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<ContactGroup>(responseString);


            }
            catch (Exception ex)
            {
                Log.Error("Error getting contact group " + contactGroupId.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public List<ContactEntity> getContactEntitiesForGroup(int groupId, string accessToken)
        {

            List<ContactEntity> retVal = new List<ContactEntity>() { };

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "contactGroups/" + groupId.ToString() + "/contacts?limit=10000";

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<List<ContactEntity>>(responseString);


            }
            catch (Exception ex)
            {
                Log.Error("Error getting contact entities for groupId " + groupId.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public Contact getBusinessContact(string contactId, string accessToken)
        {
            Contact retVal = null;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "businessContacts/" + contactId;

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.GET, null, accessToken);

                retVal = JsonConvert.DeserializeObject<Contact>(responseString);


            }
            catch (Exception ex)
            {
                Log.Error("Error getting contact " + contactId, ex);
                throw ex;
            }


            return retVal;

        }

        public bool deleteContact(string entityId, string accessToken)
        {
            bool retVal = false;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "businessContacts/" + entityId.ToString();

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.DELETE, null, accessToken);

                retVal = true;
            }
            catch (Exception ex)
            {
                Log.Error("Error deleting contact " + entityId.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public bool updateContact(string entityId, Contact contact, string accessToken)
        {
            bool retVal = false;

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "businessContacts/" + entityId.ToString();

                string requestString = JsonConvert.SerializeObject(contact, Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                Dictionary<string, string> prms = new Dictionary<string, string>() { };
                prms.Add("allowEmpty", "True");

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.PATCH, requestString, accessToken, prms);

                retVal = true;

            }
            catch (Exception ex)
            {
                Log.Error("Error updating contact " + entityId.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public string createContact(Contact contact, string accessToken)
        {
            string retVal = null;

            try
            {
                string methodURL;

                if (contact.firstName == null && contact.lastName == null)
                {
                    methodURL = EncRESTServiceBaseURL() + "businessContacts?allowEmpty=True";
                } else
                {
                    methodURL = EncRESTServiceBaseURL() + "businessContacts";
                }

                string requestString = JsonConvert.SerializeObject(contact, Newtonsoft.Json.Formatting.None,
                         new JsonSerializerSettings
                         {
                             NullValueHandling = NullValueHandling.Ignore
                         });

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.POST, requestString, accessToken, null, true);

                //extract location from this mess with regex
                /*Content - Length: 0
                Cache - Control: private
                Date: Fri, 24 Jan 2020 19:48:14 GMT
                Location: /v1/businesscontacts/83a7a4fa-3ec8-4d69-be5b-030c027227f3*/
                Regex regex = new Regex(@"(?<=Location: \/v[1-9]*\/businesscontacts\/)(.*)");
                Match match = regex.Match(responseString);
                if (match.Success)
                {
                    retVal = match.Value;
                }

            }
            catch (Exception ex)
            {
                Log.Error("Error creating contact " + contact.companyName, ex);
                //throw ex;
            }

            return retVal;


        }

        public bool addContactToGroup(string entityId, ContactType contactType, ContactGroupType groupType, string accessToken)
        {
            bool retVal = false;

            try
            {
                
                string methodURL = EncRESTServiceBaseURL() + "contactGroups/" + ((int)groupType).ToString() + "/contacts?action=add";

                ContactGroupAdd addObj = (ContactGroupAdd)(new List<ContactEntity> {
                        new ContactEntity
                        {
                            entityId = entityId,
                            entityType = contactType.ToString()
                        }
                    });
                
                string requestString = JsonConvert.SerializeObject(addObj);

                string responseString = makeServiceRequest(methodURL, WebRequestMethod.POST, requestString, accessToken);


                if (responseString == "")
                {
                    retVal = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error adding contact to group " + groupType.ToString(), ex);
                throw ex;
            }

            return retVal;


        }

        public List<ContactsResponse> queryContactsByFields(Dictionary<string, string> filterValues, List<string> fields, string accessToken)
        {

            //this is an "exact" "and" query for the fieldvals passed in as params
            List<ContactsResponse> retVal = new List<ContactsResponse>() { };

            try
            {
                string methodURL = EncRESTServiceBaseURL() + "businessContactSelector";

                ContactsRequest request = new ContactsRequest();

                ContactsRequest.filterCriteria criteria = new ContactsRequest.filterCriteria();

                foreach (KeyValuePair<string, string> filter in filterValues)
                {
                    ContactsRequest.filterCriteria.term theTerm =
                        new ContactsRequest.filterCriteria.term(filter.Key, filter.Value, ContactsRequest.filterCriteria.term.MatchType.exact);

                    criteria.terms.Add(theTerm);
                    criteria.@operator = ContactsRequest.filterCriteria.FilterOperator.And;
                }

                request.filter = criteria;

                request.fields = fields;

                //start = 0 & limit = 100 & cursorType = RandomAccess
                int iStart = 0;
                int iLimit = 1000;

                Dictionary<string, string> prms = new Dictionary<string, string>() { };
                prms.Add("start", iStart.ToString());
                prms.Add("limit", iLimit.ToString());
                prms.Add("cursorType", "RandomAccess");
                
                string requestString = JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                string responseString;
                while (iStart <= retVal.Count)
                {
                    responseString = makeServiceRequest(methodURL, WebRequestMethod.POST, requestString, accessToken, prms);
                    retVal.AddRange(JsonConvert.DeserializeObject<List<ContactsResponse>>(responseString).ToList());
                    iStart += iLimit;
                    prms["start"] = iStart.ToString();
                }
            }
            catch (Exception ex)
            {

                string filterValString = JsonConvert.SerializeObject(filterValues);

                Log.Error("Error Querying Contacts by FilterValues: " + filterValString, ex);
                throw ex;
            }

            return retVal;
        }

        
    }
}
