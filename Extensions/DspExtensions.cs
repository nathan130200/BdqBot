using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bdq.Extensions
{
    public static class DspExtensions
    {

        public static string GetFormat(this DiscordUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }


        public static DiscordEmbedBuilder WithRequestedBy(this DiscordEmbedBuilder builder, DiscordUser user, bool timestamp = false)
        {
            if (timestamp)
                builder.WithTimestamp(DateTimeOffset.Now);

            return builder.WithFooter($"Solicitado por {user.GetFormat()}", user.GetAvatarUrl(ImageFormat.Jpeg));
        }

        public static DiscordEmbedBuilder WithField(this DiscordEmbedBuilder builder, object name, object value, bool inline = false)
        {
            return builder.AddField("\u200b" + name, "\u200b" + value, inline);
        }

        public static DiscordEmbedBuilder WithInlineField(this DiscordEmbedBuilder builder, object name, object value)
        {
            return builder.WithField(name, value, true);
        }

        public static async Task TryDeleteAsync(this DiscordMessage msg, string reason = null)
        {
            try { await msg.DeleteAsync(reason).ConfigureAwait(false); }
            catch { }
        }
    }
}
