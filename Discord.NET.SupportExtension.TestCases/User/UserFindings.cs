using Discord;
using Discord.WebSocket;
using DiscordISCaseTester.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester.User {
    internal class UserFindings {
        
        public async Task Execute() {
            DiscordSocketClient client = new DiscordSocketClient();
            SocketGuild guild = client.GetGuild(9999);

            SocketGuildUser user = guild.GetUser(0);

            guild.GetUsersAsync().ElementAtAsync(0).Result.FirstOrDefault(e => e.Id == 1);

            IGuild guild1 = client.GetGuild(10101010101010);

            await guild1.GetUserAsync(2);

            var users = await guild1.GetUsersAsync();
            var _user = users.FirstOrDefault(e => e.Id == 3);

            GuildHandler guildHandler = new GuildHandler();

            var user3 = guildHandler.SocketGuild.GetUser(4);
            var user4 = await guildHandler.IGuild.GetUserAsync(5);

            if(user3.Id == 6) {

            }

            var guild_ = guildHandler.GetSocketGuild();
            var user5 = guild_.GetUser(8);
            var guild__ = guildHandler.GetSocketGuild(12345).GetUser(9);

            switch (user4.Id) {
                case 7: break;
            }
        }
    }
}
