using Discord.NET.SupportExtension.Core.Interface;
using Discord.NET.SupportExtension.Core.Tests.ContextTests;
using Discord.WebSocket;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.ContextDetectorTests {
    [TestClass]
    public class DiscordContextAnalyserTests {
        private readonly ContextTestEngine completionContextTests = new ContextTestEngine();
        public DiscordContextAnalyserTests() {
            completionContextTests.Initialize(@"../../../Discord.NET.SupportExtension.sln", "Discord.NET.SupportExtension.TestCases");
        }

        [TestCleanup]
        public void Cleanup() {
            completionContextTests.Reset();
        }

        #region Positive Testing
        [TestMethod]
        public async Task Guild_PositiveTests() {
            completionContextTests.Add("GuildBasicMethods.cs", DiscordCompletionContext.Server)
                .Add("0000") // IGuild restServer1 = await restClient.GetGuildAsync(0000);
                .Add("0001") // RestGuild restServer2 = restClient.GetGuildAsync(0001).Result;
                .Add("9999"); // IGuild socketServer1 = socketClient.GetGuild(9999);

            completionContextTests.Add("GuildCustomMethods.cs", DiscordCompletionContext.Server)
                .Add("0000") // IGuild restServer1 = GetRestServer1(0000);
                .Add("0001") // RestGuild restServer2 = await GetRestServer2(0001);
                .Add("9999") // SocketGuild socketServer1 = GetSocketServer1(9999);
                .Add("9998") // SocketGuild socketServer2 = GetSocketServer2(9998);
                .Add("9995"); // SocketGuild socketServer3 = GetSocketServer3(9995);

            completionContextTests.Add("GuildLinq.cs", DiscordCompletionContext.Server)
                .Add("0000") // IGuild restServer1 = (await restClient.GetGuildsAsync()).FirstOrDefault(e => e.Id == 0000);
                .Add("0001") // RestGuild restServer2 = restClient.GetGuildsAsync().Result.Where(e => e.Id == 0001).First();
                .Add("9999") // IGuild socketServer1 = socketClient.Guilds.First(e => e.Id == 9999);
                .Add("9998"); // SocketGuild socketServer2 = socketClient.Guilds.Where(e => e.Id == 9998).FirstOrDefault();

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task User_PositiveTests() {
            completionContextTests.Add("UserBasicMethods.cs", DiscordCompletionContext.User)
                .Add("0000") // IGuildUser restGuildUser = await restGuild.GetUserAsync(0000);
                .Add("0001") // IUser restUser = await restClient.GetUserAsync(0001);
                .Add("9999") // IGuildUser socketGuildUser = socketGuild.GetUser(9999);
                .Add("9998"); // IUser socketUser = await socketClient.GetUserAsync(9998);

            completionContextTests.Add("UserCustomMethods.cs", DiscordCompletionContext.User)
                .Add("0000") // RestGuildUser restGuildUser1 = await GetRestUser1(0000);
                .Add("0001") // IGuildUser restGuildUser2 = GetRestUser2(0001);
                .Add("9999") // SocketGuildUser socketRole1 = GetSocketUser1(9999);
                .Add("9998"); // IGuildUser socketRole2 = GetSocketUser2(9998);

            completionContextTests.Add("UserLinq.cs", DiscordCompletionContext.User)
                .Add("0000") // IUser restUser1 = await restGuild.GetUsersAsync().Select(e => e.FirstOrDefault(f => f.Id == 0000)).FirstAsync();
                .Add("9999"); // IUser socketUser1 = await socketGuild.GetUsersAsync().Select(e => e.FirstOrDefault(f => f.Id == 9999)).FirstAsync();

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task Role_PositiveTests() {
            completionContextTests.Add("RoleBasicMethods.cs", DiscordCompletionContext.Role)
                .Add("0000") // IRole restRole = restGuild.GetRole(0000);
                .Add("9999"); // IRole socketRole = socketGuild.GetRole(9999);

            completionContextTests.Add("RoleCustomMethods.cs", DiscordCompletionContext.Role)
                .Add("0000") // RestRole restRole1 = GetRestRole1(0000);
                .Add("0001") // IRole restRole2 = GetRestRole2(0001);
                .Add("9999") // SocketRole socketRole1 = GetSocketRole1(9999);
                .Add("9998"); // IRole socketRole2 = GetSocketRole2(9998);

            completionContextTests.Add("RoleLinq.cs", DiscordCompletionContext.Role)
                .Add("0000") // RestRole restRole1 = restGuild.Roles.FirstOrDefault(e => e.Id == 0000);
                .Add("0001") // IRole restRole2 = restGuild.Roles.Where(e => e.Id == 0001).First();
                .Add("9999") // SocketRole socketRole1 = socketGuild.Roles.First(e => e.Id == 9999);
                .Add("9998"); // IRole socketRole2 = socketGuild.Roles.Where(e => e.Id == 9998).FirstOrDefault();

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task Channel_PositiveTests() {
            completionContextTests.Add("ChannelBasicMethods.cs")
                .Add("0000", DiscordCompletionContext.GuildChannel) // IGuildChannel restGuildChannel = restGuild.GetChannelAsync(0000).Result;
                .Add("0001", DiscordCompletionContext.StageChannel) // IStageChannel restStageChannel = await restGuild.GetStageChannelAsync(0001);
                .Add("0002", DiscordCompletionContext.TextChannel) // ITextChannel restTextChannel = await restGuild.GetTextChannelAsync(0002);
                .Add("0003", DiscordCompletionContext.ThreadChannel) // IThreadChannel restThreadChannel = restGuild.GetThreadChannelAsync(0003).Result;
                .Add("0004", DiscordCompletionContext.VoiceChannel) // IVoiceChannel restVoiceChannel = await restGuild.GetVoiceChannelAsync(0004);
                .Add("0005", DiscordCompletionContext.DMChannel) // IDMChannel restDMChannel = await restClient.GetDMChannelAsync(0005);
                .Add("0006", DiscordCompletionContext.Channel) // IChannel restChannel = await restClient.GetChannelAsync(0006);
                .Add("9999", DiscordCompletionContext.GuildChannel) // IGuildChannel socketGuildChannel = socketGuild.GetChannel(9999);
                .Add("9998", DiscordCompletionContext.StageChannel) // IStageChannel socketStageChannel = socketGuild.GetStageChannel(9998);
                .Add("9997", DiscordCompletionContext.TextChannel) // ITextChannel socketTextChannel = socketGuild.GetTextChannel(9997);
                .Add("9996", DiscordCompletionContext.ThreadChannel) // IThreadChannel socketThreadChannel = socketGuild.GetThreadChannel(9996);
                .Add("9995", DiscordCompletionContext.VoiceChannel) // IVoiceChannel socketVoiceChannel = socketGuild.GetVoiceChannel(9995);
                .Add("9994", DiscordCompletionContext.ForumChannel) // IForumChannel socketForumChannel = socketGuild.GetForumChannel(9994);
                .Add("9993", DiscordCompletionContext.CategoryChannel) // ICategoryChannel categoryChannel = socketGuild.GetCategoryChannel(9993);
                .Add("9992", DiscordCompletionContext.PrivateChannel) // IPrivateChannel socketPrivateChannel = await socketClient.GetPrivateChannelAsync(9992);
                .Add("9991", DiscordCompletionContext.Channel); // IChannel socketChannel = socketClient.GetChannel(9991);

            completionContextTests.Add("ChannelCustomMethods.cs")
                .Add("0000", DiscordCompletionContext.GuildChannel) // IChannel restChannel = RestChannel(0000);
                .Add("0001", DiscordCompletionContext.StageChannel) // IStageChannel restStageChannel = await RestStageChannel(0001);
                .Add("0002", DiscordCompletionContext.VoiceChannel) // IVoiceChannel restVoiceChannel = RestVoiceChannel(0002).Result;
                .Add("9999", DiscordCompletionContext.GuildChannel) // IChannel socketChannel = SocketChannel(9999);
                .Add("9998", DiscordCompletionContext.TextChannel); // ITextChannel socketTextChannel = SocketTextChannel(9998);

            completionContextTests.Add("ChannelLinq.cs")
                .Add("0000", DiscordCompletionContext.GuildChannel) // IChannel restChannel = restGuild.GetChannelsAsync().Result.Where(e => e.Id == 0000).First();
                .Add("0001", DiscordCompletionContext.GuildChannel) // ITextChannel restTextChannel = (await restGuild.GetChannelsAsync())
                             //     .Where(e => e.GetChannelType().Value == ChannelType.Text)
                             //     .FirstOrDefault(e => e.Id == 0001) as ITextChannel;
                .Add("0002", DiscordCompletionContext.VoiceChannel) // IVoiceChannel restVoiceChannel = (await restGuild.GetVoiceChannelsAsync()).FirstOrDefault(e => e.Id == 0002);
                .Add("9999", DiscordCompletionContext.GuildChannel) // IChannel socketChannel = socketGuild.Channels.FirstOrDefault(e => e.Id == 9999);
                .Add("9998", DiscordCompletionContext.TextChannel); // ITextChannel socketTextChannel = socketGuild.TextChannels.GroupBy(e => e.Id).FirstOrDefault(e => e.Key == 9998).First();

            await completionContextTests.RunEngineAsync();
        }
        #endregion

        #region Negative Testing
        [TestMethod]
        public async Task Guild_NegativeTests() {
            completionContextTests.Add("GuildCustomMethods.cs", DiscordCompletionContext.Undefined)
                .Add("NT000") // IGuild restServer1 = GetRestServer1("NT0000", 0000);
                .Add("9996"); // IGuild socketServerNT1 = GetSocketServerNT1(9996); 

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task Channel_NegativeTests() {
            
        }
        #endregion
    }
}