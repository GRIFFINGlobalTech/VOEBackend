using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VOEBackend.DocuSign
{
    public class CustomException
    {

        public class TooManyEmbeddedRecipientsException : Exception, ISerializable
        {
            public TooManyEmbeddedRecipientsException()
            {

            }
            public TooManyEmbeddedRecipientsException(string message)
                : base(message)
            {

            }
            public TooManyEmbeddedRecipientsException(string message, Exception inner)
                : base(message, inner)
            {

            }

            protected TooManyEmbeddedRecipientsException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }


        public class EnvelopeNotCreatedException : Exception, ISerializable
        {
            public EnvelopeNotCreatedException()
            {

            }
            public EnvelopeNotCreatedException(string message)
                : base(message)
            {

            }
            public EnvelopeNotCreatedException(string message, Exception inner)
                : base(message, inner)
            {

            }

            protected EnvelopeNotCreatedException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }

        public class RepositoryPathNotFoundException : Exception, ISerializable
        {
            public RepositoryPathNotFoundException()
            {

            }
            public RepositoryPathNotFoundException(string message)
                : base(message)
            {

            }
            public RepositoryPathNotFoundException(string message, Exception inner)
                : base(message, inner)
            {

            }

            protected RepositoryPathNotFoundException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {

            }
        }

    }
}
