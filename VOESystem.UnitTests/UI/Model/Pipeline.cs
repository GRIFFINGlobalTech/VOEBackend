using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;

namespace VOESystem.UnitTests.UI.Model
{
    public class Pipeline : PageObjectBase
    {

        public Pipeline(NgWebDriver ngDriver)
            : base(ngDriver) { }

        
        public List<Checkbox> StatusFilters
        {
            get
            {
                return GetObjectGroup<Checkbox>("statusFilters");
            }
        }

        public List<Checkbox> PendingSubFilterCheckboxes
        {
            get
            {
                return GetObjectGroup<Checkbox>("statusSubFilterPending");
            }
        }

        public List<Checkbox> OnHoldSubFilterCheckboxes
        {
            get
            {
                return GetObjectGroup<Checkbox>("statusSubFilterOnHold");
            }
        }

        public Button ApplyFilterButton
        {
            get
            {
                return GetObject<Button>("btnApplyFilter");
            }

        }

        public _PipelineGrid PipelineGrid
        {
            get
            {
                return GetUiGrid<_PipelineGrid>(base._ngWebDriver, "gridOptions");
            }

        }

        public TextBox SearchTextBox
        {

            get
            {
                return GetObject<TextBox>("textFilter");
            }

        }

        public DropDownBox UserFilterSelect
        {
            get
            {
                return GetObject<DropDownBox>("voesFilterId");
            }
        }

        public void SetStatusFilters(List<string> ParentStatusList)
        {

            //turn all offf for starters
            foreach (Checkbox cb in this.StatusFilters) {
                if(GetCheckedState(cb)) {
                    cb.Click();
                }
            }

            //turn on the ones we need
            foreach (string status in ParentStatusList)
            {
                this.StatusFilters.Where<Checkbox>(
                    q => q.Id.ToLower().Contains(status.ToLower())).FirstOrDefault().Click();

            }

        }    

        public bool GetCheckedState(Checkbox cb)
        {

            //determine checked state
            string bkgColor = cb.ParentElement.GetCssValue("background-color");
            string darkGreen = "rgba(15, 155, 73, 1)";
            bool isChecked = cb.Checked;

            if (bkgColor == darkGreen)
            {
                //this is a group one that is checked
                isChecked = true;
            }

            return isChecked;
        }

        public class _PipelineGrid : UiGrid
        {
            private NgWebDriver _ngWebDriver;

            public _PipelineGrid(NgWebDriver ngWebDriver, IWebElement element, string ngGridAttribute)
                : base(ngWebDriver, element, ngGridAttribute) {
                    _ngWebDriver = ngWebDriver;
            }

            public UiGridCellElement GetOrderDetailLink(int RowNumber)
            {
                string retVal = string.Empty;

                Dictionary<string, UiGridCellElement> rowContents = UiGrid.getGridRow(_ngWebDriver, this, RowNumber);

                return rowContents["Order Number"];

            }

        }

        public Checkbox IncludeRevisionReqCheckbox
        {
            get
            {
                return GetObject<Checkbox>("inclRevisionReq");
            }
        }


    }


}
