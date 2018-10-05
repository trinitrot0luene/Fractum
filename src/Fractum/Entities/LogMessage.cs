using System;

namespace Fractum.Entities
{
    public sealed class LogMessage
    {
        public LogMessage(string source, string message, LogSeverity severity, Exception exception = null)
        {
            Severity = severity;
            Source = source;
            Message = message;
            Exception = exception;
        }

        public LogSeverity Severity { get; }

        public string Source { get; }

        public string Message { get; }

        public Exception Exception { get; }

        public override string ToString()
            =>
                $"{Severity.ToString().PadRight(7)} | {DateTimeOffset.UtcNow.ToString("dd/MM HH:mm:ss")} | {Source.PadRight(20)} | {Message} {(Exception is null ? string.Empty : "|")} {Exception?.Message}";
    }
}