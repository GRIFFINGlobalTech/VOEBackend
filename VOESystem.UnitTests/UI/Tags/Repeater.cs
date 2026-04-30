using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    public class Repeater : ElementObjectBase
    {
        public Repeater(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }

    }
}
