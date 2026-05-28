using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FHMC.NLogWrapper;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            FHMC.NLogWrapper.Logger logger = new Logger();
            //logger.Error("the loan officer is not licensed for 1234567890", new Exception("Random thing is wrong"));
            //logger.Info("this has no number");
            logger.Error("WILL THIS PLEASE JUST SEND AN EMAIL", new Exception("Random thing is wrong"));

            //string errMsg = "MCLRequestTypeId=888";
            //errMsg += "|RequestDateTime=2023-02-05";
            //errMsg += "|QueueId=1234";
            //errMsg += @"|RequestFileName=C:\temp\zzzoooo.pdf";

            //logger.Trace(errMsg);



        }
    }
}
