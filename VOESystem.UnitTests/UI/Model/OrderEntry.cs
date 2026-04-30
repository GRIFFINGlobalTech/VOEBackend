using System.Collections.Generic;
using Protractor;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.UI.Model
{
    public class OrderEntry : PageObjectBase
    {

        public OrderEntry(NgWebDriver ngDriver)
            : base(ngDriver) {

                //object map for employer repeater
                employerObjectMap.Add("empName", "EmployerName");
                employerObjectMap.Add("empTitle", "EmploymentTitle");
                employerObjectMap.Add("empDoVerify", "DoVerify");
                employerObjectMap.Add("empVerificationType", "VerficationType");
                employerObjectMap.Add("empLinkedOrder", "LinkedOrder");
                employerObjectMap.Add("empEmploymentStatus", "EmploymentStatus");
                employerObjectMap.Add("empIsRushRequest", "IsRushRequest");
                employerObjectMap.Add("empOrderNote", "OrderNote");
                employerObjectMap.Add("empDataCorrectionReason", "DataCorrectionReason");
                employerObjectMap.Add("empMilitaryStatus", "MilitaryStatus");
        }

        Dictionary<string, string> employerObjectMap = new Dictionary<string, string>();

        //public YesNoPopup YesNoPopup
        //{
        //    get
        //    {
        //        return this.YesNoPopup;
        //    }
        //}

        public DropDownBox BorrowerDropDown
        {
            get 
            {
                return GetObject<DropDownBox>("borrowerSelect");
            }
        }

        public DropDownBox RequestTypeDropDown
        {
            get
            {
                return GetObject<DropDownBox>("requestTypeSelect");
            }
        }

        public Checkbox SelfEmployedCheckbox
        {
            get
            {
                return GetObject<Checkbox>("selfEmploymentCheckbox");
            }
        }

        public DropDownBox Receives1099DropDown
        {
            get
            {
                return GetObject<DropDownBox>("1099Select");
            }
        }


        public DropDownBox EmploymentSelfCertDropDown
        {
            get
            {
                return GetObject<DropDownBox>("EmploymentSelfCertTypeSelect");
            }
        }

        public TextBox CPANameTextBox
        {
            get
            {
                return GetObject<TextBox>("cpaNameTextBox");
            }
        }

        public TextBox CPAPhoneTextBox
        {
            get
            {
                return GetObject<TextBox>("cpaPhoneTextBox");
            }
        }

        public TextBox CPAEmailTextBox
        {
            get
            {
                return GetObject<TextBox>("cpaEmailTextBox");
            }
        }

        public TextBox BorrowerEmailTextBox
        {
            get
            {
                return GetObject<TextBox>("borrEmailTextBox");
            }
        }

        public List<Employer> Employers
        {
            get
            {
                return GetRepeater<Employer>("employer in currentBorrowerDetails.Employers", employerObjectMap);
            }
            
        }

        public Button SubmitButton 
        {
            get 
            { 
                return GetObject<Button>("btnSubmit");
            }

        }

        public Hidden LastCreatedOrderRequestId
        {
            get
            {
                return GetObject<Hidden>("lastCreatedOrderRequestId");
            }
        }
        
        public Link AddNonBorrowerLink
        {
            get
            {
                return GetObject<Link>("addNonBorrowerLink");
            }

        }

        public TextBox NonBorrowerNameTextBox
        {

            get
            {
                return GetObject<TextBox>("nonBorrowerNameTextBox");
            }

        }

        public TextBox NonBorrowerAddressTextBox
        {

            get
            {
                return GetObject<TextBox>("nonBorrowerAddressTextBox");
            }

        }

        public TextBox NonBorrowerDOBTextBox
        {

            get
            {
                return GetObject<TextBox>("nonBorrowerDOBTextBox");
            }

        }

        public TextBox NonBorrowerSSNTextBox
        {

            get
            {
                return GetObject<TextBox>("nonBorrowerSSNTextBox");
            }

        }

        public DropDownBox NonBorrowerGenderDropDown
        {
            get
            {
                return GetObject<DropDownBox>("nonBorrowerGenderSelect");
            }
        }

        public TextBox NonBorrowerEmployerNameTextBox
        {
            get
            {
                return GetObject<TextBox>("nonBorrowerEmployerNameTextBox");
            }
        }

        public TextBox NonBorrowerEmployerAddressTextBox
        {
            get
            {
                return GetObject<TextBox>("nonBorrowerEmployerAddressTextBox");
            }
        }

        public TextBox NonBorrowerEmployerPhoneTextBox
        {
            get
            {
                return GetObject<TextBox>("nonBorrowerEmployerPhoneTextBox");
            }
        }

        public TextBox NonBorrowerEmploymentTitleTextBox
        {
            get
            {
                return GetObject<TextBox>("nonBorrowerEmploymentTitleTextBox");
            }
        }

        public DropDownBox NonBorrowerEmploymentStatusDropDown
        {
            get
            {
                return GetObject<DropDownBox>("nonBorrowerEmploymentStatusDropDown");
            }
        }

        public File NonBorrowerAuthFormFile
        {
            get
            {
                return GetObject<File>("nonBorrowerAuthFormFile");
            }
        }

        public Button AddContactButton
        {
            get
            {
                return GetObject<Button>("btnAddContact");
            }

        }

        public class Employer {

            public string EmployerName { get; set; }
            public Hidden EmploymentTitle { get; set; }
            public Checkbox DoVerify { get; set; }
            public DropDownBox VerficationType { get; set; }
            public DropDownBox LinkedOrder { get; set; }
            public DropDownBox EmploymentStatus { get; set; }
            public DropDownBox IsRushRequest { get; set; }
            public TextBox OrderNote { get; set; }
            public DropDownBox DataCorrectionReason { get; set; }
            public DropDownBox MilitaryStatus { get; set; }

        }

    }

    
}
