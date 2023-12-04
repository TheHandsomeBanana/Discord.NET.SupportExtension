using Discord.NET.SupportExtension.TestCases.Guild;
using Discord.Rest;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Channel {
    internal class ChannelServerId {
        public GuildBase ServerBase { get; set; } = new GuildBase();

        private RestGuild restGuild;
        public async Task Run() {
            ITextChannel restTextChannel = GuildBase.GetRestGuild1().Result.GetTextChannelAsync(0000).Result; // guild id = 0000
            IGuildChannel restGuildChannel = ServerBase.GetRestGuild2().GetChannelAsync(0001).Result; // guild id = 0001
            IVoiceChannel restVoiceChannel = await (await ServerBase.GetRestGuild3()).GetVoiceChannelAsync(0002); // guild id = 0002

            restGuild = await GuildBase.GetRestGuild1();
            IStageChannel restStageChannel = await restGuild.GetStageChannelAsync(0003); // guild id = 0000


            ServerBase.SocketGuild1.GetChannel(9999); // guild id = 9999
            IGuild socketGuild = ServerBase.SocketGuild2;
            ITextChannel socketTextChannel = await socketGuild.GetTextChannelAsync(9998); // guild id = 9998
        }
    }
}
