using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VOESystem.Data.Business;

namespace VOEBackend.Encompass.Reports
{
    public class Reports : BusinessBase
    {

        public void EmailReport(int reportId)
        {//used
            try
            {
                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ProdConnectionString"].ToString();
                OrmLiteConnectionFactory dbFactory = new OrmLiteConnectionFactory(ConnectionString, true, SqlServerDialect.Provider);
                OrmLiteConnection dbConn = new OrmLiteConnection(dbFactory);
                dbConn.Open();

                ReportOps rop = new ReportOps();
                rop.emailReport(dbConn, reportId);
            } 
            catch (Exception ex)
            {
                logger.Error("Error Emailing report Id: " + reportId.ToString(), ex);

            }
        }

    }
}
