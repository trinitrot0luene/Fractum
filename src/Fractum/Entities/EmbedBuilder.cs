using Fractum.Rest.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fractum.Entities
{
    public sealed class EmbedBuilder
    {
        private EmbedFooter Footer;

        private List<EmbedField> Fields;

        private Color Color;

        private DateTimeOffset? Timestamp;

        private string Title;

        private string Description;

        public EmbedBuilder()
        {
            Fields = new List<EmbedField>();
            Footer = new EmbedFooter { Text = string.Empty, IconUrl = string.Empty };
            Color = Color.Black;
        }

        public EmbedBuilder WithField(string title, string value, bool isInline = false)
        {
            if (Fields.Count >= 25)
                throw new InvalidOperationException("You cannot have more than 25 fields in an embed.");
            if (title.Length > 256)
                throw new ArgumentException("Your title cannot be longer than 256 characters.");
            if (value.Length > 1024)
                throw new ArgumentException("Your field content cannot be longer than 1024 characters.");

            Fields.Add(new EmbedField { Name = title, Value = value, IsInline = isInline });
            return this;
        }

        public EmbedBuilder WithTimestamp()
        {
            Timestamp = DateTimeOffset.UtcNow;
            return this;
        }

        public EmbedBuilder WithTitle(string title)
        {
            if (title.Length > 256)
                throw new ArgumentException("Title must be 256 characters or fewer.");

            Title = title;
            return this;
        }

        public EmbedBuilder WithDescription(string description)
        {
            if (description.Length > 2000)
                throw new ArgumentException("Description must be 2000 characters or fewer.");

            Description = description;
            return this;
        }

        public EmbedBuilder WithColor(int r, int g, int b)
        {
            Color = Color.FromArgb(r, g, b);
            return this;
        }

        public EmbedBuilder WithColor(Color color)
        {
            Color = color;
            return this;
        }

        public EmbedBuilder WithFooter(string content = null, string iconUrl = null)
        {
            if (content.Length > 2048)
                throw new ArgumentException("Footer content must not be longer than 2048 characters.");

            Footer = new EmbedFooter { Text = content, IconUrl = iconUrl };
            return this;
        }

        public Embed Create() => new Embed() { Title = Title, Description = Description, Fields = Fields.ToArray(), Color = Color.ToRGB(), Footer = Footer, Timestamp = Timestamp };
    }
}