using Discord.NET.SupportExtension.TestCases.Guild;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.User {
    internal class UserServerId {
        public GuildBase ServerBase { get; set; } = new GuildBase();

        private RestGuild restGuild;
        public async Task Run() {
            IUser restUser1 = await GuildBase.GetRestGuild1().Result.GetUserAsync(0000); // guild id = 0000
            IUser restUser2 = await ServerBase.GetRestGuild2().GetUserAsync(0001); // guild id = 0001
            IUser restUser3 = await(await ServerBase.GetRestGuild3()).GetUserAsync(0002); // guild id = 0002

            restGuild = await GuildBase.GetRestGuild1();
            IUser restUser4 = await restGuild.GetUserAsync(0003); // guild id = 0000


            IUser socketUser1 = ServerBase.SocketGuild1.GetUser(9999); // guild id = 9999

            IGuild socketGuild = ServerBase.SocketGuild2;
            IUser socketUser2 = await socketGuild.GetUserAsync(9998); // guild id = 9998
        }
    }
}
