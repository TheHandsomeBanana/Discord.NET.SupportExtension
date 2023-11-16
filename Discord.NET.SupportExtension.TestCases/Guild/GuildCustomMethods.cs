using Discord.Rest;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    public class GuildCustomMethods {
        private readonly DiscordSocketClient socketClient;
        private readonly DiscordSocketRestClient restClient;

        public async Task Run() {
            IGuild restServer1 = GetRestServer1(0000);
            RestGuild restServer2 = await GetRestServer2(0001);
            SocketGuild socketServer1 = GetSocketServer1(9999);
            SocketGuild socketServer2 = GetSocketServer2(9998);
        }

        private IGuild GetRestServer1(ulong serverId) {
            return restClient.GetGuildAsync(serverId).Result;
        }

        private Task<RestGuild> GetRestServer2(ulong server) {
            return restClient.GetGuildAsync(server);
        }

        private SocketGuild GetSocketServer1(ulong id) {
            return socketClient.GetGuild(id);
        }

        private SocketGuild GetSocketServer2(ulong id) {
            return GetSocketServer1(id);
        }
    }
}
