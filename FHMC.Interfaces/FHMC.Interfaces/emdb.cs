using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Interfaces
{
    namespace emdb
    {

        public interface IEmdbUserInfoView
        {
            string Email { get; set; }
            bool IsOOO { get; set; }
        }

    }
}
