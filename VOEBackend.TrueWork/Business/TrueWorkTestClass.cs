using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.TrueWork.Business
{
    public class TrueWorkTestClass
    {

        public void testOperation()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DevConnectionString"].ToString();
            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            dbConn.Open();

            List<int> certFieldIds = new List<int>() { };

            OrderOps oOp = new OrderOps();

            //oOp.createCredentialsOrder(dbConn, 372469, BaseClass.OrderType.Verbal, false);
            //oOp.submitNewCredentialsOrder(dbConn, 372468, "cdesimone", false, out certFieldIds, 1, true);
            //oOp.queryCredentialsOrderStatus(dbConn, 372468, "cdesimone", out certFieldIds);

            //oOp.submitNewCredentialsOrder(dbConn, 372469, "cdesimone", false, out certFieldIds, 1, true);
            //oOp.queryCredentialsOrderStatus(dbConn, 372469, "cdesimone", out certFieldIds);

            //oOp.submitNewCredentialsOrder(dbConn, 313586, "cdesimone", false, out certFieldIds, 1, true);
            //oOp.queryCredentialsOrderStatus(dbConn, 313586, "cdesimone", out certFieldIds);


            //oOp.submitNewInstantOrder(dbConn, 320085, "cdesimone", false, out certFieldIds, 1, "All", "Derick", "Miles", "");
            //oOp.submitNewReverifyOrder(dbConn, 336511, "cdesimone", false, "AAAAAAADsgIAC7jjPQISKAJVg81cVqMiJk8yWzmZSHXfmPex6lOWtZGB");

            //oOp.queryReverifyOrderStatus(dbConn, 336511, "cdesimone", out certFieldIds);

            //string responseFile = File.ReadAllText(@"C:\Temp\20230113100033915_1024443392-01_CreateInstantResponse.json");


            //CommOps cop = new CommOps();
            //cop.postRequest(dbConn, null, "123455", 1, "cdesimone", true, CommOps.TrueWorkCommType.CreateInstant, null, responseFile);


        }

    }
}
