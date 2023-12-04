using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Channel {
    internal class ChannelLinq {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IChannel restChannel = restGuild.GetChannelsAsync().Result.Where(e => e.Id == 0000).First();
            ITextChannel restTextChannel = (await restGuild.GetChannelsAsync())
                .Where(e => e.GetChannelType().Value == ChannelType.Text)
                .FirstOrDefault(e => e.Id == 0001) as ITextChannel;
            IVoiceChannel restVoiceChannel = (await restGuild.GetVoiceChannelsAsync()).FirstOrDefault(e => e.Id == 0002);

            IChannel socketChannel = socketGuild.Channels.FirstOrDefault(e => e.Id == 9999);
            ITextChannel socketTextChannel = socketGuild.TextChannels.Select(e => e).FirstOrDefault(e => e.Id == 9998);
        }
    }
}
