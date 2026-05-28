using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.NetworkInformation;

namespace FHMC.Email
{

    public class BaseClass
    {

        private bool getMAC = false;

        public BaseClass()
        {
            Log = new FHMC.NLogWrapper.Logger(GetType().FullName);
            CheckMACKey();
            CheckAuthorization();
           
        }

        public BaseClass(dynamic logger)
        {
            if (logger == null)
            {
                Log = new FHMC.NLogWrapper.Logger(GetType().FullName);
            }
            else
            {
                Log = logger;
            }

            CheckMACKey();
            CheckAuthorization();
        }

        private void CheckMACKey() {

            var macSetting = (ConfigurationManager.AppSettings["GetMAC"]);
            if (macSetting != null)
            {
                if (macSetting == "true")
                {
                    getMAC = true;
                }
            }

        }

        protected dynamic Log;

        private void CheckAuthorization()
        {

            if (!IsAuthorized())
            {
                string msg = "Not Authorized for Use of this Library.";
                Exception ex = new Exception(msg);
                Log.Error(msg, ex);
                throw ex;
            }

        }


        private bool IsAuthorized()
        {
            bool retVal = false;

            string MAC = getAuthString();
            if (authorizedStrings.Contains(MAC))
            {
                retVal = true;
            }

            return retVal;
        }

        private List<string> authorizedStrings = new List<string>()
        {
            "00-15-5D-00-57-35", //mydev3
            "00-15-5D-00-57-30",   //mydev2
            "00-15-5D-00-57-2F",  //mydev
            "00-50-56-82-54-16", //admin
            "00-50-56-B5-7D-54",  //sdk server
            "00-0D-3A-9D-3F-EE", //sdk server in azure
            "00-50-56-B5-6B-D0",  //admin2
            "00-0D-3A-8B-22-A4",  //aze-app-01
            "00-22-48-25-AF-BE", //admin2
            "00155D005735", //mydev3
            "00155D005730",   //mydev2
            "00155D00572F",  //mydev
            "005056825416", //admin
            "005056B57D54",  //sdk server
            "000D3A9D3FEE", //sdk server in azure
            "005056B56BD0",  //admin2
            "000D3A8B22A4",  //aze-app-01
            "00224825AFBE" //admin2


        };

     

        private string getAuthString()
        {
            //MAC Address
            string retVal = null;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    retVal = nic.GetPhysicalAddress().ToString();
                    break;
                }
            }

            if (getMAC)
            {
                Log.Info(retVal);
            }

            return retVal;
        }

        

    }
}
