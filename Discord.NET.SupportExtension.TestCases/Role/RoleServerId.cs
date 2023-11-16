using Discord.NET.SupportExtension.TestCases.Guild;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Role {
    internal class RoleServerId {
        public GuildBase ServerBase { get; set; } = new GuildBase();

        private RestGuild restGuild;
        public async Task Run() {
            IRole restRole1 = GuildBase.GetRestGuild1().Result.GetRole(0000); // guild id = 0000
            IRole restRole2 = ServerBase.GetRestGuild2().GetRole(0001); // guild id = 0001
            IRole restRole3 = (await ServerBase.GetRestGuild3()).GetRole(0002); // guild id = 0002

            restGuild = await GuildBase.GetRestGuild1();
            IRole restRole4 = restGuild.GetRole(0003); // guild id = 0000


            IRole socketRole1 = ServerBase.SocketGuild1.GetRole(9999); // guild id = 9999
            
            IGuild socketGuild = ServerBase.SocketGuild2;
            IRole socketRole2 = socketGuild.GetRole(9998); // guild id = 9998
        }
    }
}
