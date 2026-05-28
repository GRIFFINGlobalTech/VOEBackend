using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
       

        static void Main(string[] args)
        {

            //get access token
            FHMC.EncompassREST.Authentication auth = new FHMC.EncompassREST.Authentication();
            string accessToken = auth.getAccessToken();

            FHMC.EncompassREST.User uOps = new FHMC.EncompassREST.User();
            uOps.unlockUserAndResetPassword("cdesimone", "sshf64jgEDFH", accessToken);



        }
    }
}
