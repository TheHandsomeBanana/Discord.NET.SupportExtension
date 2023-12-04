using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Guild {
    // Required for entities per server (ServerIdAnalyser)
    public class GuildBase {
        public static DiscordRestClient RestClient { get; }
        public static DiscordSocketClient SocketClient { get; }

        public static async Task<RestGuild> GetRestGuild1() => await RestClient.GetGuildAsync(0000);
        public RestGuild GetRestGuild2() => RestClient.GetGuildAsync(0001).Result;

        public async Task<IGuild> GetRestGuild3() {
            return (await RestClient.GetGuildsAsync()).FirstOrDefault(e => e.Id == 0002);
        }

        public SocketGuild SocketGuild1 => SocketClient.GetGuild(9999);
        public SocketGuild SocketGuild2 {
            get {
                return SocketClient.GetGuild(9998);
            }
        }
        public SocketGuild SocketGuild3 {
            get => SocketClient.Guilds.Where(e => e.Id == 9997).FirstOrDefault();
        }

        public IGuild GetSocketGuild4() {
            return SocketClient.GetGuild(9996);
        }
    }
}
