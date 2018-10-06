using System;

namespace Fractum.Rest.Exceptions
{
    public sealed class NotAllowedException : Exception
    {
        internal NotAllowedException(string message) : base(message)
        {
        }
    }
}