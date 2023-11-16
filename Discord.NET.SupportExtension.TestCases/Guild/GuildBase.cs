using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    // Required for entities per server (ServerIdAnalyser)
    public class GuildBase {
        private readonly static DiscordRestClient restClient;
        private readonly static DiscordSocketClient socketClient;

        public static async Task<RestGuild> GetRestGuild1() => await restClient.GetGuildAsync(0000);
        public RestGuild GetRestGuild2() => restClient.GetGuildAsync(0001).Result;

        public async Task<IGuild> GetRestGuild3() {
            return (await restClient.GetGuildsAsync()).FirstOrDefault(e => e.Id == 0002);
        }

        public SocketGuild SocketGuild1 => socketClient.GetGuild(9999);
        public SocketGuild SocketGuild2 {
            get {
                return socketClient.GetGuild(9998);
            }
        }
        public SocketGuild SocketGuild3 {
            get => socketClient.Guilds.Where(e => e.Id == 9997).FirstOrDefault();
        }

        public IGuild GetSocketGuild4() {
            return socketClient.GetGuild(9996);
        }
    }
}
