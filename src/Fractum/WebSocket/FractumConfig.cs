using Fractum;

namespace Fractum.WebSocket
{
    public class FractumConfig
    {
        public FractumConfig()
        {
            Token = null;
            LargeThreshold = 200;
            MessageCacheLength = 100;
            AlwaysDownloadMembers = false;
            DisableLogging = false;
            LogSeverity = LogSeverity.Info;
        }

        public string Token { get; set; }

        public int LargeThreshold { get; set; }

        public int MessageCacheLength { get; set; }

        public bool AlwaysDownloadMembers { get; set; }

        public bool DisableLogging { get; set; }

        public LogSeverity LogSeverity { get; set; }
    }
}