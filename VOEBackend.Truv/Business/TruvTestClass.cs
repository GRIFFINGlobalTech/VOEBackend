using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOESystem.Data.DBSchema;
using static VOEBackend.Truv.Business.OrderOps;

namespace VOEBackend.Truv.Business
{
    public class TruvTestClass
    {

        public void testOperation(int orderrequestId)
        {

            OrderOps ooop = new OrderOps();
            ooop.forwardTruvNotifications();

            //string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DevConnectionString"].ToString();
            //OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
            //OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
            //dbConn.Open();

            //List<int> certFieldIds = new List<int>() { };

            //VOESystem.Data.Business.EmailOps eOp = new VOESystem.Data.Business.EmailOps();
            //eOp.sendTemplateEmail(dbConn, "Truv Credentialing Notification Email to Branches", orderrequestId, null, true, false, 1, false);


            //OrderOps oOp = new OrderOps();
            ////oOp.autoQueryOpenOrderStatus();
            //oOp.submitNewCredentialsOrder(dbConn, orderrequestId, "cdesimone", false, 1, true);
            //oOp.queryOrderStatus(dbConn, orderrequestId, "voesystem", out certFieldIds, QueryType.Credentials);

            //CommOps cOp = new CommOps();
            //cOp.testDownload();

            //int reqId = 431372;

            //VOESystem.Data.Business.OrderOps op = new VOESystem.Data.Business.OrderOps();
            //OrderActivity vendorOA = op.getOrderActvityForNewActivty(dbConn, reqId, "voesystem", false);
            //vendorOA.PrevOrderStatusId = 1;
            //vendorOA.CurrOrderStatusId = 24;
            //vendorOA.PrevOrderSubStatusId = null;
            //vendorOA.CurrOrderSubStatusId = 30;

            //vendorOA.ActivityNote = "Order Moved to AutoWork# Final Reverify Status";
            //dbConn.Insert<OrderActivity>(vendorOA);


        }

    }
}
