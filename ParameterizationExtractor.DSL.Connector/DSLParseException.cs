using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ParameterizationExtractor.DSL.Connector
{
    public class DSLParseException : Exception
    {
        public DSLParseException()
        {
        }

        public DSLParseException(string message) : base(message)
        {
        }

        public DSLParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DSLParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
