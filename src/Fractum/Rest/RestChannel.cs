using System;
using Newtonsoft.Json;

namespace Fractum.Rest
{
    public abstract class RestChannel : RestEntity
    {
        internal RestChannel()
        {
        }

        [JsonProperty("type")]
        public ChannelType Type { get; private set; }
    }
}