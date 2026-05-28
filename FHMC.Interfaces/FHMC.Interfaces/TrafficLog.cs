using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHMC.Interfaces
{
    namespace Utility 
    {

        public interface ITrafficDBLog
        {
            int LogRequest(DateTime RequestDateTime, string RequestFileName, int? RequestTypeId = null);

            void LogResponse(DateTime ResponseDateTime, string ResponseFileName, int RequestLogId,
                string StatusCode = null, string StatusDescription = null);
        }

    }
}
