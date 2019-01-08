using Fractum;

namespace Fractum.WebSocket
{
    public class GatewayConfig
    {
        public GatewayConfig(string token, int largeThreshold = 200, int messageCacheLength = 100, bool alwaysDownloadMembers = false)
        {
            Token = token;
            LargeThreshold = largeThreshold;
            MessageCacheLength = messageCacheLength;
            AlwaysDownloadMembers = alwaysDownloadMembers;
        }

        public string Token { get; set; }

        public int LargeThreshold { get; set; }

        public int MessageCacheLength { get; set; }

        public bool AlwaysDownloadMembers { get; set; }
    }
}