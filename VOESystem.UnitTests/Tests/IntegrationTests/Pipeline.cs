using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using VOESystem.UnitTests.Business;
using VOESystem.UnitTests.UI;
using VOESystem.UnitTests.UI.Tags;



namespace VOESystem.UnitTests.Tests.IntegrationTests
{
    [TestFixture]
    public class Pipeline : UITestBase
    {
        [Test]
        public void Test_Intgr_Pipeline_NewFilter()
        {

            
            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password);

            UI.Model.Pipeline pipelinePage = null;

            if (!ngDriver.WrappedDriver.Url.Contains("pipeline"))
            {
                ngDriver.WrappedDriver.Url = baseUrl + "pipeline";
            }

            //init model
            pipelinePage = new UI.Model.Pipeline(ngDriver);

            //only the new filter should be clicked
            pipelinePage.SetStatusFilters(new List<string> { "New" });
            
            pipelinePage.ApplyFilterButton.Click();
            ngDriver.WaitForAngular();

            //get the grid
            UI.Model.Pipeline._PipelineGrid pipelineGrid = pipelinePage.PipelineGrid;

            //click on date twice to sort descending
            pipelineGrid.HeaderRow["Requested Date"].Click();
            ngDriver.WaitForAngular();

            pipelineGrid.HeaderRow["Requested Date"].Click();
            ngDriver.WaitForAngular();

            //make sure revision requests are not included
            if (pipelinePage.GetCheckedState(pipelinePage.IncludeRevisionReqCheckbox))
            {
                pipelinePage.IncludeRevisionReqCheckbox.Click();
            }
            
            //make sure we can see proper results in grid
            //get the most recent new order
            OrderOps oOp = new OrderOps();
            string testNewOrderNumber = oOp.getOrdersByCriteria(new List<string> { "New" }, new List<string> { }, new List<string> { }, new List<string> { })
                .OrderByDescending(q => q.OrderRequestDate).FirstOrDefault().OrderNumber;

            //get contents of first row  (refreshes grid object first)
            string firstrow = String.Join("|", pipelinePage.PipelineGrid.RowText[0]);

            //check to see if the order is there
            Assert.True(firstrow.Contains(testNewOrderNumber));
            

        }

        [Test]
        public void Test_Intgr_Pipeline_RedBellFilter()
        {

            //make sure that there are some orders with unread messages for this user
            PipelineOps pO = new PipelineOps();
            pO.refreshUnreadMessagesForUser(UserName);

            Login(ngDriver.WrappedDriver, loginUrl, UserName, Password);

            UI.Model.Pipeline pipelinePage = null;

            if (!ngDriver.WrappedDriver.Url.Contains("pipeline"))
            {
                ngDriver.WrappedDriver.Url = baseUrl + "pipeline";
            }

            //init model
            pipelinePage = new UI.Model.Pipeline(ngDriver);

            //check to see if the red bell is in fact red
            Assert.That(pipelinePage.RedBell.IsRed);

            //get the number of unread messages
            int UnreadCount = pipelinePage.RedBell.UnreadMsgCount;

            //click on the red bell
            pipelinePage.RedBell.Click();

            //put the user name in teh search box
            pipelinePage.SearchTextBox.SendKeys(UserName);

            //get the grid
            UiGrid pipelineGrid = pipelinePage.PipelineGrid;

            //List<string[]> gridContents = pipelineGrid.RowText;
            //List<string[]> myGridContents = gridContents.Where<string[]>(q => string.Join("|",q).Contains(UserName)).ToList();

            //check that there are the same number of records visible
            Assert.That(pipelineGrid.RowCount == UnreadCount);


        }

    }

}