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
        }

        public string Token { get; set; }

        public int LargeThreshold { get; set; }

        public int MessageCacheLength { get; set; }

        public bool AlwaysDownloadMembers { get; set; }
    }
}