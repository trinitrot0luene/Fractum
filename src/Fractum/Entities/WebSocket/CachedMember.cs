using System;
using System.Collections.Generic;
using System.Linq;
using Fractum.Contracts;
using Fractum.WebSocket.Core;
using Fractum.WebSocket.EventModels;

namespace Fractum.Entities.WebSocket
{
    /// <summary>
    ///     Cached data of a member of a <see cref="CachedGuild" />.
    /// </summary>
    public sealed class CachedMember : PopulatedEntity, IUser
    {
        internal CachedMember(FractumCache cache, GuildMemberAddEventModel model, ulong? parentGuildId = null) :
            base(cache)
        {
            GuildId = parentGuildId ?? model.GuildId ??
                      throw new InvalidOperationException("Member with no guild id cannot be constructed.");

            Id = model.User.Id;

            RoleIds = model.RoleIds;
            IsDeafened = model.IsDeafened;
            IsMuted = model.IsMuted;
            Nickname = model.Nickname;

            cache.AddUser(model.User);
        }

        public string Mention => string.Format(Consts.USER_MENTION, Id);

        internal void Update(GuildMemberUpdateEventModel model)
        {
            RoleIds = model.Roles ?? RoleIds;
            IsDeafened = model.PartialMember?.IsDeafened ?? IsDeafened;
            IsMuted = model.PartialMember?.IsMuted ?? IsMuted;
            Nickname = Nickname ?? Nickname;
            User.Username = model.User.Username ?? User.Username;
            User.Discrim = model.User.Discrim != short.MinValue ? User.Discrim : User.Discrim;

            var newPresence = new Presence();
            newPresence.Activity = model.Activity;
            newPresence.User = User;
            if (model.NewStatus.HasValue)
                newPresence.Status = model.NewStatus.Value;

            Cache[GuildId]?.AddOrUpdate(newPresence, old =>
            {
                old.Activity = model.Activity;
                old.Status = model.NewStatus ?? old.Status;
                return old;
            });
        }

        public override string ToString() =>
            $"{(Nickname == null ? string.Empty : $"{Nickname} ")}({Username}#{Discrim:0000})";

        #region Populated Properties

        internal User User => Cache.GetUserOrDefault(Id);

        public new DateTimeOffset CreatedAt => User.CreatedAt;

        public string Username => User.Username;

        public short Discrim => User.Discrim;

        public bool IsBot => User.IsBot;

        public string GetAvatarUrl() => User.GetAvatarUrl();

        public CachedGuild Guild => Cache[GuildId].Guild;

        public Presence Presence => Cache[GuildId].GetPresence(Id);

        public IEnumerable<Role> Roles
        {
            get
            {
                foreach (var role in Cache[GuildId].GetRoles())
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