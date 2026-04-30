using System;
using System.Configuration;
using System.Data;
using System.Linq;
using ServiceStack.OrmLite;
using System.Collections.Generic;
//using EllieMae.Encompass.Configuration;
using FHMC.NLogWrapper;

namespace VOEBackend.Encompass
{
    public class BaseClass
    {
        public BaseClass()
        {
            Log = new FHMC.NLogWrapper.Logger(GetType().FullName);
        }

        protected FHMC.NLogWrapper.Logger Log { get; private set; }
        //public static string encompassServer = ConfigurationManager.AppSettings["EncompassServer"].ToString();

        //public DateTime EncBusinessDayAdd(DateTime startdate, int intervalDays, object encompasssession,
        //    string UserName, string Password)
        //{

        //    EllieMae.Encompass.Client.Session emSession;

        //    if (encompasssession == null)
        //    {
        //        emSession = new EllieMae.Encompass.Client.Session();
        //        emSession.Start(encompassServer, UserName, Password);
        //    }
        //    else
        //    {
        //        emSession = (EllieMae.Encompass.Client.Session)encompasssession;
        //    }

        //    BusinessCalendar busCal;
        //    SystemSettings emSysSettings;

        //    emSysSettings = emSession.SystemSettings;
        //    busCal = emSysSettings.GetBusinessCalendar(BusinessCalendarType.Company);

        //    DateTime returnDate = startdate;
        //    int iStep;

        //    if (intervalDays < 0)
        //    {
        //        iStep = -1;
        //    }
        //    else
        //    {
        //        iStep = 1;
        //    }

        //    int counter = 0;
        //    while (Math.Abs(counter) < Math.Abs(intervalDays))
        //    {
        //        returnDate = returnDate.AddDays(iStep);

        //        if (busCal.IsBusinessDay(returnDate))
        //        {
        //            counter += iStep;
        //        }
        //    }

        //    //cleanup local session
        //    if (encompasssession == null)
        //    {
        //        emSession.End();
        //    }

        //    return returnDate;
        //}

        public DateTime BusinessDayAdd(IDbConnection dbConn, DateTime startdate, int intervalDays)
        {

            Dictionary<string, object> prms = new Dictionary<string, object> { };
            prms.Add("date1", startdate.ToString("yyyy-MM-dd"));
            prms.Add("days", intervalDays);

            return dbConn.SqlScalar<DateTime>("SELECT [emdbReporting].dbo.fn_Date_CalculateBusinessDate(@date1, @days)", prms);
               
        }

        public string getFileExtension(string FileName, string sepChar)
        {

            if (FileName.Contains(sepChar))
            {
                string revFileName = ReverseString(FileName);
                string revExt = revFileName.Substring(0, revFileName.IndexOf(sepChar));

                return ReverseString(revExt);
            }
            else
            {
                return string.Empty;
            }


        }

        public string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
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

        public string isNotNullDateTime(DateTime? inDate, string dateFormat)
        {

            if (inDate == null)
            {
                return null;
            }
            else if (inDate <= DateTime.Parse("1900-01-01"))
            {
                return null;
            }
            else
            {
                return ((DateTime)inDate).ToString(dateFormat);
            }


        }

    }
}
