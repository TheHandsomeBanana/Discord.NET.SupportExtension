using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.SystemTest
{
    public class DiscordBot
    {
        private DiscordSocketClient client;

        public DiscordBot() {
            client = new DiscordSocketClient();
        }

        public void Run() {
            IGuild server = client.GetGuild(1157751183184760953);

            IChannel channel = server.GetTextChannelAsync(1157751183822311496).Result; // Allgemein Channel
        }
    }
}
