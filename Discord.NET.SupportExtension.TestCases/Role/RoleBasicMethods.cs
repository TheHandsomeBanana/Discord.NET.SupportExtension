using Discord.Rest;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Role {
    internal class RoleBasicMethods {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IRole restRole = restGuild.GetRole(0000);
            IRole socketRole = socketGuild.GetRole(9999);
        }
    }
}
