using Discord.Rest;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.TestCases.Channel {
    internal class ChannelCustomMethods {
        private readonly RestGuild restGuild;
        private readonly SocketGuild socketGuild;

        public async Task Run() {
            IChannel restChannel = RestChannel(0000);
            IStageChannel restStageChannel = await RestStageChannel(0001, "NT001");
            IVoiceChannel restVoiceChannel = RestVoiceChannel(0002).Result;

            IChannel socketChannel = SocketChannel(9999);
            ITextChannel socketTextChannel = SocketTextChannel(9998);
        }

        public IChannel RestChannel(UInt64 channelId) {
            return restGuild.GetChannelAsync(channelId).Result;
        }

        public async Task<IStageChannel> RestStageChannel(ulong channel, string s) {
            return await restGuild.GetStageChannelAsync(channel);
        }

        public Task<RestVoiceChannel> RestVoiceChannel(ulong id) {
            return restGuild.GetVoiceChannelAsync(id);
        }

        public IChannel SocketChannel(ulong channel) {
            return RestChannel(channel);
        }

        public ITextChannel SocketTextChannel(ulong id) {
            return socketGuild.GetTextChannel(id);
        }
    }
}
