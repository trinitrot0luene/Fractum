using System;

namespace Fractum.Rest
{
    /// <summary>
    ///     Thrown if an API request returned error code 400.
    /// </summary>
    public sealed class BadRequestException : Exception
    {
        internal BadRequestException(string message) : base(message)
        {
        }
    }
}