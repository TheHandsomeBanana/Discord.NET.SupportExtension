using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public readonly struct DiscordCompletionContext {
        public DiscordBaseCompletionContext BaseContext { get; }
        public DiscordChannelContext? ChannelContext { get; }
        public SyntaxNode ContextNode { get; }

        public DiscordCompletionContext(DiscordBaseCompletionContext baseContext = DiscordBaseCompletionContext.Undefined) {
            ContextNode = null;
            BaseContext = baseContext;
            ChannelContext = null;
        }

        public DiscordCompletionContext(DiscordBaseCompletionContext baseContext, DiscordChannelContext? channelContext) {
            ContextNode = null;
            BaseContext = baseContext;
            ChannelContext = channelContext;
        }

        public DiscordCompletionContext(SyntaxNode contextNode, DiscordBaseCompletionContext baseContext, DiscordChannelContext? channelContext) : this(baseContext, channelContext) {
            this.ContextNode = contextNode;
        }

        public static DiscordCompletionContext Undefined => new DiscordCompletionContext(DiscordBaseCompletionContext.Undefined);
        public static DiscordCompletionContext Server => new DiscordCompletionContext(DiscordBaseCompletionContext.Server);
        public static DiscordCompletionContext User => new DiscordCompletionContext(DiscordBaseCompletionContext.User);
        public static DiscordCompletionContext Role => new DiscordCompletionContext(DiscordBaseCompletionContext.Role);
        public static DiscordCompletionContext Channel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel);
        public static DiscordCompletionContext TextChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Text);
        public static DiscordCompletionContext VoiceChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Voice);
        public static DiscordCompletionContext CategoryChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Category);
        public static DiscordCompletionContext DMChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.DM);
        public static DiscordCompletionContext GroupChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Group);
        public static DiscordCompletionContext ForumChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Forum);
        public static DiscordCompletionContext GuildChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Guild);
        public static DiscordCompletionContext StageChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Stage);
        public static DiscordCompletionContext ThreadChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Thread);
        public static DiscordCompletionContext PrivateChannel => new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Private);

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
        Thread,
        Private
    }
}
