using System;
using System.Collections.Generic;
using System.Linq;
using Fractum.WebSocket;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    /// <summary>
    ///     Cached data of a member of a <see cref="CachedGuild" />.
    /// </summary>
    public sealed class CachedMember : PopulatedEntity, IUser
    {
        internal CachedMember(ISocketCache<ISyncedGuild> cache, GuildMemberAddEventModel model, ulong? parentGuildId = null) :
            base(cache)
        {
            GuildId = parentGuildId ?? model.GuildId ??
                      throw new InvalidOperationException("Member with no guild id cannot be constructed.");

            Id = model.User.Id;

            RoleIds = model.RoleIds;
            IsDeafened = model.IsDeafened;
            IsMuted = model.IsMuted;
            Nickname = model.Nickname;
            JoinedAt = model.JoinedAt;
        }

        public string Mention => string.Format(Consts.USER_MENTION, Id);

        public string Discrim => DiscrimValue.ToString("0000");

        internal void Update(GuildMemberUpdateEventModel model)
        {
            RoleIds = model.Roles ?? RoleIds;
            IsDeafened = model.PartialMember?.IsDeafened ?? IsDeafened;
            IsMuted = model.PartialMember?.IsMuted ?? IsMuted;
            Nickname = Nickname ?? Nickname;
            User.Username = model.User.Username ?? User.Username;
            User.DiscrimValue = model.User.DiscrimValue != short.MinValue ? User.DiscrimValue : User.DiscrimValue;

            var newPresence = new CachedPresence(Id, model.Activity, model.NewStatus ?? Presence.Status);

            Cache.AddOrReplace(newPresence);
        }

        public override string ToString() =>
            $"{(Nickname == null ? string.Empty : $"{Nickname} ")}({Username}#{DiscrimValue:0000})";

        #region Populated Properties

        internal User User => Cache.TryGetUser(Id, out var user) ? user : default;

        public new DateTimeOffset CreatedAt => User.CreatedAt;

        public string Username => User.Username;

        public short DiscrimValue => User.DiscrimValue;

        public bool IsBot => User.IsBot;

        public string GetAvatarUrl() => User.GetAvatarUrl();

        public CachedGuild Guild => Cache.TryGetGuild(GuildId, out var guild) ? guild.Guild : default;

        private CachedPresence Presence => Cache.TryGetPresence(Id, out var presence) ? presence : default;

        public Status Status => Presence?.Status ?? Status.Offline;

        public Activity Game => Presence?.Game;

        public Permissions Permissions
        {
            get
            {
                var perms = Permissions.None;
                foreach (var role in Roles)
                    perms |= role.Permissions;

                if (perms.HasFlag(Permissions.Administrator) || Id == Guild.OwnerId)
                    return Permissions.All;

                return perms;
            }
        }

        public IEnumerable<Role> Roles
        {
            get
            {
                foreach (var role in Guild.Roles)
                    if (RoleIds.Any(rid => rid == role.Id))
                        yield return role;
            }
        }

        #endregion

        #region Cached Properties

        internal ulong GuildId { get; }

        internal ulong[] RoleIds { get; private set; }

        public string Nickname { get; private set; }

        public bool IsMuted { get; private set; }

        public DateTimeOffset JoinedAt { get; private set; }

        public bool IsDeafened { get; private set; }

        #endregion

        #region REST

        #endregion
    }
}