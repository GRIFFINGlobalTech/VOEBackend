using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Interfaces
{
    namespace Encompass
    {
        public interface IVerifyLogin
        {
            bool Verify(string UserName, string Password);
        }
    }

}
