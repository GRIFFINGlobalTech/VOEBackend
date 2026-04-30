using NUnit.Framework;
using OpenQA.Selenium;
using VOESystem.UnitTests.UI;

namespace VOESystem.UnitTests.Tests.IntegrationTests
{

    [TestFixture]
    public class Login : UITestBase
    {

        [Test]
        public void Test_Intgr_Login_BadUserNamePassword()
        {

            
            Login(driver, loginUrl, "baduser", "apassword");

            //ensure that the red error text is there
            string visibleText = driver.FindElement(By.Id("errorNote")).Text;
            Assert.IsTrue(visibleText.Contains("failed") || visibleText.Contains("locked"));

            //ensure that the loading graphic is not visible
            Assert.IsFalse(driver.FindElement(By.Id("imgTinyProgress")).Displayed);

        }

        [Test]
        public void Test_Intgr_Login_GoodUserNamePassword()
        {

            Login(driver, loginUrl, UserName, Password);

            //ensure that we are redirected to the pipeline screen
            Assert.IsTrue(driver.Url.Contains("pipeline"));

            //ensure that username appears in upper right
            string formText = driver.FindElement(By.Id("logoutForm")).Text;
            Assert.IsTrue(formText.Contains(UserName));


        }



    }
}
