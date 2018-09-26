using System;
using System.Collections.Generic;
using System.Text;

namespace Fractum.Entities
{
    public sealed class LogMessage
    {
        public LogSeverity Severity { get; private set; }

        public string Source { get; private set; }

        public string Message { get; private set; }

        public Exception Exception { get; private set; }

        public LogMessage(string source, string message, LogSeverity severity, Exception exception = null)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Exception = exception;
        }

        public override string ToString()
            => $"{Severity.ToString().PadRight(7)} | {DateTimeOffset.UtcNow.ToString("dd/MM HH:mm:ss")} | {Source} | {Message} {(Exception is null ? string.Empty : "|")} {Exception?.Message}";
    }
}
