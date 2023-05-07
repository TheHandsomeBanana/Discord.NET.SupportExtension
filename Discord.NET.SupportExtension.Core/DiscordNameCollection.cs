using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core {
    internal static class DiscordNameCollection {

        // Base Types
        public const string IDISCORDCLIENT = "Discord.IDiscordClient";
        public const string IGUILD = "Discord.IGuild";
        public const string IUSER = "Discord.IUser";
        public const string IROLE = "Discord.IRole";
        public const string ICHANNEL = "Discord.IChannel";

        // Channel Types
        public const string ICATEGORYCHANNEL = "Discord.ICategoryChannel";
        public const string ITEXTCHANNEL = "Discord.ITextChannel";
        public const string IVOICECHANNEL = "Discord.IVoiceChannel";
        public const string IDMCHANNEL = "Discord.IDMChannel";
        public const string IFORUMCHANNEL = "Discord.IForumChannel";
        public const string IGROUPCHANNEL = "Discord.IGroupChannel";
        public const string IGUILDCHANNEL = "Discord.IGuildChannel";
        public const string ISTAGECHANNEL = "Discord.IStageChannel";
        public const string ITHREADCHANNEL = "Discord.IThreadChannel";

        // Attributes
        public const string SERVERID = "HB.NETF.Discord.NET.Toolkit.SupportExtension.ServerIdAttribute";
        public const string SERVERIDLIST = "HB.NETF.Discord.NET.Toolkit.SupportExtension.ServerIdListAttribute";
        public const string SERVERNAME = "HB.NETF.Discord.NET.Toolkit.SupportExtension.ServerNameAttribute";
        public const string SERVERNAMELIST = "HB.NETF.Discord.NET.Toolkit.SupportExtension.ServerNameListAttribute";

        private static string[] names = {
            IDISCORDCLIENT,
            IGUILD,
            IUSER,
            IROLE,
            ICHANNEL,

            SERVERID,
            SERVERIDLIST,
            SERVERNAME,
            SERVERNAMELIST,
        };

        private static string[] channels = {
            ICATEGORYCHANNEL,
            ITEXTCHANNEL,
            IVOICECHANNEL,
            IDMCHANNEL,
            IFORUMCHANNEL,
            IGROUPCHANNEL,
            IGUILDCHANNEL
        };

        public static bool Contains(string name) {
            return names.Contains(name);
        }

        public static bool ChannelContains(string name) {
            return channels.Contains(name);
        }
    }
}
