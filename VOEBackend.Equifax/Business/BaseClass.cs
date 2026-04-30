using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Configuration;

namespace VOEBackend.Equifax.Business
{
    public class BaseClass : VOESystem.Data.Business.BusinessBase
    {
       
        protected const string VENDORID = "First Home Mortgage";
        //protected const string ACCOUNTNUMBER = "999FH11176"; //dev username
        //protected const string PASSWORD = @"00ATqu8AiJjN."; //dev password

        protected const string ACCOUNTNUMBERDAY1 = "187FM07012"; //prod username day1
        protected const string PASSWORDDAY1 = @"00UM2ys.bRA7w"; //prod password day1

        protected const string ACCOUNTNUMBER = "187FM34526"; //prod username
        protected const string PASSWORD = @"00K0NjxCxMbmA"; //prod password
  
        public enum VerificationType
        {
           
            Current,
            Prior,
            All
        }

        public enum OrderType
        {
            Verbal = 1,
            Written = 2
        }

        public string isNull(object inString, string replVal)
        {

            if (inString == null)
            {
                return replVal;
            }
            else
            {
                return inString.ToString();
            }

        }

        public string BackendBaseURL
        {
            get
            {
                return ConfigurationManager.AppSettings["BackendBaseURL"].ToString();
            }
        }

    }

    //Extension methods must be defined in a static class
    public static class StringExtension
    {

        public static string ToTitleCase(this string str)
        {
            TextInfo itIinfo = new CultureInfo("en-US", false).TextInfo;
            return itIinfo.ToTitleCase(str);
        }


    }

    

}
