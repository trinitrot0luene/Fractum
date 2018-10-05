using Fractum.Rest;

namespace Fractum.WebSocket
{
    public class FractumSocketConfig : FractumRestConfig
    {
        public FractumSocketConfig()
        {
            LargeThreshold = 200;
            AlwaysDownloadMembers = false;
        }

        public int LargeThreshold { get; set; }

        public bool AlwaysDownloadMembers { get; set; }
    }
}