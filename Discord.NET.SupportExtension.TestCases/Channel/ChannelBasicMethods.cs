using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Channel {
    internal class ChannelBasicMethods {
        private readonly DiscordRestClient restClient;
        private readonly DiscordSocketClient socketClient;

        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IGuildChannel restGuildChannel = restGuild.GetChannelAsync(0000).Result;
            IStageChannel restStageChannel = await restGuild.GetStageChannelAsync(0001);
            ITextChannel restTextChannel = await restGuild.GetTextChannelAsync(0002);
            IThreadChannel restThreadChannel = restGuild.GetThreadChannelAsync(0003).Result;
            IVoiceChannel restVoiceChannel = await restGuild.GetVoiceChannelAsync(0004);

            IDMChannel restDMChannel = await restClient.GetDMChannelAsync(0005);
            IChannel restChannel = await restClient.GetChannelAsync(0006);

            IGuildChannel socketGuildChannel = socketGuild.GetChannel(9999);
            IStageChannel socketStageChannel = socketGuild.GetStageChannel(9998);
            ITextChannel socketTextChannel = socketGuild.GetTextChannel(9997);
            IThreadChannel socketThreadChannel = socketGuild.GetThreadChannel(9996);
            IVoiceChannel socketVoiceChannel = socketGuild.GetVoiceChannel(9995);
            IForumChannel socketForumChannel = socketGuild.GetForumChannel(9994);
            ICategoryChannel categoryChannel = socketGuild.GetCategoryChannel(9993);

            IPrivateChannel socketPrivateChannel = await socketClient.GetPrivateChannelAsync(9992);
            IChannel socketChannel = socketClient.GetChannel(9991);

        }
    }
}
