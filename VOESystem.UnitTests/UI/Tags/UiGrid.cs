using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    public class UiGrid : ElementObjectBase
    {
        private string _gridId;

        public UiGrid(NgWebDriver ngWebDriver, IWebElement element, string UiGridAttribute)
            : base(ngWebDriver, element, UiGridAttribute)
        {
            _gridId = UiGridAttribute;
            RowText = new List<string[]>() { };
            HeaderRow = new Dictionary<string, UiGridHeaderElement>() { };
        }

        public List<string[]> RowText { get; set; }
        public Dictionary<string, UiGridHeaderElement> HeaderRow { get; set; }

        public int RowCount
        {
            get
            {
                return RowText.Count;
            }

        }

        public class UiGridHeaderElement : ElementObjectBase
        {

            private IWebElement _element;

            public UiGridHeaderElement(NgWebDriver ngWebDriver, IWebElement element, string Id)
                : base(ngWebDriver, element, Id) { _element = element; }


        }

        public class UiGridCellElement : ElementObjectBase
        {

            public UiGridCellElement(NgWebDriver ngWebDriver, IWebElement element, string Id)
                : base(ngWebDriver, element, Id) { }

        }

        public static T getGridElement<T>(NgWebDriver ngWebDriver, string UiGridAttribute)
            where T : UiGrid
        {

            //get grid object
            IReadOnlyCollection<NgWebElement> pageGrids = ngWebDriver.FindElements(By.ClassName("ui-grid"));

            //find the one we want
            NgWebElement currGrid = null;

            foreach (NgWebElement grid in pageGrids)
            {
                if (grid.GetAttribute("ui-grid") == UiGridAttribute)
                {
                    currGrid = grid;
                    break;
                }

            }

            if (currGrid == null)
            {
                throw new Exception("UiGrid not found: " + UiGridAttribute);
            }

            return (T)Activator.CreateInstance(typeof(T), ngWebDriver, currGrid, UiGridAttribute);

        }

        public static List<string[]> getGridRowsText(UiGrid currGrid)
        {

            List<string[]> retVal = new List<string[]>() { };

            IReadOnlyCollection<NgWebElement> repeaterRows = currGrid.FindElements(NgBy.Repeater("(rowRenderIndex, row) in rowContainer.renderedRows"));

            //load row text data only
            foreach (NgWebElement row in repeaterRows)
            {
                List<string> gridRowList = new List<string>() { };

                IReadOnlyCollection<NgWebElement> columns = row.FindElements(NgBy.Repeater("col in colContainer.renderedColumns"));

                foreach (NgWebElement col in columns)
                {
                    gridRowList.Add(col.Text);
                }

                retVal.Add(gridRowList.ToArray());
            }

            return retVal;

        }

        public static Dictionary<string, UiGridCellElement> getGridRow(NgWebDriver ngWebDriver, UiGrid currGrid, int RowNumber)
        {

            Dictionary<string, UiGridCellElement> retVal = new Dictionary<string, UiGridCellElement>() { };

            IReadOnlyCollection<NgWebElement> repeaterRows = currGrid.FindElements(NgBy.Repeater("(rowRenderIndex, row) in rowContainer.renderedRows"));

            NgWebElement row = repeaterRows.ElementAt<NgWebElement>(RowNumber);

            //load cell object for row
            IReadOnlyCollection<NgWebElement> columns = row.FindElements(NgBy.Repeater("col in colContainer.renderedColumns"));

            List<string> headerLabels = currGrid.HeaderRow.Select<KeyValuePair<string, UiGridHeaderElement>, string>(q => q.Key).ToList();

            int i = 0;
            foreach (NgWebElement col in columns)
            {
                string headerLabel = headerLabels.ElementAt(i).ToString();
                retVal.Add(headerLabel, (UiGridCellElement)Activator.CreateInstance(typeof(UiGridCellElement), ngWebDriver, col, headerLabel));
                i++;
            }

            return retVal;

        }
    }
}
