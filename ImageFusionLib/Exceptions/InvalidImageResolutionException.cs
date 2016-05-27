using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace WaveletFusion.Exceptions
{
    [Serializable]
    public class InvalidImageResolutionException : Exception
    {
        public InvalidImageResolutionException ()
        {}

        public InvalidImageResolutionException (string message) 
            : base(message)
        {}

        public InvalidImageResolutionException (string message, Exception innerException)
            : base (message, innerException)
        {}

        protected InvalidImageResolutionException(SerializationInfo info, StreamingContext context)
            : base (info, context)
        {}
    }
}
