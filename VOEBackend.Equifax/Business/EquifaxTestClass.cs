using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VOEBackend.Equifax.EquifaxSchema;
using VOESystem.Data.Business;

namespace VOEBackend.Equifax.Business
{
    public class EquifaxTestClass
    {

    

        public void testOperation()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            dbConn.Open();


            List<int> certIds = new List<int>() { };

            Equifax.Business.OrderOps eOp = new Equifax.Business.OrderOps();
            //eOp.submitNewInstantOrder(dbConn, 360360, "cdesimone", false, out certIds, null, null, 2, "Current", false);
            //eOp.autoSubmitOrdersToWorkNumber(false);



            //List<string> testlist = dbConn.Query<string>("SELECT OrderId FROM tempExperianTests WHERE OrderId NOT IN (32561,32562,32563) ORDER BY OrderId");

            //OrderOps oOp = new OrderOps();

            //foreach (string testCase in testlist)
            //{
            //    //oOp.submitNewOrder(dbConn, Int32.Parse(testCase), "cdesimone", true);
            //    oOp.queryOrderStatus(dbConn, Int32.Parse(testCase), "cdesimone");
            //}

            //oOp.queryOrderStatus(dbConn, 32561, "cdesimone");

            //oOp.submitNewOrder(dbConn, 32561, "cdesimone", true);

            //verbal order  178370
            //oOp.submitNewOrder(dbConn, 18247, "cdesimone", true);

            //written order  178373
            //oOp.submitNewOrder(dbConn, 18236, "cdesimone");

            //auth form is tif  178374
            //oOp.submitNewOrder(dbConn, 18234, "cdesimone");

            //self employent flag SUMBITTED (561)
            //oOp.submitNewOrder(dbConn, 18157, "cdesimone");

            //20257 SUBMITTED 567
            //oOp.submitNewOrder(dbConn, 20257, "cdesimone");

            //21178 SUBMITTTED 569
            //oOp.submitNewOrder(dbConn, 21178, "cdesimone");

            //CommOps cOp = new CommOps();
            //cOp.testFunction(18157, "cdesimone");

            //oOp.queryOrderStatus(dbConn, 18234, "voesystem");

            ////deserialize response
            //string responseString = File.ReadAllText(@"C:\temp\Instant VOE Submit_Response.xml");


            //XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(RESPONSE_GROUP));
            //XmlReader xmlRespReader = new XmlTextReader(new StringReader(responseString));
            //RESPONSE_GROUP response = (RESPONSE_GROUP)xmlRespSerializer.Deserialize(xmlRespReader);



            //VOEBackend.Equifax.Business.OrderOps EqOop = new VOEBackend.Equifax.Business.OrderOps();
            //List<int> certFileIds;
            //string result = EqOop.submitNewInstantOrder(dbConn, 18234, "cdesimone", true, out certFileIds, "theSecretSalaryKey", null, 2, "Current");

            //VOEBackend.Equifax.Business.OrderOps EqOop = new VOEBackend.Equifax.Business.OrderOps();
            //EqOop.createInstantReverifyOrder(dbConn, 178366);
            //List<int> certFileIds;
            //EqOop.submitReverifyInstantOrder(dbConn, 178366, "cdesimone", false, out certFileIds);

            //VOEBackend.Equifax.Business.CommOps cop = new VOEBackend.Equifax.Business.CommOps();
            //string responseString = File.ReadAllText(@"\\hq-sdk-1v\e$\VOERepository\Documents\EquifaxComm\20200124131013357_1075313576-08_EquifaxInstantOrderSubmitResponse.xml");
            //cop.postRequest(dbConn, null, "1075313576-08", 9, "cdesimone", true, false, responseString);
        }

    }
}
