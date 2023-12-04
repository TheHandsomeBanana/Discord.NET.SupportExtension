using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    internal class GuildBasicMethods {
        private readonly DiscordSocketClient socketClient;
        private readonly DiscordSocketRestClient restClient;
        public async Task Run() {
            IGuild restServer1 = await restClient.GetGuildAsync(0000);
            RestGuild restServer2 = restClient.GetGuildAsync(0001).Result;
            IGuild socketServer1 = socketClient.GetGuild(9999);
        }
    }
}
