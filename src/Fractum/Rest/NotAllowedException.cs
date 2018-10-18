using System;

namespace Fractum.Rest
{
    public sealed class NotAllowedException : Exception
    {
        internal NotAllowedException(string message) : base(message)
        {
        }
    }
}