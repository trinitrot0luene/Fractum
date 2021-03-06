﻿using System;

namespace Fractum
{
    [Flags]
    public enum ChannelPermissions
    {
        CreateInvite = 0x00000001,
        ManageChannels = 0x00000010,
        AddReactions = 0x00000040,
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
        ManagePermissions = 0x10000000,
        ManageWebHooks = 0x20000000,

        All = CreateInvite | ManageChannels | AddReactions | ViewChannel | SendMessages
              | SendTTSMessages | ManageMessages | EmbedLinks | AttachFiles | ReadMessageHistory
              | MentionEveryone | UseExternalEmojis | Connect | Speak | MuteMembers | DeafenMembers
              | MoveMembers | UseVoiceActivityDetection | PrioritySpeaker | ManagePermissions | ManageWebHooks
    }
}