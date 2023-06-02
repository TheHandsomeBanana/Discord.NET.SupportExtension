using HB.NETF.Code.Analysis.TestEngine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord.NET.SupportExtension.Core.Tests.AnalyserTests {
    [TestClass]
    public class DiscordAnalyserTests : TestBase {
        private DiscordServerIdTestEngine serverDetailTests;

        [TestInitialize]
        public void Initialize() {
            serverDetailTests = new DiscordServerIdTestEngine(@"P:\Projects\DOTNET\TestSolution\TestSolution.sln", "DiscordSupportExtensionTestCases");
        }

        [TestMethod]
        public async Task ServerIdTests() {
            serverDetailTests.Add("GuildFindings.cs")
                .Add("ServerId", new ulong[] { 0123456789 });

            await serverDetailTests.RunEngineAsync();
        }
    }
}
