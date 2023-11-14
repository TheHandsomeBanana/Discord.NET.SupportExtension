using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester.Guild {
    internal class GuildHandler {
        public IGuild IGuild { get; set; }
        public SocketGuild SocketGuild { get; set; }

        private DiscordSocketClient _socketClient;

        public GuildHandler() {
            IDiscordClient client = new DiscordSocketClient();
            _socketClient = new DiscordSocketClient();

            IGuild = client.GetGuildAsync(190).Result;
            SocketGuild = _socketClient.GetGuild(948571888148443156);
        }

        public SocketGuild GetSocketGuild() {
            return SocketGuild;
        }

        public SocketGuild GetSocketGuild(ulong id) {
            return _socketClient.GetGuild(id);
        }
    }
}
