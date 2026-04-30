using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("a")]
    public class Link : ElementObjectBase
    {
        public Link(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }


        public string URL
        {
            get
            {
                return GetAttribute("href");
            }
        }


    }
}
