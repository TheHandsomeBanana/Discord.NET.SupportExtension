using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.User {
    internal class UserBasicMethods {
        private readonly DiscordRestClient restClient;
        private readonly DiscordSocketClient socketClient;

        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IGuildUser restGuildUser = await restGuild.GetUserAsync(0000);
            IUser restUser = await restClient.GetUserAsync(0001);

            IGuildUser socketGuildUser = socketGuild.GetUser(9999);
            IUser socketUser = await socketClient.GetUserAsync(9998);
        }
    }
}
