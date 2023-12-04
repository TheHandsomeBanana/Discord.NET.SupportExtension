using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.User {
    internal class UserCustomMethods {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            RestGuildUser restGuildUser1 = await GetRestUser1(0000);
            IGuildUser restGuildUser2 = GetRestUser2(0001);
            SocketGuildUser socketRole1 = GetSocketUser1(9999);
            IGuildUser socketRole2 = GetSocketUser2(9998);
        }

        private Task<RestGuildUser> GetRestUser1(ulong userId) {
            return restGuild.GetUserAsync(userId);
        }

        private IGuildUser GetRestUser2(ulong user) {
            return GetRestUser1(user).Result;
        }

        private SocketGuildUser GetSocketUser1(ulong id) {
            return socketGuild.GetUser(id);
        }

        private IGuildUser GetSocketUser2(ulong userId) {
            return GetSocketUser1(userId);
        }
    }
}
