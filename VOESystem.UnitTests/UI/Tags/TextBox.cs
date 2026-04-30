using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("input[type=\"text\"]")]
    public class TextBox : ElementObjectBase
    {
        public TextBox(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }

    }
}
