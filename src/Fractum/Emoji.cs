using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Fractum
{
    public class Emoji
    {
        private static readonly Regex _guildEmoteRegex = new Regex(":(\\w+):\\d+", RegexOptions.Compiled);
        private static readonly Regex _nameRegex = new Regex("(\\w+)", RegexOptions.Compiled);

        private Emoji()
        {
        }

        [JsonProperty("id")]
        public ulong? Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        ///     Parse an <see cref="Emoji"></see> from either a raw unicode string or the :name:id Discord markdown format.
        /// </summary>
        /// <param name="input">The string to parse an <see cref="Emoji"></see> from.</param>
        /// <returns></returns>
        public static Emoji Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Invalid input string.");

            var emoji = new Emoji();
            if (_guildEmoteRegex.IsMatch(input))
            {
                var matches = _nameRegex.Matches(input);
                if (matches.Count != 2)
                    throw new ArgumentException("The input string couldn't be parsed as a valid guild emoji.");

                emoji.Name = matches[0].Value;

                if (ulong.TryParse(matches[1].Value, out var emojiId))
                    emoji.Id = emojiId;
                else throw new ArgumentException("The supplied id was not valid.");
            }
            else
            {
                emoji.Name = input.Replace("\\", string.Empty);
                emoji.Id = default;
            }

            return emoji;
        }

        public override string ToString()
            => Id == null ? Name : $":{Name}:{Id}";
    }
}