using System;

namespace Fractum.Rest
{
    /// <summary>
    ///     Thrown if the requested resource was not found on the server.
    /// </summary>
    public sealed class NotFoundException : Exception
    {
        internal NotFoundException(string message) : base(message)
        {
        }
    }
}