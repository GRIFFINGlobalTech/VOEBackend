using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("table")]
    public class Table : ElementObjectBase
    {


        public Table(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) 
        {
            TableRows = new List<TableRow>() { };
        }

        public List<TableRow> TableRows { get; set; }

        [ElementCssSelector("tr")]
        public class TableRow : ElementObjectBase
        {

            public TableRow(NgWebDriver ngWebDriver, IWebElement element, string Id, object[] optionalArgs = null)
                : base(ngWebDriver, element, Id) {

                    TableCells = new List<TableCell>() { };
            
            }

            public List<TableCell> TableCells { get; set; }

            public string GetCellValue(string Id)
            {
                return this.TableCells.Where<TableCell>(q => q.Id == Id).FirstOrDefault().Text;
            }

            public TableCell GetCell(string Id)
            {
                return this.TableCells.Where<TableCell>(q => q.Id == Id).FirstOrDefault();
            }

            [ElementCssSelector("td")]
            public class TableCell : ElementObjectBase
            {
                public TableCell(NgWebDriver ngWebDriver, IWebElement element, string Id)
                    : base(ngWebDriver, element, Id) { }

                
            }


        }


    }
}
