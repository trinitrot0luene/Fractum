using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Rest.Exceptions
{
    /// <summary>
    /// Thrown if the requested resource was not found on the server.
    /// </summary>
    public sealed class NotFoundException : Exception
    {
        internal NotFoundException(string message) : base(message) { }
    }
}
