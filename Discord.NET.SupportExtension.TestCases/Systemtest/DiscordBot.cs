using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Discord.NET.SupportExtension.TestCases.SystemTest {
    public class DiscordBot {
        private DiscordSocketClient client;
        private DiscordRestClient restClient;
        public DiscordBot() {
            client = new DiscordSocketClient();
        }

        public void Run() {
            IGuild server = client.GetGuild(948571888148443156 /* Banana-Land (Server) */);

            server.GetRole(948587247408668682 /* Banana King 🍌👑 (Role [Banana-Land]) */);

            IGuild server2 = GetServerById(948571888148443156 /* Banana-Land (Server) */);
        }


        public IGuild GetServerById(ulong id) {
            return client.GetGuild(id);
        }
    }
}
