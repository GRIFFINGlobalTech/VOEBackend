using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{
    [ElementCssSelector("select")]
    public class DropDownBox : ElementObjectBase
    {


        NgWebDriver _ngWebDriver;

        public DropDownBox(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id)
        {
            _ngWebDriver = ngWebDriver;
        }

        public List<DropDownBoxOption> Options
        {
            get
            {
                List<NgWebElement> optionElements = this.FindElements(By.TagName("option")).ToList();
                List<DropDownBoxOption> options = new List<DropDownBoxOption>() { };

                foreach(NgWebElement option in optionElements ) {
                    options.Add(new DropDownBoxOption(_ngWebDriver, option, option.GetAttribute("value"),
                        this.Id, option.Text));
                }

                return options;
             
            }
        }

        //overriding protractor web element AND ElementObjectBase text property
        public string Text
        {
            get
            {
                return this.GetAttribute("value");
            }
        }

        public string SelectedText
        {

            get
            {
                string optionText = String.Empty;

                foreach(DropDownBoxOption opt in this.Options)
                {
                    if (opt.Selected)
                    {
                        optionText = opt.Text;
                    }

                }

                if (optionText.EndsWith("..."))
                {
                    optionText = String.Empty;
                }

                return optionText;

            }
            
            
        }

        
        public class DropDownBoxOption : ElementOptionBase
        {
            public DropDownBoxOption(NgWebDriver ngWebDriver, IWebElement element, string value, string parentId, string text)
                : base(ngWebDriver, element, value, parentId, text) { }


        }
    }



}
