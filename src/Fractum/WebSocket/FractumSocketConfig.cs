using Fractum.Rest;

namespace Fractum.WebSocket
{
    public class FractumSocketConfig : FractumRestConfig 
    {
        public int? LargeThreshold { get; set; }
    }
}