using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VOESystem.Data.Business;
using VOESystem.Data.DBSchema;

namespace VOESystem.UnitTests.Tests.ServiceTests
{
    [TestFixture]
    public class PipelineService : ServiceTestBase
    {

        //[Test]
        //public void Test_Services_PipelineService_TestPermissions()
        //{


        //    PipelineOps po = new PipelineOps();
        //    Dictionary<string, VOESystem.Data.DTO.PipelineReq.OrderStatusFilter> filters = po.getOrderStatusFilters(Db);

        //    Services.PipelineService.PipelineRequest request = new Services.PipelineService.PipelineRequest
        //    {
        //       auditView = false,
        //       finalsView = false,
        //       orderStatusFilters = filters,
        //       redbellFilter = false,
        //       vendorFinalsView = false,
        //       voesFilterId = ""
        //    };

        //    List<string> roles = new List<string>() { "Encompass User" };;

        //    VOESystem.UnitTests.Business.PipelineOps testPo = new VOESystem.UnitTests.Business.PipelineOps();
        //    List<tempPermissionTestCases> cases = testPo.getTempPermissionTestCases().Where(q => q.TestId == 16).ToList();

        //    foreach (tempPermissionTestCases testCase in cases)
        //    {

        //        Services.PipelineService oService = GetServiceInstance<Services.PipelineService>(false, roles, testCase.UserId);

        //        List<PipelineSP> response = oService.Any(request);

        //        testPo.saveTempPipelineSPRecord(response, testCase.TestId);

        //    }


        //}


    }
}
