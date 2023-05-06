using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Interface {
    public enum DiscordCompletionContext {
        Undefined,
        Server,
        User,
        Role,
        Channel
    }

    public enum DiscordChannelContext {
        Undefined,
        Text,
        Voice,
        Category,
        DM,
        Group,
        Forum,
        Guild,
    }
}
