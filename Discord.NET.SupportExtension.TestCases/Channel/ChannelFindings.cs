using Discord;
using Discord.WebSocket;
using DiscordISCaseTester.Guild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordISCaseTester.Channel {
    internal class ChannelFindings {

        public async Task Execute() {

            GuildHandler gh = new GuildHandler();

            SocketGuild socketGuild = gh.SocketGuild;
            IGuild iguild = gh.IGuild;

            iguild.GetChannelsAsync().Result.First(f => f.Id == 1);

            socketGuild.GetTextChannel(2);

            var category = socketGuild.GetCategoryChannel(id);
            category.Channels.First(e => e.Id == 948571888148443157);

            if (category.Channels.Any(f => f.Id != 4)) {

            }
            
            IChannel channel = GetRandomChannel(category);
            switch (channel.Id) {
                case 5: break;
                case 6: break;
            }

            socketGuild.GetVoiceChannel(7);
            socketGuild.GetCategoryChannel(8);
            socketGuild.GetTextChannel(9);
            socketGuild.GetStageChannel(10);
            socketGuild.GetForumChannel(11);
            socketGuild.GetThreadChannel(12);
        }

        private SocketGuildChannel GetRandomChannel(SocketCategoryChannel category) {
            Random random = new Random();

            return category.Channels.ElementAt(random.Next(0, category.Channels.Count));
        }

        const ulong id = 1;
    }
}
