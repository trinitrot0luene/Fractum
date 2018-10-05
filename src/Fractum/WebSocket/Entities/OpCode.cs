namespace Fractum.WebSocket.Entities
{
    public enum OpCode
    {
        Dispatch = 0,
        Heartbeat = 1,
        Identify = 2,
        StatusUpdate = 3,
        VoicesessionUpdate = 4,
        Resume = 6,
        Reconnect = 7,
        RequestGuildMembers = 8,
        InvalidSession = 9,
        Hello = 10,
        HeartbeatACK = 11
    }
}