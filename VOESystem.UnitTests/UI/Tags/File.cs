using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("input[type=\"file\"]")]
    public class File : ElementObjectBase
    {
        public File(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }
    }
}
