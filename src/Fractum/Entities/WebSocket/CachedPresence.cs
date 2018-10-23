using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    public sealed class CachedPresence
    {
        internal CachedPresence(PresenceModel presence)
        {
            UserId = presence.User.Id;
            Game = presence.Activity;
            Status = presence.Status;
        }

        internal CachedPresence(ulong userId, Activity game, Status status)
        {
            UserId = userId;
            Game = game;
            Status = status;
        }

        public ulong UserId { get; private set; }

        public Activity Game { get; private set; }

        public Status Status { get; private set; }
    }
}