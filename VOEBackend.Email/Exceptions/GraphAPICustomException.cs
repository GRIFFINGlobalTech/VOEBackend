using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.Email.Exceptions
{

    public class GraphAPICustomException : Exception, ISerializable
    {
        public string HTTPCode { get; private set; }
        public string HTTPMessage { get; private set; }

        public GraphAPICustomException()
        {

        }
        public GraphAPICustomException(string message)
            : base(message)
        {

        }
        public GraphAPICustomException(string message, Exception inner)
            : base(message, inner)
        {

        }


        public GraphAPICustomException(string message, string statuscode, string httpmessage)
            : base(message)
        {
            message += "; HTTP Status: " + statuscode;
            message += "; HTTP Message: " + httpmessage;

            HTTPCode = statuscode;
            HTTPMessage = httpmessage;

        }

        protected GraphAPICustomException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
    }
    
}
