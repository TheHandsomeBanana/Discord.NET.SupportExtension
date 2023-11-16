using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Role {
    internal class RoleLinq {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            RestRole restRole1 = restGuild.Roles.FirstOrDefault(e => e.Id == 0000);
            IRole restRole2 = restGuild.Roles.Where(e => e.Id == 0001).First();

            SocketRole socketRole1 = socketGuild.Roles.First(e => e.Id == 9999);
            IRole socketRole2 = socketGuild.Roles.Where(e => e.Id == 9998).FirstOrDefault();
        }
    }
}
