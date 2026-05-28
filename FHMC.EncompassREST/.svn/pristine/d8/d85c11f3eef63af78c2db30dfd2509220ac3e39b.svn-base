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
        public class BorrowerPair
        {
            public Borrower borrower { get; set; }
            public Borrower coborrower { get; set; }
            public int applicationIndex { get; set; }
            public List<Residence> residences { get; set; }
            public List<Employment> employment { get; set; }

        }

        public class Borrower
        {

            public string firstName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string firstNameWithMiddleName { get; set; }
            public string lastName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string lastNameWithSuffix { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string aliasName { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool? isBorrower { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string experianCreditScore { get; set; }

            public string taxIdentificationIdentifier { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? equifaxDatePulled { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? experianDatePulled { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? transUnionDatePulled { get; set; }

            public string homePhoneNumber { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string mobilePhone { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string hmdaGenderType { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? birthDate { get; set; }


            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string equifaxScore { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string transUnionScore { get; set; }

            public string emailAddressText { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public bool authorizedCreditReportIndicator { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string authorizedCreditReportDate { get; set; }
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string creditReportAuthorizationMethod { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public List<Residence> residences { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public decimal? totalMonthlyIncomeAmount { get; set; }

        }

        public class Residence
        {
            public string residencyType { get; set; }
            public string addressCity { get; set; }
            public string addressPostalCode { get; set; }
            public string addressState { get; set; }
            public string addressStreetLine1 { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string applicantType { get; set; }
        }

        public class Employment
        {

            public string owner { get; set; }
            public string employerName { get; set; }
            public string addressCity { get; set; }
            public string addressPostalCode { get; set; }
            public string addressState { get; set; }
            public string addressStreetLine1 { get; set; }
            public bool currentEmploymentIndicator { get; set; }
            public bool selfEmployedIndicator { get; set; }
            public DateTime? employmentStartDate { get; set; }
            public DateTime? endDate { get; set; }
            public string businessPhone { get; set; }
            public string phoneNumber { get; set; }
            public string fax { get; set; }
            public string positionDescription { get; set; }
            public int? timeInLineOfWorkYears { get; set; }
            public int? timeOnJobTermMonths { get; set; }
            public int? timeOnJobTermYears { get; set; }
            public string email { get; set; }


        }

    }

}
