using System;
using System.Collections.Generic;
using System.Text;

namespace MartinGC94.PSRegistry.API
{
    [Serializable]
    public sealed class ApiException : Exception
    {
        public ApiException() { }
        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
    }
}
