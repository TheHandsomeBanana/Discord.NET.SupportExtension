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
        private DiscordCompletionContextTestEngine _discordCaseTestEngine;
        private DiscordChannelContextTestEngine _discordContextChannelTestEngine;

        [TestInitialize]
        public void Initialize() {
            _discordCaseTestEngine = new DiscordCompletionContextTestEngine(@"P:\DEPRECATED_Projects\DiscordISCaseTester\DiscordCaseTester.sln", "DiscordISCaseTester");
            _discordContextChannelTestEngine = new DiscordChannelContextTestEngine(@"P:\DEPRECATED_Projects\DiscordISCaseTester\DiscordCaseTester.sln", "DiscordISCaseTester");
        }

        [TestMethod] // Special Test for Channel Type Detection
        public async Task ChannelContextTests() {
            _discordContextChannelTestEngine.Add("ChannelFindings.cs")
                        .Add("0", DiscordChannelContext.Guild)
                        .Add("1", DiscordChannelContext.Guild)
                        .Add("2", DiscordChannelContext.Text)
                        .Add("3", DiscordChannelContext.Guild)
                        .Add("4", DiscordChannelContext.Guild)
                        .Add("5", null)
                        .Add("6", null)
                        .Add("7", DiscordChannelContext.Voice)
                        .Add("8", DiscordChannelContext.Category)
                        .Add("9", DiscordChannelContext.Text)
                        .Add("10", DiscordChannelContext.Voice)
                        .Add("11", DiscordChannelContext.Forum)
                        .Add("12", DiscordChannelContext.Text);

            await _discordContextChannelTestEngine.RunEngineAsync();
        }


        // ----------------------------------------------------------------------------------------- //
        // ----------------------------------- Testdriven Dev ------------------------------------ //
        // ----------------------------------------------------------------------------------------- //

        [TestMethod]
        public async Task GuildTests() {
            _discordCaseTestEngine.Add("GuildFindings.cs", DiscordCompletionContext.Server)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5").Add("6").Add("7").Add("8").Add("9").Add("10")
                .Add("11").Add("12").Add("13").Add("14").Add("15").Add("16").Add("17").Add("18").Add("19");

            await _discordCaseTestEngine.RunEngineAsync();
        }

        [TestMethod]
        public async Task UserTests() {
            _discordCaseTestEngine.Add("UserFindings.cs", DiscordCompletionContext.User)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5");

            await _discordCaseTestEngine.RunEngineAsync();
        }

        [TestMethod]
        public async Task RoleTests() {
            _discordCaseTestEngine.Add("RoleFindings.cs", DiscordCompletionContext.Role)
                .Add("1").Add("2").Add("3").Add("4").Add("5").Add("6");

            await _discordCaseTestEngine.RunEngineAsync();
        }

        [TestMethod]
        public async Task ChannelTests() {
            _discordCaseTestEngine.Add("ChannelFindings.cs", DiscordCompletionContext.Channel)
                .Add("0").Add("1").Add("2").Add("3").Add("4").Add("5").Add("6");

            await _discordCaseTestEngine.RunEngineAsync();
        }
    }
}