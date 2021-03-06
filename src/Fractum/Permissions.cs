﻿using System;

namespace Fractum
{
    [Flags]
    public enum Permissions
    {
        None = 0x00000000,
        CreateInvite = 0x00000001,
        KickUsers = 0x00000002,
        BanUsers = 0x00000004,
        Administrator = 0x00000008,
        ManageChannels = 0x00000010,
        ManageGuild = 0x00000020,
        AddReactions = 0x00000040,
        ViewAuditLogs = 0x00000080,
        ViewChannel = 0x00000400,
        SendMessages = 0x00000800,
        SendTTSMessages = 0x00001000,
        ManageMessages = 0x00002000,
        EmbedLinks = 0x00004000,
        AttachFiles = 0x00008000,
        ReadMessageHistory = 0x00010000,
        MentionEveryone = 0x00020000,
        UseExternalEmojis = 0x00040000,
        Connect = 0x00100000,
        Speak = 0x00200000,
        MuteMembers = 0x00400000,
        DeafenMembers = 0x00800000,
        MoveMembers = 0x01000000,
        UseVoiceActivityDetection = 0x02000000,
        PrioritySpeaker = 0x00000100,
        ChangeNickname = 0x04000000,
        ManageNicknames = 0x08000000,
        ManagePermissions = 0x10000000,
        ManageWebHooks = 0x20000000,
        ManageEmojis = 0x40000000,

        All = KickUsers | BanUsers | Administrator | ManageGuild | ViewAuditLogs | ChangeNickname | ManageNicknames |
              ManageEmojis | CreateInvite | ManageChannels
              | AddReactions | ViewChannel | SendMessages | SendTTSMessages | ManageMessages | EmbedLinks |
              AttachFiles | ReadMessageHistory | MentionEveryone
              | UseExternalEmojis | Connect | Speak | MuteMembers | DeafenMembers | MoveMembers |
              UseVoiceActivityDetection | PrioritySpeaker | ManagePermissions
              | ManageWebHooks
    }
}