using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;
using VOESystem.Data.DTO;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    [TestFixture]
    public class BaseDataService : ServiceTestBase
    {

        //"/api/base/orderstatus/list"
        [Test]
        public void Test_Services_BaseDataService_ListOrderStatuses()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            OrderStatusListResp response = oService.Any(new VOESystem.Services.BaseDataService.OrderStatusListRequest() { });

            //test that there are entries
            Assert.That(response.OrderStatusList.Count > 0);
           
            //test that the list includes parents
            Assert.That(response.OrderStatusList.Where<OrderStatus>(q => q.ParentStatusId == 0).ToList().Count > 0);

            //test that the list includes children
            Assert.That(response.OrderStatusList.Where<OrderStatus>(q => q.ParentStatusId != 0).ToList().Count > 0);

        }

        //"/api/base/orderstatus/list"
        [Test]
        public void Test_Services_BaseDataService_ListOrderStatusesParents()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            OrderStatusListResp response = oService.Any(new VOESystem.Services.BaseDataService.OrderStatusListRequest() {
                param = "parents"
            });

            //test that there are entries
            Assert.That(response.OrderStatusList.Count > 0);

            //test that the list includes parents
            Assert.That(response.OrderStatusList.Where<OrderStatus>(q => q.ParentStatusId == 0).ToList().Count > 0);

            //test that the list does not include children
            Assert.That(response.OrderStatusList.Where<OrderStatus>(q => q.ParentStatusId != 0).ToList().Count == 0);

        }

        //"/api/base/requesttype/list"
        [Test]
        public void Test_Services_BaseDataService_ListRequestTypes()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            RequestTypeListResp response = oService.Any(new VOESystem.Services.BaseDataService.RequestTypeListRequest() { });

            //test that there are entries
            Assert.That(response.RequestTypeList.Count > 0);
        
        }

        //"/api/base/voes/list"
        [Test]
        public void Test_Services_BaseDataService_ListVOESWithAdmin()
        {

            //make sure that test settings are such that user is not order assignment eligible
            //so we can make sure that this service is adding admin properly
            bool oldIsEligibleOrderAssignment = updateIsEligibleOrderAssignment(false);

            List<string> roles = new List<string> { "Administrator" };
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>(false, roles);

            VOESpecialistListResp response = oService.Any(new VOESystem.Services.BaseDataService.VOESpecialistListRequest() { });

            try
            {
                //test that there are entries
                Assert.That(response.VOESpecialistList.Count > 0);

                //test that this contains current user
                CustomUserSession session = (CustomUserSession)oService.GetSession();
                Assert.That(response.VOESpecialistList.Where(q => q.UserName == session.UserAuthId).ToList().Count > 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //return IsEligibleOrderAssignment to previous value
                updateIsEligibleOrderAssignment(oldIsEligibleOrderAssignment);
            }
        }

        //"/api/base/voes/list"
        [Test]
        public void Test_Services_BaseDataService_ListVOESWithoutAdmin()
        {
            //make sure that test settings are such that user is not order assignment eligible
            //so we can make sure that this service is adding admin properly
            bool oldIsEligibleOrderAssignment = updateIsEligibleOrderAssignment(false);

            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();
            CustomUserSession session = (CustomUserSession)oService.GetSession();
            if (session.HasRole("Administrator"))
            {
                session.Roles.Remove("Administrator");
            }

            VOESpecialistListResp response = oService.Any(new VOESystem.Services.BaseDataService.VOESpecialistListRequest() { });
            try
            {
                //test that there are entries
                Assert.That(response.VOESpecialistList.Count > 0);

                //test that this does not contain current user
                Assert.That(response.VOESpecialistList.Where(q => q.UserName == session.UserAuthId).ToList().Count == 0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //return IsEligibleOrderAssignment to previous value
                updateIsEligibleOrderAssignment(oldIsEligibleOrderAssignment);
                if (!session.HasRole("Administrator"))
                {
                    session.Roles.Add("Administrator");
                }
            }

        }

        //"/api/base/vendor/list"
        [Test]
        public void Test_Services_BaseDataService_ListVendors()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            VendorListResp response = oService.Any(new VOESystem.Services.BaseDataService.VendorListRequest() { });

            //test that there are entries
            Assert.That(response.VendorList.Count > 0);
           

        }

        //"/api/base/vendorremovalreason/list"
        [Test]
        public void Test_Services_BaseDataService_ListVendorRemovalReasons()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            VendorRemovalReasonListResp response = oService.Any(new VOESystem.Services.BaseDataService.VendorRemovalReasonListRequest() { });

            //test that there are entries
            Assert.That(response.VendorRemovalReasonList.Count > 0);

        }


        //"/api/base/logger"
        [Test]
        public void Test_Services_BaseDataService_StackTraceLog()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            oService.Any(new VOESystem.Services.BaseDataService.StackTraceLogRequest {
                URL = "http:\\fakeurl.com",
                Cause = "testing Cause",
                Message ="Test Message",
                StackTrace = "Test Stack t Trace - A Really Long String - A Really Long String",
                Type = "Test Type"
            });

            //just as long as there are no errors, i guess it was OK.  
        }

        //"/api/base/orderauditablefield/list"
        [Test]
        public void Test_Services_BaseDataService_OrderAuditableFieldList()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            OrderAuditableFieldListResp response = oService.Any(new VOESystem.Services.BaseDataService.OrderAuditableFieldListRequest() { });
   
            //test that there are entries
            Assert.That(response.FieldList.Count > 0);

        }


        //"/api/base/datacorrectionreason/list"
        [Test]
        public void Test_Services_BaseDataService_DataCorrectionReasonList()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            DataCorrectionReasonListResp response = oService.Any(new VOESystem.Services.BaseDataService.DataCorrectionReasonListRequest() { });

            //test that there are entries
            Assert.That(response.ReasonList.Count > 0);

        }

        //"/api/base/ooo/list"
        [Test]
        public void Test_Services_BaseDataService_OOOList()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            OOOListResp response = oService.Any(new VOESystem.Services.BaseDataService.OOOListRequest() { });

            //test that there are entries
            Assert.That(response.OOOList.Count > 0);

        }

        //"/api/base/empstatusreason/list"
        [Test]
        public void Test_Services_BaseDataService_EmpStatusReasonList()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            EmpStatusReasonListResp response = oService.Any(new VOESystem.Services.BaseDataService.EmpStatusReasonListRequest() { });

            //test that there are entries
            Assert.That(response.ReasonList.Count > 0);

        }

        //"/api/base/orderstatusreason/list"
        [Test]
        public void Test_Services_BaseDataService_OrderStatusReasonList()
        {
            Services.BaseDataService oService = GetServiceInstance<Services.BaseDataService>();

            OrderStatusReasonListResp response = oService.Any(new VOESystem.Services.BaseDataService.OrderStatusReasonListRequest() { });

            //test that there are entries
            Assert.That(response.ReasonList.Count > 0);

        }



    }
}
