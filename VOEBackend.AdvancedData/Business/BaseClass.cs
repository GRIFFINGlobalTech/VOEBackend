using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.AdvancedData.Business
{
    public class BaseClass : VOESystem.Data.Business.BusinessBase
    {


        protected const string ACCOUNTNUMBER = "fhmvoe";
        protected const string PASSWORD = "Htgtaa@147";

        //prod credentials
        protected const string ADURL = @"api.creditinterlink.com/api/VOE?Client=firsthome";

        //test credentials
        //protected const string ADURL = @"api-test.creditinterlink.com/api/VOE?Client=firsthome";
  

    }


}
