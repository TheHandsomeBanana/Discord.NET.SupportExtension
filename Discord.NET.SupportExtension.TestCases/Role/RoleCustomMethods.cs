using Discord.Rest;
using Discord.WebSocket;

namespace Discord.NET.SupportExtension.TestCases.Role {
    internal class RoleCustomMethods {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public void Run() {
            RestRole restRole1 = GetRestRole1(0000);
            IRole restRole2 = GetRestRole2(0001);
            SocketRole socketRole1 = GetSocketRole1(9999);
            IRole socketRole2 = GetSocketRole2(9998);
        }

        private RestRole GetRestRole1(ulong roleId) {
            return restGuild.GetRole(roleId);
        }

        private IRole GetRestRole2(ulong role) {
            return GetRestRole1(role);
        }

        private SocketRole GetSocketRole1(ulong id) {
            return socketGuild.GetRole(id);
        }

        private IRole GetSocketRole2(ulong roleId) {
            return GetSocketRole1(roleId);
        }
    }
}
