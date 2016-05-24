using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace WaveletFusion.Exceptions
{
    [Serializable]
    class InvalidImageSizeException : Exception
    {
        public InvalidImageSizeException ()
        {}

        public InvalidImageSizeException (string message) 
            : base(message)
        {}

        public InvalidImageSizeException (string message, Exception innerException)
            : base (message, innerException)
        {}

        protected InvalidImageSizeException(SerializationInfo info, StreamingContext context)
            : base (info, context)
        {}
    }
}
