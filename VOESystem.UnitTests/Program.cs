using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnitLite;
using VOESystem.UnitTests.Tests.IntegrationTests;


//VOESystem.UnitTests.exe --where="namespace==VOESystem.UnitTests.Tests.ServiceTests"
//VOESystem.UnitTests.exe --where="class==VOESystem.UnitTests.Tests.ServiceTests.DocumentService"

namespace UnitTests
{
    public class Program
    {
        public static int Main(string[] args)
        {

            
            return new AutoRun().Execute(args);
        }
    }
}
