using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Protractor;

namespace VOESystem.UnitTests.Tests
{
    public class UITestBase : UnitTests.Business.BusinessBase
    {
        public IWebDriver driver;
        public NgWebDriver ngDriver;

        
        [SetUp]
        public void Initialize()
        {

            //using chrome here
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(120);
            
            ngDriver = new NgWebDriver(driver);
        }

        [TearDown]
        public void EndTest()
        {
            var testResult = TestContext.CurrentContext.Result.Outcome;

            if (testResult == ResultState.Failure || testResult == ResultState.Error)
            {

                string testName = TestContext.CurrentContext.Test.Name;
                string message = TestContext.CurrentContext.Result.Message;
                string stackTrace = TestContext.CurrentContext.Result.StackTrace;

                logger.Error(testName + ": " + message + "\n" + stackTrace,
                    new Exception("Test Result Error"));

            }


            driver.Close();
            ngDriver.Quit();
        }

        public void Login(IWebDriver browser, string loginUrl, string UserName, string Password, string forwardURL = null)
        {

            if (IsLoggedIn)
            {
                ngDriver.WrappedDriver.Url = baseUrl + forwardURL;
            }
            else
            {
                if (forwardURL != null)
                {
                    string virtualDir = string.Empty;
                    Uri uri = new Uri(baseUrl);
                    virtualDir = uri.LocalPath; 
                    browser.Url = loginUrl + "?redirect=" + virtualDir + forwardURL;
                }
                else
                {
                    browser.Url = loginUrl;
                }

                IWebElement userNameTextBox = browser.FindElement(By.Id("username"));
                IWebElement passwordTextBox = browser.FindElement(By.Id("password"));

                userNameTextBox.Clear();
                passwordTextBox.Clear();

                userNameTextBox.SendKeys(UserName);
                passwordTextBox.SendKeys(Password);

                browser.FindElement(By.Id("loginButton")).Click();
            }


        }

        public List<Cookie> Cookies
        {
            get
            {
                IReadOnlyCollection<Cookie> jar = driver.Manage().Cookies.AllCookies;
                return jar.Select<Cookie, Cookie>(q => q).ToList();
  
            }
            
        }

        public bool IsLoggedIn
        {
            get
            {
                return Cookies.Where<Cookie>(q => q.Name == "statusFilterModel").FirstOrDefault() != null;
            }

        }


        public class TestException : CustomException 
        {
            protected string LoanNumber = String.Empty;
        }


        public static partial class TestExceptions
        {

            public class NoAvailLoanNumbersException : TestException
            {

                public override string Message
                {
                    get
                    {
                        return "No Loan Numbers met the search criteria for this test";
                    }
                }

                public NoAvailLoanNumbersException() { }

            }

            public class NoAvailOrdersException : TestException
            {

                public override string Message
                {
                    get
                    {
                        return "No orders met the search criteria for this test";
                    }
                }

                public NoAvailOrdersException() { }

            }

        }

    }
}
