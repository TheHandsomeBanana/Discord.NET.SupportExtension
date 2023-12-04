using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    public class GuildCustomMethods {
        private readonly DiscordSocketClient socketClient;
        private readonly DiscordSocketRestClient restClient;

        public async Task Run() {
            IGuild restServer1 = GetRestServer1("NT000", 0000);
            RestGuild restServer2 = await GetRestServer2(0001);
            SocketGuild socketServer1 = GetSocketServer1(9999);
            SocketGuild socketServer2 = GetSocketServer2(9998);
            SocketGuild socketServer3 = GetSocketServer3(9995);
            IGuild socketServerNT1 = GetSocketServerNT1(9996);
        }

        private IGuild GetRestServer1(string s, ulong serverId) {
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

        private SocketGuild GetSocketServer3(ulong id) {
            return (SocketGuild)socketClient.GetSocketServer1(id);
        }

        private IGuild GetSocketServerNT1(ulong serverId) { // Negative test, serverId should not have IGuild context
            ulong test = 0;
            Console.WriteLine(serverId);
            serverId = 0;
            test = serverId;

            GuildCustomMethodsExtra.GetSocketServerNT1(socketClient, serverId, "");
            socketClient.GetSocketServerNT1(serverId, "s");
            return socketClient.GetGuild(9997);
        }
    }
}
