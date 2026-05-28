using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {


        static FHMC.NLogWrapper.Logger Log;

        static void Main(string[] args)
        {

            try
            {

                Log = new FHMC.NLogWrapper.Logger("aLogger");


                //FHMC.Graph.FileIO fOp = new FHMC.Graph.FileIO(Log);

                //Log.Info("Test Complete");

                

            }

            catch (Exception ex)
            {
                Log.Fatal("Test Fatal Error", ex);
            }
           
        }
    }
}
