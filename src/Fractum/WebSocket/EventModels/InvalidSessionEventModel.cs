namespace Fractum.WebSocket.EventModels
{
    public class InvalidSessionEventModel : EventModelBase
    {
        public bool Resumable { get; internal set; }
    }
}