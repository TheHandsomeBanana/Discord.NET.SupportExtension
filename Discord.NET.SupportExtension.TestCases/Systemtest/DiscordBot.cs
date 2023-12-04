using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;

namespace Discord.NET.SupportExtension.TestCases.SystemTest {
    public class DiscordBot {
        private DiscordSocketClient client;

        public DiscordBot() {
            client = new DiscordSocketClient();
        }

        public void Run() {
            IGuild server = client.GetGuild(id: 948571888148443156 /* Banana-Land (Server) */);

            IChannel channel = server.GetCategoriesAsync().Result.First(e => e.Id == 948893874049409024 /* Albion (Category [Banana-Land]) */);

            IEnumerable<IStageChannel> channels = server.GetChannelsAsync().Result.Cast<IStageChannel>();



        }
    }
}
