using System;
using OpenQA.Selenium;
using Protractor;
using VOESystem.UnitTests.UI.Model;

namespace VOESystem.UnitTests.UI.Tags
{

    [ElementCssSelector("input[type=\"checkbox\"]")]
    public class Checkbox : ElementObjectBase
    {

        public Checkbox(NgWebDriver ngWebDriver, IWebElement element, string Id)
            : base(ngWebDriver, element, Id) { }

        public bool Checked
        {
            get
            {
                // this.Selected
                return this.GetAttribute("checked") == "true";
            }

        }


        public bool Enabled
        {
            get
            {
                bool result;
                if (this.GetAttribute("disabled") == null)
                {
                    result = true;
                } 
                else if (this.GetAttribute("disabled") == "false") 
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
                return result;
                
            }

        }

        //this overrides the ngwebelement click 
        public void Click()
        {
            try
            {
                ((NgWebElement)this).Click();
            }
            catch (Exception ex)  
            {
                //this obviously only tries the immediate parent.  if necessary we can loop this up the chain a couple of times
                if (ex.Message.ToLower().Contains("other element would receive the click"))
                {
                    ((NgWebElement)this.ParentElement).Click();
                }
                else
                {
                    throw ex;
                }
            }

        }

    }
}