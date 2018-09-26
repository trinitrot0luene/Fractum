using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Rest.Exceptions
{
    public sealed class NotAllowedException : Exception
    {
        internal NotAllowedException(string message) : base(message) { }
    }
}
