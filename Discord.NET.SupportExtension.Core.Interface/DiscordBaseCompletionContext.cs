using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public struct DiscordCompletionContext {
        public DiscordBaseCompletionContext BaseContext { get; set; }
        public DiscordChannelContext? ChannelContext { get; set; }

        public DiscordCompletionContext(DiscordBaseCompletionContext baseContext, DiscordChannelContext? channelContext) {
            BaseContext = baseContext;
            ChannelContext = channelContext;
        }

        public DiscordCompletionContext(DiscordBaseCompletionContext baseContext = DiscordBaseCompletionContext.Undefined) {
            BaseContext = baseContext;
            ChannelContext = null;
        }

        public static DiscordCompletionContext Server => new DiscordCompletionContext(DiscordBaseCompletionContext.Server);
        public static DiscordCompletionContext User => new DiscordCompletionContext(DiscordBaseCompletionContext.User);
        public static DiscordCompletionContext Role => new DiscordCompletionContext(DiscordBaseCompletionContext.Role);
        public static DiscordCompletionContext Channel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel);

        public static bool operator == (DiscordCompletionContext left, DiscordCompletionContext right) {
            return left.ChannelContext == right.ChannelContext && left.BaseContext == right.BaseContext;
        }

        public static bool operator != (DiscordCompletionContext left, DiscordCompletionContext right) {
            return !(left == right);
        }

        public override bool Equals(object obj) {
            return obj is DiscordCompletionContext context &&
                   BaseContext == context.BaseContext &&
                   ChannelContext == context.ChannelContext;
        }

        public override int GetHashCode() {
            int hashCode = -258111414;
            hashCode = hashCode * -1521134295 + BaseContext.GetHashCode();
            hashCode = hashCode * -1521134295 + ChannelContext.GetHashCode();
            return hashCode;
        }

        public override string ToString() {
            return $"{BaseContext} ({ChannelContext})";
        }
    }

    public enum DiscordBaseCompletionContext {
        Undefined,
        Server,
        User,
        Role,
        Channel
    }

    public enum DiscordChannelContext {
        Text,
        Voice,
        Category,
        DM,
        Group,
        Forum,
        Guild,
        Stage,
        Thread
    }
}
