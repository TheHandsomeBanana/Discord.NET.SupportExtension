using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace DiscordISCaseTester.Guild {
    public class GuildFindings {
        private DiscordSocketClient _client;

        public Dictionary<ulong, IGuild> Guilds { get; set; } = new Dictionary<ulong, IGuild>();

        public GuildFindings() {
            _client = new DiscordSocketClient();
            _client.GetGuild(0);
        }

        public void ToAnalyse() {
            IReadOnlyCollection<SocketGuild> guilds = _client.Guilds;

            _client.GetGuild(1);
            guilds.Where(e => e.Id == 2);


            if (_client.Guilds.ElementAt(0)?.Id != 3) {
            }            
            
            var x = _client?.GetGuild(4);

            var g = _client != null ? _client.GetGuild(5) : throw new Exception();

            IDiscordClient client = (IDiscordClient)_client;
            IDiscordClient client2 = _client as IDiscordClient;
            client.GetGuildAsync(6);

            client2.GetExt(16);
            _client.GetExt_v2(17);
            GuildAssistor.Get(_client, 18);
            GuildAssistor.GetExt(_client, 19);

            IGuild guild = Guilds[7];


            DiscordShardedClient dsc = new DiscordShardedClient();
            SocketGuild guild2 = dsc.GetGuild(8);
        }

        public async Task ToAnalyseAsync() {
            await _client.Rest.GetGuildAsync(9);
            (await _client.Rest.GetGuildsAsync()).Any(f => f.Id == 10);

            _client.Rest.GetGuildsAsync().Result.FirstOrDefault(e => e.Id == 11);
            foreach(IGuild guild in await _client.Rest.GetGuildsAsync()) {
                if(guild.Id == 12) {

                }

                switch (guild.Id) {
                    case 13: break;
                    case 14: break;
                    case 15: break;
                }
            }
        }
    }
}