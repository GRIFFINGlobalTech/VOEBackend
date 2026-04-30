using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VOEBackend.Xactus.Schema;

namespace VOEBackend.Xactus.Business
{
    public class XactusTestClass
    {

        public void testOperation()
        {

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
            OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            dbConn.Open();

            CommOps cop = new CommOps();
            string testResponseFile = @"C:\temp\20241218074009105_1024493175-02_XactusExperianInstantOrderSubmitResponse.xml";
            cop.postRequest(dbConn, null, "1024493175-02", 438427, "cdesimone", true, false, false, BaseClass.SubVendor.Experian, null, testResponseFile);


        }


        public string serializeRequest(REQUEST_GROUP request)
        {

            string retVal = null;

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(REQUEST_GROUP), "");
            string postString = String.Empty;

            StringWriter textWriter = new StringWriter();

            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter,
                              new XmlWriterSettings()
                              {
                                  OmitXmlDeclaration = false,
                                  ConformanceLevel = ConformanceLevel.Auto,
                                  NewLineHandling = NewLineHandling.Replace,
                                  NewLineChars = ""
                              }))
            {
                var nsSerializer = new XmlSerializerNamespaces();
                nsSerializer.Add("", "");
                xmlSerializer.Serialize(xmlWriter, request, nsSerializer);
                retVal = textWriter.ToString().Replace("utf-16", "utf-8");
            }

            return retVal;
        }

        public RESPONSE_GROUP deserializeRequest(string response)
        {

            RESPONSE_GROUP retVal = new RESPONSE_GROUP();

            XmlSerializer xmlRespSerializer = new XmlSerializer(typeof(RESPONSE_GROUP));
            XmlReader xmlRespReader = new XmlTextReader(new StringReader(response));
            retVal = (RESPONSE_GROUP)xmlRespSerializer.Deserialize(xmlRespReader);

            return retVal;
        }

    }


}
