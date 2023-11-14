using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester.Guild {
    public static class GuildAssistor {

        public static SocketGuild Get(DiscordSocketClient client, ulong id) {
            return client.GetGuild(id);            
        }

        public static IGuild GetExt(this IDiscordClient client, ulong id) {
            return client.GetGuildAsync(id).Result;
        }
        
        public static SocketGuild GetExt_v2(this DiscordSocketClient client, ulong id) => client.GetGuild(id);
        

    }
}
