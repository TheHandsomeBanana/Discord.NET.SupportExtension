using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester {
    internal class RoleFindings {
        private const ulong roleId = 0;
        public SocketGuild Guild { get; set; }
        private IGuild _guild;

        public RoleFindings() { 
            
        }

        public RoleFindings(IGuild guild) {
            var role = guild.GetRole(948585856447447080);
        }

        public void ToAnalyse() {
            DiscordSocketClient client = new DiscordSocketClient();
            client.GetGuild(948571888148443156).GetRole(2);

            Guild = client.GetGuild(10101);

            Guild.GetRole(3);
            _guild.GetRole(roleId);

            var role = GetRole(Guild);

            Guild.Roles.First(e => e.Id == 7);
        }

        public async Task ToAnalyseAsync() {
            IDiscordClient client = new DiscordSocketClient();
            _guild = await client.GetGuildAsync(101010101010);
            _guild.GetRole(4);

            var role = new RoleInvoker(g => g.GetRole(5))(_guild);
        }


        public delegate IRole RoleInvoker(IGuild guild);

        private SocketRole GetRole(SocketGuild guild) {
            return guild.GetRole(6);
        }
    }
}
