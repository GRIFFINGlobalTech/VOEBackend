using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VOEBackend.AdvancedData.Business;

namespace VOEBackend.AdvancedData.Business
{
    public class ADTestClass
    {

        public void testOperation()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            dbConn.Open();

            Partners.OrderOps oOp = new Partners.OrderOps();
            //oOp.submitNewOrder(dbConn, 469289, "kvogtm191", false);
            //oOp.submitNewOrderNoAPI(dbConn, 469289, "kvogtm191", false, "Pending", "CCH82680");
            oOp.queryOrderStatus(dbConn, 469289, "kvogtm191");
            //Schema.Partners.REQUEST_GROUP part = oOp.createOrder(dbConn, 100929, "cdesimone", true);
            //Schema.Partners.REQUEST_GROUP part = oOp.createQuery(dbConn, 100929);

            //Partners.CommOps cop = new Partners.CommOps();
            //cop.postRequest(dbConn, part, "4022250686-03", 100929, Partners.OrderOps.CommOperation.Query, "cdesimone");

            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(Schema.Partners.REQUEST_GROUP), "");
            //string postString = String.Empty;

            //StringWriter textWriter = new StringWriter();

            //using (XmlWriter xmlWriter = XmlWriter.Create(textWriter,
            //                  new XmlWriterSettings()
            //                  {
            //                      OmitXmlDeclaration = true,
            //                      ConformanceLevel = ConformanceLevel.Auto,
            //                      NewLineHandling = NewLineHandling.Replace,
            //                      NewLineChars = ""
            //                  }))
            //{
            //    var nsSerializer = new XmlSerializerNamespaces();
            //    nsSerializer.Add("", "");
            //    xmlSerializer.Serialize(xmlWriter, part, nsSerializer);
            //    postString = textWriter.ToString();
            //}

            var res = 3;

            //verbal order  178370
            //oOp.submitNewOrder(dbConn, 18247, "cdesimone");

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

            //

            //oOp.queryOrderStatus(dbConn, 231467, "voesystem");


        }

    }
}
