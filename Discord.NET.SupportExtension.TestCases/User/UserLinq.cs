using Discord.Rest;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.User {
    internal class UserLinq {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IUser restUser1 = await restGuild.GetUsersAsync().Select(e => e.FirstOrDefault(f => f.Id == 0000)).FirstAsync();
            IUser socketUser1 = await socketGuild.GetUsersAsync().Select(e => e.FirstOrDefault(f => f.Id == 9999)).FirstAsync();
        }
    }
}
