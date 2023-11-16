using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    internal class GuildLinq {
        private readonly DiscordSocketClient socketClient;
        private readonly DiscordSocketRestClient restClient;

        public async Task Run() {
            IGuild restServer1 = (await restClient.GetGuildsAsync()).FirstOrDefault(e => e.Id == 0000);
            RestGuild restServer2 = restClient.GetGuildsAsync().Result.Where(e => e.Id == 0001).First();

            IGuild socketServer1 = socketClient.Guilds.First(e => e.Id == 9999);
            SocketGuild socketServer2 = socketClient.Guilds.Where(e => e.Id == 9998).FirstOrDefault();
        }
    }
}
