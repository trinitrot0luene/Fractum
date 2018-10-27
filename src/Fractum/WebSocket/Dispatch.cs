namespace Fractum.WebSocket
{
    /// <summary>
    ///     Helper class to provide intellisense for websocket events when registering handlers.
    /// </summary>
    public static class Dispatch
    {
        public static string BAN_ADD = "BAN_ADD";

        public static string BAN_REMOVE = "BAN_REMOVE";

        public static string CHANNEL_CREATE = "CHANNEL_CREATE";

        public static string CHANNEL_DELETE = "CHANNEL_DELETE";

        public static string CHANNEL_PINS_UPDATE = "CHANNEL_PINS_UPATE";

        public static string EMOJIS_UPDATE = "EMOJIS_UPDATE";

        public static string GUILD_CREATE = "GUILD_CREATE";

        public static string GUILD_DELETE = "GUILD_DELETE";

        public static string GUILD_MEMBER_ADD = "GUILD_MEMBER_ADD";

        public static string GUILD_MEMBER_REMOVE = "GUILD_MEMBER_REMOVE";

        public static string GUILD_MEMBERS_CHUNK = "GUILD_MEMBERS_CHUNK";

        public static string GUILD_MEMBER_UPDATE = "GUILD_MEMBER_UPDATE";

        public static string INTEGRATIONS_UPDATED = "INTEGRATIONS_UPDATED";

        public static string MESSAGE_CREATE = "MESSAGE_CREATE";

        public static string MESSAGE_DELETE = "MESSAGE_DELETE";

        public static string MESSAGE_UPDATE = "MESSAGE_UPDATE";

        public static string MESSAGE_REACTION_ADD = "MESSAGE_REACTION_ADD";

        public static string MESSAGE_REACTION_REMOVE = "MESSAGE_REACTION_REMOVE";

        public static string MESSAGE_CREATION_REMOVE_ALL = "MESSAGE_REACTION_REMOVE_ALL";

        public static string GUILD_ROLE_CREATE = "GUILD_ROLE_CREATE";

        public static string GUILD_ROLE_DELETE = "GUILD_ROLE_DELETE";

        public static string GUILD_ROLE_UPDATE = "GUILD_ROLE_UPDATE";

        public static string USER_UPDATE = "USER_UPDATE";

        public static string READY = "READY";

    }
}