using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("input[type=\"hidden\"]")]
    public class Hidden : ElementObjectBase
    {
        public Hidden(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }

        //overriding protractor web element AND ElementObjectBase text property
        public new string Text
        {
            get
            {
                return GetAttribute("value");
            }
        }
    }
}
