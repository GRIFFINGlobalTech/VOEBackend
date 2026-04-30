using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.DocuSign
{

    public class BaseClass : VOESystem.Data.Business.BusinessBase
    {
        protected BaseClass()
        {

        }


        protected static string PrivateKeyFilename = ConfigurationManager.AppSettings["PrivateKeyFilename"].ToString();
        protected static string IntegratorKey = ConfigurationManager.AppSettings["IntegratorKey"].ToString();
        protected static string UserId = ConfigurationManager.AppSettings["UserId"].ToString();
        protected static string OAuthBasePath = ConfigurationManager.AppSettings["OAuthBasePath"].ToString();
        protected static int ExpiresInHours = Int32.Parse(ConfigurationManager.AppSettings["ExpiresInHours"].ToString());
        protected static string Host = ConfigurationManager.AppSettings["DocuSignHost"].ToString();

        //this.Host = (host != null) ? host : "https://demo.docusign.net/restapi";
        //this.Username = (username != null) ? username : "cdesimone@firsthome.com";
        //this.Password = (password != null) ? password : "S43JH!78gbn2";
        //this.IntegratorKey = (integratorKey != null) ? integratorKey : "c8ed1c0f-7d52-451b-b7cd-c89d6eb6dc64";

        //for "consent required initial error, need user to grant consent 
        //https://account-d.docusign.com/oauth/auth?response_type=code&scope=signature%20impersonation&client_id=CLIENT_ID&redirect_uri=https://docusign.com

        public DateTime BusinessDayAdd(IDbConnection dbConn, DateTime startdate, int intervalDays)
        {

            Dictionary<string, object> prms = new Dictionary<string, object> { };
            prms.Add("date1", startdate.ToString("yyyy-MM-dd"));
            prms.Add("days", intervalDays);

            return dbConn.SqlScalar<DateTime>("SELECT dbo.fn_Date_CalculateBusinessDaysDate(@date1, @days)", prms);

        }




    }


}

