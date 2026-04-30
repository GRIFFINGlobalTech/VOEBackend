using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;


namespace VOESystem.UnitTests.UI.Tags
{
    public class NgGrid : ElementObjectBase
    {
        private string _gridId;

        public NgGrid(NgWebDriver ngWebDriver, IWebElement element, string NgGridAttribute)
            : base(ngWebDriver, element, NgGridAttribute) 
        {
            _gridId = NgGridAttribute;
            RowText = new List<string[]>() { };
            HeaderRow = new Dictionary<string, NgGridHeaderElement>() { };
        }

        public List<string[]> RowText { get; set; }
        public Dictionary<string,NgGridHeaderElement> HeaderRow { get; set; }
        
        public int RowCount
        {
            get
            {
                return RowText.Count;
            }

        }
       
        public class NgGridHeaderElement : ElementObjectBase
        {

            private IWebElement _element;

            public NgGridHeaderElement(NgWebDriver ngWebDriver, IWebElement element, string Id)
                : base(ngWebDriver, element, Id) { _element = element; }

     
        }

        public class NgGridCellElement : ElementObjectBase
        {

            public NgGridCellElement(NgWebDriver ngWebDriver, IWebElement element, string Id)
                : base(ngWebDriver, element, Id) { }

        }

        public static T getGridElement<T>(NgWebDriver ngWebDriver, string ngGridAttribute)
            where T : NgGrid
        {

            //get grid object
            IReadOnlyCollection<NgWebElement> pageGrids = ngWebDriver.FindElements(By.ClassName("ngGrid"));

            //find the one we want
            NgWebElement currGrid = null;

            foreach (NgWebElement grid in pageGrids)
            {
                if (grid.GetAttribute("ng-grid") == ngGridAttribute)
                {
                    currGrid = grid;
                    break;
                }

            }

            if (currGrid == null)
            {
                throw new Exception("NgGrid not found: " + ngGridAttribute);
            }

            return (T)Activator.CreateInstance(typeof(T), ngWebDriver, currGrid, ngGridAttribute);

        }

        public static List<string[]> getGridRowsText(NgGrid currGrid)
        {

            List<string[]> retVal = new List<string[]>() { };

            IReadOnlyCollection<NgWebElement> repeaterRows = currGrid.FindElements(NgBy.ExactRepeater("row in renderedRows"));

            //load row text data only
            foreach (NgWebElement row in repeaterRows)
            {
                List<string> gridRowList = new List<string>() { };

                IReadOnlyCollection<NgWebElement> columns = row.FindElements(NgBy.ExactRepeater("col in renderedColumns"));

                foreach (NgWebElement col in columns)
                {
                    gridRowList.Add(col.Text);
                }

                retVal.Add(gridRowList.ToArray());
            }

            return retVal;

        }

        public static Dictionary<string,NgGridCellElement> getGridRow(NgWebDriver ngWebDriver, NgGrid currGrid, int RowNumber)
        {

            Dictionary<string,NgGridCellElement> retVal = new Dictionary<string,NgGridCellElement>() { };

            IReadOnlyCollection<NgWebElement> repeaterRows = currGrid.FindElements(NgBy.ExactRepeater("row in renderedRows"));

            NgWebElement row = repeaterRows.ElementAt<NgWebElement>(RowNumber);

            //load cell object for row
            IReadOnlyCollection<NgWebElement> columns = row.FindElements(NgBy.ExactRepeater("col in renderedColumns"));

            List<string> headerLabels = currGrid.HeaderRow.Select<KeyValuePair<string, NgGridHeaderElement>, string>(q => q.Key).ToList();

            int i = 0;
            foreach (NgWebElement col in columns)
            {
                string headerLabel = headerLabels.ElementAt(i).ToString();
                retVal.Add(headerLabel,(NgGridCellElement)Activator.CreateInstance(typeof(NgGridCellElement), ngWebDriver, col, headerLabel));
                i++;
            }

            return retVal;

        }

    
    }   
}
