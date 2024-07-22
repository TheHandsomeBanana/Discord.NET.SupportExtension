using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Discord.NET.SupportExtension.TestCases.SystemTest {
    public class DiscordBot {
        private DiscordSocketClient discordSocketClient;
        public DiscordBot() {
            discordSocketClient = new DiscordSocketClient();
        }

        public async Task Run() {
            SocketGuild extensionTestServer
                = discordSocketClient.GetGuild(1157751183184760953 /* Discord.NET Support Extension Testserver (Server) */);

        }

    }
}
