using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.TrueWork.TrueWorkSchema
{

    public enum RequestType
    {
        [Description("employment")]
        employment,
        [Description("employment-income")]
        employment_income  //should be employment-income
    }

    public enum PermissiblePurpose
    {
        child_support,
        [Description("credit-application")]
        credit_application,
        employee_eligibility,
        employee_request,
        employee_review_or_collection,
        employment,
        insurance_underwriting_application,
        legitimate_reason_initiated,
        legitimate_reason_review,
        risk_assessment,
        subpoena
    }

    public enum UseCase
    {
        [Description("mortgage")]
        mortgage,
        background,
        tenant,
        government,
        auto,
        lending,
        credit,
        identity,
        insurance,
        health,
        offers,
        account_management,
        [Description("preapproval")]
        preapproval,
        other
    }

    public enum EmployerFilter
    {
        [Description("all-employers")]
        all_employers,
        [Description("current-employer")]
        current_employer,
        [Description("previous-employers")]
        previous_employers,
        [Description("target-employer")]
        target_employer
    }

    public class Request
    {

        public class Target
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string social_security_number { get; set; }
            public string contact_email { get; set; }
            public string date_of_birth { get; set; }
            public Company company { get; set; }
        }

        public class Company {
            public string name { get; set; }
        }

        public class Document
        {
            public string filename { get; set; }
            public string content { get; set; }  //base64endcoded
        }

        public class RequestParamaters
        {
            public VerificationMethods verification_methods { get; set; }
            public string employer_filter { get; set; }
        }

        public class VerificationMethods
        {

            public MethodEnabled instant { get; set; }
            public MethodEnabled credentials { get; set; }
            public MethodEnabled smart_outreach { get; set; }
        }

        public class MethodEnabled
        {
            public bool enabled { get; set; }
        }

        public string type { get; set; }
        public string permissible_purpose { get; set; }
        public Target target { get; set; }
        public string loan_id { get; set; }
        public string use_case { get; set; }
        public List<Document> documents { get; set; }
        public RequestParamaters request_parameters { get; set; }
        public string report_id { get; set; }

    }
}
