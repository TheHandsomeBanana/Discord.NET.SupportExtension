using Discord.NET.SupportExtension.Core.Interface;
using HB.NETF.Code.Analysis.TestEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.ContextDetectorTests {
    [TestClass]
    public class DiscordCompletionContextTests : TestBase {
        private DiscordCompletionContextTestEngine completionContextTests;

        [TestInitialize]
        public void Initialize() {
            completionContextTests = new DiscordCompletionContextTestEngine(@"P:\Projects\DOTNET\TestSolution\TestSolution.sln", "DiscordSupportExtensionTestCases");
        }

        [TestMethod] // Special Test for Channel Type Detection
        public async Task ChannelContextTests() {
            completionContextTests.Add("ChannelFindings.cs")
                        .Add("0", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Guild))
                        .Add("1", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Guild))
                        .Add("2", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Text))
                        .Add("3", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Guild))
                        .Add("4", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Guild))
                        .Add("5", DiscordCompletionContext.Channel)
                        .Add("6", DiscordCompletionContext.Channel)
                        .Add("7", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Voice))
                        .Add("8", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Category))
                        .Add("9", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Text))
                        .Add("10", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Voice))
                        .Add("11", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Forum))
                        .Add("12", new DiscordCompletionContext(DiscordBaseCompletionContext.Channel, DiscordChannelContext.Text));

            await completionContextTests.RunEngineAsync();
        }


        // ----------------------------------------------------------------------------------------- //
        // ----------------------------------- Testdriven Dev ------------------------------------ //
        // ----------------------------------------------------------------------------------------- //

        [TestMethod]
        public async Task GuildTests() {
            completionContextTests.Add("GuildFindings.cs", DiscordCompletionContext.Server)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5").Add("6").Add("7").Add("8").Add("9").Add("10")
                .Add("11").Add("12").Add("13").Add("14").Add("15").Add("16").Add("17").Add("18").Add("19");

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task UserTests() {
            completionContextTests.Add("UserFindings.cs", DiscordCompletionContext.User)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5");

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task RoleTests() {
            completionContextTests.Add("RoleFindings.cs", DiscordCompletionContext.Role)
                .Add("1").Add("2").Add("3").Add("4").Add("5").Add("6");

            await completionContextTests.RunEngineAsync();
        }

        [TestMethod]
        public async Task ChannelTests() {
            completionContextTests.Add("ChannelFindings.cs", DiscordCompletionContext.Channel)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5").Add("6");

            await completionContextTests.RunEngineAsync();
        }
    }
}